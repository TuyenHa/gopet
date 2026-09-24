-- Độ bền trang bị pet + sửa ở Thợ Rèn (Thành phố Linh Thú) bằng Đá mài sửa chữa.
-- Chạy 1 lần trên DB game. Độ bền nằm trong JSON player.items nên KHÔNG cần đổi cột.

-- 1) Vật phẩm Đá mài sửa chữa (type 29 = ITEM_REPAIR_STONE, xếp chồng, giao dịch được).
INSERT INTO `item`(`itemId`,`name`,`description`,`type`,`iconPath`,`isStackable`,`canTrade`,`price`)
VALUES (1000091,'Đá mài sửa chữa',
        'Mang tới Thợ Rèn ở Thành phố Linh Thú để sửa đầy độ bền 1 trang bị pet.',
        29,'items/1000091.png',1,1,10);
-- Không ON DUPLICATE KEY: nếu prod đã có item 1000091 thì lỗi rõ ràng (runner dừng deploy)
-- thay vì âm thầm biến món đó thành Đá mài. Runner đã ghi sổ schema_migrations nên không chạy lại.

-- 2) NPC Thợ Rèn ở map 11 (NPC -32 cùng ảnh đang đứng ở map 28, toạ độ NPC là toàn cục
--    nên tạo NPC riêng). Option 98 = sửa trang bị, 99 = giải thích độ bền.
--    Toạ độ (258,106): đứng sát trước chiếc ghế trên cỏ (ghế ~x 262-301, chân ghế y≈91),
--    bên phải cột đèn (object 166 tại 221,113 trong map 11).
INSERT INTO `npc`(`npcId`,`name`,`chat`,`optionName`,`optionId`,`x`,`y`,`imgPath`,`type`,`bounds`)
VALUES (-42,'THO REN','["Đồ mòn thì mang đây, có Đá mài là ta sửa như mới!"]',
        '["Sửa trang bị","Độ bền là gì?"]','[98,99]',258,106,'npcs/Tho_Ren.png',1,'[-25,-25,50,50]');

-- Nối thêm -42 vào CUỐI danh sách hiện có, không ghi đè (prod có thể đã thêm/bớt NPC ở map 11).
UPDATE `map` SET `npc` = CONCAT(TRIM(TRAILING ']' FROM TRIM(`npc`)), ',-42]')
WHERE `mapID` = 11 AND `npc` NOT LIKE '%-42%';
