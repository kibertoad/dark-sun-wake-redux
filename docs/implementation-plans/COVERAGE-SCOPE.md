# Coverage scope consistency

Coverage scope consistency tooling: apply the owner's executable exclusions to
the work-baseline missing-file calculation as well as measured views. Evidence:
the current local report includes the excluded DOSBox executable as missing work.
Acceptance: excluded host files never count as missing game work; missing game
files and their measured byte-identical aliases remain visible. Validate with
synthetic excluded-host, missing-game and alias controls, then rerun the real
local baseline and Test.ps1. Exit: both report sections respect the recorded
scope, with licensed-source reports kept outside Git.
