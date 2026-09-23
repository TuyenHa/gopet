using System;
using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Panel quản lý bang hội: 5 tab (Info, Members, Chat, Top, Skills).
    /// Nếu chưa có bang → hiện danh sách bang + tìm kiếm + gia nhập.
    /// </summary>
    public sealed partial class GuildView : MonoBehaviour
    {
        private const float Padding = 8f;
        private const float TabHeight = 32f;
        private const float HeaderHeight = 36f;
        private const float CloseSize = 34f;
        private const int TabCount = 5;

        private static readonly string[] TabLabels = { "Thông tin", "Thành viên", "Chat", "Top", "Kỹ năng" };
        private static readonly Color TabActive = new Color(0.95f, 0.75f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.86f, 0.91f, 0.99f, 1f);
        private static readonly Color GuildText = new Color(0.10f, 0.15f, 0.23f, 1f);
        private static readonly Color GuildMutedText = new Color(0.34f, 0.40f, 0.50f, 1f);
        private static readonly Color GuildRow = new Color(0.88f, 0.93f, 1f, 1f);

        private Font _font;
        private Transform _panel;
        private Button[] _tabButtons;
        private Image[] _tabImages;
        private GameObject[] _tabPages;
        private int _activeTab = -1;
        private int _clanId;

        public event Action CloseRequested;
        public event Action<int> JoinRequested;
        public event Action<string> SearchRequested;
        public event Action<int> KickRequested;
        public event Action<int, string> ChatSent;
        public event Action<int> SkillRentRequested;
        public event Action TopFundRequested;
        public event Action MemberListRequested;
        public event Action DonateRequested;

        /// <summary>Người chơi chọn một MỨC góp quỹ; tham số là <c>GuildDonateOption.Id</c>
        /// server gửi kèm, không phải chỉ số dòng trên màn hình.</summary>
        public event Action<int> DonateOptionChosen;

        /// <summary>Xin mở thêm một ô kỹ năng bang. Server tự chọn ô kế tiếp.</summary>
        public event Action UnlockSkillSlotRequested;
        public event Action ChatHistoryRequested;

        public static GuildView Create(Transform parent)
        {
            var backdrop = new GameObject("Guild Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var view = backdrop.AddComponent<GuildView>();

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.95f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.97f, 0.985f, 1f, 1f);
            img.raycastTarget = true;
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            view._font = UiBuilder.DefaultFont();
            view._panel = panel.transform;
            view.BuildHeader();
            view.BuildTabs();
            view.BuildPages();
            view.SelectTab(0);
            return view;
        }

        private void BuildHeader()
        {
            var title = UiBuilder.MakeText(_panel, _font, "Title", 16, false);
            title.text = "Bang hội";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = GuildText;
            UiBuilder.PlaceRow(title.rectTransform, Padding, 22f, Padding);

            var closeGo = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(_panel, false);
            var cr = (RectTransform)closeGo.transform;
            cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.anchoredPosition = new Vector2(4f, 4f);
            cr.sizeDelta = new Vector2(CloseSize, CloseSize);

            var closeImage = closeGo.GetComponent<Image>();
            closeImage.preserveAspect = true;
            var closeSprite = HudSkin.Get(HudSkin.Close);
            if (closeSprite != null)
            {
                closeImage.sprite = closeSprite;
                closeImage.color = Color.white;
            }
            else
            {
                closeImage.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var xLabel = UiBuilder.MakeText(closeGo.transform, _font, "X", 18, true);
                xLabel.text = "×";
                xLabel.alignment = TextAnchor.MiddleCenter;
                UiBuilder.SetFontStyle(xLabel, FontStyle.Bold);
                xLabel.color = Color.white;
            }
            closeGo.GetComponent<Button>().onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private void BuildTabs()
        {
            _tabButtons = new Button[TabCount];
            _tabImages = new Image[TabCount];

            var tabBar = new GameObject("TabBar", typeof(RectTransform));
            tabBar.transform.SetParent(_panel, false);
            UiBuilder.PlaceRow((RectTransform)tabBar.transform, HeaderHeight, TabHeight, Padding);

            for (var i = 0; i < TabCount; i++)
            {
                var go = new GameObject($"Tab:{TabLabels[i]}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(tabBar.transform, false);
                var r = (RectTransform)go.transform;
                float xMin = (float)i / TabCount;
                float xMax = (float)(i + 1) / TabCount;
                r.anchorMin = new Vector2(xMin, 0f);
                r.anchorMax = new Vector2(xMax, 1f);
                r.offsetMin = new Vector2(2f, 0f);
                r.offsetMax = new Vector2(-2f, 0f);

                _tabImages[i] = go.GetComponent<Image>();
                RoundedUiSprite.Apply(_tabImages[i]);
                _tabImages[i].color = TabInactive;

                var label = UiBuilder.MakeText(go.transform, _font, "Label", 12, true);
                label.text = TabLabels[i];
                label.alignment = TextAnchor.MiddleCenter;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
                label.color = GuildText;

                _tabButtons[i] = go.GetComponent<Button>();
                var idx = i;
                _tabButtons[i].onClick.AddListener(() => SelectTab(idx));
            }
        }

        private void BuildPages()
        {
            _tabPages = new GameObject[TabCount];
            float top = HeaderHeight + TabHeight + 4f;

            for (var i = 0; i < TabCount; i++)
            {
                var page = new GameObject($"Page:{TabLabels[i]}", typeof(RectTransform));
                page.transform.SetParent(_panel, false);
                var r = (RectTransform)page.transform;
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(1f, 1f);
                r.offsetMin = new Vector2(Padding, Padding);
                r.offsetMax = new Vector2(-Padding, -top);
                _tabPages[i] = page;
            }

            BuildInfoPage(_tabPages[0].transform);
            BuildMembersPage(_tabPages[1].transform);
            BuildChatPage(_tabPages[2].transform);
            BuildTopPage(_tabPages[3].transform);
            BuildSkillsPage(_tabPages[4].transform);
        }

        private void SelectTab(int index)
        {
            if (index == _activeTab) return;
            _activeTab = index;
            for (var i = 0; i < TabCount; i++)
            {
                _tabImages[i].color = i == index ? TabActive : TabInactive;
                _tabPages[i].SetActive(i == index);
            }
            if (index == 1) MemberListRequested?.Invoke();
            if (index == 2) ChatHistoryRequested?.Invoke();
            if (index == 3) TopFundRequested?.Invoke();
        }
    }
}
