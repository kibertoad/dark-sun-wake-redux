# Checker preflight adoption

Adopt the published checker release containing toolkit PR549, with the same
release-tag commit pinned for CI and the exact package version locked locally.
This tooling change provides faster optional editing feedback. Complete
validation, citation, schema, queue and staged checks remain acceptance gates.
No game behavior, evidence status or asset-pack contract changes.

The wrapper accepts `docs --preflight`, removes only its implicit CI
`--scheduled-generation` flag in this mode, and preserves explicit caller
arguments so the checker rejects incompatible requests. Preflight never
writes generated files. Keep its partial-pass message and skipped-check list.
Ordinary docs/check/generate behavior and explicit failures remain unchanged.

Acceptance uses the actual installed checker against small synthetic spec
metadata: preflight succeeds with stale generated output and does not rewrite
it, invalid entries fail, incompatible explicit flags fail, and ordinary full
checking still rejects uncited source addresses. Run focused wrapper controls,
offline pin/link checks and the repository-required acceptance gate. Record
unavailable schema compilation and preflight skips without claiming a full pass.
Publish the template adoption as a separate PR from validation-suite changes.

## Acceptance result

Published checker 6.1.0 and the matching release-tag commit
`98e2e2000e365537c85ffa7605e4813a82a46cb4` are pinned locally and in CI.
The real project's preflight passed with its explicit partial-pass and skipped
checks. Its synthetic wrapper controls passed. Full Test.ps1 -NoRestore passed
176 Node tests and 745 .NET tests with no test skips. The complete documentation
check passed with the pre-existing argument-count skips, missing compilation
for 59 Kaitai definitions, and scheduled-generation handling.

The original rules snapshot, reader and Python pins are retained. Preflight
never substitutes for the required complete gate. The independent template
adoption is [PR147](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/147);
it passed its prescribed local focused gate and its CI run remains separate.
