const fs = require("fs");
const path = require("path");

const unityRoot = path.resolve(__dirname, "../..");
const repoRoot = path.resolve(unityRoot, "..");
const serverRoot = path.join(repoRoot, "SRCGOPETGOC", "GServer");
const unityScripts = path.join(unityRoot, "Assets", "Scripts");
const serverConstantsPath = path.join(serverRoot, "Server", "GopetCMD.cs");
const unityConstantsPath = path.join(unityScripts, "Net", "GopetCmd.cs");
const decisionsPath = path.join(__dirname, "decisions.json");
const reportPath = path.join(repoRoot, "plans", "260913-jar-unity-parity-audit-and-certification",
  "reports", "protocol-coverage.generated.json");

function filesUnder(root) {
  const result = [];
  for (const entry of fs.readdirSync(root, { withFileTypes: true })) {
    if (["bin", "obj", "packages", "Library", "Temp", "Logs"].includes(entry.name)) continue;
    const full = path.join(root, entry.name);
    if (entry.isDirectory()) result.push(...filesUnder(full));
    else if (entry.name.endsWith(".cs")) result.push(full);
  }
  return result;
}

function constants(file, className) {
  const text = fs.readFileSync(file, "utf8");
  const values = new Map();
  const pattern = /public const sbyte\s+(\w+)\s*=\s*(-?\d+)\s*;/g;
  for (const match of text.matchAll(pattern)) values.set(`${className}.${match[1]}`, Number(match[2]));
  return values;
}

const serverConstants = constants(serverConstantsPath, "GopetCMD");
const unityConstants = constants(unityConstantsPath, "GopetCmd");
const constantsByName = new Map([...serverConstants, ...unityConstants]);
const serverNamesByValue = new Map();
for (const [name, value] of serverConstants) {
  if (!serverNamesByValue.has(value)) serverNamesByValue.set(value, []);
  serverNamesByValue.get(value).push(name.replace("GopetCMD.", ""));
}

function valueOf(expression) {
  const clean = expression.replace(/\(sbyte\)/g, "").trim();
  if (/^-?\d+$/.test(clean)) return Number(clean);
  return constantsByName.get(clean);
}

function route(top, sub) {
  return sub === undefined ? `${top}` : `${top}/${sub}`;
}

const emitted = new Map();
function addEmitted(top, sub, file, index, detail) {
  if (top === undefined) return;
  const key = route(top, sub);
  if (!emitted.has(key)) emitted.set(key, []);
  const line = fs.readFileSync(file, "utf8").slice(0, index).split(/\r?\n/).length;
  emitted.get(key).push(`${path.relative(repoRoot, file).replace(/\\/g, "/")}:${line} ${detail}`);
}

for (const file of filesUnder(serverRoot)) {
  const text = fs.readFileSync(file, "utf8");
  const argument = "(GopetCMD\\.\\w+|\\(sbyte\\)\\s*-?\\d+|-?\\d+)";
  for (const match of text.matchAll(new RegExp(`new\\s+Message\\(\\s*${argument}\\s*\\)`, "g"))) {
    const top = valueOf(match[1]);
    addEmitted(top, undefined, file, match.index, match[0]);
    if (top === serverConstants.get("GopetCMD.PET_SERVICE")) {
      const tail = text.slice(match.index + match[0].length, match.index + match[0].length + 500);
      const subMatch = tail.match(/\.putsbyte\(\s*(GopetCMD\.\w+|-?\d+)\s*\)/);
      if (subMatch) addEmitted(top, valueOf(subMatch[1]), file, match.index, `PET_SERVICE -> ${subMatch[1]}`);
    }
  }
  for (const match of text.matchAll(new RegExp(`new\\s+ListWriterMessage\\(\\s*\\d+\\s*,\\s*${argument}\\s*\\)`, "g"))) {
    const top = valueOf(match[1]);
    addEmitted(top, undefined, file, match.index, match[0]);
    const tail = text.slice(match.index + match[0].length, match.index + match[0].length + 500);
    const subMatch = tail.match(/\.putsbyte\(\s*(GopetCMD\.\w+|-?\d+)\s*\)/);
    if (subMatch) addEmitted(top, valueOf(subMatch[1]), file, match.index,
      `ListWriterMessage -> ${subMatch[1]}`);
  }
  for (const match of text.matchAll(new RegExp(`(?:GameController\\.|GopetPlace\\.)?messagePetService\\(\\s*${argument}\\s*\\)`, "g"))) {
    addEmitted(serverConstants.get("GopetCMD.PET_SERVICE"), valueOf(match[1]), file, match.index, match[0]);
  }
  // One server helper forwards its final argument into messagePetService(cmd).
  // Expand its constant call sites so route 47/80 cannot disappear behind a parameter.
  for (const match of text.matchAll(/writeSelectItemEnchant\([^;]*,\s*(GopetCMD\.\w+)\s*\)/g)) {
    addEmitted(serverConstants.get("GopetCMD.PET_SERVICE"), valueOf(match[1]), file,
      match.index, `${match[0]} via messagePetService(cmd)`);
  }
  for (const match of text.matchAll(/(?:GameController\.)?clanMessage\(\s*(GopetCMD\.\w+|-?\d+)\s*\)/g)) {
    addEmitted(serverConstants.get("GopetCMD.PET_SERVICE"), serverConstants.get("GopetCMD.CLAN"), file,
      match.index, `${match[0]} nested=${match[1]}`);
  }
}

