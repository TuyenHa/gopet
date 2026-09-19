using System;
using System.Collections.Generic;
using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Popup danh sách kỹ năng, mở ra từ nút tròn bên trái.
    ///
    /// <para>Chỉ pet của người chơi mới có popup này. Kỹ năng của quái KHÔNG hiển thị —
    /// quái tự chọn kỹ năng theo <c>useRate</c> trong bảng <c>gopet_mob_skill</c>
    /// (<c>PetBattle.mobAttack</c>), người chơi không cần biết trước.</para></summary>
    public sealed partial class BattleSkillPopup : MonoBehaviour
    {
        private readonly List<BattleSkillEntry> _rows = new List<BattleSkillEntry>();
        private SkillCooldownTracker _cooldowns;
        private int _currentMp;
        private bool _locked = true;

        public event Action<int> SkillUsed;

        public bool IsOpen => gameObject.activeSelf;

        public void Toggle() => SetOpen(!IsOpen);

        public void SetOpen(bool open)
        {
            gameObject.SetActive(open);
            if (open) RefreshLabels();
        }

        public void RefreshState(int currentMp, bool locked)
        {
            _currentMp = currentMp;
            _locked = locked;
            if (IsOpen) RefreshLabels();
        }

        private void OnSkillClicked(int skillId)
        {
            SkillUsed?.Invoke(skillId);
            // Đóng ngay sau khi chọn: lượt đã dùng xong, để popup mở che mất sân đấu.
            SetOpen(false);
        }

        private void RefreshLabels()
        {
            foreach (var row in _rows)
            {
                var ready = _cooldowns == null || _cooldowns.IsReady(row.SkillId);
                var usable = !_locked && ready && row.MpCost <= _currentMp;

                if (row.MpLabel != null)
                {
                    var cd = ready ? "" : $"  CD{_cooldowns.TurnsLeft(row.SkillId)}";
                    row.MpLabel.text = $"MP: {row.MpCost}{cd}";
                    row.MpLabel.color = usable
                        ? new Color(0.62f, 0.72f, 0.85f)
                        : new Color(0.85f, 0.45f, 0.45f);
                }
                if (row.NameLabel != null)
                {
                    row.NameLabel.color = usable ? Color.white : new Color(0.62f, 0.66f, 0.72f);
                }
                if (row.Button != null) row.Button.interactable = usable;
            }
        }

        /// <summary>Một dòng trong popup.</summary>
        private sealed class BattleSkillEntry
        {
            public int SkillId, MpCost;
            public Text NameLabel, MpLabel;
            public Button Button;
        }
    }
}
