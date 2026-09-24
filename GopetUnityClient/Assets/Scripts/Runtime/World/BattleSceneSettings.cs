using System;
using Gopet.Net.Guider;
using Gopet.UiLogic;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Khung cảnh màn đấu của người chơi, giữ suốt phiên. Server là nguồn sự thật (sở hữu,
    /// giá, lựa chọn); lớp này chỉ nhớ gói STATE mới nhất và chuyển lệnh mua/chọn đi.
    ///
    /// <para>Xin STATE ngay sau đăng nhập để trận ĐẦU TIÊN đã có đúng khung cảnh. Server cũ
    /// không hiểu gói thì không bao giờ trả lời — khi đó <see cref="SelectedId"/> giữ rừng
    /// mặc định và màn đấu vẫn chạy như trước.</para></summary>
    public sealed class BattleSceneSettings
    {
        private readonly GuiderHandler _guider;

        public BattleSceneState State { get; private set; }
        public int SelectedId => State?.SelectedId ?? BattleSceneCatalog.DefaultId;

        /// <summary>STATE mới tới (sau khi mở, mua, chọn — kể cả khi server từ chối).</summary>
        public event Action<BattleSceneState> Changed;

        public BattleSceneSettings(GuiderHandler guider)
        {
            _guider = guider ?? throw new ArgumentNullException(nameof(guider));
            _guider.BattleSceneStateReceived += OnState;
        }

        public void Refresh() => _guider.RequestBattleScenes();
        public void Buy(int sceneId) => _guider.BuyBattleScene(sceneId);
        public void Select(int sceneId) => _guider.SelectBattleScene(sceneId);

        private void OnState(BattleSceneState state)
        {
            State = state;
            Changed?.Invoke(state);
        }
    }
}
