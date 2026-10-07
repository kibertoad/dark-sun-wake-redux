---
id: FND-EXE-115
title: Local-flag-zero continuation writes a second-record zero byte or returns without another callback call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4320..0x005A4366
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4522..0x005A453B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A46AA..0x005A46AB
tool: Ghidra 12.1.3 PUBLIC bounded local-zero second-record write reading
environment: null
---

## Observation

FND-EXE-113 records local F and its zero test at `0x005A42AF` in the
first callback callee; FND-EXE-114 records the equality branch's join there.
F zero selects `0x005A4320` with retained object O. It tests mask one
of byte O plus `0x0164`. Mask absent restores the local frame and saved
registers and returns at `0x005A4366`, without reading the second record
or making another callback call. EAX is not normalized on that exit.

Mask present reads full second-record pointer R from O plus `0x0154`,
full count C at R plus sixteen, full index I at twelve and full size L at
eight. It forms P = I + C modulo thirty-two bits. When P is at least L
unsigned it subtracts L once. It then compares C with L unsigned. C below
L reads R's full base, writes byte zero at base plus the selected P, and
increments the current full count at R plus sixteen modulo thirty-two bits,
after the byte write. It restores the frame and registers and returns with
EAX retaining the size L read earlier; there is no local success conversion.

C at least L instead chooses selected P minus one when nonzero, or L minus
one modulo thirty-two bits when selected P is zero. It then freshly reads
R's full base, writes zero at base plus that position and returns through
`0x005A4360` with EAX retaining the base just read. This path makes no
direct count increment. Neither write path stores a new index. Base, count,
index and size validity and record aliases are not checked locally. Size
zero can reach the all-ones replacement position, and the one subtraction
does not establish a valid byte bound for arbitrary inputs.

There is no direct or indirect call between this branch's gate test and
its described returns. Earlier removal/insertion or equality state-change
calls in FND-EXE-113 and FND-EXE-114 may already have changed the object;
this no-call statement concerns this reached continuation only. No later
local priority publication follows these writes. Post-write alias effects,
exceptional storage access and caller return use remain unresolved.

## Interpretation

Local F zero has a concrete gate-controlled second-record zero-byte path
with append versus replacement count behavior. Its return value varies by
path and is not a success predicate. Q-EXE-009 in FMT-EXE-006 still requires
F producers/admission, second-record bounds/lifetime and field meaning,
other local-F paths and caller contracts. No complete first-callee reading
or actual PATH outcome follows.

## Alternatives

- Gate absent avoids the second-record read and does not normalize EAX.
- The stored value is zero, not the initial input byte from FND-EXE-113.
- Append increments count after the byte write; replacement does not.
- A one-subtraction position is not general modulo or proof of a valid bound.
- This continuation does not undo earlier callback or state changes.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-113 independently supplies the local-F branch and frame. Use the
saved Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x005A41F0` limit 140, `0x005A4470` limit 65 and `0x005A46AA` limit
8. Restrict observations to the declared ranges, following wrapped-position
reduction at `0x005A4522`, append at `0x005A452C`, replacement decrement
at `0x005A46AA`, and returns at `0x005A4366` and `0x005A453B`.
Track retained size versus fresh base, full count increment after the byte
store, single subtraction and absence of calls within this bounded path.
Keep reports in GAME_DIR/analysis/exe-batches; commit no original listings
or bytes and execute neither interpreter nor game.