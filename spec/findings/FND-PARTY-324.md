---
id: FND-PARTY-324
title: Known literal sound-mode producers yield only zero or one and adjacent stores do not overlap the mode byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:010F..277B:012E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:00E3..277B:00E8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0233..277B:0236
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E3F7..0x0004E3F8
tool: window_operands.py and resident_listing.py at 0a455348; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; existing FND-SOUND-010, FND-CONFIG-218 and FND-PARTY-320/322
environment: null
---

## Observation

Q-PARTY-136 retains actual mode admission for setup/reset and ordinary
sound shutdown. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

FND-SOUND-010 already establishes the shipped byte at modeled
57E0:13F7 as one. Its entry-based parser reading binds -H and
-L to the store of one at 277B:0119, and -M to the store
of zero at 0126. This reuses those switch bindings rather than
inferring them from an isolated store.

The controlled overlap window 13F4..13F8 covers possible encoded
displacements that overlap byte 13F7 for operands up to four bytes.
It reports 53 operands in inventoried instruction ranges, with four
write operands, all in the established resident main/parser routine:

| Site | Local write | Relation to the mode byte |
|---|---|---|
| 277B:00E3 | Byte DS:13F6 becomes zero. | Ends before 13F7. |
| 277B:0119 | Byte DS:13F7 becomes one. | Known -H/-L producer. |
| 277B:0126 | Byte DS:13F7 becomes zero. | Known -M producer. |
| 277B:0233 | Word DS:13F4 receives AX. | Ends at 13F6, before 13F7. |

The two mode stores independently established in FND-SOUND-010
are positive write controls. The known setup/shutdown mode reads
from FND-PARTY-320/322 are present as read controls. No matched
routine contains a decoded string store in the report's linear ranges.
This does not cover string operations in other routines or prove
their destinations unrelated.

The report retains six raw candidates outside the established starts.
Three have write-like access: resident 2D0F:0007, overlay 182
+2383 and overlay 187 +277E. All are marked inside another
decoded instruction in the established pass, not proved alternate
entries. The other candidates are reads at overlay 187 +2566/
+25EC and an indexed jump at overlay 199 +1128. No group of
raw candidates is promoted to reachable code or discarded as a
whole-program absence proof. Adjusted pointers, different displacements,
register-only destinations, wider operands, block/string copies, physical
aliases, changed code and alternate streams remain outside the
classified established-write set.

### Predicate consequences are conditional on actual state history

Starting with the shipped one, any sequence containing only the two
known mode stores leaves this byte in the set zero or one. It does
not produce FND-PARTY-322's illustrative mode-two distinction.
For that restricted state history, nonzero and equal-to-one tests of
the same unchanged byte select the same branch. They must still
remain separate reads across intervening helpers or interrupts.

The -H/-L one-store also sets other sound/speech bytes; the -M
zero-store does not locally undo those other fields. The mode byte
is not a combined settings record. FND-CONFIG-218 likewise already
distinguishes the F4/F5 own preference changes at DS:1436/1435
from their reads of DS:13F7. The F5 message's read of global
sound mode does not make it another local mode writer. Its reached
services and excluded write mechanisms are not thereby dismissed.

## Interpretation

Q-PARTY-136 can remove unclassified meanings for these four overlap
writes and arbitrary byte values from this restricted producer model.
It retains actual DS/field history through excluded mechanisms, state
retention between separate reads, alternate parser/root admission and
FND-PARTY-320/322's ready/reset/shutdown conditions. A concrete
nonbinary mode requires a changed-state mechanism outside the two
known stores; none is established here. No native interval invariant,
queue closure or complete reading follows. FMT-PARTY-007 stays
supported.

## Alternatives

- A nearby word store changes mode: its two-byte extent ends before
  the byte in question.
- Known switch stores can produce every nonzero mode: they write
  only one and zero, starting from the recorded shipped one.
- Equal predicates under a restricted value domain prove equality
  across time: actual intervening writers and bindings remain open.
- F5 changes global mode because its message reads it: its own
  preference effect is on the separately recorded effects setting.

## How to reproduce

Reuse FND-SOUND-010's source and switch-table reading and
FND-CONFIG-218's own preference effects. Run window_operands.py
with installed DSUN.EXE, the committed installed inventory and
13F4..13F8. Decode the resident locations above, retaining each
write's width and segment instead of classifying by displacement alone.

Reports under GAME_DIR/analysis/handler-census-20261106/ are
getter-sound-mode-writers.txt, getter-sound-mode-producers.txt and
getter-sound-mode-adjacent-word.txt. Their exploratory prefixes are
not entry evidence; the two mode stores already have entry-based
provenance in FND-SOUND-010. No original game, emulator session
or emulated call supplied evidence here.
