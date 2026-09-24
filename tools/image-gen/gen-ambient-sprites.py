#!/usr/bin/env python3
"""Draw the tiny pixel-art sprites used by battle-scene ambient effects.

Sprites are hand-authored as ASCII grids (1 char = 1 pixel) so they stay crisp and are
trivial to tweak. Unity scales them with Point filtering (BattleSkin.SpriteScale), so they
are drawn at native low resolution here.

Also writes a Unity .meta (single sprite, no mipmaps) for every PNG in Battle/ambient and
Battle/bg that does not have one yet, avoiding the Multiple-sprite meta trap described in
docs/battle-system.md section 10.3.

Usage: python gen-ambient-sprites.py
"""
from __future__ import annotations

import uuid
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent.parent
BATTLE = ROOT / "GopetUnityClient" / "Assets" / "Resources" / "Battle"
AMBIENT = BATTLE / "ambient"

# Palette: char -> RGBA. '.' is transparent.
P = {
    ".": (0, 0, 0, 0),
    "k": (34, 24, 40, 255),     # dark outline
    "o": (247, 148, 29, 255),   # butterfly orange
    "y": (255, 214, 64, 255),   # yellow
    "w": (255, 255, 255, 255),  # white
    "b": (184, 226, 255, 255),  # pale blue (snow shade, dragonfly wing)
    "c": (126, 200, 240, 200),  # translucent wing
    "t": (32, 160, 150, 255),   # dragonfly teal body
    "g": (20, 110, 100, 255),   # dragonfly dark teal
    "p": (255, 183, 206, 255),  # petal pink
    "q": (240, 128, 168, 255),  # petal deep pink
    "v": (72, 52, 92, 255),     # bat body purple
    "u": (110, 82, 138, 255),   # bat wing light
    "r": (230, 40, 40, 255),    # bat eye / ember red
    "e": (255, 120, 20, 255),   # ember orange
}

SPRITES = {
    "butterfly-0": [  # wings open
        "k...k.......",
        ".k.k........",
        "ooo.k.ooo...",
        "oyyokkoyyo..",
        "oyyykkyyyo..",
        ".oookkooo...",
        "..ookkoo....",
        "..oo.k.oo...",
    ],
    "butterfly-1": [  # wings raised (side view)
        "k...k.......",
        ".k.k........",
        "..ooko......",
        "..oyko......",
        "..oykk......",
        "..ookk......",
        "....kk......",
        ".....k......",
    ],
    "dragonfly-0": [
        "..cc.cc..........",
        ".cccbccc.........",
        "..cc.cc..........",
        "kttkkttttttttgg..",
        "..cc.cc..........",
        ".cccbccc.........",
        "..cc.cc..........",
    ],
    "dragonfly-1": [
        "...cccc..........",
        "..cccbcc.........",
        "...ccc...........",
        "kttkkttttttttgg..",
        "...ccc...........",
        "..cccbcc.........",
        "...cccc..........",
    ],
    "petal": [
        ".pp..",
        "pppq.",
        "ppqq.",
        ".qq..",
    ],
    "snowflake": [
        "..w..",
        ".bwb.",
        "wwwww",
        ".bwb.",
        "..w..",
    ],
    "ember": [
        ".e.",
        "eye",
        ".e.",
    ],
    "bat-fly-0": [  # wings up
        "u...........u",
        "uu.........uu",
        ".uuu.v.v.uuu.",
        "..uuvvvvvuu..",
        "....vrvrv....",
        ".....vvv.....",
    ],
    "bat-fly-1": [  # wings down
        ".....v.v.....",
        "....vvvvv....",
        "..uuvrvrvuu..",
        ".uuu.vvv.uuu.",
        "uu.........uu",
        "u...........u",
    ],
    "bat-hang": [  # hanging upside down, wings wrapped
        "..k.k..",
        "..vvv..",
        ".uvvvu.",
        ".uvvvu.",
        ".uvvvu.",
        "..vvv..",
        "..vrv..",
        "...v...",
    ],
}


def draw(rows: list[str]) -> Image.Image:
    w = max(len(r) for r in rows)
    img = Image.new("RGBA", (w, len(rows)), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            img.putpixel((x, y), P[ch])
    return img


META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: {filter}
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: {compression}
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def write_missing_metas(folder: Path, pixel: bool) -> None:
    """Point filter + no compression for pixel sprites; bilinear like bg-forest for backgrounds."""
    for png in folder.glob("*.png"):
        meta = png.with_suffix(".png.meta")
        if meta.exists():
            continue
        meta.write_text(META.format(guid=uuid.uuid4().hex, filter=0 if pixel else 1,
                                    compression=0 if pixel else 1), encoding="utf-8")
        print(f"  meta {meta.name}")


def main() -> None:
    AMBIENT.mkdir(parents=True, exist_ok=True)
    for name, rows in SPRITES.items():
        draw(rows).save(AMBIENT / f"{name}.png")
        print(f"  sprite {name}")
    write_missing_metas(AMBIENT, pixel=True)
    if (BATTLE / "bg").exists():
        write_missing_metas(BATTLE / "bg", pixel=False)


if __name__ == "__main__":
    main()
