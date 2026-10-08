# Shared runtime migration

Bounded settings reads, validated backup writes and per-field recovery provenance.

The migration uses published `6.2.0` NuGet packages for the required shared APIs:
portable paths, recoverable persistence, and PCM/audio with streaming WAVE.
Only packages used by this restoration are referenced. Input support is independent
and is not required by this migration.

Game save payloads, version admission, slot naming, defaults, audio routing, fades, voice limits
and control policies remain local. Synthetic checks establish migration behavior, not parity
with the original game or live device behavior. No original assets entered this change.

Self-contained installers use separate committed runtime/mode lock profiles. Ordinary build
locks and target/mode locks are refreshed together for the release. Profile maintenance names the target runtime explicitly, and CI
rejects a missing or mismatched packaging lock instead of regenerating it.

## Runtime 11.0.0, 2026-10-08

`RefurbishedDinosaursVersion` moves from 10.0.0 to 11.0.0, and the ordinary
and every target/mode NuGet lock are regenerated. The release adds
`RecoverableFile.ReadAndRepair` and `Restore`, makes `Read` throw
`FileGenerationsUnreadableException` when neither generation loads, and
promotes a written file with `File.Replace`. `LaunchSettings` calls only
`Write` and `ReadBounded` and reads both generations itself through
`SettingsRecovery`, so no consumer code changes. On Windows a rewritten
settings file now keeps the replaced file's creation time and attributes.
The pack revision remains 36.

## Latest release acceptance, 2026-10-05

Runtime 6.2.0 is registry-verified and pinned exactly through Directory.Build.props.
Core, LegacyFormats and Media.Fli consumers build and pass the canonical gate.
Reviewed the breaking overlay-record normalization, Latin-1 volume identity,
CDDA long-sector signatures and shared image-limit contracts. No consumer uses
removed BMP pixel-limit constants or parses the old volume diagnostic spelling.
No required extracted asset contract changed; pack revision remains 36.

Research tooling adopts engine 8.1.1 with its published wheel SHA-256; reader
2.1.0 and checker 0.2.0 already match the latest registry releases. Wheel,
sdist, release-tag and installed production bytes agree. The full released
suite, including the installed-engine Node bridge and test-only Unicorn controls,
passes; Unicorn is not added to production requirements and no original is emulated.

Issue response acceptance is in TOOLKIT-RESPONSE-ACCEPTANCE.md. Remaining source
acceptance is tracked by project issue 5; focused toolkit issues remain open.

## Latest registry adoption, 2026-10-05

Runtime 6.6.0 and engine 9.1.0 supersede the preceding adoption. Reader 2.1.0
and checker 0.2.0 remain the latest registry releases. Intervening runtime
additions are opt-in short-stream PCX filling, static MsvcRandom transitions,
InstallShield version-zero cabinets and InstallShield 3 archives. Existing
consumer behavior and required derived assets do not change; the pack revision
remains 36. Ordinary and every target/mode NuGet lock are regenerated.

Engine 9.0.0 changes Ghidra exact-start diagnostics and bounded-flow coverage;
9.1.0 adds lastWriter checkpoint memory probes. Packaged scripts retain their
shared helper directory. No local parser consumes the changed window/export
format. Exact wheel/sdist/tag/installed bytes and the full released suite pass,
including synthetic Unicorn oracle
and installed-engine Node bridge tests. Unicorn remains test-only.

Gap-by-gap disposition is in LATEST-RELEASE-GAP-AUDIT.md. Source controls remain
local under GAME_DIR; no original executable was run or emulated.
