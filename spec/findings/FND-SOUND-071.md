---
id: FND-SOUND-071
title: Music selection uses the recorded DJ table fields and the literal region-writer census includes two overlay-182 transition stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:056F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:234E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:389A..28C9:391D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0001..2834:043B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0508..2834:0673
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:08BA..57E0:08C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069FD3
tool: writer-census correction at 887dba04 with window_operands.py, resident_listing.py and overlay_listing.py, Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; historical Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; searches of the load image and the overlays for stores to the words at 57E0:08BC and 57E0:140C and the byte at 57E0:14E7
environment: null
---

## Observation

This replacement retains FND-SOUND-012's selector and field observations
and corrects its claim that only three sites write the current region.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The current census uses the inventory
with LF SHA-256
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
Historical selector observations below remain subject to actual input,
buffer/frame and segment conditions in Q-SOUND-004/011; this correction
is not a complete-reading declaration or an exhaustive alias census.


`DS` is the data segment `57E0`. The table is the one `2834:043B` loads from `DJ.DAT`
(FND-SOUND-011): the count byte at `DS:4252`, the word at `DS:4253`, and records of 6 bytes at the
far pointer `DS:424E`. In a record, byte 0 is compared as a signed byte, so 255 is -1.

The main loop far-calls `28C9:2322` at `277B:056F`. That routine near-calls `28C9:389A` and then,
when the byte at `DS:13F7` is not 0, far-calls `2834:000C` with the word at `DS:140C` and the byte
at `DS:14E7`.

`28C9:389A` goes through records 0 to 3 of the 49-byte records at the far pointer `DS:19C9`
(`combatants`, FMT-COMBAT-001) and the 66-byte records at `DS:19C5` (`combatant_details`,
FMT-COMBAT-002). For each whose `character_id` is not 0 it adds `hit_points`, signed, to one 32-bit
sum and `max_hit_points`, unsigned, to another. It sets `DS:14E7` to 10 minus
`9 * hit_sum / max_sum`, the quotient unsigned and truncated, or to 10 when the second sum is 0.

`2834:0001` stores its word argument in the word at `DS:08BC`; the only calls to it are the two in
overlay 187 of FND-SOUND-008, with 0 and with 2 or 3. `2834:000C` itself stores 2 there. In the
file the words at `DS:08BA`, `DS:08BC`, `DS:08BE` and `DS:08C0` hold 0, 1, 1 and 0.

`2834:000C(region, health)`, where `region` is the word and `health` the byte widened to a word:

1. When `DS:08BC` is 2 it returns 0; when it is 0 it returns 1.
2. A `region` of `0x38` to `0x3A` becomes `0x38`. It adds 1 to the word at `DS:43F8`.
3. When `DS:08BC` is 3 and `DS:08BE` is 1 or 2, it calls `4A32:0185` and sets `DS:08BA` to 0.
4. When `DS:08BA` is not 0: if `DS:43F8` is above the word at `DS:4253`, unsigned, it sets
   `DS:08BA` to 0 and goes on; otherwise it returns 0.
5. When the byte at `DS:14E3` is not 0 it returns 0 if `4ABF:01E5` returns a non-zero byte. (It
   first calls `2834:063F`, which returns 0 when the byte at `4E71:0C3F` is 0 and otherwise the
   byte `4ABF:01E5` returns.)
6. When `DS:08BC` is 1 it goes through the records in order. A record whose word at offset 3 is 1
   and whose byte 0 equals `region` is played at once: it calls `2834:0665`, which calls
   `4A32:0011` with the record's byte 5, sets `DS:08BE` to 1 and `DS:08BC` to 2, and returns 1. A
   record whose word at offset 3 is 1 and whose byte 0 is -1 has its index added to a list. After
   the last record it takes the list entry `2834:061C` gives for the list's length minus 1, plays
   that record the same way, and returns 1.
7. When `DS:08BC` is 3 it sorts the records whose word at offset 3 is 3 into three lists by byte 2
   (1, 2 or 3). `health` below 4 takes the first list, 4 to 7 the second, and above 8 the third;
   the last index is that list's length minus 1. With `health` 8 the first list is taken with a
   last index of 0. It looks through the taken list, up to the last index, for the first record
   whose byte 0 equals `region`, and takes an index `2834:061C` gives for the last index.
   - With a record found, it passes the record's byte 1 to `2834:0519`; when that succeeds it
     passes the same byte again, and plays the record when the second call succeeds and calls
     `2834:0508` when it fails. When the first call fails it tries the list's record at the
     random index instead.
   - With no record found, or on that fallback, it passes the random record's byte 1 to
     `2834:0519`, plays the record when that succeeds and calls `2834:0508` when it fails.
   Then it copies `DS:08BC` to `DS:08BE` and returns 1.
