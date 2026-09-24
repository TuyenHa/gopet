#!/usr/bin/env python3
"""Generate battle-scene backgrounds from the existing forest arena as a layout reference.

Each scene reuses bg-forest.png through the images.edit endpoint so the empty
central clearing and the two pet standing spots stay in the same place. The result is
resized to 960x640 (same as bg-forest) and written to
GopetUnityClient/Assets/Resources/Battle/bg/<name>.png.

Animated creatures/particles (butterflies, snow, petals, bats, embers) are drawn by code
at runtime, so the prompts forbid them here.

Usage:
    python gen-battle-backgrounds.py                 # all missing
    python gen-battle-backgrounds.py --only bg-snow  # regenerate one
"""
from __future__ import annotations

import argparse
import base64
import io
import sys
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

MODEL = "gpt-image-2"
ROOT = Path(__file__).resolve().parent.parent.parent
BATTLE = ROOT / "GopetUnityClient" / "Assets" / "Resources" / "Battle"
REFERENCE = BATTLE / "bg-forest.png"
OUT_DIR = BATTLE / "bg"
TARGET = (960, 640)

COMMON = (
    "Redraw this 16-bit pixel-art side-view battle arena as a different environment. "
    "KEEP EXACTLY the same composition: a wide empty flat ground clearing in the center and "
    "lower half where two creatures will stand on the left and right, a path leading into the "
    "distance at the top center, scenery framing the top and both side edges, darker foliage "
    "or rocks in the bottom-left and bottom-right corners. Same camera angle, same pixel-art "
    "style, same level of detail and soft lighting. No characters, no animals, no insects, no "
    "falling particles, no text, no UI. Environment: "
)

SCENES = {
    "bg-canopy": "a dense deep forest under a thick closed tree canopy, huge mossy trunks, "
                 "hanging vines, dappled green light beams through leaves, mossy grass ground, "
                 "a few wildflowers along the edges.",
    "bg-sakura": "a japanese cherry blossom grove in spring, pink sakura trees framing the top "
                 "and sides, soft pink fallen petals scattered on a light stone-and-grass ground, "
                 "a small wooden torii far in the distance, warm pastel light.",
    "bg-snow": "a snowy winter pine forest, snow-covered pines and bushes, deep white snow "
               "ground with soft blue shadows and light footprints, frosted rocks, pale cold sky "
               "light.",
    "bg-cave": "a large dark rocky cave, jagged stalactites hanging from the ceiling along the "
               "top edge, stone walls on the sides, glowing blue crystals in cracks, a flat "
               "stone floor with small pebbles, dim torch-like warm light in the center.",
    "bg-fire": "a volcanic hellscape, dark basalt rock ground with glowing orange lava cracks, "
               "charred dead trees on the sides, lava pools in the corners, smoky red-orange "
               "sky, burning embers glow on the rocks.",
}


def generate(client: OpenAI, name: str, desc: str) -> None:
    print(f"Generating {name} ...", flush=True)
    with REFERENCE.open("rb") as ref:
        result = client.images.edit(
            model=MODEL, image=ref, prompt=COMMON + desc, size="1536x1024", quality="high",
        )
    data = result.data[0].b64_json
    if not data:
        raise RuntimeError(f"no image data for {name}")
    img = Image.open(io.BytesIO(base64.b64decode(data))).convert("RGBA")
    img = img.resize(TARGET, Image.LANCZOS)
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    out = OUT_DIR / f"{name}.png"
    img.save(out)
    print(f"  Saved {out}", flush=True)


def main() -> int:
    p = argparse.ArgumentParser()
    p.add_argument("--only", nargs="*", help="scene names to (re)generate")
    args = p.parse_args()
    load_dotenv(Path(__file__).parent / ".env")
    client = OpenAI()
    names = args.only or [n for n in SCENES if not (OUT_DIR / f"{n}.png").exists()]
    failed = 0
    for name in names:
        try:
            generate(client, name, SCENES[name])
        except Exception as exc:  # noqa: BLE001 - keep going with other scenes
            failed += 1
            print(f"  ! {name} failed: {exc}", file=sys.stderr)
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
