using Gopet.Net.Battle;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Nhóm xử lý thao tác người chơi của <see cref="BattleView"/>.
    /// Tách khỏi phần dựng UI để giữ mỗi file dưới ngưỡng 200 dòng.</summary>
    public sealed partial class BattleView
    {
        private void OnAttack()
        {
            if (!_turn.CanAct) return;
            _handler.SendNormalAttack();
            MarkSent();
        }

        /// <summary>KHÔNG gọi <see cref="MarkSent"/>: server trả lời nút này bằng một menu
        /// chọn vật phẩm (<c>MenuController.MENU_SELECT_ITEM_SUPPORT_PET</c>), KHÔNG phải
        /// gói lượt. Huỷ menu hoặc không có bình máu nào thì sẽ chẳng có gói 37 nào về để
        /// mở khoá lại. Gói lượt chỉ tới sau khi người chơi thật sự chọn một vật phẩm.</summary>
        private void OnPotion()
        {
            if (!_turn.CanAct) return;
            _handler.SendUseItem();
        }

        /// <summary>Nút tròn mở/đóng popup. Không gate theo lượt — xem được kỹ năng lúc nào
        /// cũng được, chỉ từng dòng mới khoá khi chưa tới lượt hoặc thiếu MP.</summary>
        private void ToggleSkillPopup() => _skillPopup?.Toggle();

        private void OnSkillUsed(int skillId)
        {
            if (!_turn.CanAct) return;
            SoundManager.Instance?.PlayEffect("s_button_ingame");
            _handler.SendSkill(skillId);
            _cooldowns.MarkUsed(skillId);
            MarkSent();
        }

        /// <summary>Khoá thao tác tới khi gói lượt về. Watchdog trong
        /// <see cref="Update"/> mở lại nếu gói không bao giờ tới.</summary>
        private void MarkSent()
        {
            _turn.MarkActionSent();
            _pendingSince = Time.unscaledTime;
            RefreshLocks();
        }

        /// <summary>Xin thua KHÔNG gate theo lượt: server xếp hàng hành động này và xử lý
        /// khi tới lượt người chơi (<c>PetBattle.cs:211-217</c>, <c>:677-684</c>).</summary>
        private void OnSurrenderClicked()
        {
            if (_surrendered) return;
            var d = YesNoDialog.Create(transform, "Bạn chắc chắn muốn xin thua?", "Xin thua", "Huỷ");
            d.Confirmed += () =>
            {
                _surrendered = true;
                _handler.SendSurrender();
                _actionBar.LockSurrender();
                Destroy(d.gameObject);
            };
            d.Cancelled += () => Destroy(d.gameObject);
        }

        private void OnBackClicked()
        {
            if (_result != null) { Closed?.Invoke(); return; }
            OnSurrenderClicked();
        }

        private static void PlayHitSound(BattleEffect effect, Transform target)
        {
            if (effect.SkillId == 1)
            {
                BattleFloatText.CreateMiss(target);
                SoundManager.Instance?.PlayEffect("s_attack_miss");
            }
            else if (effect.SkillId == 2) SoundManager.Instance?.PlayEffect("s_attack_crit");
            else if (effect.HpDelta < 0) SoundManager.Instance?.PlayEffect("s_hit");
        }
    }
}
