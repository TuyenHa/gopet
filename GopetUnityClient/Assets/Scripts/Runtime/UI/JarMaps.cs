using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nạp bố cục map của jar theo số hiệu, có cache — song sinh với
    /// <see cref="JarSkin"/> nhưng cho dữ liệu nhị phân thay vì sprite.
    ///
    /// <para><c>tools/unpack-jar-dat</c> đổi đuôi <c>.dat</c> thành <c>.bytes</c> khi
    /// copy: Unity chỉ sinh <see cref="TextAsset"/> cho vài đuôi định sẵn, và
    /// <c>.dat</c> KHÔNG nằm trong số đó — để nguyên thì <c>Resources.Load</c> trả
    /// <c>null</c> dù file nằm đúng chỗ.</para>
    /// </summary>
    public static class JarMaps
    {
        private const string MapRoot = "Jar/Maps/";

        private static readonly Dictionary<int, JarMapLayout> Cache = new Dictionary<int, JarMapLayout>();

        public static JarMapLayout Load(int mapId)
        {
            if (Cache.TryGetValue(mapId, out var cached)) return cached;

            var asset = Resources.Load<TextAsset>($"{MapRoot}{mapId}");
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy map {mapId} tại Resources/{MapRoot}{mapId}.bytes. " +
                    "Đã chạy `node tools/unpack-jar-dat/index.js` chưa?");
            }

            var layout = JarMapLayout.Parse(asset.bytes);
            Cache[mapId] = layout;
            return layout;
        }
    }
}
