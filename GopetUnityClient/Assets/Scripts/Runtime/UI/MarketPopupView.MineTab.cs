using System;
using System.Collections.Generic;
using Gopet.Net.Market;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Tab "Gian hàng của tôi": danh sách cuộn không phân trang, nút Chỉ định/Gỡ
    /// mỗi dòng, footer "Đăng bán".</summary>
    public sealed partial class MarketPopupView
    {
        /// <summary>Khớp <c>GopetManager.KIOSK_PET</c> phía server — dùng để chọn đúng dòng phí.</summary>
        private const sbyte KioskPet = 4;
        private const float FooterHeight = 26f;
        private const float SellButtonWidth = 96f;
        private const float FooterGap = 6f;
        private const float MineActionWidth = 120f;
        private const float MineButtonHeight = 24f;

        private RectTransform _minePanel;
        private PopupItemList _mineList;
        private MarketAssignPanel _assignPanel;
        private readonly List<MarketListingRowView> _mineRows = new List<MarketListingRowView>();
        private MarketListingRow _assignTarget;

        private void BuildMineTab(RectTransform content, float contentWidth)
        {
            // KHÔNG SetActive(false) ở đây: HorizontalLayoutGroup bên trong (nút footer,
            // dòng giá của MarketListingRowView...) cần hierarchy đang BẬT để Unity tính
            // layout lần đầu. Tắt tab này diễn ra ở SelectTab ngay sau khi build xong cả
            // 2 tab — cùng khuôn DailyCheckinView (build cả 2 trang rồi mới Select(0)).
            _minePanel = MakeTabPanel(content, "Tab_Mine");

            var listArea = new GameObject("ListArea", typeof(RectTransform));
            listArea.transform.SetParent(_minePanel, false);
            var areaRect = (RectTransform)listArea.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0f, FooterHeight + FooterGap);
            areaRect.offsetMax = Vector2.zero;

            _mineList = PopupItemList.Create(areaRect, _font);
            _mineList.ShowPlaceholder("Đang tải…");

            BuildSellFooter(_minePanel);

            _assignPanel = MarketAssignPanel.Create(_minePanel, _font);
            _assignPanel.Submitted += OnAssignSubmitted;
        }

        private void BuildSellFooter(RectTransform parent)
        {
            var go = new GameObject("SellButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            // Nút nhỏ neo góc phải dưới, không kéo hết bề ngang.
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(SellButtonWidth, FooterHeight);
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 12, true);
            RoundedUiSprite.Apply(image);
            image.color = PopupPalette.ButtonBlue;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = "Đăng bán";

            go.GetComponent<Button>().onClick.AddListener(() => SellRequested?.Invoke());
        }

        private void RequestMine() => _market.RequestMine();

        private void ApplyMineState(MarketMineState state)
        {
            if (state == null) return;
            RebuildMineRows(state.Rows);
        }

        private void RebuildMineRows(IReadOnlyList<MarketListingRow> rows)
        {
            foreach (var row in _mineRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _mineRows.Clear();

            for (var i = 0; i < rows.Count; i++)
            {
                var listing = rows[i];
                var view = MarketListingRowView.Create(_mineList.Rows, _font);
                view.Bind(listing, _assets, TimeLeftText(listing));
                view.SetSeparatorVisible(i < rows.Count - 1);
                ((RectTransform)view.transform).anchoredPosition =
                    new Vector2(0f, -i * MarketListingRowView.Height);

                var captured = listing;
                // 2 nút cùng một hàng, giữa dòng: Chỉ định | Gỡ.
                view.SetActionWidth(MineActionWidth);
                const float top = (MarketListingRowView.Height - MineButtonHeight) / 2f;
                view.AddButton("Chỉ định", top, MineButtonHeight, () => OpenAssign(captured), 0f, 0.58f);
                view.AddButton("Gỡ", top, MineButtonHeight, () => ConfirmCancel(captured), 0.58f, 1f);

                _mineRows.Add(view);
            }

            _mineList.SetRowsHeight(rows.Count * MarketListingRowView.Height);
            _mineList.ShowPlaceholder(rows.Count == 0 ? "Bạn chưa treo món nào." : null);
        }

        private static string TimeLeftText(MarketListingRow row)
        {
            var seconds = Math.Max(0, row.SecondsLeft);
            var hours = seconds / 3600;
            var minutes = (seconds % 3600) / 60;
            var text = $"Còn lại: {hours}h{minutes:00}";
            return row.HasAssignedBuyer ? text + $" · Chỉ bán cho: {row.AssignedName}" : text;
        }

        private void OpenAssign(MarketListingRow row)
        {
            _assignTarget = row;
            _assignPanel.Show(row.KioskType == KioskPet, row.AssignedName);
        }

        private void OnAssignSubmitted(string buyerName)
        {
            if (_assignTarget == null) return;
            _market.Assign(_assignTarget.KioskType, _assignTarget.ListingId, buyerName);
            _assignTarget = null;
        }

        private void ConfirmCancel(MarketListingRow row)
        {
            void Send() => _market.Cancel(row.KioskType, row.ListingId);

            if (ConfirmRequested != null)
            {
                ConfirmRequested(new MenuSelection.ConfirmPrompt
                {
                    Text = $"Gỡ {JarIconTokens.Strip(row.Name)} khỏi Chợ trời?",
                    ConfirmLabel = "Gỡ",
                    CancelLabel = "Huỷ",
                }, Send);
            }
            else
            {
                Send();
            }
        }
    }
}
