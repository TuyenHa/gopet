#!/usr/bin/env python3
"""Generate pixel-art skill icons for the battle screen.

Reads skill data from a hardcoded list (extracted from DB `skill` table),
generates 64x64 pixel-art icons via OpenAI image API, saves to
GopetUnityClient/Assets/Resources/Battle/skills/{skillID}.png.

Skips icons that already exist on disk.

Usage:
    python generate-battle-skills.py           # generate all missing
    python generate-battle-skills.py --dry-run  # show what would be generated
    python generate-battle-skills.py --ids 101 102  # generate specific IDs only
"""
from __future__ import annotations

import argparse
import base64
import os
import sys
import time
from pathlib import Path

os.environ.setdefault("PYTHONIOENCODING", "utf-8")
if sys.stdout.encoding != "utf-8":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")

try:
    from openai import OpenAI
except ImportError:
    sys.exit("pip install -r requirements.txt")

try:
    from dotenv import load_dotenv
except ImportError:
    sys.exit("pip install -r requirements.txt")

try:
    from PIL import Image
    import io
except ImportError:
    sys.exit("pip install Pillow")

MODEL = "gpt-image-2"
OUTPUT_DIR = Path(__file__).resolve().parent.parent.parent / "GopetUnityClient" / "Assets" / "Resources" / "Battle" / "skills"
ICON_SIZE = 64

STYLE_PREFIX = (
    "16-bit pixel art game skill icon, 64x64, bold dark outline, "
    "vibrant saturated colors, transparent background, centered, "
    "no text, no letters, clean simple design, retro JRPG style. "
)

SKILLS = [
    (101, "song kích", "đánh 2 đòn liên tiếp", "two fists punching in rapid combo"),
    (102, "giảm sát thương", "giảm sát thương địch 3 lượt", "broken cracked sword with down arrow"),
    (103, "cuồng nộ", "tăng 10% sức tấn công 3 lượt", "red glowing angry fist with flames"),
    (104, "băng", "sát thương và giảm sức tấn công", "blue ice crystal shard"),
    (105, "sấm sét", "giáng sấm sét, tê liệt 1 lượt", "yellow lightning bolt striking down"),
    (106, "lửa", "tung ngọn lửa đốt kẻ địch", "orange fire fireball"),
    (107, "hạ độc", "mất 50 máu mỗi lượt", "green poison bottle with skull"),
    (108, "hạ thủ", "1 đòn công phá lớn, tỷ lệ hụt", "large heavy axe swing with impact lines"),
    (109, "vô ảnh", "tăng tỷ lệ né 50%", "ghostly transparent silhouette with speed lines"),
    (110, "phản đòn", "phản 25% đòn đánh 4 lượt", "shield with reflected arrow bouncing back"),
    (111, "hút máu", "hút 30% máu theo sát thương", "red blood drop with fangs vampire"),
    (112, "đốt mana", "đốt 40% mana theo sát thương", "blue mana orb burning with fire"),
    (113, "thiên cân thủ", "1 đòn mạnh, đôi khi hụt", "massive stone hammer crushing down"),
    (114, "liên hoàn cước", "giảm sức tấn công 3 lượt", "rapid kick martial arts leg sweep"),
    (115, "lá chắn", "tăng 50 phòng thủ, hồi 30 máu 4 lượt", "golden glowing shield with green cross"),
    (116, "Bão Sét", "sát thương và tê liệt 1 lượt", "dark storm cloud with multiple lightning bolts"),
    (117, "Nộ Khí", "tăng % sức tấn công 3 lượt", "red aura energy fist power up"),
    (118, "Nguyệt Ma", "sát thương và giảm tấn công", "dark purple crescent moon with evil eye"),
    (119, "Gai Độc", "mất máu mỗi lượt", "green thorny cactus spikes with poison drops"),
    (120, "Búa Thor", "1 đòn cực mạnh", "mythical golden war hammer with lightning"),
    (121, "Thần Kiếm", "đâm xuyên, đôi khi hụt", "glowing celestial sword with holy light"),
    (122, "Phi Tiêu", "cắt cực mạnh, tỉ lệ hụt", "four throwing stars shuriken flying"),
    (123, "Phản Nghịch", "phản sát thương 4 lượt", "dark mirror shield reflecting energy back"),
    (124, "Thiên Thạch", "thiên thạch thiêu cháy", "fiery meteor falling from sky with fire trail"),
    (125, "Roi Huyền Ảo", "đốt % mana theo sát thương", "magical purple whip with sparks"),
    (126, "địa chấn", "sát thương cực cao, tỉ lệ hụt", "earth cracking ground shockwave"),
    (127, "Hắc ám ảo ảnh", "tăng tỉ lệ né 3 lượt", "dark shadow clone afterimage"),
    (128, "Bạo Vũ Lê Hoa", "sát thương cao, tỉ lệ hụt", "explosive flower petal storm blade"),
    (129, "Huyết Nhận", "sát thương lớn đốt mana hồi hp", "blood red blade dripping with dark energy"),
    (130, "Lá Chắn Hắc Ám", "tăng giáp hồi máu 3 lượt", "dark purple shield with healing green glow"),
]


