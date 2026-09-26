using System.Collections.Generic;

/// <summary>
/// Luật mở map theo tiến độ nhiệm vụ: map nào đòi hoàn thành nhiệm vụ nào.
///
/// <para>Bảng nằm TRONG code chứ không phải một cột của <c>map_template</c> vì luật này
/// đổi vài lần mỗi năm chứ không phải mỗi ngày: một migration + loader + đường rollback
/// DB đắt hơn nhiều so với build lại server. Đổi ý thì chuyển sang DB sau, chữ ký
/// <see cref="TryGetRequiredTask"/> giữ nguyên nên chỗ gọi không phải sửa.</para>
///
/// <para>Thêm một luật = thêm một dòng vào <see cref="Required"/>:
/// <c>{ 21, 105 }</c> nghĩa là map 21 chỉ mở khi người chơi đã hoàn thành nhiệm vụ có
/// <c>taskTemplateId</c> 105 (giá trị nằm trong <c>playerData.wasTask</c>).</para>
///
/// <para>Bảng đang RỖNG — hành vi hiện tại không đổi. Điền sau khi chốt danh sách
/// map ↔ nhiệm vụ, và luôn thử trên tài khoản test trước: điền nhầm id sẽ khoá một map
/// mà người chơi đang đứng trong đó.</para>
/// </summary>
public static class MapUnlockRules
{
    private static readonly Dictionary<int, int> Required = new()
    {
    };

    /// <summary>
    /// Map này có đòi nhiệm vụ nào không. <c>false</c> = map không thuộc diện khoá theo
    /// nhiệm vụ (vẫn có thể bị khoá bởi luật khác, ví dụ thượng giới).
    /// </summary>
    public static bool TryGetRequiredTask(int mapId, out int taskId) =>
        Required.TryGetValue(mapId, out taskId);

    /// <summary>
    /// Map TẠM ĐÓNG với mọi người chơi thường: không có trong TELE_MENU, không warp / đổi
    /// kênh vào được (admin vẫn vào qua menu admin). 22 = Chợ trời — đóng 2026-09-26; map
    /// vẫn được nạp vì popup Chợ trời toàn cục dùng ki-ốt của nó. Bỏ id khỏi đây là mở lại
    /// (nhớ bỏ luôn ở client: WorldMapLayout.HiddenMaps, MapRenderer ẩn cổng).
    /// </summary>
    private static readonly HashSet<int> Closed = new() { 22 };

    public static bool IsClosed(int mapId) => Closed.Contains(mapId);
}
