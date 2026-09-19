"""Ve ngon lua phang kieu vector hoat hinh de lam hieu ung roi xuong pet.

Ve bang code chu khong dung anh stock: ban anh nguoi dung dua la ban xem truoc —
khong co kenh alpha, watermark in de len than lua. Ve lai thi sach ban quyen, ra
duoc moi do phan giai, va doi dang lua chi la doi vai con so.

Cau truc: moi lop la vai "giot lua" — day tron, phinh o 1/4 duoi, vuot nhon len
ngon va quap sang mot ben. Lop ngoai toi mau va cao, vao trong sang va thap dan.
"""
import math
import numpy as np
from PIL import Image, ImageDraw

SS = 4                                   # sieu lay mau roi thu nho -> vien muot
W, H = 420, 440
BASE_Y = H - 28
FAT = 0.25                               # cao do phinh to nhat, tinh tu day

def drop(cx, h, w, sweep, n=96):
    """Mot giot lua: day tron, ngon nhon quap sang ben theo do cong `sweep`."""
    left, right = [], []
    for i in range(n + 1):
        t = i / n
        # Quap theo t^2: goc gan thang dung, cang len ngon cang nga — dac trung
        # cua lua hoat hinh. Nga tuyen tinh se thanh ngon giao xien.
        x = cx + sweep * h * t ** 2.0
        y = BASE_Y - h * t
        if t < FAT:                       # nua duoi: cung tron
            hw = w * math.sqrt(max(0.0, 1 - ((FAT - t) / FAT) ** 2))
        else:                             # nua tren: vuot nhon
            hw = w * (1 - (t - FAT) / (1 - FAT)) ** 0.70
        left.append((x - hw, y))
        right.append((x + hw, y))
    return left + right[::-1]

# mau -> [(lech tam, cao, rong, do quap)]
LAYERS = [
    ("#E4471B", [(0, 358, 104, -0.10), (-80, 204, 46, -0.36), (84, 218, 50, 0.33),
                 (-118, 126, 30, -0.52), (120, 138, 32, 0.48)]),
    ("#F4701E", [(-6, 282, 77, -0.08), (-58, 150, 33, -0.32), (62, 162, 35, 0.29)]),
    ("#FB9A25", [(-4, 212, 55, -0.06), (44, 112, 25, 0.26)]),
    ("#FDC43C", [(-2, 150, 39, -0.04)]),
    ("#FDE873", [(1, 92, 24, 0.02)]),
]

img = Image.new("RGBA", (W * SS, H * SS), (0, 0, 0, 0))
dr = ImageDraw.Draw(img)
for color, drops in LAYERS:
    for cx, h, w, sweep in drops:
        dr.polygon([(x * SS, y * SS) for x, y in drop(W / 2 + cx, h, w, sweep)], fill=color)

img = img.resize((W, H), Image.LANCZOS)

# Cat sat noi dung: sprite thua vien trong suot thi luc dat trong game phai bu tru
# bang tay, rat de lech. Cat roi thi day anh = chan ngon lua, tam ngang = tam lua.
a = np.asarray(img)[:, :, 3]
ys, xs = np.where(a > 4)
M = 3
box = (max(0, xs.min() - M), max(0, ys.min() - M),
       min(W, xs.max() + 1 + M), min(H, ys.max() + 1 + M))
img = img.crop(box)
img.save("flame.png")
print("da ve va cat ->", img.size)
