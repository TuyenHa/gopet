using Gopet.Data.Collections;
using Gopet.Data.Dialog;
using Gopet.Data.Map;
using Gopet.Util;

/// <summary>
/// Hướng dẫn "nhiệm vụ tiếp theo" (menu <see cref="MenuController.MENU_SHOW_NEXT_TASK_GUIDE"/>):
/// nhiệm vụ chính nào nhận được, gặp NPC nào, ở map nào.
///
/// <para>Hai loại dòng, nhận được ngay đứng trước:</para>
/// <list type="bullet">
///   <item>Đủ điều kiện (mọi nhiệm vụ cần đã xong) → "Đến {map} gặp {NPC} để nhận."</item>
///   <item>Chỉ còn thiếu các nhiệm vụ ĐANG làm → "Hoàn thành {nhiệm vụ đang làm} rồi đến…".
///         Nhiệm vụ còn thiếu điều kiện chưa nhận thì bỏ, tránh lộ cả chuỗi.</item>
/// </list>
/// <para>Chỉ nhiệm vụ chính (<see cref="TaskCalculator.TASK_TYPE_MAIN"/>): nhiệm vụ lặp/ngày/
/// bang không có chuỗi "tiếp theo".</para>
/// </summary>
public static class TaskGuide
{
    public static JArrayList<MenuItemInfo> BuildNextTaskItems(Player player)
    {
        PlayerData data = player.playerData;
        JArrayList<MenuItemInfo> items = new();
        if (data == null) return items;

        List<(bool ready, TaskTemplate task, string text)> rows = new();
        foreach (TaskTemplate task in GopetManager.taskTemplate.Values)
        {
            if (task.getType() != TaskCalculator.TASK_TYPE_MAIN) continue;
            int id = task.getTaskId();
            if (data.wasTask.Contains(id) || data.tasking.Contains(id)) continue;

            int[] needs = task.getTaskNeed() ?? Array.Empty<int>();
            List<int> missing = needs.Where(n => !data.wasTask.Contains(n)).ToList();
            if (missing.Any(n => !data.tasking.Contains(n))) continue;

            string where = DescribeNpc(task.getFromNpc(), player);
            string text = missing.Count == 0
                ? Utilities.Format(player.Language.TASK_GUIDE_READY, where)
                : Utilities.Format(player.Language.TASK_GUIDE_AFTER, string.Join(", ", missing.Select(n => TaskName(n, player))), where);
            rows.Add((missing.Count == 0, task, text));
        }

        foreach (var row in rows.OrderByDescending(r => r.ready).ThenBy(r => r.task.getTaskId()))
        {
            // Không chọn được: đây là lời chỉ đường, nhận nhiệm vụ vẫn phải gặp NPC.
            items.add(new MenuItemInfo(TaskName(row.task.getTaskId(), player), row.text, "dialog/1.png", false));
        }
        return items;
    }

    /// <summary>"{NPC} tại {map}" — map tra ngược từ <c>MapTemplate.npc</c>.</summary>
    private static string DescribeNpc(int npcId, Player player)
    {
        string npcName = GopetManager.npcTemplate.TryGetValue(npcId, out var npc)
            ? SafeName(() => npc.getName(player), npc.name)
            : $"NPC {npcId}";
        List<string> maps = new();
        foreach (var entry in GopetManager.mapTemplate)
        {
            MapTemplate map = entry.Value;
            if (map?.npc == null || !map.npc.Contains(npcId)) continue;
            string mapName = player.Language.MapLanguage.TryGetValue(map.mapId, out var localized) ? localized : map.name;
            if (!string.IsNullOrEmpty(mapName) && !maps.Contains(mapName)) maps.Add(mapName);
        }
        return maps.Count == 0
            ? Utilities.Format(player.Language.TASK_GUIDE_MEET_NPC, npcName)
            : Utilities.Format(player.Language.TASK_GUIDE_MAP_NPC, string.Join(", ", maps), npcName);
    }

    private static string TaskName(int taskId, Player player)
    {
        TaskTemplate task = GopetManager.taskTemplate.get(taskId);
        if (task == null) return $"nhiệm vụ {taskId}";
        return SafeName(() => task.getName(player), task.name);
    }

    /// <summary>Bảng ngôn ngữ có thể thiếu khoá (dữ liệu mới thêm) — rơi về tên gốc.</summary>
    private static string SafeName(Func<string> localized, string fallback)
    {
        try
        {
            string name = localized();
            return string.IsNullOrEmpty(name) ? fallback : name;
        }
        catch (KeyNotFoundException)
        {
            return fallback;
        }
    }
}
