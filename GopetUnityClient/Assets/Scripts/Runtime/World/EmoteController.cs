using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Emote/sit local — <b>KHÔNG có opcode server</b> (đã grep GopetCMD.cs).
    /// Jar's <c>ei.java</c> chuyển state animation actor client-side; other players
    /// không thấy (không có broadcast).
    ///
    /// <para>Class này giữ enum + state cho local visual; render layer subscribe
    /// <see cref="EmoteChanged"/> để đổi clip.</para>
    ///
    /// <para><b>Chưa nối vào <see cref="JarActorAnimation"/>:</b> cần map clip index
    /// cho Sit / Wave / Cheer trong file .anu của actor — hiện dùng chung clip Idle.
    /// Khi có clip mapping đầy đủ, subscribe event bên <c>WorldActorView</c> để đổi
    /// <c>CurrentClip</c>.</para>
    /// </summary>
    public sealed class EmoteController : MonoBehaviour
    {
        public enum Emote { None, Sit, Wave, Cheer }

        public event System.Action<Emote> EmoteChanged;

        public Emote Current { get; private set; } = Emote.None;

        public void SetEmote(Emote emote)
        {
            if (Current == emote) return;
            Current = emote;
            Debug.Log($"[Gopet] Emote → {emote} (client-side visual only, server không broadcast).");
            EmoteChanged?.Invoke(emote);
        }

        public void Toggle(Emote emote) => SetEmote(Current == emote ? Emote.None : emote);
    }
}
