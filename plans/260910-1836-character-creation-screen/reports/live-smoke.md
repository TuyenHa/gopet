# Character creation live-smoke

Date: 2026-09-10

## Automated checks

- Runtime Unity compatibility compile: PASS.
- PlayMode compile compatibility: PASS.
- Unit tests: PASS (575 tests).
- Asset/protocol/asmdef checks: PASS.

The repository-wide `verify.ps1` size gate still reports pre-existing files over 200 lines
(`LoginFormView`, `ShopPopupView`, `UiRoot`, `GameSession`, and others); this change's three new
runtime files are each below 200 lines.

## Manual flow checklist

The end-to-end smoke requires the Unity Editor/build and the server container to be available at
the same time. Run from `GopetUnityClient` with the Editor closed for batchmode:

1. Start the server (`docker compose up` from the server compose directory).
2. Start the Unity client and register a fresh account.
3. Log in; verify the two avatar previews and that the male preset is selected initially.
4. Enter a valid lower-case name (5–20 `a-z0-9` characters), select Nữ, and submit.
5. Verify the busy notice remains visible while the server closes the socket.
6. Verify automatic reconnect/login and transition to the map.
7. Repeat with an existing character name; verify the name remains in the field and the server
   rejection is shown inline.

This environment did not run the manual Unity/server session, so no screenshot or live log is
claimed here. The deterministic compile and unit results above are the recorded smoke evidence;
attach the Unity Game view and `Logs/playmode-run.log` when the live session is performed.
