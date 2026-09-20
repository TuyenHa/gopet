"""Sinh ao sen (nước + lá sen + hoa sen) đặt trên map rừng nhiệt đới.

    python image-gen/make-lotus-pond.py     # hoặc: npm run gen:lotus-pond

Ghi ra Assets/Resources/Jar/Art/Raw/newMapData/14200.png — vật thể MỚI, không thay
ảnh gốc nào, nên id nằm ngoài dải của jar. Neo giữa-đáy như mọi vật thể map
(engine vẽ tại x - w/2, y - h).

Dáng và bảng màu LẤY MẪU TỪ ẢNH THAM CHIẾU người chơi gửi:

  - Hồ méo, không phải bầu dục: hợp của mấy đĩa chồng nhau.
  - Mép nước có hai vành: một nét lam sáng sát bờ rồi một dải lam đậm, trong cùng mới
    là mặt nước phẳng (33,172,191). Không có bờ cát — nước giáp thẳng nền.
  - Lá sen TO và tròn, gân toả từ tâm, dưới mép lá có vệt sáng nổi trên mặt nước.
  - Hoa sen nhiều lớp cánh: cánh ngoài hồng nhạt, cánh trong hồng đậm, nhị vàng.

Màu hồng/lam ở đây là màu MỚI theo ảnh mẫu, không lấy từ bảng của jar.
"""

import math
import os

from pixel_art import canvas, save

OUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Jar", "Art", "Raw", "newMapData")

TARGET_ID = 14200
WIDTH, HEIGHT = 132, 84

WATER = (33, 172, 191)
WATER_EDGE = (16, 138, 168)
WATER_RIM = (108, 224, 222)
PAD_BODY = (63, 238, 101)
PAD_VEIN = (41, 206, 76)
PAD_SHADE = (32, 168, 66)
PETAL_OUT = (255, 199, 202)
PETAL_IN = (242, 113, 143)
POLLEN = (255, 213, 74)

# Hồ = hợp của năm đĩa; lệch tâm và khác cỡ nên bờ gấp khúc tự nhiên.
DISCS = ((46, 36, 34, 24), (86, 36, 32, 22), (66, 46, 42, 26),
         (34, 48, 26, 20), (100, 46, 28, 20))

# Lá sen: (x, y, bán kính ngang). To gần bằng 1/5 bề ngang hồ như ảnh mẫu.
PADS = ((40, 30, 13), (78, 26, 12), (58, 48, 14), (99, 45, 11), (28, 52, 11))
# Hoa sen: (x, y) tâm bông — hai bông nép mép lá, một bông nổi giữa nước.
BLOSSOMS = ((60, 22), (92, 36), (44, 46))

PAD_SQUASH = 0.84       # lá hơi bẹt theo chiều đứng, gần tròn như ảnh mẫu
PAD_VEINS = 7

# Hình học bông sen: 8 cánh, mỗi cánh là một bầu dục nhỏ nằm cách tâm PETAL_OFFSET.
BLOSSOM_PETALS = 6
BLOSSOM_SQUASH = 0.82
PETAL_OFFSET = 3.4
PETAL_LONG = 3.4
PETAL_WIDE = 2.0
POLLEN_RADIUS = 1.6


def disc_mask():
    """Bitmap mặt hồ: pixel nào nằm trong ÍT NHẤT một đĩa."""
    mask = [[False] * WIDTH for _ in range(HEIGHT)]
    for cx, cy, rx, ry in DISCS:
        for y in range(max(0, cy - ry - 1), min(HEIGHT, cy + ry + 2)):
            for x in range(max(0, cx - rx - 1), min(WIDTH, cx + rx + 2)):
                dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
                if dx * dx + dy * dy <= 1.0:
                    mask[y][x] = True
    return mask


