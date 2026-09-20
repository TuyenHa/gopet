"""Sinh nhà lá Việt Nam thay nhà gỗ phủ tuyết cho map rừng nhiệt đới.

    python image-gen/make-thatch-house.py   # hoặc: npm run gen:thatch-house

Ghi ra Assets/Resources/Jar/Art/Raw/newMapData/14177.png — bản thay của 177.png
(nhà gỗ mái ngói đỏ, phủ tuyết). Giữ ĐÚNG khung 103x91 và neo giữa-đáy của bản gốc:
engine vẽ vật thể tại (x - w/2, y - h), lệch khung là nhà lệch chỗ trên map.

Dựng bằng hình khối phối cảnh trục đo (isometric) chứ không vẽ tay từng pixel: mọi
mặt đều là đa giác lồi suy từ một cái móng hình thoi, nên đổi kích thước nhà chỉ cần
sửa ba vector ở FOOT_*. Mái tranh bốn chái (mái hông), hiên đua rộng, vách phên tre —
dáng nhà lá đồng bằng Bắc Bộ.
"""

import os

from pixel_art import canvas, fill_polygon, line, outline, save

OUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Jar", "Art", "Raw", "newMapData")

TARGET_ID = 14177
WIDTH, HEIGHT = 103, 91

# Mái tranh: rơm khô, sáng ở mặt hứng nắng (hướng đông-nam), tối ở chái tây-nam.
#
# Cố ý KHÔNG lấy vàng rơm tươi: nền map là đất cát (190,174,114), rơm tươi sáng gần
# đúng tông đó nên cả cái nhà chìm vào nền. Hạ sáng + ngả nâu để mái nổi lên.
THATCH_LIT = (198, 158, 86)
THATCH_LIT_LINE = (166, 126, 62)
THATCH_DIM = (156, 118, 58)
THATCH_DIM_LINE = (126, 92, 44)
THATCH_RIDGE = (112, 80, 38)
# Vách phên tre và cột gỗ — nâu đậm hơn hẳn nền đất để cái nhà tách khỏi mặt đất.
WALL_LIT = (168, 122, 68)
WALL_DIM = (128, 90, 48)
WALL_LIT_LINE = (146, 104, 56)
WALL_DIM_LINE = (108, 74, 38)
POST = (88, 56, 28)
DOOR = (54, 36, 22)
OUTLINE = (46, 28, 14)

# Móng nhà hình thoi: S là góc trước (gần người xem nhất), u chạy sang đông,
# w chạy sang tây. Đổi ba dòng này là đổi toàn bộ tỉ lệ nhà.
FOOT_S = (51, 84)
FOOT_U = (36, -11)
FOOT_W = (-36, -11)
WALL_HEIGHT = 24
RIDGE_HEIGHT = 19
EAVE_OUT = (6, 2)        # hiên đua ra khỏi vách, theo hướng nhìn
EAVE_LONG = (5, -2)      # đua thêm về hai đầu hồi


def add(*points):
    return (sum(p[0] for p in points), sum(p[1] for p in points))


def scale(point, factor):
    return (point[0] * factor, point[1] * factor)


def geometry():
    """Mọi đỉnh cần vẽ, suy từ móng + chiều cao. Trả về dict cho dễ đọc bên dưới."""
    s = FOOT_S
    e = add(s, FOOT_U)
    w = add(s, FOOT_W)
    up = (0, -WALL_HEIGHT)
    st, et, wt, nt = add(s, up), add(e, up), add(w, up), add(s, FOOT_U, FOOT_W, up)

    # Nóc chạy song song trục u, nằm trên đường giữa hai mái.
    ridge_up = (0, -RIDGE_HEIGHT)
    ridge_s = add(scale(add(st, wt), 0.5), ridge_up)
    ridge_e = add(scale(add(et, nt), 0.5), ridge_up)

    out, long = EAVE_OUT, EAVE_LONG
    back = (-out[0], -out[1])
    return {
        "ground": (w, s, e),
        "wall_left": (w, s, st, wt),
        "wall_right": (s, e, et, st),
        # Diềm mái: đua ra khỏi vách rồi đua tiếp về hai đầu hồi.
        "eave_s": add(st, out, (-long[0], -long[1])),
        "eave_e": add(et, out, long),
        "eave_w": add(wt, back, (-long[0], -long[1])),
        "eave_n": add(nt, back, long),
        "ridge_s": add(ridge_s, (-long[0], -long[1])),
        "ridge_e": add(ridge_e, long),
    }


