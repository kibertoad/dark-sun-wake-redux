---
id: RULE-ITEM-008
title: DATA 235 hands the target's first held item of the lowest byte 0x12 below 6 to overlay 173 +2B88 when a d20 falls below 15 less the item's byte 0x16
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-CONFIG-143, FND-PARTY-259, FND-EXE-600, FND-PARTY-248, FND-ITEM-013, FND-PARTY-214, FND-PARTY-218, FND-ITEM-014, FND-ITEM-016, FND-ITEM-017, FND-ITEM-018, FND-MAGIC-024, FND-SOUND-007, FND-EXE-602, FND-ITEM-028, FND-ITEM-031, FND-ITEM-032, FND-COMBAT-032, FND-COMBAT-033, FND-COMBAT-046]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-007]
---

## Summary

`lowest_item_test` goes through the target's held-object entries of kind 1 and keeps the first
whose item has the lowest byte `+0x12` below 6. With none it returns 1 and makes no roll.
Otherwise it rolls a d20 and, below 15 less the item's signed byte `+0x16`, calls overlay 173
`+2B88` with the list, the item, the entry and 1. It returns 1 on every path.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 235, after queuing `DATA` 311
for the target (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and bytes `+0x12` (unsigned) and `+0x16`
(signed) of each listed item's record in `item_records`.

The signed +0x16 byte is the item bonus used by the name formatter and
recorded combat consumers (FND-PARTY-248). Byte +0x12 remains the numeric
ranking input here; its complete writer and semantic contract is still open.

## Procedure

```text
define lowest_item_test(slot):
    let list = fn_2D40_2196(slot)
    let best: INT16 = -1
    let lowest: INT16 = 6
    for e in 0..count(list):
        if list[e].kind == 1 and INT16(item_records[list[e].item][0x12]) < lowest:
            best = e
            lowest = item_records[list[e].item][0x12]
    if best == -1:
        return 1
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    if INT16(roll_sum(1, 20)) < 15 - INT16(INT8(item_records[list[best].item][0x16])):
        fn_5671_0075(list, list[best].item, best, 1)
    return 1
```

## Outputs

Always 1, so the switch always sets bit 0x02 of the hit's flags byte (FND-ITEM-010); and the call
of `fn_5671_0075` when the roll succeeds.

The helper tests shared type byte +0x0B mask 0x02. With the mask clear it
requests removal through `fn_2D40_2452` and returns zero if that call returns.
With the mask set and attached-item quantity word +2 initially zero, it
returns zero without requesting removal. Otherwise it stores the low
16 bits of quantity minus the requested count (one here). Every nonzero
result is retained and returned, including wrapped or signed-negative
words; a zero result requests removal after the store and returns zero
if deletion returns (FND-ITEM-016). This is the local arithmetic contract
under stable, valid record storage. Caller admission, independent writers
and complete removal effects remain open.

When recursive deletion reaches its kind-one branch, it passes the attached
record index to a helper that links the record into the free list and clears
its identifier word. This is the helper's own local effect under stable,
disjoint storage (FND-ITEM-013). The deletion body's local order is kind
release, conditional parent-field request, map clear, conditional reparenting, then
recursive calls for retained children. Its reparenting failure branch can
be non-returning; admitted state and whole-callee effects remain open
(FND-ITEM-014, FND-PARTY-214, FND-PARTY-218).

Under admitted near-pointer, frame and record storage, the hit wrapper uses
result mask 0x02 set with damage mask 0x01 clear to select presentation
requests. Otherwise a zero combatant record word +0 also selects that path.
Mode gates control its subsequent requests, and the wrapper returns the flags
byte. These gates do not prove successful visible feedback or later caller
behavior (FND-ITEM-017, Q-ITEM-010).

Known hit-call continuations handle that byte differently: one wrapper saves
and forwards it across queue processing; the queue continuation does not
explicitly interpret it; casting zero-extends and aggregates it, tests only
mask 0x01 at its recorded follow-up gate, and returns separate bookkeeping.
These local paths do not establish complete caller coverage or whole-child
preservation (FND-ITEM-018). The known direct outer continuations forward,
replace or independently interpret the byte; the remaining resident kind
dispatcher reloads its own bookkeeping byte (FND-ITEM-028). This settles
local register handling, while other incoming routes and frame admission
remain Q-ITEM-010, Q-ITEM-011 and Q-ITEM-013.

For admitted aligned effect rows, valid counts/indices, complete source
spans and preserved copy storage, the known row moves do not introduce new
active code values. Insertion retains the old active codes and adds its
supplied code (FND-ITEM-032). Removal's extra boundary-row copy lands outside
the reduced active count (FND-COMBAT-032). Owner transfer preserves the
saved row's code and replaces only its owner (FND-COMBAT-033). Thus these
moves preserve an existing active-code bound under those assumptions; they
do not establish that bound initially or validate restored rows. The known
insertion route's conditional 1..127 bound still permits bitmap offset 15,
so it does not establish a fifteen-byte output span (FND-ITEM-031/032).
Other producers, copy admission and frame aliases remain Q-ITEM-013.

The shipped RESOURCE.GFF also contains a concrete producer-field exception
to a proposed 0..112 bound: DATA/313's signed byte +0x19 is positive 127
(FND-COMBAT-046). Its reviewed handler-default continuation does not locally
reject that value. This source and local-path evidence does not prove an
admitted insertion of 127, a particular bitmap outcome or a native failure;
selection, intervening state and restored rows remain Q-ITEM-013.

The first request selects the sound effect numbered by DATA byte +0x16.
Its routine applies sound-setting and driver gates, preferring a BVOC resource
before the numbered loose SOUND file (FND-SOUND-007). The mode-one visual
helper places and draws an object subject to its view and creation gates;
the alternate helper requests frame 4 of BMP 19001 through its slot, resource
and layout gates and recorded counter wait (FND-EXE-602). The visual number
comes from DATA word +0x14 or the recorded fallback (FND-MAGIC-024). These
requests do not guarantee audible or visible output. Remaining feedback
admission/effects are Q-ITEM-012; lower-level audio is Q-SOUND-002.

## Edge cases

A later entry with the same byte `+0x12` does not replace the first. Each call that finds an entry
draws once. A byte `+0x16` of 0 succeeds on 1 to 14; 14 or more never does, and -6 or less always
does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Canonical RDFF byte +12 values are 0,3,4,5,6. The known initializer writes
  one, so source values are not a universal runtime range; independent writers
  and use-time admission remain Q-ITEM-006 (FND-ITEM-043).

- Recursive removal/storage admission remains Q-ITEM-004. The quantity
  helper `fn_5671_0075` caller, writer and record-admission contract is Q-ITEM-007;
  its local arithmetic and zero branches are recorded above.
- Independent writers and admitted uses of the +0x12 rank and +0x16 signed
  bonus remain Q-ITEM-006; hit-result segment identity remains Q-ITEM-011
  and frame/alias admission Q-ITEM-013,
  feedback effects Q-ITEM-012 and other incoming routes Q-ITEM-010. Known
  direct local consumers are recorded by FND-ITEM-028.
- Shared effect-row admission uses Q-COMBAT-009 for codes, Q-COMBAT-017
  for counts, Q-COMBAT-018 for restored rows/count, Q-COMBAT-020 for mutation
  callers and Q-COMBAT-024 for rows retained across games. These are
  prerequisites of the bitmap/frame reading, not separate item-table copies.
  The restore prerequisite separates copy admission (Q-COMBAT-063), runtime
  supplying resources (Q-COMBAT-064), destination storage (Q-COMBAT-065)
  and reached child effects (Q-COMBAT-066).
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).
