---
id: FND-EXE-546
title: Game resident request selected paths change segment links and return a segment-offset pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:143B..1000:1464
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:143B..1000:1464
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1582..1000:15A5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:1582..1000:15A5
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-531's exact-size path calls near 143B with the selected
segment in DS and DX. That helper copies DS into BX and compares
BX with current DS word six. Equality clears CS:135F and returns
near. Inequality loads ES from current DS word six, then replaces
DS with current DS word four. It stores ES into this new DS
word six, stores this new DS into ES word four and publishes DS
at CS:135F. It then restores DS from held BX and returns near.
The ordered reads and stores cross segments; numeric offsets four and six
do not denote the same storage in each access.

Both paths preserve DX and AX locally, make no calls or interrupts,
do not change SS and return without incoming cleanup. BX retains the
incoming segment. ES changes only on the unequal path; DS is restored
locally there. Incoming links, writable extents, aliases and frame integrity
are not checked. The stores can therefore not alone prove a valid list.

The wrapper then reads selected DS word eight into BX and stores
it into selected DS word two before replacing AX with four. Under
intact local execution, returned DX retains the selected segment and AX
is four. The wrapper's shared DS restoration follows these stores; it
does not replace DX. FND-EXE-529's startup caller uses that high word
after its joint zero-pair test.

The wrapper's greater-size path calls near 1582 with AX the transformed
request quantity and DX/DS identifying the selected segment. It holds DX
in BX and subtracts AX from current DS word zero at word width.
It adds the freshly read result to DX at word width and loads DS
from resulting DX. It stores AX at this segment's word zero, stores
held old segment BX at word two, copies DX into BX and adds
fresh current DS word zero to BX. It loads DS from resulting BX
and stores DX into this final segment's word two. It then sets AX
to four and returns near without cleanup.

This helper does not locally restore DS: the last metadata write uses
the final segment, while returned DX retains the preceding segment. It
does not modify ES, SI, DI, BP or SS locally and makes no calls
or interrupts. Each arithmetic operation is word-width without a carry or
borrow test. Later reads follow earlier stores, so aliases can affect
the quantities and selected storage; no disjointness or segment extent is
admitted. The wrapper restores DS afterward from its shared code word,
and passes this helper's DX with AX four to the recorded caller.

Both editions have identical instructions in these bodies. No unconditional
allocation-unit, header-size or valid returned-buffer contract follows from
the segment arithmetic and fixed offset four.

## Interpretation

This resolves two immediate callee bodies of the resident request wrapper
and the returned pair on its selected-record paths. It distinguishes restored
DS in the exact-size helper from changed DS in the split helper,
and follows both into the wrapper's restoration and startup consumer.
Q-EXE-007 retains 14C4 and 1528, shared-word and record/link
producers, units, segments, extents, aliases and lifetime, other callers and
remaining startup/native dependencies. No complete allocator or launch contract
is claimed.

## Alternatives

Treating all word-two stores as one field ignores segment changes.
Assuming the split helper restores DS contradicts its final assignment.
Treating returned DX as that final DS ignores the second segment formation.
Calling the returned pair a proven buffer ignores missing storage admission.
Treating the exact-size helper's head update as unconditional ignores equality.

## How to reproduce

At revision 995a3d2 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
663B..6664 and 6782..67A5 in sixteen-bit mode. Track DS,
ES, BX and DX separately through both near helpers. Name the segment
at every read/write, retain execution order under aliases, and follow their
returns into FND-EXE-531's selected branches and shared DS restoration.
Use FND-EXE-529 for the subsequent pair test and high-word arithmetic.
Keep storage and producer admission explicit. Licensed bytes remain outside
Git; no original process, DOSBox or emulated call runs.
