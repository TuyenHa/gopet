using System;
using System.Collections.Generic;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Đọc file JSON phẳng <c>{"5": "T.Khoản", ...}</c> sinh bởi
    /// <c>tools/extract-jar-strings</c>. Thuần C# — test được ngoài Editor.
    ///
    /// <para><b>Vì sao tự viết chứ không dùng thư viện JSON:</b> Unity không kèm
    /// <c>System.Text.Json</c> ở netstandard2.1 (chỉ có trong .NET hiện đại, không
    /// phải BCL mà Unity build), và dự án chưa có gói JSON nào khác. Cùng lý do với
    /// <c>JavaBinaryReader</c>: định dạng đủ hẹp và do chính ta sinh ra, viết tay
    /// rẻ hơn kéo thêm phụ thuộc.</para>
    ///
    /// <para>Không hỗ trợ JSON tổng quát — chỉ đúng hình dạng file này sinh ra: một
    /// object phẳng, khoá là số, giá trị là chuỗi. Gặp gì khác thì ném.</para>
    /// </summary>
    public static class JarStringTable
    {
        public static Dictionary<int, string> Parse(string json)
        {
            var result = new Dictionary<int, string>();
            var i = 0;

            SkipWhitespace(json, ref i);
            Expect(json, ref i, '{');
            SkipWhitespace(json, ref i);

            if (Current(json, i) == '}')
            {
                return result;
            }

            while (true)
            {
                SkipWhitespace(json, ref i);
                var key = ParseString(json, ref i);

                SkipWhitespace(json, ref i);
                Expect(json, ref i, ':');
                SkipWhitespace(json, ref i);

                var value = ParseString(json, ref i);
                result[int.Parse(key)] = value;

                SkipWhitespace(json, ref i);
                var separator = Advance(json, ref i);
                if (separator == '}') break;
                if (separator != ',')
                {
                    throw new FormatException($"JSON hỏng tại vị trí {i}: mong đợi ',' hoặc '}}', gặp '{separator}'.");
                }
            }

            return result;
        }

        private static string ParseString(string s, ref int i)
        {
            Expect(s, ref i, '"');
            var sb = new StringBuilder();

            while (true)
            {
                var c = Advance(s, ref i);
                if (c == '"') break;

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                var escape = Advance(s, ref i);
                switch (escape)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        var hex = s.Substring(i, 4);
                        i += 4;
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        break;
                    default:
                        throw new FormatException($"Escape JSON lạ tại vị trí {i}: \\{escape}");
                }
            }

            return sb.ToString();
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static char Current(string s, int i) => s[i];

        private static char Advance(string s, ref int i) => s[i++];

        private static void Expect(string s, ref int i, char expected)
        {
            var actual = Advance(s, ref i);
            if (actual != expected)
            {
                throw new FormatException($"JSON hỏng tại vị trí {i - 1}: mong đợi '{expected}', gặp '{actual}'.");
            }
        }
    }
}
