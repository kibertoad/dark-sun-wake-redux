# Handover

## State

- Stage: Slices; slices 2 and 3 remain in progress. No active goal.
- Stop reason: owner request. The full-game-research goal was dropped on
  2026-10-19 after its last batch; Git keeps its file at a70411bc. Start no
  research item in this maintenance session.
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

The dropped goal's condition is still the next research target: every parity
row at supported or better (27 rows are below it), and no Static queue item
naming a slice in its Blocks: line (50 remain). These print what is left:

```sh
awk -F'|' 'NF>8 && $2 ~ /`/ {s=$4; gsub(/ /,"",s); if (s!="supported" && s!="established" && s!="validated") print FILENAME, $2, s}' parity/*.md
awk '/^## /{s=$2} /^- Q-/{inq=(s=="Static"); id=$2} inq && /Blocks: *slices? *[0-9]/{print FILENAME, id}' queue/*.md
```

Order: items blocking the lowest-numbered open slice, then items that raise a
parity row below supported, then other slice-blocking items. An item that
cannot close statically moves to the section of the evidence that would
settle it instead of splitting again. Delegated batches follow "Delegated
batches" in docs/goals/README.md.

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
- Checkpoints saved by rust-dos v1.7.2 are refused under v1.7.5; each runtime
  part needs a new v1.7.5 attempt before a run relies on it. Preserve
  FMT-EXE-005 and FND-EXE-520..524 as runtime review prerequisites.

## Unfinished

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
- Template issues 81, 82 and 90: machine-wide GAME_DIR stores, inherited
  TEMP/TMP sandbox access, scheduled-generation fixture assumptions.
- Toolkit issue 538: Capstone's synthetic 16-bit 98/99 mnemonic labeling;
  check mode and prefixes directly.
