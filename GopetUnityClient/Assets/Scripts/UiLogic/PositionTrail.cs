using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Records a moving owner's route and returns a point a fixed travelled distance behind it.
    /// Unlike a simple positional offset, the returned point follows corners and reversals.
    /// </summary>
    public sealed class PositionTrail
    {
        private const int MaxPoints = 128;
        private readonly List<(float x, float y)> _points = new List<(float, float)>();

        public float TotalLength { get; private set; }

        public void Reset(float x, float y)
        {
            _points.Clear();
            _points.Add((x, y));
            TotalLength = 0f;
        }

        /// <summary>Adds a route sample when it is far enough from the previous sample.</summary>
        public bool Add(float x, float y, float minimumSpacing = 1f)
        {
            if (_points.Count == 0)
            {
                Reset(x, y);
                return true;
            }

            var last = _points[_points.Count - 1];
            var dx = x - last.x;
            var dy = y - last.y;
            var segment = (float)Math.Sqrt(dx * dx + dy * dy);
            if (segment < Math.Max(0.01f, minimumSpacing)) return false;

            _points.Add((x, y));
            TotalLength += segment;
            while (_points.Count > MaxPoints)
            {
                var first = _points[0];
                var second = _points[1];
                var sx = second.x - first.x;
                var sy = second.y - first.y;
                TotalLength -= (float)Math.Sqrt(sx * sx + sy * sy);
                _points.RemoveAt(0);
            }
            return true;
        }

        /// <summary>Returns the interpolated point <paramref name="distance"/> along the old route.</summary>
        public (float x, float y) PointBehind(float distance)
        {
            if (_points.Count == 0) return (0f, 0f);
            if (_points.Count == 1 || distance <= 0f) return _points[_points.Count - 1];

            var remaining = distance;
            for (var i = _points.Count - 1; i > 0; i--)
            {
                var newer = _points[i];
                var older = _points[i - 1];
                var dx = older.x - newer.x;
                var dy = older.y - newer.y;
                var segment = (float)Math.Sqrt(dx * dx + dy * dy);
                if (segment <= 0f) continue;
                if (remaining <= segment)
                {
                    var t = remaining / segment;
                    return (newer.x + dx * t, newer.y + dy * t);
                }
                remaining -= segment;
            }
            return _points[0];
        }
    }
}
