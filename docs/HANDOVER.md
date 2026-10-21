# Handover

## State

- Stage: Slices; slices 2 and 3 remain in progress. Active goal:
  docs/goals/full-game-research.md (research side, on main).
- Stop reason: owner requested wrap-up and a push to main on 2026-10-21. The
  goal's Audit condition is unmet; the goal file stays and its Handover holds
  the next items.
- Last full gate: 2026-10-21, ./tools/Test.ps1 -NoRestore passed on 6af0232b,
  including 715 .NET tests. The documentation check passes with known skips.
- Template, toolkit and shared-library pins are recorded in
  docs/TEMPLATE-ACCEPTANCE.md; get the last gate's result from the latest
  section there and from Git.
- Full ./tools/Test.ps1 remains required before every batch commit. Use
  ./tools/Invoke-Validation.ps1 -ChangedSince <base> while a batch is under
  way. Text-size limits come from tools/repository-policy.json.
- Runtime admission is per part in docs/RUNTIME.md. Agents never operate
  GOG's emulator; owner-only visual capture remains binding. Required asset
  pack revision remains 36.

## Remaining research toward slices 2 to 7

Full mapping remains unfinished. Use docs/RESEARCH-CLOSURE.md and the
protocol's priority order; compute current coverage and queue counts with
`npm exec -- standard-coverage` instead of carrying forward old totals. Next
items are in docs/goals/full-game-research.md (Handover).

Q-PARTY-020 is split into Q-PARTY-112 and Q-PARTY-113. Q-PARTY-108, Q-PARTY-107, Q-PARTY-106
and Q-PARTY-111 retain separate admission questions. Q-PARTY-082 and
Q-PARTY-099 remain dependencies. Follow docs/goals/README.md before any
new owner-requested goal claim.

## Local tooling

- Use PowerShell 7 (on PATH). Set EVIDENCE_PYTHON to this checkout's
  artifacts/evidence-python/Scripts/python.exe; the inherited value belongs
  to another project, so do not modify that environment.
- Unset process-local NoDefaultCurrentDirectoryInExePath for validation.
  TEMP/TMP for the full gate must be outside the checkout; hook snapshots can
  use artifacts/hook-tmp. Machine-wide GAME_DIR belongs to another project;
  tools/game-dir.mjs resolves this one through tools/project-config.json.
- If normal project obj files are absent, run
  dotnet restore DarkSunWakeRedux.slnx --locked-mode before -NoRestore.
- FND-UI-054 and docs/RUNTIME.md name all six verified v1.7.5 checkpoints
  under GAME_DIR/captures/rust-dos-regenerated/; final Play is play-ready.
  Historical states remain preserved. The session's emulator-digest guard
  does not establish intrinsic cross-release format incompatibility. Preserve
  FMT-EXE-005 and FND-EXE-520..524 as runtime review prerequisites.
- Run the gate from PowerShell as ./tools/Test.ps1 -NoRestore *> <log> and
  read $LASTEXITCODE; a pipe hides it. Run evidence scripts from files with
  -I: `python -I -` with a heredoc hangs here.
- Ghidra needs its JVM diagnostics directory on a path without spaces
  (C:\Users\kiber\AppData\Local\Temp\dsun-restage-jvm), or it fails with
  "JAVA_HOME invalid". The DSUN restage order is in docs/EVIDENCE-TOOLS.md.
- No emulator session was started. Reusable MSBuild nodes remain
  intentionally alive.

## Unfinished

- Q-PARTY-113: preliminary bounded overlay-213 listings, without a new finding,
  are in GAME_DIR/analysis/party-item-presentation-20261019/. The retained
  census is in GAME_DIR/analysis/party-record-fields-20261018/. No answer is
  claimed from those preliminary listings. FND-PARTY-256's report is in
  GAME_DIR/analysis/party-item-ability-rebuild-20261019/; check its decoder
  version provenance before further citation.
- The installed DSUN.EXE inventory was restaged (FND-EXE-597, FND-EXE-599,
  FND-EXE-601). Reconcile FND-PARTY-255's inventory fragment against it
  before relying on complete ownership; toolkit issue 477 has the case.
- Q-PARTY-082 graph-table queries have no finding yet; local configurations
  are under GAME_DIR/analysis/party-record-heads/. Validate reached entries
  and initialization before treating candidates as runtime state.
- Q-CONFIG-008: do not repeat the bounded incoming inventories
  (FND-CONFIG-114), the literal slot/metadata, selector-base and setup-gate
  writer queries (FND-CONFIG-142, 148, 149), or the MZ, same-segment and FBOV
  incoming queries (FND-CONFIG-196, 198) without new pointer or registration
  coverage.
- Historical commit-message repair awaits approval: fa0d8d1 has title Record;
  research commit 4e5d4e8 was amended as 290a177 and lost its Spec trailer
  (git show -s --format=%B 4e5d4e8). Do not rewrite shared history without
  approval.
- Owner decision pending: AGENTS.md is about 1000 lines (17k tokens read by
  every worker). It has its own 1050-line cap in tools/repository-policy.json
  until the owner decides whether to split it into a short core plus linked
  documents.

## Blockers

- Person-only patched-save and live-session requests remain separate.
  Q-PARTY-001 waits on docs/live-sessions/shipped-party.md.
- Live signing, signing-environment branch protection and high/mixed-DPI
  original-window capture remain separate acceptance checks. No release made.

## Upstream requests

- refurbished-dinosaurs issue 77 and toolkit issue 297: build listing records;
  tools/evidence/build-listing.mjs holds this project's reconciliation.
- Template issues 82 and 90: inherited TEMP/TMP sandbox access,
  scheduled-generation fixture assumptions.
- Template main holds unadopted 4a92b0c (offscreen capture controls) and
  b715696 (per-checkout project inputs, closing issue 81); bde25ad's buffer
  is already applied locally. Adopt them in a tooling batch.
- Template PR 138 adds a generic Ghidra near-target correction with declared
  entries and flows. Once merged, adopt it and keep only this project's
  overlay segment map local. The SLEIGH fix is draft kibertoad/ghidra PR 1.
- Toolkit issue 538: Capstone's synthetic 16-bit 98/99 mnemonic labeling;
  check mode and prefixes directly.
- Toolkit issue 477, comment 6256765999: FND-PARTY-255 inventory fragment.
