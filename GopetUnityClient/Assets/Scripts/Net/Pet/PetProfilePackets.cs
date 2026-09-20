namespace Gopet.Net.Pet
{
    public static class PetProfilePackets
    {
        private static Message PetService(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);

        public static Message RequestProfile() => PetService(GopetCmd.MAGIC);

        public static Message RequestGym() => PetService(GopetCmd.GYM);

        public static Message AddPotential(int value, sbyte statIndex) =>
            PetService(GopetCmd.UP_TIEM_NANG).PutInt(value).PutSByte(statIndex);

        /// <summary>Xin server mở menu chọn kỹ năng (<c>MENU_LEARN_NEW_SKILL = 799</c>).
        ///
        /// <para><paramref name="skillId"/> mang HAI nghĩa, server lưu vào
        /// <c>Player.skillId_learn</c> rồi dùng lúc người chơi chọn xong
        /// (<c>MenuController.selectMenu.cs:352-400</c>):
        /// <list type="bullet">
        ///   <item><see cref="LearnNewSlot"/> (-1) — HỌC MỚI vào ô trống, cần pet còn điểm kỹ năng;</item>
        ///   <item>id một kỹ năng pet ĐANG có — THAY đúng kỹ năng đó.</item>
        /// </list></para>
        ///
        /// <para>Server KHÔNG reset <c>skillId_learn</c> sau khi dùng, nên phải gửi đúng giá trị
        /// ở mỗi lần bấm chứ đừng trông vào trạng thái còn sót từ lần trước.</para></summary>
        public static Message LearnSkill(int skillId) =>
            PetService(GopetCmd.MAGIC_LEARN_SKILL).PutInt(skillId);

        /// <summary>Giá trị báo "học vào ô trống" thay vì thay kỹ năng đang có.</summary>
        public const int LearnNewSlot = -1;
    }
}
