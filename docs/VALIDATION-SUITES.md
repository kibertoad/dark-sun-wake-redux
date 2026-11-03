# Domain validation (template PR146/147 adoption)

Record the base before editing, then run
`./tools/Test.ps1 -ChangedSince <base-commit>` or the corresponding
`Invoke-Validation.ps1` command. `tools/validation-suites.json` owns Node test
files, domain inputs, dependencies and reusable-suite contracts. Selection
unions committed, staged, unstaged and untracked paths, retaining both rename
sides and staged changes whose working copy was restored. Dependencies select
their consumers. Invalid maps fail; unknown/shared paths, unregistered tests or
an unavailable base require complete integration.

Research uses `-Research -ChangedSince <base-commit>`: fresh dependency, policy,
configuration, infrastructure, section-link, complete documentation/citation/
schema and queue checks, plus affected tooling controls, with no .NET rebuild
tests. It rejects src/tests changes and explicit .NET requests. Uncertain
research inputs fail for separate integration acceptance. In-place changes to
implemented entries retain the Code partial/Defect handoff to implementation.
Rule, format and screen changes outside research select runtime consumers.

`-Integration` discovers every Node test and runs the complete .NET suite;
filters cannot narrow it. CI and release acceptance use fresh integration.
`Invoke-Validation.ps1 -Integration` additionally builds Release, publishes an
assetless game and runs its smoke test. NoRestore retains dependency verification
and never falls back to installing or restoring. Generated indexes and PARITY.md
remain owned by the scheduled main job.

Reuse a check only when its command, tool identities, configuration,
environment, original inputs and declared file inputs match.
Each suite's `receipt` in the same ownership map declares reusable inputs; only
synthetic text-policy and goal-run suites currently enable reuse. Integration
bypasses receipts. The empty `validation-inputs.json` is retained for generic
non-suite helpers. See [Validation inputs and final records](VALIDATION-INPUTS.md).
Final documentation and staged-tree checks always run freshly; hooks remain
enabled. Result logs and handovers follow [VALIDATION-RECORDS](VALIDATION-RECORDS.md);
`tools/Check-TextPolicy.mjs` applies `tools/repository-policy.json` in the gate
and the staged snapshot. Synthetic selector and PowerShell routing controls cover this behavior.