8. For any other value of `DS:08BC` it sets `DS:08C0` to the word at `4E71:0C1D` when that is
   larger, and returns 0.

`2834:0508` sets `DS:08BA` to 1 and `DS:43F8` to 0. `2834:061C` is `random_mod` and `2834:0519`
is `chance_in_ten` (RULE-RNG-001, FND-RNG-003, FND-RNG-006); FND-RNG-007 found this routine
without identifying its table.

### Corrected literal mode and region writer component

The exact 08BC..08BD query reports fifteen listed operands: the
setter's one write, two selector writes of mode two and twelve reads.
The setter inventory root 27E5:04F1 is the same linear address as
2834:0001; its store 27E5:04F7 is 2834:0007. It forwards its
incoming word unchanged to DS:08BC and returns far. Its two known
encoded callers remain the overlay-187 speech routines. The one raw
exact-mode candidate is an interior decoding, not another established
writer; other computed/overlapping/indirect writes remain outside this
literal component.

The exact 140C..140D query reports twenty-seven listed operands,
including five direct word stores to DS:140C: 277B:0182,
277B:03C1, overlay 182 +15FC and +16B5, and overlay 187 +1216.
The first, second and last are the sites previously recorded. The two
newly accounted sites disprove the former three-site exclusion.
The query retains three raw candidates, one outside listed starts and
two inside prefixed instructions, without promoting them to accesses.

Both new stores belong to overlay 182 +15D7. The entry saves SI
and loads its first word argument into SI, then a byte argument at
BP+0A into DL. With DS:0DAB zero, it optionally calls 3D72:0B84
when DL is zero, stores held SI to DS:140C at +15FC, sets AL one
and joins its epilogue. With 0DAB nonzero and DL nonzero it takes
a separate transition/cleanup path. With DL zero and current 0DAB
four, it passes the status/stop calls and 362C:06B9, then stores
held SI to DS:140C at +16B5 before the subsequent transition calls.
The locally intervening child outcomes, DS/SI/frame retention and
actual argument domains remain conditions; the writes do not by
themselves prove a particular region is entered natively.

Positive controls are the known mode setter and selector's mode-two
stores, plus known region stores 277B:03C1 and 187 +1216 from
the prior findings. All query locations remain bounded to decoded
inventory ranges; indexed, copied, indirect, modified and segment-
aliased writes or accesses beginning outside the windows remain open.
The earlier wider queries 08B9..08BE and 1409..140F retain their
separate neighboring-state scopes and are not substituted for these
exact-offset counts.

The historical health-byte search identifies the producer in
28C9:389A and overlay 204 +03DD. This batch does not rerun or
upgrade that separate writer component to a universal exclusion.

## Interpretation

This is the music selector the startup message calls "Mel DJ". `DS:140C` is the current region
number and `DS:14E7` a measure of how hurt the party is, 1 at full health and 10 with no hit
points left. `DS:08BC` is the music mode:

- 0: no music is chosen (set while a line is spoken);
- 1, at startup: a track for the current region is chosen from the mode-1 records, otherwise one
  of the records for any region, and the mode becomes 2;
- 2: a track has been chosen and nothing more is chosen;
- 3, set after speech in combat: a combat track is chosen from the mode-3 records by the party's
  health, favouring one tied to the region, each time the last one has ended; a failed chance
  waits until the selector has run more than 1,000 times, the word `DJ.DAT` gives, before trying
  again.

The records whose word at offset 3 is 2, 18 of the 38, are never chosen by this routine.

## Alternatives

That `4ABF:01E5` reports whether music is playing and `4A32:0185` stops it are read from their
use here. The random pick in mode 1 takes an index below the list's length minus 1, so the last
record of the list is never picked; with one record the index is 0. Whether other code writes
`DS:08BC` through a pointer, which would explain the mode-2 records, and actual region/health source domains and the effect of other computed or
aliased stores remain unresolved. The two overlay-182 region sites above
are now explicit, without claiming their complete caller histories.

## How to reproduce

Disassemble `28C9:2322`, `28C9:389A` and `2834:0001` to `2834:0673`, read the bytes at file offset
`0x4D8BA`, and search the load image and the overlays for `C7 06 BC 08`, `A3 BC 08`, `A3 0C 14`,
`C7 06 0C 14` and `A2 E7 14`.

For this correction, run window_operands.py for exact windows 08BC..08BD
and 140C..140D and retain music-exact-mode-region-window.txt under
GAME_DIR/analysis/handler-census-20261106/. Preserve both listed and
raw results. Read overlay 182 file 69E27..69FD3 with overlay_listing.py
as music-region-overlay182-producer.txt and setter 27E5:04F1..04FC
with resident_listing.py as music-mode-setter-contract.txt. The wider
music-mode-region-writer-window.txt is retained with its distinct scope.
No original game, emulator session or emulated call supplied new evidence.
