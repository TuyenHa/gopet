using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Phần GÓP QUỸ của <see cref="GuildView"/>. Tách khỏi TopSkills để mỗi file
    /// giữ dưới ngưỡng 200 dòng.</summary>
    public sealed partial class GuildView
    {
        /// <summary>Mỗi mức góp là một DÒNG BẤM ĐƯỢC, không phải một khối text: xem được bảng
        /// giá mà không góp được thì luồng cụt ngay trước bước cuối.
        ///
        /// <para>Giữ NGUYÊN thứ tự server gửi và gửi lại <c>option.Id</c> chứ không đánh số lại —
        /// server tra <c>donateInfos.get(menuId)</c> theo đúng chỉ số đó
        /// (<c>GameController.cs:2653-2655</c>), lệch một nấc là góp nhầm mức tiền.</para></summary>
        public void ShowDonateOptions(GuildDonateOption[] options)
        {
            SelectTab(0);
            _infoText.gameObject.SetActive(false);
            _searchField.transform.parent.gameObject.SetActive(false);
            _guildListContainer.gameObject.SetActive(true);

            foreach (Transform child in _guildListContainer) Destroy(child.gameObject);
            if (options == null) return;
            for (var i = 0; i < options.Length; i++) MakeDonateRow(options[i], i);
        }

        private void MakeDonateRow(GuildDonateOption option, int index)
        {
            var go = new GameObject($"Donate:{option.Id}", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(_guildListContainer, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            var top = index * 42f;
            r.offsetMin = new Vector2(0f, -(top + 40f));
            r.offsetMax = new Vector2(0f, -top);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = GuildRow;

            var label = UiBuilder.MakeText(go.transform, _font, "Desc", 12, false);
            label.text = option.Description;
            label.color = GuildText;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = new Vector2(0.78f, 1f);
            lr.offsetMin = new Vector2(8f, 0f);
            lr.offsetMax = Vector2.zero;

            var optionId = option.Id;
            MakeRowButton(go.transform, "Góp", () => DonateOptionChosen?.Invoke(optionId));
        }

        /// <summary>Nút nhỏ nằm mép phải một dòng — dùng chung cho dòng góp quỹ và ô kỹ năng.</summary>
        private void MakeRowButton(Transform row, string text, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.8f, 0.15f);
            rect.anchorMax = new Vector2(0.98f, 0.85f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = TabActive;
            var label = UiBuilder.MakeText(go.transform, _font, "BtnLabel", 11, true);
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.1f, 0.08f, 0.02f, 1f);
            go.GetComponent<Button>().onClick.AddListener(onClick);
        }
    }
}
