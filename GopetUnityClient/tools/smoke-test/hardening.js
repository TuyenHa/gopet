/**
 * Kiem tra GServer chiu duoc goi tin di dang.
 *
 * Truoc khi va, GameController.cs:215 va :784 cap phat mang theo do dai do
 * client gui: `new int[message.reader().readInt()]`. Mot int co 2 ty lam
 * may chu nem OutOfMemoryException. Voi ON_OTHER_USER_MOVE con te hon vi
 * mang do duoc phat lai cho ca khu vuc qua sendMove - mot goi vao, N goi ra.
 *
 * Moi ca kiem tra: may chu dong ket noi cua ke tan cong, va VAN SONG de
 * phuc vu client hop le ngay sau do.
 *
 *   node hardening.js
 */

const net = require('net');
const { TeaReference } = require('../gen-test-vectors/tea-reference.js');

const HOST = process.env.GOPET_HOST || '127.0.0.1';
const PORT = Number(process.env.GOPET_PORT || 19180);

const i32 = (v) => Buffer.from([(v >> 24) & 0xff, (v >> 16) & 0xff, (v >> 8) & 0xff, v & 0xff]);
function utf(s) {
    const b = Buffer.from(s, 'utf8');
    return Buffer.concat([Buffer.from([b.length >> 8, b.length & 0xff]), b]);
}

const CLIENT_INFO = 0xdc;          // -36
const ON_OTHER_USER_MOVE = 27;
const COMMAND_GUIDER = 122;
const TYPE_DIALOG_INPUT = 7;

// Server.cs:62 chi cho 1 ket noi moi IP trong 2 giay. Reconnect nhanh hon se
// bi tu choi ngay tang accept, va nhin tu ben ngoai giong het may chu da chet.
// Cho lau hon nguong mot chut giua cac lan ket noi.
const CONNECT_THROTTLE_MS = 2500;
const wait = (ms) => new Promise((r) => setTimeout(r, ms));

function clientInfoPayload() {
    return Buffer.concat([
        Buffer.from([CLIENT_INFO]),
        Buffer.from([0]),
        i32(4),
        utf('1.4.3'),
        utf('hardening-test'),
        i32(1280),
        i32(720),
        utf('vi'),
        utf('ref-test'),
    ]);
}

/** Mo ket noi, handshake, gui CLIENT_INFO, roi gui payload tan cong. */
function attack(name, buildPayload) {
    return new Promise((resolve) => {
        const sock = net.connect(PORT, HOST);
        let closed = false;
        let sawResponse = false;

        const timer = setTimeout(() => {
            if (!closed) {
                sock.destroy();
                resolve({ name, verdict: 'FAIL', detail: 'may chu GIU ket noi (dang le phai dong)' });
            }
        }, 5000);

        sock.on('connect', () => {
            const key = BigInt(Date.now());
            const hs = Buffer.alloc(9);
            hs[0] = 9;
            for (let i = 0; i < 8; i++) hs[i + 1] = Number((key >> BigInt(56 - i * 8)) & 0xffn);
            sock.write(hs);

            const tea = new TeaReference(key);
            const send = (payload) => {
                const enc = Buffer.from(tea.encrypt([...payload]));
                sock.write(Buffer.concat([i32(enc.length + 1), Buffer.from([1]), enc]));
            };

            send(clientInfoPayload());
            setTimeout(() => send(buildPayload()), 300);
        });

        sock.on('data', () => { sawResponse = true; });

        sock.on('close', () => {
            closed = true;
            clearTimeout(timer);
            resolve({ name, verdict: 'PASS', detail: `may chu dong ket noi (co phan hoi truoc do: ${sawResponse})` });
        });

        sock.on('error', () => {
            closed = true;
            clearTimeout(timer);
            resolve({ name, verdict: 'PASS', detail: 'ket noi bi ngat' });
        });
    });
}

/**
 * Sau moi don tan cong, may chu phai con phuc vu duoc client hop le.
 *
 * Gom du frame roi moi parse. Ban dau ham nay doc thang chunk TCP dau tien
 * va bao sai la "may chu chet" trong khi no van chay - TCP khong dam bao
 * mot lan ghi den nguyen mot lan doc.
 */
