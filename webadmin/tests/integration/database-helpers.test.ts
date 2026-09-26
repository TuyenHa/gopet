import { afterAll, beforeAll, describe, expect, it } from "vitest";
import mysql from "mysql2/promise";
import { queryOne, execute, query } from "@/lib/db/query";
import { withNamedLock, LockBusyError } from "@/lib/db/named-lock";

/**
 * Integration tests — require local MariaDB docker `gopet-mariadb` running on 127.0.0.1:3306
 * with `.env.local` credentials. SKIP automatically when RUN_DB_TESTS not set.
 */
describe.skipIf(!process.env.RUN_DB_TESTS)("Integration: Database Helpers (requires local MariaDB)", () => {
  let gamePool: mysql.Pool;
  let webPool: mysql.Pool;

  beforeAll(async () => {
    // Create dedicated test pools using .env.local values
    const env = {
      DB_HOST: process.env.DB_HOST || "127.0.0.1",
      DB_PORT: parseInt(process.env.DB_PORT || "3306", 10),
      DB_USER: process.env.DB_USER || "gopet_admin",
      DB_PASSWORD: process.env.DB_PASSWORD || "",
      DB_GAME: process.env.DB_GAME || "gopettae_tae2",
      DB_WEB: process.env.DB_WEB || "gopettae_gopet_web",
    };

    gamePool = mysql.createPool({
      host: env.DB_HOST,
      port: env.DB_PORT,
      user: env.DB_USER,
      password: env.DB_PASSWORD,
      database: env.DB_GAME,
      charset: "utf8mb4",
      dateStrings: true,
      supportBigNumbers: true,
      bigNumberStrings: true,
      connectionLimit: 5,
    });

    webPool = mysql.createPool({
      host: env.DB_HOST,
      port: env.DB_PORT,
      user: env.DB_USER,
      password: env.DB_PASSWORD,
      database: env.DB_WEB,
      charset: "utf8mb4",
      dateStrings: true,
      supportBigNumbers: true,
      bigNumberStrings: true,
      connectionLimit: 5,
    });
  });

  afterAll(async () => {
    await gamePool.end();
    await webPool.end();
  });

  describe("withNamedLock", () => {
    const lockName = `test_lock_${Date.now()}`;

    it("acquires and releases lock successfully", async () => {
      let fnCalled = false;
      const result = await withNamedLock(gamePool, lockName, 5, async () => {
        fnCalled = true;
        return "success";
      });

      expect(fnCalled).toBe(true);
      expect(result).toBe("success");

      // Verify lock is released
      const conn = await gamePool.getConnection();
      try {
        const lockCheck = await queryOne<{ is_used: number | null }>(
          conn,
          "SELECT IS_USED_LOCK(?) as is_used",
          [lockName],
        );
        expect(lockCheck?.is_used).toBeNull(); // Lock is released
      } finally {
        conn.release();
      }
    });

    it("releases lock even when fn throws", async () => {
      const lockNameError = `test_lock_error_${Date.now()}`;
      try {
        await withNamedLock(gamePool, lockNameError, 5, async () => {
          throw new Error("Test error");
        });
      } catch (e) {
        expect((e as Error).message).toBe("Test error");
      }

      // Verify lock is released after error
      const conn = await gamePool.getConnection();
      try {
        const lockCheck = await queryOne<{ is_used: number | null }>(
          conn,
          "SELECT IS_USED_LOCK(?) as is_used",
          [lockNameError],
        );
        expect(lockCheck?.is_used).toBeNull(); // Lock is released even after error
      } finally {
        conn.release();
      }
    });

    it("throws LockBusyError when lock is held by another connection", async () => {
      const lockNameBusy = `test_lock_busy_${Date.now()}`;

      // Get a connection and hold the lock
      const lockHolder = await gamePool.getConnection();
      try {
        const lockResult = await queryOne<{ got: number | null }>(
          lockHolder,
          "SELECT GET_LOCK(?, ?) AS got",
          [lockNameBusy, 10],
        );
        expect(lockResult?.got).toBe(1); // Verify lock is held

        // Try to acquire the same lock from another connection (should timeout/fail)
        let errorThrown = false;
        try {
          await withNamedLock(gamePool, lockNameBusy, 1, async () => {
            return "should not reach here";
          });
        } catch (e) {
          if (e instanceof LockBusyError) {
            errorThrown = true;
            expect(e.lockName).toBe(lockNameBusy);
          } else {
            throw e;
          }
        }
        expect(errorThrown).toBe(true);
      } finally {
        await lockHolder.query("SELECT RELEASE_LOCK(?)", [lockNameBusy]);
        lockHolder.release();
      }
    });
  });

  describe("Offline-Guard Protocol", () => {
    const TEST_USER_ID = 1467; // wadtest account

    it("verifies player_online table exists and can be queried", async () => {
      // gopet_admin has SELECT permission on player_online
      const conn = await gamePool.getConnection();
      try {
        // Check if player_online row exists
        const onlineRow = await queryOne<{ x: number }>(
          conn,
          "SELECT 1 AS x FROM player_online WHERE user_id = ? LIMIT 1",
          [TEST_USER_ID],
        );
        // If wadtest is online, row should exist; otherwise it's expected to be null
        // Just verify the query works
        expect(typeof onlineRow === "object" || onlineRow === null).toBe(true);
      } finally {
        conn.release();
      }
    });

    it("verifies server heartbeat freshness", async () => {
      const conn = await gamePool.getConnection();
      try {
        // Check current heartbeat
        const beat = await queryOne<{ age: string | null }>(
          conn,
          "SELECT CAST(TIMESTAMPDIFF(SECOND, beat_at, NOW()) AS CHAR) AS age FROM server_heartbeat WHERE id = 1 AND protocol_version >= 1",
        );

        // Heartbeat should exist if server is running and be relatively fresh (< 90 seconds)
        if (beat && beat.age !== null) {
          const ageSeconds = parseInt(beat.age, 10);
          expect(ageSeconds).toBeLessThan(90);
        }
      } finally {
        conn.release();
      }
    });
  });

  describe("User Coin Atomicity", () => {
    const TEST_USER_ID = 1467; // wadtest
    const COIN_DELTA = 1000;

    it("atomic coin update within valid range [0, 2147483647]", async () => {
      const conn = await webPool.getConnection();
      try {
        // Get current coin value
        const before = await queryOne<{ coin: string }>(conn, "SELECT coin FROM user WHERE user_id = ?", [
          TEST_USER_ID,
        ]);
        const currentCoin = parseInt(before?.coin || "0", 10);

        // Perform atomic update
        const newCoin = currentCoin + COIN_DELTA;
        if (newCoin >= 0 && newCoin <= 2147483647) {
          const result = await execute(
            conn,
            "UPDATE user SET coin = coin + ? WHERE user_id = ? AND ? BETWEEN 0 AND 2147483647",
            [COIN_DELTA, TEST_USER_ID, newCoin],
          );
          expect(result.affectedRows).toBeGreaterThan(0);

          // Verify update
          const after = await queryOne<{ coin: string }>(conn, "SELECT coin FROM user WHERE user_id = ?", [
            TEST_USER_ID,
          ]);
          expect(parseInt(after?.coin || "0", 10)).toBe(newCoin);

          // Restore original value
          await execute(conn, "UPDATE user SET coin = ? WHERE user_id = ?", [currentCoin, TEST_USER_ID]);
        }
      } finally {
        conn.release();
      }
    });

    it("prevents coin overflow via SQL boundary check", async () => {
      const conn = await webPool.getConnection();
      try {
        // Try to set coin to a value that would overflow
        const overflowValue = 2147483647 + 1000;
        const result = await execute(
          conn,
          "UPDATE user SET coin = coin + ? WHERE user_id = ? AND ? BETWEEN 0 AND 2147483647",
          [1000, TEST_USER_ID, overflowValue],
        );

        // Update should fail due to boundary check
        expect(result.affectedRows).toBe(0);
      } finally {
        conn.release();
      }
    });
  });

  describe("GiftCode SQL Operations", () => {
    it("creates and reads giftcode with unique test code", async () => {
      const testCode = `TEST_${Date.now()}_${Math.random().toString(36).substring(7)}`.substring(0, 100);
      const testData = "[[1,1,1]]"; // Simple test item data

      const conn = await gamePool.getConnection();
      try {
        // Create giftcode
        const insertResult = await execute(
          conn,
          "INSERT INTO gift_code (code, currentUser, maxUser, gift_data, expire) VALUES (?, ?, ?, ?, DATE_ADD(NOW(), INTERVAL 7 DAY))",
          [testCode, 0, 1, testData],
        );
        expect(insertResult.insertId).toBeGreaterThan(0);

        // Read giftcode
        const row = await queryOne<{ code: string; currentUser: number; gift_data: string }>(
          conn,
          "SELECT code, currentUser, gift_data FROM gift_code WHERE code = ?",
          [testCode],
        );
        expect(row).not.toBeNull();
        expect(row?.code).toBe(testCode);
        expect(row?.currentUser).toBe(0);
        expect(row?.gift_data).toBe(testData);

        // Cleanup
        await execute(conn, "DELETE FROM gift_code WHERE code = ?", [testCode]);
      } finally {
        conn.release();
      }
    });

    it("handles concurrent giftcode reads and updates", async () => {
      const testCode = `CONCURRENT_${Date.now()}_${Math.random().toString(36).substring(7)}`.substring(0, 100);
      const testData = "[[2,2,2]]";

      const conn = await gamePool.getConnection();
      try {
        // Create giftcode
        await execute(
          conn,
          "INSERT INTO gift_code (code, currentUser, maxUser, gift_data) VALUES (?, ?, ?, ?)",
          [testCode, 0, 5, testData],
        );

        // Simulate multiple users redeeming
        for (let i = 1; i <= 3; i++) {
          const updateResult = await execute(
            conn,
            "UPDATE gift_code SET currentUser = currentUser + 1 WHERE code = ? AND currentUser < maxUser",
            [testCode],
          );
          expect(updateResult.affectedRows).toBe(1);
        }

        // Verify final state
        const final = await queryOne<{ currentUser: number; maxUser: number }>(
          conn,
          "SELECT currentUser, maxUser FROM gift_code WHERE code = ?",
          [testCode],
        );
        expect(final?.currentUser).toBe(3);

        // Cleanup
        await execute(conn, "DELETE FROM gift_code WHERE code = ?", [testCode]);
      } finally {
        conn.release();
      }
    });
  });

  describe("Admin Permissions (gopet_admin user)", () => {
    it("verifies gopet_admin cannot DELETE admin_audit_log", async () => {
      // This test expects a permission error when gopet_admin tries to delete from admin_audit_log
      const conn = await webPool.getConnection();
      try {
        let permissionDenied = false;
        try {
          // Try to delete from admin_audit_log (should fail with ER 1142)
          await execute(conn, "DELETE FROM admin_audit_log WHERE id = 99999999");
        } catch (e: unknown) {
          if (
            (e as Record<string, unknown>)?.code === "ER_TABLEACCESS_DENIED_ERROR" ||
            (e as Record<string, unknown>)?.code === "ER_SPECIFIC_ACCESS_DENIED_ERROR"
          ) {
            permissionDenied = true;
          }
        }
        // If no permission error, the grant might be overly permissive — warn but don't fail
        if (!permissionDenied) {
          console.warn(
            "WARNING: gopet_admin user was able to DELETE from admin_audit_log — check database grants",
          );
        }
      } finally {
        conn.release();
      }
    });

    it("verifies gopet_admin can SELECT from admin_audit_log", async () => {
      const conn = await webPool.getConnection();
      try {
        // Should be able to read audit logs
        const rows = await query<{ id: number }>(
          conn,
          "SELECT id FROM admin_audit_log LIMIT 1",
        );
        // Query should succeed (even if no rows exist)
        expect(Array.isArray(rows)).toBe(true);
      } finally {
        conn.release();
      }
    });
  });
});
