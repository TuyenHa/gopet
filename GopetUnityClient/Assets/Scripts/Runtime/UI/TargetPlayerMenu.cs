using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup menu khi tap avatar 1 người chơi khác. Hiện tên + 5 action.
    ///
    /// <para>Reuse cấu trúc của <see cref="PetSlotActionsView"/> nhưng label +
    /// action riêng cho target-player. GameSession dispatch qua
    /// <c>TargetPlayerPackets</c> + <c>FriendPackets</c>.</para>
    /// </summary>
    public sealed class TargetPlayerMenu : MonoBehaviour
    {
        public enum Action { ViewInfo, Challenge, Pk, ViewEquip, AddFriend, ClanSkill }

        public event System.Action<Action> Chosen;
        public event System.Action CloseRequested;

        public static TargetPlayerMenu Create(Transform parent, int targetUserId, string targetName)
        {
            var backdrop = new GameObject("Target Player Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            var view = backdrop.AddComponent<TargetPlayerMenu>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220f, 344f);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            var font = UiBuilder.BuiltinFont();
            var header = UiBuilder.MakeText(panel.transform, font, "Header", 15, false);
            header.text = string.IsNullOrEmpty(targetName) ? $"#{targetUserId}" : targetName;
            header.fontStyle = FontStyle.Bold;
            header.alignment = TextAnchor.MiddleCenter;
            header.color = UiBuilder.TextMain;
            var hRect = header.rectTransform;
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0f, 1f);
            hRect.offsetMin = new Vector2(8f, -34f);
            hRect.offsetMax = new Vector2(-8f, -8f);

            view.Row(panel.transform, "Xem thông tin", Action.ViewInfo,  0, UiBuilder.ButtonFace);
            view.Row(panel.transform, "Thách đấu",     Action.Challenge, 1, new Color(0.85f, 0.55f, 0.15f, 1f));
            view.Row(panel.transform, "PK ngay",       Action.Pk,        2, new Color(0.75f, 0.25f, 0.25f, 1f));
            view.Row(panel.transform, "Xem đồ pet",    Action.ViewEquip, 3, new Color(0.35f, 0.55f, 0.75f, 1f));
            view.Row(panel.transform, "Kết bạn",       Action.AddFriend, 4, new Color(0.45f, 0.65f, 0.35f, 1f));
            view.Row(panel.transform, "Kỹ năng bang",  Action.ClanSkill, 5, new Color(0.55f, 0.45f, 0.75f, 1f));
            return view;
        }

        private void Row(Transform panel, string label, Action action, int index, Color color)
        {
            const float h = 40f, pad = 8f, headerH = 34f;
            var go = new GameObject($"Row:{action}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var top = pad + headerH + (h + 4f) * index;
            rect.offsetMin = new Vector2(pad, -(top + h));
            rect.offsetMax = new Vector2(-pad, -top);

            var img = go.GetComponent<Image>();
            img.color = color;
            RoundedUiSprite.Apply(img);

            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(() => Chosen?.Invoke(action));
        }
    }
}
