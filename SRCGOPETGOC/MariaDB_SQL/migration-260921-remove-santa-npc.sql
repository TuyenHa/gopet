-- Bỏ NPC "ÔNG GIÀ NOEL" (npcId -24) khỏi map Thành Phố Linh Thú (mapId 11).
--
-- LÝ DO: NPC sự kiện Noel đứng quanh năm giữa map khởi đầu. Phần thưởng của nó đi
-- qua `GameController.noelDaily()` — cơ chế điểm danh Noel 2024 — nay đã có tab
-- "Điểm danh" trong popup Sự kiện lo việc đó.
--
-- PHẠM VI: chỉ gỡ khỏi danh sách NPC của map. Bản ghi npcId -24 và code
-- `noelDaily()` giữ nguyên, nên muốn bày lại vào mùa Noel thì chỉ cần thêm -24 vào
-- cột `npc` là xong, không phải khôi phục gì.

UPDATE `map`
SET `npc` = '[-1,-7,-15, -25]'
WHERE `mapId` = 11 AND `npc` = '[-1,-7,-15, -24, -25]';
