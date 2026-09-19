"""Dung sprite cot lua cho ky nang 106, bam theo so do do tu anh mau.

So do (quy ve don vi canvas 960x540, lay bang cach quet pixel anh mau):
  than cot  : R bao hoa den ~22 don vi o dinh, ~30 sat diem va cham
  mat cat   : tam R254 G244 B174 -> +12 G/R=0.50 -> +30 G/R=0.31 (dinh BANG, khong phai gaussian)
  quat tia  : toa ~ +-60 so voi phuong thang dung, vuon toi ~72 don vi moi ben
  va cham   : cao hon chan pet ~131 don vi = ~1.0 lan chieu cao pet trong game
"""
import numpy as np
from PIL import Image

W, H = 560, 1628
CX = (W - 1) / 2.0
IMPACT_FRAC = 0.96
IMPACT_Y = IMPACT_FRAC * (H - 1)
PX = W / 145.0                     # 1 don vi canvas = bao nhieu pixel sprite

rgb = np.zeros((H, W, 3), np.float32)
ys = np.arange(H, dtype=np.float32)[:, None]
xs = np.arange(W, dtype=np.float32)[None, :]

# ------------------------------------------------------------------ than cot
t = np.clip(ys / IMPACT_Y, 0, 1)
hw = (24.0 + 9.0 * t ** 1.6) * PX
sd = (xs - CX) / hw                # co dau, de dat soc lech tam
d = np.abs(sd)

# Mu 3.4 cho DINH BANG: anh mau giu R=254 deu ra tan mep roi moi do dot ngot.
# Gaussian (mu 2) tat ngay tu giua nen cot bi mong va do quach.
I = 1.30 * np.exp(-(d ** 3.4))

# Soc sang doc, lay tu cac buou G do duoc o anh mau (+30/+40/+60 don vi).
stripe = np.zeros_like(I)
for off, sig, amp in ((0.30, 0.10, 0.30), (-0.27, 0.09, 0.25), (0.56, 0.11, 0.22),
                      (-0.58, 0.10, 0.20), (0.12, 0.07, 0.18), (-0.14, 0.07, 0.15)):
    stripe += amp * np.exp(-((sd - off) / sig) ** 2)

gf = 0.24 + 0.72 * np.exp(-(d / 0.40) ** 2) + stripe * 0.55
bf = 0.70 * np.exp(-(d / 0.20) ** 2) + stripe * 0.10
I = I * (1.0 + stripe * 0.30)

beam = np.stack([np.ones_like(I), np.clip(gf, 0, 1.2), np.clip(bf, 0, 1.2)], axis=2)
beam *= I[:, :, None]
beam *= np.clip((IMPACT_Y - ys) / (0.015 * H), 0, 1)[:, :, None]   # dung o diem va cham
beam *= np.clip(ys / (0.05 * H), 0, 1)[:, :, None]                 # vut nhe dinh cot
rgb += beam

# -------------------------------------------------------------- diem va cham
# Bat doi xung tren/duoi: de doi xung thi quang sang trum qua day sprite roi bi
# cat phang thanh hinh qua trung.
sy_b = np.where(ys < IMPACT_Y, 58.0, 16.0)
sy_h = np.where(ys < IMPACT_Y, 96.0, 26.0)
blob = np.exp(-((xs - CX) ** 2 / (2 * 30.0 ** 2) + (ys - IMPACT_Y) ** 2 / (2 * sy_b ** 2)))
rgb += 2.30 * blob[:, :, None] * np.array((1.0, 1.0, 0.94), np.float32)
halo = np.exp(-((xs - CX) ** 2 / (2 * 62.0 ** 2) + (ys - IMPACT_Y) ** 2 / (2 * sy_h ** 2)))
rgb += 0.95 * halo[:, :, None] * np.array((1.0, 0.66, 0.16), np.float32)

# --------------------------------------------------------------------- tia
def splat(px, py, amp, rad, color):
    x0, x1 = max(0, int(px - rad * 3)), min(W, int(px + rad * 3) + 1)
    y0, y1 = max(0, int(py - rad * 3)), min(H, int(py + rad * 3) + 1)
    if x0 >= x1 or y0 >= y1:
        return
    gx = np.arange(x0, x1, dtype=np.float32)[None, :] - px
    gy = np.arange(y0, y1, dtype=np.float32)[:, None] - py
    g = np.exp(-((gx ** 2 + gy ** 2) / (2 * rad ** 2)))[:, :, None]
    rgb[y0:y1, x0:x1] += amp * g * np.array(color, np.float32)

rng = np.random.default_rng(11)
MAX_R = 0.48 * W                   # ~70 don vi canvas, khop quat tia anh mau

# Tia THANG nhu nhung ngon giao, toa trong khoang +-60 — khong phai hinh mat troi
# toa deu 180 do. To dan va nguoi dan ve phia ngon.
for side in (-1, 1):
    for k in range(7):
        ang = np.radians(16 + k * 7.4 + rng.uniform(-1.8, 1.8))
        L = MAX_R * rng.uniform(0.76, 1.0)
        dx, dy = side * np.sin(ang), -np.cos(ang)
        for u in np.linspace(0.04, 1.0, 140):
            px, py = CX + dx * L * u, IMPACT_Y + dy * L * u
            fade = min(1.0, u / 0.10) * min(1.0, (1.0 - u) / 0.28) ** 0.85
            wide = 2.2 + 5.2 * u
            splat(px, py, 0.80 * fade, wide, (1.0, 0.62 - 0.26 * u, 0.12 - 0.10 * u))
            splat(px, py, 1.15 * fade, wide * 0.38, (1.0, 0.94 - 0.20 * u, 0.60 - 0.46 * u))

# ---------------------------------------------------------------- xuat RGBA
# Lua la anh sang: mau giu nguyen do tuoi, cuong do don het vao alpha. Neu de cuong
# do lam toi mau thi vung mo ra vet do duc chu khong ra quang sang.
peak = rgb.max(axis=2)
out = np.clip(rgb / np.maximum(peak[:, :, None], 1e-6), 0, 1)
alpha = np.clip(peak, 0, 1) ** 1.05
alpha[alpha < 0.016] = 0.0         # cat lop suong mo phu kin khung hinh

img = np.zeros((H, W, 4), np.uint8)
img[:, :, :3] = (out * 255).round()
img[:, :, 3] = (alpha * 255).round()
Image.fromarray(img, "RGBA").save(
    r"C:\Users\hatuy\AppData\Local\Temp\claude\D--game\cab9498c-ff63-4684-a853-53aaac7f697b\scratchpad\fire-col.png")
print("da ghi", W, "x", H, "impact_frac", IMPACT_FRAC,
      "alpha>0:", int((img[:, :, 3] > 0).sum()), "/", W * H)
