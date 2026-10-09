---
id: FND-EXE-265
title: Resident filename callers include a direct bounded tail and an unbounded external-string copy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:0050
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:01B5..4AE5:01C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:01C1..4AE5:0206
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The selected root in FND-EXE-175 establishes BP from SP, allocates twenty
local bytes, loads DS through CS-relative word five and clears direction.
Its state-word-zero arm bypasses the filename helpers. The other arm first
calls `4AE5:01C1` and accepts carry clear. On carry set, it tests both
SS-relative argument words at BP plus six and eight. Only when both are
zero does it replace them with offset `0x0728` and an MZ-relocated segment
immediate. The latter's shipped site `0x0004008E` contains raw `0x45E8`,
resolving under load segment `0x1000` to `55E8:0000`, shipped base
`0x0004B080`. This records the pointer publication, not its pointed-to content.
There is no intervening local length test of a nonzero argument pointer.

It then calls `4AE5:01B5`, accepts carry clear, or calls FND-EXE-264's
prefix helper at `4AE5:0206`. Carry set from that final helper selects
AX `0xFFFE`; no-carry results bypass that error. Each selected helper
uses the root's retained BP rather than creating its own frame. This is
a conditional frame relationship, not proof of all external preservation
or native callers.

The direct helper saves DS, sets DI to `0x008C`, copies DS to ES, calls
the tail helper `4AE5:0263`, restores DS and returns near without changing
carry after the call. Under the admitted incoming state segment and
direction, its thirteen-byte maximum tail starts at the buffer base.
This bound does not include another helper's earlier writes.

The first helper sets AH `0x30` and invokes interrupt `0x21`. It compares
returned AL with three: the below-three path returns immediately with
carry set from that comparison. The other path writes byte `0x20` through
current DS-relative offset six, saves DS, loads another segment and then
loads DS through that segment's word at `0x008C`. Starting at SI zero,
it scans through the double-zero string terminator and consumes a further
word with LODSW. No local scan length is enforced.

It sets DI and BX to `0x008C` and loads ES through the MZ-relocated
immediate at shipped site `0x0004023B`: raw `0x45CE`, resolving to
`55CE:0000` and shipped base `0x0004AEE0`, the initial state base in
FND-EXE-176. It copies bytes with advancing SI and DI until a copied zero.
Every copied backslash updates BX to the following destination offset.
After the zero, DI is replaced with BX and the bounded tail helper is
called. Restoring DS and the near return leave that call's carry intact.
The first copy has no count or destination-end guard. Rewinding DI for
the tail does not undo bytes already written beyond that position.

## Interpretation

Q-EXE-001 and Q-EXE-010 have a second concrete indexed write path into the
initial state segment, distinct from FND-EXE-264's prefix construction.
An output bound must cover the complete external-string copy including
its zero, and the tail written at the last-backslash position separately.
A short final filename alone cannot prove a short earlier write interval.
Conversely, this reading does not establish an admitted oversized input
or native corruption. External string bounds, termination, segment and
register preservation, aliases, every caller and repeat-entry effects
remain open. No complete_reading or inventory replacement follows.

## Alternatives

Treating the direct helper's output bound as universal ignores both bulk
copy paths. Treating the last-backslash rewind as a bound on earlier
writes ignores their execution order. Assuming the root always replaces
its pointer ignores its nonzero-pointer branch. The interrupt services
and externally supplied storage may enforce bounds, but that contract
has not been established by these local bodies.

## How to reproduce

At revision `ded4f74`, use the installed source identity in FND-EXE-236.
Decode shipped offsets `0x00040060..0x000400A0` at initial IP `0x0010`
and `0x00040205..0x00040256` at IP `0x01B5` in sixteen-bit mode.
Use model segment `0x4AE5`, MZ header `0x5200`, load segment `0x1000`.
Run the committed operand wrapper at sites `0x0004008E` and `0x0004023B`,
targetOffset zero, with sourceKind mz and the same hash-guarded source.

Run x86-bounds separately for entries `0x00040205` and `0x00040211`,
each the sole entry in region `0x00040050..0x00042050`, segment `0x4AE5`,
IP zero. Supply no seeds or summaries; defaults are 512 steps, 64 paths
and depth eight. The direct helper reports seven instructions; the first
helper reports 34. Retain their assumed returning calls and interrupt.
Conditional CFG closure does not prove string-loop termination or external
preservation. Follow BP-relative accesses through the root's frame and
count all executed stores before DI is rewound. Original source and local
reports stay in GAME_DIR.
