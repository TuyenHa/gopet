-- Nạp dữ liệu XÃ HỘI mẫu cho tài khoản test `gopettest` (user_id 1458, player 102 "kzhd9x"):
-- hộp thư, danh sách bạn, lời mời kết bạn đang chờ và danh sách chặn.
--
-- LÝ DO: dump gốc để bốn cột này rỗng, nên không thể bấm thử hộp thư / danh sách bạn
-- trên client Unity. Đây là dữ liệu để DEV THỬ GIAO DIỆN, không phải dữ liệu game thật.
--
-- CÁCH DÙNG:
--   docker exec -i gopet-mariadb mysql -uroot -p<pass> --default-character-set=utf8mb4 \
--       gopettae_tae2 < migration-260922-seed-social-data-gopettest.sql
--
-- BẮT BUỘC: chạy khi gopettest ĐANG OFFLINE. Server giữ PlayerData trong RAM và ghi đè
-- cả bốn cột này lúc lưu, nên seed trong khi tài khoản đang đăng nhập sẽ bị mất trắng.
--
-- CÁC RÀNG BUỘC ĐÃ ĐỐI CHIẾU VỚI CODE SERVER:
--   * ListFriends / RequestAddFriends / BlockFriendLists chứa `user_id`, KHÔNG phải `player.ID`
--     — MenuController.sendMenu.cs:221 tra `... FROM player WHERE player.user_id IN (...)`.
--   * letters phải SẮP XẾP TĂNG DẦN theo LetterId: PlayerData.FindLetter() dùng BinarySearch.
--   * LetterId là số nguyên >= 10, duy nhất trong hộp thư (Utilities.BinaryObjectAdd).
--   * Type: 1 = FRIEND, 2 = ADMIN, 3 = EVENT (hằng số trong Data/User/Letter.cs).
--   * Xuống dòng trong Content viết \\n chứ không phải \n: MySQL tự giải nghĩa dấu \ trong
--     chuỗi literal, viết \n sẽ thành ký tự xuống dòng THẬT nằm giữa chuỗi JSON và làm
--     JSON_VALID() = 0 (đã vấp đúng lỗi này lúc chạy lần đầu).
--   * IsMark = true nghĩa là ĐÃ ĐỌC; GameController.sendHasLetter() báo chấm đỏ khi còn
--     thư IsMark = false. Seed để lại 3 thư chưa đọc cho thấy badge hoạt động.

-- Hộp thư: 6 thư, đủ cả 3 loại, 3 chưa đọc + 3 đã đọc.
UPDATE `player` SET `letters` = '[
  {"LetterId":10241,"IsMark":false,"Type":2,"Title":"Chào mừng đến với goPet",
   "ShortContent":"Quà tân thủ đã được chuyển vào hành trang của bạn.",
   "Content":"Xin chào!\\nCảm ơn bạn đã tham gia goPet. Quà tân thủ đã được chuyển vào hành trang.\\nChúc bạn chơi game vui vẻ."},
  {"LetterId":10358,"IsMark":false,"Type":3,"Title":"Sự kiện Trung Thu",
   "ShortContent":"Săn lồng đèn ở Thành Phố Linh Thú để đổi quà.",
   "Content":"Từ hôm nay đến hết tuần, quái ở Thành Phố Linh Thú sẽ rơi Lồng Đèn.\\nGom đủ 10 lồng đèn rồi gặp NPC sự kiện để đổi quà."},
  {"LetterId":11470,"IsMark":false,"Type":1,"Title":"smoke151505 gửi bạn",
   "ShortContent":"Tối nay đi Đấu Trường không?",
   "Content":"Tối nay 8h mình lập đội đi Đấu Trường, bạn tham gia nhé!"},
  {"LetterId":12066,"IsMark":true,"Type":1,"Title":"tuyenhavan gửi bạn",
   "ShortContent":"Cảm ơn vụ pet hôm qua nha.",
   "Content":"Cảm ơn bạn đã cho mượn pet hôm qua. Hôm nào rảnh mình trả lễ."},
  {"LetterId":13315,"IsMark":true,"Type":2,"Title":"Nhắc nhở bảo mật",
   "ShortContent":"Không chia sẻ mật khẩu cho bất kỳ ai.",
   "Content":"Ban quản trị KHÔNG BAO GIỜ hỏi mật khẩu của bạn.\\nHãy tự bảo vệ tài khoản của mình."},
  {"LetterId":14892,"IsMark":true,"Type":3,"Title":"Điểm danh hằng ngày",
   "ShortContent":"Bạn đã điểm danh 5 ngày liên tiếp.",
   "Content":"Bạn đã điểm danh 5 ngày liên tiếp. Đủ 7 ngày sẽ nhận thưởng lớn.\\nĐừng bỏ lỡ nhé!"}
]'
WHERE `user_id` = 1458;

-- Bạn bè: tuoithoavt1 (1), tester (2), smoke151505 (1459), tuyenhavan (1462).
-- Lời mời đang chờ: pvp13534715 (1463), pvp13535284 (1464).
-- Chặn: 67657567 (1465).
UPDATE `player`
SET `ListFriends`       = '[1,2,1459,1462]',
    `RequestAddFriends` = '[1463,1464]',
    `BlockFriendLists`  = '[1465]'
WHERE `user_id` = 1458;

-- Bạn bè hai chiều: phía kia cũng phải thấy gopettest, nếu không danh sách bạn sẽ
-- lệch ngay khi đăng nhập bằng mấy tài khoản còn lại.
UPDATE `player` SET `ListFriends` = '[2,1458]' WHERE `user_id` = 1 AND `ListFriends` = '[2]';
UPDATE `player` SET `ListFriends` = '[1458]' WHERE `user_id` IN (2, 1459, 1462) AND `ListFriends` = '[]';
