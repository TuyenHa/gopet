using System;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>
    /// Popup chọn/mua khung cảnh màn đấu. Chỉ hiển thị gói STATE của server và phát sự kiện;
    /// xác nhận giá và gửi gói do <see cref="BattleView"/> lo.
    ///
    /// <para>Sau khi bấm Mua/Chọn mọi nút khoá lại tới khi STATE mới tới (server luôn gửi lại
    /// STATE, kể cả khi từ chối) — chống bấm hai lần gửi hai gói.</para></summary>
    public sealed partial class BattleScenePopup : MonoBehaviour
    {
        private const float Width = 380f;
        private const float TitleHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowGap = 4f;
        private const float Pad = 8f;
        private const float MaxListHeight = 5.2f * (RowHeight + RowGap);

        private Font _font;
        private Transform _content;
        private Text _status;
        private BattleSceneState _state;
        private long? _gold;
        private bool _busy;

        public event Action<BattleSceneState.Entry> BuyRequested;
        public event Action<int> SelectRequested;

        public bool IsOpen => gameObject.activeSelf;

        public static BattleScenePopup Create(Transform parent, Font font)
        {
            var root = new GameObject("Popup khung cảnh", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var popup = root.AddComponent<BattleScenePopup>();
            popup._font = font;

            // Nền mờ phủ kín: bấm ra ngoài là đóng, và chặn click xuyên xuống nút đánh. Phải là
            // ANH EM đứng trước panel, không phải cha: Unity đẩy click lên cha gần nhất có
            // handler, nên nút đóng đặt ở gốc sẽ nuốt mọi cú chạm trượt trong panel.
            var dim = new GameObject("Nền mờ", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            UiBuilder.Stretch((RectTransform)dim.transform);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            dim.GetComponent<Button>().onClick.AddListener(() => popup.SetOpen(false));

            var panel = BuildPanel(root.transform, MaxListHeight);
            AddTitle(panel, font, popup);
            popup._content = AddScrollList(panel, MaxListHeight);
            popup._status = UiBuilder.MakeText(panel, font, "Trạng thái", 13, true);
            popup._status.alignment = TextAnchor.MiddleCenter;
            popup._status.color = UiBuilder.TextMuted;

            root.SetActive(false);
            return popup;
        }

        public void SetOpen(bool open)
        {
            gameObject.SetActive(open);
            if (open) transform.SetAsLastSibling(); // nằm trên mọi thứ dựng sau nó
        }

        /// <summary>Vẽ lại theo STATE mới (null = chưa có, hiện "Đang tải...").</summary>
        public void Bind(BattleSceneState state, long? gold)
        {
            _state = state;
            _gold = gold;
            _busy = false;
            Rebuild();
        }

        /// <summary>Vàng đổi (vừa mua xong) thì tô lại giá đủ/thiếu, giữ nguyên trạng thái khoá.</summary>
        public void SetGold(long gold)
        {
            _gold = gold;
            if (IsOpen) Rebuild();
        }

        private void Rebuild()
        {
            for (var i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            var empty = _state == null || _state.Entries.Count == 0;
            _status.gameObject.SetActive(empty);
            if (empty)
            {
                _status.text = "Đang tải...";
                return;
            }
            foreach (var entry in _state.Entries) AddRow(entry);
        }

        private void OnBuy(BattleSceneState.Entry entry)
        {
            if (_busy) return;
            BuyRequested?.Invoke(entry);
        }

        private void OnSelect(int sceneId)
        {
            if (_busy) return;
            LockUntilState();
            SelectRequested?.Invoke(sceneId);
        }

        /// <summary>Khoá mọi nút tới khi <see cref="Bind"/> được gọi với STATE mới.</summary>
        public void LockUntilState()
        {
            _busy = true;
            foreach (var button in _content.GetComponentsInChildren<Button>()) button.interactable = false;
        }
    }
}
