---
id: FND-CONFIG-109
title: Overlay 208 setup stores the selector gate and code from its arguments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:007F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:0084
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV fixup and trampoline mapping
environment: null
---

## Observation

Overlay 208 entry 57A6:0066 begins at file offset `0x00092EA2`
and returns at `0x00092F2D`. It retains the first word argument in
SI and third word argument in DI. After calls to two resident helpers,
it stores these selector-input fields:

| Stored field | Input or literal | Store file offset |
|---|---|---|
| DS:9BD4 | Second word argument | `0x00092EE2` |
| DS:9BE2, selector code | Third word argument | `0x00092EE5` |
| DS:9BDE, second selector word | Minus one | `0x00092EE9` |
| DS:9BD3, selector gate | Byte from fourth argument at BP+0C | `0x00092EF2` |
| DS:9BE0, first selector word | First word argument | `0x00092EF5` |
| DS:9BE5, fourth selector word's byte | Byte from fifth argument at BP+0E | `0x00092EFC` |
| DS:9BE6, pointed selector state | Zero | `0x00092EFF` |

It also clears DS:9BE4 and DS:9BE7. An earlier helper call receives
the third word argument and returns a far pointer; another helper
receives that pointer, destination DS:9B86 and count 73. This reading
does not assign the latter helper's transfer semantics. The entry
then sets the stored record pointer DS:9BCF to DS:9B86 explicitly.

After the listed stores, the entry calls another helper with its first
word argument and literal one, sets DS:9BDC and DS:9BDA to minus
one, and calls local 57A6:0084 at `0x00092F27`. That local helper
can enter the selector caller 007F (FND-CONFIG-110). There is no
branch or preceding return between this setup entry and the local call.

A raw search for the exact two-byte operand DS:9BD3 within overlay
208's declared code-and-data range produces two candidates; decoding
from their exported entry boundaries verifies the setup store and
007F read. The same bounded search for DS:9BE2 produces nine
candidates, including this store and the 007F argument read
(FND-CONFIG-108). This is a literal-operand inventory within one
overlay, not a whole-executable or indirect-writer proof.

## Interpretation

The stored selector gate and code have an explicit producer. On this
setup route their assigned values come from separate input arguments;
initialization does not itself restrict the code or force a zero gate.
The pointed state starts zero at its store, but intervening helpers,
selector calls and later state still matter.

## Alternatives

The six declared setup invocations' arguments are read in
FND-CONFIG-111. Other writers, indirect or block updates, helper
effects and the record's live byte values remain open
(Q-CONFIG-008). A local zero assignment alone does not prove the
pointed byte is zero at a later selector invocation.

## How to reproduce

Resolve overlay 208's 0066 trampoline with FMT-EXE-002 through
FMT-EXE-004, then inspect the complete bounded entry
`0x00092EA2..0x00092F2E`. Track arguments BP+06, BP+08,
BP+0A, BP+0C and BP+0E separately through the stores. Compare
007F's reads in FND-CONFIG-108. For the literal-operand search,
restrict to `0x00091890..0x00093088`, retain raw candidates
separately, and decode from each enclosing exported entry. Verify
the known 007F reads as positive controls; do not linearly decode
across the whole overlay to infer absence.
