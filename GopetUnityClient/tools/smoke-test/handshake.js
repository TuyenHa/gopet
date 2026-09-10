/**
 * Kiem tra end-to-end: GServer co chap nhan handshake 9 byte khong.
 *
 * Dung dung logic ma Assets/Scripts/Net/GopetSocket.cs cai dat, viet lai bang
 * Node de kiem chung doc lap voi ban C#.
 *
 * Server sau handshake se doc goi tin. Ta gui CLIENT_INFO (opcode -36 = 0xDC)
 * da ma hoa TEA va cho phan hoi -36 voi 1 byte: 1 = chap nhan.
 */

const net = require('net');
const { TeaReference } = require('../gen-test-vectors/tea-reference.js');

const HOST = process.env.GOPET_HOST || '127.0.0.1';
const PORT = Number(process.env.GOPET_PORT || 19180);

// --- writeUTF cua server: 2 byte do dai big-endian + UTF-8 ---
function utf(s) {
    const b = Buffer.from(s, 'utf8');
    return Buffer.concat([Buffer.from([b.length >> 8, b.length & 0xff]), b]);
}
const i32 = (v) => Buffer.from([(v >> 24) & 0xff, (v >> 16) & 0xff, (v >> 8) & 0xff, v & 0xff]);

const sock = net.connect(PORT, HOST, () => {
    console.log(`[OK]   Ket noi ${HOST}:${PORT}`);

    // --- Handshake: [0]=9, [1..8]=key big-endian ---
    const key = BigInt(Date.now());
    const hs = Buffer.alloc(9);
    hs[0] = 9;
    for (let i = 0; i < 8; i++) hs[i + 1] = Number((key >> BigInt(56 - i * 8)) & 0xffn);
    sock.write(hs);
    console.log(`[OK]   Gui handshake: ${hs.toString('hex')}`);

    const tea = new TeaReference(key);

    // --- CLIENT_INFO (opcode -36 = 0xDC), thu tu field theo Player.cs:104-129 ---
    const payload = Buffer.concat([
        Buffer.from([0xdc]),   // opcode
        Buffer.from([0]),      // CLIENT_TYPE
        i32(4),                // PROVIDER (tu MANIFEST.MF)
        utf('1.4.3'),          // version -> phai >= VERSION_142 (1.4.2)
        utf('unity-test'),     // info
        i32(1280),             // displayWidth
        i32(720),              // displayHeight
        utf('vi'),             // languageCode -> phai co trong GopetManager.Language
        utf('ref-test'),       // Refcode
    ]);

    const enc = Buffer.from(tea.encrypt([...payload]));
    sock.write(Buffer.concat([i32(enc.length + 1), Buffer.from([1]), enc]));
    console.log(`[OK]   Gui CLIENT_INFO (${payload.length} byte -> ${enc.length} byte da ma hoa)`);
});

let buf = Buffer.alloc(0);
sock.on('data', (d) => {
    buf = Buffer.concat([buf, d]);
    while (buf.length >= 5) {
        const len = buf.readInt32BE(0);
        const total = 4 + 1 + (len - 1);
        if (buf.length < total) break;

        const encrypted = buf[4];
        const body = buf.subarray(5, total);
        buf = buf.subarray(total);

        const op = body.readInt8(0);
        console.log(`[RECV] opcode=${op} enc=${encrypted} len=${body.length} hex=${body.toString('hex').slice(0, 60)}`);

        if (op === -36) {
            const accepted = body[1] === 1;
            console.log(accepted
                ? '[PASS] Server CHAP NHAN client (setClientOK=1)'
                : '[FAIL] Server TU CHOI client (setClientOK=0) - kiem tra version/languageCode');
            sock.end();
            process.exit(accepted ? 0 : 1);
        }
    }
});

sock.on('close', () => console.log('[INFO] Ket noi dong'));
sock.on('error', (e) => { console.log(`[FAIL] ${e.message}`); process.exit(1); });
setTimeout(() => { console.log('[FAIL] Het 10s khong co phan hoi'); process.exit(1); }, 10000);
