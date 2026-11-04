---
id: FND-ITEM-049
title: The placement load gate compares a narrowed minimum aggregate with the second Strength-table column
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0321..1AA0:039A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:03D2..1AA0:06BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:0236..27E5:0283
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077893..0x000778EB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D425..0x0004D441
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D7B0..0x0004D818
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 26a392cb; Python 3.12.10, Capstone module 5.0.7; FND-PARTY-234 and FND-CONFIG-140/145
environment: null
---

## Observation

FND-ITEM-048 leaves its load comparison as K versus W. This reading removes
the local result-selection and Strength-table uncertainty. It does not assume
that a load message establishes a sum of carried item weights.

### Caller and aggregate selection

FND-PARTY-234 binds overlay 189 +2EFD to 1AA0:0321 with the current character
handle, a near list whose first word is 89, starting ordinal zero, exclusive
end one, result selector zero, traversal mode zero and comparison value zero.
The pair of words zero/one is one packed doubleword argument push; it does
not select aggregate result one. The main caller retains only returned AX.

0321 initializes seven local doublewords from the 28 bytes at DS:0425. The
fingerprinted installed image has first doubleword 99999999 and six zeros.
It makes one outer call to 03D2 for this caller, provided the handle is not
9999. It then returns the doubleword at four times its result selector from
the local aggregate array. Selector zero therefore returns the first member.
The returned high word is not consumed by the placement comparison.

At 03D2's mode-zero branch, the supplied list's first word selects the field
getter, here field 89. Its DX:AX result is passed as the sample value to 063F.
That helper skips all aggregate changes when DS:40C4 is nonzero. Otherwise:

- Aggregate member zero becomes the lesser of its current value and the
  sample, using a signed 32-bit comparison.
- Member one becomes the greater, using a signed 32-bit comparison.
- Member two adds the sample as a wrapping doubleword; member three counts
  samples. Members four through six retain equality/greater/less flags
  against the supplied comparison value.

Only member zero is selected by this placement caller. In particular, the
sum in member two is not returned here. For the actual successful samples
X encountered under preserved aggregate storage, the selected value is
min(99999999, X1, ..., Xn). With no samples, the initial value is retained.
This formula includes failed-getter exclusion and the initializer clamp;
it does not silently count a failed getter's returned zero as a sample.

03D2 continues through 0566's selector enumeration and 051C's pending pops.
It does not merely request field 89 on the root. FND-CONFIG-145 supplies
those local helper contracts; FND-COMBAT-167 bounds initialized pending
counts to 0..31 but retains saturated candidate loss and alias conditions.
Thus these are encountered successful samples, not a proof that every item
in an arbitrary object graph is visited exactly once. Traversal cycles,
getter mutations, current-object snapshots and actual termination/storage
admission remain separate obligations.

### Strength-table lookup and final width

27E5:0236 copies 104 bytes from DS:07B0 into its SS-local array. It resolves
the supplied character through the three-byte slot table, selects its
49-byte combatant record and zero-extends byte +19 hex as S. FND-MAGIC-013
binds this byte to Strength. The helper reads a word at four times S plus
twice its second argument's low byte. This caller supplies one, selecting
the second word of each four-byte row. No local bound check admits S or
that selector to the copied table.

For S in 0..25, the complete installed second-column mapping, grouped only
where adjacent rows agree, is:

| Strength S | Returned K |
|---|---:|
| 0..1 | 30 |
| 2 | 50 |
| 3 | 100 |
| 4..5 | 250 |
| 6..7 | 550 |
| 8..9 | 900 |
| 10..11 | 1150 |
| 12..13 | 1400 |
| 14..15 | 1700 |
| 16 | 1950 |
| 17 | 2200 |
| 18 | 3300 |
| 19 | 6400 |
| 20 | 7000 |
| 21 | 8100 |
| 22 | 9700 |
| 23 | 11300 |
| 24 | 14400 |
| 25 | 17500 |

The placement gate compares signed 16-bit K with signed 16-bit lowWord of
the selected minimum. It rejects only when K is less; equality passes.
With no successful sample, the initial 99999999 narrows to unsigned 57599,
or signed -7937. For the admitted table rows above, that value does not
cause this comparison to reject. This is a conditional arithmetic result,
not a native empty-inventory observation.

As another arithmetic distinction, successful samples 100 and 1 select
minimum 1, not sum 101; a K of 30 would pass this comparison. No claim is
made that those samples and that K occur together in the original. The
example distinguishes the operations without supplying speculative gameplay.

### Preconditions

The fixed copies use 1000:0452's forward-copy contract (FND-CONFIG-099).
Their sources must retain the fingerprinted values. Near accesses to local
aggregate, pending state, selector list and copied Strength table must denote
the intended SS-local storage, with valid nonwrapping spans and disjoint
frames/state; DS=SS at these uses is a sufficient binding, not proved here
by startup equality. Getters and traversal/copy children must return with
aggregate/caller storage preserved except for the documented outputs.
Slot, record, descriptor and metadata tables, current character/temporary
object state and all intermediate indices need independent admission.

## Interpretation

The gate's local capacity operand is a narrowed minimum of successful field-89
samples. It is not justified to replace it with a carried-weight sum merely
because member two exists or because the rejection message mentions load.
FMT-ITEM-002 separately binds kind-four field 89 to byte two; actual sample
kinds, signed getter widths and graph selection still determine values.
FND-ITEM-050 subsequently bounds the preserved canonical byte-width sample
class and its complete Strength-row rejection domain. Other native classes
and actual use-time admission remain open.

Q-ITEM-003 no longer needs the aggregate result selector, final comparison
width or admitted Strength-table mapping. It still needs native caller,
copy/table/near-state and traversal/getter admission, and snapshot/removal effects. Spell/store work is Q-ITEM-014/015.
No confidence, parity or complete-reading
promotion follows from this local composition.

## Alternatives

Selecting member two would return a sum; this caller selects zero. Treating
the full returned doubleword as W omits the caller's AX-only retention and
signed word comparison. Treating the traversal as read-only ignores the
conditional temporary-slot writes and snapshots of FND-CONFIG-140/145.
An out-of-range Strength byte is not clamped by this helper.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. With resident_listing.py read
1AA0:0321..039A, 1AA0:03D2..051C, 1AA0:051C..06BB and
27E5:0236..0283. Retained placement-capacity-children.txt and
placement-aggregate-children.txt are under GAME_DIR/analysis/item-removal-20261104/.
Reuse FND-ITEM-048's placement caller and FND-PARTY-234's argument binding.

Run report.mjs table with that source fingerprint. The seed table has start
4D425 hex, count seven, stride four and one unsigned four-byte field at zero.
The Strength table has start 4D7B0 hex, count 26, stride four and unsigned
two-byte fields at zero/two. Counts come from the fixed copy sizes, not an
assumed score range. Require S in 0..25 separately for an in-table result.
Configurations placement-aggregate-seed-controls.json and
placement-strength-table-controls.json and corresponding reports/repeats stay
in the same private directory. Both reports repeat byte-for-byte. Their
SHA-256 values are 62ef97eee478dc29b05044da8151e29eb1d196c12dbf362e9a4665f9a03b8fce
and 3ad0eaeb95ec3d26d5ba59ff7658863cac0d8335f683c72039be365812eeb82b
respectively. No original code/data fixture is committed or executed.
