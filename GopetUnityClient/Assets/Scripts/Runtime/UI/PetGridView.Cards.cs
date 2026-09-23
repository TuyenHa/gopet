using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class PetGridView
    {
        private static readonly string[] StatNames = { "strength-v2", "agility-v2", "hp-v2", "mp" };
        private static readonly string[] StatLabels = { "Sức mạnh", "Độ nhanh", "Máu tối đa", "Kỹ năng" };

        private sealed class Card
        {
            public GameObject Root;
            public RectTransform Rect;
            public GameObject Divider;
            public RawImage Icon;
            public Text Title, Description, ActionLabel;
            public Button Action;
            public readonly RectTransform[] Separators = new RectTransform[4];
            public readonly RectTransform[] StatIcons = new RectTransform[4];
            public readonly Text[] Stats = new Text[4];
            public MenuItemInfo Item;
            public int Index, Version;
            public RemoteAssetCache Assets;
        }

        private Card CreateCard()
        {
            var go = new GameObject("PetCard", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, CardHeight);
            go.GetComponent<Image>().color = Color.clear;
            var empty = new MenuItemInfo();
            BuildRowDivider(go.transform, true);
            BuildPetIcon(go.transform);
            BuildInfo(go.transform, empty);
            for (var i = 0; i < 4; i++) BuildStat(go.transform, StatNames[i], StatLabels[i], null, i);
            BuildAction(go.transform, empty);
            var card = new Card
            {
                Root = go, Rect = rect,
                Divider = go.transform.Find("RowDivider").gameObject,
                Icon = go.transform.Find("PetIcon").GetComponent<RawImage>(),
                Title = go.transform.Find("Title").GetComponent<Text>(),
                Description = go.transform.Find("Description").GetComponent<Text>(),
                Action = go.transform.Find("Action").GetComponent<Button>(),
                ActionLabel = go.transform.Find("Action/Label").GetComponent<Text>()
            };
            for (var i = 0; i < 4; i++)
            {
                card.Separators[i] = (RectTransform)go.transform.Find("Separator_" + StatNames[i]);
                card.StatIcons[i] = (RectTransform)go.transform.Find(StatNames[i] + "Icon");
                card.Stats[i] = go.transform.Find(StatNames[i] + "Text").GetComponent<Text>();
            }
            // The listener reads the slot's current binding, never a former item/index.
            card.Action.onClick.AddListener(() =>
            {
                if (card.Item != null) SelectItem(card.Item, card.Index);
            });
            return card;
        }

        private void BindCard(Card card, MenuItemInfo item, int index)
        {
            card.Item = item;
            card.Index = index;
            card.Version++;
            card.Root.name = "PetCard_" + index;
            card.Root.SetActive(true);
            card.Rect.anchoredPosition = new Vector2(0f, -index * (CardHeight + Gap));
            card.Divider.SetActive(index < _screen.Items.Length - 1);
            card.Title.text = Shorten(CapitalizeFirst(item.Title));
            card.Description.text = _mode == PetGridMode.Shop ? ShopElementLabel(item.Description)
                : _mode == PetGridMode.Top ? TopRankLabel(item.Description) : CleanDescription(item.Description);
            var infoWidth = _mode == PetGridMode.Top ? TopInfoWidth : InfoWidth;
            Place(card.Title.rectTransform, PortraitWidth, 16f, -6f, infoWidth);
            Place(card.Description.rectTransform, PortraitWidth, 27f, -21f, infoWidth);
            var strength = FindStat(item.Description, "str", "sức mạnh");
            var agility = FindStat(item.Description, "agi", "độ nhanh");
            BindStat(card, 0, strength, infoWidth);
            BindStat(card, 1, agility, infoWidth);
            BindStat(card, 2, MaxHp(item.Description, strength), infoWidth);
            BindStat(card, 3, MaxMp(item.Description, agility), infoWidth);
            card.Action.gameObject.SetActive(_mode != PetGridMode.Top);
            card.Action.interactable = item.CanSelect;
            card.Action.GetComponent<Image>().color = item.CanSelect ? Blue : new Color(0.65f, 0.69f, 0.75f, 1f);
            card.ActionLabel.text = _buttonLabel;
            BindIcon(card, item.ImagePath);
        }

        private void BindStat(Card card, int index, string value, float infoWidth)
        {
            var visible = _mode != PetGridMode.Top || index >= 2;
            card.Separators[index].gameObject.SetActive(visible);
            card.StatIcons[index].gameObject.SetActive(visible);
            card.Stats[index].gameObject.SetActive(visible);
            var column = _mode == PetGridMode.Top ? index - 2 : index;
            var start = PortraitWidth + infoWidth + column * StatWidth;
            card.Separators[index].anchoredPosition = new Vector2(start, 0f);
            card.StatIcons[index].anchoredPosition = new Vector2(start + 5f, 0f);
            card.Stats[index].rectTransform.anchoredPosition = new Vector2(start + 23f, -1f);
            card.Stats[index].text = StatLabels[index] + "\n<size=9><color=#087EF4>" + (value ?? "—") + "</color></size>";
        }

        private void BindIcon(Card card, string path)
        {
            card.Assets = _assets;
            card.Icon.texture = _assets == null ? null : _assets.Placeholder;
            if (_assets == null || string.IsNullOrEmpty(path)) return;
            var version = card.Version;
            _assets.Get(path, ImagePackets.TypeIcon, card.Icon, texture =>
            {
                if (this != null && card.Icon != null && card.Version == version && card.Item != null)
                    card.Icon.texture = texture;
            });
        }
    }
}
