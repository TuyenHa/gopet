/**
 * Bản port TEA sang JavaScript, dịch TRỰC TIẾP từ
 * SRCGOPETGOC/GServer/Server/IO/TEA.cs.
 *
 * Đây là bản triển khai ĐỘC LẬP với Assets/Scripts/Net/Tea.cs, cố ý như vậy.
 * Test round-trip (mã hoá rồi giải mã ra chuỗi gốc) không chứng minh gì về
 * tương thích với server — một bản port sai đối xứng vẫn round-trip tốt.
 * Đối chiếu với một bản port độc lập mới bắt được lỗi.
 *
 * JS int32: dùng `| 0` để ép, và `^`/`<<`/`>>>` vốn đã coerce về int32
 * nên phép cộng tràn được xử lý đúng như Java/C#.
 */

const DELTA = 1640531527;
const SUM_INITIAL = -957401312;

class TeaReference {
    /** @param {bigint} longKey */
    constructor(longKey) {
        const key = new Array(16);
        TeaReference.generateKey(longKey, key);

        this.S = new Array(4);
        let off = 0;
        for (let i = 0; i < 4; i++) {
            this.S[i] =
                (key[off] | (key[off + 1] << 8) | (key[off + 2] << 16) | (key[off + 3] << 24)) | 0;
            off += 4;
        }
    }

    /** 8 byte của long (big-endian), lặp 2 lần thành 16 byte. */
    static generateKey(value, array) {
        for (let i = 0; i < 8; i++) {
            const b = Number((value >> BigInt(56 - i * 8)) & 0xffn);
            array[i] = b;
            array[i + 8] = b;
        }
    }

    encrypt(clear) {
        const paddedSize = ((clear.length >> 3) + (clear.length % 8 === 0 ? 0 : 1)) << 1;
        const buffer = new Array(paddedSize + 1).fill(0);
        buffer[0] = clear.length;
        TeaReference.pack(clear, buffer, 1);
        this.brew(buffer);
        return TeaReference.unpack(buffer, 0, buffer.length << 2);
    }

    decrypt(crypt) {
        if (crypt.length % 4 !== 0 || (crypt.length >> 2) % 2 !== 1) return null;
        const buffer = new Array(crypt.length >> 2).fill(0);
        TeaReference.pack(crypt, buffer, 0);
        this.unbrew(buffer);
        if (buffer[0] < 0) return null;
        return TeaReference.unpack(buffer, 1, buffer[0]);
    }

    brew(buf) {
        if (buf.length % 2 !== 1) return;
        for (let i = 1; i < buf.length; i += 2) {
            let v0 = buf[i];
            let v1 = buf[i + 1];
            let sum = 0;
            for (let n = 0; n < 32; n++) {
                sum = (sum - DELTA) | 0;
                v0 = (v0 + ((((v1 << 4) + this.S[0]) ^ v1) + (sum ^ (v1 >>> 5)) + this.S[1])) | 0;
                v1 = (v1 + ((((v0 << 4) + this.S[2]) ^ v0) + (sum ^ (v0 >>> 5)) + this.S[3])) | 0;
            }
            buf[i] = v0;
            buf[i + 1] = v1;
        }
    }

    unbrew(buf) {
        if (buf.length % 2 !== 1) return;
        for (let i = 1; i < buf.length; i += 2) {
            let v0 = buf[i];
            let v1 = buf[i + 1];
            let sum = SUM_INITIAL;
            for (let n = 0; n < 32; n++) {
                v1 = (v1 - ((((v0 << 4) + this.S[2]) ^ v0) + (sum ^ (v0 >>> 5)) + this.S[3])) | 0;
                v0 = (v0 - ((((v1 << 4) + this.S[0]) ^ v1) + (sum ^ (v1 >>> 5)) + this.S[1])) | 0;
                sum = (sum + DELTA) | 0;
            }
            buf[i] = v0;
            buf[i + 1] = v1;
        }
    }

    static pack(src, dest, destOffset) {
        if (destOffset + (src.length >> 2) > dest.length) return;

        // C# ném IndexOutOfRangeException ở đây; JS thì lặng lẽ nới mảng.
        // Mô phỏng lại hành vi C# để bản reference trung thực — nếu không,
        // vector sinh ra sẽ mô tả một giao thức mà server không nói.
        if (destOffset >= dest.length) {
            throw new RangeError(
                `pack: destOffset ${destOffset} >= dest.length ${dest.length} — ` +
                `server sẽ ném IndexOutOfRangeException. Đầu vào không hợp lệ (payload rỗng?).`
            );
        }

        let shift = 24;
        let j = destOffset;
        dest[destOffset] = 0;
        for (let i = 0; i < src.length; i++) {
            dest[j] = (dest[j] | (src[i] << shift)) | 0;
            if (shift === 0) {
                shift = 24;
                j++;
                if (j < dest.length) dest[j] = 0;
            } else {
                shift -= 8;
            }
        }
    }

    static unpack(src, srcOffset, destLength) {
        if (destLength < 0) return null;
        if (destLength > (src.length - srcOffset) << 2) return null;

        const dest = new Array(destLength);
        let i = srcOffset;
        let count = 0;
        for (let j = 0; j < destLength; j++) {
            dest[j] = (src[i] >> (24 - (count << 3))) & 255;
            count++;
            if (count === 4) {
                count = 0;
                i++;
            }
        }
        return dest;
    }
}

module.exports = { TeaReference, DELTA, SUM_INITIAL };
