using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Nạp TestVectors.json — sinh bởi tools/gen-test-vectors từ một bản port
    /// TEA độc lập với Assets/Scripts/Net/Tea.cs.
    ///
    /// Đây là điểm mấu chốt: test round-trip chỉ chứng minh code tự nhất quán
    /// với chính nó. Đối chiếu với bản port độc lập mới chứng minh nó nói đúng
    /// giao thức của server.
    /// </summary>
    public static class TestVectorData
    {
        private static readonly Lazy<VectorFile> Lazy = new Lazy<VectorFile>(Load);

        public static VectorFile Vectors => Lazy.Value;

        private static VectorFile Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestVectors.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy {path}. Chạy: node tools/gen-test-vectors/index.js", path);
            }

            return JsonSerializer.Deserialize<VectorFile>(File.ReadAllText(path));
        }

        public static byte[] FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();

            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        public static string ToHex(byte[] bytes)
        {
            if (bytes == null) return "<null>";

            var chars = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                chars[i * 2] = "0123456789abcdef"[b >> 4];
                chars[i * 2 + 1] = "0123456789abcdef"[b & 0xF];
            }

            return new string(chars);
        }
    }

    public sealed class VectorFile
    {
        [JsonPropertyName("tea")] public List<TeaVector> Tea { get; set; }
        [JsonPropertyName("handshake")] public List<HandshakeVector> Handshake { get; set; }
        [JsonPropertyName("utf")] public List<UtfVector> Utf { get; set; }
        [JsonPropertyName("keyDerivation")] public List<KeyDerivationVector> KeyDerivation { get; set; }
    }

    public sealed class TeaVector
    {
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("payloadName")] public string PayloadName { get; set; }
        [JsonPropertyName("plainHex")] public string PlainHex { get; set; }
        [JsonPropertyName("encryptedHex")] public string EncryptedHex { get; set; }
    }

    public sealed class HandshakeVector
    {
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("bytesHex")] public string BytesHex { get; set; }
    }

    public sealed class UtfVector
    {
        [JsonPropertyName("value")] public string Value { get; set; }
        [JsonPropertyName("encodedHex")] public string EncodedHex { get; set; }
    }

    public sealed class KeyDerivationVector
    {
        [JsonPropertyName("originalKey")] public string OriginalKey { get; set; }
        [JsonPropertyName("handshakeHex")] public string HandshakeHex { get; set; }
        [JsonPropertyName("derivedKey")] public string DerivedKey { get; set; }
    }
}
