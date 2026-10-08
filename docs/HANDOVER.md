# Handover

Current work outside a goal. Active goals keep their own handovers in
docs/goals/; queue items and findings stay in their own files.

## Upstream update (2026-10-08)

Commit e7016d7 adopts rules 11884c7, checker 2.8.0, RefurbishedDinosaurs
11.0.0, executable-reader 2.4.0 and engine 13.3.0; template main is still the
adopted 0b9ab9c. It corrects 107 DOSBox.exe range ends in place under the
owner's decision in docs/DECISIONS.md. Assetless Test.ps1 passed with 715
.NET tests. Not pushed. The protocol-work worktree is still at 4fcf09c: merge
main into it before resuming Q-EXE-009, and write range ends half-open
(research-item skill). docs/VALIDATION.md's 64 local `artifacts/`
log citations were removed; AGENTS.md now forbids them. About 230 more remain
in the audit documents, some naming uncommitted driver scripts.

## Today’s wrap-up (2026-10-08)

Completed protocol research and current template updates are integrated on main.
The ongoing goal remains documented in docs/goals/protocol-work.md; resume
Q-EXE-009 in its isolated worktree. Final research entry: FND-EXE-144.
Combined assetless Test.ps1 -NoRestore passed, including all .NET tests; log:
UserContent/worktrees/protocol-work/artifacts/wrapup-merge-full-gate-fixed-fixtures.log.
Main's indexes and parity were regenerated. Documentation checks retain their
reported argument-count skips. No original runtime was started.
Toolkit issue 353 has the external-archive citation case; template issue 90
records scheduled-generation fixture assumptions. The owner explicitly asked
to wrap up for today and push main after completion. Do not start another item.
Hooks remain enabled; use an absolute GIT_INDEX_FILE for main commits when the
staged snapshot runs Git from a temporary directory. Preserve unknown and
reusable processes; no confirmed task orphan was found during integration.

## State

- Stage: Slices; slices 2 and 3 remain in progress. Intake, Runtime access and
  Survey have ended (docs/BOOTSTRAP-CHECKLIST.md).
- Last gate: 2026-10-08, `./tools/Test.ps1` passed. Run it with PowerShell 7
  (`artifacts/pwsh7/runtime/pwsh.exe` here; Windows PowerShell 5.1 now refuses
  it) and with `GAME_DIR` and `NoDefaultCurrentDirectoryInExePath` unset, since
  both are set machine-wide for other projects. Template issue 81 asks for a
  per-project fix.
- Sandbox validation: set process-local TEMP/TMP to the executing account's
  accessible temporary directory (template issue 82). In this session Git's
  shell could not create the hook snapshot there: set TMPDIR inside Git Bash
  to the writable `artifacts/hook-tmp` directory before committing. Keep the
  hook enabled. Validation and failure logs are listed in docs/VALIDATION.md.
- Upstream adoption state: docs/TEMPLATE-ACCEPTANCE.md and
  docs/LATEST-RELEASE-GAP-AUDIT.md.
- Required asset pack revision is 36. The owner's revision-35 pack under
  LocalAppData is rejected until play.bat re-extracts it.
- Native runtime remains owner-only. Agents never run the original or take the
  run lock.

## Unfinished

- Commit-message repair awaits owner approval: the validation commit fa0d8d1
  has the shortened title `Record`. A concurrent research commit 4e5d4e8 was
  accidentally amended as 290a177 with the validation message and lost its
  original Spec trailer. Its original message remains available through
  `git show -s --format=%B 4e5d4e8`; the research tree is unchanged. Automatic
  approval review rejected a message-only rebase because it rewrites shared
  history. Do not retry without approval or amend the current HEAD.

## Blockers

- Live signing, repository signing-environment branch protection and
  high/mixed-DPI original-window capture remain separate acceptance checks. No
  release was performed.
- Q-PARTY-001 (shipped-party live session) waits on the owner accepting
  docs/live-sessions/shipped-party.md.

## Next

1. Q-PARTY-013, Q-PARTY-011 (56 indirect calls left), then Q-PARTY-012, for
   RULE-PARTY-006 (slice 2). Q-PARTY-012 reads the shared resource reader,
   which config-static's CONFIG findings also cover; coordinate with that goal
   before adding entries in its areas.
2. Q-PARTY-002, Q-PARTY-003, Q-PARTY-004 and Q-PARTY-006 (slice 2).
3. Q-UI-005 and Q-SAVE-001 for SCR-UI-013 and SCR-UI-014; Q-UI-002 for SCR-UI-007.
4. Q-EXE-002 and Q-EXE-001.
5. Configure ES_CERTIFICATE_THUMBPRINT and main-only release-signing deployment
   branches before a signed release.

## Upstream requests from this work

- refurbished-dinosaurs issue 77: a committed build listing record, directory
  exclusions and the `CD:` path form. Toolkit issue 297 waits on it; this
  project's reconciliation is `tools/evidence/build-listing.mjs`.
- refurbished-dinosaurs-template issue 81: machine-wide `GAME_DIR` and local
  stores inside the install directory.
- refurbished-dinosaurs-template issue 82: inherited owner TEMP/TMP causes
  Java canonical-path access failure under sandbox validation.
- refurbished-dinosaurs-toolkit issue 322: a reachability command over the
  call graph, with unresolved sites listed.
- refurbished-dinosaurs-toolkit issue 323: an inventory check reporting call
  targets missing from a committed inventory.
- refurbished-dinosaurs-toolkit issue 319: comment adding a one-sentence
  Parameters section case.

## Shared runtime migration

See [migration status](SHARED-RUNTIME-MIGRATION.md).
