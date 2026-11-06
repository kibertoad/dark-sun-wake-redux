---
id: FND-SOUND-074
title: Region transition callers distinguish restored current-region reloads from incoming region arguments and failure-controlled rebuild flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070F62..0x0007118B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069FD3
tool: direct_callers.py and overlay_listing.py at d447505f; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

This connected input/result reading uses the installed DSUN.EXE,
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. It reuses
FND-SOUND-071's transition body, FND-COMBAT-079's byte-argument and
child-flag contract, FND-COMBAT-035's reader interface and FND-SAVE-015's
source table. Its question is how the four known direct calls supply the
region and flags, not whether every native transition succeeds.

The controlled direct-call query agrees with FND-PARTY-125: four far
call sites, all overlay 187, use trampoline 56BD:00C0 to enter overlay
182 +15D7. The two containing exported streams are +0EBE and +1132.
Reading from those roots confirms these argument tuples, in declaration
order (region word, option byte, preparation byte, restore byte):

| Call site | Region source | Option | Preparation | Restore |
|---|---|---|---|---|
| +0F21 | Fresh DS:140C word | 0 | 1 | 0 |
| +0F9C | Fresh DS:140C word | 0 | 0 | 0 |
| +1169 | Incoming SS:BP+6 word | 0 | 1 | 0 |
| +12D0 | Incoming SS:BP+6 word | Saved reader-failure byte | 0 | 1 |

The first wrapper, +0EBE, tests the file-copy result in AL. Zero returns
AL zero before either transition call. On the other path, both transition
results are ignored. Between them it calls the numbered reader with selector
zero and saves AL at BP-2; its own placed-object loop does not inspect that
success byte before the second transition. The optional archive-close call
can replace BP-2 with its AL result. The wrapper returns saved BP-2,
not either transition result. FND-COMBAT-128 retains archive/handle effects.

The two DS:140C arguments are separate loads, not a frozen region snapshot.
The source-table row for SAVE/18 names DS:140C and a shipped length of two
(FND-SAVE-015). Selector zero admits that row in the 61-request reader
(FND-COMBAT-035). Under the recorded table and destination binding, this is
a concrete computed region-write mechanism between the two loads. The
whole-resource reader uses the selected resource length, not the shipped
length table; two bytes is not an enforced transfer bound. Failure need not
leave the destination unchanged and there is no reader rollback. Actual
resource selection, length, source-table changes and other aliases remain
conditions. A literal-store census alone cannot exclude this producer.

The +1132 wrapper loads BP+6 independently at both transition sites and
also stores that incoming word directly to DS:140C at +1216 before passing
it to the numbered reader. Its first transition result is ignored. A zero
reader result sets BP-3 to one; nonzero leaves the initialized byte zero.
Before +12D0 it loads only AL from BP-3 and pushes AX. AH is not normalized,
but +15D7 consumes the second argument as a byte. Thus AH does not select
the local option-byte branches described here. FND-COMBAT-044 already
records the flag construction and later result consumer.

The third byte is always zero at +12D0, independently of reader failure.
Consequently this flag is not a request for +15D7's preparation branch.
On its admitted state-4 continuation, the second byte becomes both the
second and fourth arguments of child +0365, whose third byte is zero.
Failure byte one admits +0365's object-rebuild block; success byte zero
skips it under FND-COMBAT-079's byte test. The separate fourth byte one
admits +15D7's optional overlay-187 +146C call in either case. This differs
from the +0EBE calls, whose option and restore bytes are both zero.

The +12D0 return AL is saved at BP-2 and later tested after further calls.
The outer far-return epilogue does not reload that saved byte. This is
conditional result use, not an outer normalized success return.

## Interpretation

The known direct caller argument-construction component is answered: one
wrapper reloads potentially restored region state; the other forwards its
incoming frame word and keeps failure/rebuild and preparation flags separate.
The computed SAVE/18 producer must remain in Q-SOUND-004's region histories.
No numeric bound on either native region source follows from these tuples.

## Alternatives

A single preserved current-region value across numbered restoration is not
supported: the second argument is freshly loaded after a reader that can
write it. A claim that failed reading selects the preparation branch is
contradicted by the constant third byte zero. No universal writer or caller
exclusion follows. Actual frame/segment retention, child continuations,
state-4 admission, changed targets and computed callers remain unresolved.

## How to reproduce

With committed tools/research/exec-census/ at d447505f, run direct_callers.py
on DSUN.EXE for 182+15D7 and overlay_listing.py descriptor 187 ranges
70CEE..70DE7 and 70F62..7118B. Retain music-region-transition-callers.txt,
music-region-numbered-wrapper.txt and music-region-transition-wrapper.txt
under GAME_DIR/analysis/handler-census-20261106/. Reuse
music-region-overlay182-producer.txt (descriptor 182, 69E27..69FD3).
FND-SAVE-015 and FND-COMBAT-035 give the source-table and reader reproduction.
The known four calls are positive controls for the resolved trampoline query;
indirect calls, jumps, changed targets and unmapped code are outside it.
No original-game run or emulated call supplies this result.
