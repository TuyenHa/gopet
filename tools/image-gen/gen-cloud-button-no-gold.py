#!/usr/bin/env python3
"""Variant of cloud-corner button (candidate 2) with the golden border removed.

Input:  tools/image-gen/output/cloud-button-2-raw.png (from gen-cloud-corner-button.py)
Output: tools/image-gen/output/cloud-button-nogold-<n>.png (magenta keyed to transparent, cropped).
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

HERE = Path(__file__).resolve().parent
PROMPT = (
    "Keep this button exactly the same (glossy royal blue face, the four fluffy white clouds in "
    "the corners, proportions) but remove the golden border completely. The blue face now forms "
    "the button edge itself, with a slightly darker blue rounded rim. The clouds stay anchored "
    "on the four corners, billowing inward over the blue face. No gold anywhere. No text. "
    "Keep the solid flat pure magenta #FF00FF background, no glow, no shadow."
)
COUNT = 2


def key_magenta(img: Image.Image) -> Image.Image:
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            r, g, b, _ = px[x, y]
            m = min(r, b) - g
            if m > 120:
                px[x, y] = (0, 0, 0, 0)
            elif m > 50:
                px[x, y] = (r, g, b, int(255 * (120 - m) / 70))
    return img


load_dotenv(HERE / ".env")
with (HERE / "output" / "cloud-button-2-raw.png").open("rb") as ref:
    result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT, n=COUNT,
                                  size="1536x1024", quality="high")
for i, data in enumerate(result.data, 1):
    img = Image.open(io.BytesIO(base64.b64decode(data.b64_json))).convert("RGBA")
    img.save(HERE / "output" / f"cloud-button-nogold-{i}-raw.png")
    img = key_magenta(img)
    img = img.crop(img.getbbox())
    out = HERE / "output" / f"cloud-button-nogold-{i}.png"
    img.save(out)
    print("Saved", out, img.size)
