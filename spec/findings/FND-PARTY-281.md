---
id: FND-PARTY-281
title: Six archive alias writers reduce to record-pointer low bytes and explicit counter predecessor ranges
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37ED:0064..37ED:00B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:077C..37FC:08FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5..38FF:0429
tool: tools/research/exec-census/resident_listing.py at a3e7de22; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-CONFIG-067/068 and FND-COMBAT-096/126/127
environment: null
---

## Observation

Q-PARTY-124 retains 26 other unclassified writer values after
FND-PARTY-279's count-domain reading. Six overlap selected word
DS:9D9E: four double-word stores at 9D9F and two word updates at
9D9D. FND-PARTY-275/276 identify these instruction sites and exclude
the separate immediate-zero stores. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.
All offsets and ranges below are hexadecimal unless stated otherwise.

### The selected word combines two distinct fields

Let C be the word at DS:9D9D and P the double word at DS:9D9F.
Selected word W at 9D9E has low byte equal to C's high byte and
high byte equal to P's low byte. Thus W is decimal 1001 (03E9)
exactly when C's high byte is E9 and P's low byte is 03.
This is a byte-overlap relation, not an archive handle conversion.
FND-CONFIG-067 distinguishes the numeric handle from the record pointer.

### Four pointer-store producers

| Store | Reused producer and observed binding |
|---|---|
| 37ED:00B0 | After signature admission, 39A9:0001 resolves the supplied numeric handle. Its DX:AX result is saved at BP-4 and must be nonnull before the double-word publication. |
| 37FC:08AE | The replacement allocated by 444C:0008 is saved at BP-4. On the successful transfer continuation, the old record must equal current active P; the saved replacement is then published. |
| 38FF:027F | The constructed record saved at BP-0C is published after the opener's transfer checks. FND-COMBAT-127 retains allocation versus supplied-storage branches and their admission conditions. |
| 38FF:03F4 | When the closing target is active, select its nonnull +0C link, otherwise current head DS:9DA3. FND-COMBAT-096 retains earlier graph updates and the separate count-one clear. |

FND-CONFIG-067/068 and FND-COMBAT-126/127 already supply these
producer meanings. The bounded source rereading confirms the saved
locations, equality gates and final loads. None of the four stores
masks, normalizes or independently rejects pointer low byte 03.
A nonnull pointer check alone does not reject that byte.

At each such store, W becomes 03E9 exactly when the published
pointer has low byte 03 and the contemporaneous C high byte is E9,
under the stated DS identity. Pointer allocation/layout, supplied
storage, resolver/link provenance, intervening aliases and count
admission remain obligations. A numeric archive-handle bound does not
by itself settle the record pointer's low byte.

### Exact word-update predecessor conditions

The successful opener increments C at 38FF:02AB. The admitted closer
decrements C at 38FF:03F8. Neither instruction has a local saturation
or overflow check. For the instruction-point values, word arithmetic gives:

| Update | C immediately before the instruction that yields high byte E9 |
|---|---|
| Increment modulo 10000 | E8FF..E9FE inclusive |
| Decrement modulo 10000 | E901..EA00 inclusive |

For either update, W becomes 03E9 exactly when C is in its listed
predecessor range and the current P low byte is 03. Values outside
the respective range exclude that result at that update. These are
necessary and sufficient local conditions, not claims those states
occur during the game.

The open path publishes P before the count increment, with intervening
record and optional caller-output stores. The close path can change
links/head/P before its decrement. Therefore the pointer byte for each
condition is the byte at the actual update, not an earlier function-entry
snapshot. Distinct record/output/global storage and preservation must be
admitted before combining the sequential effects. The close's later
release request does not prove preservation after the instruction point.

## Interpretation

These six instruction-value expressions are answered by existing producer
findings plus the exact overlap arithmetic. Twenty other writer values
remain unclassified. The six archive sites remain conditional obligations:
record-pointer low-byte admission and preservation, and current counter
high byte or the two explicit predecessor ranges. They are not added to
the 36 unconditional constant-store exclusions. FND-PARTY-279's separate
selection-count storage conditions remain.

No whole getter site closes: 83 other direct sites and repeated-hook
preservation remain. FMT-PARTY-007 stays supported. There is no queue
closure or complete-reading declaration.

## Alternatives

- The small number of usual open archives is not a proved count bound.
  The word updates alone cannot exclude E9 high bytes or wrapped state.
- A far pointer's low byte is an offset byte, not its segment or numeric
  archive handle. General nonnull or paragraph-allocation intuition is
  insufficient without the actual producer and supplied-storage contracts.
- The exact instruction-point relations do not exclude other aliases,
  indexed/block writes, different segment identity or later changes.

## How to reproduce

Run resident_listing.py on the fingerprinted DSUN.EXE with the four
half-open ranges in locations, from their established function starts.
The retained report is
GAME_DIR/analysis/class-table-closure-20261106/archive-alias-producers.txt.
Trace the listed stores back to their saved pointer or link sources.
Reuse FND-CONFIG-067/068 and FND-COMBAT-096/126/127 for their existing
producer contracts and unresolved admission; do not reclassify them as
unconditionally safe helpers.

Intersect the destination intervals with 9D9E..9DA0 independently of
their field names. Solve ((C + 1) modulo 10000) >> 8 = E9 and
((C - 1) modulo 10000) >> 8 = E9 for the two predecessor ranges;
bind P's low byte separately. The census includes raw candidates too:
retain FND-PARTY-277/278's instruction-interior classifications.
No original game ran.