def parse_args():
    p = argparse.ArgumentParser()
    p.add_argument("--dry-run", action="store_true", help="Show what would be generated")
    p.add_argument("--ids", type=int, nargs="+", help="Only generate these skill IDs")
    return p.parse_args()


def generate_icon(client: OpenAI, prompt: str) -> bytes | None:
    try:
        result = client.images.generate(
            model=MODEL,
            prompt=prompt,
            size="1024x1024",
            quality="medium",
            n=1,
            background="transparent",
        )
        if result.data and result.data[0].b64_json:
            return base64.b64decode(result.data[0].b64_json)
    except Exception as e:
        print(f"  ! API error: {e}", file=sys.stderr)
    return None


def resize_nearest(raw_bytes: bytes, size: int) -> bytes:
    img = Image.open(io.BytesIO(raw_bytes))
    img = img.resize((size, size), Image.NEAREST)
    buf = io.BytesIO()
    img.save(buf, format="PNG")
    return buf.getvalue()


def main():
    args = parse_args()
    load_dotenv()

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    skills_to_gen = SKILLS
    if args.ids:
        skills_to_gen = [s for s in SKILLS if s[0] in args.ids]

    missing = []
    for skill_id, name, desc, visual in skills_to_gen:
        path = OUTPUT_DIR / f"{skill_id}.png"
        if path.exists():
            print(f"  SKIP {skill_id} ({name}) — already exists")
            continue
        missing.append((skill_id, name, desc, visual, path))

    if not missing:
        print("All skill icons already exist. Nothing to generate.")
        return

    print(f"\n{len(missing)} skill icon(s) to generate:")
    for sid, name, _, _, _ in missing:
        print(f"  {sid}: {name}")

    if args.dry_run:
        print("\n--dry-run: stopping here.")
        return

    client = OpenAI()
    generated = 0

    for i, (skill_id, name, desc, visual, path) in enumerate(missing):
        prompt = f"{STYLE_PREFIX}{visual}"
        print(f"\n[{i+1}/{len(missing)}] Generating {skill_id} ({name})...")
        raw = generate_icon(client, prompt)
        if raw is None:
            print(f"  FAILED {skill_id}")
            continue
        resized = resize_nearest(raw, ICON_SIZE)
        path.write_bytes(resized)
        print(f"  Saved: {path} ({len(resized)} bytes)")
        generated += 1
        if i < len(missing) - 1:
            time.sleep(0.5)

    print(f"\nDone. Generated {generated}/{len(missing)} icons.")
    if generated < len(missing):
        print("Re-run to retry failed ones (existing files are skipped).")


if __name__ == "__main__":
    raise SystemExit(main())
