#!/usr/bin/env node
/**
 * So sánh hai file packet dump, tìm chỗ lệch đầu tiên.
 *
 * Đây là công cụ chính để bắt desync giao thức — rủi ro số 1 của cả dự án.
 * Cách dùng: làm CÙNG một thao tác trên client J2ME cũ và client Unity mới,
 * rồi diff hai dump. Chuỗi opcode phải khớp; lệch ở đâu thấy ngay ở đó.
 *
 * Dùng: node index.js <dump-a> <dump-b> [--direction IN|OUT] [--hex] [--opcodes-only]
 *
 * Thoát: 0 = khớp, 1 = lệch, 2 = không so được (tham số sai, không có gói nào).
 *
 * Format dòng (khớp PacketLogger.cs cả hai đầu):
 *   {timestamp}\t{IN|OUT}\t{opcode}\t{enc}\t{len}\t{hex}
 */

const fs = require('fs');

function parseDump(file, directionFilter) {
    // StreamWriter của .NET ghi BOM khi dùng Encoding.UTF8, nên dòng header
    // bắt đầu bằng ﻿ chứ không phải '#'. Cắt bỏ trước khi tách dòng.
    const raw = fs.readFileSync(file, 'utf8').replace(/^﻿/, '');
    const lines = raw.split(/\r?\n/);
    const packets = [];

    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        if (!line || line.startsWith('#')) continue;

        const parts = line.split('\t');
        if (parts.length < 6) {
            console.warn(`  ! ${file}:${i + 1} dòng sai format, bỏ qua`);
            continue;
        }

        const [timestamp, dir, opcode, enc, len, hex] = parts;
        if (directionFilter && dir !== directionFilter) continue;

        packets.push({
            line: i + 1,
            timestamp,
            dir,
            opcode: parseInt(opcode, 10),
            enc: enc === '1',
            len: parseInt(len, 10),
            hex,
        });
    }

    return packets;
}

function describe(p) {
    if (!p) return '<hết dump>';
    return `dòng ${p.line}: ${p.dir} opcode=${p.opcode} enc=${p.enc ? 1 : 0} len=${p.len}`;
}

/** Vị trí byte đầu tiên khác nhau giữa hai chuỗi hex. */
function firstHexDiff(a, b) {
    const n = Math.min(a.length, b.length);
    for (let i = 0; i < n; i += 2) {
        if (a[i] !== b[i] || a[i + 1] !== b[i + 1]) return i / 2;
    }
    return a.length === b.length ? -1 : n / 2;
}

function main() {
    const args = process.argv.slice(2);
    const showHex = args.includes('--hex');

    // Chỉ so CHUỖI OPCODE, bỏ qua độ dài. Dùng khi hai client cố tình gửi dữ
    // liệu khác nhau ở cùng một gói — ví dụ CLIENT_INFO mang chuỗi mô tả nền
    // tảng và độ phân giải, vốn không thể giống nhau giữa J2ME và Unity.
    // Không có cờ này thì diff dừng ngay ở gói đầu vì lệch độ dài, và ta mất
    // khả năng kiểm điều thực sự cần kiểm: thứ tự gói có giống nhau không.
    const opcodesOnly = args.includes('--opcodes-only');

    const dirIdx = args.indexOf('--direction');
    const directionFilter = dirIdx >= 0 ? args[dirIdx + 1] : null;

    // Bỏ cả cờ lẫn GIÁ TRỊ của nó. Lọc mỗi "không bắt đầu bằng --" sẽ coi
    // "IN" trong "--direction IN" là tên file, rồi than sai số lượng tham số.
    const files = args.filter((a, i) => {
        if (a.startsWith('--')) return false;
        if (dirIdx >= 0 && i === dirIdx + 1) return false;
        return true;
    });

    if (files.length !== 2) {
        console.error('Dùng: node index.js <dump-a> <dump-b> [--direction IN|OUT] [--hex] [--opcodes-only]');
        process.exit(2);
    }

    if (directionFilter && directionFilter !== 'IN' && directionFilter !== 'OUT') {
        // Viết thường thì lọc sạch mọi gói, và "0 gói khớp 0 gói" trông y hệt
        // một lần chạy thành công. Chặn thẳng còn hơn để nó xanh giả.
        console.error(`--direction phải là IN hoặc OUT, nhận được "${directionFilter}"`);
        process.exit(2);
    }

    const [fileA, fileB] = files;
    const a = parseDump(fileA, directionFilter);
    const b = parseDump(fileB, directionFilter);

    console.log(`A: ${fileA}  (${a.length} gói)`);
    console.log(`B: ${fileB}  (${b.length} gói)`);
    if (directionFilter) console.log(`Lọc hướng: ${directionFilter}`);
    console.log('');

    const n = Math.max(a.length, b.length);

    if (n === 0) {
        // Không có gì để so thì không chứng minh được gì. Báo OK ở đây là dối.
        console.error('Không có gói nào để so (sai đường dẫn, hay lọc hướng không khớp?).');
        process.exit(2);
    }

    for (let i = 0; i < n; i++) {
        const pa = a[i];
        const pb = b[i];

        if (!pa || !pb) {
            console.log(`LỆCH tại gói #${i + 1} — số lượng gói khác nhau`);
            console.log(`  A: ${describe(pa)}`);
            console.log(`  B: ${describe(pb)}`);
            process.exit(1);
        }

        if (pa.opcode !== pb.opcode) {
            console.log(`LỆCH tại gói #${i + 1} — opcode khác nhau`);
            console.log(`  A: ${describe(pa)}`);
            console.log(`  B: ${describe(pb)}`);
            process.exit(1);
        }

        if (pa.enc !== pb.enc) {
            // Cùng một gói mà một bên mã hoá một bên không là lệch thật, không
            // phải khác dữ liệu — nên kiểm cả ở chế độ --opcodes-only.
            console.log(`LỆCH tại gói #${i + 1} — cờ mã hoá khác nhau (opcode ${pa.opcode})`);
            console.log(`  A: ${describe(pa)}`);
            console.log(`  B: ${describe(pb)}`);
            process.exit(1);
        }

        if (pa.len !== pb.len && !opcodesOnly) {
            console.log(`LỆCH tại gói #${i + 1} — độ dài khác nhau (opcode ${pa.opcode})`);
            console.log(`  A: ${describe(pa)}`);
            console.log(`  B: ${describe(pb)}`);
            if (showHex) {
                console.log(`  A hex: ${pa.hex}`);
                console.log(`  B hex: ${pb.hex}`);
            }
            process.exit(1);
        }

        if (showHex && pa.hex !== pb.hex) {
            const at = firstHexDiff(pa.hex, pb.hex);
            console.log(`KHÁC NỘI DUNG tại gói #${i + 1} (opcode ${pa.opcode}), byte ${at}`);
            console.log(`  A: ${pa.hex}`);
            console.log(`  B: ${pb.hex}`);
            // Nội dung khác chưa chắc là lỗi (timestamp, khoá ngẫu nhiên,
            // tên nhân vật...). Chỉ cảnh báo, không dừng.
        }
    }

    if (opcodesOnly) {
        console.log(`OK — ${n} gói khớp về chuỗi opcode (bỏ qua độ dài).`);
        return;
    }

    console.log(`OK — ${n} gói khớp về opcode và độ dài.`);
    if (!showHex) console.log('Thêm --hex để so cả nội dung.');
}

main();
