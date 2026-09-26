-- Thợ Rèn (-42) thế chỗ cây ATM ở Thành Phố Linh Thú (map 11).
-- Cây ATM là vật hoạt ảnh 72 trong file map (không phải NPC): client bỏ vẽ nó và vùng bấm
-- building loại 9 đi kèm (MapRenderer.AtmAnimationId/AtmBuildingType). Chân ATM ~ (440,108)
-- — thân máy x 427..452, y 64..108 — nên NPC đứng đúng đó, toạ độ NPC là điểm giữa-chân.
UPDATE `npc` SET `x` = 440, `y` = 108 WHERE `npcId` = -42;
