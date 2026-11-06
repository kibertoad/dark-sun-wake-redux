---
id: FND-PARTY-279
title: The selection-count store forces the aliased low byte away from E9 under the list builders' explicit capacity and frame conditions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009785F..0x00097902
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062500..0x00062592
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006260A..0x000626B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097BCC..0x00097C50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061540..0x00061573
tool: tools/research/exec-census/overlay_listing.py at 281591b9; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-280/243/275/276 reuse
environment: null
---

## Observation

Q-PARTY-124 retains 27 decoded writer-value matches after the constant
store exclusions. One is overlay 211 +1AEF, a word store at DS:9D79
overlapping selected word 9D7A. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

### Dispatcher and exact aliased byte

Overlay 211 +1A4F initializes DS:9C1E to zero, reads the current
member's mode byte, and selects these paths:

| Mode | Count producer |
|---|---|
| 1 or 2 | Overlay 177 +0400 with list kind one or two, sign-extended member level and output far pointer DS:9C20. |
| 3 | Overlay 211 +1DBC with the member's discipline byte and the same output far pointer. The helper reads only the byte, not the unnormalized high half of the pushed word. |
| All other values | No list-builder call; retain the initialized zero. |

For a builder path, +1AE9 stores returned AX into DS:9C1E.
+1AEC loads that word and +1AEF stores it at DS:9D79, with no
intervening call or segment assignment. This names the value at that
store, not later list cleanup or preservation until another consumer.

Word 1001 requires low byte E9 and high byte 03. The selected word
begins at 9D7A, so its low byte is the count's high byte. A count high
byte of zero or FF rules out 1001 there regardless of the neighboring
byte at 9D7B.

### Spell-list capacity contract

Overlay 177 +0400 uses capacity 235 for exact level -1 and 21 for
every other level word. Its counter SI starts at zero. After a successful
predicate it tests the output far pointer, then compares SI with capacity
before the list store. If SI is at least capacity it returns that capacity;
otherwise it stores and increments SI. A rejected predicate leaves SI
unchanged. The ordinary exhaustion return is SI.

The dispatcher supplies a nonnull far pointer because its offset is
9C20, independent of the segment value. Under retained argument/frame
and counter state, the builder therefore returns 0..235 for level -1
and 0..21 otherwise. This uses the actual capacity branch, not the
contents or level-validity of the source boundary tables.

The predicate +050A saves incoming SI at +050D and restores it at
+05B4 on its normal returns. Its own alternate branches share that
epilogue. Its reached helpers need not be assumed to preserve the
caller's live count register: the predicate restores its saved SI.
Actual writes or aliases to that saved word, the capacity/end locals
or the output-pointer argument are not excluded by the push/pop pattern.
The supplied list storage must remain distinct from those live frames.

Even if the predicate's internal calls affected the candidate register DI,
that alone would not bypass the nonnull-output SI capacity check. No
termination claim or valid sequence of list entries is inferred from the
count-result bound.

### Discipline-list capacity contract

Overlay 211 +1DBC rejects signed discipline bytes below zero or at
least three with AX FFFF. For a valid byte it initializes counter DI to
zero, takes byte-sized start/end boundaries and uses a local capacity
byte 14. With the nonnull output pointer, an accepted element at capacity
returns zero-extended 14 before a store; otherwise it stores and
increments DI. Exhaustion returns DI. Thus, under retained frame and
counter conditions, its result is 0..14 or the FFFF sentinel.

Its predicate is overlay 176 +0000. The complete 0000..0033 body
has no child call and does not modify SI or DI. It tests a member's
power byte, or sets AL to one for its above-three member branch; the
list builder tests AL only. No full-word normalized boolean return is
needed for the counter-preservation argument.

### Result and remaining storage conditions

Under these contracts, the dispatcher supplies zero, a count no greater
than 235, or FFFF to the final word store. Its high byte is therefore
00 or FF, never E9. The +1AEF store cannot produce selected word
1001 while those contracts hold. The clears of 9D79 were independently
excluded as immediate zero stores in FND-PARTY-276.

The outstanding conditions are concrete: the builders' nonnull output
argument and capacity/end fields, the spell predicate's saved SI, the
discipline counter and valid disjoint list/frame backing, plus the
dispatcher's DS:9C1E-to-9D79 handoff. Subsequent indexed list writes,
other aliases and lifetime remain outside this store-point result.

## Interpretation

The ordinary count-value domain is answered. Of the 27 nonconstant
writer matches, 26 retain unclassified values; this count writer instead
retains the explicit capacity/frame/alias admission conditions above.
It is not added to the 36 unconditional constant-result exclusions.

This changes no whole getter-site count: the 83 other direct sites and
repeated-hook preservation remain. FMT-PARTY-007 stays supported; no
queue item closes or complete reading is declared.

## Alternatives

- An arbitrary or modified level does not alone defeat the count bound:
  capacity is a literal chosen locally, including the level -1 case.
- A saved register is not an admitted saved-memory lifetime. A list/frame
  alias or writer to the named fields can defeat the stated contract.
- A bounded count does not prove bounded list contents, successful
  availability lookups, termination or later preservation of the aliased word.

## How to reproduce

Read overlay_listing.py on the fingerprinted executable: descriptor 211
at `9785F..97902` and `97BCC..97C50`; descriptor 177 at
`62500..62592` and `6260A..626B7`; descriptor 176 at
`61540..61573`. Bounds are hexadecimal and half-open.
Follow the nonnull-pointer tests, signed capacity comparisons, increments,
early-capacity returns and saved SI epilogue, not just the intended
spell/power ranges in FND-PARTY-280/243. Bind the store's byte overlap
at 9D7A separately from the count's own low byte at 9D79.

Reports under GAME_DIR/analysis/class-table-closure-20261106/ have
count-producer211-1A4F, count-producer177-0400,
count-producer211-1DBC, count-predicate177-050A and
count-predicate176-0000 stems. Some exploratory reports extend into
the next routine; use the exact ranges above for this contract.
No original game ran.
