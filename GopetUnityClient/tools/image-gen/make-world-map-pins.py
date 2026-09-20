"""Sinh icon điểm đến cho màn bản đồ thế giới.

    python image-gen/make-world-map-pins.py

Ghi thẳng vào Assets/Resources/Ui/WorldMap/ hai file:

    world-map-pin.png          pin vàng  - map mở, bấm vào là dịch chuyển
    world-map-pin-current.png  pin xanh  - map đang đứng, có ngôi sao trong lòng

Vẽ bằng script chứ không sinh bằng AI: icon chỉ 16-20px trên màn, ảnh AI ở cỡ đó
rìa lem và alpha bẩn, phải dọn tay. Script thì đổi màu/tỉ lệ là chạy lại một dòng.

Mọi hình vẽ ở độ phân giải gấp SUPERSAMPLE lần rồi thu nhỏ bằng LANCZOS — vẽ
thẳng ở cỡ thật thì đường cong của giọt nước răng cưa rõ.
"""

import math
import os

from PIL import Image, ImageDraw

SUPERSAMPLE = 8
PIN_SIZE = (64, 80)

OUTLINE = (43, 26, 14, 255)
GOLD_TOP, GOLD_BOTTOM = (255, 211, 77, 255), (232, 147, 15, 255)
GOLD_EYE = (255, 246, 213, 255)
GREEN_TOP, GREEN_BOTTOM = (107, 239, 155, 255), (23, 168, 90, 255)
GREEN_OUTLINE = (12, 58, 32, 255)
STAR = (255, 255, 255, 255)

OUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Ui", "WorldMap")


def teardrop(draw, center, radius, tip_y, color):
    """Đầu tròn + đuôi nhọn chúc xuống — dáng pin bản đồ kinh điển."""
    cx, cy = center
    draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=color)

    # Hai điểm tiếp tuyến từ mũi nhọn tới đầu tròn: nối thẳng tới mép tròn thì chỗ
    # giáp nhau gãy thành một bậc, tiếp tuyến mới cho đuôi liền mạch.
    distance = tip_y - cy
    if distance <= radius:
        return
    along = radius * radius / distance
    across = math.sqrt(max(radius * radius - along * along, 0.0))
    draw.polygon([
        (cx - across, cy + along),
        (cx, tip_y),
        (cx + across, cy + along),
    ], fill=color)


def vertical_gradient(size, top, bottom):
    width, height = size
    column = Image.new("RGBA", (1, height))
    pixels = column.load()
    for y in range(height):
        t = y / max(height - 1, 1)
        pixels[0, y] = tuple(int(round(a + (b - a) * t)) for a, b in zip(top, bottom))
    return column.resize((width, height), Image.NEAREST)


def star_points(center, outer, inner, points=5):
    cx, cy = center
    result = []
    for i in range(points * 2):
        angle = -math.pi / 2 + i * math.pi / points
        radius = outer if i % 2 == 0 else inner
        result.append((cx + radius * math.cos(angle), cy + radius * math.sin(angle)))
    return result


def make_pin(top, bottom, outline, eye_star):
    """Pin có viền tối dày: nền bản đồ nhiều màu, không viền là icon chìm mất."""
    width, height = (value * SUPERSAMPLE for value in PIN_SIZE)
    scale = SUPERSAMPLE
    center = (32 * scale, 26 * scale)
    radius, tip_y, stroke = 21 * scale, 78 * scale, 3 * scale

    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)
    teardrop(draw, center, radius + stroke, tip_y, outline)

    # Thân tô bằng dải chuyển màu, cắt theo đúng vùng bên trong viền.
    mask = Image.new("L", (width, height), 0)
    teardrop(ImageDraw.Draw(mask), center, radius, tip_y - stroke, 255)
    canvas.paste(vertical_gradient((width, height), top, bottom), (0, 0), mask)

    draw = ImageDraw.Draw(canvas)
    if eye_star:
        draw.polygon(star_points(center, 12 * scale, 5.5 * scale), fill=STAR)
    else:
        eye = 9 * scale
        draw.ellipse([center[0] - eye - stroke * 0.6, center[1] - eye - stroke * 0.6,
                      center[0] + eye + stroke * 0.6, center[1] + eye + stroke * 0.6],
                     fill=outline)
        draw.ellipse([center[0] - eye, center[1] - eye, center[0] + eye, center[1] + eye],
                     fill=GOLD_EYE)

    # Chấm sáng lệch trên-trái: cho khối tròn có chiều sâu thay vì phẳng lì. Vẽ ở
    # lớp riêng rồi chồng lên — ImageDraw GHI ĐÈ pixel chứ không trộn alpha, tô
    # thẳng lên thân là thủng một mảng trắng đục.
    gloss = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    ImageDraw.Draw(gloss).ellipse(
        [center[0] - 14 * scale, center[1] - 16 * scale,
         center[0] - 2 * scale, center[1] - 9 * scale],
        fill=(255, 255, 255, 110))
    canvas.alpha_composite(gloss)
    return canvas.resize(PIN_SIZE, Image.LANCZOS)


def main():
    out = os.path.normpath(OUT_DIR)
    os.makedirs(out, exist_ok=True)
    make_pin(GOLD_TOP, GOLD_BOTTOM, OUTLINE, eye_star=False) \
        .save(os.path.join(out, "world-map-pin.png"))
    make_pin(GREEN_TOP, GREEN_BOTTOM, GREEN_OUTLINE, eye_star=True) \
        .save(os.path.join(out, "world-map-pin-current.png"))
    # Console Windows mac dinh cp1252 - thong bao giu ASCII cho khoi vo print.
    print("Wrote 2 pin icons to " + out)


if __name__ == "__main__":
    main()
