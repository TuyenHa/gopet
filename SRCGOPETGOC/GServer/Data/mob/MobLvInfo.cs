namespace Gopet.Data.Mob
{
    /// <summary>Một dòng của bảng `gopet_mob` — chỉ số quái theo cấp.</summary>
    public class MobLvInfo
    {
        public int lvl, str, agi, _int, exp, coin, hp;

        /// <summary>Tấn công / phòng thủ riêng của quái. NULL thì quay về công thức gốc
        /// <c>GameObject</c> (<c>str/3 + 5</c> và <c>agi/3</c>).
        ///
        /// <para>Cần hai cột này vì quái KHÔNG chạy <c>Pet.applyInfo()</c> — hàm nhân
        /// <c>atk = str*30</c>, <c>def = agi*20</c> chỉ có ở lớp <c>Pet</c>. Thiếu chúng thì
        /// quái cấp cao nhất vẫn thua xa DEF của pet có trang bị, và sát thương bị kẹp sàn 1
        /// nên quái gõ mãi cũng chỉ trừ đúng 1 máu.</para></summary>
        public int? atk, def;
    }
}
