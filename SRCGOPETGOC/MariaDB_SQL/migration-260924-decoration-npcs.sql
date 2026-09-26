-- NPC trang trí: chỉ giữ hình và vị trí, bỏ tên, nút "Nói chuyện" và lời giới thiệu.
--
-- LÝ DO: các NPC dưới đây không có chức năng — optionId/chat rỗng ngay từ dump gốc, không
-- có nhiệm vụ (bảng task), server và client jar gốc không xử lý riêng id nào — nhưng vẫn
-- hiện tên và nút "Nói chuyện", bấm vào ra menu trống. type = 9 là NPC chỉ đứng làm cảnh:
-- client Unity ẩn tên, không hiện nút "Nói chuyện", không nhận bấm
-- (xem NpcSpawn.DecorationType trong GopetUnityClient).
--
--   28 Quảng trường chính : -27 DAI TIEN NU, -32 THO REN, -33 SHOP THIEN DINH, -40 WINDY
--   29 TP Thiên Thần      : -29 TONG QUAN THIEN DINH, -30 BANG XEP HANG,
--                           -31 NGUOI GIU CONG, -38 BAC SI THIEN THAN
--   27 Đài tưởng niệm     : -34 AC, -35 QUAN LY THIEN THAN
--   26 Vùng đất phong ấn  : -36 KE THU THAP, -37 DU CON THIEN THAN
--   30 Khu vực bang hội   : -19 Nhà nghiên cứu
--   (không map nào)       : -23 HUONG DAN
--
-- Muốn NPC nói chuyện lại (khi đã gán chức năng): đặt type về giá trị cũ — 0 cho -19,
-- 1 cho các NPC còn lại.

UPDATE `npc` SET `type` = 9
WHERE `npcId` IN (-19, -23, -27, -29, -30, -31, -32, -33, -34, -35, -36, -37, -38, -40);
