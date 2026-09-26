-- database: gopettae_gopet_web
-- Khoá mọi tài khoản còn dùng hash mật khẩu cũ (plaintext/SHA256 — GopetHashHelper.cs:22-26).
-- Mở lại chỉ qua "Buộc reset" (super-admin, webadmin) → bcrypt mới + role=1.
-- Tài khoản `admin` (đã là bcrypt, role=3) không bị ảnh hưởng vì password LIKE '$2%'.
UPDATE user SET role=0 WHERE password NOT LIKE '$2%' AND role<>0;
