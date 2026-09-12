# Ngoại lệ giới hạn 200 dòng

Release gate vẫn áp dụng giới hạn 200 dòng cho mọi file mới. Các file dưới đây là nợ kỹ
thuật đã tồn tại ở baseline trước đợt hoàn thiện parity JAR ngày 2026-09-12 và được giữ
nguyên để tránh trộn một đợt refactor giao diện/world diện rộng vào thay đổi protocol:

- Bootstrap/network lifecycle: `GopetBootstrap.cs`, `GopetClient.cs`.
- UI cũ: `GenericMenuView.cs`, `LoginFormView.Actions.cs`, `LoginFormView.cs`,
  `LoginScreens.cs`, `ShopPopupView.cs`, `UiRoot.cs`, `CharacterHud.cs`, `CurrencyBar.cs`.
- World cũ: `GameSession.cs`, `MapPortalView.cs`, `MapRenderer.cs`, `MapScene.cs`.
- Test cũ: `LoginFormViewTests.cs`.
- Thuật toán liền khối đã có ngoại lệ từ trước: `Tea.cs`.

Các phần parity mới được tách thành partial/component riêng và đều dưới 200 dòng. Danh
sách này là allowlist cố định, không phải wildcard; file mới vượt giới hạn vẫn làm
`verify.ps1` thất bại. Việc tách các file legacy sẽ được thực hiện trong một đợt refactor
riêng có snapshot UI để tránh hồi quy không liên quan.
