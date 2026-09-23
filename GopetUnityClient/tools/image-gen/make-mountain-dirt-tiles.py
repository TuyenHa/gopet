"""Create only 12179/12180: snow to Linh Lam dirt, preserving rock and alpha."""
import runpy
from pathlib import Path


def main():
    forest = runpy.run_path(str(Path(__file__).with_name('make-path-reskin-tiles.py')))
    palette = dict(forest['SNOW_TO_DIRT'])
    # Additional mountain snow shadow; same dirt shadow as forest snow.
    palette[(219, 236, 240)] = (178, 162, 103)
    for source_id in (179, 180):
        target_id = 12000 + source_id
        cells, changed = forest['convert'](source_id, target_id, palette)
        print(f'Wrote {target_id}.png ({cells} cells, {changed} snow pixels changed)')


if __name__ == '__main__':
    main()
