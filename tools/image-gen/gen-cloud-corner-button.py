#!/usr/bin/env python3
"""Generate candidate "cloud corner" frames for the common button (GameButtonSkin),
using the current Resources/Ui/Generated/button_frame_blue.png as style reference.

Output: tools/image-gen/output/cloud-button-<n>.png (preview, magenta keyed to transparent, cropped).
No text is baked in — labels are drawn by code over the button face.
"""
import base64
import io
from pathlib import Path

from dotenv import load_dotenv
from openai import OpenAI
from PIL import Image

HERE = Path(__file__).resolve().parent
REF = HERE.parent.parent / "GopetUnityClient" / "Assets" / "Resources" / "Ui" / "Generated" / "button_frame_blue.png"
COUNT = 3
PROMPT = (
    "Redraw this game button as one wide horizontal rectangular button (about 3.5 times wider "
    "than tall) filling the canvas width, same glossy royal blue face and golden border. "
    "Replace the four square blue gems in the corners with big fluffy white cartoon clouds: "
    "each cloud is anchored on a corner of the golden border and billows inward over the blue "
    "face, about 40 percent of the button height tall and 3 to 4 puffs wide, soft white with "
    "light blue shading and a thin darker blue outline so it reads clearly at small size. Keep the middle of the face and the middle of "
    "every edge clean and plain (no ornaments) so text can go in the center. "
    "No text, no letters, no icons. Solid flat pure magenta #FF00FF background around the button, "
    "no glow, no shadow, no halo."
)


def key_magenta(img: Image.Image) -> Image.Image:
    """Turn the magenta backdrop (and its anti-aliased fringe) transparent."""
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            r, g, b, _ = px[x, y]
            # Magenta: R and B high, G low. Fringe pixels get partial alpha.
            m = min(r, b) - g
            if m > 120:
                px[x, y] = (0, 0, 0, 0)
            elif m > 50:
                px[x, y] = (r, g, b, int(255 * (120 - m) / 70))
    return img


load_dotenv(HERE / ".env")
with REF.open("rb") as ref:
    result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT, n=COUNT,
                                  size="1536x1024", quality="high")
for i, data in enumerate(result.data, 1):
    img = Image.open(io.BytesIO(base64.b64decode(data.b64_json))).convert("RGBA")
    img.save(HERE / "output" / f"cloud-button-{i}-raw.png")
    img = key_magenta(img)
    img = img.crop(img.getbbox())
    out = HERE / "output" / f"cloud-button-{i}.png"
    img.save(out)
    print("Saved", out, img.size)
