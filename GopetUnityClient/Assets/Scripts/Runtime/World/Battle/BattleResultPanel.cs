using System;
using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public static class BattleResultPanel
    {
        public static GameObject Create(Transform parent, BattleResult result,
            int localActorId, bool isParticipant, Action onContinue)
        {
            var go = new GameObject("Kết quả", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(460f, 230f);
            go.GetComponent<Image>().color = new Color(0.02f, 0.14f, 0.23f, 0.97f);

            var font = UiBuilder.BuiltinFont();
            var title = UiBuilder.MakeText(go.transform, font, "Tiêu đề", 28, false);
            UiBuilder.PlaceRow(title.rectTransform, 15f, 45f, 15f);
            title.alignment = TextAnchor.MiddleCenter;
            title.text = result.WinnerId == localActorId ? "THẮNG CUỘC" :
                isParticipant ? "THUA CUỘC" : "KẾT THÚC";

            var body = UiBuilder.MakeText(go.transform, font, "Thưởng", 18, false);
            UiBuilder.PlaceRow(body.rectTransform, 70f, 90f, 20f);
            body.alignment = TextAnchor.UpperCenter;
            body.text = $"Ngọc: {result.Coin}    EXP: {result.Experience}\n{string.Join("\n", result.Messages)}";

            var btnGo = new GameObject("Tiếp tục", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            var bRect = (RectTransform)btnGo.transform;
            bRect.anchorMin = bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = new Vector2(0f, 16f);
            bRect.sizeDelta = new Vector2(170f, 48f);
            btnGo.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var bText = UiBuilder.MakeText(btnGo.transform, font, "Nhãn", 17, true);
            bText.text = "Tiếp tục";
            bText.alignment = TextAnchor.MiddleCenter;
            bText.color = Color.white;
            bText.fontStyle = FontStyle.Bold;
            btnGo.GetComponent<Button>().onClick.AddListener(() => onContinue?.Invoke());
            return go;
        }
    }
}
