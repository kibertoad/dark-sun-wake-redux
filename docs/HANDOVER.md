# Handover

Where the work outside any goal stands now. Rewrite this file at the end of
every session that works under no goal; do not append to it. A session under a
goal writes the same sections in its goal file's Handover instead, so two
sessions running at once never write the same file. It is at most 200 lines.
History is in git, open research questions are in `queue/`, the plan is in
`docs/IMPLEMENTATION-PLAN.md`, the goals running are the files in
`docs/goals/`, and the open live session requests are the files in
`docs/live-sessions/`. Name items and entries by ID; what research found or
tried belongs in the spec and the queue, never here.
See the [work protocol](../vendor/upstream/work-protocol.md#working-files) (lines 10-30).

## State

- Stage: Slices; slices 2 and 3 remain in progress. Survey exit still needs Q-EXE-003.
- Last gate: 2026-10-01, tools/Invoke-Validation.ps1 and the final tools/Test.ps1 pass with repository-local PowerShell 7. Locked restore, Release build and assetless publish/smoke pass. Logs: artifacts/migration-acceptance.log and artifacts/migration-final-test.log.
- Relevant template infrastructure gaps are addressed; capability dispositions and validation limits are in docs/TEMPLATE-ACCEPTANCE.md. Existing pinned rules and toolkit remain exact.
- Signing/release workflow was compared with New Chrome. Shared safeguards are submitted in [template PR 38](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/38). Root changes remain local; only the template PR branch was pushed.
- Broad exporters compile against public Ghidra APIs and synthetic local-output boundaries pass. No original-game run, DOSBox control or executable analysis occurred.
- Native runtime remains owner-only. Historical hosted installer acceptance predates this batch; new signing/workflow changes have local synthetic acceptance.

## Unfinished

No unfinished implementation or tooling changes. Research follow-up remains in queue/ and gaps.md.

## Blockers

Live signing, repository signing-environment branch protection and high/mixed-DPI original-window capture remain separate acceptance checks. No release was performed.

## Next

1. Q-EXE-003 Survey exit; then Q-CONFIG-008, Q-CONFIG-007 and Q-CONFIG-002 for RULE-CONFIG-005 and FMT-CONFIG-003.
2. Q-UI-005 and Q-SAVE-001 for SCR-UI-013 and SCR-UI-014; Q-UI-002 for SCR-UI-007.
3. Q-CONFIG-001 owner live session; Q-PARTY-001, Q-PARTY-005 and Q-PARTY-009.
4. Review template PR 38; configure ES_CERTIFICATE_THUMBPRINT and main-only release-signing deployment branches before a signed release.
