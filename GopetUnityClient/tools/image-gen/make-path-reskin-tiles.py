"""Sinh dải ô nền THAY MẶT TUYẾT của lối đi cho các map rừng.

    python image-gen/make-path-reskin-tiles.py

Ghi vào Assets/Resources/Jar/Art/Raw/newMapData/:

    12151 / 12161 / 12162.png  <- 151 / 161 / 162, mặt ĐẤT   (Linh Lâm 13, Đại Linh Cảnh 15)
    13151 / 13161 / 13162.png  <- 151 / 161 / 162, mặt ĐÁ     (hiện KHÔNG map nào dùng)

Cùng cách làm với make-mountain-grass-tiles.py: KHÔNG vẽ lại, chỉ thay màu trên dải
gốc nên hình dạng, mép nối và thứ tự ô giữ nguyên tuyệt đối — map cũ ghép vào vẫn
khít, không phải đụng vào file map nhị phân. Ba dải gốc chỉ có 16 màu mỗi dải nên
bảng tra tay là đủ.

Nguồn hai bảng màu:
  - đất: lấy mẫu từ ảnh tham chiếu người chơi gửi (nền đất cát khô của bản gốc mobile) —
         tông cát nhạt (190,174,114), KHÔNG phải vàng cam như bản trước
  - đá : đúng bảng 3.png -> 11003.png (lối đá Thành Phố Linh Thú) nên hai map cùng chất đá

Cỏ giữ nguyên — chỉ tuyết mới đổi. Màu xám (116,117,121) và màu nước
(144,242,255)/(75,193,209) của 162 cũng giữ nguyên: đó là ô xám và ô nước cuối dải,
không thuộc lối đi.
"""

import os

from PIL import Image

SRC_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "Assets", "Resources", "Jar", "Art", "Raw", "newMapData")

TILE = 24

# Ba dải mang mặt tuyết của lối đi: 151 là mặt lối, 161/162 là cỏ giáp lối.
SOURCE_IDS = (151, 161, 162)

# Tuyết -> đất. Sáng ứng sáng, tối ứng tối, giữ nguyên tương quan độ sáng để hoạ tiết
# lấm tấm của mặt tuyết trở thành hoạ tiết sỏi của mặt đất chứ không bẹt thành mảng phẳng.
#
# Bốn tông MẶT ĐƯỜNG bám sát ảnh tham chiếu: nền chính đúng (190,174,114), ba tông còn
# lại nằm trong dải hẹp quanh nó nên mặt đường phẳng và dịu như ảnh mẫu, không loang lổ.
# Bốn tông RÌA/VIỀN là dải chuyển 1-2 px giữa mặt đường và cỏ (xem bản đồ màu của
# 161.png): ở bản tuyết chúng là màu băng xanh nhạt nên đọc như mép mềm. Đổi thẳng sang
# nâu đậm thì lối đi bị bo một viền đen sì — ảnh mẫu không có viền đó. Nên giữ chúng
# trong họ đất, chỉ tối dần, thành vệt đổ bóng nhẹ ở mép đường.
SNOW_TO_DIRT = {
    (255, 255, 255): (206, 192, 136),   # trắng tinh
    (243, 247, 251): (197, 182, 122),   # sáng nhất của mặt tuyết
    (240, 245, 251): (190, 174, 114),   # mặt tuyết chính
    (213, 227, 243): (178, 162, 103),   # mặt tuyết đổ bóng
    (180, 216, 254): (181, 165, 107),   # rìa sáng
    (160, 191, 228): (169, 153, 99),    # rìa tối
    (56, 193, 234): (156, 140, 91),     # viền băng sáng
    (55, 153, 182): (138, 124, 81),     # viền băng tối
}

# Tuyết -> đá. Năm dòng có chú thích "(11003)" là ĐÚNG màu mà bản 11003.png đang dùng
# cho cùng màu tuyết đó, nên lối đá ở đây và lối đá Thành Phố Linh Thú cùng một chất.
# Hai màu còn lại không có trong 3.png nên nội suy theo độ sáng của dải đá đó.
SNOW_TO_STONE = {
    (255, 255, 255): (228, 230, 231),   # trắng tinh
    (243, 247, 251): (210, 213, 214),   # sáng nhất của mặt tuyết
    (240, 245, 251): (184, 188, 190),   # mặt tuyết chính          (11003)
    (213, 227, 243): (166, 171, 174),   # mặt tuyết đổ bóng
    (180, 216, 254): (228, 230, 231),   # rìa sáng                 (11003)
    (160, 191, 228): (127, 135, 140),   # rìa tối                  (11003)
    (56, 193, 234): (72, 80, 86),       # viền băng sáng           (11003)
    (55, 153, 182): (79, 87, 92),       # viền băng tối            (11003)
}

# Tiền tố id của từng bản: 11xxx đã là bản cỏ xanh (áo mùa hè) nên bản mới bắt đầu từ 12xxx.
VARIANTS = (
    ("dat", 12000, SNOW_TO_DIRT),
    ("da", 13000, SNOW_TO_STONE),
)


def repaint(image, palette):
    """Đổi mọi màu tuyết sang màu của bảng, giữ nguyên cỏ/xám/nước."""
    pixels = image.load()
    width, height = image.size
    changed = 0
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            target = palette.get((r, g, b))
            if target is None:
                continue
            pixels[x, y] = target + (a,)
            changed += 1
    return changed


def convert(source_id, target_id, palette):
    image = Image.open(os.path.join(SRC_DIR, f"{source_id}.png")).convert("RGBA")
    changed = repaint(image, palette)
    image.save(os.path.join(SRC_DIR, f"{target_id}.png"))
    return image.width // TILE, changed


def main():
    for name, prefix, palette in VARIANTS:
        for source_id in SOURCE_IDS:
            target_id = prefix + source_id
            cells, changed = convert(source_id, target_id, palette)
            print(f"Wrote {target_id}.png ({name}, {cells} cells, {changed} px doi mau)")


if __name__ == "__main__":
    main()
