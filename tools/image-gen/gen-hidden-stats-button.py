#!/usr/bin/env python3
"""Generate candidate frames for the "Kích ẩn" button, using btn-skill-round.png as style reference.

Output: tools/image-gen/output/hidden-stats-button-<n>.png (preview only, transparent, cropped).
No text is baked in — the label is drawn by code over the button face.
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

HERE = Path(__file__).resolve().parent
REF = HERE.parent.parent / "GopetUnityClient" / "Assets" / "Resources" / "Battle" / "btn-skill-round.png"
COUNT = 3
PROMPT = (
    "Using the same 16-bit pixel-art style, golden frame and dark blue patterned fabric as this "
    "round button, draw a single wide horizontal rectangular game button (about 3.5 times wider "
    "than tall) filling the canvas width. Thick golden pixel frame with slightly rounded corners, "
    "a small gold diamond gem ornament centered on the left and right ends, tiny blue sapphire "
    "studs at the four corners. The inner face is deep royal blue with a subtle darker woven "
    "pattern and a soft lighter highlight band on top, leaving the center clean for a text label. "
    "No text, no letters, no icons inside. Transparent background, no glow halo, no drop shadow."
)

load_dotenv(HERE / ".env")
with REF.open("rb") as ref:
    result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT, n=COUNT,
                                  size="1536x1024", quality="high", background="transparent")
for i, data in enumerate(result.data, 1):
    img = Image.open(io.BytesIO(base64.b64decode(data.b64_json))).convert("RGBA")
    img = img.crop(img.getbbox())
    out = HERE / "output" / f"hidden-stats-button-{i}.png"
    img.save(out)
    print("Saved", out, img.size)
