/**
 * `gopettae_gopet_web.user` (MyISAM). KHÔNG có `password`/`secretKey` ở đây — hai cột đó
 * chỉ được đọc trong login/reauth và không bao giờ trả ra ngoài hàm. UI chỉ nhận cờ dẫn xuất.
 */
export interface UserRow {
  user_id: number;
  username: string;
  coin: number; // int(11) — không phải bigint
  role: number; // 0 = khoá/chưa kích hoạt, 1 = user, 3 = admin portal cũ
  isBaned: number; // 0 | 1 (có hạn, banTime) | 2 (vĩnh viễn)
  banTime: string; // bigint epoch ms
  banReason: string;
  email: string;
  phone: string | null;
  create_date: string;
  tongnap: number;
}
