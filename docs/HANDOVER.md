# Handover

Current work outside a goal. Active goals keep their own handovers in
docs/goals/; queue items and findings stay in their own files.

## Latest template update

- Applied latest template 403a749 in local commit c6dd863, following 25c5808
  in local commits 7a66e54 and 8683cee.
  Migration and retained contracts: docs/implementation-plans/LATEST-TEMPLATE-SYNC.md.
- Canonical assetless tools/Test.ps1 passed on 2026-10-07; log:
  artifacts/template-403a749-test.log. Documentation checks passed with existing skips;
  log: artifacts/template-403a749-docs.log.
- Current project plans and owner-only runtime restrictions are preserved.
  Capture fixture planning moved to docs/implementation-plans/SYNTHETIC-CAPTURE-READINESS.md.
- No push. The separate goal/protocol-work worktree remains independent.

## State

- Stage: Slices; slices 2 and 3 remain in progress. Intake, Runtime access and
  Survey have ended (docs/BOOTSTRAP-CHECKLIST.md).
- Last gate: 2026-10-07, `./tools/Test.ps1 -NoRestore` passed. Run it with PowerShell 7
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
