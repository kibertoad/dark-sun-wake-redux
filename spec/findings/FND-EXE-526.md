---
id: FND-EXE-526
title: Game size-adjustment caller discards setter failure after metadata changes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2289..1000:231E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2289..1000:231E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The far caller 22CB saves SI, DI and BP, establishes BP from SP,
then loads incoming offset at SS:BP+10 into BX and quantity at
SS:BP+12 into AX. Quantity zero passes the offset to 20A3,
then replaces AX with zero regardless of that helper's result and removes
two argument bytes. Nonzero quantity with zero offset calls 2172 with
the quantity, removes two bytes and returns its AX. FND-EXE-513
and FND-EXE-518/519 record those immediate helpers.

With both inputs nonzero, it subtracts four from BX without testing
borrow and reads current word DS:BX into CX, then decrements CX.
It forms DX as incoming quantity plus five at word width, clears
the low bit and raises results below eight to eight unsigned. Unlike
2172, this transformation does not locally check addition carry.
It compares CX and DX unsigned. Equality advances BX by four and
returns it in AX. CX smaller calls near 2254, whose body remains
outside this reading, then returns current BX in AX. CX larger calls
near 2289 and likewise returns its current BX. Every path restores
BP, DI and SI and returns far without incoming cleanup.

Near helper 2289 holds the incoming new internal quantity DX in AX,
adds eight to DX at word width and compares resulting DX unsigned
with old quantity CX. Greater takes its common BX-plus-four return without
metadata changes. There is no carry check on this threshold addition.
Otherwise it replaces DX with CX and compares BX with current shared
word DS:390E installed or DS:3882 on disc.

Equality stores AX at DS:BX, increments that memory word, adds BX
to the still unincremented AX at word width, then pushes BX and that
sum into FND-EXE-517's near 0F46. It pops the sum into BX,
then restores saved BX with the second pop. It neither tests nor uses
returned AX before adding four to BX and returning near without incoming
cleanup. Thus metadata changes precede any setter rejection; the outer
caller replaces the setter's result with the resulting BX offset.

Inequality forms DI as BX plus AX, stores BX at DS:DI+2,
subtracts AX from held old quantity DX and subtracts resulting DX from
current word DS:BX. It forms SI as DI plus DX and stores DI
at DS:SI+2. It increments DX and stores it at DS:DI, holds
original BX in CX, sets BX to DI and calls 20FA. The recorded
20FA/214F/2133 paths do not locally modify CX, so this local
chain preserves the held original BX there. After return it restores BX
from CX, advances it by four and returns near. Ordered stores, word
arithmetic and later reloads remain subject to aliases and admitted extents.

Both editions share local control flow except the shared-word offset above.
These bodies contain no local interrupt and do not locally change DS/SS.
Input record units, headers, source and destination extents, segments and
lifetime remain unadmitted. The unread 2254 branch prevents a complete
result or preservation contract for the outer caller.

## Interpretation

This admits FND-EXE-518's positive setter-call lead at 22A2 and
traces its argument and result into a concrete caller. Setter failure does
not locally restore metadata or become this caller's return. Q-EXE-007
retains 2254, outer callers and input producers, other shared-offset writers,
actual DS/SS, block/storage admission, aliases and lifetime, remaining
initialization and earlier startup/launch coverage. No complete size-adjustment
or writer census is claimed.

## Alternatives

Treating the outer quantity transform as the guarded allocation transform
ignores its absent carry check. Treating a setter error as the outer
return ignores AX's replacement from BX. Treating failure as transactional
ignores metadata writes before the setter. Treating the helper's old BX
snapshot as unspecified after 20FA ignores the read local CX preservation;
its storage and callers still need admission.

## How to reproduce

At revision 704fcc7 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
7489..751E in sixteen-bit mode. Track the far caller's saved registers
and argument offsets, zero-input branches, word-width unguarded transforms,
unsigned comparisons and AX replacement. Follow 2289's threshold, metadata
stores, setter arguments and two cleanup pops. Trace CX through the
20FA chain using FND-EXE-514/519, including the fall-through return.
Keep unread 2254 and storage admission explicit. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
