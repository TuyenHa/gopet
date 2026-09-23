using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void BuildFriendsTab()
        {
            var friends = MakePane("Bạn bè", 0f, 0.42f);
            var tools = MakePane("Kết nối bạn bè", 0.42f, 1f);
            TightenFriendsTitle(MakeTitle(friends, "Danh sách bạn bè", FriendsIcon(0)));
            _leftListHost = MakeListHost(friends, 38f);
            _friendsEmptyState = BuildFriendsEmptyState(_leftListHost);

            TightenFriendsTitle(MakeTitle(tools, "Tìm kiếm bạn bè", FriendsIcon(2)));
            BuildFriendSearch(tools);

            var invitesTitle = MakeTitle(tools, "Lời mời kết bạn", FriendsIcon(1));
            TightenFriendsTitle(invitesTitle);
            var invitesTitleRect = invitesTitle.rectTransform;
            invitesTitleRect.offsetMin = new Vector2(48f, invitesTitleRect.offsetMin.y);
            invitesTitleRect.anchoredPosition = new Vector2(0f, -104f);
            _rightListHost = MakeListHost(tools, 136f);
            Request(CharacterMenuAction.FriendManage);
        }

        private static Transform BuildFriendsEmptyState(Transform parent)
        {
            var go = new GameObject("Empty friends", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, 120f);
            rect.anchoredPosition = new Vector2(0f, 10f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(64f, 64f);
            iconRect.anchoredPosition = new Vector2(0f, 0f);
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = FriendsIcon(1);
            iconImage.preserveAspect = true;

            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Empty label", 16, true);
            label.text = "Danh sách bạn bè";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.48f, 0.60f, 0.82f, 1f);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -68f);
            return go.transform;
        }

        private static void TightenFriendsTitle(Text title)
        {
            if (title == null) return;
            var titleRect = title.rectTransform;
            titleRect.offsetMin = new Vector2(36f, titleRect.offsetMin.y);
            var siblingIndex = title.transform.GetSiblingIndex() + 1;
            if (siblingIndex >= title.transform.parent.childCount) return;
            var icon = title.transform.parent.GetChild(siblingIndex) as RectTransform;
            if (icon == null || icon.name != "Title icon") return;
            icon.sizeDelta = new Vector2(24f, 24f);
            icon.anchoredPosition = new Vector2(20f, -20f);
        }

        private void BuildFriendSearch(Transform parent)
        {
            var inputGo = new GameObject("Friend name input", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(parent, false);
            var inputRect = (RectTransform)inputGo.transform;
            inputRect.anchorMin = new Vector2(0f, 1f);
            inputRect.anchorMax = new Vector2(1f, 1f);
            inputRect.pivot = new Vector2(0f, 1f);
            inputRect.offsetMin = new Vector2(8f, -78f);
            inputRect.offsetMax = new Vector2(-122f, -42f);
            var inputImage = inputGo.GetComponent<Image>();
            RoundedUiSprite.Apply(inputImage);
            inputImage.color = Color.white;
            var inputOutline = inputGo.AddComponent<Outline>();
            inputOutline.effectColor = new Color(0.68f, 0.72f, 0.80f, 1f);
            inputOutline.effectDistance = new Vector2(1f, -1f);

            var input = inputGo.GetComponent<InputField>();
            input.contentType = InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;
            input.textComponent = UiBuilder.MakeText(inputGo.transform, UiBuilder.DefaultFont(), "Text", 13, false);
            input.textComponent.color = new Color(0.14f, 0.2f, 0.3f, 1f);
            input.textComponent.alignment = TextAnchor.MiddleLeft;
            UiBuilder.Stretch(input.textComponent.rectTransform);
            input.textComponent.rectTransform.offsetMin = new Vector2(12f, 0f);
            input.textComponent.rectTransform.offsetMax = new Vector2(-44f, 0f);
            var placeholder = UiBuilder.MakeText(inputGo.transform, UiBuilder.DefaultFont(), "Placeholder", 13, false);
            placeholder.text = "Nhập tên bạn bè...";
            placeholder.color = UiBuilder.TextMuted;
            placeholder.alignment = TextAnchor.MiddleLeft;
            UiBuilder.Stretch(placeholder.rectTransform);
            placeholder.rectTransform.offsetMin = new Vector2(12f, 0f);
            placeholder.rectTransform.offsetMax = new Vector2(-44f, 0f);
            input.placeholder = placeholder;

            var searchIcon = new GameObject("Search icon", typeof(RectTransform), typeof(Image));
            searchIcon.transform.SetParent(inputGo.transform, false);
            var searchRect = (RectTransform)searchIcon.transform;
            searchRect.anchorMin = searchRect.anchorMax = new Vector2(1f, 0.5f);
            searchRect.pivot = new Vector2(1f, 0.5f);
            searchRect.sizeDelta = new Vector2(28f, 28f);
            searchRect.anchoredPosition = new Vector2(-10f, 0f);
            var searchImage = searchIcon.GetComponent<Image>();
            searchImage.sprite = FriendsIcon(2);
            searchImage.preserveAspect = true;

            var send = MakeAction(parent, "Gửi lời mời", 44f);
            var sendRect = send.GetComponent<RectTransform>();
            sendRect.anchorMin = new Vector2(1f, 1f);
            sendRect.anchorMax = new Vector2(1f, 1f);
            sendRect.pivot = new Vector2(1f, 1f);
            sendRect.sizeDelta = new Vector2(108f, 36f);
            sendRect.anchoredPosition = new Vector2(-8f, -42f);
            var sendText = send.GetComponentInChildren<Text>();
            sendText.alignment = TextAnchor.MiddleCenter;
            send.onClick.AddListener(() =>
            {
                var name = input.text == null ? string.Empty : input.text.Trim();
                if (name.Length == 0) return;
                FriendAddRequested?.Invoke(name);
                input.text = string.Empty;
            });
        }

    }
}
