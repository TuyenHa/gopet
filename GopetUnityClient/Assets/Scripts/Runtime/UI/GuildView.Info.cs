using System;
using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class GuildView
    {
        private Text _infoText;
        private Transform _guildListContainer;
        private InputField _searchField;
        private GameObject _donateBtn;

        private void BuildInfoPage(Transform page)
        {
            _infoText = UiBuilder.MakeText(page, _font, "InfoText", 13, false);
            _infoText.alignment = TextAnchor.UpperLeft;
            _infoText.color = UiBuilder.TextMain;
            _infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _infoText.verticalOverflow = VerticalWrapMode.Overflow;
            var r = _infoText.rectTransform;
            r.anchorMin = new Vector2(0f, 0.3f);
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(4f, 0f);
            r.offsetMax = new Vector2(-4f, 0f);

            _donateBtn = new GameObject("DonateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            _donateBtn.transform.SetParent(page, false);
            var dr = (RectTransform)_donateBtn.transform;
            dr.anchorMin = new Vector2(0.3f, 0.22f);
            dr.anchorMax = new Vector2(0.7f, 0.3f);
            dr.offsetMin = dr.offsetMax = Vector2.zero;
            RoundedUiSprite.Apply(_donateBtn.GetComponent<Image>());
            _donateBtn.GetComponent<Image>().color = TabActive;
            var dl = UiBuilder.MakeText(_donateBtn.transform, _font, "Label", 12, true);
            dl.text = "Cống hiến";
            dl.alignment = TextAnchor.MiddleCenter;
            dl.fontStyle = FontStyle.Bold;
            dl.color = new Color(0.1f, 0.08f, 0.02f, 1f);
            _donateBtn.GetComponent<Button>().onClick.AddListener(() => DonateRequested?.Invoke());
            _donateBtn.SetActive(false);

            BuildSearchBar(page);

            var listGo = new GameObject("GuildList", typeof(RectTransform));
            listGo.transform.SetParent(page, false);
            var lr = (RectTransform)listGo.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = new Vector2(1f, 0.68f);
            lr.offsetMin = new Vector2(0f, 0f);
            lr.offsetMax = Vector2.zero;
            _guildListContainer = listGo.transform;
        }

        private void BuildSearchBar(Transform page)
        {
            var bar = new GameObject("SearchBar", typeof(RectTransform));
            bar.transform.SetParent(page, false);
            var br = (RectTransform)bar.transform;
            br.anchorMin = new Vector2(0f, 0.68f);
            br.anchorMax = new Vector2(1f, 0.74f);
            br.offsetMin = br.offsetMax = Vector2.zero;

            var fieldGo = new GameObject("SearchInput", typeof(RectTransform), typeof(Image));
            fieldGo.transform.SetParent(bar.transform, false);
            var fr = (RectTransform)fieldGo.transform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(0.7f, 1f);
            fr.offsetMin = new Vector2(0f, 2f);
            fr.offsetMax = new Vector2(-4f, -2f);
            fieldGo.GetComponent<Image>().color = UiBuilder.Field;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(fieldGo.transform, false);
            UiBuilder.Stretch((RectTransform)textGo.transform);
            var txt = textGo.GetComponent<Text>();
            txt.font = _font;
            txt.fontSize = 12;
            txt.color = UiBuilder.TextMain;
            txt.supportRichText = false;

            _searchField = fieldGo.AddComponent<InputField>();
            _searchField.textComponent = txt;

            var btnGo = new GameObject("SearchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(bar.transform, false);
            var btnR = (RectTransform)btnGo.transform;
            btnR.anchorMin = new Vector2(0.72f, 0f);
            btnR.anchorMax = Vector2.one;
            btnR.offsetMin = new Vector2(0f, 2f);
            btnR.offsetMax = new Vector2(0f, -2f);
            RoundedUiSprite.Apply(btnGo.GetComponent<Image>());
            btnGo.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var btnLabel = UiBuilder.MakeText(btnGo.transform, _font, "Label", 12, true);
            btnLabel.text = "Tìm";
            btnLabel.alignment = TextAnchor.MiddleCenter;
            btnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                var text = _searchField.text;
                if (!string.IsNullOrEmpty(text)) SearchRequested?.Invoke(text);
            });
        }

        public void ShowGuildInfo(int clanId, string[] infoLines)
        {
            _clanId = clanId;
            _infoText.text = string.Join("\n", infoLines);
            _infoText.gameObject.SetActive(true);
            _searchField.transform.parent.gameObject.SetActive(false);
            _guildListContainer.gameObject.SetActive(false);
            _donateBtn.SetActive(clanId > 0);
        }

        public void ShowGuildList(GuildListResponse response)
        {
            _clanId = 0;
            _infoText.gameObject.SetActive(false);
            _donateBtn.SetActive(false);
            _searchField.transform.parent.gameObject.SetActive(true);
            _guildListContainer.gameObject.SetActive(true);

            foreach (Transform child in _guildListContainer) Destroy(child.gameObject);

            if (response.Entries == null) return;
            for (var i = 0; i < response.Entries.Length; i++)
            {
                var entry = response.Entries[i];
                MakeGuildListRow(_guildListContainer, entry, i);
            }
        }

        private void MakeGuildListRow(Transform container, GuildListEntry entry, int index)
        {
            var go = new GameObject($"Guild:{entry.ClanId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(container, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            float top = index * 42f;
            r.offsetMin = new Vector2(0f, -(top + 40f));
            r.offsetMax = new Vector2(0f, -top);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;

            var label = UiBuilder.MakeText(go.transform, _font, "Name", 12, false);
            label.text = entry.Name;
            label.color = UiBuilder.TextMain;
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = new Vector2(0.7f, 1f);
            lr.offsetMin = new Vector2(8f, 0f);
            lr.offsetMax = Vector2.zero;

            var joinGo = new GameObject("Join", typeof(RectTransform), typeof(Image), typeof(Button));
            joinGo.transform.SetParent(go.transform, false);
            var jr = (RectTransform)joinGo.transform;
            jr.anchorMin = new Vector2(0.72f, 0.15f);
            jr.anchorMax = new Vector2(0.98f, 0.85f);
            jr.offsetMin = jr.offsetMax = Vector2.zero;
            RoundedUiSprite.Apply(joinGo.GetComponent<Image>());
            joinGo.GetComponent<Image>().color = TabActive;
            var jLabel = UiBuilder.MakeText(joinGo.transform, _font, "JoinLabel", 11, true);
            jLabel.text = "Gia nhập";
            jLabel.alignment = TextAnchor.MiddleCenter;
            jLabel.color = new Color(0.1f, 0.08f, 0.02f, 1f);
            jLabel.fontStyle = FontStyle.Bold;

            var clanId = entry.ClanId;
            joinGo.GetComponent<Button>().onClick.AddListener(() => JoinRequested?.Invoke(clanId));
        }
    }
}
