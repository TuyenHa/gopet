"""Sinh hiệu ứng CHÉM của nút đánh quái ngoài map.

    python image-gen/make-slash-effect.py

Ghi ra `Assets/Resources/Ui/fx-slash.png` (192x192).

Vệt lửa vàng quét thành vòng cung rộng, dựng theo ảnh tham chiếu người chơi gửi
(clip game 2D mobile): KHÔNG phải một lưỡi liềm đặc mà là NHIỀU SỢI lửa chồng lệch
nhau, dày ở đỉnh cung, tãi thành tua ở hai đầu, quanh có quầng sáng vàng và vài đốm
sao lấp lánh. Chính mấy sợi lệch nhau + quầng sáng mới tạo cảm giác "quét" — vẽ một
dải đặc trơn sẽ ra hình trăng khuyết, không ra nhát chém.

Bảng màu lấy MẪU TỪ CHÍNH CLIP: lõi vàng trắng, thân vàng chanh `(248,240,64)`,
rìa vàng cam. Vệt hướng sang PHẢI (+X) ở trạng thái gốc; lúc chạy
`WorldSlashEffect` xoay theo hướng pet → quái và quay thêm một nhịp cho ra động tác
vung, nên chỉ cần một ảnh cho mọi hướng.

Vẽ ở độ phân giải gấp SUPERSAMPLE lần rồi thu nhỏ bằng LANCZOS.
"""

import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

SUPERSAMPLE = 4
SIZE = 192

# Lấy mẫu từ clip tham chiếu: lõi gần trắng, thân vàng chanh, rìa vàng cam.
CORE = (255, 252, 214)
BODY = (248, 240, 64)
EDGE = (250, 196, 36)
GLOW = (252, 176, 24)

ARC_RADIUS = 0.34        # bán kính đường tâm của cung, theo cạnh ảnh
ARC_SWEEP = math.radians(200)
STEPS = 260              # số chấm rải dọc một sợi

# Các sợi lửa: (lệch bán kính, bề dày, mũ vuốt nhọn, lệch góc độ, màu, alpha).
# Lệch bán kính/góc khác nhau nên các sợi không chồng khít -> ra chất "cuộn" của lửa.
STRANDS = (
    (0.00, 0.115, 1.05, 0, EDGE, 235),
    (0.055, 0.060, 1.25, -14, EDGE, 205),
    (-0.050, 0.052, 1.30, 10, EDGE, 195),
    (0.015, 0.072, 1.10, -4, BODY, 245),
    (-0.020, 0.045, 1.35, 16, BODY, 220),
    (0.038, 0.030, 1.45, -22, BODY, 210),
    (0.004, 0.032, 1.20, -2, CORE, 255),
    (-0.030, 0.018, 1.40, 12, CORE, 235),
)

# Đốm sao lấp lánh quanh cung: (góc độ, lệch bán kính, bán kính sao).
SPARKS = ((-78, 0.10, 0.030), (-30, -0.10, 0.020), (18, 0.11, 0.026),
          (62, -0.09, 0.018), (88, 0.06, 0.022))


def strand(draw, size, radius_offset, thickness, taper, rot_offset, color, alpha):
    """Rải chấm tròn dọc một cung; bán kính chấm vuốt nhọn dần về hai đầu."""
    center = size / 2
    radius = size * (ARC_RADIUS + radius_offset)
    rot = math.radians(rot_offset)
    max_half = size * thickness / 2
    for i in range(STEPS + 1):
        t = i / STEPS
        angle = rot - ARC_SWEEP / 2 + ARC_SWEEP * t
        half = max_half * math.sin(math.pi * t) ** taper
        if half <= 0.4:
            continue
        x = center + math.cos(angle) * radius
        y = center + math.sin(angle) * radius
        draw.ellipse((x - half, y - half, x + half, y + half), fill=color + (alpha,))


def spark(draw, size, angle_deg, radius_offset, scale):
    """Sao bốn cánh: hai hình thoi gầy bắt chéo, đúng kiểu lấp lánh trong clip."""
    center = size / 2
    radius = size * (ARC_RADIUS + radius_offset)
    angle = math.radians(angle_deg)
    x = center + math.cos(angle) * radius
    y = center + math.sin(angle) * radius
    long_arm = size * scale
    short_arm = long_arm * 0.22
    draw.polygon([(x, y - long_arm), (x + short_arm, y), (x, y + long_arm), (x - short_arm, y)],
                 fill=CORE + (240,))
    draw.polygon([(x - long_arm, y), (x, y - short_arm), (x + long_arm, y), (x, y + short_arm)],
                 fill=CORE + (240,))


def build():
    size = SIZE * SUPERSAMPLE
    flame = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    for radius_offset, thickness, taper, rot_offset, color, alpha in STRANDS:
        layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        strand(ImageDraw.Draw(layer), size, radius_offset, thickness, taper, rot_offset, color, alpha)
        flame.alpha_composite(layer)

    # Quầng sáng: chính mấy sợi lửa làm mờ rồi nhuộm vàng cam, đặt DƯỚI để lửa vẫn nét.
    glow = flame.filter(ImageFilter.GaussianBlur(size * 0.022))
    tint = Image.new("RGBA", (size, size), GLOW + (0,))
    tint.putalpha(glow.getchannel("A").point(lambda a: int(a * 0.55)))

    image = Image.alpha_composite(tint, flame)
    sparkles = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(sparkles)
    for angle_deg, radius_offset, scale in SPARKS:
        spark(draw, size, angle_deg, radius_offset, scale)
    image.alpha_composite(sparkles)
    return image.resize((SIZE, SIZE), Image.LANCZOS)


def main():
    out_dir = os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "..", "..", "Assets", "Resources", "Ui")
    path = os.path.join(out_dir, "fx-slash.png")
    build().save(path)
    print(f"Wrote {os.path.normpath(path)} ({SIZE}x{SIZE})")


if __name__ == "__main__":
    main()
