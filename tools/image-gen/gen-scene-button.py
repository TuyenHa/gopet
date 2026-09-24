#!/usr/bin/env python3
"""Generate the round "KHUNG CẢNH" battle button from btn-skill-round.png as style reference.

Output: GopetUnityClient/Assets/Resources/Battle/btn-scene-round.png (transparent, cropped).
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

BATTLE = Path(__file__).resolve().parent.parent.parent / "GopetUnityClient" / "Assets" / "Resources" / "Battle"
PROMPT = (
    "Redraw this round 16-bit pixel-art game button with the exact same golden ring frame, "
    "diamond ornaments and dark blue patterned disc. Replace the crossed swords with a small "
    "framed landscape picture icon: green hills, a pine tree, snowy mountain peak and a sun. "
    "Replace the label text with 'KHUNG CẢNH' in the same white pixel font. Transparent "
    "background outside the ring, no glow halo."
)

load_dotenv(Path(__file__).parent / ".env")
with (BATTLE / "btn-skill-round.png").open("rb") as ref:
    result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT,
                                  size="1024x1024", quality="high", background="transparent")
img = Image.open(io.BytesIO(base64.b64decode(result.data[0].b64_json))).convert("RGBA")
img = img.crop(img.getbbox()).resize((512, 512), Image.LANCZOS)
out = BATTLE / "btn-scene-round.png"
img.save(out)
print("Saved", out)