def depth_map(mask):
    """Khoảng cách từ mỗi pixel nước tới bờ gần nhất (chamfer 2 lượt).

    Cần độ sâu chứ không chỉ cần biết trong/ngoài: hai vành mép nước dày mỏng theo
    khoảng cách tới bờ, tính bằng cách co hình thì phải co nhiều lần, chậm và rối.
    """
    big = WIDTH * HEIGHT
    dist = [[0 if not mask[y][x] else big for x in range(WIDTH)] for y in range(HEIGHT)]
    for y in range(HEIGHT):
        for x in range(WIDTH):
            if not mask[y][x]:
                continue
            best = dist[y][x]
            for dx, dy in ((-1, 0), (0, -1), (-1, -1), (1, -1)):
                nx, ny = x + dx, y + dy
                near = dist[ny][nx] if 0 <= nx < WIDTH and 0 <= ny < HEIGHT else 0
                best = min(best, near + 1)
            dist[y][x] = best
    for y in range(HEIGHT - 1, -1, -1):
        for x in range(WIDTH - 1, -1, -1):
            if not mask[y][x]:
                continue
            best = dist[y][x]
            for dx, dy in ((1, 0), (0, 1), (1, 1), (-1, 1)):
                nx, ny = x + dx, y + dy
                near = dist[ny][nx] if 0 <= nx < WIDTH and 0 <= ny < HEIGHT else 0
                best = min(best, near + 1)
            dist[y][x] = best
    return dist


def draw_water(put, dist):
    """Mặt nước ba vành theo độ sâu: nét sáng sát bờ, dải đậm, rồi nước phẳng."""
    for y in range(HEIGHT):
        for x in range(WIDTH):
            depth = dist[y][x]
            if depth <= 0:
                continue
            if depth == 1:
                put(x, y, WATER_RIM)
            elif depth <= 2:
                put(x, y, WATER_EDGE)
            else:
                put(x, y, WATER)


def draw_pad(put, cx, cy, rx):
    """Một lá sen: thân xanh sáng, gân toả từ tâm, nửa dưới tối, mép dưới có vệt nước."""
    ry = rx * PAD_SQUASH
    for y in range(int(cy - ry) - 2, int(cy + ry) + 3):
        for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            r2 = dx * dx + dy * dy
            if r2 > 1.0:
                # Vệt sáng ôm mép DƯỚI lá — chỗ lá chạm mặt nước.
                if r2 <= 1.5 and dy > 0.25:
                    put(x, y, WATER_RIM)
                continue
            angle = math.atan2(dy, dx)
            on_vein = abs((angle * PAD_VEINS / math.pi) % 2 - 1) > 0.86
            if r2 > 0.82 or (on_vein and r2 > 0.12):
                put(x, y, PAD_VEIN if dy < 0.35 else PAD_SHADE)
            else:
                put(x, y, PAD_BODY)


def draw_blossom(put, cx, cy):
    """Bông sen ~13x11: tám cánh rời toả quanh nhị vàng.

    Vẽ TỪNG CÁNH chứ không tô mấy vòng tròn đồng tâm: vòng đồng tâm ở cỡ này ra cái
    bia bắn (thử rồi), phải thấy được từng cánh thì mới ra hoa sen.
    """
    for y in range(cy - 7, cy + 8):
        for x in range(cx - 8, cx + 9):
            ux, uy = x + 0.5 - cx, (y + 0.5 - cy) / BLOSSOM_SQUASH
            if ux * ux + uy * uy <= POLLEN_RADIUS * POLLEN_RADIUS:
                put(x, y, POLLEN)
                continue
            for petal in range(BLOSSOM_PETALS):
                angle = math.pi / 2 + petal * 2 * math.pi / BLOSSOM_PETALS
                cos_a, sin_a = math.cos(angle), math.sin(angle)
                along = ux * cos_a + uy * sin_a - PETAL_OFFSET
                across = -ux * sin_a + uy * cos_a
                if (along / PETAL_LONG) ** 2 + (across / PETAL_WIDE) ** 2 > 1.0:
                    continue
                # Nửa ngoài cánh nhạt hơn nửa trong — cánh sen thật cũng nhạt dần ra mép.
                put(x, y, PETAL_OUT if along > 0 else PETAL_IN)
                break


def main():
    image, put = canvas(WIDTH, HEIGHT)

    draw_water(put, depth_map(disc_mask()))
    for cx, cy, rx in PADS:
        draw_pad(put, cx, cy, rx)
    for cx, cy in BLOSSOMS:
        draw_blossom(put, cx, cy)

    save(image, OUT_DIR, TARGET_ID)


if __name__ == "__main__":
    main()
