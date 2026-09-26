#!/usr/bin/env python3
"""Generate the "Thợ Rèn" (blacksmith) NPC sprite, using Tho_San.png as style reference.

Preview mode (default): writes candidates to output/blacksmith-npc-<n>.png (game size) and
output/blacksmith-npc-<n>-preview.png (4x, NEAREST) for review — does NOT touch game assets.
Apply mode: `python gen-blacksmith-npc.py --apply <n>` copies candidate n to
SRCGOPETGOC/GServer/assets/npcs/Tho_Ren.png.
"""
import argparse
import base64
import io
import shutil
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

HERE = Path(__file__).resolve().parent
NPCS = HERE.parent.parent / "SRCGOPETGOC" / "GServer" / "assets" / "npcs"
OUT_DIR = HERE / "output"
# Cao 62 như Thợ Săn (Tho_San.png 52x62) — nhỏ hơn bản cũ 71x79.
TARGET_HEIGHT = 62
PROMPT = (
    "Redraw this character sprite in exactly the same chibi pixel-art style, outline weight, "
    "proportions (big head, small body), front 3/4 view and palette softness, but as a lively "
    "village BLACKSMITH: stocky cheerful man with a short brown beard and a red bandana, "
    "rolled-up sleeves showing strong arms, brown leather apron over a grey shirt, holding a "
    "big iron forging hammer resting on his shoulder, small glowing orange sparks near the "
    "hammer head, sturdy boots, confident smile. Full body, standing, centered, transparent "
    "background, no text, no ground shadow."
)


def generate(count: int) -> None:
    load_dotenv(HERE / ".env")
    OUT_DIR.mkdir(exist_ok=True)
    with (NPCS / "Tho_San.png").open("rb") as ref:
        result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT, n=count,
                                      size="1024x1024", quality="high", background="transparent")
    for i, data in enumerate(result.data, 1):
        img = Image.open(io.BytesIO(base64.b64decode(data.b64_json))).convert("RGBA")
        img = img.crop(img.getbbox())
        width = round(img.width * TARGET_HEIGHT / img.height)
        small = img.resize((width, TARGET_HEIGHT), Image.LANCZOS)
        small.save(OUT_DIR / f"blacksmith-npc-{i}.png")
        small.resize((width * 4, TARGET_HEIGHT * 4), Image.NEAREST).save(
            OUT_DIR / f"blacksmith-npc-{i}-preview.png")
        print("Saved candidate", i, small.size)


def apply(index: int) -> None:
    src = OUT_DIR / f"blacksmith-npc-{index}.png"
    shutil.copyfile(src, NPCS / "Tho_Ren.png")
    print("Applied", src, "->", NPCS / "Tho_Ren.png")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-n", "--count", type=int, default=3)
    parser.add_argument("--apply", type=int, metavar="N", help="copy candidate N into game assets")
    args = parser.parse_args()
    if args.apply:
        apply(args.apply)
    else:
        generate(args.count)
