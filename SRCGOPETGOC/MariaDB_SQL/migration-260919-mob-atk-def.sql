-- Thêm chỉ số tấn công / phòng thủ riêng cho quái.
--
-- VẤN ĐỀ: bảng `gopet_mob` chỉ có str/agi/_int/hp. Quái không bao giờ chạy
-- `Pet.applyInfo()` (hàm đó chỉ có ở lớp Pet) nên trường atk/def của nó luôn = 0 và
-- chỉ còn công thức gốc `GameObject`:
--     getAtk() = atk + str/3 + 5
--     getDef() = def + agi/3
-- Trong khi pet được nhân hệ số rất lớn: atk = str*30, def = agi*20, cộng trang bị.
-- Hệ quả: quái cấp cao nhất (lvl 110) vẫn thua xa DEF của một pet có trang bị, mà
-- sát thương bị kẹp sàn 1 (`PetBattle.cs` nhánh `sum = Math.Max(sum - def, 1)`),
-- nên quái gõ mãi cũng chỉ trừ đúng 1 máu.
--
-- CÁCH SỬA: cho quái bộ chỉ số riêng trong DB. `MobLvInfo.atk` vốn đã là `int?` và
-- `Mob.getAtk()` đã ưu tiên nó — chỉ thiếu dữ liệu. Thêm nốt `def` cho đối xứng.
-- Cột để NULL thì quái quay về hành vi cũ, nên đổi ngược lại được.
--
-- SỐ KHỞI ĐIỂM: neo theo `hp` của chính con quái để nhịp trận nhất quán mọi cấp.
--     atk = round(hp * 0.25)   → pet cần ~4 đòn mới gục nếu không đỡ nổi
--     def = round(hp * 0.06)   → đủ để pet thấy có kháng, không đủ để hoá đá
-- Đây chỉ là mốc xuất phát. Chỉnh cân bằng bằng UPDATE, không cần sửa code.

ALTER TABLE `gopet_mob`
  ADD COLUMN `atk` INT NULL DEFAULT NULL COMMENT 'Tấn công; NULL = dùng công thức str/3 cũ',
  ADD COLUMN `def` INT NULL DEFAULT NULL COMMENT 'Phòng thủ; NULL = dùng công thức agi/3 cũ';

UPDATE `gopet_mob`
SET `atk` = ROUND(`hp` * 0.25),
    `def` = ROUND(`hp` * 0.06);
