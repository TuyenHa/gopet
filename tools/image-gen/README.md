# Game Image Generator

Small CLI that generates images for the game via OpenAI's `gpt-image-1.5` model.

## Setup

```bash
cd tools/image-gen
python -m venv .venv           # optional but recommended
.venv\Scripts\activate         # Windows (PowerShell: .venv\Scripts\Activate.ps1)
pip install -r requirements.txt

cp .env.example .env           # then edit .env and paste your OPENAI_API_KEY
```

## Usage

```bash
# Basic — saves to output/<timestamp>.png
python generate-image.py "pixel-art fire dragon boss, side view"

# Custom path, size and quality
python generate-image.py "wooden sword icon" -o assets/sword.png -s 1024x1024 -q high

# Transparent background (great for sprites/icons)
python generate-image.py "health potion icon" --transparent

# Generate several variations at once
python generate-image.py "forest tile" -n 4
```

## Options

| Flag | Description | Default |
|------|-------------|---------|
| `prompt` | Text description (required) | — |
| `-o, --output` | Output file path (single image only) | `output/<timestamp>.png` |
| `-s, --size` | `1024x1024`, `1024x1536`, `1536x1024`, `auto` | `1024x1024` |
| `-q, --quality` | `low`, `medium`, `high`, `auto` | `high` |
| `-n, --count` | Number of images to generate | `1` |
| `--transparent` | Transparent background | off |

## Notes

- `OPENAI_API_KEY` is read from `.env` (or the shell environment). Never commit `.env`.
- Generated images and `.env` are git-ignored.
- Image generation is billed per image by OpenAI.
