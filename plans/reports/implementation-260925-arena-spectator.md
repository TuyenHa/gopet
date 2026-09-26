# Arena spectator and tournament execution

Scope: finish phase 03 of `plans/260917-1812-pvp-arena-battle-parity` and verify registration, pairing and multiple rounds. Pet League is not part of this change.

Ruling: work in the supplied managed workspace on `fix/performance`; preserve the two untracked Pet League documents and do not mutate read-only git metadata.

Baseline: 943 Net tests pass. Full verify fails before edits: generated opcode mismatch; BlacksmithRepairPopupView.cs and PetEquipHandlerTests.cs exceed the existing line budget; Unity assemblies are absent at the configured Editor path. No Unity process or local Gopet process found. Docker is available outside the sandbox but no Gopet database is running.

Actor lookup: `MapScene.TryGetAvatarTransform(int)` and `PetLayer.PetOf(int)`. Spectator displays use the pet transform when available, otherwise the owner's transform, and wait up to two seconds for missing actors.

Ruling: reuse world pet sprites and existing skill/float-text effects with compact world-space vitals, rather than duplicate BattlePetCard sprites. The own-battle overlay remains separate.

## Progress

- [x] Spectator state tests fail then pass.
- [x] World rendering, delayed spawn, cap and cleanup implemented; visual verification pending.
- [x] Arena regression tests fail then pass; in-process multi-round verification.
- [x] Net/server tests and available compile checks.
- [x] Record explicit environment blocker evidence for Unity PlayMode and visual/live verification.
- [x] Review changes and update phase documentation with verified scope.

## Findings and changes

- Registration previously charged and enrolled a player without a live pet or
  even a lobby place. Reproduced by a failing server test; now rejected before
  charging. Eligibility is rechecked before pairing to avoid missing/dead pets.
- `CanJournalism` only checked remaining time. A fighting/stopped event with a
  residual timer accepted registration. Reproduced and fixed by checking event
  state as well as the timer.
- The initial suspicion that ArenaData did not store its slot was incorrect.
  A passing regression confirmed the assignment already existed; it was not changed.
- Spectator rendering handles normal/critical attacks and skill effects. An
  independent review caught missing normal-attack effects and overlay-sized
  effect bounds; both were corrected. Follow-up review caught world/local coordinate
  mixing under the scaled effect parent; actor travel and flame trajectories now
  transform their vectors correctly. Final review found no concrete blocker in
  the diff. Five lifecycle tests and four parameterized scale cases await Unity.

## Verification

- `dotnet test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj --no-restore`:
  950 passed, zero failed, zero skipped (943 baseline plus seven new cases).
- `dotnet run --project tests/GServer.Performance.Tests/GServer.Performance.Tests.csproj --no-restore -p:WarningLevel=0`:
  34 passed, including three new arena cases. Tournament fixture runs real menu
  registration, ArenaEvent.NextTurn, ArenaMap.AddBattle, PetBattle startup/result,
  ArenaData scoring and ArenaEvent termination. Four players produce two pairs
  then a final, and three total winner points. Outcomes use controlled HP changes.
- The fixture replaces map asset/transfer traffic with in-memory placement;
  it does not certify login, database persistence, real clients or visual map transfer.
- `run-playmode-tests.ps1` exits with missing
  `D:\Unity Editor\6000.5.4f1\Editor\Unity.exe`. No screenshots or PlayMode pass claimed.
- Baseline full `verify.ps1` has unrelated failures: generated opcode mismatch,
  two pre-existing overlength files and missing Unity assemblies. These are not
  silently repaired or counted as passing in this scoped change.

## Remaining certification

1. Provide installed Unity Editor via `UNITY_EXE` and its assemblies via
   `UNITY_MANAGED_DIR`, open the project so package assemblies are generated.
2. Run SpectatorBattleTests, SpectatorEffectScaleTests and existing battle
   lifecycle PlayMode tests. Unity runtime compilation also remains unverified.
3. Visually inspect five pairs, normal/critical/skill effects, join-in-progress,
   actor departure and place changes on desktop/mobile resolutions.
4. Run four real clients against the local Gopet server through registration,
   pairing, two rounds and return to map 19. No Gopet DB/server was running here;
   Docker inventory contained unrelated services only.

No production database, binary JAR or Pet League gameplay was changed.
