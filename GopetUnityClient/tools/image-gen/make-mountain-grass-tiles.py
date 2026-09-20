"""Sinh dải ô nền HẾT TUYẾT cho map Đường lên đỉnh núi (map 16).

    python image-gen/make-mountain-grass-tiles.py    # hoặc: npm run gen:mountain-tiles

Ghi vào Assets/Resources/Jar/Art/Raw/newMapData/:

    11180.png  <- 180.png, mặt tuyết          -> thảm cỏ xanh
    11179.png  <- 179.png, vách đá phủ tuyết  -> vách đá, phần tuyết thành cỏ

Không vẽ lại từ đầu mà thay màu trên dải gốc: hình dạng, mép nối và thứ tự ô giữ
nguyên tuyệt đối nên map cũ ghép vào vẫn khít, không phải sửa file map nhị phân.
Dải gốc chỉ có 6-16 màu nên bảng tra tay là đủ, không cần dò ngưỡng.

Mặt cỏ KHÔNG tô màu phẳng mà LẤY THẲNG hoạ tiết cỏ của 11161 — đó chính là dải cỏ
phủ nửa dưới map 16, hai vùng giáp nhau phải cùng một chất cỏ thì mới liền mạch.
Đá và đất nâu của 179 giữ nguyên: chỉ tuyết mới phải đi.
"""

import os

from PIL import Image

SRC_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Jar", "Art", "Raw", "newMapData")

TILE = 24

# Ô cỏ nguồn: mọi ô của 11161 đều là cỏ đặc, lấy ô đầu làm hoạ tiết.
GRASS_SOURCE = "11161.png"

# Sáu màu tuyết/băng của dải gốc, kèm mức chỉnh sáng khi đổi sang cỏ. Rìa băng
# thành cỏ sẫm để nét bo tròn của ô gốc không biến mất giữa nền cỏ.
SNOW_TO_GRASS_SHADE = {
    (240, 245, 251): 0,     # mặt tuyết chính
    (219, 236, 240): -14,   # tuyết đổ bóng
    (180, 216, 254): 16,    # rìa sáng
    (160, 191, 228): -28,   # rìa tối
    (56, 193, 234): -42,    # viền băng sáng
    (55, 153, 182): -66,    # viền băng tối
}


def shade(color, amount):
    return tuple(max(0, min(255, channel + amount)) for channel in color)


def grass_patch():
    """Một ô cỏ 24×24 để lát lên chỗ từng là tuyết."""
    source = Image.open(os.path.join(SRC_DIR, GRASS_SOURCE)).convert("RGBA")
    return source.crop((0, 0, TILE, TILE)).load()


def grassify(image):
    """
    Đổi mọi màu tuyết/băng sang cỏ, giữ nguyên đá và đất. Lấy mẫu hoạ tiết cỏ theo
    toạ độ TRONG Ô (x % 24) nên cỏ liền mạch khi các ô nằm cạnh nhau.
    """
    patch = grass_patch()
    pixels = image.load()
    width, height = image.size
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            amount = SNOW_TO_GRASS_SHADE.get((r, g, b))
            if amount is None:
                continue
            gr, gg, gb, _ = patch[x % TILE, y % TILE]
            pixels[x, y] = shade((gr, gg, gb), amount) + (a,)


def convert(source, target):
    image = Image.open(os.path.join(SRC_DIR, source)).convert("RGBA")
    grassify(image)
    image.save(os.path.join(SRC_DIR, target))
    return target, image.width // TILE


def main():
    for source, target in (("180.png", "11180.png"), ("179.png", "11179.png")):
        name, cells = convert(source, target)
        print(f"Wrote {name} ({cells} cells)")


if __name__ == "__main__":
    main()
