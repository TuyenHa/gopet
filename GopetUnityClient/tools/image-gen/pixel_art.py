"""Hàm vẽ pixel dùng chung cho các generator trong image-gen/.

Ba generator của bộ nhiệt đới (cây dừa, ao sen, nhà lá) đều cần đúng bộ này: một
canvas RGBA với hàm đặt pixel có chặn biên, vẽ đoạn thẳng, tô đa giác lồi và bo viền.
Để mỗi script tự chép một bản thì sửa một chỗ phải nhớ sửa ba chỗ.

Module này KHÔNG phải thư viện chung của cả repo — chỉ là chỗ để chung của image-gen,
nên giữ đúng những gì đang có người dùng.
"""

from PIL import Image


def canvas(width, height):
    """Ảnh RGBA trong suốt + hàm đặt pixel. Toạ độ ngoài khung bị bỏ, không ném lỗi:
    mọi hình ở đây đều dựng bằng công thức nên tràn biên là chuyện thường."""
    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    pixels = image.load()

    def put(x, y, color):
        x, y = int(x), int(y)
        if 0 <= x < width and 0 <= y < height:
            pixels[x, y] = color + (255,)

    return image, put


def line(put, a, b, color):
    """Đoạn thẳng lấy mẫu dày gấp đôi số pixel — thưa hơn là đường bị đứt quãng."""
    steps = int(max(abs(b[0] - a[0]), abs(b[1] - a[1]))) * 2 + 1
    for i in range(steps + 1):
        t = i / steps
        put(round(a[0] + (b[0] - a[0]) * t), round(a[1] + (b[1] - a[1]) * t), color)


def fill_polygon(put, points, color):
    """Tô một đa giác LỒI bằng phép thử tích có hướng.

    Chấp nhận CẢ HAI chiều quay: các mặt suy ra từ một khối hộp có mặt thuận mặt
    nghịch, ép một chiều thì nửa số mặt rỗng không.
    """
    xs = [p[0] for p in points]
    ys = [p[1] for p in points]
    for y in range(int(min(ys)), int(max(ys)) + 1):
        for x in range(int(min(xs)), int(max(xs)) + 1):
            positive = negative = False
            for i, a in enumerate(points):
                b = points[(i + 1) % len(points)]
                cross = (b[0] - a[0]) * (y + 0.5 - a[1]) - (b[1] - a[1]) * (x + 0.5 - a[0])
                if cross > 0:
                    positive = True
                elif cross < 0:
                    negative = True
            if not (positive and negative):
                put(x, y, color)


def outline(image, line_color, fill_colors=None):
    """Viền 1 px quanh hình — art gốc của jar hình nào cũng có nét bao.

    <paramref name="fill_colors"/> None nghĩa là bo quanh toàn bộ phần đục; đưa vào một
    tập màu thì chỉ bo quanh cụm màu đó, dùng khi một ảnh có nhiều chất liệu cần nét
    bao khác nhau (thân cây nâu, tán lá xanh).
    """
    pixels = image.load()
    edge = []
    for y in range(image.height):
        for x in range(image.width):
            if pixels[x, y][3]:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if not (0 <= nx < image.width and 0 <= ny < image.height):
                    continue
                neighbour = pixels[nx, ny]
                if neighbour[3] and (fill_colors is None or neighbour[:3] in fill_colors):
                    edge.append((x, y))
                    break
    for x, y in edge:
        pixels[x, y] = line_color + (255,)


def save(image, out_dir, target_id):
    """Ghi ảnh ra newMapData/<id>.png và in lại cỡ + số pixel đục để soi nhanh."""
    path = f"{out_dir}/{target_id}.png"
    image.save(path)
    filled = sum(1 for pixel in image.getdata() if pixel[3])
    print(f"Wrote {target_id}.png ({image.width}x{image.height}, {filled} px)")
