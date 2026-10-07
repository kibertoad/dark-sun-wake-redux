---
id: FND-EXE-114
title: First-callee equality branch publishes mask one and rejoins after optional state-change calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4470..0x005A451D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A45F0..0x005A45F7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A46F9..0x005A4705
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4736..0x005A4740
tool: Ghidra 12.1.3 PUBLIC bounded equality-priority and state-call reading
environment: null
---

## Observation

FND-EXE-113 records the post-removal count equality that reaches
`0x005A4470` in the first callback callee with retained object O. It
zero-extends byte O plus `0x0118`, ORs mask one into that byte value,
reads and zero-extends byte `0x011C`, computes their full intersection
M and publishes the modified `0x0118` byte. It then publishes byte
`0x011E` according to the first matching condition in this ordered table.

| First matching condition in M | Published byte |
|---|---|
| mask `0x04` present | 6 |
| otherwise mask `0x10` present | 12 |
| otherwise mask `0x01` present | 4 |
| otherwise mask `0x02` present | 2 |
| otherwise mask `0x08` absent | 1 |
| otherwise mask `0x08` present | 0 |

All selector paths converge at `0x005A4494`, which tests full M for
zero. Bits outside the selector masks still affect that decision. M nonzero
checks byte O plus `0x011D`; already nonzero rejoins at `0x005A42AF`.
Otherwise it tests byte `0x0123`, publishes one to `0x011D`, and only
for a nonzero tested gate freshly reads full `0x0110` and calls
`0x004F2060` with that value. M zero instead checks `0x011D`;
already zero rejoins immediately, otherwise tests `0x0123`, publishes
zero to `0x011D`, and only for a nonzero gate freshly reads full
`0x0110` and calls `0x004F20E0` with it. The gate tests precede state
writes and their flags survive those writes.

Both are ordinary calls. Normal return jumps to the same continuation at
`0x005A42AF`; neither callee result is tested. A zero gate suppresses
the call but does not cancel the state-byte write. The flag and selector
publications likewise remain on ordinary no-call paths. The local F byte
from FND-EXE-113 is not directly changed by this equality branch. At the
shared continuation it will be tested; later paths and callee/alias effects
are outside this observation. Exceptional or nonreturning calls do not prove
the continuation is reached.

## Interpretation

Post-removal count equality selects flag/priority publication and optional
state-changing calls rather than the immediate callback insertion described
for inequality in FND-EXE-113. The paths rejoin without a success predicate.
Field meanings, local-F continuation, input bounds, storage lifetime and call
effects remain Q-EXE-009 in FMT-EXE-006. Similar priority tests in other
procedures do not establish shared field meanings or complete behavior here.
No complete first-callee reading or actual PATH outcome is claimed.

## Alternatives

- Publication order is flag, selector, then any state byte and gated call.
- Selector zero can coexist with M nonzero when mask eight is selected.
- Suppressing a call leaves the state-byte update in place.
- Ordinary calls rejoin locally, unlike the value-seven tail transfers in
  FND-EXE-110; no result test or rollback is present on their normal returns.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-113 independently supplies the equality branch, object and frame.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x005A4470` limit 65,
`0x005A45F0` limit 3, `0x005A46F9` limit 10 and `0x005A4736` limit
3. Restrict observations to the declared ranges, excluding following
unrelated arms. Check every priority jump, full versus byte tests, gate flags
surviving stores, fresh call arguments and both post-call joins. The
`0x005A41F0` limit-140 window from FND-EXE-113 supplies the continuation's
local-F test at `0x005A42AF`. Keep reports in GAME_DIR/analysis/exe-batches;
commit no original listings or bytes and execute neither interpreter nor game.