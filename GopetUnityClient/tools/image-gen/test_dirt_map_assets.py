"""Read-only pixel contracts for dirt strips; run with python -m unittest."""
import unittest
from pathlib import Path
from PIL import Image

ART = Path(__file__).resolve().parents[2] / 'Assets/Resources/Jar/Art/Raw/newMapData'
PAIRS = ((151, 12151), (161, 12161), (162, 12162), (179, 12179), (180, 12180))
# Hand-reviewed colors of snow, not a threshold that could recolor blue water.
SNOW = {
    (255,255,255), (243,247,251), (240,245,251), (213,227,243),
    (180,216,254), (160,191,228), (56,193,234), (55,153,182), (219,236,240),
}
DIRT = {
    (206,192,136), (197,182,122), (190,174,114), (178,162,103),
    (181,165,107), (169,153,99), (156,140,91), (138,124,81),
}


class DirtMapAssetTests(unittest.TestCase):
    def load_pair(self, source_id, target_id):
        target = ART / f'{target_id}.png'
        self.assertTrue(target.is_file(), f'Missing dirt asset: {target.name}')
        with Image.open(ART / f'{source_id}.png') as image:
            source = image.convert('RGBA')
        with Image.open(target) as image:
            dirt = image.convert('RGBA')
        return source, dirt

    def test_geometry_and_alpha_preserved(self):
        for source_id, target_id in PAIRS:
            with self.subTest(target=target_id):
                source, dirt = self.load_pair(source_id, target_id)
                self.assertEqual(source.size, dirt.size)
                self.assertEqual(24, dirt.height)
                self.assertEqual(0, dirt.width % 24)
                self.assertEqual(source.getchannel('A').tobytes(), dirt.getchannel('A').tobytes())

    def test_only_snow_changes_and_road_stays_dirt(self):
        for source_id, target_id in PAIRS:
            with self.subTest(target=target_id):
                source, dirt = self.load_pair(source_id, target_id)
                changed = 0
                for before, after in zip(source.getdata(), dirt.getdata()):
                    if before[3] and before[:3] in SNOW:
                        self.assertIn(after[:3], DIRT)
                        changed += 1
                    else:
                        self.assertEqual(before, after, 'Water/grass/rock/transparent pixel changed')
                self.assertGreater(changed, 0)

    def test_water_and_gray_cells_kept_in_forest_strip(self):
        source, dirt = self.load_pair(162, 12162)
        protected = {(144,242,255), (75,193,209), (116,117,121)}
        seen = set()
        for before, after in zip(source.getdata(), dirt.getdata()):
            if before[:3] in protected:
                seen.add(before[:3])
                self.assertEqual(before, after)
        self.assertEqual(protected, seen)


if __name__ == '__main__':
    unittest.main()
