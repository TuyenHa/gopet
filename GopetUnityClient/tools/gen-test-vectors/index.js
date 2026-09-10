#!/usr/bin/env node
/**
 * Sinh test vector cho tầng transport, ghi ra tests/Gopet.Net.Tests/TestVectors.json.
 *
 * Vector được tính bằng tea-reference.js — bản port ĐỘC LẬP từ TEA.cs của server.
 * Bài test C# đối chiếu Assets/Scripts/Net/Tea.cs với các vector này. Hai bản port
 * sai giống hệt nhau là chuyện gần như không xảy ra, nên khớp = đúng wire.
 *
 * Dùng: node index.js
 */

const fs = require('fs');
const path = require('path');
const { TeaReference } = require('./tea-reference');

const OUT = path.resolve(__dirname, '..', '..', 'tests', 'Gopet.Net.Tests', 'TestVectors.json');

const toHex = (arr) => Buffer.from(arr).toString('hex');

/** Khoá handshake 9 byte đúng như eq.java:a(long) sinh ra. */
function handshakeBytes(key) {
    const buf = [9];
    for (let i = 0; i < 8; i++) {
        buf.push(Number((key >> BigInt(56 - i * 8)) & 0xffn));
    }
    return buf;
}

/** writeUTF của server: 2 byte độ dài big-endian + UTF-8. */
function encodeUtf(s) {
    const bytes = Buffer.from(s, 'utf8');
    return [bytes.length >> 8, bytes.length & 0xff, ...bytes];
}

function main() {
    // Khoá cố định để vector tái lập được. Giá trị thật là Date.now().
    const keys = [
        1740000000000n,
        1n,
        0n,
        -1n & 0xffffffffffffffffn,
        9007199254740991n,
    ];

    // Payload rỗng KHÔNG có trong danh sách: TEA.cs của server ném
    // IndexOutOfRangeException với đầu vào rỗng (pack() ghi dest[1] khi
    // dest.Length == 1). Không xảy ra thực tế vì mọi Message luôn có ít nhất
    // byte opcode. Client chặn tường minh — xem Tea.Encrypt.
    const payloads = [
        { name: 'single-opcode', bytes: [0x01] },
        { name: 'exactly-8', bytes: [1, 2, 3, 4, 5, 6, 7, 8] },
        { name: 'nine-bytes', bytes: [1, 2, 3, 4, 5, 6, 7, 8, 9] },
        // CLIENT_INFO = -36 -> 0xDC. Gói thật đầu tiên client gửi.
        { name: 'client-info-opcode', bytes: [0xdc, 0x00, 0x00, 0x00, 0x00, 0x04] },
        { name: 'ascending-64', bytes: Array.from({ length: 64 }, (_, i) => i) },
        { name: 'all-0xff', bytes: new Array(16).fill(0xff) },
    ];

    const teaVectors = [];
    for (const key of keys) {
        const tea = new TeaReference(key);
        for (const p of payloads) {
            const encrypted = tea.encrypt(p.bytes);
            const decrypted = tea.decrypt(encrypted);

            // Tự kiểm: bản reference phải round-trip được, nếu không thì vector vô nghĩa.
            if (toHex(decrypted) !== toHex(p.bytes)) {
                throw new Error(`Reference TEA không round-trip được: key=${key} payload=${p.name}`);
            }

            teaVectors.push({
                key: key.toString(),
                payloadName: p.name,
                plainHex: toHex(p.bytes),
                encryptedHex: toHex(encrypted),
            });
        }
    }

    const handshakeVectors = keys.map((k) => ({
        key: k.toString(),
        bytesHex: toHex(handshakeBytes(k)),
    }));

    const utfVectors = [
        '',
        'admin',
        'Bảo trì cập nhật chỉ số boss',
        'Thú cưng',
        'goPet — 1.4.3',
        'a'.repeat(300),
    ].map((s) => ({ value: s, encodedHex: toHex(encodeUtf(s)) }));

    // Khoá TEA lấy từ 8 byte cuối của handshake — kiểm tra client và server
    // dựng ra cùng một khoá.
    const keyDerivation = keys.map((k) => {
        const hs = handshakeBytes(k);
        let derived = 0n;
        for (let i = 1; i <= 8; i++) {
            derived = (derived << 8n) | BigInt(hs[i]);
        }
        return { originalKey: k.toString(), handshakeHex: toHex(hs), derivedKey: derived.toString() };
    });

    const out = {
        _comment:
            'Sinh bởi tools/gen-test-vectors. KHÔNG sửa tay. ' +
            'Nguồn thuật toán: SRCGOPETGOC/GServer/Server/IO/TEA.cs',
        tea: teaVectors,
        handshake: handshakeVectors,
        utf: utfVectors,
        keyDerivation,
    };

    fs.mkdirSync(path.dirname(OUT), { recursive: true });
    fs.writeFileSync(OUT, JSON.stringify(out, null, 2), 'utf8');

    console.log(`Đã sinh vector -> ${OUT}`);
    console.log(`  TEA:            ${teaVectors.length}`);
    console.log(`  Handshake:      ${handshakeVectors.length}`);
    console.log(`  UTF:            ${utfVectors.length}`);
    console.log(`  Key derivation: ${keyDerivation.length}`);
}

main();
