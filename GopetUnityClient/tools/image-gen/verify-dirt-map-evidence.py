"""Verify saved offline tile renders and baseline assets; never edits game assets.

Usage: python -B GopetUnityClient/tools/image-gen/verify-dirt-map-evidence.py
The before/after PNGs are diagnostic layer renders, NOT Unity screenshots.
"""
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw


ROOT = Path(__file__).resolve().parents[3]
EVIDENCE = ROOT / 'docs/superpowers/plans/assets/doi-duong-dat'
CHANGED_MAPS = {12, 14, 17, 18, 20, 21, 22}


def main():
    baseline = json.loads((EVIDENCE / 'baseline-hashes.json').read_text())
    hashes = {}
    errors = []
    for relative, expected in baseline.items():
        path = ROOT / relative
        actual = hashlib.sha256(path.read_bytes()).hexdigest() if path.is_file() else None
        hashes[relative] = actual
        if actual != expected:
            errors.append(f'Baseline resource changed or missing: {relative}')

    maps = []
    for map_id in range(11, 35):
        with Image.open(EVIDENCE / f'before/{map_id}.png') as source:
            before = source.convert('RGB')
        with Image.open(EVIDENCE / f'after/{map_id}.png') as source:
            after = source.convert('RGB')
        if before.size != after.size:
            errors.append(f'Map {map_id}: render dimensions changed')
            continue
        changed = sum(a != b for a, b in zip(before.getdata(), after.getdata()))
        expected_change = map_id in CHANGED_MAPS
        if bool(changed) != expected_change:
            errors.append(f'Map {map_id}: unexpected changed-pixel count {changed}')
        maps.append({
            'map_id': map_id, 'size': list(before.size),
            'changed_pixels': changed, 'expected_change': expected_change,
            'changed_bounds': ImageChops.difference(before, after).getbbox(),
            'before': f'before/{map_id}.png', 'after': f'after/{map_id}.png',
        })

    # Side-by-side contact sheets, nearest-neighbor scaling for pixel art.
    for start in (11, 19, 27):
        sheet = Image.new('RGB', (1024, 8 * 280), '#303f4a')
        draw = ImageDraw.Draw(sheet)
        for row, map_id in enumerate(range(start, start + 8)):
            for col, phase in enumerate(('before', 'after')):
                with Image.open(EVIDENCE / f'{phase}/{map_id}.png') as source:
                    frame = source.convert('RGB')
                frame.thumbnail((500, 250), Image.Resampling.NEAREST)
                x, y = col * 512, row * 280
                draw.text((x + 8, y + 5), f'Map {map_id} - {phase} (offline tiles)', fill='white')
                sheet.paste(frame, (x, y + 25))
        sheet.save(EVIDENCE / f'compare-{start}-{start + 7}.png')

    report = {
        'scope': 'Offline tile layers only; no Unity import, objects, animation or gameplay validation',
        'baseline_count': len(baseline),
        'baseline_mismatches': [p for p, value in hashes.items() if value != baseline[p]],
        'maps': maps, 'errors': errors,
        'unity_playmode': 'NOT RUN: Unity Editor 6000.5.4f1 unavailable',
        'interactive_gameplay': 'NOT RUN: requires Unity Editor and game session',
    }
    (EVIDENCE / 'after-hashes.json').write_text(json.dumps(hashes, indent=2) + '\n')
    (EVIDENCE / 'verification.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f'Baseline: {len(baseline)} files; mismatches: {len(report["baseline_mismatches"])}')
    for item in maps:
        print(f'Map {item["map_id"]}: {item["changed_pixels"]} changed pixels')
    if errors:
        raise SystemExit('\n'.join(errors))
    print('PASS: exactly the seven approved maps changed; all 17 preserved maps are pixel-identical.')


if __name__ == '__main__':
    main()
