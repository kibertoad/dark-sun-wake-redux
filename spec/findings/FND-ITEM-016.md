---
id: FND-ITEM-016
title: Counted-item consumption bypasses initial zero and retains every nonzero wrapped subtraction result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D4A8..0x0005D50D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D50F..0x0005D520
tool: tools/research/exec-census/overlay_listing.py at 88e210f4; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

This resolves the remaining local arithmetic part of Q-ITEM-004 for
overlay 173 +2B88, whose coarse quantity/flag branch FND-EXE-600 records.
The installed DSUN.EXE has length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The bounded reader checks both, derives
the overlay from its descriptor, and resolves the deletion call through
the shipped fixup. No original function was executed.

Use L for the far graph argument, r for the attached-record index, e for
the graph entry index, and n for the requested subtraction word. The body
loads r and e independently; it does not check that L[e] names r.
It selects the 23-byte attached record through DS:19C1 using the low word
of 23*r. That record's word +0A selects a 15-byte shared type through
DS:19CD, again with low-word multiplication and offset addition.
Neither index, the table capacities nor the supplied graph are checked.

The helper tests shared type byte +0B mask 02. Its complete local branches,
under stable table pointers and valid disjoint record/frame storage, are:

| Gate | Own quantity effect | Deletion request | Returning AX |
|---|---|---|---|
| Mask clear | No quantity subtraction | 2D40:2452(L, e) | Zero after that call returns |
| Mask set, initial quantity q zero | None | None | Reloaded quantity, zero under the stated stability condition |
| Mask set, q nonzero, wrapped q-n nonzero | Store the low 16 bits of q-n | None | Reloaded stored quantity |
| Mask set, q nonzero, wrapped q-n zero | Store zero before the call | 2D40:2452(L, e) | Zero after that call returns |

The initial zero comparison branches directly to the ordinary quantity
reload. It does not join the deletion request. On the nonzero path, the
subtraction is a word store. The body reloads that word into AX, performs
a logical self-test, then takes an unsigned-above branch to the reload.
The logical test clears carry and sets zero from AX; consequently that
branch means AX is nonzero, not signed-positive, and does not retain the
borrow or overflow from the subtraction. There is no saturation, signed
lower-bound test or comparison between n and the initial quantity.

These are arithmetic examples of the local instructions, not observed
native inputs or executed experiments:

| Mask set, initial q | n | Stored/result word | Local route |
|---|---|---|---|
| 0 | 1 | 0, unchanged | Return without deletion |
| 1 | 1 | 0 | Store, then request deletion |
| 2 | 1 | 1 | Retain |
| 1 | 2 | FFFF | Retain |
| 8000 | 0 | 8000 | Retain despite the signed-negative word |
| FFFF | FFFF | 0 | Store, then request deletion |

The subtraction reloads the attached table's offset while retaining ES
from its preceding far-pointer load. Its interpretation as the same
record requires that pointer and storage remain stable and non-aliasing.
The returning nondeletion branch reloads the complete table pointer.
No child is called on either nondeletion path. The sole child target is
2D40:2452. The two bytes +2BED..+2BEF form a jump with no predecessor in
this entry's control flow, as FND-EXE-600 already records; the function
inventory excludes them. They are not an extra reachable deletion path.

## Interpretation

RULE-ITEM-008's caller requests one unit, but this helper itself accepts a
word and implements modular subtraction. Its rule summary must distinguish
an initial zero from a subtraction that becomes zero and must not replace
the nonzero test with a signed-positive test.

Both already-recorded direct callers pass one: overlay 173 +2A9A in
FND-EXE-600 and overlay 191 +0D60 in FND-RNG-035. For that request,
a nonzero initial word cannot underflow on subtraction; the initial-zero
bypass also prevents subtracting one from zero. The arbitrary-word
underflow example therefore does not demonstrate an underflow through
either recorded caller. Completeness of caller coverage remains separate.

This settles the complete local arithmetic, zero bypass and return/request
ordering. It does not settle which quantities or subtraction words the
other callers admit, the independent quantity writers, table/index bounds,
L/e/r consistency, or deletion's complete effects and termination. A
deletion request is not proof of successful removal. No shipped underflow
bug, complete_reading or confidence promotion is claimed.

## Alternatives

- An initial zero with mask 02 set does not request deletion locally.
- Subtraction does not clamp at zero; nonzero underflow is retained.
- The result branch does not test signed positivity or subtraction carry.
- Retaining a nonzero result does not prove that every such quantity can
  occur through a native caller.

## How to reproduce

With the locked evidence Python and verified installed <dsun>, run:

```text
python -I tools/research/exec-census/overlay_listing.py <dsun> 173 5D4A8 5D520
```

Compare the reachable spans with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv.
Retain UTF-8 output only under GAME_DIR/analysis/item-removal-20261104/,
as quantity-arithmetic.txt, SHA-256
532b19efd6f46a1969e6ada08e7739b92af4d529b23e099ccc8b7d6fa6978ba2.
The broader listing includes the excluded two-byte gap solely to review
its lack of an incoming edge from the function. No original listing or
bytes are committed.
