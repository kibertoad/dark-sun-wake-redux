---
id: FND-ITEM-048
title: Inventory placement orders category container capacity and hand checks before origin and class eligibility
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077514..0x0007761B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007761B..0x00077A92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077D26..0x00077F3A
tool: tools/research/exec-census/overlay_listing.py; Python 3.12.10, Capstone module 5.0.7; FND-EXE-616 position categories and FND-PARTY-217/220/221 graph bindings
environment: null
---

## Observation

This reading answers the local placement and class predicates in Q-ITEM-003,
not its spell/store questions or universal native admission. Overlay 189 starts
at file 749B0 hex. Its +2C6B takes an attached-item index, logical position L
and message-enable byte. Assume valid nonwrapping attached/type/details/map/
graph spans, L in 4..47, stable table pointers and current-character binding,
and disjoint stack/global/record storage. Child returns and preservation are
conditions below, not inferred successes. The code does not itself establish
those bounds or lifetimes.

### Initial and category checks

The current character's 66-byte details word +0E must be nonzero. The item's
23-byte record word +0A selects type T; unsigned T at least 9999 rejects.
That sentinel check is not a 102-row table-capacity check. Type byte four
with either mask 10 or 20 hex set rejects next. Type byte five is retained as
category C, and byte zero as flags F.

When DS:617D is nonnull, map words at byte offsets 1C and 0E select entries
of the current character graph. Non-FFFF entries supply attached-record type
words as the two other-hand candidates; absent entries retain 9999. The
local +3376 gate is then called with L and message-enable. AL zero rejects
before category matching. Its separate snapshot/record dependencies remain
conditions, not evidence that every source slot may be removed.

For L below 18, compare C with byte L-4 of 506F:0000. FND-EXE-616 records
the fourteen category bytes as 3,11,12,5,9,2,10,6,7,1,5,9,8,4. Equality passes
this stage. Mismatch rejects, with the cannot-ready message when C is zero
and illegal-placement otherwise, except for the earlier special L=11 case:
absolute signed item word zero equal to 31961 takes its own message sequence
and rejects. L at least 18 skips category matching. This gate uses logical
indices, not the position byte before FND-PARTY-217's addition of four.

### Container, capacity and count checks

Next, type byte eleven mask 08 hex rejects L in 36..41 with the bag message,
or L in 42..47 with the box message. Other positions pass this test. This
binds the mask in this consumer without declaring every possible container
or every use of the bit.

The gate then calls 1AA0:0321 with the current character and its recorded
local/output and packed arguments, retaining AX as W. It calls 27E5:0236
with the current character and one, obtaining K. Signed K below W rejects
with the load message; equality passes. FND-ITEM-049 subsequently binds W
to the narrowed minimum aggregate and K to the second Strength-table column.
Actual traversal/getter, output/near-pointer and table admission remain open.

It constructs a graph for DS:1A32 at 5072:0040, keeps the returned AX, and
constructs the current character graph using DS:6189 plus four times that
character index. The sum of both returned words wraps to sixteen bits.
Signed sum at least 80 rejects with the item-count message. This is not a
checked nonnegative sum or proof that either graph fits its buffer.

L at least 18 now returns AL one. Thus this gate's backpack/container
positions bypass the remaining hand and character-eligibility checks, while
still requiring all preceding checks. It does not write the position here;
child side effects and subsequent transfers remain separate.

### Hand conflicts and character eligibility

For L=7 the other-hand type is from map byte offset 1C; for L=14 it is
from offset 0E. All other L below 18 use sentinel 9999. With a nonsentinel
other type, these checks run in order:

1. Other type byte eleven mask 40 hex rejects with the two-handed-in-use
   message.
2. When both types have byte-zero mask 01 hex set and both unsigned byte-two
   values exceed 30, reject with the two-heavy-weapons message. Equality
   30 in either type does not take this rejection.
3. New type byte eleven mask 40 hex rejects with the two-free-hands message.

After those checks, or immediately with the sentinel, when F mask 04 hex
and the other type's byte-zero mask 04 hex are both set, reject with the
two-shields message. A sentinel skips that second type read.

The remaining branch calls local +2B64 with the item index, message byte
and current character details pointer, returning that child's AL. This
helper first rejects type byte-four masks 10/20 hex. If details origin byte
+12 hex equals eight, only categories 0,11,12,5,7 pass its category filter;
other origins bypass that filter. It then returns whether type word +0C
shares at least one bit with details word +10. This is an overlap test,
not equality or a requirement to satisfy every bit of a multiclass mask.
It does not independently test a literal weapon-name list.

Message-disable bypasses these ordinary rejection messages, not the
predicates. A message helper's complete effects/preservation remain outside
this reading; no unconditional message-mode equivalence is claimed under
aliases or arbitrary callee effects. The special L=11 sequence and the
snapshot-based removal gate retain their own child contracts.

## Interpretation

This removes local order, thresholds, origin-category filtering and class
mask combination from Q-ITEM-003's uncertainty. It adds direct consumer
bindings to Q-ITEM-005: byte-five category, byte-four 10/20 masks, byte-eleven
08/40 masks, byte-zero 01/04 masks, byte-two unsigned threshold and word +0C
class overlap. The automatic equipment route of FND-EXE-616 remains distinct:
its origin filter and hand accumulation are not substitutes for this gate.

Actual incoming event/position producers, snapshot/removal and capacity
children, native table/graph/current-character/stack admission and all
intervening state remain open. This does not promote the manual-derived
rules, implementation parity or complete-reading status.

## Alternatives

A blanket rule that every carried item must pass character eligibility
would add a check this local L>=18 success branch bypasses. Requiring all
class bits would reject overlaps the helper accepts. Unsigned or unbounded
count arithmetic would change the recorded rejection domain. Category
compatibility alone does not bypass later container, capacity or hand checks.

## How to reproduce

Use installed DSUN.EXE length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 189
over file ranges 77514..77691, 7761B..77A92 and 77D26..77F3A (hexadecimal).
Compare the first two entry/body ranges with their function inventory rows.
Trace each conditional edge to the common AL-zero exit or final child call;
retain ignored padding jumps separately from reachable paths. Reuse the
category table binding of FND-EXE-616 and graph projection of FND-PARTY-217.

Retained listings placement-prefix.txt, placement-checks.txt and
placement-removal-gate.txt are under GAME_DIR/analysis/item-removal-20261104/.
The middle report spans 77680..77B30 and includes the remainder of the gate;
the prefix supplies its entry. No original code, text, payload or fixture is
committed. No original function or game was executed.
