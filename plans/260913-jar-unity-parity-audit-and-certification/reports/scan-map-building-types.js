const fs = require("fs");
const path = require("path");

function parseBuildingTypes(data) {
  let offset = 0;
  const byte = () => data[offset++];
  const signedByte = () => data.readInt8(offset++);
  const short = () => { const value = data.readInt16BE(offset); offset += 2; return value; };
  const int = () => { const value = data.readInt32BE(offset); offset += 4; return value; };

  const imageCount = byte();
  const resourceCount = imageCount + byte();
  const resourceTypes = [];
  for (let index = 0; index < resourceCount; index++) {
    short();
    resourceTypes.push(byte());
  }

  const width = byte();
  const height = byte();
  const layerCount = byte();
  offset += layerCount * width * height;
  offset += width * height;

  const objectCount = int();
  for (let index = 0; index < objectCount; index++) {
    const resourceIndex = byte();
    short();
    short();
    signedByte();
    if (resourceTypes[resourceIndex] === 1) offset += 5;
  }

  const entityCount = int();
  const buildings = [];
  for (let index = 0; index < entityCount; index++) {
    const kind = byte();
    const buildingType = signedByte();
    short();
    short();
    offset += 5;
    if (kind === 0) {
      buildings.push(buildingType);
    } else {
      byte();
      byte();
      const utfLength = data.readUInt16BE(offset);
      offset += 2 + utfLength + 1;
    }
  }
  return buildings;
}

const mapsDir = path.resolve(__dirname, "../../../GopetUnityClient/Assets/Resources/Jar/Maps");
const miniGameBuildings = [];
for (let mapId = 11; mapId <= 34; mapId++) {
  const bytes = fs.readFileSync(path.join(mapsDir, `${mapId}.bytes`));
  const buildings = parseBuildingTypes(bytes);
  console.log(`${mapId}: ${buildings.join(",")}`);
  for (const buildingType of buildings) {
    if (buildingType >= 13 && buildingType <= 16) miniGameBuildings.push({ mapId, buildingType });
  }
}
console.log(`mini-game buildings: ${JSON.stringify(miniGameBuildings)}`);
