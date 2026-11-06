---
id: FND-PARTY-319
title: Timer old-vector publication and nesting reset have explicit state producers and flag-restoration paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:040C..4842:04DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:04DD..4842:059C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:06CF..4842:071A
tool: window_operands.py, resident_listing.py and direct_callers.py at cc831dcc; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-TIME-012 and FND-PARTY-316/317/318
environment: null
---

## Observation

This narrows Q-PARTY-136's timer continuation and nesting obligations.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

### Saved vector is captured from the current environment

4842:0520 saves DS, SI, DI and flags, then disables maskable
interrupts. With ES zero it reads offset 0020 into BX and then
segment 0022 into ES. It stores that pair at CS:011A/011C.
This is the current interrupt-8 vector, not a fixed BIOS entry.
It publishes 4842:0519 as slot-16 callback and writes its own
4842:040C pair into the interrupt vector. The old-vector callback
0519 pushes flags before its far call through CS:011A.
Whether the captured target is firmware, a previous handler or changed
memory remains an actual vector-history question.

The 056C restoration routine loads the saved pair and writes it back
to the IVT, first setting the timer scan index to FFFF. Neither
routine locally replaces the saved pair with a constant. The controlled
0117..011E operand window reports 32 established operands and
16 raw candidates outside their starts. The only established writes
through this timer CS are the two saved-vector stores and period-high
stores at 04EA/06AA, whose word 0118 ends before 011A.
The stores and the known callback's far read provide positive controls.

Other write matches are 4AE5:0122/0316/05A4 through their current
DS and overlay 197 +34EB through indexed ES. Their displacements
alone do not establish timer-CS backing. Actual segment/object aliases
and wider, adjusted-pointer, copied, register-only or alternate writes
remain outside the classified timer-CS set. No whole writer absence
or immutable saved-vector claim follows.

### Nesting counter has a reset as well as handler updates

The controlled 0001..0006 window reports 13380 established operands
and 12239 raw candidates outside their starts. Restricting classification
to the timer segment's established CS operands, its nesting word 0004
is incremented at 040C, compared at 041B, decremented at 04D3,
and explicitly zeroed at 06DB. The handler updates from FND-PARTY-316
are independent read-and-write controls. Unclassified operands elsewhere
are not dismissed; the broad window is not a whole-program alias proof.

Initializer 06CF saves DS/SI/DI/flags and disables maskable interrupts.
It clears the allocation count, nesting count and CS:0E22, then clears
32 words at CS:0126..0166, fills 16 words at 0166..0186 with
FFFF, and clears 16 words at 0186..01A6. These string extents do
not reach captured SS/SP at 03B6/03B8 or saved vector 011A/011C.
The literal reset can nevertheless change whether a later interrupt
takes the handler's first-entry capture path. It cannot be treated as
an unknown or absent counter writer. The encoded far-call search finds
two leads: resident file 3C7FC and overlay 180 +04AE; their actual
call admission and excluded transfer forms remain unresolved here.

### Masking ends through an overlapping IRET entry

The initializer ends its protected work by PUSH CS and near CALL
0710. The call enters the IRET byte inside the sequential instruction
starting 070F. At that entry, the near return offset, pushed CS and
earlier saved flags form the IRET frame. With intact stack state,
IRET resumes at 0716 with the saved flags, then the routine restores
DI/SI/DS and returns far. There is no separate POPF on this path.

The timer-table clear 04DD, vector installer 0520 and vector restorer
056C use the same stack construction with IRET entries 050F, 0562
and 0592, returning to 0515, 0568 and 0598 respectively. Reading
only their sequential OR instructions hides these entered streams.
This proves local flag restoration under intact frames, not immunity
from all interrupts, physical aliases or external effects.

## Interpretation

Q-PARTY-136 now names the vector's IVT capture epoch and actual old
target, the explicit nesting reset and its admission, and the concrete
flag-restoration frames. The known initializer's string extents do not
write captured SS/SP or the saved vector. Other aliases, excluded
writers, callback and BIOS-chain effects remain. No complete retained
number contract, queue closure or complete reading follows;
FMT-PARTY-007 stays supported.

## Alternatives

- The chained target is a fixed BIOS address: setup captures the current IVT.
- Only handler increment/decrement changes nesting: initialization clears it.
- Missing POPF means flags stay masked: entered IRET helpers consume the
  saved flags with their constructed return frame.
- Sequential decoding proves there is no IRET entry: direct calls enter
  those bytes inside the sequential instructions.

## How to reproduce

Use window_operands.py with the installed DSUN inventory and windows
0117..011E and 0001..0006, retaining raw candidates and exclusions.
Read resident_listing.py for the locations above; also start decoding
at 050F, 0562, 0592 and 0710 to check the entered IRET streams.
Run direct_callers.py for 4842:06CF and retain its transfer limitations.

Reports under GAME_DIR/analysis/handler-census-20261106/ are
getter-timer-old-vector-writers.txt, getter-timer-vector-publication.txt,
getter-timer-nesting-writers.txt, getter-timer-nesting-reset.txt,
getter-timer-nesting-reset-callers.txt and getter-timer-flags-return.txt.
The initializer's sequential listing and encoded call establish the 0710
entry; the flags-return report covers the other three entered streams.
No original game, emulator session or emulated call supplied evidence.