const handled = new Set();
for (const file of filesUnder(unityScripts)) {
  const text = fs.readFileSync(file, "utf8");
  const localConstants = new Map(constantsByName);
  for (const match of text.matchAll(/(?:public|private|internal|protected)\s+const\s+sbyte\s+(\w+)\s*=\s*(-?\d+)\s*;/g)) {
    localConstants.set(match[1], Number(match[2]));
  }
  const localValueOf = expression => {
    const clean = expression.replace(/\(sbyte\)/g, "").trim();
    if (/^-?\d+$/.test(clean)) return Number(clean);
    return localConstants.get(clean);
  };
  const argument = "(GopetCmd\\.\\w+|[A-Za-z_]\\w*|\\(sbyte\\)\\s*-?\\d+|-?\\d+)";
  for (const match of text.matchAll(new RegExp(`\\.Register\\(\\s*${argument}\\s*,`, "g"))) {
    const top = localValueOf(match[1]);
    if (top !== undefined) handled.add(route(top));
  }
  for (const match of text.matchAll(new RegExp(`\\.RegisterEnvelope\\(\\s*${argument}\\s*\\)`, "g"))) {
    const top = localValueOf(match[1]);
    if (top !== undefined) handled.add(route(top));
  }
  for (const match of text.matchAll(new RegExp(`\\.RegisterSub\\(\\s*${argument}\\s*,\\s*${argument}\\s*,`, "g"))) {
    const top = localValueOf(match[1]);
    const sub = localValueOf(match[2]);
    if (top !== undefined && sub !== undefined) handled.add(route(top, sub));
  }
}

const decisions = fs.existsSync(decisionsPath)
  ? JSON.parse(fs.readFileSync(decisionsPath, "utf8")) : { handled: [], intentionallyIgnored: [], serverUnused: [], notes: {} };
for (const item of decisions.handled || []) handled.add(item);
const intentionallyIgnored = new Set(decisions.intentionallyIgnored || []);
const serverUnused = new Set(decisions.serverUnused || []);

const rows = [...emitted.keys()].sort((a, b) => {
  const aa = a.split("/").map(Number), bb = b.split("/").map(Number);
  return aa[0] - bb[0] || (aa[1] || -999) - (bb[1] || -999);
}).map(key => {
  const [top, sub] = key.split("/").map(Number);
  const status = handled.has(key) ? "handled"
    : intentionallyIgnored.has(key) ? "intentionally-ignored"
    : serverUnused.has(key) ? "server-unused" : "missing";
  const names = value => (serverNamesByValue.get(value) || ["literal"]).join("|");
  return { route: key, name: sub === undefined ? names(top) : `${names(top)}/${names(sub)}`,
    status, evidence: emitted.get(key), note: decisions.notes?.[key] || "" };
});

if (process.argv.includes("--write")) {
  fs.mkdirSync(path.dirname(reportPath), { recursive: true });
  fs.writeFileSync(reportPath, `${JSON.stringify(rows, null, 2)}\n`);
  console.log(`Wrote ${path.relative(repoRoot, reportPath)}`);
} else if (process.argv.includes("--json")) console.log(JSON.stringify(rows, null, 2));
else {
  for (const row of rows) console.log(`${row.status.padEnd(21)} ${row.route.padEnd(8)} ${row.name}`);
  const counts = rows.reduce((all, row) => ((all[row.status] = (all[row.status] || 0) + 1), all), {});
  console.log(`Protocol coverage: ${JSON.stringify(counts)}`);
}

const missing = rows.filter(row => row.status === "missing");
if (missing.length) {
  console.error(`Missing protocol decisions: ${missing.map(row => row.route).join(", ")}`);
  process.exitCode = 1;
}
