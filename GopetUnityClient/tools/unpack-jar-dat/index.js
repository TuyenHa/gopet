#!/usr/bin/env node
/**
 * Giải asset của client J2ME cũ (`client.jar_Decompiler.com`) vào Unity:
 *   1. 5 kho ảnh `.dat` (xem dat-bank.js) -> PNG rời
 *   2. PNG rời sẵn có (tile map, icon pet, skill...) -> copy nguyên văn
 *   3. WAV (nhạc nền, hiệu ứng) -> copy nguyên văn
 *
 * Tất định và tái lập được: chạy lại luôn ra đúng byte đó. `--check` so sánh
 * chứ không ghi, dùng cho CI — giống lệ của gen-gopet-cmd.
 *
 * Dùng: node index.js [--check]
 *   --check  chỉ so sánh, không ghi. Exit 1 nếu thiếu file hoặc lệch nội dung.
 */

'use strict';

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { decodeBank } = require('./dat-bank');
const { patchWaterBanks } = require('./map-water-bank-patch');

const REPO_ROOT = path.resolve(__dirname, '..', '..', '..');
const JAR_DIR = path.join(REPO_ROOT, 'client.jar_Decompiler.com');
// Duoi Resources/ de Resources.Load(ten) doc duoc trong BUILD THAT (khong chi Editor).
// JarSkin/SoundBank doc theo ten chuoi luc runtime -> bat buoc phai la Resources.
const ART_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'Art');
const AUDIO_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'Audio');
// Bo cuc map (`maps/<n>.dat`, format rieng — xem ef.java). Doi thanh `.bytes` vi
// Unity chi nap duoc file nhi phan qua Resources.Load<TextAsset> khi duoi la .bytes;
// `.dat` bi Editor bo qua, khong sinh asset nao ca.
const MAPS_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'Maps');
// Metadata hoạt ảnh của object map (`dy.java`). Unity cần đuôi `.bytes` để nạp
// thành TextAsset; ảnh atlas tương ứng vẫn nằm ở Art/Raw/newMapData/<id>_a.png.
const MAP_ANIMATIONS_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'MapAnimations');
const BATTLE_ANIMATIONS_DIR = path.join(REPO_ROOT, 'GopetUnityClient', 'Assets', 'Resources', 'Jar', 'BattleAnimations');

/** 5 kho ảnh biết trước — không dò tự động, để phát hiện ngay khi jar có thêm/bớt. */
const BANKS = ['lg', 'common', 'avatar', 'buttonicon', 'mui'];

function sha256(buffer) {
    return crypto.createHash('sha256').update(buffer).digest('hex');
}

/** Tìm mọi file khớp đuôi mở rộng, đệ quy, trả về đường dẫn TƯƠNG ĐỐI so với `root`. */
function walk(root, ext, base = root, out = []) {
    for (const entry of fs.readdirSync(base, { withFileTypes: true })) {
        const full = path.join(base, entry.name);
        if (entry.isDirectory()) walk(root, ext, full, out);
        else if (entry.name.toLowerCase().endsWith(ext)) out.push(path.relative(root, full));
    }
    return out;
}

