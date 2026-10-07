---
id: FND-EXE-119
title: Object-gate-absent priority path rejoins local-flag publication after optional state calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4379..0x005A43CA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A45A8..0x005A45E6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4690..0x005A46A5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A470A..0x005A4711
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4745..0x005A4762
tool: Ghidra 12.1.3 PUBLIC bounded gate-absent priority and suffix admission reading
environment: null
---

## Observation

FND-EXE-116 records nonzero local F reaching `0x005A4379` when mask
one of object O's byte `0x0164` is absent. This path zero-extends bytes
O plus `0x0118` and `0x011C`, ORs mask four into the former, computes
full intersection M and publishes the modified `0x0118` byte before
priority selection. It publishes `0x011E` according to the first matching
condition: mask four in M gives six; otherwise mask sixteen gives twelve;
otherwise mask one gives four; otherwise mask two gives two; otherwise
mask eight absent gives one and present gives zero. Branches test these
masks in that order, not by a generic highest/lowest-bit rule.

All selectors reach a full M zero test. M nonzero checks byte `0x011D`;
already nonzero joins `0x005A43D0`. Otherwise it tests byte `0x0123`,
publishes one to `0x011D`, and a nonzero gate freshly reads full
`0x0110` and calls `0x004F2060` with it. M zero instead checks
`0x011D`; already zero joins directly, otherwise tests `0x0123`,
publishes zero to `0x011D`, and a nonzero gate freshly reads full
`0x0110` and calls `0x004F20E0`. In both state-changing paths a
zero gate suppresses the call without cancelling the state write. The
gate-test flags survive that write.

Both are ordinary calls and normal return joins `0x005A43D0` without
a result test. That shared entry, read in FND-EXE-118, freshly reads
local F and ORs it into O plus `0x0125` before counter processing.
Thus this priority path's normal no-call and post-call exits enter local-F
publication, rather than the later `0x005A43DB` entry that bypasses it.
The path does not directly change local F or directly write the second
record; preceding paths or aliases and callees may have affected those
locations. Exceptional/nonreturning callees do not prove the join occurs.

## Interpretation

Object-gate absence selects priority/state handling and shared F publication,
not immediate return or a second-record write. All described ordinary paths
join before the counter suffix's first local-F read. Q-EXE-009 in FMT-EXE-006
still requires the post-record current-byte-nonzero priority path, producer
and alias contracts, callee effects and other suffix admissions. No complete
first-callee reading or actual PATH outcome follows.

## Alternatives

- Gate absence does not avoid flag publication or the shared counter suffix.
- Selector zero does not imply M zero when mask eight is selected.
- Suppressing a state call leaves the direct state-byte update in place.
- This path joins before the `0x0125` OR; not every suffix entry does so.
- Callee results do not control the normal join and no rollback is shown.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-116 independently supplies gate-absent admission. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x005A41F0` limit 140, `0x005A45A8` limit 16, `0x005A4690` limit 8
and `0x005A4745` limit 8. The `0x005A46F9` limit-10 window in
FND-EXE-114 supplies the mask-one arm at `0x005A470A`. FND-EXE-118's
`0x005A43D0` limit-40 window independently supplies shared publication.
Restrict observations to the declared ranges and cited shared entry. Follow
every selector, full versus byte tests, flags across state stores, fresh call
arguments and both post-call joins. Keep reports in GAME_DIR/analysis/exe-batches;
commit no original listings or bytes and execute neither interpreter nor game.