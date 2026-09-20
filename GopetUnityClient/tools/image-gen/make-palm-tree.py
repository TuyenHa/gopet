"""Sinh cây dừa thay cây thông cho map rừng nhiệt đới.

    python image-gen/make-palm-tree.py      # hoặc: npm run gen:palm

Ghi ra Assets/Resources/Jar/Art/Raw/newMapData/14158.png — bản thay của 158.png
(cây thông có tuyết). Giữ ĐÚNG khung 52x62 và điểm neo giữa-đáy của bản gốc: engine
đặt vật thể tại (x - w/2, y - h) nên lệch khung là 55 gốc cây trên map 15 lệch theo.

Bảng màu lấy nguyên từ 158.png — cùng tông lá, cùng tông thân, nên cây dừa đứng lẫn
với art gốc không bị chói. Chỉ BỎ hai màu tuyết (56,193,234)/(55,153,182): map đã
chuyển sang nền đất nhiệt đới, không còn tuyết trên tán.
"""

import math
import os

from pixel_art import canvas, outline, save

OUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Jar", "Art", "Raw", "newMapData")

TARGET_ID = 14158
WIDTH, HEIGHT = 52, 62

# Nguyên văn bảng lá/thân của 158.png.
LEAF_LIGHT = (117, 219, 17)
LEAF_MID = (101, 176, 20)
LEAF_DARK = (53, 119, 20)
LEAF_DEEP = (36, 73, 22)
LEAF_LINE = (27, 47, 17)
BARK_LIGHT = (118, 70, 28)
BARK_MID = (79, 46, 18)
BARK_LINE = (32, 15, 5)

# Thân cong: gốc ở giữa đáy, ngọn nghiêng trái. t=0 gốc, t=1 ngọn.
TRUNK_BASE_X, TRUNK_BASE_Y = 27, 61
TRUNK_TOP_X, TRUNK_TOP_Y = 24, 24
CROWN = (TRUNK_TOP_X, TRUNK_TOP_Y)

# Tám tàu lá: góc toả (độ, 0 = sang phải, âm = hướng lên), độ dài, độ rủ.
# Tàu ngang rủ mạnh, tàu chĩa lên rủ nhẹ — dáng vòm của cây dừa.
FRONDS = (
    (-178, 21, 13.0), (-150, 22, 11.0), (-122, 18, 7.0), (-96, 15, 5.0),
    (-84, 15, 5.0), (-58, 18, 7.0), (-30, 22, 11.0), (-2, 21, 13.0),
)
FROND_MAX_BLADE = 4.2


def trunk_point(t):
    """Điểm giữa thân tại tham số t; mũ 1.7 cho cong dần về ngọn như cây dừa thật."""
    x = TRUNK_BASE_X + (TRUNK_TOP_X - TRUNK_BASE_X) * t ** 1.7
    y = TRUNK_BASE_Y + (TRUNK_TOP_Y - TRUNK_BASE_Y) * t
    return x, y


def draw_trunk(put):
    """Thân thon dần, sáng ở mép trái tối ở mép phải, có khía đốt như bẹ dừa."""
    steps = 160
    for i in range(steps + 1):
        t = i / steps
        cx, cy = trunk_point(t)
        half = (6.0 - 2.6 * t) / 2.0
        # Khía đốt: cứ 5 px một vạch ngắn bên phải, không kẻ ngang hết thân —
        # kẻ hết thân thì thân trông như cái thang.
        notch = int(cy) % 5 == 0
        for dx in range(-3, 4):
            if abs(dx) > half:
                continue
            shade = BARK_MID if dx > 0 or (notch and dx > -1) else BARK_LIGHT
            put(int(round(cx)) + dx, int(round(cy)), shade)


def draw_fronds(put):
    """Mỗi tàu là một cung rủ xuống: bản lá phình ở giữa, thon hai đầu, mép ngoài
    khía răng cưa cho ra khe lá chét thay vì một dải trơn."""
    cx, cy = CROWN
    for index, (degrees, length, droop) in enumerate(FRONDS):
        angle = math.radians(degrees)
        lifted = math.sin(angle) < -0.55          # tàu chĩa lên đón sáng
        body = LEAF_MID if lifted else LEAF_DARK
        top = LEAF_LIGHT if lifted else LEAF_MID
        steps = length * 8
        for i in range(steps + 1):
            s = i / steps
            px = cx + math.cos(angle) * length * s
            py = cy + math.sin(angle) * length * s + droop * s * s
            nx, ny = -math.sin(angle), math.cos(angle)
            # Bản lá: phình nhất ở khoảng 40% tàu rồi thon về đầu.
            blade = FROND_MAX_BLADE * math.sin(math.pi * s ** 0.65)
            put(px, py, LEAF_DEEP)                # sống tàu luôn tối hơn bản lá
            for side in (-1, 1):
                for k in range(1, int(blade) + 1):
                    # Khía mép ngoài theo nhịp 3 px -> trông như nhiều lá chét.
                    if k >= blade - 0.6 and i % 3 == 0:
                        continue
                    put(px + nx * k * side, py + ny * k * side,
                        top if side < 0 else body)


def draw_coconuts(put):
    """Hai quả kẹp hai bên thân, ngay dưới mép tán. Đặt lệch vào giữa là thân che mất;
    đặt cao hơn là tán che mất — khe nhìn thấy quả chỉ rộng chừng 4 px."""
    cx, cy = CROWN
    for ox, oy, r in ((-5, 8, 2), (5, 9, 2)):
        for dy in range(-r, r + 1):
            for dx in range(-r, r + 1):
                if dx * dx + dy * dy > r * r:
                    continue
                put(cx + ox + dx, cy + oy + dy,
                    BARK_LIGHT if dx + dy < 0 else BARK_MID)


def main():
    image, put = canvas(WIDTH, HEIGHT)

    # Thân trước, tán sau: tán phải đè lên đầu thân, không thì thấy thân chọc qua lá.
    draw_trunk(put)
    outline(image, BARK_LINE, {BARK_LIGHT, BARK_MID})
    draw_fronds(put)
    outline(image, LEAF_LINE, {LEAF_LIGHT, LEAF_MID, LEAF_DARK, LEAF_DEEP})
    # Quả vẽ SAU tán: vẽ trước thì tán phủ kín, không còn thấy quả nào.
    draw_coconuts(put)
    outline(image, BARK_LINE, {BARK_LIGHT, BARK_MID})

    save(image, OUT_DIR, TARGET_ID)


if __name__ == "__main__":
    main()
