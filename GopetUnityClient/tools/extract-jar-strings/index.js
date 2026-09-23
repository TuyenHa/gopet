#!/usr/bin/env node
/**
 * Bóc bảng chuỗi VN + EN từ `a.java` của client J2ME cũ vào hai file JSON dùng
 * được ở runtime (<c>Resources.Load&lt;TextAsset&gt;</c>).
 *
 * `a.java` có một hàm `public static String a(int)` với HAI khối
 * `switch (var0) { case N: return "..."; ... }` lồng trong `switch (a)` (biến
 * ngôn ngữ: 0 = VN, 1 = EN). Xem README cùng thư mục để biết vì sao không gõ tay.
 *
 * Dùng: node index.js [--check]
 *   --check  chỉ so sánh, không ghi. Exit 1 nếu lệch hoặc jar nguồn đổi.
 */

'use strict';

const fs = require('fs');
const path = require('path');

const REPO_ROOT = path.resolve(__dirname, '..', '..', '..');
const SOURCE = path.join(REPO_ROOT, 'client.jar_Decompiler.com', 'a.java');
const OUT_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'Strings');

const METHOD_START = 'public static String a(int var0) {';
const BLOCK_END = 'return String.valueOf(var0);';

/** `case 123:\n   return "chuỗi có \" và \n";` — bắt cả escape của Java. */
const CASE_RE = /case\s+(\d+):\s*\r?\n\s*return\s+"((?:[^"\\]|\\.)*)"\s*;/g;

/** Java chỉ dùng \n \t \" \\ \r trong bảng này (kiểm bằng --check nếu jar đổi). */
function unescapeJava(text) {
    return text.replace(/\\(.)/g, (_, ch) => {
        switch (ch) {
            case 'n': return '\n';
            case 't': return '\t';
            case 'r': return '\r';
            case '"': return '"';
            case '\\': return '\\';
            default: throw new Error(`Escape lạ chưa xử lý: \\${ch}`);
        }
    });
}

function extractCases(block) {
    const entries = {};
    let m;
    CASE_RE.lastIndex = 0;
    while ((m = CASE_RE.exec(block)) !== null) {
        entries[m[1]] = unescapeJava(m[2]);
    }
    return entries;
}

function parse(source) {
    const start = source.indexOf(METHOD_START);
    if (start < 0) throw new Error(`Không tìm thấy "${METHOD_START}" trong a.java.`);

    const firstEnd = source.indexOf(BLOCK_END, start);
    const secondEnd = source.indexOf(BLOCK_END, firstEnd + BLOCK_END.length);
    if (firstEnd < 0 || secondEnd < 0) {
        throw new Error('Không tìm đủ hai khối "default: return String.valueOf(var0);" — cấu trúc a.java đã đổi.');
    }

    const viBlock = source.slice(start, firstEnd);
    const enBlock = source.slice(firstEnd, secondEnd);

    const vi = extractCases(viBlock);
    const en = extractCases(enBlock);

    if (Object.keys(vi).length === 0 || Object.keys(en).length === 0) {
        throw new Error('Bóc được 0 chuỗi — regex không khớp, kiểm tra lại CASE_RE.');
    }

    return { vi, en };
}

function emit(table) {
    // Sắp theo số để diff dễ đọc; JSON.stringify không tự sắp key số.
    const sorted = {};
    for (const key of Object.keys(table).sort((a, b) => Number(a) - Number(b))) {
        sorted[key] = table[key];
    }
    return JSON.stringify(sorted, null, 2) + '\n';
}

function matchesGenerated(actual, expected) {
    // Normalize checkout line endings only. Escaped newlines inside JSON string
    // values remain untouched, so changes to the actual translations still fail.
    return actual.replace(/\r\n/g, '\n') === expected;
}

function main() {
    const checkOnly = process.argv.includes('--check');

    if (!fs.existsSync(SOURCE)) {
        console.error(`Không tìm thấy nguồn: ${SOURCE}`);
        process.exit(1);
    }

    const { vi, en } = parse(fs.readFileSync(SOURCE, 'utf8'));
    const viJson = emit(vi);
    const enJson = emit(en);

    const viPath = path.join(OUT_DIR, 'strings-vi.json');
    const enPath = path.join(OUT_DIR, 'strings-en.json');

    if (checkOnly) {
        const viOk = fs.existsSync(viPath) && matchesGenerated(fs.readFileSync(viPath, 'utf8'), viJson);
        const enOk = fs.existsSync(enPath) && matchesGenerated(fs.readFileSync(enPath, 'utf8'), enJson);
        if (!viOk || !enOk) {
            console.error('Bảng chuỗi đã lệch so với a.java. Chạy: node index.js');
            process.exit(1);
        }
        console.log(`OK — ${Object.keys(vi).length} chuỗi VN, ${Object.keys(en).length} chuỗi EN, khớp nguồn.`);
        return;
    }

    fs.mkdirSync(OUT_DIR, { recursive: true });
    fs.writeFileSync(viPath, viJson, 'utf8');
    fs.writeFileSync(enPath, enJson, 'utf8');
    console.log(`Đã bóc ${Object.keys(vi).length} chuỗi VN, ${Object.keys(en).length} chuỗi EN -> ${path.relative(REPO_ROOT, OUT_DIR)}`);

    if (Object.keys(vi).length !== Object.keys(en).length) {
        console.warn(
            `Cảnh báo: VN và EN LỆCH SỐ LƯỢNG (${Object.keys(vi).length} vs ${Object.keys(en).length}). ` +
            'Đã biết trong bản jar gốc — không phải lỗi bóc tách, xem README.'
        );
    }
}

module.exports = { parse, emit, matchesGenerated };
if (require.main === module) main();
