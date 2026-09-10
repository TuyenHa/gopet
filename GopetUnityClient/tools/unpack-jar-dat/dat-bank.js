/**
 * Giải format kho ảnh `.dat` của client J2ME cũ. Nguồn: `gu.java` (86 dòng),
 * xem `plans/260905-1355-gopet-unity-client-rebuild/phase-05-1-jar-assets-and-login-skin.md`.
 *
 * Format:
 *   int        count               <- SỐ MỤC TRONG BẢNG OFFSET, không phải số ảnh
 *   int×count  offset               <- cộng thêm base = (count<<2)+4 để ra vị trí byte thật
 *   rồi:       các khối PNG nối đuôi nhau
 *
 * BẪY ĐÃ TRẢ GIÁ: `count` KHÔNG phải số ảnh. `gu.a(int)` đọc kích thước ảnh bằng
 * `a[i+1] - a[i]`, nên ảnh thứ `count-1` (ảnh cuối) sẽ cần `a[count]` — NGOÀI MẢNG.
 * Java nuốt exception đó và trả về mảng rỗng; nghĩa là bản thân client J2ME cũng
 * không đọc được ảnh cuối theo cách này. Suy ra: mục cuối trong bảng offset không
 * phải một ảnh, nó là SENTINEL đánh dấu hết dữ liệu — đã kiểm chứng bằng cách đọc
 * cả 5 file thật: `offset[count-1]` luôn đúng bằng độ dài file.
 *
 * Số ảnh THẬT của một bank = count - 1.
 */

'use strict';

const PNG_SIGNATURE = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);

/**
 * @param {Buffer} buffer nội dung file `.dat`
 * @returns {{ imageCount: number, images: Buffer[] }}
 * @throws khi header hỏng hoặc bất kỳ khối nào không phải PNG hợp lệ
 */
function decodeBank(buffer) {
    if (buffer.length < 4) {
        throw new Error(`File quá ngắn để đọc header (${buffer.length} byte).`);
    }

    const count = buffer.readInt32BE(0);
    const base = (count << 2) + 4;

    if (count < 2 || base > buffer.length) {
        throw new Error(`Header khai ${count} mục — ngoài khoảng hợp lệ cho file ${buffer.length} byte.`);
    }

    const offsets = new Array(count);
    for (let i = 0; i < count; i++) {
        offsets[i] = buffer.readInt32BE(4 + 4 * i) + base;
    }

    const sentinel = offsets[count - 1];
    if (sentinel !== buffer.length) {
        throw new Error(
            `Offset cuối (${sentinel}) không khớp độ dài file (${buffer.length}) — ` +
            'sentinel sai thì mọi phép trừ kích thước ảnh phía trước đều sai theo.'
        );
    }

    const imageCount = count - 1;
    const images = [];

    for (let i = 0; i < imageCount; i++) {
        const start = offsets[i];
        const end = offsets[i + 1];

        if (end <= start || start < base || end > buffer.length) {
            throw new Error(`Ảnh ${i}: khoảng byte [${start}:${end}] vô lý.`);
        }

        const chunk = buffer.subarray(start, end);
        if (!chunk.subarray(0, 8).equals(PNG_SIGNATURE)) {
            throw new Error(`Ảnh ${i}: không bắt đầu bằng chữ ký PNG tại byte ${start}.`);
        }

        images.push(Buffer.from(chunk));
    }

    return { imageCount, images };
}

module.exports = { decodeBank, PNG_SIGNATURE };
