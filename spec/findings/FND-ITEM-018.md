---
id: FND-ITEM-018
title: Known hit callers save and forward the result byte, leave it untested at queue exit, or aggregate it before a separate bookkeeping return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E32C..0x0007E370
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E7D6..0x0007E907
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064FFA..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065579..0x00065591
tool: tools/research/exec-census/overlay_listing.py at 631aee5f; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

FND-PARTY-096 already records three direct callers of overlay 179 +0D2F:
near calls at +0D1A and +1299 and a far call from overlay 193 +04DE.
FND-MAGIC-018 and FND-PARTY-201 record the third caller's per-target loop
and aggregate OR. This reading resolves those known callers' subsequent
local result handling for Q-ITEM-009. It is not a new exhaustive direct,
indirect or alternate-segment call census.

The reader verifies installed DSUN.EXE length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, derives overlay bases 642E0 for 179
and 7E2F0 for 193, and resolves the far-call fixups. No original code ran.
All statements below describe returning local paths with valid, disjoint
frames; child termination, storage aliases and native admission are separate.

### The two near callers

After the +0D1A call, wrapper +0C7F removes the 26 argument bytes, saves
AL in SS:[BP-1], calls queue runner +11DC, reloads that saved byte into
AL, restores its frame and returns. AH is not cleared by that reload.
Thus the own instructions explicitly preserve and forward the earlier
hit byte across the queue call, conditional on the child's return and
preservation of that live frame byte. The queue call's incidental return
is not substituted for the saved hit result.

After the +1299 call, queue runner +11DC removes 26 argument bytes,
decrements DS:0915 as a byte and compares it as signed with -1. A value
greater than -1 restarts at +11E7; otherwise it restores its frame and
returns. This continuation neither stores nor tests AL. Its local exit
also does not overwrite AL, so a hit byte can remain incidentally on that
return; a restarting iteration is not a result aggregation. No declared
void/Boolean result contract or whole-loop termination follows from it.

### Casting aggregate and post-loop consumer

Overlay 193 +003C allocates 364 local bytes and initializes the word at
SS:[BP-0x22] to zero at +0050. At +04E6, immediately after the hit call and
argument cleanup recorded by FND-MAGIC-018, it clears AH, then ORs AX
into that word. Each contributing hit result is therefore zero-extended
from AL; unspecified AH from FND-ITEM-017 is not part of the aggregate.

The following own gates for fight-state setup use the DATA flag, current
mode, target number and caster number that FND-PARTY-201 records. They
do not use the aggregate in this continuation. The counter then advances
and the signed loop test either returns to +0483 for another target or
enters the post-loop path at +0559.

The post-loop path can first rebuild a target list and request 371D:050F
for its entries: the gate is DATA number 26, 150 or 310, or mask 02 of
the DATA record's word +1A. That DATA mask is distinct storage from the
aggregate word and from the hit byte. No inference about the item's
result mask is made from this numerically equal mask.

At +05C9 the own aggregate consumer tests mask 01, not mask 02. Only
when it is set and SI is signed greater than -1 does the body request
overlay 195 +09A7 with SI and DI. A mask-02-only aggregate does not
satisfy this explicit gate. This is a local request condition, not a
claim about the child's complete effects or other aliased reads.

The ending continuation then tests the optional far pointer argument
at BP+0x16. A null pointer returns AL zero. The nonnull path reads its
byte and compares against a threshold computed from the retained DATA
record. Its increment branch increments that pointed byte and sets AL
one; the other branch clears the pointed byte and sets AL zero. All
three routes join the epilogue at +0613. This post-loop route therefore
returns its separate zero-or-one bookkeeping result, not the aggregate
flags. The byte's producer, admitted range and whole-routine early exits
are not established by this ending alone.

## Interpretation

The hit byte has three distinct known local continuations: explicit
save/forward across queue processing, an untested queue-exit residue,
and zero-extended per-target aggregation followed by a different return.
Treating every caller's return as an item-effect success Boolean would
erase these differences. Mask 02 alone does not trigger the recorded
casting aggregate's mask-01 follow-up.

This settles the previously unread local result handling at those three
recorded direct call sites. Remaining Q-ITEM-009 obligations are complete
incoming coverage, admitted near-pointer/frame/record relationships,
whole-child effects/preservation and consumers beyond the forwarding
wrapper or queue return. Reuse this finding rather than reading the same
post-call instructions again. No complete_reading, native observation,
successful presentation claim or confidence promotion follows.

## Alternatives

- The casting aggregate does not accidentally include the hit's AH;
  the caller clears it before its word OR.
- The casting post-loop return is not that aggregate word or low byte.
- The queue continuation does not explicitly interpret the hit byte,
  but its returning path also does not explicitly clear AL.
- Equal mask values in DATA word +1A and the hit result are not one field.
- Saving a byte across a child call does not prove preservation for
  arbitrary aliased frames, nor completion of that child.

## How to reproduce

With the locked evidence Python and verified installed <dsun>, run:

```text
python -I tools/research/exec-census/overlay_listing.py <dsun> 193 7E32C 7E370
python -I tools/research/exec-census/overlay_listing.py <dsun> 193 7E7D6 7E907
python -I tools/research/exec-census/overlay_listing.py <dsun> 179 64FFA 6500F
python -I tools/research/exec-census/overlay_listing.py <dsun> 179 65579 65591
```

The ranges lie inside the caller bodies in
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Reports are UTF-8 and retained only in
GAME_DIR/analysis/item-removal-20261104/, with SHA-256:

- cast-result-initialization.txt:
  3f2f3497f1869031f8472557680216fc122116401a4792c02123c4355ad94485
- cast-result-continuation.txt:
  5ad30da0b9acc179b8690683f8d8eb85206aa3a826e405f69a99dc1fdc857770
- hit-near-caller-one.txt:
  9ec695aebd7ab6b589ce3b7f8e630b298c36ac7bae394caf9720b41f13fc6957
- hit-near-caller-two.txt:
  d9fb2597f3bc89b2a343a919ff48235f180788422b8c6afbb38cfcdc1edc5c3e

No original listing, bytes or extracted records are committed.
