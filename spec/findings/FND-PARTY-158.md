---
id: FND-PARTY-158
title: Details byte 0x40 selects one of eight psionic power lists from which overlays 174 and 198 pick a non-party combatant's power at random
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F404..0x0005F4A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000875C2..0x0008772D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5284:0003..5284:0023
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py; scratch scripts that read the far pointers at 5284:0003 and the lists they point at from the file, and print the name at +0x20 of DATA resources read through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A `CHAR` load copies bytes `0x85` and `0x86` to bytes `+0x40` and `+0x41` of a details
record (FND-PARTY-051), whose table is at the far pointer `DS:19C5` with records of 0x42 bytes.
`field_reads.py <dsun> <inventory> --es --table 19c5 40 41` finds two reads of `+0x40`, at overlay
174 `+064A` and overlay 198 `+02F0`, and none of `+0x41` (FND-PARTY-149); `field_stores.py <dsun>
<inventory> --es 40 41` finds no store after a load of `DS:19C5`.

**Overlay 198 `+02B2`** (trampoline `+0025`) takes a slot `s` and a second slot, and returns 0 for
`s` below 4. It reads byte `+0x40` of `s`'s details record as `k` and leaves when `k` is not 1 to 8.
It counts the bytes of the list at the far pointer `5284:0003 + 4 * (k - 1)` up to a byte 0xFF, picks
the byte at index `rand * count / 32768`, from `1000:0822`, as `p`, and goes on only when overlay
208 `+0057` of `s` and `p + 235` is at least the distance `1B0B:000C` gives between the two slots'
positions, after `1B0B:00A4` of those positions returns 0. It returns 1 when overlay 176 `+0057` of
`p` is not above the word at `+0x02` of `s`'s combatant record, the current psionic points
(FND-PARTY-059), and 0 otherwise (`+02BA..+041C`). **Overlay 174 `+0604`** reads the byte the same way, through the
combatant record's details index, returns 0 for a slot below 4, and picks `p` from the same list
the same way (`+060C..+0692`).

**The lists.** The eight far pointers at `5284:0003` point at lists of power numbers ending with
0xFF:

| `k` | Powers |
|---|---|
| 1 | none |
| 2 | 20, 25, 26 |
| 3 | 26, 4, 20, 30, 25 |
| 4 | 21, 26, 28, 22, 8, 4, 15, 1 |
| 5 | 7, 9, 24, 31, 27, 28, 30, 33 |
| 6 | 1, 0, 4, 31, 25, 26 |
| 7 | 3, 22, 2, 8, 28, 26 |
| 8 | 4, 20 |

`DATA` 235 + `p` of `RESOURCE.GFF` names each, read at `+0x20`: 0 `DETONATE`, 1 `DISINTEGRATE`,
2 `PROJECT FORCE`, 3 `BALLISTIC ATTACK`, 4 `CONTROL BODY`, 7 `ENERGY CONTROL`, 8 `LIFE DRAIN`,
9 `ABSORB DISEASE`, 15 `ENHANCED STRENGTH`, 20 `DOMINATION`, 21 `MASS DOMINATION`, 22 `PSYCHIC
CRUSH`, 24 `TOWER OF IRON WILL`, 25 `EGO WHIP`, 26 `ID INSINUATION`, 27 `INTELLECT FORTRESS`, 28
`MENTAL BARRIER`, 30 `MIND BLANK`, 31 `PSIONIC BLAST` and 33 `THOUGHT SHIELD`.

## Interpretation

CHAR byte `0x85` is the psionic power list of a monster or other non-party combatant, as `0x7D` is
its spell list (FND-PARTY-151): 0 or 1 gives none, 2 to 8 a fixed set of powers. Overlay 198 `+02B2` tests whether a power picked at random from the
set reaches the other slot and costs no more than the combatant's points, and overlay 174 `+0604`
picks from the set in the same way. The byte is 0 in the shipped `CHAR` records, and the code skips slots 0 to 3, so it does
nothing for the party. Nothing found reads byte `0x86`.

## Alternatives

- Where overlay 198 `+02B2`'s result is used, and what overlay 174 `+0604` does with its pick after
  `+0692`, was not read.
- A read of `+0x40` or `+0x41` through a copy of the record or another pointer is outside the
  search (toolkit issue 468).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 40 41`,
`field_stores.py <dsun> <inventory> --es 40 41`, `overlay_listing.py <dsun> 198 875C2 8772D` and
`overlay_listing.py <dsun> 174 5F404 5F4A0`. Read the eight far pointers at `5284:0003` and the
bytes each points at up to 0xFF, taking a stored segment `s` and offset `o` at file offset
`header + 16 * s + o`, and print the 32 bytes at `+0x20` of `DATA` 235 to 268 of `RESOURCE.GFF`.