/** Kế hoạch đầy đủ: mọi file đích và nội dung của nó, tính từ nguồn — không đọc gì ở đích. */
function buildPlan() {
    const plan = [];

    for (const bank of BANKS) {
        const src = path.join(JAR_DIR, `${bank}.dat`);
        const { imageCount, images } = decodeBank(fs.readFileSync(src));
        images.forEach((bytes, i) => {
            plan.push({ dest: path.join(ART_DIR, bank, `${i}.png`), bytes, kind: `${bank}.dat[${i}]/${imageCount}` });
        });
    }

    for (const rel of walk(JAR_DIR, '.png')) {
        // PNG rời nằm ngoài mọi kho .dat — copy y nguyên để giữ cấu trúc thư mục gốc.
        plan.push({ dest: path.join(ART_DIR, 'Raw', rel), bytes: fs.readFileSync(path.join(JAR_DIR, rel)), kind: `raw png ${rel}` });
    }

    for (const rel of walk(path.join(JAR_DIR, 'maps'), '.dat')) {
        // 3 map bang duoc mo bo nuoc - xem map-water-bank-patch.js. Cac map khac giu nguyen byte.
        const bytes = patchWaterBanks(rel, fs.readFileSync(path.join(JAR_DIR, 'maps', rel)));
        plan.push({ dest: path.join(MAPS_DIR, rel.replace(/\.dat$/i, '.bytes')), bytes, kind: `map ${rel}` });
    }

    for (const rel of walk(path.join(JAR_DIR, 'newMapData'), '_b')) {
        const id = path.basename(rel, '_b');
        plan.push({
            dest: path.join(MAP_ANIMATIONS_DIR, `${id}.bytes`),
            bytes: fs.readFileSync(path.join(JAR_DIR, 'newMapData', rel)),
            kind: `map animation ${rel}`
        });
    }

    const battleRoot = path.join(JAR_DIR, 'pet', 'battle');
    for (const rel of walk(battleRoot, '')) {
        const source = path.join(battleRoot, rel);
        if (path.extname(rel)) continue;
        plan.push({
            dest: path.join(BATTLE_ANIMATIONS_DIR, `${rel}.bytes`),
            bytes: fs.readFileSync(source),
            kind: `battle animation ${rel}`
        });
    }

    for (const rel of walk(path.join(battleRoot, 'skills'), '.anu')) {
        plan.push({
            dest: path.join(BATTLE_ANIMATIONS_DIR, 'skills', rel.replace(/\.anu$/i, '.anu.bytes')),
            bytes: fs.readFileSync(path.join(battleRoot, 'skills', rel)),
            kind: `battle actor animation ${rel}`
        });
    }

    for (const rel of walk(path.join(JAR_DIR, 'sound'), '.wav')) {
        plan.push({ dest: path.join(AUDIO_DIR, rel), bytes: fs.readFileSync(path.join(JAR_DIR, 'sound', rel)), kind: `raw wav ${rel}` });
    }

    return plan;
}

function write(plan) {
    for (const item of plan) {
        fs.mkdirSync(path.dirname(item.dest), { recursive: true });
        fs.writeFileSync(item.dest, item.bytes);
    }
}

/** So từng file trong kế hoạch với đĩa. Không đối chiếu ngược (file thừa trên đĩa không bị bắt ở đây). */
function check(plan) {
    const problems = [];

    for (const item of plan) {
        if (!fs.existsSync(item.dest)) {
            problems.push(`THIẾU: ${path.relative(REPO_ROOT, item.dest)} (${item.kind})`);
            continue;
        }

        const onDisk = fs.readFileSync(item.dest);
        if (sha256(onDisk) !== sha256(item.bytes)) {
            problems.push(`LỆCH: ${path.relative(REPO_ROOT, item.dest)} (${item.kind}) — nội dung khác nguồn`);
        }
    }

    return problems;
}

function main() {
    const checkOnly = process.argv.includes('--check');

    if (!fs.existsSync(JAR_DIR)) {
        console.error(`Không tìm thấy nguồn: ${JAR_DIR}`);
        process.exit(1);
    }

    const plan = buildPlan();
    const bankImages = plan.filter((p) => p.kind.includes('.dat[')).length;
    const rawPngs = plan.filter((p) => p.kind.startsWith('raw png')).length;
    const wavs = plan.filter((p) => p.kind.startsWith('raw wav')).length;
    const maps = plan.filter((p) => p.kind.startsWith('map ') && !p.kind.startsWith('map animation ')).length;
    const mapAnimations = plan.filter((p) => p.kind.startsWith('map animation ')).length;
    const battleAnimations = plan.filter((p) => p.kind.startsWith('battle animation ')).length;
    const actorAnimations = plan.filter((p) => p.kind.startsWith('battle actor animation ')).length;

    if (checkOnly) {
        const problems = check(plan);
        if (problems.length > 0) {
            console.error(`${problems.length} vấn đề:\n` + problems.join('\n'));
            process.exit(1);
        }
        console.log(`OK — ${bankImages} ảnh từ .dat + ${rawPngs} PNG + ${wavs} WAV + ${maps} map + ${mapAnimations} map animation + ${battleAnimations} battle animation + ${actorAnimations} actor animation, khớp nguồn.`);
        return;
    }

    write(plan);
    console.log(
        `Đã giải ${bankImages} ảnh từ ${BANKS.length} file .dat, ` +
        `copy ${rawPngs} PNG + ${wavs} WAV + ${maps} map + ${mapAnimations} map animation + ${battleAnimations} battle animation + ${actorAnimations} actor animation -> ${path.relative(REPO_ROOT, path.dirname(ART_DIR))}`
    );
}

main();
