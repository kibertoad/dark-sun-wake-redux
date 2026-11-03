# Adopt template validation suites and checker integration

Adopt merged template PR146 (485e9899164b259c78359c8b449e8888b2b338bb)
and PR147 (8fbe7293a8abc539bccf42e9fdde4a33f280d314). This tooling batch
changes validation cost and selection, not game behavior or evidence status.

## Outcome and evidence

Research batches run fresh policy, configuration, complete documentation,
citation, schema and queue checks without rebuilding the game. Tooling and
implementation changes select the reviewed test domains they affect.
Unknown or shared inputs require fresh integration, and CI discovers every
Node test and runs the complete .NET suite. Existing project-specific
evidence tools, source isolation, generated-file policy and packaging remain.

The authoritative source is the merged template at the commits above. Its
process PR128 snapshot is adopted byte-for-byte at
e5ccb6e7f34a83a0495cd0004c215a3737bdb5b1. Checker 6.1.0 and matching CI
98e2e2000e365537c85ffa7605e4813a82a46cb4 are already adopted; retain the
project's local-main baseline adaptation and preflight controls.

## Implementation and acceptance

- Port the versioned domain selector/runner and same-map suite receipt
  contracts. Own every existing Node test, including local research tools
  and packaging controls. Preserve project-specific dependency pins.
- Extend Test.ps1 and Invoke-Validation.ps1 with explicit research,
  change-base and fresh integration modes. Retain NoRestore and meaningful
  failure propagation. Research rejects src/tests changes and explicit
  rebuild requests; an uncertain research selection fails for a separate
  integration run. Implementation of changed rules/formats/screens still
  selects .NET consumers. CI and release acceptance use full integration.
- Replace narrow checker-option copies with minimal synthetic metadata,
  retaining the whole-checkout configuration integration control.
- Share the production repository-policy evaluator with its CLI and batch
  diagnostic controls in one PowerShell host. Each case has an independent
  fixture, cwd and Git environment; retain real CLI success/failure checks.
- Enable reviewed receipts only for synthetic text-policy and goal-run
  suites. Configuration, tools, environment, project-local inputs and
  declared bytes invalidate reuse. Integration and staged checks stay fresh.
- Update local instructions, linked ranges and validation documentation to
  the adopted contract. Keep all game-specific restrictions and evidence.

Automated acceptance includes selector controls for dependencies, unknown
inputs, unowned tests, invalid maps and staged/unstaged paths; real suite
receipt hit/invalidation/failure controls; research/no-restore orchestration
controls; actual checker preflight/option controls; independent policy
cases; fresh full integration; and enabled staged hooks.

## Exit

All adopted controls and the complete assetless .NET suite pass under the
new fresh integration mode. A clean research selection performs no rebuild
or .NET tests while running all required fresh checks. Current rules and
checker pins verify offline, linked section ranges agree, and generated
indexes and parity totals are unchanged. Record missing Kaitai compilation
explicitly; do not present it as completed. Commit this tooling batch and
its goal handover separately, without pushing absent an owner request.

## Acceptance result

2026-11-04: final fresh Invoke-Validation.ps1 -Integration -NoRestore passed
192 Node controls and all 745 .NET tests, with zero test skips. Release build
completed with zero warnings/errors; assetless publish and smoke passed.
Pins, section ranges, policy, infrastructure and complete documentation checks
passed. Documentation retains the existing argument-count skips, scheduled
generation handling and unavailable compilation of 59 Kaitai definitions.
All 33 Node test files have owners in the reviewed map. Configured gate
controls prove research performs no dotnet command and retains fresh checks;
integration refuses manual narrowing. The first integration attempt exposed
an omitted receipt-fixture freshness reset; the upstream isolation and fresh
invalidation control were restored before the final passing run.
