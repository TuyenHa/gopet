using Gopet.Net.Guider;
using Gopet.Net.Player;
using Gopet.Runtime.UI;
using Gopet.Runtime.World.Battle;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Khung cảnh màn đấu: nền + hiệu ứng, nút tròn bên phải và popup chọn/mua.
    /// Áp dụng cho mọi loại trận (PvE, PvP, đấu trường); mỗi người thấy khung cảnh của mình.</summary>
    public sealed partial class BattleView
    {
        private BattleSceneSettings _scenes;
        private BattleBackdrop _backdrop;
        private BattleScenePopup _scenePopup;
        private UnityEngine.UI.Button _sceneButton;
        private long? _gold;

        private void BuildBackdrop()
        {
            _backdrop = BattleBackdrop.Create(transform, _scenes?.SelectedId ?? 0);
        }

        /// <summary>Không có <see cref="BattleSceneSettings"/> (test, dựng lẻ) thì chỉ có nền rừng, không có nút.</summary>
        private void BuildSceneControls(Font font)
        {
            if (_scenes == null) return;
            _sceneButton = BattleSceneButton.Create(transform, font, ToggleScenePopup);
            _scenePopup = BattleScenePopup.Create(transform, font);
            _scenePopup.BuyRequested += ConfirmBuyScene;
            _scenePopup.SelectRequested += _scenes.Select;
            _scenes.Changed += OnSceneState;
        }

        private void ToggleScenePopup()
        {
            if (_scenePopup.IsOpen) { _scenePopup.SetOpen(false); return; }
            // Hết trận thì không mở nữa: popup đưa lên trên cùng sẽ đè panel kết quả/nút OK.
            if (HasResult || _closeRequested) return;
            _scenePopup.Bind(_scenes.State, _gold);
            _scenePopup.SetOpen(true);
            _scenes.Refresh(); // lấy sở hữu/giá mới nhất; STATE tới sẽ Bind lại
        }

        private void OnSceneState(BattleSceneState state)
        {
            _backdrop?.Apply(state.SelectedId);
            _scenePopup?.Bind(state, _gold);
        }

        private void ConfirmBuyScene(BattleSceneState.Entry entry)
        {
            var d = YesNoDialog.Create(transform,
                $"Mua khung cảnh \"{entry.Name}\" với {BattleScenePopup.FormatGold(entry.PriceGold)} vàng?",
                "Mua", "Huỷ");
            d.Confirmed += () =>
            {
                _scenePopup.LockUntilState();
                _scenes.Buy(entry.Id);
                Destroy(d.gameObject);
            };
            d.Cancelled += () => Destroy(d.gameObject);
        }

        /// <summary>Tiền đổi giữa trận (vừa mua khung cảnh, nhận thưởng): cập nhật thanh trên và giá trong popup.</summary>
        public void ApplyPlayerStats(PlayerStats stats)
        {
            if (stats == null) return;
            _gold = stats.Gold;
            _topBar?.UpdateCurrency(stats);
            _scenePopup?.SetGold(stats.Gold);
        }

        private void UnbindScenes()
        {
            if (_scenes != null) _scenes.Changed -= OnSceneState;
        }
    }
}
