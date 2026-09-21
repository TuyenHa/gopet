using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Phần đo và dựng khung của <see cref="ChatBubble"/>: xuống dòng, đo bề rộng chữ thật
    /// rồi co khung ôm sát. Tách khỏi phần vòng đời/hiển thị để mỗi file giữ dưới 200 dòng.
    /// </summary>
    public sealed partial class ChatBubble
    {
        private const int MaxCharactersPerLine = 26;
        private const int MaxLines = 3;

        // Đệm quanh chữ, đơn vị local của bong bóng. Khung đo theo chữ THẬT nên chỗ trống
        // còn lại đúng bằng hai con số này — không còn phần dư của phép ước lượng.
        private const float PaddingX = 6f;
        private const float PaddingY = 5f;

        // Mốc ước lượng, CHỈ dùng cho frame đầu trước khi đo được chữ thật (xem LateUpdate).
        // Ước lượng rộng tay cho chắc; frame sau là khung co lại đúng cỡ chữ.
        private const float EstimatedCharWidth = 6f;
        private const float EstimatedLineHeight = 14f;
        private const float EstimatedBoxHeight = 18f;
        private const float MaxWidth = 164f;

        private void SetText(string text)
        {
            var formatted = Wrap(text ?? string.Empty, out var longestLine, out var lineCount);
            if (_mesh != null)
            {
                _mesh.text = formatted;
                _mesh.characterSize = BaseCharacterSize * _textScale;
            }

            // Khung tạm theo ước lượng để frame đầu không bị hụt; LateUpdate đo chữ thật
            // xong sẽ co lại đúng cỡ.
            ApplyBox(
                Mathf.Clamp(8f + longestLine * EstimatedCharWidth * _textScale, 26f, MaxWidth * _textScale),
                (EstimatedBoxHeight + (lineCount - 1) * EstimatedLineHeight) * _textScale);
            _needsFit = true;
        }

        /// <summary>
        /// Co khung về đúng bề ngang/bề dọc của chữ đã dựng xong.
        ///
        /// <para>Trước đây khung tính bằng ước lượng <c>số ký tự × 6</c>. Font TTF hẹp hơn mốc
        /// đó nhiều nên khung thừa khoảng 30% bề ngang — nhìn ra là bong bóng to quá khổ so với
        /// câu chữ bên trong. Giờ đo thẳng <c>bounds</c> của mesh chữ, nên chỗ trống đúng bằng
        /// <see cref="PaddingX"/>/<see cref="PaddingY"/> bất kể cỡ chữ hay font.</para>
        ///
        /// <para>Phải làm ở <c>LateUpdate</c>: <c>TextMesh</c> dựng mesh cuối frame, đọc
        /// <c>bounds</c> ngay trong <c>SetText</c> chỉ ra số của lần gán text TRƯỚC.</para>
        /// </summary>
        private void LateUpdate()
        {
            if (!_needsFit || _mesh == null) return;
            var textRenderer = _mesh.GetComponent<MeshRenderer>();
            if (textRenderer == null) { _needsFit = false; return; }

            // bounds là world-space; bong bóng bị thu nhỏ nên phải quy về đơn vị local.
            var lossy = Mathf.Abs(transform.lossyScale.x);
            var size = textRenderer.bounds.size;
            if (size.x <= 0f || lossy <= 0.0001f) return; // mesh chưa dựng xong, chờ frame sau

            _needsFit = false;
            ApplyBox(Mathf.Min(size.x / lossy + PaddingX * 2f, MaxWidth * _textScale),
                size.y / lossy + PaddingY * 2f);
        }

        private void ApplyBox(float boxWidth, float boxHeight)
        {
            var height = Mathf.RoundToInt(boxHeight);
            if (_panel != null) _panel.sprite = BubbleSprite(Mathf.RoundToInt(boxWidth), height);
            var clickArea = GetComponent<BoxCollider2D>();
            if (clickArea != null)
            {
                clickArea.size = new Vector2(boxWidth, height + TailHeight);
                clickArea.offset = new Vector2(0f, TailHeight * 0.5f);
            }
            if (_textTransform != null)
                _textTransform.localPosition = new Vector3(0f, TailHeight * 0.5f, 0f);
        }

        private static string Wrap(string text, out int longestLine, out int lineCount)
        {
            var words = text.Trim().Split(' ');
            var lines = new List<string>(MaxLines);
            var current = new StringBuilder();

            foreach (var rawWord in words)
            {
                var word = rawWord;
                while (word.Length > MaxCharactersPerLine)
                {
                    if (current.Length > 0)
                    {
                        lines.Add(current.ToString());
                        current.Length = 0;
                        if (lines.Count == MaxLines) break;
                    }
                    lines.Add(word.Substring(0, MaxCharactersPerLine));
                    word = word.Substring(MaxCharactersPerLine);
                    if (lines.Count == MaxLines) break;
                }
                if (lines.Count == MaxLines) break;
                if (word.Length == 0) continue;

                if (current.Length > 0 && current.Length + 1 + word.Length > MaxCharactersPerLine)
                {
                    lines.Add(current.ToString());
                    current.Length = 0;
                    if (lines.Count == MaxLines) break;
                }
                if (current.Length > 0) current.Append(' ');
                current.Append(word);
            }
            if (current.Length > 0 && lines.Count < MaxLines) lines.Add(current.ToString());
            if (lines.Count == 0) lines.Add(string.Empty);

            longestLine = 0;
            foreach (var line in lines) longestLine = Mathf.Max(longestLine, line.Length);
            lineCount = lines.Count;
            return string.Join("\n", lines);
        }

    }
}
