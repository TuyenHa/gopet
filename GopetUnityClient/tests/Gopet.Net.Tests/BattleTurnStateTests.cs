using Gopet.Net.Battle;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Kiểm chứng BattleTurnState — trạng thái lượt client-side.
    ///
    /// <para>Bất biến gốc: gói PET_BATTLE mang ActorId của người VỪA ra đòn
    /// (<c>PetBattle.cs:307-308</c> gửi rồi mới <c>nextTurn()</c>), nên lượt kế tiếp
    /// thuộc về bên còn lại. Xem plan 260919 phase-01.</para></summary>
    public sealed class BattleTurnStateTests
    {
        private const int LocalActor = 10;
        private const int MobActor = -1517289310; // id quái LUÔN âm (GopetPlace.cs:90)

        private static BattleTurnState Opened(bool localStarts = true) =>
            new BattleTurnState(LocalActor, localStarts);

        [Fact]
        public void MoTranVoiLocalStartsTrue_DuocPhepBamNgay()
        {
            var t = Opened();
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void MoTranPvE_QuaiDiTruoc_ChuaDuocBam()
        {
            // Constructor PvE đặt setIsActiveTurn(false) (PetBattle.cs:66) và MobAttackTime
            // khởi tạo = Now (:33) ⇒ mobAttack() chạy ngay tick đầu. BattleHandler.OnMobBattle
            // vì thế gán LocalStarts = false.
            var t = Opened(localStarts: false);
            Assert.False(t.IsLocalTurn);
            Assert.False(t.CanAct);
        }

        /// <summary>HỒI QUY: id quái là số ÂM (`GopetPlace.cs:90`
        /// `mobId = -Utilities.nextInt(2, int.MaxValue - 12)`). Bản đầu dùng guard
        /// `actorId &lt;= 0` nên nuốt sạch gói lượt của quái — lượt không bao giờ lật về
        /// người chơi, nhãn không hiện, nút khoá vĩnh viễn.</summary>
        [Fact]
        public void IdQuaiAm_VanPhaiDoiLuot()
        {
            var t = Opened(localStarts: false);
            t.ApplyTurnPacket(-1517289310, BattleTurn.Normal, 1);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void ChiDungSoAmMinusMot_MoiLaGoiHeThong()
        {
            // -2 là biên gần nhất của dải id quái, PHẢI được xử lý như một actor bình thường:
            // quái vừa ra đòn ⇒ lượt lật về người chơi. Chỉ đúng -1 mới là gói hệ thống.
            var t = Opened(localStarts: false);
            t.ApplyTurnPacket(-2, BattleTurn.Normal, 1);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void GoiVatPhamTrongLuotQuai_KhongTuNhienTraLuotChoNguoiChoi()
        {
            // Gói WAIT+0 effect chỉ mở khoá pending, KHÔNG được tự đặt IsLocalTurn = true.
            var t = Opened(localStarts: false);
            t.ApplyTurnPacket(LocalActor, BattleTurn.Wait, 0);
            Assert.False(t.IsLocalTurn);
            Assert.False(t.CanAct);
        }

        [Fact]
        public void QuaiBiDinhThan_GoiTurnSkipped_TraLuotVeChoNguoiChoi_IdAm()
        {
            // Server gửi gói hình dạng "đòn trượt" (NORMAL + 1 effect SKILL_MISS) qua
            // sendTurnSkipped() khi quái bị định thân — client phải nhận lại lượt.
            var t = Opened(localStarts: false);
            t.ApplyTurnPacket(MobActor, BattleTurn.Normal, 1);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void GuiHanhDong_KhoaChoDenKhiGoiLuotVe()
        {
            var t = Opened();
            t.MarkActionSent();
            Assert.True(t.ActionPending);
            Assert.False(t.CanAct);
            Assert.True(t.IsLocalTurn); // vẫn là lượt mình, chỉ đang chờ server
        }

        [Fact]
        public void MinhVuaDanh_ChuyenSangLuotQuai()
        {
            var t = Opened();
            t.MarkActionSent();
            t.ApplyTurnPacket(LocalActor, BattleTurn.Normal, 1);
            Assert.False(t.IsLocalTurn);
            Assert.False(t.CanAct);
            Assert.False(t.ActionPending);
        }

        [Fact]
        public void QuaiVuaDanh_TraLuotVeChoNguoiChoi()
        {
            var t = Opened(localStarts: false);
            t.ApplyTurnPacket(MobActor, BattleTurn.Normal, 1);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void GoiHeThongPetIdAm_KhongDoiLuotVaKhongMoKhoa()
        {
            // Độc / phản đòn gửi petId = -1 (PetBattle.cs:1547, :1617) ngay sau gói hành động.
            var t = Opened();
            t.MarkActionSent();
            t.ApplyTurnPacket(-1, BattleTurn.Normal, 1);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.ActionPending);
        }

        [Fact]
        public void DungVatPham_GiuNguyenLuotVaMoKhoaLai()
        {
            // PetBattle.cs:1643 gửi WAIT với turnDatas rỗng và KHÔNG gọi nextTurn().
            var t = Opened();
            t.MarkActionSent();
            t.ApplyTurnPacket(LocalActor, BattleTurn.Wait, 0);
            Assert.True(t.IsLocalTurn);
            Assert.True(t.CanAct);
        }

        [Fact]
        public void DungKyNang_CungLaWaitNhungCoEffect_VanDoiLuot()
        {
            // PetBattle.cs:1109 gửi WAIT nhưng :1102/:1106 luôn add ≥1 effect.
            var t = Opened();
            t.MarkActionSent();
            t.ApplyTurnPacket(LocalActor, BattleTurn.Wait, 1);
            Assert.False(t.IsLocalTurn);
        }

        [Fact]
        public void ClearPending_MoKhoaTheoLuotHienTai()
        {
            var t = Opened();
            t.MarkActionSent();
            t.ClearPending();
            Assert.True(t.CanAct);

            var mobTurn = Opened(localStarts: false);
            mobTurn.MarkActionSent();
            mobTurn.ClearPending();
            Assert.False(mobTurn.CanAct); // hết pending nhưng vẫn lượt quái
        }
    }
}
