using System;
using System.Text;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn thông tin pet dùng chung cho pet bản thân và pet người chơi khác.</summary>
    public sealed class PetProfileView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action GymRequested;
        public event Action TattooRequested;

        public static PetProfileView Create(Transform parent, PetProfile profile,
            RemoteAssetCache assets, bool editable)
        {
            var root = new GameObject("Pet Profile", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var view = root.AddComponent<PetProfileView>();
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build(profile, assets, editable);
            return view;
        }

        private void Build(PetProfile value, RemoteAssetCache assets, bool editable)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560f, 430f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());

            var title = UiBuilder.MakeText(panel.transform, UiBuilder.BuiltinFont(), "Title", 20, false);
            UiBuilder.PlaceRow(title.rectTransform, 12f, 34f, 16f);
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.text = value.Name ?? "Pet";

            var body = UiBuilder.MakeText(panel.transform, UiBuilder.BuiltinFont(), "Details", 15, false);
            UiBuilder.PlaceRow(body.rectTransform, 54f, 300f, 24f);
            body.alignment = TextAnchor.UpperLeft;
            body.text = Describe(value);

            if (editable)
            {
                var gym = MakeButton(panel.transform, "Cộng tiềm năng", new Vector2(-95f, 18f));
                gym.onClick.AddListener(() => GymRequested?.Invoke());
                gym.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180f, 18f);
                var tattoo = MakeButton(panel.transform, "Hình xăm", new Vector2(0f, 18f));
                tattoo.onClick.AddListener(() => TattooRequested?.Invoke());
            }
            var close = MakeButton(panel.transform, "Đóng", new Vector2(95f, 18f));
            close.onClick.AddListener(() => CloseRequested?.Invoke());
            if (editable) close.GetComponent<RectTransform>().anchoredPosition = new Vector2(180f, 18f);
        }

        private static string Describe(PetProfile value)
        {
            var text = new StringBuilder();
            text.Append("Cấp ").Append(value.Level)
                .Append("   Hệ ").Append(value.Element)
                .Append("   Lớp ").Append(value.PetClass).AppendLine();
            text.Append("EXP: ").Append(value.Experience).Append(" / ")
                .Append(value.ExperienceToNextLevel).AppendLine();
            text.Append("STR ").Append(value.Str).Append("   AGI ").Append(value.Agi)
                .Append("   INT ").Append(value.Int).AppendLine();
            text.Append("ATK ").Append(value.Atk).Append("   DEF ").Append(value.Def).AppendLine();
            text.Append("HP ").Append(value.Hp).Append('/').Append(value.MaxHp)
                .Append("   MP ").Append(value.Mp).Append('/').Append(value.MaxMp).AppendLine();
            text.Append("Điểm tiềm năng: ").Append(value.PotentialPoints).AppendLine().AppendLine();
            text.AppendLine("Kỹ năng:");
            foreach (var skill in value.Skills)
                text.Append("• ").Append(skill.Name).Append(" (MP ").Append(skill.MpCost)
                    .Append(") — ").Append(skill.Description).AppendLine();
            if (value.Tattoos.Length > 0)
            {
                text.AppendLine().AppendLine("Hình xăm:");
                foreach (var tattoo in value.Tattoos) text.Append("• ").AppendLine(tattoo.Name);
            }
            return text.ToString();
        }

        private static Button MakeButton(Transform parent, string label, Vector2 position)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(170f, 42f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }
    }
}
