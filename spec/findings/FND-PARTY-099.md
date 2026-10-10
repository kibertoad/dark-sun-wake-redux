---
id: FND-PARTY-099
title: Overlay 179 +2A46 rolls a saving throw, reading the target's details byte at 0x30 plus the save number in bits 5 to 7 of byte +0x1F of the effect's DATA record, and succeeds on a natural 20 or when the roll plus modifiers reaches that byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066D26..0x00066EB7
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Records and slots are as FND-PARTY-081 gives; `2C5F:0003` returns the `DATA` resource
numbered by its argument (FND-PARTY-096). `direct_callers.py` finds **overlay 179 `+2A46`**
(trampoline `56A7:0075`) called from overlay 173 `+1C5A`, overlay 195 `+03A2`, overlay 197 `+19E5`
and, near, overlay 179 `+0EDF`. It takes a slot `t`, a second slot and a number `n`, and returns a
byte, 1 or 0:

1. It returns 0 when the byte at `DS:45AA` is not 0 and the byte at `DS:43CE` is 0, or when bit 0
   of the byte at `+0x1F` of `DATA` `n` is clear (`+2A54..+2A7F`).
2. It calls `44B6:0011`: when its result has bit 0 or 1 set and the byte at `DS:143C` is not 0 it
   returns 0, and when the result has bit 2 set and `DS:143C` is not 0 it returns 1
   (`+2A82..+2AA9`).
3. It keeps the word at `+0x1A` of `DATA` `n` and fills two local records for `t` and the second
   slot through overlay 193 `+0025` (`+2AAC..+2AD7`). It takes `k`, bits 5 to 7 of the byte at
   `+0x1F` of `DATA` `n`, and returns 0 when `k` is 0 (`+2ADA..+2AF8`).
4. It returns 0 when bit 5 of a byte of `t`'s local record is set, or when `t`'s combatant record
   has 3 in its byte at `+0x14` (`+2AFB..+2B30`).
5. It reads `s`, the byte at `0x30 + k` of the details record numbered by `t`'s combatant record's
   word at `+4`, through `DS:19C5`, forming the address as the record's start plus `k` with the
   displacement `0x30` (`+2B33..+2B50`).
6. It draws `r` with `28C9:391D` (`roll_sum`) and the arguments 1 and 20 (`+2B53..+2B61`). It
   returns 0 when `r` is 1, and 1 when `r` is 20 (`+2B64..+2B70`). It returns 0 when bit 7 of a
   byte of the second slot's local record is set (`+2B73..+2B77`). It doubles `r` when the word
   at `+0x1A` of `DATA` `n` has any of the bits `0x86` set, adds the result of overlay 197
   `575A:00A7` with both slots, the local records, `k`, `n` and its fourth argument, and adds bits
   1 to 4 of the byte at `+0x1F` of `DATA` `n` read as a signed 4-bit number, all in byte width
   (`+2B79..+2BC3`).
7. It returns 1 when `r` is at least `s`, comparing unsigned bytes, and 0 otherwise
   (`+2BC6..+2BD6`).

## Interpretation

This is the saving throw. An effect whose `DATA` record has bit 0 of its byte `+0x1F` set allows
one, and bits 5 to 7 name which: 1 to 5 read the five bytes at `0x31` to `0x35` that overlay 210
sets (FND-PARTY-085), so they act in play as the target's saving throws against such effects. A
natural 1 fails and a natural 20 saves; otherwise the target saves when the d20 roll, doubled for
some effects, plus overlay 197's modifier and the effect's own bonus, reaches its save. The
numbers 6 and 7 would read the details bytes at `0x36` and `0x37`, which a load fills from the
FMT-PARTY-001 bytes at `0x7B` and `0x7C`.

## Alternatives

- What `44B6:0011`, `DS:143C`, `DS:45AA` and `DS:43CE` select, what overlay 197 `575A:00A7`
  adds, and what the local records' bits mean were not read.
- Whether any shipped `DATA` record names the save number 6 or 7: the records were not read.
- The callers were not each read, so which spells, traps and attacks call this routine is not
  known.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 66D26 66EB7` and
`direct_callers.py <dsun> 179+2A46`.