function healthCheck() {
    return new Promise((resolve) => {
        const sock = net.connect(PORT, HOST);
        let buf = Buffer.alloc(0);
        let done = false;

        const finish = (ok, why) => {
            if (done) return;
            done = true;
            clearTimeout(timer);
            sock.destroy();
            resolve({ ok, why });
        };

        const timer = setTimeout(() => finish(false, 'het 5s khong co phan hoi'), 5000);

        sock.on('connect', () => {
            const key = BigInt(Date.now());
            const hs = Buffer.alloc(9);
            hs[0] = 9;
            for (let i = 0; i < 8; i++) hs[i + 1] = Number((key >> BigInt(56 - i * 8)) & 0xffn);
            sock.write(hs);
            const tea = new TeaReference(key);
            const p = clientInfoPayload();
            const enc = Buffer.from(tea.encrypt([...p]));
            sock.write(Buffer.concat([i32(enc.length + 1), Buffer.from([1]), enc]));
        });

        sock.on('data', (d) => {
            buf = Buffer.concat([buf, d]);
            while (buf.length >= 5) {
                const len = buf.readInt32BE(0);
                const total = 4 + 1 + (len - 1);
                if (buf.length < total) return;      // chua du, doi them
                const body = buf.subarray(5, total);
                buf = buf.subarray(total);
                if (body.readInt8(0) === -36) {
                    finish(body[1] === 1, body[1] === 1 ? 'chap nhan' : 'tu choi client');
                    return;
                }
            }
        });

        sock.on('close', () => finish(false, 'may chu dong ket noi ma khong tra loi'));
        sock.on('error', (e) => finish(false, `loi socket: ${e.message}`));
    });
}

const attacks = [
    {
        name: 'points do dai 2 ty (OOM + khuech dai)',
        build: () => Buffer.concat([
            Buffer.from([ON_OTHER_USER_MOVE]),
            i32(1), Buffer.from([0]), i32(1),
            i32(2000000000),
        ]),
    },
    {
        name: 'points do dai 0 (truoc day panic o points[len-2])',
        build: () => Buffer.concat([
            Buffer.from([ON_OTHER_USER_MOVE]),
            i32(1), Buffer.from([0]), i32(1),
            i32(0),
        ]),
    },
    {
        name: 'points do dai am',
        build: () => Buffer.concat([
            Buffer.from([ON_OTHER_USER_MOVE]),
            i32(1), Buffer.from([0]), i32(1),
            i32(-1),
        ]),
    },
    {
        name: 'texts do dai 2 ty (TYPE_DIALOG_INPUT)',
        build: () => Buffer.concat([
            Buffer.from([COMMAND_GUIDER]),
            Buffer.from([TYPE_DIALOG_INPUT]),
            i32(1),
            i32(2000000000),
        ]),
    },
];

(async () => {
    console.log(`Muc tieu ${HOST}:${PORT}\n`);

    // Cho truoc ca lan kiem tra dau tien: script khac (vd handshake.js) vua
    // chay xong se lam throttle 2 giay/IP cua Server.cs:62 tu choi ket noi nay,
    // va bao ECONNABORTED - trong giong het may chu chet.
    await wait(CONNECT_THROTTLE_MS);

    const pre = await healthCheck();
    if (!pre.ok) {
        console.log('[FAIL] May chu khong phan hoi truoc khi test: ' + pre.why);
        process.exit(1);
    }
    console.log('[OK]   May chu song truoc khi test\n');

    let failed = 0;
    for (const a of attacks) {
        await wait(CONNECT_THROTTLE_MS);
        const r = await attack(a.name, a.build);
        console.log(`[${r.verdict}] ${r.name}\n         ${r.detail}`);
        if (r.verdict === 'FAIL') failed++;

        await wait(CONNECT_THROTTLE_MS);
        const alive = await healthCheck();
        console.log(`         may chu con song sau don nay: ${alive.ok ? 'CO' : 'KHONG (' + alive.why + ')'}`);
        if (!alive.ok) {
            console.log('\n[FAIL] May chu ngung phuc vu - lo hong chua duoc va.');
            process.exit(1);
        }
        console.log('');
    }

    console.log(failed === 0
        ? 'TAT CA PASS - may chu tu choi goi di dang va van phuc vu binh thuong.'
        : `${failed} ca THAT BAI.`);
    process.exit(failed === 0 ? 0 : 1);
})();
