---
id: FND-SAVE-047
title: Character identifiers occupy the negative key side of the object-reference selector
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0AB6..2D40:0B00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0D94..2D40:0E1D
tool: tools/research/exec-census/resident_listing.py at e97916c6; locked Python 3.12.10 with Capstone distribution 5.0.9; recorded field interfaces in FND-SCRIPT-034 and FND-CONFIG-140/154
environment: null
---

## Observation

Q-SAVE-002 asks where the CACT identifier originates and what its high bit
means. FND-SAVE-045 answers the normal producer: whole-word candidates
`0x8015..0x82FF`. FND-SAVE-046 connects the runtime identifier to the
unchanged two-byte stored payload. This reading checks a positive sign-based
consumer contract rather than looking for an absent literal mask globally.

The full selector at `2D40:0D94..0E1D` retains the input word and initializes
its result to 9999. Its initial signed-nonnegative gate takes the direct
branch: return that word as the slot and set the supplied cursor to 320.
The later sentinel tests cannot redirect a negative word into that branch
from this entry: every signed-negative word differs from positive 9999 and
is below it. Negative words instead select the scan.

Starting at the supplied cursor, that scan skips zero kind bytes in the
three-byte object table and calls `1AA0:0009(index, 0)` for each other
entry while the signed index is below 320. It compares the returned AX
against the entire original input word. On equality it returns that slot
and writes slot plus one to the cursor; no match returns 9999 and sets the
cursor to 320. It neither clears bit 15 nor negates the input. There is no
lower-bound guard on a supplied cursor. The complete caller at
`2D40:0AB6..0AE5` supplies a local zero cursor and returns the first result,
using 9999 when the selector returns that sentinel.

FND-SCRIPT-034 supplies the independently read kind-two field-zero binding:
the combatant record's word at +06 is its character identifier. Its FNFO
metadata is nine; FND-CONFIG-154 supplies the width-two initialization
condition. Under those successfully loaded and preserved table/width and
valid-record conditions, FND-CONFIG-140's getter returns that signed word,
with the same low 16 bits. The selector compares AX, so the sign extension
into the upper word does not change equality. Other kinds have other
field-zero keys; this is not a character-only namespace.

The adjacent complete normalization wrapper `2D40:0AE5..0B00` complements
that service. A signed-negative argument is returned unchanged. A
nonnegative argument is passed to the same field-zero getter and the
wrapper returns AX. It does not test the getter error word. Thus an already
negative character identifier stays a key, while a valid nonnegative
combatant slot can be converted to its character key through the getter.

## Interpretation

Bit 15 has a concrete reference-dispatch role: when an identifier is used
as an input to these object-reference services, a set high bit makes it a
signed-negative key rather than a direct nonnegative slot. A generated
identifier is therefore in the key range without a separate conversion.
The complete word participates in key equality; the high bit is not a flag
to strip before matching. For example, `0x8015` takes the scan and can match
that whole identifier, whereas `0x0015` takes the direct-slot branch and
returns slot 21 without scanning. These are conditional code results, not
an observed event or proof that a particular live slot holds that value.

This answers the positive high-bit question in Q-SAVE-002 together with the
normal producer and persistence evidence. It does not assert that every
consumer has this interface or that no other use of the bit exists. Nor
does it classify a character as player-controlled, hostile or imported.
Actual trigger delivery, getter table/record admission and every arbitrary
caller remain with Q-SCRIPT-022 and Q-CONFIG-008; no complete-reading
declaration for FMT-SAVE-001 follows from answering this question.

## Alternatives

- The bit is globally opaque to consumers: ruled out by the selector's
  initial signed gate and its two different behaviors.
- Clearing the high bit obtains the placed-object slot: ruled out by the
  whole-word scan comparison and the independent direct-slot branch.
- The bit identifies a character as a player-controlled party member:
  unsupported; the key service serves other kinds too, and these bodies
  do not test party membership or control state.
- A mask census of all code is required to show this positive use:
  unnecessary for this claim. Such a census would be needed for an
  exhaustive absence claim, which is not made here. Unknown writers and
  metadata/record lifetime still prevent stronger actual-state claims.

## How to reproduce

Use the licensed DSUN.EXE, size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, with the locked evidence Python:
`tools/research/exec-census/resident_listing.py <dsun>
2D40:0AB6..2D40:0AE5 2D40:0AE5..2D40:0B00
2D40:0D94..2D40:0E1D`. The existing reporter checks the source and applies
MZ relocations for modeled load segment 1000. Read all three complete
bodies, including cursor updates and returns. Compare otherwise identical
inputs `0x8015` and `0x0015`, and scan cases with no match and an exact
whole-word match, preserving returning getters and valid zero-cursor state.
Do not substitute a masked comparison or claim a synthetic case was run.
Reuse FND-SCRIPT-033/034 and FND-CONFIG-140/141/154 for the already-recorded
caller, field and getter interfaces, and FND-SAVE-045/046 for the producer
and persistence. No original-game run or new research tool was used.
