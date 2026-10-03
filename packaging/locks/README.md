# Installer dependency locks

Profiles record the exact target runtime and single-file or directory dependency shape.
The Windows game uses a directory; the extractor and portable builds use single files.
Ordinary project locks remain separate. Publishers restore in locked mode and fail on
missing profiles; they never regenerate locks as a CI fallback.

Runtime and ILLink dependencies are pinned to 10.0.12. Maintenance sets RuntimeIdentifier
explicitly so another platform's profile cannot inherit the build host runtime.

Regenerate after dependency changes with `./tools/Update-PackagingLocks.ps1`, then verify
all profiles with `./tools/Update-PackagingLocks.ps1 -Check`.