def thatch_lines(put, eave_a, eave_b, ridge_a, ridge_b, color, rows=6):
    """Gân rơm: mấy đường song song với diềm, thưa dần lên nóc — chính cái này làm
    mái đọc ra 'tranh' chứ không phải một mảng màu phẳng."""
    for row in range(1, rows):
        t = row / rows
        a = (eave_a[0] + (ridge_a[0] - eave_a[0]) * t, eave_a[1] + (ridge_a[1] - eave_a[1]) * t)
        b = (eave_b[0] + (ridge_b[0] - eave_b[0]) * t, eave_b[1] + (ridge_b[1] - eave_b[1]) * t)
        line(put, a, b, color)


def draw_walls(put, g):
    fill_polygon(put, g["wall_left"], WALL_DIM)
    fill_polygon(put, g["wall_right"], WALL_LIT)
    # Phên tre: nan dọc thưa, mặt tối kẻ dày hơn cho thấy chiều sâu.
    # Nan phên: thưa và chỉ chênh một tông. Kẻ dày/đậm thì vách thành hàng rào cọc.
    for face, shade in ((g["wall_left"], WALL_DIM_LINE), (g["wall_right"], WALL_LIT_LINE)):
        bottom_a, bottom_b, top_b, top_a = face
        for i in range(1, 8):
            t = i / 8
            a = (bottom_a[0] + (bottom_b[0] - bottom_a[0]) * t,
                 bottom_a[1] + (bottom_b[1] - bottom_a[1]) * t)
            b = (top_a[0] + (top_b[0] - top_a[0]) * t, top_a[1] + (top_b[1] - top_a[1]) * t)
            line(put, a, b, shade)
    # Ba cột cái ở ba góc nhìn thấy.
    for base, top in ((g["wall_left"][0], g["wall_left"][3]),
                      (g["wall_left"][1], g["wall_left"][2]),
                      (g["wall_right"][1], g["wall_right"][2])):
        line(put, base, top, POST)
        line(put, (base[0] + 1, base[1]), (top[0] + 1, top[1]), POST)


def draw_door(put, g):
    """Cửa ra vào giữa vách trước, có khung gỗ hai bên."""
    s, e, et, st = g["wall_right"]
    for frame, (lo, hi), tall in ((POST, (0.28, 0.62), 18), (DOOR, (0.32, 0.58), 16)):
        left = (s[0] + (e[0] - s[0]) * lo, s[1] + (e[1] - s[1]) * lo)
        right = (s[0] + (e[0] - s[0]) * hi, s[1] + (e[1] - s[1]) * hi)
        top_left = (left[0], left[1] - tall)
        top_right = (right[0], right[1] - tall)
        fill_polygon(put, (left, right, top_right, top_left), frame)


def draw_roof(put, g):
    """Mái trước (hứng nắng) và chái tây-nam (khuất) — hai mặt duy nhất nhìn thấy."""
    front = (g["eave_s"], g["eave_e"], g["ridge_e"], g["ridge_s"])
    hip = (g["eave_s"], g["ridge_s"], g["eave_w"])
    fill_polygon(put, front, THATCH_LIT)
    fill_polygon(put, hip, THATCH_DIM)
    thatch_lines(put, g["eave_s"], g["eave_e"], g["ridge_s"], g["ridge_e"], THATCH_LIT_LINE)
    thatch_lines(put, g["eave_s"], g["eave_w"], g["ridge_s"], g["ridge_s"], THATCH_DIM_LINE, 4)
    # Bờ nóc: nhà lá bó một con trạch rơm dọc sống mái, sẫm hơn hẳn hai mặt mái.
    for d in range(2):
        line(put, (g["ridge_s"][0], g["ridge_s"][1] + d),
             (g["ridge_e"][0], g["ridge_e"][1] + d), THATCH_RIDGE)

    # Diềm mái tua rua: rơm thõng xuống không đều, thẳng băng là ra mái tôn.
    for edge, color in ((("eave_s", "eave_e"), THATCH_LIT_LINE),
                        (("eave_s", "eave_w"), THATCH_DIM_LINE)):
        a, b = g[edge[0]], g[edge[1]]
        steps = int(abs(b[0] - a[0]))
        for i in range(steps + 1):
            t = i / max(steps, 1)
            x = a[0] + (b[0] - a[0]) * t
            y = a[1] + (b[1] - a[1]) * t
            for d in range(1 + (i % 3)):
                put(round(x), round(y) + d, color)


def main():
    image, put = canvas(WIDTH, HEIGHT)
    g = geometry()
    draw_walls(put, g)
    draw_door(put, g)
    draw_roof(put, g)          # mái vẽ SAU: phải đè lên đầu vách
    outline(image, OUTLINE)
    save(image, OUT_DIR, TARGET_ID)


if __name__ == "__main__":
    main()
