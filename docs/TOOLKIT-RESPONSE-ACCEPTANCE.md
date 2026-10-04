# Toolkit response acceptance

## 2026-10-04: issues 108, 109 and 110

The owner responses on toolkit issues
[108](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/108),
[109](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/109) and
[110](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/110)
were checked against the consumer, not treated as closure from toolkit tests.
This is a tooling acceptance batch; no game behavior or spec status changes.

### Shared runtime: issue 108

Dark Sun pins Core and LegacyFormats exactly at 2.0.0 in Directory.Build.props.
The committed project and packaging lock files record the NuGet content hashes.
The migration commits are cc48ccf (content verification) and 2496721 (settings,
consumer PR 4). The packages use XXH3-128 asset identities; source verification
uses LegacyFormats. No vendored implementation replaces the package imports.

| Slice | Consumer disposition and positive control |
|---|---|
| R1 portable paths | Adopted. Extractor inventory, region catalog, startup extraction and Resources asset references call PortableAssetPath.Relative. SourceCorpusInventoryTests and StartupAssetExtractorTests exercise known source paths and source-mapped extracted output. |
| R2 persistence/settings | Adopted. LaunchSettingsStore uses RecoverableFile.Write, ReadBounded and SettingsRecovery.Select, retaining the game's serializer and field admission as the runtime guide prescribes. LaunchSettingsStoreTests cover round trips, backup retention, corrupt-primary recovery and rejected oversized/version-invalid input. |
| R3 PCM/audio/WAVE | No PCM/WAVE conversion or playback consumer was found in src or the Inspect tooling. Not claimed as adopted or tested. |
| R4 input | Not migrated: Game retains previous KeyboardState and local chord/edge helpers. A separate implementation batch must preserve existing input semantics while adopting InputState/InputBindings and run the game's binding controls. The current research-side goal does not authorize changes to src. |

The full consumer Test.ps1 -NoRestore gate passed. Its log is
artifacts/issue108-root-gate.log. Toolkit release status is resolved, but the
consumer-wide issue is not closed while R4 remains outstanding. No new consumer
PR or push was made in this batch.

### Transfer inputs and segment reporting: issue 109

Upstream accepts Gap 39 as closed. Its existing installed-wheel regression
controls and the complete reporting-contract audit remain authoritative.
Gap 37 remains open. Adopted engine is 4.0.0, reader 2.0.0; an earlier issue
comment saying consumer engine 2.0.0 is historical, not current.

A separate candidate environment installed the published engine 6.1.1 wheel,
SHA-256 1b88ff60e41f926f96cc9d557577e25ee1b018d47b50bb6d1e08194a0b3c5e1d,
and published reader 2.1.0. These versions were not substituted into the
project's pins. Both use prepared protocol 3; runtime dependencies remain
Capstone 5.0.7, pypcode 4.0.0 and xxhash 4.0.1. The source was checked by
Bootstrap-Project.ps1 -ValidateFactsOnly and each query's XXH3 identity guard.
No original execution or emulation occurred.

The supported producer recipe is now the next step: one connected trace before
the producers; failing that an evidenced register-only entry; or returned
registers and bounded preserved frame bytes for unread calls. A control's
assume input does not steer paths. Entry-memory inputs would need a separate
ADR and protocol migration; they are not required or promised by this response.

Candidate controls used effects, explicit DS/SS/SP, unknown initial memory,
200 steps per path, 16 paths, 5000 total steps and visit limit 4:

- The complete FND-CONFIG-192 body again reaches four hardware sites only on
  stopped paths. Placement remains unresolved; step/visit stops and dropped
  paths remain explicit. Its serialized report is 7778355 bytes.
- An entry at the complete FND-CONFIG-193 initializer, declaring both producer
  and consumer regions, stops at the initializer's loop visit limit after 44
  steps, before any transfer event. Its serialized report is 54798 bytes.
  A one-step negative control stops without reaching the consumer.
- This is not a connected caller-tree acceptance: declaring two regions does
  not create an edge between them. The recorded startup call precedes later
  requests, but a complete route from that caller to the selected transfer and
  its later slot/count/mask writers is not established by FND-CONFIG-193.
  No number of intervening calls or external-input admission is invented.

Do not raise the old budgets or stitch these reports. The next acceptance
needs a supported connected caller entry and writer/input coverage; the new
producer diagnostic alone does not prove the supported recipe impossible.

### Argument frames: issue 110

The same candidate pair ran arguments on the saved FND-CONFIG-179 caller and
FND-CONFIG-180 consumer configs, with sourceKind mz, returnBytes 4, entry and
evidence-labelled regions. The complete caller retains its existing bounds:
1000 steps, 5000 total steps, 64 paths. The consumer-only config retains
256 steps, 1500 total steps, 16 paths and two explicit balanced-return,
register-preservation call models; unknown memory is never silently preserved.

The known registration call has argumentFrames and an argumentFrameSites row.
The report recognizes the far window (offset 0, width 4), identifier (offset 4,
width 2), and far callback (offset 6, width 4) on the paths that consume them.
The site does not agree: bypass paths leave callback/mask slots unread, and
other paths stop in unread callees. Forwarded window/identifier reads into a
deeper frame are retained. The full caller also retains path-limit gaps.
The consumer-only query has no argument frame for its modeled setters, so it
is not passed off as a caller-to-callee positive control.

Assertions require an actual known call frame and a consumed four-byte callback
grouping, and require the unresolved site to remain disagreed. Gap 35 stays
open; no hand-reading workaround was deleted. Closure requires settled widths
and forwarding controls against adopted packages, not this qualified candidate.

Reproduction: set EVIDENCE_PYTHON to the isolated candidate Python, then run
node artifacts/issue-response-review/run-cases.mjs. Configs and original-derived
reports remain only in GAME_DIR/analysis/reporter-audit/issue-response-review;
the driver and log are ignored local artifacts. The command invokes the
published reader's arguments/effects commands. No proprietary report is posted
upstream or committed.
