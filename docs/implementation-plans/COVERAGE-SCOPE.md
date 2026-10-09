# Coverage scope consistency

Coverage scope consistency tooling: apply the owner's executable exclusions to
the work-baseline missing-file calculation as well as measured views. Evidence:
the current local report includes the excluded DOSBox executable as missing work.
Acceptance: excluded host files never count as missing game work; missing game
files and their measured byte-identical aliases remain visible. Validate with
synthetic excluded-host, missing-game and alias controls, then rerun the real
local baseline and Test.ps1. Exit: both report sections respect the recorded
scope, with licensed-source reports kept outside Git.

Formal-reading measurement follow-up: count active entries with nonempty
complete_reading declarations numerically, rather than returning null whenever
one exists. Exclude superseded entries and explain that declaration counts alone
do not establish evidence completeness. Acceptance: empty declarations and
recorded findings alone count zero; two active declarations count two even when
they share evidence; superseded declarations do not count. Run synthetic controls,
the real local baseline and Test.ps1; keep the report outside Git. Exit: measured
counts and explanatory text remain correct for both zero and nonzero cases.
