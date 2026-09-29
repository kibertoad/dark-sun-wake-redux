---
id: FND-CONFIG-147
title: The zero-gate setup preserves cleared slot 523 through its intervening helpers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:03C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0CC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0B54
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:0106
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit disassembly; declared MZ and FBOV operand mapping
environment: null
---

## Observation

FND-CONFIG-144 reads overlay 188 setup 5702:00D4,
including its DS:193E zero gate, head initialization, table
clear and later traversal call. The three heads DS:56ED,
56EF and 56F1 are assigned 9999 before the clear.
This reading follows every intervening call on that branch.
After clearing all 524 three-byte slots, the setup performs
these calls in order:

| Input range, inclusive | Callee | Local call site |
|---|---|---|
| 48 through 318 | 2D40:03C1 | 0x00073C2E |
| 320 through 519 | 2D40:0C92 | 0x00073C41 |
| 4 through 47, with head pointer DS:56ED | 2D40:0CC2 | 0x00073C57 |
| 0 through 3 | 2D40:0B54 | 0x00073C89 |

Entry 03C1 (`0x000229C1..0x00022A53`) selects
head pointer DS:56F1 for inputs 48 through 318, calls
0CC2 with that input and head pointer, then calls overlay
188 entry 0106 with the input. Its input-below-48 branch
has another helper and fills, but the stated setup range
cannot enter it. No unread callee remains on the selected path.

Entry 0C92 (`0x00023292..0x000232C2`) calls
0CC2 with head pointer DS:56EF for signed inputs 320
through 519. Its other branch clears the selected slot byte;
that branch is not taken by this setup loop.

Entry 0CC2 (`0x000232C2..0x00023359`) rejects
signed input below five, at least 520, or equal to 319.
Otherwise it walks paired words from the supplied head until
9999 or until it has found the input index. If it is absent,
it clears the input slot's byte, puts the previous head in its
paired word and makes that input the new head. The DS:56EF
and DS:56F1 cases also decrement their separate bookkeeping
words DS:55C6 and DS:55C8; those words do not control the
local traversal. There is no further call.

Each setup chain starts at 9999 and receives distinct inputs
from one of the three stated ranges. A new input's paired word
therefore points to the previously built finite chain. In the
third loop, input four is rejected and inputs five through 47
are inserted. This local construction neither encounters a
pre-existing cycle nor inserts slot 523. It describes only
these reset inputs, not arbitrary incoming lists or later states.

Overlay trampoline 5702:0106 resolves to code offset 1876,
file offset `0x00074716`. Its complete body ends with return
at `0x0007475C`. It scans record indices five through 47.
When a word at record offset four is not 15 and its word at
offset zero equals the input, it clears the offset-four word.
These records start at 4F49:08A3 with stride 19; their
fields remain before the three-byte table at 4F49:0C33.
It has no callee or write to the three-byte slots.

Entry 0B54 (`0x00023154..0x000231A4`) conditionally
updates DS:5AF5 and the bytes at offsets 17 and 18 of those
19-byte records. Setup initialized DS:5AF5 to minus one and
both record bytes to FF. Its calls with inputs zero through
three therefore use a previous index of minus one or an earlier
index in that range; these writes remain in the separate record
span. It has no callee. The setup's other direct writes after
the table clear use record indices zero through three, fields
4C13:0369 and 035F, DS:60EB, 1A32 and 1A34, and
4F49:000B. None writes slot 523 at 4F49:1254..1256.

The resident table operands at `0x000232B6`,
`0x000232F6`, `0x00023333`, `0x0002334A`,
`0x00023168` and `0x00023199` are declared MZ
relocations mapping raw 3F49 to 4F49. The overlay call's
operand at `0x00022A4C` maps raw 4702 to 5702.
The four setup call operands at `0x00073C31`,
`0x00073C44`, `0x00073C5A` and `0x00073C8C`
are declared FBOV fixups naming descriptor 25, mapped 2D40.
Overlay 0106's operands at `0x00074724`,
`0x00074736` and `0x0007474A` name descriptor 113,
mapped 4F49. The setup's operands at `0x00073C96`
and `0x00073CA8` name descriptor 93, mapped 4C13;
`0x00073CB4` names descriptor 113, mapped 4F49.

## Interpretation

For the zero-gate setup path and the stated local call inputs,
slot 523 remains zero from the clear until the traversal call.
The setup assigns DS:1A32 to 523 and calls 2D40:2196
with output pointer 5072:0040. FND-CONFIG-143's zero-slot
path therefore writes word 523 at output offset six, bytes FF
and zero at output offsets zero and one, and returns zero.
It never calls the traversal helpers in FND-CONFIG-145 on
this path. Their near-pointer segment and selector-table gaps
do not block this particular local result.

This does not establish when setup is invoked, the effect of
its nonzero DS:193E bypass on previously held state, later slot
activation, later stored-index changes or the rest caller's
reachable termination. No behavioral rule's status is raised.

## Alternatives

Q-CONFIG-008 retains setup invocation and bypass state,
post-setup table producers and later iterator inputs. The earlier
possibility that these particular intervening calls activate slot
523 is ruled out by their complete selected paths and bounds.
Activation by another caller or a later write remains open.
The finite-chain argument applies to the initialized heads and
new inputs here; malformed incoming heads elsewhere are not
validated by this reading. FND-CONFIG-148 leaves selector-table
producer forms open for paths that actually call the helpers.

## How to reproduce

Read setup from its verified entry `0x00073B77`, following
head assignments, table clear, exact loop bounds and the later
stored-index assignment and traversal call. Read all four
resident bodies at the stated bounds. Follow only the 03C1
branch allowed by its setup inputs, retaining its skipped helper
as unread for other callers. Resolve trampoline 0106 and read
its complete body separately from the next entry at 0x0007475D.
Verify the named relocation and fixup operands. Trace each new
chain from head 9999 and compare all write spans with slot 523.
Use FND-CONFIG-144 for the clear, mapping and output pointer,
FND-CONFIG-143 for the complete zero-slot return, and
FND-CONFIG-145 for the traversal paths this setup does not call.
