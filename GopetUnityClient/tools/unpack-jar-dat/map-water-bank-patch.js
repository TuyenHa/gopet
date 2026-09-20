'use strict';

/**
 * Mở BỜ NƯỚC của 3 map băng: Băng động 1 (23), Sông băng (24), Băng động 2 (25).
 *
 * VẤN ĐỀ: lớp va chạm gốc viền quanh mọi vũng nước bằng một đường ô chặn một phần
 * (mặt nạ 1..14), nên nhân vật đứng trên bờ không bước xuống mặt băng được. Ba map
 * này vì thế vỡ thành nhiều vùng rời rạc — vùng đi được lớn nhất chỉ chiếm 48% / 80%
 * / 75% diện tích, trong khi 21 map còn lại đều 93–100%. Riêng map 23 bị cắt hẳn làm
 * đôi: hai nửa chỉ chạm nhau đúng MỘT pixel chéo ở ô (13,12), và 6/11 điểm sinh quái
 * nằm ở nửa người chơi không tới được.
 *
 * CÁCH SỬA: xoá mặt nạ (đặt 0) của mọi ô chặn-một-phần nằm sát ô nước sâu. Sau khi mở:
 * vùng lớn nhất 98% / 92% / 89%, và 11/11 quái của map 23 đều tới được. KHÔNG đụng vào
 * ô chặn toàn phần (mặt nạ 15) nên vách đá, thác và tường vẫn chặn y như cũ.
 *
 * DANH SÁCH Ô sinh bằng cách: dựng lại lớp nền của map, tính màu trung bình từng ô,
 * coi ô có (B − R) > 120 là nước sâu (nước ~ (79,160,242), băng ~ (130,191,217)),
 * rồi lấy mọi ô mặt nạ 1..14 kề (4 hướng, kể cả chính nó) một ô nước. Muốn tính lại
 * thì lặp đúng quy tắc đó — đừng sửa tay từng số.
 *
 * ĐÂY LÀ THAY ĐỔI CÓ CHỦ Ý so với jar gốc. Vá ngay trong pipeline chứ không sửa tay
 * file .bytes: nhờ vậy `--check` vẫn so được với nguồn, và chạy lại unpack không làm
 * mất bản vá.
 */

/** Ô cần mở, theo từng map: rows[hàng] = [các cột]. Toạ độ ô, không phải pixel. */
const PATCHES = {
    '23.dat': {
        width: 32, height: 24,
        rows: {
            2: [17, 18, 19, 20, 21, 22, 23, 24, 25],
            4: [17, 18, 19, 23, 24, 25],
            5: [19, 23, 25],
            6: [19, 23, 25],
            7: [19, 23, 25],
            8: [19, 20, 22, 23, 25],
            9: [19, 20, 22, 23, 24, 25],
            10: [19, 23],
            11: [14, 15, 16, 17, 19],
            12: [13, 14, 19],
            13: [13, 19],
            14: [13, 18, 19],
            15: [0, 13, 14, 15, 16, 17, 18],
            16: [1, 16, 17],
            17: [1],
            18: [1],
            19: [1],
            20: [1, 13, 14, 15, 16],
            21: [1, 2, 3, 12, 13, 17],
            22: [4, 12, 16, 17],
            23: [11, 12, 16],
        },
    },
    '24.dat': {
        width: 28, height: 24,
        rows: {
            0: [1],
            1: [1],
            2: [1],
            3: [1],
            4: [1],
            5: [1],
            6: [0],
            9: [12, 13, 14, 15],
            10: [11, 12, 15, 16],
            11: [11, 12, 14, 15, 16],
            12: [10, 11, 12, 14, 15],
            13: [11, 12, 15, 16],
            14: [0, 11],
            15: [1, 11, 12, 13, 15, 16],
            16: [1, 12, 13, 15],
            17: [1, 12, 13, 14, 15],
            18: [1],
            19: [1, 12, 13, 14, 15],
            20: [0, 1, 12, 15, 16, 27],
            21: [0, 11, 12, 15, 20, 21, 22, 23, 26, 27],
            22: [0, 11, 12, 15, 16, 17, 18, 19, 20, 23, 24, 25, 26],
            23: [0, 1, 11, 12],
        },
    },
    '25.dat': {
        width: 30, height: 24,
        rows: {
            8: [15],
            9: [14, 15, 16],
            10: [13, 14, 16],
            11: [13, 16],
            12: [13, 16],
            13: [13, 16],
            14: [13, 16],
            15: [7, 8, 12, 13, 16, 17, 18, 19, 20, 21, 22, 23],
            16: [6, 9, 12, 13, 23, 24, 28, 29],
            17: [6, 7, 8, 9, 12, 13, 16, 17, 19, 20, 24, 27],
            18: [12, 13, 16, 17, 18, 19, 20, 21, 22, 23, 24, 27, 28],
            19: [11, 12, 13, 14, 15, 16, 27, 28],
            20: [27],
            21: [11, 12, 13, 14, 15, 16, 17, 18, 27],
            22: [10, 14, 15, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27],
            23: [10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29],
        },
    },
};

/**
 * Vị trí byte đầu tiên của lớp va chạm trong file map (xem ef.java:104-190):
 * 2 byte đếm tài nguyên → 3 byte/tài nguyên → W, H, số lớp → các lớp nền.
 */
function collisionOffset(buffer) {
    const total = buffer[0] + buffer[1];
    let at = 2 + total * 3;
    const width = buffer[at];
    const height = buffer[at + 1];
    const layers = buffer[at + 2];
    at += 3 + layers * height * width;
    return { at, width, height };
}

/**
 * Trả về buffer đã mở bờ nước, hoặc chính `buffer` nếu map không nằm trong danh sách.
 * Ném lỗi nếu dữ liệu nguồn đã đổi (kích thước lệch, hoặc ô cần mở vốn đã đi được) —
 * im lặng bỏ qua sẽ để bản vá mục nát mà không ai biết.
 */
function patchWaterBanks(mapFileName, buffer) {
    const patch = PATCHES[mapFileName];
    if (!patch) return buffer;

    const { at, width, height } = collisionOffset(buffer);
    if (width !== patch.width || height !== patch.height) {
        throw new Error(`${mapFileName}: map nguon ${width}x${height} khac ${patch.width}x${patch.height} - tinh lai danh sach o`);
    }

    const out = Buffer.from(buffer);
    for (const [row, columns] of Object.entries(patch.rows)) {
        for (const column of columns) {
            const index = at + Number(row) * width + column;
            const mask = out[index];
            if (mask < 1 || mask > 14) {
                throw new Error(`${mapFileName}: o (${column},${row}) co mat na ${mask}, khong phai o chan mot phan - tinh lai danh sach o`);
            }
            out[index] = 0;
        }
    }
    return out;
}

module.exports = { patchWaterBanks };
