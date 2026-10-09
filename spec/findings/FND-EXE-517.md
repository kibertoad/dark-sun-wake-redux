---
id: FND-EXE-517
title: Game allocation offset ships at the zero-fill endpoint and has a stack-margin setter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D09C..0x0004D09E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004D00C..0x0004D00E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0F46..1000:0F68
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0F46..1000:0F68
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0F99..1000:0FA5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0F99..1000:0FA5
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The shipped little-endian word at installed file 4D09C is A4EC;
the word at disc file 4D00C is A436. Under FND-EXE-509's
modeled data segments 57E0/57D7 these map to DS:009C,
FND-EXE-516's shared allocation offset. Each value equals its edition's
recorded zero-fill end offset. The zero-fill excludes this low shared-word
location under that modeled mapping. These are shipped values and numerical
relationships, not admitted runtime segment or lifetime evidence.

Near setter 0F46 saves BP and reads incoming word SS:BP+4
into AX. It copies current SP into DX and subtracts 0200 at
word width. It compares incoming AX unsigned with that resulting DX.
Below stores AX into current DS:009C and returns zero in AX.
At or above stores eight into DS:0094 and returns FFFF. It
restores BP and returns near without incoming cleanup. No calls or
interrupts occur, and DS/SS are locally unchanged.

The margin subtraction has no borrow check. If current SP is below
0200, its resulting threshold wraps and can admit a numerically large
offset. The setter also has no lower-bound, monotonicity, allocation-unit
or admitted-extent test. A selected offset can move the shared word
backward or forward numerically. Failure does not locally update DS:009C,
subject to unadmitted aliases involving the error word or incoming frame.

Far wrapper 0F99 saves BP, pushes its incoming SS:BP+6 word
to 0F46, removes two argument bytes into CX, restores BP and
returns far without incoming cleanup. It returns the near helper's AX
unchanged and performs no independent validation. Its nested call depth
determines the SP value the setter compares against, rather than using
the wrapper's incoming stack pointer.

Both editions have identical setter and wrapper bodies. A bounded physical
literal-store search supplied the setter lead, but does not establish that
these and FND-EXE-516's exchange are every writer. Indirect, aliased,
encoded and runtime-created writers and callers remain unexamined.

## Interpretation

This records a shipped seed for the allocator offset and another explicit
writer with a different margin formulation from FND-EXE-516's addition
guard. Q-EXE-007 retains actual DS/SS, startup and later writers,
0F99/0F46 callers and incoming offset producers, stack/storage extent,
aliases and lifetime, other initialization helpers and broader launch coverage.
Neither the matching zero-fill endpoint nor this setter proves valid storage
at the diagnostic. No complete writer census is claimed.

## Alternatives

Treating the shipped seed as unchanged runtime state ignores other writers
and segment admission. Treating SP-minus-margin as a nonwrapping bound
ignores the absent borrow test. Treating a passing setter as allocation
success ignores its unchecked storage and freely replaceable offset.

## How to reproduce

At revision 8aec9ce require both DSUN.EXE identities from FND-EXE-350.
Read the little-endian words at shipped 4D09C and 4D00C and
compare with FND-EXE-509's zero-fill endpoints. The lead search scans
shipped 5200 to before the modeled data base (4D000 installed,
4CF70 on disc) for literal store forms C7069C00 and A39C00;
the latter supplies the candidate at 6156. This is a positive lead,
not an absence test or complete instruction census. With header size
5200 and modeled load segment 1000, decode shipped 6146..6168
and 6199..61A5 in sixteen-bit mode. Track SS arguments, current
SP, word subtraction, unsigned comparison and ordered shared/error stores.
Licensed bytes stay outside Git; no game process, DOSBox or emulated call runs.
