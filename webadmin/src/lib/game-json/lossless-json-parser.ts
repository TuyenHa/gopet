/** Bộ phân tích JSON (recursive descent) dùng riêng bởi `lossless-json.ts` — tách file để mỗi
 * file dưới ~200 dòng. Xem doc bất biến/lý do "lossless" ở đầu `lossless-json.ts`. */
import { RawNumber, type JsonNode } from "./lossless-json-types";

const WHITESPACE = new Set([" ", "\t", "\n", "\r"]);
const DIGITS = new Set("0123456789");

export class LosslessJsonParser {
  private i = 0;
  constructor(private readonly text: string) {}

  parse(): JsonNode {
    this.skipWs();
    const value = this.parseValue();
    this.skipWs();
    if (this.i !== this.text.length) throw new Error(`JSON dư ký tự ở vị trí ${this.i}`);
    return value;
  }

  private skipWs() {
    while (this.i < this.text.length && WHITESPACE.has(this.text[this.i])) this.i++;
  }

  private parseValue(): JsonNode {
    const c = this.text[this.i];
    if (c === "{") return this.parseObject();
    if (c === "[") return this.parseArray();
    if (c === '"') return this.parseString();
    if (c === "t") return this.expectLiteral("true", true);
    if (c === "f") return this.expectLiteral("false", false);
    if (c === "n") return this.expectLiteral("null", null);
    if (c === "-" || DIGITS.has(c)) return this.parseNumber();
    throw new Error(`Ký tự JSON không hợp lệ ở vị trí ${this.i}: ${JSON.stringify(c)}`);
  }

  private expectLiteral<T>(literal: string, value: T): T {
    if (this.text.slice(this.i, this.i + literal.length) !== literal) {
      throw new Error(`Mong đợi "${literal}" ở vị trí ${this.i}`);
    }
    this.i += literal.length;
    return value;
  }

  private parseObject(): { [key: string]: JsonNode } {
    this.i++; // {
    // Object.create(null): key "__proto__" gán qua `obj[key] = ...` trên object literal thường
    // sẽ đổi prototype thay vì tạo thuộc tính riêng (rớt mất field, có thể gây ô nhiễm
    // prototype) — object không prototype không có setter đặc biệt này (Low finding).
    const obj: { [key: string]: JsonNode } = Object.create(null) as { [key: string]: JsonNode };
    this.skipWs();
    if (this.text[this.i] === "}") {
      this.i++;
      return obj;
    }
    for (;;) {
      this.skipWs();
      const key = this.parseString();
      this.skipWs();
      if (this.text[this.i] !== ":") throw new Error(`Mong đợi ":" ở vị trí ${this.i}`);
      this.i++;
      this.skipWs();
      obj[key] = this.parseValue();
      this.skipWs();
      const c = this.text[this.i];
      if (c === ",") {
        this.i++;
        continue;
      }
      if (c === "}") {
        this.i++;
        break;
      }
      throw new Error(`Mong đợi "," hoặc "}" ở vị trí ${this.i}`);
    }
    return obj;
  }

  private parseArray(): JsonNode[] {
    this.i++; // [
    const arr: JsonNode[] = [];
    this.skipWs();
    if (this.text[this.i] === "]") {
      this.i++;
      return arr;
    }
    for (;;) {
      this.skipWs();
      arr.push(this.parseValue());
      this.skipWs();
      const c = this.text[this.i];
      if (c === ",") {
        this.i++;
        continue;
      }
      if (c === "]") {
        this.i++;
        break;
      }
      throw new Error(`Mong đợi "," hoặc "]" ở vị trí ${this.i}`);
    }
    return arr;
  }

  private parseString(): string {
    if (this.text[this.i] !== '"') throw new Error(`Mong đợi chuỗi ở vị trí ${this.i}`);
    this.i++;
    let out = "";
    for (;;) {
      const c = this.text[this.i];
      if (c === undefined) throw new Error("Chuỗi JSON chưa đóng");
      if (c === '"') {
        this.i++;
        break;
      }
      if (c === "\\") {
        const esc = this.text[this.i + 1];
        switch (esc) {
          case '"':
            out += '"';
            break;
          case "\\":
            out += "\\";
            break;
          case "/":
            out += "/";
            break;
          case "b":
            out += "\b";
            break;
          case "f":
            out += "\f";
            break;
          case "n":
            out += "\n";
            break;
          case "r":
            out += "\r";
            break;
          case "t":
            out += "\t";
            break;
          case "u": {
            const hex = this.text.slice(this.i + 2, this.i + 6);
            out += String.fromCharCode(Number.parseInt(hex, 16));
            this.i += 4;
            break;
          }
          default:
            throw new Error(`Escape JSON không hợp lệ ở vị trí ${this.i}`);
        }
        this.i += 2;
      } else {
        out += c;
        this.i++;
      }
    }
    return out;
  }

  private parseNumber(): RawNumber {
    const start = this.i;
    if (this.text[this.i] === "-") this.i++;
    while (DIGITS.has(this.text[this.i])) this.i++;
    if (this.text[this.i] === ".") {
      this.i++;
      while (DIGITS.has(this.text[this.i])) this.i++;
    }
    if (this.text[this.i] === "e" || this.text[this.i] === "E") {
      this.i++;
      if (this.text[this.i] === "+" || this.text[this.i] === "-") this.i++;
      while (DIGITS.has(this.text[this.i])) this.i++;
    }
    return new RawNumber(this.text.slice(start, this.i));
  }
}
