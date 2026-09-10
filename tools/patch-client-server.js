#!/usr/bin/env node
/**
 * Tro client.jar goc ve mot may chu khac, bang cach vá chuoi dia chi trong
 * constant pool cua fb.class.
 *
 * TAI SAO CAN: client khong co man hinh nhap dia chi. Khi RMS chua co record
 * "server_list" (lan chay dau), fb.java:335 fallback ve dung mot entry hardcode:
 *
 *     dw var7 = new dw("test", "160.30.136.115", 19180);
 *
 * TAI SAO AN TOAN: class file cua Java tham chieu constant pool bang INDEX,
 * khong phai offset byte. Doi do dai mot entry CONSTANT_Utf8 lam file dai/ngan
 * di nhung khong lam hong tham chieu nao.
 *
 * Tim theo mau tag+length nen gan nhu khong the nham:
 *     0x01 <u2 do_dai> <bytes>
 *
 * Dung:
 *   node patch-client-server.js <client.jar> <dia-chi-moi> [output.jar]
 *
 * Vi du:
 *   node patch-client-server.js ../SRCGOPETGOC/client.jar 127.0.0.1
 *
 * File goc KHONG bi sua - script luon ghi ra ban sao.
 */

const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const os = require('os');

const ORIGINAL_HOST = '160.30.136.115';
const CLASS_IN_JAR = 'fb.class';

function patchUtf8Constant(buf, oldStr, newStr) {
    const oldBytes = Buffer.from(oldStr, 'utf8');
    const newBytes = Buffer.from(newStr, 'utf8');

    // CONSTANT_Utf8_info: tag=1, length=u2 big-endian, roi la bytes
    const needle = Buffer.concat([
        Buffer.from([0x01, (oldBytes.length >> 8) & 0xff, oldBytes.length & 0xff]),
        oldBytes,
    ]);

    const at = buf.indexOf(needle);
    if (at === -1) return null;

    // Kiem tra khong co lan xuat hien thu hai - neu co thi phai xem lai
    // truoc khi va, chu khong doan.
    if (buf.indexOf(needle, at + 1) !== -1) {
        throw new Error(`Tim thay nhieu hon mot entry "${oldStr}" - can kiem tra thu cong`);
    }

    const replacement = Buffer.concat([
        Buffer.from([0x01, (newBytes.length >> 8) & 0xff, newBytes.length & 0xff]),
        newBytes,
    ]);

    return Buffer.concat([
        buf.subarray(0, at),
        replacement,
        buf.subarray(at + needle.length),
    ]);
}

function main() {
    const [jarPath, newHost, outArg] = process.argv.slice(2);

    if (!jarPath || !newHost) {
        console.error('Dung: node patch-client-server.js <client.jar> <dia-chi-moi> [output.jar]');
        console.error('Vi du: node patch-client-server.js ../SRCGOPETGOC/client.jar 127.0.0.1');
        process.exit(2);
    }

    if (!fs.existsSync(jarPath)) {
        console.error(`Khong thay ${jarPath}`);
        process.exit(1);
    }

    // Do dai moi phai vua 65535 (gioi han u2 cua CONSTANT_Utf8)
    if (Buffer.byteLength(newHost, 'utf8') > 65535) {
        console.error('Dia chi qua dai');
        process.exit(1);
    }

    const out = outArg || path.join(
        path.dirname(jarPath),
        `client-${newHost.replace(/[^\w.]/g, '_')}.jar`
    );

    fs.copyFileSync(jarPath, out);
    console.log(`[patch] Ban sao: ${out}`);

    const work = fs.mkdtempSync(path.join(os.tmpdir(), 'gopet-patch-'));

    try {
        // Rut fb.class ra bang `jar` cua JDK
        execFileSync('jar', ['--extract', '--file', path.resolve(jarPath), CLASS_IN_JAR], { cwd: work });

        const classPath = path.join(work, CLASS_IN_JAR);
        const before = fs.readFileSync(classPath);

        const after = patchUtf8Constant(before, ORIGINAL_HOST, newHost);
        if (!after) {
            console.error(`[patch] Khong tim thay "${ORIGINAL_HOST}" trong ${CLASS_IN_JAR}.`);
            console.error('        Co the client.jar la ban khac. Kiem tra lai fb.java:335 trong ban decompile.');
            process.exit(1);
        }

        fs.writeFileSync(classPath, after);
        console.log(`[patch] ${CLASS_IN_JAR}: "${ORIGINAL_HOST}" -> "${newHost}" (${before.length} -> ${after.length} byte)`);

        // Nhet class da va tro lai jar
        execFileSync('jar', ['--update', '--file', path.resolve(out), '-C', work, CLASS_IN_JAR]);
        console.log('[patch] Da dong goi lai');

        // Kiem chung: doc lai tu jar da va va xac nhan chuoi moi co mat
        const verifyDir = fs.mkdtempSync(path.join(os.tmpdir(), 'gopet-verify-'));
        execFileSync('jar', ['--extract', '--file', path.resolve(out), CLASS_IN_JAR], { cwd: verifyDir });
        const verified = fs.readFileSync(path.join(verifyDir, CLASS_IN_JAR));
        fs.rmSync(verifyDir, { recursive: true, force: true });

        const hasNew = verified.includes(Buffer.from(newHost, 'utf8'));
        const hasOld = verified.includes(Buffer.from(ORIGINAL_HOST, 'utf8'));

        console.log(`[patch] Kiem chung: co "${newHost}"=${hasNew}, con "${ORIGINAL_HOST}"=${hasOld}`);

        if (!hasNew || hasOld) {
            console.error('[patch] THAT BAI: jar sau khi va khong dung nhu mong doi');
            process.exit(1);
        }

        console.log(`\n[patch] XONG. Chay bang:`);
        console.log(`  java -jar tools/freej2me/build/freej2me_plus.jar "${path.resolve(out)}"`);
    } finally {
        fs.rmSync(work, { recursive: true, force: true });
    }
}

main();
