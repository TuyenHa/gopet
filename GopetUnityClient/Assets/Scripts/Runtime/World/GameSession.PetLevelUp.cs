namespace Gopet.Runtime.World
{
    /// <summary>Chúc mừng pet lên cấp (toast), hoãn tới khi đóng màn đánh.</summary>
    public sealed partial class GameSession
    {
        /// <summary>Cấp vừa đạt trong trận, chờ đóng màn đánh mới báo (0 = không có).</summary>
        private int _pendingPetLevel;

        /// <summary>
        /// Chúc mừng pet lên cấp. Lên cấp thường xảy ra lúc thắng quái, khi màn đánh (canvas 40)
        /// còn che toast của HUD (35) — hoãn tới lúc đóng màn đánh, không thì toast tắt trước
        /// khi người chơi kịp thấy.
        /// </summary>
        private void AnnouncePetLevelUp(int level)
        {
            if (level <= 0) return;
            if (_battle?.View != null)
            {
                _pendingPetLevel = System.Math.Max(_pendingPetLevel, level);
                return;
            }
            ShowToast(PetLevelUpText(level));
        }

        private void FlushPetLevelUp()
        {
            if (_pendingPetLevel <= 0) return;
            var level = _pendingPetLevel;
            _pendingPetLevel = 0;
            ShowToast(PetLevelUpText(level));
        }

        private string PetLevelUpText(int level)
        {
            var name = _hud?.Character?.PlayerName;
            return string.IsNullOrWhiteSpace(name)
                ? $"Chúc mừng! Thú cưng đã đạt cấp {level}"
                : $"Chúc mừng! {name} đã đạt cấp {level}";
        }
    }
}
