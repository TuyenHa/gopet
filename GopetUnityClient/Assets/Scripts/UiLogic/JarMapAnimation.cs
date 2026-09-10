using System;

namespace Gopet.UiLogic
{
    /// <summary>Metadata cắt atlas và chạy hoạt ảnh object map, port từ <c>dy.java</c>.</summary>
    public sealed class JarMapAnimation
    {
        public int DirectionCount;
        public Region[] Regions = Array.Empty<Region>();
        public Frame[] Frames = Array.Empty<Frame>();
        public Clip[] Clips = Array.Empty<Clip>();

        public sealed class Region
        {
            public int X, Y, Width, Height;
        }

        public sealed class Part
        {
            public int RegionIndex, X, Y, Transform;
        }

        public sealed class Frame
        {
            public Part[] Parts = Array.Empty<Part>();
        }

        public sealed class Clip
        {
            public int[] FrameIndices = Array.Empty<int>();
            public int[] DurationsMs = Array.Empty<int>();
        }

        public static JarMapAnimation Parse(byte[] data)
        {
            var r = new JarBigEndianReader(data);
            var result = new JarMapAnimation { DirectionCount = Count(r.SignedByte(), 32, "direction") };

            result.Regions = new Region[Count(r.SignedByte(), 255, "region")];
            for (var i = 0; i < result.Regions.Length; i++)
                result.Regions[i] = new Region
                {
                    X = r.Int(), Y = r.Int(), Width = r.Int(), Height = r.Int()
                };

            result.Frames = new Frame[Count(r.SignedByte(), 255, "frame")];
            for (var i = 0; i < result.Frames.Length; i++)
            {
                r.SignedByte(); // id; client JAR cũng lưu frame theo thứ tự đọc
                var parts = new Part[Count(r.SignedByte(), 255, "frame part")];
                for (var j = 0; j < parts.Length; j++)
                    parts[j] = new Part
                    {
                        RegionIndex = r.SignedByte(), X = r.Int(), Y = r.Int(),
                        Transform = r.SignedByte()
                    };

                // Offset riêng theo hướng. Map hiện vẽ direction 0, nhưng vẫn phải đọc
                // hết block để clip phía sau bắt đầu đúng byte.
                var directionOverrides = Count(r.SignedByte(), 32, "direction override");
                for (var j = 0; j < directionOverrides; j++)
                {
                    r.SignedByte();
                    r.Int();
                    r.Int();
                }
                result.Frames[i] = new Frame { Parts = parts };
            }

            result.Clips = new Clip[Count(r.SignedByte(), 255, "clip")];
            for (var i = 0; i < result.Clips.Length; i++)
            {
                r.SignedByte(); // id; giống dy.java, index mảng mới là id runtime
                var defaultDuration = r.Int();
                var count = Count(r.SignedByte(), 255, "clip frame");
                var clip = new Clip
                {
                    FrameIndices = new int[count],
                    DurationsMs = new int[count]
                };
                for (var j = 0; j < count; j++)
                {
                    clip.FrameIndices[j] = r.SignedByte();
                    var duration = r.Int();
                    clip.DurationsMs[j] = duration == -1 ? defaultDuration : duration;
                }
                result.Clips[i] = clip;
            }

            if (!r.Eof) throw new InvalidOperationException($"Metadata hoạt ảnh còn dư byte tại {r.Position}.");
            return result;
        }

        private static int Count(int value, int max, string label)
        {
            if (value < 0 || value > max)
                throw new InvalidOperationException($"Số lượng {label} không hợp lệ: {value}.");
            return value;
        }
    }
}
