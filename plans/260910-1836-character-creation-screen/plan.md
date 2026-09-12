---
title: "Character Creation Screen — visual Nam/Nữ + name (Unity parity với jar)"
description: "Sau khi đăng ký + đăng nhập thành công tài khoản CHƯA có nhân vật, hiện màn hình chọn giới tính (avatar sprite) + nhập tên. Nâng cấp `FormView` hiện có thành view chuyên biệt có preview trực quan."
status: in_progress
priority: P2
branch: ""
tags:
  - unity
  - login-flow
  - character-creation
  - jar-parity
blockedBy: []
blocks: []
created: "2026-09-10T12:02:20.303Z"
createdBy: "ck:plan"
source: skill
---

# Character Creation Screen — visual Nam/Nữ + name (Unity parity với jar)

## Overview

**Trạng thái hiện có (đã reverse):**
- Server: `LOGIN → loginOK → LoadMap → if(playerData==null) createChar()` (Player.cs:521-574). Response `CREATE_CHAR(21) [sbyte 0][int 0][int 0]` = signal "cần tạo char". Client gửi `CREATE_CHAR [UTF name][sbyte gender]`. Server validate `^[a-z0-9]+$` + 5-20 char + duplicate check (GameController.cs:714-742), rồi **đóng kết nối** buộc reconnect+login lại.
- Unity Net layer đã có: `AuthHandler.CharacterCreationRequired` event, `AuthPackets.CreateCharacter(name, gender)`, `AuthRules.IsValidCharacterName`, `LoginFlow.OnCharacterRequired` + `SubmitCharacter` (đã handle timeout + expected-close + auto-relogin).
- Unity UI hiện có: `LoginScreens.BuildCharacterCreation` (line 180) — dùng `FormView` với title "Tạo nhân vật", 1 text field, 2 nút "Nam"/"Nữ". Hoạt động nhưng **thô** — chỉ nút chữ, không có avatar preview.

**Mục tiêu plan:** thay `FormView` bằng view chuyên biệt hiển thị **2 sprite avatar** (nam/nữ, tận dụng `AvatarAppearance` sẵn có), input tên có **validation inline** (khớp `AuthRules`), và feedback rõ khi server từ chối (tên trùng, sai regex). Giao thức KHÔNG đổi.

**Ngoài phạm vi:**
- Đổi wire format tạo char (giữ nguyên UTF+sbyte).
- Skin picker / clothing selector (jar chỉ có 2 preset nam/nữ, giữ nguyên).
- Retry Reconnect logic (đã có ở `LoginFlow`).

**Định hướng:** view mới đặt tại `Runtime/UI/CharacterCreationView.cs` — dựng sprite jar bằng `AvatarAppearance` (giống HUD pet portrait). Không đụng `FormView` (dùng chỗ khác). Wire vào `LoginScreens` thay thế nhánh `BuildCharacterCreation`.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Discovery+UI-Contract](./phase-01-discovery-ui-contract.md) | Complete |
| 2 | [Character Preview Panel](./phase-02-character-preview-panel.md) | Complete |
| 3 | [Name Input & Validation](./phase-03-name-input-validation.md) | Complete |
| 4 | [Screen Wire-up + Reconnect Flow](./phase-04-screen-wire-up-reconnect-flow.md) | Complete |
| 5 | [PlayMode Tests + Live Smoke](./phase-05-playmode-tests-live-smoke.md) | Partial — unit test (625/625) và PlayMode test đã có sẵn/xanh về mã nguồn; chạy PlayMode thật + live-smoke tạo-nhân-vật cần người dùng: đóng Unity Editor, và quyết định có chèn account test vào DB thật hay không (`reports/live-smoke.md`) |

## Thứ tự thực thi

- **Phase 1** trước (chốt contract UI & assets); các phase sau bám theo.
- **Phase 2 và 3** song song được (preview độc lập với input).
- **Phase 4** chặn bởi 2+3 (cần view lắp xong).
- **Phase 5** chạy cuối (test flow đầy đủ, live smoke).

## Ràng buộc

- **KHÔNG** thay đổi wire format `CREATE_CHAR` — server đọc `readUTF() + readsbyte()`.
- Rule 200 dòng/file: view mới phải chia hợp lý (preview + input + root ≈ 3 file nhỏ).
- Validation client-side **phải** khớp `AuthRules.IsValidCharacterName` (regex `^[a-z0-9]+$`, 5-20 ký tự).
- Đóng kết nối sau create là **behavior đúng** của server — không "fix"; chỉ hiển thị "Đang kết nối lại…" và để `LoginFlow.Resume` auto-relogin.

## Dependencies

Không depend plan nào; tận dụng hạ tầng có sẵn (`AuthHandler`, `LoginFlow`, `AvatarAppearance`).
