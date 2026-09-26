/**
 * Bộ đọc/ghi JSON "lossless" tự viết (không phụ thuộc thư viện ngoài — nằm ngoài phạm vi file
 * sở hữu của phase 6 để thêm dependency vào package.json). Bộ phân tích (tokenizer) nằm ở
 * `lossless-json-parser.ts`, kiểu dữ liệu chung ở `lossless-json-types.ts` — tách file để mỗi
 * file dưới ~200 dòng theo quy ước dự án.
 *
 * Vì sao không dùng `JSON.parse`/`JSON.stringify` thường: cột `items`/`pets`/`petSelected`/
 * `PetDefLeague` do GServer ghi bằng Newtonsoft (`Adapter/JsonAdapter.cs`) có field `float`
 * (vd `gemOptionValue`) ghi dạng `5.0`. `JSON.parse` đổi `5.0` thành số JS `5`, và
 * `JSON.stringify` lại in ra `"5"` — mất định dạng gốc dù giá trị không đổi. Với field ta
 * KHÔNG sửa, invariant "giữ nguyên round-trip" (xem `item-json.ts`/`pet-json.ts`) bắt buộc
 * giữ đúng chuỗi số gốc. Vì vậy mọi số được giữ dưới dạng {@link RawNumber} (chuỗi gốc), chỉ
 * đổi sang số JS thường khi CHÍNH TA gán giá trị mới cho field đang sửa.
 *
 * Big-number safety: đã quét toàn bộ `gopettae_tae2.player` (items/pets/petSelected/
 * PetDefLeague) — không có chuỗi số nào ≥16 chữ số (ngưỡng an toàn của `Number`, 2^53 ≈
 * 9×10^15 ~ 16 chữ số). id (`itemId`/`petId`/`petEuipId`) là `int` C# (tối đa ~2.1 tỷ). Vì
 * vậy dùng `Number` (không cần `BigInt`) là an toàn cho các phép so sánh/sắp xếp; field
 * KHÔNG động tới vẫn giữ nguyên văn bản qua `RawNumber` nên dù có giá trị lớn hơn trong
 * tương lai cũng không bị sai lệch khi round-trip.
 */
import { LosslessJsonParser } from "./lossless-json-parser";
import { RawNumber, type JsonNode } from "./lossless-json-types";

export { RawNumber, type JsonNode };

/** Đọc 1 node số (RawNumber hoặc number thường ta vừa gán) ra `number` JS để so sánh/sắp xếp. */
export function toNumber(node: JsonNode | undefined): number {
  if (node === undefined || node === null) return NaN;
  if (typeof node === "number") return node;
  if (node instanceof RawNumber) return Number(node.raw);
  return NaN;
}

/** Parse JSON text -> cây node giữ nguyên định dạng số gốc (xem module doc). */
export function parseLossless(text: string): JsonNode {
  return new LosslessJsonParser(text).parse();
}

/**
 * Ghi cây node ra JSON text. Field không sửa (RawNumber, string/boolean/null nguyên trạng)
 * in lại đúng dạng; field ta gán `number`/`string`/`boolean` mới in theo `JSON.stringify`
 * chuẩn (đúng và hợp lệ JSON, không cần khớp byte với văn bản gốc vì đây là thay đổi cố ý).
 *
 * Lưu ý: object key kiểu chỉ-số-nguyên-không-âm (vd map `items` theo invType "0","1"...) sẽ
 * bị JS tự sắp lại theo thứ tự số tăng dần bất kể thứ tự chèn — vô hại vì đây là field kiểu
 * Dictionary/HashMap phía server (thứ tự key không mang ý nghĩa nghiệp vụ), và dữ liệu thật
 * quan sát được luôn đã ở dạng tăng dần sẵn.
 */
export function stringifyLossless(node: JsonNode): string {
  if (node === null) return "null";
  if (typeof node === "boolean") return node ? "true" : "false";
  if (node instanceof RawNumber) return node.raw;
  if (typeof node === "number") return String(node);
  if (typeof node === "string") return JSON.stringify(node);
  if (Array.isArray(node)) return `[${node.map(stringifyLossless).join(",")}]`;
  const keys = Object.keys(node);
  return `{${keys.map((k) => `${JSON.stringify(k)}:${stringifyLossless(node[k])}`).join(",")}}`;
}
