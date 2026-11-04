---
id: FND-ITEM-053
title: Rebuilt class flags make item eligibility an any-admitted-class test and exclude canonical row fifty
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:000F..27E5:00C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D788..0x0004D7B0
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 58616908; Python 3.12.10, Capstone module 5.0.7; composition with FND-PARTY-085/258 and FND-ITEM-048
environment: null
---

## Observation

Q-ITEM-003 retains the relation between class combinations and item eligibility.
FND-PARTY-085/258 describe the producer of details word +10 hex;
FND-ITEM-048 reads its placement consumer. This checks the complete producer
and canonical flag/mask tables, separating the local combination rule from
native lifetime and every manual item-name restriction.

### Class-flag producer

27E5:000F initializes its accumulator to zero and copies twenty words from
DS:0788 to its local table. It selects the supplied slot's 66-byte details
record directly through DS:19C5 plus 66 times that slot. For each of three
class positions, it may OR the table word for the zero-extended class code
into its accumulator. The admitted table domain is codes 0..19; no own
class-code bound check supplies that domain.

Position zero always contributes its class flag, independently of its level.
For positions one and two, a zero level is skipped. Nonzero later levels
must be below a limit: the first position's level for origin code one, or
100 for every other origin. Level bytes are zero-extended for comparison.
Thus equality to the current level excludes a former class for origin one,
and a later level of 100 or above is excluded even for other origins. This
matches FND-PARTY-258's retained dual-class interpretation without treating
all stored class bytes as admitted active classes.

The complete copied flag mapping is:

| Stored class code | Flag, hexadecimal | Existing class-label binding |
|---|---|---|
| 0 | 0000 | None |
| 1,2,3,4 | 0001,0002,0004,0008 respectively | Cleric variants |
| 5..8 | 0010 | Druid variants |
| 9 | 0020 | Fighter |
| 10 | 0040 | Gladiator |
| 11 | 0080 | Preserver |
| 12 | 0100 | Psionicist |
| 13..16 | 0200 | Ranger variants |
| 17 | 0400 | Thief |
| 18 | 0800 | Not assigned a class name here |
| 19 | 1000 | Not assigned a class name here |

FND-PARTY-057 binds names for codes 1..17. The extra two table words do
not establish selectable named classes. The union of all twenty words is
1FFF hex. The completed producer replaces details word +10 with its computed
OR; it does not OR into the old stored word. Therefore a retained result
from this admitted producer has no bits outside 1FFF.

### Composition with item eligibility

Let A be the class positions admitted above, f their table flags and M the
shared item's word +0C. After this producer completes, with the same details
record and its class word preserved, FND-ITEM-048's final class predicate is
M AND OR(f(c) for c in A) nonzero. This is equivalent to at least one
admitted class flag overlapping M. A disallowed second class does not cancel
an overlap contributed by the first. A class excluded by the producer's
level rule contributes nothing, even when M contains its flag.

This predicate is still preceded by the item's byte-four 10/20 hex rejection
and the origin-code-eight category filter. The main placement gate's L>=18
branch bypasses this eligibility helper after its other checks. The local
any-class rule is not a general assertion that any inventory action succeeds
whenever one class permits an item.

### Complete canonical source-mask census

The bounded ALL/4 read takes word +0C from all 102 fifteen-byte source rows.
There are 34 distinct masks and no zero mask. Every row except row 50 has
only bits within 1FFF. Zero-based row 50 alone has mask 2000 hex, with no
bit in the bounded producer's union. Hence for an unchanged canonical row
50 and a retained admitted producer result, the final class-overlap predicate
is false for every class combination, including codes 18 and 19.

This does not say row 50 is unusable in every context. Runtime writers can
change either word, and the carried-position path bypasses this helper.
FND-ITEM-036/041 and Q-PARTY-115 retain separate row-50 Look/name questions;
no item name, encounter or gameplay purpose is inferred from this mask.

### Preconditions

The copied table, record and source masks must retain their fingerprinted
values. The local table is written through an SS pointer and later read
through a near DS offset; DS/SS binding, valid nonwrapping frames, record
spans and disjoint state remain required. The fixed copy's forward contract
is FND-CONFIG-099. Class codes, slot/record identity and later writes need
independent native admission. Reaching this producer once does not prove its
result is still the eligibility input at a later call.

## Interpretation

The remaining local multiclass ambiguity is removed: this consumer accepts
any overlap with the OR of admitted class flags, not the intersection of all
stored classes' permissions. The origin/level producer rule decides which
positions contribute before the item predicate runs. It does not select a
single universally most restrictive class.

Q-ITEM-003 retains producer-to-use retention, caller/record/near-state and
runtime table admission, class-mask mutations and the relation between named
manual weapons/materials and individual source rows. The source census does
not prove those names or every independent equipment route. Q-ITEM-005 keeps
complete shared-field consumer/writer coverage. No rule-status, parity or
complete-reading promotion is claimed.

## Alternatives

ANDing class flags would change the producer; requiring overlap for every
stored class would change the consumer. Including all three class positions
without the level/origin tests would change the producer's admitted set.
Reading an unrelated stored bit as evidence that this producer emits 2000
would omit its zero initialization and replacement store. Out-of-range class
codes are outside this bounded table composition.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and OBJEX.GFF length 6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d. Read 27E5:000F..00C1 with
resident_listing.py, preserving the first-position branch, zero-level skip,
strict limit and replacement store. Reuse FND-ITEM-048's final overlap test.

Run report.mjs table for DSUN.EXE start 4D788 hex, count twenty, stride two,
one unsigned two-byte flag at zero; count evidence is the forty-byte copy.
For OBJEX.GFF use start 1F291 hex, count 102, stride fifteen, one unsigned
two-byte classMask at offset twelve; count evidence is ALL/4 length 1530.
OR every flag, check all source masks and retain the exact row-50 identity.

Reports placement-class-mask-producer.txt, placement-class-flags.json and
placement-class-mask-source.json, corresponding -controls.json configurations
and -repeat.json reports remain under GAME_DIR/analysis/item-removal-20261104/.
Both tables repeat byte-for-byte. Their SHA-256 values are
03ae3c2b4c1bf904811ffe86a6fae9bde67a6665705d0bebdfb3407362745a68
for flags and 87020e8fec2109702720bc95a5653fe276418fe710e0c73922f1311593b155e4
for masks. No original code, source table fixture or asset is committed or
executed.
