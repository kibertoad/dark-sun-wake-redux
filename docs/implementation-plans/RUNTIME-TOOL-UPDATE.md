# Runtime tooling update, 2026-11-04

Owner request: update dosbox-session and rust-dos to their latest published
versions. Use PyPI dinorefurb-dosbox-session 1.0.0 and its matching upstream
rust-dos v1.8.1 Windows x86_64 ZIP. This is tooling work, not gameplay.

Update the exact wheel/hash requirement, installer release/archive/executable
pins and required companion DLL checks. Install both into their existing
local tooling locations; preserve the previous emulator and historical
checkpoint identities. Do not relabel or migrate old states implicitly.

The session release removes DOSBox-X and mutes host output with rust-dos's
native null output. Existing RustDosSession probes must retain pinned
executable verification, configured emulated sound devices and no host audio.
Review their imports and configuration against the installed public API.
Update active runtime setup guidance while retaining historical evidence
and capability records with their original versions.

Acceptance: official PyPI wheel digest and GitHub release ZIP digest match;
the extracted executable and DLLs match the session's release documentation;
hash-locked Python restore and package-pin verification pass. Import-check
all probe imports without executing their entry points. Verify the fixed
startup_segments.py probe through the owned package session: stop at main
with its existing RNG guard, no input/writes and muted host output. Record
compatibility only for this attempted scope. Older checkpoint capabilities
remain historical until separately regenerated and checked.

Run the required tooling/integration gate, save local probe output only in
GAME_DIR, and commit the upgrade separately from research. Exit: both
installed versions and committed pins agree, required checks pass, the
bounded probe outcome and remaining capability limits are documented.

Verified 2026-11-04: hash-locked session 1.0.0 installation and rust-dos
v1.8.1 ZIP/executable checks passed; all three companion DLLs are present.
Installer repeat-run acceptance passed. All 118 session imports resolve
without executing probe modules. The fixed startup-to-main probe passed;
its session record confirms null host output. Historical checkpoints were
preserved and not revalidated under the new executable.

Test.ps1 -Integration -NoRestore passed 192 Node and 745 .NET tests with
the project-local EVIDENCE_PYTHON selected explicitly. Existing argument-count,
59 uncompiled Kaitai definitions and scheduled-generation skips remain.
Official release metadata: [PyPI session 1.0.0](https://pypi.org/project/dinorefurb-dosbox-session/1.0.0/)
and [rust-dos v1.8.1](https://github.com/dividebysandwich/rust-dos/releases/tag/v1.8.1).

## Post-initialization equipment flag observation

Q-PARTY-118 and FND-ITEM-070 require the value after initialization callbacks,
rather than inference from the earlier zero fill. Extend startup_segments.py
with one named byte observation at main's first instruction. Verify the exact
main breakpoint and relocated DS before reading DS:6174; reject invalid
relocations, real-mode physical overflow and short reads. Retain the existing
RNG trap, owned muted session, source fingerprint and no-input/no-write bounds.
Record the byte and its address in the private report. This establishes only
the observed endpoint, never continuous preservation or later gameplay values.

Synthetic controls must reject a wrong stop, wrong DS, short read and invalid
mapping before admitting evidence, and verify an exact one-byte observation.
Run Test.ps1 for the tooling batch, then record any native result in a separate
research batch. Exit: guarded observation and controls pass; native evidence
and remaining limits are documented without promoting complete reading.
