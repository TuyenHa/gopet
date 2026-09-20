using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Mây trôi trên tranh bản đồ: vài cụm mây RỜI, mỗi cụm một tốc độ, cao độ và cỡ
    /// riêng rồi vòng lại khi ra khỏi mép — tự nhiên hơn một dải mây liền trôi đều.
    ///
    /// <para>Trôi theo THỜI GIAN (<c>Time.unscaledDeltaTime</c>) chứ không theo frame:
    /// cộng pixel mỗi frame thì máy 120fps mây bay nhanh gấp đôi máy 60fps. Dùng
    /// unscaled để mây vẫn trôi khi game tạm dừng <c>Time.timeScale</c>.</para>
    ///
    /// <para>Vị trí/cỡ giữ theo TỈ LỆ màn và quy ra pixel mỗi frame: lúc dựng, RectTransform
    /// chưa qua layout nên <c>rect.size</c> còn bằng 0 — chốt pixel ngay lúc đó thì mây
    /// đứng im ở góc.</para>
    ///
    /// <para>Mây nằm DƯỚI lớp pin (dựng trước <c>Nodes</c>) nên không che tên map, và
    /// <c>raycastTarget = false</c> nên không nuốt cú chạm.</para>
    /// </summary>
    public sealed partial class WorldMapView
    {
        private const string CloudResourceFormat = "Ui/WorldMap/cloud-{0}";
        private const int CloudSpriteCount = 3;
        /// <summary>Số cụm mây trên màn — đủ thấy chuyển động, không rối mắt khi đọc tên map.</summary>
        private const int CloudCount = 6;
        /// <summary>Bề ngang một cụm mây theo tỉ lệ bề ngang màn.</summary>
        private const float CloudMinWidthFrac = 0.22f, CloudMaxWidthFrac = 0.46f;
        /// <summary>Tốc độ trôi: tỉ lệ bề ngang màn mỗi giây.</summary>
        private const float CloudMinSpeed = 0.012f, CloudMaxSpeed = 0.045f;
        private const float CloudMinAlpha = 0.32f, CloudMaxAlpha = 0.7f;

        private readonly List<Cloud> _clouds = new List<Cloud>();
        private RectTransform _cloudLayer;

        private sealed class Cloud
        {
            public RectTransform Rect;
            public float NormX;       // tâm mây theo tỉ lệ bề ngang màn
            public float NormY;       // cao độ theo tỉ lệ chiều cao màn
            public float WidthFrac;
            public float Speed;
            public float AspectRatio; // cao / rộng của sprite
        }

        private void BuildClouds()
        {
            // Con của TRANH, dựng trước lớp pin: mây phải trôi trên tranh mà dưới tên
            // map. Treo vào root thì nó là em của Picture nên vẽ đè lên cả tên map.
            var go = new GameObject("Clouds", typeof(RectTransform), typeof(RectMask2D));
            go.transform.SetParent(_picture, false);
            _cloudLayer = (RectTransform)go.transform;
            UiBuilder.Stretch(_cloudLayer);

            for (var i = 0; i < CloudCount; i++) MakeCloud(i);
        }

        private void MakeCloud(int index)
        {
            var sprite = Resources.Load<Sprite>(
                string.Format(CloudResourceFormat, index % CloudSpriteCount + 1));
            if (sprite == null)
            {
                // Thiếu asset mây thì bỏ hiệu ứng, KHÔNG chặn cả màn bản đồ.
                if (index == 0)
                    Debug.LogWarning("[Gopet] Thiếu sprite mây tại Resources/Ui/WorldMap/cloud-N — bỏ hiệu ứng mây.");
                return;
            }

            var go = new GameObject($"Cloud{index}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_cloudLayer, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var widthFrac = Random.Range(CloudMinWidthFrac, CloudMaxWidthFrac);
            var nearness = Mathf.InverseLerp(CloudMinWidthFrac, CloudMaxWidthFrac, widthFrac);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            // Mây to = ở gần = mờ hơn một chút để không lấn tranh nền.
            image.color = new Color(1f, 1f, 1f, Mathf.Lerp(CloudMaxAlpha, CloudMinAlpha, nearness));

            _clouds.Add(new Cloud
            {
                Rect = rect,
                // Rải đều cao độ rồi xê dịch nhẹ: ngẫu nhiên thuần dễ dồn cục về một dải.
                NormY = Mathf.Clamp01((index + 0.5f) / CloudCount * 0.78f + 0.18f
                    + Random.Range(-0.04f, 0.04f)),
                NormX = Random.Range(-widthFrac, 1f + widthFrac),
                WidthFrac = widthFrac,
                Speed = Mathf.Lerp(CloudMinSpeed, CloudMaxSpeed, nearness),
                AspectRatio = sprite.rect.height / sprite.rect.width
            });
        }

        private void Update()
        {
            LayoutPicture();
            TryDeclutter();
            if (_clouds.Count == 0 || _cloudLayer == null) return;
            var size = _cloudLayer.rect.size;
            if (size.x <= 0f || size.y <= 0f) return;

            var delta = Time.unscaledDeltaTime;
            foreach (var cloud in _clouds)
            {
                if (cloud.Rect == null) continue;
                cloud.NormX += cloud.Speed * delta;
                // Ra hẳn khỏi mép phải thì vòng lại từ mép trái, giữ nguyên cao độ.
                if (cloud.NormX - cloud.WidthFrac * 0.5f > 1f)
                    cloud.NormX = -cloud.WidthFrac * 0.5f;

                var width = size.x * cloud.WidthFrac;
                cloud.Rect.sizeDelta = new Vector2(width, width * cloud.AspectRatio);
                cloud.Rect.anchoredPosition = new Vector2(cloud.NormX * size.x, cloud.NormY * size.y);
            }
        }
    }
}
