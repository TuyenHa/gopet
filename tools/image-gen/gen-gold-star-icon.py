#!/usr/bin/env python3
"""Generate the gold five-pointed star icon shown after pet names (star rating).

Output: GopetUnityClient/Assets/Resources/Ui/Generated/star-gold.png — 48x48 RGBA,
transparent, cropped tight. Used for EVERY star slot by StarNameLabel (thay sao jar).
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent.parent
OUT = ROOT / "GopetUnityClient" / "Assets" / "Resources" / "Ui" / "Generated" / "star-gold.png"
SIZE = 48
PROMPT = (
    "Game UI icon: a single classic five-pointed star with sharp, evenly spaced points, "
    "glossy bright gold-yellow fill with a soft lighter highlight on the upper-left, thin "
    "darker orange-gold outline, front view, perfectly symmetrical, centered, transparent "
    "background, no text, no shadow, no sparkles, clean readable silhouette at small size."
)

load_dotenv(Path(__file__).parent / ".env")
result = OpenAI().images.generate(model="gpt-image-2", prompt=PROMPT, size="1024x1024",
                                  quality="high", background="transparent")
img = Image.open(io.BytesIO(base64.b64decode(result.data[0].b64_json))).convert("RGBA")
img = img.crop(img.getbbox())
side = max(img.size)
square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
square.paste(img, ((side - img.width) // 2, (side - img.height) // 2))
# Bản lớn để xem trước; bản game 48x48.
square.resize((256, 256), Image.LANCZOS).save(Path(__file__).parent / "output" / "star-gold-preview.png")
square.resize((SIZE, SIZE), Image.LANCZOS).save(OUT)
print("Saved", OUT)
