using System;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;
namespace Gopet.Runtime.UI
{
    public sealed class MinimapWidget : MonoBehaviour
    {
        private const float SizeFrac = 0.08f, MarginFrac = 0.025f;
        private const int MaxTextureSize = 512;
        private RawImage _mapImage;
        private RectTransform _viewport, _dot;
        private Text _fallback;
        private RenderTexture _baked;
        private PlayerAvatar _follow;
        private int _mapWidth, _mapHeight;
        public event Action Clicked;
        public static MinimapWidget Create(Transform parent, Font font)
        {
            var root = new GameObject("MinimapWidget", typeof(RectTransform), typeof(Image), typeof(Button)); root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(1f - MarginFrac - SizeFrac, 1f - MarginFrac - SizeFrac);
            rect.anchorMax = new Vector2(1f - MarginFrac, 1f - MarginFrac);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var view = root.AddComponent<MinimapWidget>();
            // Dùng chính nền chữ nhật làm viền xanh qua inset 3 px của ảnh map;
            // không tạo Border GameObject và không bo góc.
            var background = root.GetComponent<Image>();
            background.color = new Color(0.05f, 0.42f, 0.95f, 1f);
            var viewport = new GameObject("Map", typeof(RectTransform), typeof(RawImage));
            viewport.transform.SetParent(root.transform, false);
            view._viewport = (RectTransform)viewport.transform;
            UiBuilder.Stretch(view._viewport);
            view._viewport.offsetMin = new Vector2(3f, 3f); view._viewport.offsetMax = new Vector2(-3f, -3f);
            view._mapImage = viewport.GetComponent<RawImage>();
            view._mapImage.raycastTarget = false;
            var dot = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(view._viewport, false);
            view._dot = (RectTransform)dot.transform;
            view._dot.sizeDelta = new Vector2(10f, 10f);
            dot.SetActive(false);
            var dotImage = dot.GetComponent<Image>();
            dotImage.color = new Color(1f, 0.12f, 0.08f);
            dotImage.raycastTarget = false;
            RoundedUiSprite.Apply(dotImage);
            view._fallback = UiBuilder.MakeText(root.transform, font, "Fallback", 11, true);
            view._fallback.alignment = TextAnchor.MiddleCenter; view._fallback.text = "Bản đồ";
            root.GetComponent<Button>().onClick.AddListener(() => view.Clicked?.Invoke());
            return view;
        }
        public void SetMap(JarMapLayout map, int mapId)
        {
            ReleaseBake();
            _mapWidth = map?.WidthPixels ?? 0; _mapHeight = map?.HeightPixels ?? 0;
            _follow = null;
            if (map == null || _mapWidth <= 0 || _mapHeight <= 0)
            {
                ShowFallback(mapId);
                return;
            }
            try
            {
                Bake(map);
                _mapImage.texture = _baked; _mapImage.enabled = true; _fallback.enabled = false;
                FitViewport(_mapWidth, _mapHeight);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Gopet] Không bake được minimap {mapId}: {ex.Message}");
                ReleaseBake();
                ShowFallback(mapId);
            }
        }
        public void BindPlayer(PlayerAvatar avatar)
        {
            _follow = avatar;
            _dot.gameObject.SetActive(avatar != null && _mapWidth > 0 && _mapHeight > 0);
        }
        private void Update()
        {
            if (_follow == null || _mapWidth <= 0 || _mapHeight <= 0) return;
            var (x, y) = _follow.JarPosition;
            var anchor = new Vector2(Mathf.Clamp01((float)x / _mapWidth),
                1f - Mathf.Clamp01((float)y / _mapHeight));
            _dot.anchorMin = _dot.anchorMax = anchor;
            _dot.anchoredPosition = Vector2.zero;
        }
        private void Bake(JarMapLayout map)
        {
            var scale = Mathf.Min(1f, (float)MaxTextureSize / Mathf.Max(_mapWidth, _mapHeight));
            var width = Mathf.Max(1, Mathf.RoundToInt(_mapWidth * scale)); var height = Mathf.Max(1, Mathf.RoundToInt(_mapHeight * scale));
            _baked = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Bilinear }; _baked.Create();
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = _baked; GL.Clear(true, true, new Color(0.03f, 0.06f, 0.08f, 1f));
                GL.PushMatrix(); GL.LoadPixelMatrix(0f, width, height, 0f);
                foreach (var layer in map.Layers)
                    for (var row = 0; row < map.HeightTiles; row++)
                        for (var col = 0; col < map.WidthTiles; col++)
                            DrawTile(map, layer[row][col], col, row, scale);
            }
            finally
            {
                GL.PopMatrix(); RenderTexture.active = previous;
            }
        }
        private void DrawTile(JarMapLayout map, byte tile, int col, int row, float scale)
        {
            var sprite = TileAssetProvider.CellFromMap(map, JarMapLayout.StripOf(tile), JarMapLayout.CellOf(tile));
            if (sprite == null) return;
            var tr = sprite.textureRect;
            var uv = new Rect(tr.x / sprite.texture.width, tr.y / sprite.texture.height,
                tr.width / sprite.texture.width, tr.height / sprite.texture.height);
            Graphics.DrawTexture(new Rect(col * 24f * scale, row * 24f * scale,
                24f * scale, 24f * scale), sprite.texture, uv, 0, 0, 0, 0);
        }
        private void FitViewport(float width, float height)
        {
            var ratio = width / height;
            _viewport.offsetMin = new Vector2(3f, 3f); _viewport.offsetMax = new Vector2(-3f, -3f);
            if (ratio >= 1f)
            {
                var pad = (1f - 1f / ratio) * 0.5f;
                _viewport.anchorMin = new Vector2(0f, pad); _viewport.anchorMax = new Vector2(1f, 1f - pad);
            }
            else
            {
                var pad = (1f - ratio) * 0.5f;
                _viewport.anchorMin = new Vector2(pad, 0f); _viewport.anchorMax = new Vector2(1f - pad, 1f);
            }
        }
        private void ShowFallback(int mapId)
        {
            _mapImage.enabled = false; _fallback.enabled = true;
            _fallback.text = mapId > 0 ? $"Map {mapId}" : "Bản đồ";
        }
        private void ReleaseBake()
        {
            _mapImage.texture = null;
            if (_baked != null) { _baked.Release(); Destroy(_baked); }
            _baked = null;
        }
        private void OnDestroy() => ReleaseBake();
    }
}
