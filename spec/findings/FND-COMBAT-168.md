---
id: FND-COMBAT-168
title: Preserved non-five roots exclude enumeration zero-result exits and retain their original cleanup root index
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:248D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:27B7..2D40:286C
tool: tools/research/exec-census/resident_listing.py at 15b98f5f; Python 3.12.10; Capstone 5.0.7; retained complete listing and composition with FND-COMBAT-164/165/167
environment: null
---

## Observation

Q-COMBAT-067 retains zero-result root-field admission separately from save
admission. FND-COMBAT-167 describes both bounded positive prefixes and an
invalid-kind exit that resets count and overwrites the root index. This
reading distinguishes which root kinds can reach those exits under its
storage and returning-callee conditions.

### Conditions

Use FND-COMBAT-167's valid, separate output/state/frame storage, near
DS-to-SS identity, initialized pending bounds and admitted returning
getter/copy conditions. The input root is unsigned below 9999 and its
kind is one through four when enumeration begins. Its kind remains valid
through the first output stores. Those stores, all callees and later
local writes preserve the root entry's kind word after it is written.
There are no external writes between a candidate's final kind test and
the following loop-header or finish kind test. The admitted read/write
spans and stack accesses remain valid. These conditions admit a class;
they are not observations of a native cleanup call.

The proof does not require every object kind to remain immutable during
getter/restore calls. Candidate validation follows those calls. It does
require their results and intervening writes to obey the named storage
and control-flow conditions.

### Root-relative filter and all count-reset paths

The first iteration passes the initial kind-zero and unsigned-above-five
rejections. It emits the root with kind word one through four, original
root index at word +6 and count local still zero. No child call precedes
that root kind/index construction.

After the descendant-scan and pop calls, +2329 loads the output BASE far
pointer; +232C compares word +2 at that base with five. This tests the
root entry's kind on every iteration, not the most recently emitted
entry's kind. With the preserved one-through-four root, +2331 cannot
bypass validation even if a later emitted entry has kind five.

A popped index equal to 9999 goes directly to the increment/finish path.
For another index, validation rejects record number 9999, signed-negative
record numbers, kind zero, kind six, kind four and unsigned kinds above
five, retrying the pending pop. Any other accepted candidate consequently
has kind one, two, three or five. This is a kind/record filter, not a proof
of a valid object index, record capacity or ownership.

After the final acceptance test, +23A3 increments the output count;
the parent/selector reloads and count/index loop checks make no call.
An unsigned next index below 9999 reaches the next outer kind check only
with the just-accepted valid kind. A next index at least 9999 finishes.
Thus the +21DC invalid-kind count reset is unreachable in this class.

There is one other explicit count reset after initialization. On finish,
+23D4 skips the final kind check for an unsigned current index at least
9999. Otherwise +23E4 checks its kind and +23EC resets count only for
an unsigned kind above five. Such an index was just accepted with kind
one, two, three or five; the no-intervening-write condition excludes
that reset too. At the 79-entry cap, the same final check applies to the
accepted next candidate even though no next output entry is emitted.

At least the root was emitted. Every normal finish has count one through
79 under FND-COMBAT-167's progress/bounds conditions. Saturation may lose
candidates and kind-four filtering may omit them; neither gives zero.

### Cleanup root index

The ordinary entry store at +2295 writes each entry's word +6. After
root construction, later entry stores address positive ordinals. The
invalid-kind base-word overwrite at +21DF is excluded above. The final
sentinel is at offset 10*N with N positive and the root count store
changes byte +1 only. Root word +6 therefore remains the input root
index through enumeration return, and its kind word is preserved.

For root 319 of kind two this supplies positive count, root kind two and
root word +6 equal to 319 to 2452. If map construction preserves the graph
and its admitted storage, its forced map[0] is consequently 319, as
FND-COMBAT-164 and FND-PARTY-214 describe. With the additional map-value,
record/pool, alias and stack conditions of FND-COMBAT-165, that finding's
finite rooted-deletion proof applies. No map-value or release-state
admission follows from the count/root proof alone.

## Interpretation

For the admitted preserved kind-two root, zero-result retained fields
are not an additional cleanup outcome to simulate: both count-reset
paths are excluded and the original root index is retained. The remaining
obligations are actual root/storage/getter/map/pool admission and later
release effects. FND-COMBAT-167's general zero-result warning remains
necessary for other root/state classes.

No native run, save admission, confidence/parity promotion or complete
reading of the cleanup subsystem is claimed.

## Alternatives

A kind-five ROOT takes the +2331 validation bypass. A kind-five descendant
under a preserved kind-two root does not. Treating that comparison as the
current entry's kind would incorrectly include a bypass in this proof.
A changed root output word or intervening aliased write invalidates the
conditions. Root kinds zero/above five and unsigned out-of-range inputs
are also outside this class; the general invalid-result contract remains.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. Use the project-local
evidence Python and resident_listing.py on the complete half-open ranges
in Locations. The approved reader verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and the declared load-segment-1000 mappings.
Reuse GAME_DIR/analysis/hit-incoming-20261104/temporary-enumeration-prefix.txt,
whose complete enumeration, cleanup wrapper and map-builder bodies cover
these ranges. Follow all count stores, root versus current output pointer
loads, +232C/+2331, every retry, +23A3..+23F1 and root word +6 writes.
Original-derived output remains only in GAME_DIR; no original ran.
