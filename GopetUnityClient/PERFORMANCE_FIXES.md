# Performance fixes ? 2026-09-23

## Image pipeline

- Coalesces each image path through disk lookup, network response and decode, sharing one texture among consumers.
- A serialized background disk worker performs directory creation, reads, writes and disk eviction. Main-thread ticks only consume completed results and decode PNGs (default: four per frame). Disposal skips queued I/O without blocking the Unity thread.
- Bootstrap disposes cache resources on scene teardown. Sprite slices are keyed by texture identity and released with their texture; teardown clears remaining slices.
- The configurable 64 MiB texture budget remains soft to protect visible images. It evicts least recently accessed textures without live owners, at most once per second. Active images can exceed the budget. Counts estimate RGBA pixel storage, not all driver/object overhead or queued PNG buffers.
- Runtime image consumers identify their image slot or actor. Destroyed/released owners allow eviction. Binding generations suppress stale responses, including reuse of the same path. The compatibility overload without an owner pins images until disposal.
- Decoded textures are non-readable to avoid retaining a CPU pixel copy.

## UI and cleanup

- Pet lists realize only visible rows plus overscan, and reuse the complete row hierarchy while scrolling. Bind, resize and scroll events update rows; no per-frame layout rebuild. Receive/Shop/Top modes share the pool, keep absolute selection indices, and release image owners on recycling.
- Removed BattleSkillIconKey, EmoteController, JarFont and the redundant ActionLabel wrapper. The portal test now checks current TextMesh rendering instead of legacy bitmap metrics.
- Split oversized classes into coherent partials without widening the line-limit allowlist.
- Generated opcode/string checks accept physical CRLF/LF differences while retaining content checks. All 187 server opcode values remain unchanged.
- Preserved the intentional newMapData/158.png artwork through an explicit source override with source/output hashes, rather than overwriting it.
- verify.ps1 discovers the repo-local .NET SDK when PATH lacks dotnet and reports missing Unity prerequisites explicitly.

## Validation

- Full .NET suite: **942 passed, zero failed** (includes three disk-worker tests).
- verify.ps1: **7/10 stages passed**: opcode/protocol, assembly references, assets/strings, netstandard2.1 compilation, unit tests, LiveSmoke compilation, and file-size gate.
- Remaining three stages (Runtime, Editor and PlayMode compilation) cannot run: Unity assemblies are missing at the configured Editor path. PlayMode execution is also unavailable. Set UNITY_MANAGED_DIR / UNITY_EXE to the installed project Editor and initialize the project's Library to run them.
- C# 9 syntax checked with the SDK's Roslyn parser: **578 files, zero syntax errors**. This does not replace compilation against Unity assemblies.
- PNG override regression checks, generated-string regression checks and git diff --check passed.
- Added fourteen Unity test cases covering image sharing/budget/disposal, owner lifecycle, stale callbacks, pool reuse, original selection indices, mode rebinding and viewport resizing. They are written but **not executed** locally.
- No FPS, GPU-memory or on-device profiler results are claimed. Profile repeated navigation, uncached shared images, fast scrolling and logout/login cycles in Unity/on the target device.
