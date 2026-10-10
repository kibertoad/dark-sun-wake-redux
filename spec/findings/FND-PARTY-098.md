---
id: FND-PARTY-098
title: 2D40:207E runs the second entry point of trigger record s when its word at +6 is 1 and the slot in 4C13:0369 is within the radius in the low 7 bits of +0x10 with 1B0B:00A4 returning 0, or the word is 2 and that slot is beyond the radius with 1B0B:00A4 returning nonzero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:207E..2D40:2196
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B0B:000C..1B0B:00A4
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py)
environment: null
---

## Observation

The trigger records are the 19-byte records of FMT-SCRIPT-005 in the table at `4F49:08A3`. Overlay
173 `+0C65`, which the fight routine calls (FND-PARTY-097), calls `2D40:207E` with a number `s`
at `+0DEA`.

**`2D40:207E`** takes `s` and reads the record `s` of that table and the 37-byte record `s` at
`DS:67C5`:

1. It returns when the record's word at `+6` (inside `unk_02`) is 0 (`2089..209B`).
2. With the positions at `+0` and `+2` of the 37-byte records, each shifted right by 4, of `s`
   and of the slot in the word at `4C13:0369`, it calls `1B0B:00A4` with both positions and 0,
   keeping the result `r`, and `1B0B:000C` with both positions, keeping the result `d`
   (`209E..2111`). It takes `n`, the record's byte at `+0x10` with bit 7 cleared (`2113..2127`).
3. It runs the script when the word at `+6` is 1, `r` is 0 and `d` is at most `n`, or when the
   word is 2, `d` is above `n` and `r` is not 0, comparing `d` and `n` signed; for any other word
   it does nothing (`2129..215E`).
4. To run it, it stores `s` in the word at `4C13:0367` and calls the script interpreter
   `172C:000C` with the record's word at `+0x0E` (`entry_script_1`), the word at `+0x0A`
   (`entry_offset_1`) and 1, then returns (`2160..2196`).

**`1B0B:000C`** takes two positions. With the segment `4C49` as DS, it passes the differences of
their first and of their second coordinates to `1B0B:0088`, returns early when either check
fails, and compares the larger of the two results plus a fraction of the smaller with a stored
limit (`000F..0075`).

## Interpretation

During a fight a combatant's trigger record runs its second entry point when the slot in
`4C13:0369` comes within the record's radius (word 1) or stays beyond it (word 2), each also
gated by `1B0B:00A4`, and the script runs with the triggering record's number in `4C13:0367`.
`1B0B:000C` measures an approximate distance. So a trigger script with opcode `0x24` changes the
current record during a fight when the party's slot in `4C13:0369` meets the record's distance
test.

## Alternatives

- What `1B0B:00A4` tests, and what the stored limit in `1B0B:000C` means, were not read, so the
  gate is named only by its return value.
- Whether `s` numbers the same thing in the trigger table and at `DS:67C5` everywhere: this
  routine indexes both with it, and its callers were not each read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
2D40:207E..2D40:21A0` and `1B0B:000C..1B0B:00A4`.
