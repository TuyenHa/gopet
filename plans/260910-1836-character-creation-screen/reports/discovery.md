# Discovery report

## UI contract

`CharacterCreationView` is a runtime-created uGUI root composed of:

- `CharacterPreviewPanel`: two side-by-side presets, gender `0` (Nam) and `1` (Nữ), rendered through `AvatarAppearance.Create`.
- `CharacterNameInput`: an `InputField` limited to 20 characters and validated through `AuthRules.IsValidCharacterName`.
- Submit/cancel buttons and a notice/error label.

The root emits exactly `Submitted(sbyte gender, string name)` and `Cancelled`.

## Assets and styling

| Use | Existing source | Notes |
| --- | --- | --- |
| Male/female avatar | `JarSkin.Bank("avatar", index)` via `AvatarAppearance.Build` | Gender-specific parts are selected by `gender != 0`; no new sprite asset is introduced. |
| Name labels | `JarNameLabel` + `JarFont` | Bitmap-style labels “Nam” and “Nữ” below each preview. |
| UI font | `UiBuilder.BuiltinFont()` | Existing LegacyRuntime/Arial fallback. |
| Colors | `UiBuilder.JarBackground`, `Panel`, `Field`, `ButtonFace`, `TextMain`, `TextMuted` | Reuses login and HUD palette. Highlight is `#FFCC33`. |
| Rounded controls | `RoundedUiSprite.Apply` | Existing generated 9-slice sprite. |

`AvatarAppearance.Create` only creates child transforms and sprite renderers; it does not read map/session state. Its only runtime dependency is the already-loaded `JarSkin` resource bank, so it can be created by the login screen. Because the renderers are world-space, each preview slot owns a world transform while an invisible uGUI click surface handles touch/mouse input.

## Layout mockup

```text
┌──────────────────────────────────────────────┐
│              Chọn nhân vật                   │
│                                              │
│       [  avatar Nam  ] [  avatar Nữ  ]       │
│            └ Nam ┘       └ Nữ ┘              │
│                                              │
│      Tên nhân vật (5-20, a-z 0-9)            │
│      [____________________________]           │
│      inline validation / server notice       │
│                                              │
│       [ Tạo nhân vật ]   [ Quay lại ]        │
└──────────────────────────────────────────────┘
```

No wire-format or `LoginFlow` protocol changes are required. The planned source files are:

- `Assets/Scripts/Runtime/UI/CharacterCreationView.cs`
- `Assets/Scripts/Runtime/UI/CharacterPreviewPanel.cs`
- `Assets/Scripts/Runtime/UI/CharacterNameInput.cs`
