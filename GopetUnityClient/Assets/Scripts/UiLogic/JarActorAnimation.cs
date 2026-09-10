using System;

namespace Gopet.UiLogic
{
    /// <summary>Định dạng ActorFactory `a.java` dùng cho sáu skill 125..130.</summary>
    public sealed class JarActorAnimation
    {
        public Range[] Clips = Array.Empty<Range>();
        public Step[] Steps = Array.Empty<Step>();
        public Frame[] Frames = Array.Empty<Frame>();
        public Region[] Regions = Array.Empty<Region>();

        public sealed class Range { public int First, Last; }
        public sealed class Step { public int Frame, DurationTicks, DeltaX, DeltaY; }
        public sealed class Frame { public Part[] Parts = Array.Empty<Part>(); }
        public sealed class Part { public int Region, X, Y, Transform, Image; }
        public sealed class Region { public int X, Y, Width, Height; }

        public static JarActorAnimation Parse(byte[] data)
        {
            var r = new JarBigEndianReader(data);
            r.Short(); r.Utf(); // version + tên actor cũ
            var result = new JarActorAnimation();
            result.Clips = new Range[Count(r.SignedByte(), 64, "clip")];
            for (var i = 0; i < result.Clips.Length; i++)
                result.Clips[i] = new Range { First = r.Short(), Last = r.Short() };

            result.Steps = new Step[Count(r.Short(), 4096, "step")];
            for (var i = 0; i < result.Steps.Length; i++)
                result.Steps[i] = new Step
                {
                    Frame = r.Short(), DurationTicks = r.SignedByte(),
                    DeltaX = r.Short(), DeltaY = r.Short()
                };

            var partValueCount = Count(r.Short(), 32767, "giá trị part");
            var frameCount = Count(r.Short(), 4096, "frame");
            result.Frames = new Frame[frameCount];
            var partsRead = 0;
            for (var i = 0; i < frameCount; i++)
            {
                var parts = new Part[Count(r.Short(), 4096, "part")];
                for (var j = 0; j < parts.Length; j++)
                {
                    var region = r.Short(); var x = r.Short(); var y = r.Short(); var flags = r.SignedByte();
                    parts[j] = new Part
                    {
                        Region = region, X = x, Y = y,
                        Transform = (flags & 7) >> 1, Image = (flags & 248) >> 3
                    };
                    partsRead += 4;
                }
                result.Frames[i] = new Frame { Parts = parts };
            }
            if (partsRead != partValueCount)
                throw new InvalidOperationException($"ANU khai {partValueCount} giá trị part nhưng đọc {partsRead}.");

            var regionTotal = Count(r.Short(), 32767, "region");
            result.Regions = new Region[regionTotal];
            var imageCount = Count(r.SignedByte(), 32, "ảnh");
            var regionIndex = 0;
            for (var image = 0; image < imageCount; image++)
            {
                var count = Count(r.Short(), regionTotal, "region ảnh");
                for (var j = 0; j < count; j++) result.Regions[regionIndex++] = new Region
                {
                    X = r.Short(), Y = r.Short(), Width = r.Short(), Height = r.Short()
                };
            }
            if (regionIndex != regionTotal)
                throw new InvalidOperationException($"ANU khai {regionTotal} region nhưng đọc {regionIndex}.");

            SkipShapes(r, 4); SkipShapes(r, 2); SkipShapes(r, 2); SkipShapes(r, 4);
            var finalCount = Count(r.Short(), 32767, "dữ liệu cuối");
            for (var i = 0; i < finalCount; i++) r.Short();
            if (!r.Eof) throw new InvalidOperationException($"ANU còn dư byte tại {r.Position}.");
            Validate(result);
            return result;
        }

        private static void SkipShapes(JarBigEndianReader r, int shorts)
        {
            var count = Count(r.Short(), 32767, "shape");
            for (var i = 0; i < count; i++) { for (var j = 0; j < shorts; j++) r.Short(); r.Int(); }
        }

        private static void Validate(JarActorAnimation value)
        {
            foreach (var clip in value.Clips)
                if (clip.First < 0 || clip.Last < clip.First || clip.Last >= value.Steps.Length)
                    throw new InvalidOperationException("ANU có khoảng clip ngoài danh sách step.");
            foreach (var step in value.Steps)
                if (step.Frame < 0 || step.Frame >= value.Frames.Length)
                    throw new InvalidOperationException("ANU có step trỏ ra ngoài frame.");
            foreach (var frame in value.Frames) foreach (var part in frame.Parts)
                if (part.Region < 0 || part.Region >= value.Regions.Length)
                    throw new InvalidOperationException("ANU có part trỏ ra ngoài region.");
        }

        private static int Count(int value, int max, string label)
        {
            if (value < 0 || value > max) throw new InvalidOperationException($"Số {label} ANU sai: {value}.");
            return value;
        }
    }
}
