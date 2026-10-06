# Handover

Current work outside a goal. Active goals keep their own handovers in
docs/goals/; queue items and findings stay in their own files.

## State

- Stage: Slices; slices 2 and 3 remain in progress. Intake, Runtime access and
  Survey have ended (docs/BOOTSTRAP-CHECKLIST.md).
- Last gate: 2026-10-06, `./tools/Test.ps1` passed. Run it with PowerShell 7
  (`artifacts/pwsh7/runtime/pwsh.exe` here; Windows PowerShell 5.1 now refuses
  it) and with `GAME_DIR` and `NoDefaultCurrentDirectoryInExePath` unset, since
  both are set machine-wide for other projects. Template issue 81 asks for a
  per-project fix.
- Upstream adoption state: docs/TEMPLATE-ACCEPTANCE.md and
  docs/LATEST-RELEASE-GAP-AUDIT.md.
- Required asset pack revision is 36. The owner's revision-35 pack under
  LocalAppData is rejected until play.bat re-extracts it.
- Native runtime remains owner-only. Agents never run the original or take the
  run lock.

## Unfinished

None.

## Blockers

- Live signing, repository signing-environment branch protection and
  high/mixed-DPI original-window capture remain separate acceptance checks. No
  release was performed.
- Q-PARTY-001 (shipped-party live session) waits on the owner accepting
  docs/live-sessions/shipped-party.md.

## Next

1. Q-PARTY-011, then Q-PARTY-012, for RULE-PARTY-006 (slice 2). Q-PARTY-012
   reads the shared resource reader, which config-static's CONFIG findings also
   cover; coordinate with that goal before adding entries in its areas.
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

## Shared runtime migration

See [migration status](SHARED-RUNTIME-MIGRATION.md).
