#!/usr/bin/env python3
"""Generate the hammering animation strip for the "Thợ Rèn" NPC from the chosen sprite.

Input : output/blacksmith-npc-3.png (mẫu đã chọn, 56x62).
Step 1: gpt-image vẽ 3 pose trong MỘT ảnh (cùng ảnh thì nhân vật nhất quán hơn): giơ búa,
        búa giữa đường, búa nện xuống đe tóe lửa.
Step 2: tách 3 pose theo cột trong suốt, căn chân chung, thu về cao 62, xếp thành strip
        theo nhịp [giơ, giơ, giơ, vung, nện, nện] — client đổi frame đều 0.16s nên lặp frame
        để giữ lâu (dừng ở trên, nện nhanh, đọng lại lúc chạm đe).
Output: output/blacksmith-hammer-strip.png (+ -preview.gif). `--apply` chép strip vào
        SRCGOPETGOC/GServer/assets/npcs/Tho_Ren.png và Resources/Jar/Art/Raw/npcs/Tho_Ren.png.
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
ROOT = HERE.parent.parent
OUT = HERE / "output"
REF = OUT / "blacksmith-npc-3-preview.png"
RAW = OUT / "blacksmith-hammer-raw.png"
STRIP = OUT / "blacksmith-hammer-strip.png"
TARGETS = [ROOT / "SRCGOPETGOC" / "GServer" / "assets" / "npcs" / "Tho_Ren.png",
           ROOT / "GopetUnityClient" / "Assets" / "Resources" / "Jar" / "Art" / "Raw" / "npcs" / "Tho_Ren.png"]
HEIGHT = 62
SEQUENCE = [0, 0, 0, 1, 2, 2]  # chỉ số pose cho từng frame của strip
PROMPT = (
    "Pixel-art sprite sheet of EXACTLY this same blacksmith character (same face, red "
    "bandana, beard, grey shirt, brown leather apron, boots, colors and chibi proportions), "
    "3 frames side by side in one row with wide empty gaps between them, all the same size "
    "and standing on the same baseline, a small dark iron anvil in front of him in every "
    "frame. Frame 1: both hands raising the forging hammer high above his head. Frame 2: "
    "hammer swinging down halfway. Frame 3: hammer striking the anvil with bright orange "
    "sparks bursting. Body and legs stay in the same place in all frames, only arms and "
    "hammer move. Transparent background, no text, no ground shadow."
)


def generate() -> None:
    load_dotenv(HERE / ".env")
    with REF.open("rb") as ref:
        result = OpenAI().images.edit(model="gpt-image-2", image=ref, prompt=PROMPT,
                                      size="1536x1024", quality="high", background="transparent")
    Image.open(io.BytesIO(base64.b64decode(result.data[0].b64_json))).convert("RGBA").save(RAW)
    print("Saved", RAW)


def split_poses(sheet: Image.Image) -> list:
    """Cắt 3 ô đều nhau (model vẽ 3 pose theo lưới ngang), bỏ điểm mờ alpha thấp."""
    alpha = sheet.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    sheet = sheet.copy()
    sheet.putalpha(alpha)
    width = sheet.width // 3
    return [sheet.crop((i * width, 0, (i + 1) * width, sheet.height)) for i in range(3)]


PALETTE_COLORS = 40


def pixelize(img: Image.Image) -> Image.Image:
    """Ép về pixel art thật như NPC jar (Tho_San: 0 điểm bán trong suốt, ~35 màu): alpha
    cứng trong/đục — hết viền nhoè lấm tấm khi Unity vẽ filter Point — và bảng màu gọn."""
    alpha = img.getchannel("A").point(lambda v: 255 if v >= 128 else 0)
    rgb = img.convert("RGB").quantize(colors=PALETTE_COLORS, method=Image.Quantize.MEDIANCUT,
                                      dither=Image.Dither.NONE).convert("RGB")
    out = rgb.convert("RGBA")
    out.putalpha(alpha)
    return out


def anchor(pose: Image.Image) -> tuple:
    """Điểm neo = tâm ngang + đáy của dải đáy (đe + ủng) — không đổi giữa các pose,
    nên căn theo nó thì thân đứng yên, chỉ tay và búa chuyển động."""
    left, top, right, bottom = pose.getbbox()
    band = pose.crop((0, bottom - max(1, (bottom - top) // 10), pose.width, bottom)).getbbox()
    return (band[0] + band[2]) / 2, bottom


def build_strip() -> None:
    poses = split_poses(Image.open(RAW).convert("RGBA"))
    # Thu theo THÂN: pose nện (không giơ búa) cao đúng HEIGHT; pose giơ búa cao hơn.
    scale = HEIGHT / (poses[2].getbbox()[3] - poses[2].getbbox()[1])
    small = [p.resize((round(p.width * scale), round(p.height * scale)), Image.LANCZOS) for p in poses]
    # Cùng MỘT bảng màu cho cả 3 pose — quantize riêng từng pose thì màu nhảy giữa các frame.
    joined = Image.new("RGBA", (sum(s.width for s in small), max(s.height for s in small)))
    x = 0
    for s in small:
        joined.paste(s, (x, 0))
        x += s.width
    joined = pixelize(joined)
    x, parts = 0, []
    for s in small:
        parts.append(joined.crop((x, 0, x + s.width, s.height)))
        x += s.width
    small = parts
    anchors = [anchor(s) for s in small]
    boxes = [s.getbbox() for s in small]
    # Khung chung đủ chứa mọi pose sau khi dời về cùng điểm neo (neo tại (ax, ay) của khung).
    left = max(a[0] - b[0] for a, b in zip(anchors, boxes))
    right = max(b[2] - a[0] for a, b in zip(anchors, boxes))
    up = max(a[1] - b[1] for a, b in zip(anchors, boxes))
    cell_w, cell_h = int(left + right) + 2, int(up) + 1
    frames = []
    for s, (ax, ay) in zip(small, anchors):
        frame = Image.new("RGBA", (cell_w, cell_h), (0, 0, 0, 0))
        frame.paste(s, (int(round(left - ax)) + 1, int(round(cell_h - ay))), s)
        frames.append(frame)
    strip = Image.new("RGBA", (cell_w * len(SEQUENCE), cell_h), (0, 0, 0, 0))
    for i, pose in enumerate(SEQUENCE):
        strip.paste(frames[pose], (i * cell_w, 0))
    strip.save(STRIP)
    big = [frames[p].resize((cell_w * 4, cell_h * 4), Image.NEAREST) for p in SEQUENCE]
    bg = [Image.alpha_composite(Image.new("RGBA", b.size, (120, 170, 90, 255)), b).convert("RGB") for b in big]
    bg[0].save(OUT / "blacksmith-hammer-preview.gif", save_all=True, append_images=bg[1:],
               duration=160, loop=0)
    sheet = Image.new("RGBA", (cell_w * 3 * 4 + 40, cell_h * 4 + 20), (120, 170, 90, 255))
    for i, f in enumerate(frames):
        sheet.alpha_composite(f.resize((cell_w * 4, cell_h * 4), Image.NEAREST), (10 + i * (cell_w * 4 + 10), 10))
    sheet.save(OUT / "blacksmith-hammer-poses.png")
    print("Saved", STRIP, strip.size, "cell", (cell_w, cell_h), "frames", len(SEQUENCE))


def apply() -> None:
    for target in TARGETS:
        shutil.copyfile(STRIP, target)
        print("Applied ->", target)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--skip-generate", action="store_true", help="chỉ dựng lại strip từ raw")
    parser.add_argument("--apply", action="store_true", help="chép strip vào asset game")
    args = parser.parse_args()
    if args.apply:
        apply()
    else:
        if not args.skip_generate:
            generate()
        build_strip()
