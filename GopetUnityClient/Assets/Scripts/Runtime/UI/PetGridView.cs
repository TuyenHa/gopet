using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public enum PetGridMode
    {
        Receive,
        Shop,
        Top
    }

    /// <summary>Danh sách pet dạng thẻ ngang dùng cho tab Nhận pet và Shop pet.</summary>
    public sealed partial class PetGridView : MonoBehaviour
    {
        /// <summary>
        /// Không hở giữa các dòng: dòng phân tách nhau bằng một vạch ngang ở chân,
        /// giống danh sách cửa hàng. Trước đây mỗi dòng là một thẻ có viền riêng, và
        /// viền thẻ đè lên viền khung chứa thành hai nét chồng nhau.
        /// </summary>
        private const float Gap = 0f;
        private const float CardHeight = 46f;
        private const float IconSize = 31f;
        private const float PortraitWidth = 50f;
        private const float InfoWidth = 90f;
        private const float TopInfoWidth = 200f;
        private const float StatWidth = 68f;
        private const float ActionWidth = 60f;

        private static readonly Color Blue = new Color(0.035f, 0.48f, 0.96f, 1f);
        private static readonly Color MainText = new Color(0.13f, 0.16f, 0.21f, 1f);
        private static readonly Color MutedText = new Color(0.32f, 0.35f, 0.4f, 1f);

        private readonly Dictionary<int, Card> _realized = new Dictionary<int, Card>();
        private readonly Stack<Card> _pool = new Stack<Card>();
        private readonly List<int> _recycle = new List<int>();
        private Font _font;
        private RemoteAssetCache _assets;
        private GuiderHandler _guider;
        private MenuScreen _screen;
        private RectTransform _content;
        private ScrollRect _scrollRect;
        private string _buttonLabel = "Nhận";
        private PetGridMode _mode;

        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public static PetGridView Create(Transform parent, Font font)
        {
            var go = new GameObject("PetGrid", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(go.transform, false);
            var content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;

            var view = go.AddComponent<PetGridView>();
            view._font = font;
            view._content = content;
            view._scrollRect = go.GetComponent<ScrollRect>();
            view._scrollRect.viewport = (RectTransform)go.transform;
            view._scrollRect.content = content;
            view._scrollRect.horizontal = false;
            view._scrollRect.vertical = true;
            view._scrollRect.movementType = ScrollRect.MovementType.Clamped;
            view._scrollRect.scrollSensitivity = CardHeight * 0.75f;
            view._scrollRect.onValueChanged.AddListener(_ => view.Refresh());
            return view;
        }

        public void Bind(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider,
            string buttonLabel = "Nhận", PetGridMode mode = PetGridMode.Receive)
        {
            RecycleAll();
            _screen = screen;
            _assets = assets;
            _guider = guider;
            _buttonLabel = string.IsNullOrEmpty(buttonLabel) ? "Nhận" : buttonLabel;
            _mode = mode;
            _scrollRect.StopMovement();
            _content.anchoredPosition = Vector2.zero;
            UpdateLayout();
        }

        private void OnRectTransformDimensionsChange() => UpdateLayout();
        private void OnEnable() => UpdateLayout();
        private void OnDisable() => RecycleAll();
        private void OnDestroy() => RecycleAll();

        private void UpdateLayout()
        {
            if (_content == null) return;
            var height = Mathf.Max(0f, ((RectTransform)transform).rect.height);
            var contentHeight = MenuVirtualizer.ContentHeight(_screen?.Items?.Length ?? 0, CardHeight + Gap);
            _content.sizeDelta = new Vector2(0f, Mathf.Max(contentHeight, height));
            var position = _content.anchoredPosition;
            position.y = Mathf.Clamp(position.y, 0f, Mathf.Max(0f, contentHeight - height));
            _content.anchoredPosition = position;
            Refresh();
        }

        private void Refresh()
        {
            if (_content == null || !isActiveAndEnabled) return;
            var range = MenuVirtualizer.Compute(_screen?.Items?.Length ?? 0, CardHeight + Gap,
                ((RectTransform)transform).rect.height, _content.anchoredPosition.y);
            _recycle.Clear();
            foreach (var pair in _realized)
                if (!range.Contains(pair.Key)) _recycle.Add(pair.Key);
            foreach (var index in _recycle)
            {
                Recycle(_realized[index]);
                _realized.Remove(index);
            }
            for (var index = range.First; index < range.LastExclusive; index++)
            {
                if (_realized.ContainsKey(index)) continue;
                var card = _pool.Count > 0 ? _pool.Pop() : CreateCard();
                BindCard(card, _screen.Items[index], index);
                _realized.Add(index, card);
            }
        }

        private void RecycleAll()
        {
            foreach (var card in _realized.Values) Recycle(card);
            _realized.Clear();
        }

        private void Recycle(Card card)
        {
            card.Version++;
            card.Assets?.ReleaseOwner(card.Icon);
            card.Assets = null;
            card.Icon.texture = null;
            card.Item = null;
            card.Root.name = "PooledPetCard";
            card.Root.SetActive(false);
            _pool.Push(card);
        }

        private void SelectItem(MenuItemInfo item, int index)
        {
            if (_screen == null || _guider == null || !item.CanSelect) return;
            var screen = _screen;
            var guider = _guider;
            Action send = () => guider.Select(screen, index,
                item.PaymentOptions != null && item.PaymentOptions.Length > 0 ? 0 : -1);
            if (item.ShowDialog && ConfirmRequested != null)
                ConfirmRequested(MenuSelection.PromptFor(_screen, index), send);
            else
                send();
        }

    }
}
