using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Phần dựng khung của <see cref="BattleScenePopup"/>: panel giữa màn + tiêu đề + danh sách cuộn.</summary>
    public sealed partial class BattleScenePopup
    {
        private static Transform BuildPanel(Transform root, float listHeight)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, TitleHeight + listHeight + Pad * 2f);
            BattlePopupChrome.ApplyPanel(go);
            return go.transform;
        }

        private static void AddTitle(Transform panel, Font font, BattleScenePopup popup) =>
            BattlePopupChrome.AddTitle(panel, font, "KHUNG CẢNH TRẬN ĐẤU", 15, TitleHeight, Pad,
                () => popup.SetOpen(false));

        private static Transform AddScrollList(Transform panel, float listHeight) =>
            BattlePopupChrome.AddScrollList(panel, TitleHeight, listHeight, Pad, RowGap);
    }
}
