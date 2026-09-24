#!/usr/bin/env python3
"""Generate the "Đá mài sửa chữa" (repair whetstone) item icon.

Output: SRCGOPETGOC/GServer/assets/items/1000091.png — 32x32 RGBA like the other item
icons (e.g. 240024.png), cropped tight then downscaled with NEAREST to keep pixel edges.
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent.parent
OUT = ROOT / "SRCGOPETGOC" / "GServer" / "assets" / "items" / "1000091.png"
SIZE = 32
PROMPT = (
    "16-bit pixel art game item icon: a rectangular blue-grey sharpening whetstone "
    "(repair stone) seen at a slight angle, with a small golden sparkle and a tiny spark "
    "on its edge, bold dark outline, vibrant colors, centered, transparent background, "
    "no text, no letters, simple readable silhouette at 32x32."
)

load_dotenv(Path(__file__).parent / ".env")
result = OpenAI().images.generate(model="gpt-image-2", prompt=PROMPT, size="1024x1024",
                                  quality="high", background="transparent")
img = Image.open(io.BytesIO(base64.b64decode(result.data[0].b64_json))).convert("RGBA")
img = img.crop(img.getbbox())
side = max(img.size)
square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
square.paste(img, ((side - img.width) // 2, (side - img.height) // 2))
# Lưu thêm bản lớn để xem trước; bản game là 32x32.
square.resize((256, 256), Image.LANCZOS).save(Path(__file__).parent / "output" / "repair-stone-preview.png")
square.resize((SIZE, SIZE), Image.LANCZOS).save(OUT)
print("Saved", OUT)
