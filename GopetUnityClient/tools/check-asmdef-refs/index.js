#!/usr/bin/env node
/**
 * Kiem tra moi asmdef co khai bao du tham chieu cho nhung namespace no thuc su dung.
 *
 * VI SAO CAN: cac du an compile-only (Gopet.*.UnityCompat) gop nhieu thu muc vao
 * MOT assembly, nen ranh gioi asmdef bien mat va chung khong the thay loi kieu
 * "Gopet.Runtime dung Gopet.UiLogic ma quen khai bao". Unity thi ton trong ranh
 * gioi do va tu choi compile - phat hien luc chay PlayMode test thi da muon, vi
 * luc ay Editor da phai dong.
 *
 * Dung: node index.js
 * Thoat: 0 = du, 1 = thieu tham chieu, 2 = khong doc duoc gi.
 */

const fs = require('fs');
const path = require('path');

const ASSETS = path.resolve(__dirname, '..', '..', 'Assets');

/** Tim moi file khop duoi mo rong, de quy. */
function walk(dir, ext, out = []) {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) walk(full, ext, out);
        else if (entry.name.endsWith(ext)) out.push(full);
    }
    return out;
}

const asmdefFiles = walk(ASSETS, '.asmdef');
if (asmdefFiles.length === 0) {
    console.error('Khong tim thay asmdef nao duoi Assets/');
    process.exit(2);
}

// Ban do: namespace goc -> ten assembly. Chi quan tam asmdef cua chinh du an.
const byNamespace = new Map();
const assemblies = [];

for (const file of asmdefFiles) {
    const json = JSON.parse(fs.readFileSync(file, 'utf8'));
    if (!json.name || !json.name.startsWith('Gopet.')) continue;

    const info = {
        name: json.name,
        rootNamespace: json.rootNamespace || json.name,
        references: json.references || [],
        dir: path.dirname(file),
        file,
    };

    assemblies.push(info);
    byNamespace.set(info.rootNamespace, info.name);
}

/**
 * Assembly cua package ma asmdef PHAI khai bao tay, doi chieu bang kieu no cung cap.
 *
 * Chi liet ke nhung package dat "autoReferenced": false - nhung cai khac (UnityEngine.UI,
 * Unity.InputSystem) Unity tu noi vao nen khai bao hay khong deu compile duoc, va bat
 * chung o day chi tao bao dong gia.
 *
 * Vi sao can: cac du an compile-only tham chieu thang DLL chu khong qua asmdef, nen
 * chung khong the thay "quen khai bao Unity.InputSystem.TestFramework". Unity thi tu
 * choi compile - va chi lo ra sau khi da bat nguoi dung dong Editor.
 */
const TYPE_ASSEMBLIES = [
    ['InputTestFixture', 'Unity.InputSystem.TestFramework'],
    ['UnityTest', 'UnityEngine.TestRunner'],
];

let problems = 0;

for (const asm of assemblies) {
    const sources = walk(asm.dir, '.cs');
    const needed = new Map(); // ten assembly -> file dau tien dung toi

    for (const source of sources) {
        const text = fs.readFileSync(source, 'utf8');
        // Bat ca "using Gopet.X;" lan "Gopet.X.Y" dung truc tiep trong code.
        for (const match of text.matchAll(/\busing\s+(Gopet\.[A-Za-z0-9_.]+)\s*;/g)) {
            for (const [ns, assemblyName] of byNamespace) {
                if (assemblyName === asm.name) continue;
                if (match[1] === ns || match[1].startsWith(ns + '.')) {
                    if (!needed.has(assemblyName)) needed.set(assemblyName, source);
                }
            }
        }

        for (const [typeName, assemblyName] of TYPE_ASSEMBLIES) {
            if (new RegExp('\\b' + typeName + '\\b').test(text) && !needed.has(assemblyName)) {
                needed.set(assemblyName, source);
            }
        }
    }

    for (const [assemblyName, source] of needed) {
        if (asm.references.includes(assemblyName)) continue;

        problems++;
        console.log(`THIEU: ${asm.name} dung ${assemblyName} nhung khong khai bao tham chieu`);
        console.log(`       thay o: ${path.relative(ASSETS, source)}`);
        console.log(`       sua: them "${assemblyName}" vao "references" cua ${path.basename(asm.file)}`);
    }
}

if (problems > 0) {
    console.log('');
    console.log(`${problems} asmdef thieu tham chieu — Unity se tu choi compile.`);
    process.exit(1);
}

console.log(`OK — ${assemblies.length} asmdef khai bao du tham chieu.`);
