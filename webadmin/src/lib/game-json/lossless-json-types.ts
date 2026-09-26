/** Kiểu dữ liệu dùng chung giữa `lossless-json.ts` và `lossless-json-parser.ts`. */

/** Bọc 1 số JSON, giữ nguyên chuỗi gốc để round-trip không đổi định dạng (`5.0` ≠ `5`). */
export class RawNumber {
  constructor(readonly raw: string) {}
  valueOf(): number {
    return Number(this.raw);
  }
  toString(): string {
    return this.raw;
  }
}

export type JsonNode =
  | null
  | boolean
  | number
  | string
  | RawNumber
  | JsonNode[]
  | { [key: string]: JsonNode };
