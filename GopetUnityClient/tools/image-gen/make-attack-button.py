"""Sinh icon nút đánh quái (hai thanh kiếm bắt chéo) cho HUD.

    python image-gen/make-attack-button.py

Ghi ra `Assets/Resources/Ui/attack-button.png` (128x128).

Bảng màu lấy thẳng từ `Ui/dpad.png` và `Ui/dpad-knob.png` — nút đánh đứng cạnh
cần điều khiển nên phải cùng một bộ: mặt trắng xanh bóng, vành xanh nhạt, chi
tiết xanh thép.

Vẽ ở độ phân giải gấp SUPERSAMPLE lần rồi thu nhỏ bằng LANCZOS; kiếm xoay 40 độ
nên vẽ thẳng ở cỡ thật thì lưỡi kiếm răng cưa thấy rõ.
"""

import os

from PIL import Image, ImageDraw

SUPERSAMPLE = 8
SIZE = 128

# Lay mau tu dpad.png / dpad-knob.png
FACE_LIGHT = (252, 253, 254, 255)
FACE_EDGE = (196, 217, 237, 255)
RIM_LIGHT = (246, 249, 250, 255)
RIM_DARK = (150, 185, 220, 255)
BLADE_LIGHT = (243, 248, 252, 255)
BLADE_DARK = (150, 183, 218, 255)
STEEL_LINE = (70, 108, 152, 255)
GRIP = (60, 91, 130, 255)
GUARD = (110, 147, 192, 255)

OUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Ui")


def radial_disc(size, inner, outer):
    """Đĩa chuyển màu từ tâm ra vành — cho mặt nút cảm giác cong, không phẳng lì."""
    disc = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    pixels = disc.load()
    center = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            distance = ((x - center) ** 2 + (y - center) ** 2) ** 0.5 / center
            if distance > 1.0:
                continue
            t = min(distance, 1.0)
            pixels[x, y] = tuple(int(round(a + (b - a) * t)) for a, b in zip(inner, outer))
    return disc


def make_pad(scale):
    """Vành ngoài + mặt trong, đúng kiểu nút tròn bóng của D-pad."""
    size = SIZE * scale
    pad = Image.new("RGBA", (size, size), (0, 0, 0, 0))

    rim = radial_disc(size, RIM_LIGHT, RIM_DARK)
    pad.alpha_composite(rim)

    inset = int(9 * scale)
    face = radial_disc(size - inset * 2, FACE_LIGHT, FACE_EDGE)
    pad.alpha_composite(face, (inset, inset))

    # Vệt sáng vòng cung phía trên: chỗ duy nhất khiến nút trông bóng chứ không bẹt.
    gloss = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    ImageDraw.Draw(gloss).ellipse(
        [int(24 * scale), int(14 * scale), int(104 * scale), int(56 * scale)],
        fill=(255, 255, 255, 120))
    pad.alpha_composite(gloss)
    return pad


def make_sword(scale):
    """Một thanh kiếm dựng đứng, mũi hướng lên; hàm gọi xoay nó để bắt chéo."""
    size = SIZE * scale
    sword = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(sword)

    def box(x0, y0, x1, y1):
        return [x0 * scale, y0 * scale, x1 * scale, y1 * scale]

    # Lưỡi: thân thẳng thóp dần thành mũi nhọn. Viền xanh thép vẽ trước, thân
    # sáng vẽ đè lên và hụt vào 1px mỗi bên để lộ viền.
    blade = [(64, 16), (55, 32), (55, 74), (73, 74), (73, 32)]
    draw.polygon([(x * scale, y * scale) for x, y in blade], fill=STEEL_LINE)
    inner = [(64, 21), (57.5, 34), (57.5, 71.5), (70.5, 71.5), (70.5, 34)]
    draw.polygon([(x * scale, y * scale) for x, y in inner], fill=BLADE_DARK)
    # Nửa trái sáng hơn: ánh kim đơn giản, một mặt hứng sáng một mặt tối.
    draw.polygon([(x * scale, y * scale) for x, y in
                  [(64, 21), (57.5, 34), (57.5, 71.5), (64, 71.5)]], fill=BLADE_LIGHT)

    draw.rounded_rectangle(box(44, 74, 84, 85), radius=4 * scale, fill=GUARD,
                           outline=STEEL_LINE, width=max(1, int(1.5 * scale)))
    draw.rounded_rectangle(box(58, 85, 70, 106), radius=3 * scale, fill=GRIP)
    draw.ellipse(box(56, 103, 72, 119), fill=GUARD, outline=STEEL_LINE,
                 width=max(1, int(1.5 * scale)))
    return sword


def make_icon(scale):
    """Hai kiếm bắt chéo trên mặt nút; xoay ±40° để chữ X cân, không lệch."""
    pad = make_pad(scale)
    sword = make_sword(scale)
    pad.alpha_composite(sword.rotate(40, resample=Image.BICUBIC))
    pad.alpha_composite(sword.rotate(-40, resample=Image.BICUBIC))
    return pad


def main():
    out = os.path.normpath(OUT_DIR)
    os.makedirs(out, exist_ok=True)
    make_icon(SUPERSAMPLE).resize((SIZE, SIZE), Image.LANCZOS) \
        .save(os.path.join(out, "attack-button.png"))
    print("Wrote attack button icon to " + out)


if __name__ == "__main__":
    main()
