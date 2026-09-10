using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Selector 3 kênh chat: Place (khu vực) / Community (cộng đồng) / Guild (bang).
    /// Nút cạnh input trong <c>GameHud</c>. Bấm để cycle qua 3 kênh; hiển thị nhãn ngắn.
    ///
    /// <para>Server phân biệt qua opcode gửi khác nhau — không phải bơm channel tag vào
    /// text. Xem <c>ChatHandler</c> (Place, opcode 9) và <c>ChatChannelPackets</c>
    /// (Community + Guild).</para>
    /// </summary>
    public sealed class ChatChannelSelector : MonoBehaviour
    {
        public enum Channel { Place, Community, Guild }

        private static readonly (string label, Color color)[] Styles =
        {
            ("KV",  new Color(0.42f, 0.58f, 0.85f, 1f)),
            ("CĐ",  new Color(0.85f, 0.65f, 0.28f, 1f)),
            ("BANG",new Color(0.5f, 0.75f, 0.35f, 1f)),
        };

        public event Action<Channel> Changed;
        public Channel Current { get; private set; } = Channel.Place;

        private Text _label;
        private Image _bg;

        public static ChatChannelSelector Create(Transform parent)
        {
            var go = new GameObject("Chat Channel", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(58f, -5f);

            var comp = go.AddComponent<ChatChannelSelector>();
            comp._bg = go.GetComponent<Image>();
            RoundedUiSprite.Apply(comp._bg);

            comp._label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 12, true);
            comp._label.alignment = TextAnchor.MiddleCenter;
            comp._label.fontStyle = FontStyle.Bold;
            comp._label.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(comp.Cycle);
            comp.Apply();
            return comp;
        }

        public void SetChannel(Channel channel)
        {
            Current = channel;
            Apply();
            Changed?.Invoke(Current);
        }

        private void Cycle()
        {
            var next = (int)Current + 1;
            if (next > (int)Channel.Guild) next = 0;
            SetChannel((Channel)next);
        }

        private void Apply()
        {
            var s = Styles[(int)Current];
            _label.text = s.label;
            _bg.color = s.color;
        }
    }
}
