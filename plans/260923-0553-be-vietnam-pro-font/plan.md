---
title: "Áp dụng font Be Vietnam Pro cho UI Unity"
description: "Thay font Arial/LegacyRuntime của uGUI Text bằng Be Vietnam Pro, giữ fallback, không chuyển TMP"
status: pending
priority: P2
branch: "dev_plan"
tags: [unity, ui, font]
blockedBy: []
blocks: []
created: "2026-09-22T23:03:40.852Z"
createdBy: "ck:plan"
source: skill
---

# Áp dụng font Be Vietnam Pro cho UI Unity

## Overview

UI Unity dựng bằng code, toàn bộ là `UnityEngine.UI.Text` (legacy), font lấy từ một điểm duy nhất `UiBuilder.BuiltinFont()` (LegacyRuntime/Arial). Arial xử lý dấu chồng tiếng Việt kém ở cỡ 11–14. Plan: nhập Be Vietnam Pro (OFL), đổi điểm lấy font, có fallback, rồi chỉnh bố cục chỗ bị tràn.

**Ngoài phạm vi (YAGNI):**
- Chuyển sang TextMeshPro (~71 file, không cần cho mục tiêu này).
- Font bitmap jar `JarFont`/`JarNameLabel` (tên trên map) — giữ nguyên.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Import font assets](./phase-01-import-font-assets.md) | Pending |
| 2 | [Font loader trong UiBuilder](./phase-02-font-loader-trong-uibuilder.md) | Pending |
| 3 | [Font dam that cho text Bold](./phase-03-font-dam-that-cho-text-bold.md) | Pending |
| 4 | [Kiem thu va chinh bo cuc](./phase-04-kiem-thu-va-chinh-bo-cuc.md) | Pending |

Phase 1→2→4 là lõi. Phase 3 (Bold thật) tách riêng, có thể hoãn nếu faux-bold chấp nhận được.

## Key Facts (từ scout)

- `GopetUnityClient/Assets/Scripts/Runtime/UI/UiBuilder.cs:42` — `BuiltinFont()`; file 97 dòng.
- 60 file gọi `BuiltinFont()` (87 lời gọi trực tiếp); `GopetBootstrap.cs:80` inject font cho login/UiRoot/HUD.
- 105 chỗ `fontStyle = FontStyle.Bold`, 4 chỗ Italic.
- Cỡ chữ phổ biến 11–14 (vài chỗ 9–10); `CanvasScaler` ref 720×1280.
- Test tham chiếu font: `Assets/Tests/PlayMode/UiContrastTests.cs:41` + nhiều test gọi `UiBuilder.BuiltinFont()`.
- Unity 6000.5.4f1, `com.unity.ugui` 2.5.0, Android minSdk 26.

## Dependencies

- Không chặn/bị chặn. Liên quan nhẹ: `260913-jar-unity-parity-audit-and-certification` (mục Audio/visual "JAR skin/font") — plan này KHÔNG đụng font bitmap jar, nhưng đổi diện mạo UI popup → ghi chú khi chứng nhận parity.

## Decisions

- 2026-09-23: Chữ đậm dùng **SemiBold (600)** — không nhập file Bold (700).

## Unresolved Questions

1. Có muốn làm Phase 3 ngay hay chấp nhận faux-bold trước?
