#!/usr/bin/env python3
"""Generate the "Chợ trời" (open-air market) HUD icon from shop.png as style reference.

Output: GopetUnityClient/Assets/Resources/Ui/Hud/market.png — 128x128 RGBA transparent,
same outline weight/palette as the other HUD icons (shop, service, event, mail).
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

HUD = Path(__file__).resolve().parent.parent.parent / "GopetUnityClient" / "Assets" / "Resources" / "Ui" / "Hud"
SIZE = 128
PROMPT = (
    "Redraw this HUD icon in exactly the same style, outline weight and palette, but "
    "replace the shop subject with a small open-air market stall: striped red-yellow "
    "awning, wooden counter with goods and a coin pouch. No text, no letters, transparent "
    "background."
)

load_dotenv(Path(__file__).parent / ".env")
with (HUD / "shop.png").open("rb") as ref:
    result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT,
                                  size="1024x1024", quality="high", background="transparent")
img = Image.open(io.BytesIO(base64.b64decode(result.data[0].b64_json))).convert("RGBA")
img = img.crop(img.getbbox())
side = max(img.size)
square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
square.paste(img, ((side - img.width) // 2, (side - img.height) // 2))

# Bản lớn để xem trước; bản game HUD là 128x128 như các icon khác.
square.resize((512, 512), Image.LANCZOS).save(Path(__file__).parent / "output" / "market-hud-icon-preview.png")
out = HUD / "market.png"
square.resize((SIZE, SIZE), Image.LANCZOS).save(out)
print("Saved", out)
