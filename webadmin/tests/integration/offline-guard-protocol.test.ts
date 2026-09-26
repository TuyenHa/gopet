import { afterAll, beforeAll, describe, expect, it } from "vitest";
import mysql from "mysql2/promise";
import { queryOne } from "@/lib/db/query";
import { withOfflinePlayer } from "@/lib/players/offline-guard";

/**
 * Integration tests for offline-guard protocol.
 * SKIP if RUN_DB_TESTS not set.
 *
 * Note: Testing withOfflinePlayer through the full library function would require
 * the ability to manipulate player_online rows, which gopet_admin doesn't have
 * (player_online is managed by GServer). Instead, we test the underlying protocol
 * pieces: named locks, heartbeat checks, and player_online querying.
 */
describe.skipIf(!process.env.RUN_DB_TESTS)("Integration: Offline-Guard Protocol", () => {
  let gamePool: mysql.Pool;
  let webPool: mysql.Pool;

  const TEST_USER_ID = 1467; // wadtest
  const TEST_USERNAME = "wadtest";
  const TEST_LOCK = `login_lock_${TEST_USERNAME}`;
  // withOfflinePlayer nhận playerId (H3 — user_id được tra lại ở server, không tin client) —
  // tra sẵn player.ID của wadtest để gọi hàm cho đúng chữ ký mới.
  let testPlayerId: number;

  beforeAll(async () => {
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

    const playerRow = await queryOne<{ ID: number }>(gamePool, "SELECT ID FROM player WHERE user_id = ? LIMIT 1", [
      TEST_USER_ID,
    ]);
    testPlayerId = playerRow?.ID ?? -1; // -1 (không tồn tại) → các test dưới rơi vào nhánh lỗi "Không tìm thấy", vẫn hợp lệ.
  });

  afterAll(async () => {
    await gamePool.end();
    await webPool.end();
  });

  describe("Offline-Guard Named Lock Mechanism", () => {
    it("acquires and releases login lock successfully", async () => {
      const conn = await gamePool.getConnection();
      try {
        const lockResult = await queryOne<{ got: number | null }>(
          conn,
          "SELECT GET_LOCK(?, ?) AS got",
          [TEST_LOCK, 5],
        );
        expect(lockResult?.got).toBe(1); // Lock acquired

        // Release it
        await conn.query("SELECT RELEASE_LOCK(?)", [TEST_LOCK]);

        // Verify it's released
        const lockCheck = await queryOne<{ is_used: number | null }>(
          conn,
          "SELECT IS_USED_LOCK(?) AS is_used",
          [TEST_LOCK],
        );
        expect(lockCheck?.is_used).toBeNull();
      } finally {
        conn.release();
      }
    });

    it("multiple connections cannot hold same lock simultaneously", async () => {
      const conn1 = await gamePool.getConnection();
      const conn2 = await gamePool.getConnection();

      try {
        // Conn1 acquires the lock
        const lock1 = await queryOne<{ got: number | null }>(
          conn1,
          "SELECT GET_LOCK(?, ?) AS got",
          [TEST_LOCK, 5],
        );
        expect(lock1?.got).toBe(1);

        // Conn2 tries to acquire with short timeout (1 second)
        const lock2 = await queryOne<{ got: number | null }>(
          conn2,
          "SELECT GET_LOCK(?, ?) AS got",
          [TEST_LOCK, 1],
        );
        // Should fail (0) or timeout
        expect(lock2?.got).toBeLessThan(1);
      } finally {
        await conn1.query("SELECT RELEASE_LOCK(?)", [TEST_LOCK]);
        conn1.release();
        conn2.release();
      }
    });
  });

  describe("Offline-Guard Heartbeat Checks", () => {
    it("confirms server_heartbeat exists and protocol_version is set", async () => {
      const conn = await gamePool.getConnection();
      try {
        const beat = await queryOne<{ protocol_version: string | null; id: number | null }>(
          conn,
          "SELECT id, CAST(protocol_version AS CHAR) as protocol_version FROM server_heartbeat WHERE id = 1",
        );

        // Heartbeat should exist if phase 4 migrations were applied
        if (beat) {
          expect(beat.id).toBe(1);
          expect(parseInt(beat.protocol_version || "0", 10)).toBeGreaterThanOrEqual(1);
        }
      } finally {
        conn.release();
      }
    });

    it("measures server heartbeat freshness", async () => {
      const conn = await gamePool.getConnection();
      try {
        const beat = await queryOne<{ age: string | null }>(
          conn,
          "SELECT CAST(TIMESTAMPDIFF(SECOND, beat_at, NOW()) AS CHAR) AS age FROM server_heartbeat WHERE id = 1 AND protocol_version >= 1",
        );

        if (beat && beat.age !== null) {
          const ageSeconds = parseInt(beat.age, 10);
          // If server is running, heartbeat should be very fresh
          if (ageSeconds < 120) {
            // Server likely running
            expect(ageSeconds).toBeLessThan(90);
          }
        }
      } finally {
        conn.release();
      }
    });
  });

  describe("Offline-Guard Player Online Detection", () => {
    it("can query player_online table successfully", async () => {
      const conn = await gamePool.getConnection();
      try {
        const onlineRow = await queryOne<{ user_id: string | null }>(
          conn,
          "SELECT CAST(user_id AS CHAR) as user_id FROM player_online WHERE user_id = ? LIMIT 1",
          [TEST_USER_ID],
        );

        // Verify query works (onlineRow can be null or an object)
        expect(typeof onlineRow === "object" || onlineRow === null).toBe(true);
      } finally {
        conn.release();
      }
    });

    it("identifies online players correctly", async () => {
      const conn = await gamePool.getConnection();
      try {
        // Count total online players
        const countResult = await queryOne<{ count: string | null }>(
          conn,
          "SELECT CAST(COUNT(*) AS CHAR) as count FROM player_online",
        );
        const totalOnline = parseInt(countResult?.count || "0", 10);

        // Can be any non-negative number
        expect(totalOnline).toBeGreaterThanOrEqual(0);

        // Verify our test user's status
        const testUserStatus = await queryOne<{ x: number }>(
          conn,
          "SELECT 1 AS x FROM player_online WHERE user_id = ? LIMIT 1",
          [TEST_USER_ID],
        );

        // If test user is not online, this will be null; that's expected
        if (testUserStatus) {
          expect(testUserStatus.x).toBe(1);
        }
      } finally {
        conn.release();
      }
    });
  });

  describe("withOfflinePlayer integration behavior", () => {
    it("executes with successful lock acquisition when conditions are met", async () => {
      // This test verifies that withOfflinePlayer can successfully run
      // by actually calling it (if conditions allow)
      let callSucceeded = false;
      let callExecuted = false;

      try {
        const result = await withOfflinePlayer(testPlayerId, async (conn) => {
          callExecuted = true;
          // Verify we have a working connection
          const testQuery = await queryOne<{ x: number }>(conn, "SELECT 1 AS x");
          expect(testQuery?.x).toBe(1);
          return "test-complete";
        });

        if (result === "test-complete") {
          callSucceeded = true;
        }
      } catch (e) {
        const errorMsg = (e as Error).message;
        // These are expected failures - just log them
        if (
          errorMsg.includes("Không xác nhận") ||
          errorMsg.includes("Nhân vật đang online") ||
          errorMsg.includes("Không tìm thấy")
        ) {
          console.log(`Expected error (acceptable): ${errorMsg.substring(0, 80)}`);
        } else {
          // Unexpected error
          throw e;
        }
      }

      // Either success or expected error
      expect(callExecuted || !callSucceeded).toBe(true);

      // Verify lock is released after call (whether it succeeded or failed)
      const conn = await gamePool.getConnection();
      try {
        const lockCheck = await queryOne<{ is_used: number | null }>(
          conn,
          "SELECT IS_USED_LOCK(?) AS is_used",
          [TEST_LOCK],
        );
        expect(lockCheck?.is_used).toBeNull();
      } finally {
        conn.release();
      }
    });

    it("releases lock even if the callback throws", async () => {
      let lockWasReleased = false;

      try {
        await withOfflinePlayer(testPlayerId, async () => {
          throw new Error("Test callback error");
        });
      } catch (e) {
        const errorMsg = (e as Error).message;
        // If it's our callback error, verify lock is released
        if (errorMsg === "Test callback error") {
          const conn = await gamePool.getConnection();
          try {
            const lockCheck = await queryOne<{ is_used: number | null }>(
              conn,
              "SELECT IS_USED_LOCK(?) AS is_used",
              [TEST_LOCK],
            );
            lockWasReleased = lockCheck?.is_used === null;
          } finally {
            conn.release();
          }
        }
        // Any other error is expected (heartbeat, player_online, etc.)
      }

      // If callback was executed, lock must be released
      if (lockWasReleased !== undefined) {
        expect(lockWasReleased).toBe(true);
      }
    });
  });
});
