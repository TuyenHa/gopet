#!/usr/bin/env python3
"""Generate game images via OpenAI gpt-image-1.5.

Usage:
    python generate_image.py "a pixel-art fire dragon boss, transparent bg"
    python generate_image.py "wooden sword icon" -o assets/sword.png -s 1024x1024 -q high
    python generate_image.py "forest tileset" -n 3 --transparent

API key is read from the OPENAI_API_KEY variable in a .env file (or the
environment). Copy .env.example to .env and fill in your key.
"""
from __future__ import annotations

import argparse
import base64
import sys
from datetime import datetime
from pathlib import Path

try:
    from openai import OpenAI
except ImportError:
    sys.exit("Missing dependency 'openai'. Run: pip install -r requirements.txt")

try:
    from dotenv import load_dotenv
except ImportError:
    sys.exit("Missing dependency 'python-dotenv'. Run: pip install -r requirements.txt")

MODEL = "gpt-image-1.5"
VALID_SIZES = ("1024x1024", "1024x1536", "1536x1024", "auto")
VALID_QUALITY = ("low", "medium", "high", "auto")
DEFAULT_OUTPUT_DIR = Path(__file__).parent / "output"


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(
        description="Generate game images with OpenAI gpt-image-1.5",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    p.add_argument("prompt", help="Text description of the image to generate")
    p.add_argument("-o", "--output", help="Output file path (single image only)")
    p.add_argument(
        "-s", "--size", default="1024x1024", choices=VALID_SIZES, help="Image size"
    )
    p.add_argument(
        "-q", "--quality", default="high", choices=VALID_QUALITY, help="Render quality"
    )
    p.add_argument(
        "-n", "--count", type=int, default=1, help="Number of images to generate"
    )
    p.add_argument(
        "--transparent",
        action="store_true",
        help="Transparent background (forces PNG output)",
    )
    return p.parse_args()


def build_output_paths(args: argparse.Namespace) -> list[Path]:
    """Resolve where each generated image will be saved."""
    ext = "png" if args.transparent else "png"
    if args.output:
        if args.count > 1:
            sys.exit("Cannot use --output with --count > 1 (names would collide).")
        path = Path(args.output)
        path.parent.mkdir(parents=True, exist_ok=True)
        return [path]

    DEFAULT_OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d-%H%M%S")
    if args.count == 1:
        return [DEFAULT_OUTPUT_DIR / f"{stamp}.{ext}"]
    return [DEFAULT_OUTPUT_DIR / f"{stamp}-{i + 1}.{ext}" for i in range(args.count)]


def generate(client: OpenAI, args: argparse.Namespace):
    """Call the OpenAI images API and return the response."""
    kwargs = {
        "model": MODEL,
        "prompt": args.prompt,
        "size": args.size,
        "quality": args.quality,
        "n": args.count,
    }
    if args.transparent:
        kwargs["background"] = "transparent"
    return client.images.generate(**kwargs)


def main() -> int:
    args = parse_args()
    if args.count < 1:
        sys.exit("--count must be >= 1")

    load_dotenv()  # loads OPENAI_API_KEY from .env if present
    try:
        client = OpenAI()  # reads OPENAI_API_KEY from env
    except Exception as exc:  # noqa: BLE001
        sys.exit(f"Failed to init OpenAI client: {exc}\nDid you set OPENAI_API_KEY?")

    paths = build_output_paths(args)

    print(f"Generating {args.count} image(s) with {MODEL} ({args.size}, {args.quality})...")
    try:
        result = generate(client, args)
    except Exception as exc:  # noqa: BLE001
        sys.exit(f"Image generation failed: {exc}")

    saved = 0
    for item, path in zip(result.data, paths):
        if not item.b64_json:
            print(f"  ! No image data returned for {path.name}", file=sys.stderr)
            continue
        path.write_bytes(base64.b64decode(item.b64_json))
        print(f"  Saved: {path.resolve()}")
        saved += 1

    if saved == 0:
        sys.exit("No images were saved.")
    print(f"Done. {saved} image(s) saved.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
