using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Đọc file mô tả khung hình hiệu ứng của jar (lớp <c>dy</c> trong bản dịch ngược) — file
    /// KHÔNG ĐUÔI nằm cạnh file <c>.png</c> cùng tên, ví dụ <c>pet/petInteract/kiss</c>.
    ///
    /// <para>Định dạng, big-endian, mọi số đếm là 1 byte có dấu:</para>
    /// <code>
    /// byte  partCount                      // bảng ghi đè theo "part", hiệu ứng pet không dùng (=0)
    /// byte  regionCount
    ///   regionCount × { int x, y, w, h }   // ô cắt trên ảnh .png, gốc toạ độ GÓC TRÁI-TRÊN
    /// byte  frameCount
    ///   frameCount × {
    ///     byte  (id, không dùng)
    ///     byte  pieceCount
    ///     pieceCount × { byte regionIndex, int offsetX, int offsetY, byte transform }
    ///     byte  overrideCount
    ///     overrideCount × { byte partIndex, int x, int y }
    ///   }
    /// byte  animCount
    ///   animCount × {
    ///     byte  (id, không dùng)
    ///     int   defaultDurationMs
    ///     byte  stepCount
    ///     stepCount × { byte frameIndex, int durationMs }   // -1 = lấy defaultDurationMs
    ///   }
    /// </code>
    ///
    /// <para>Chỉ lấy animation ĐẦU TIÊN: cả ba file hiệu ứng pet đều đúng một animation, và
    /// jar cũng chỉ dựng <c>dz</c> từ <c>a[0]</c>.</para>
    ///
    /// <para>Thuần C#, không đụng UnityEngine — để test được ngoài Editor. Khác hẳn
    /// <see cref="JarActorAnimation"/>: đó là định dạng <c>.anu</c> của ActorFactory, có
    /// header version + tên và mọi số đếm là short.</para>
    /// </summary>
    public sealed class JarEffectAnimation
    {
        /// <summary>Một ô cắt trên ảnh nguồn. <paramref name="Y"/> đo từ MÉP TRÊN.</summary>
        public readonly struct Region
        {
            public readonly int X, Y, Width, Height;
            public Region(int x, int y, int w, int h) { X = x; Y = y; Width = w; Height = h; }
        }

        /// <summary>Một mảnh ghép trong khung: dán <see cref="RegionIndex"/> lệch đi (dx, dy).</summary>
        public readonly struct Piece
        {
            public readonly int RegionIndex, OffsetX, OffsetY;
            public Piece(int region, int dx, int dy) { RegionIndex = region; OffsetX = dx; OffsetY = dy; }
        }

        /// <summary>Một bước của animation: chiếu khung nào, trong bao lâu.</summary>
        public readonly struct Step
        {
            public readonly int FrameIndex, DurationMs;
            public Step(int frame, int ms) { FrameIndex = frame; DurationMs = ms; }
        }

        public Region[] Regions { get; private set; }
        public Piece[][] Frames { get; private set; }
        public Step[] Steps { get; private set; }

        /// <summary>Số mảnh nhiều nhất của một khung — người vẽ cần biết để dựng sẵn renderer.</summary>
        public int MaxPiecesPerFrame { get; private set; }

        public static JarEffectAnimation Parse(byte[] data)
        {
            if (data == null || data.Length < 3) throw new ArgumentException("File hiệu ứng rỗng hoặc quá ngắn.");
            var r = new JarBigEndianReader(data);
            var result = new JarEffectAnimation();

            r.SignedByte(); // partCount — hiệu ứng pet luôn 0, bảng ghi đè bên dưới rỗng theo.

            var regionCount = r.SignedByte();
            result.Regions = new Region[regionCount];
            for (var i = 0; i < regionCount; i++)
                result.Regions[i] = new Region(r.Int(), r.Int(), r.Int(), r.Int());

            var frameCount = r.SignedByte();
            result.Frames = new Piece[frameCount][];
            for (var i = 0; i < frameCount; i++)
            {
                r.SignedByte();
                var pieceCount = r.SignedByte();
                var pieces = new Piece[pieceCount];
                for (var p = 0; p < pieceCount; p++)
                {
                    var region = r.SignedByte();
                    var dx = r.Int();
                    var dy = r.Int();
                    r.SignedByte(); // transform (lật/xoay) — cả ba file hiệu ứng pet đều là 0
                    pieces[p] = new Piece(region, dx, dy);
                }
                result.Frames[i] = pieces;
                if (pieceCount > result.MaxPiecesPerFrame) result.MaxPiecesPerFrame = pieceCount;

                // Bảng ghi đè theo part: phải ĐỌC QUA cho đúng nhịp dù partCount = 0.
                var overrideCount = r.SignedByte();
                for (var o = 0; o < overrideCount; o++) { r.SignedByte(); r.Int(); r.Int(); }
            }
            var animCount = r.SignedByte();
            if (animCount <= 0) throw new ArgumentException("File hiệu ứng không có animation nào.");
            r.SignedByte();
            var defaultDuration = r.Int();
            var stepCount = r.SignedByte();
            result.Steps = new Step[stepCount];
            for (var s = 0; s < stepCount; s++)
            {
                var frame = r.SignedByte();
                var ms = r.Int();
                result.Steps[s] = new Step(frame, ms < 0 ? defaultDuration : ms);
            }
            if (!r.Eof) throw new ArgumentException($"File hiệu ứng còn dư byte tại {r.Position}.");
            return result;
        }

    }
}
