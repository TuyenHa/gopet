namespace Gopet.Net.Auth
{
    /// <summary>
    /// Đổi mật khẩu. Server đọc trong <c>GameController.cs:329-340</c>:
    /// <c>CHANGE_NEW_PASSWORD / sbyte 2 / sbyte 7 / UTF oldPassword / UTF newPassword</c>.
    /// Hai byte <c>2</c> và <c>7</c> là hardcoded "signature" server verify.
    ///
    /// <para><b>Bảo mật:</b> plaintext trên dây; server BCrypt work-factor 12 phía sau.
    /// Đăng ký <see cref="PacketLogger"/> mask cho opcode này (Phase 5 rollout).</para>
    /// </summary>
    public static class ChangePasswordPackets
    {
        // GopetCMD.CHANGE_NEW_PASSWORD — cần confirm opcode value ở GopetCmd.cs.
        // Sub magic: 2 và 7 là verify byte server hard-code.
        private const sbyte Sig1 = 2;
        private const sbyte Sig2 = 7;

        public static Message ChangePassword(string oldPassword, string newPassword)
        {
            return Message.Create(GopetCmd.CHANGE_NEW_PASSWORD)
                .PutSByte(Sig1)
                .PutSByte(Sig2)
                .PutUtf(oldPassword ?? string.Empty)
                .PutUtf(newPassword ?? string.Empty);
        }
    }
}
