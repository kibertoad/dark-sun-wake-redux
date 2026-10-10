---
id: FND-PARTY-100
title: The details bytes at 0x25, 0x28, 0x2B and 0x2E plus an attack number give a natural attack's rate, damage dice, dice sides and damage bonus, the byte at 0x24 is read for an armed attack's rate, and no read at a fixed displacement through DS:19C5 or DS:1429 takes the five bytes at 0x31
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000869A4..0x00086A40
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CEB6..0x0005CF92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D37D..0x0005D3BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D09F..0x0005D0BC
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_offsets.py, overlay_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The details records are the 66-byte records through the far pointer `DS:19C5`, and a
load copies the FMT-PARTY-001 bytes `0x69` to `0x7A` to their bytes `0x24` to `0x35`
(FND-PARTY-051). Overlay 210 sets the bytes at `0x24`, `0x25`, `0x27` and `0x31..0x35`
(FND-PARTY-085).

**The searches.** `field_reads.py <dsun> <inventory> --es --table 19c5` lists these reads for the
displacements `0x24`, `0x25`, `0x27` and `0x31` to `0x35`: `0x24` at overlay 173 `+058A` and
`+25C4`, overlay 183 `+1A75` and overlay 190 `+2E5E`; `0x25` at overlay 173 `+2272`, overlay 179
`+0FF5`, overlay 190 `+2BAB` and overlay 197 `+191A` and `+35EF`; `0x31` at overlay 210 `+0698`,
the setter's own comparison (FND-PARTY-085); none for `0x27` or `0x32` to `0x35`. The same search
with `--table 1429`, the other far pointer to a details record (FND-PARTY-055), lists none. Without
`--table`, the reads through ES of `0x31` to `0x35` are 54, all but overlay 210 `+0698` in resident
code that compares or pushes words and dwords, or decodings that start inside those instructions.
A search of the inventoried code for `add`, `mov`, `lea`, `sub` or `or` with an immediate or a
`lea` displacement of `0x31` to `0x35` within six instructions after a text naming `19c5` or
`1429` lists none; the same search for the immediate 1 lists 18, as a control.

**Overlay 197 `+35A4`** (trampoline `575A:0034`) takes a slot, an attack number `k` and four far
pointers. When the slot's state byte is 2, it reads the details record of the slot's combatant
and stores, through the four pointers, its bytes at `0x25 + k`, `0x28 + k` and `0x2B + k`
unsigned and `0x2E + k` signed (`+35AB..+3640`). `direct_callers.py` finds it called from overlay
173 `+266A`, overlay 183 `+1A58` and overlay 190 `+2BCB`.

**Overlay 173 `+2560`**, the swing of FND-PARTY-091, takes an item number `n` and an attack
number. When `n` is 0 or more it takes four values from item records: as the first, the byte at
`+7` of the record through `DS:19CD` when a later argument is above 1, and otherwise the
attacker's details byte at `0x24`; and that record's bytes at `+9` and `+8` and its signed byte at
`+0xA` plus further terms (`+2596..+2652`). When `n` is below 0 it takes the four values through
overlay 197 `+35A4` with its attack number (`+2652..+266F`), and doubles the first when overlay 197
`575A:005C` returns 4 (`+2672..+2689`). On a hit it calls `+2A5D` with the second, third and
fourth values (`+278B..+279C`). **`+2A5D`** draws `1000:0822` as many times as its first argument,
adds each draw times its second argument divided by 32,768 (signed) plus 1, and returns the sum
plus its third argument (`+2A5D..+2A9A`).

## Interpretation

For each of three natural attacks `k`, the details bytes at `0x25 + k`, `0x28 + k`, `0x2B + k` and
`0x2E + k` stand where an item's bytes at `+7`, `+9`, `+8` and `+0xA` stand for a weapon: a rate,
the number of damage dice, the dice's sides, and a signed damage bonus, since `+2A5D` rolls that
many dice of that many sides and adds the bonus. In FMT-PARTY-001 these are the bytes at `0x6A`,
`0x6D`, `0x70` and `0x73` with `k`. The byte at `0x24` is the rate of an armed attack unless the
weapon's own byte at `+7` applies. So overlay 210's 8 for a thri-kreen at `0x25` and 2 at `0x27`
are the rates of its first and third natural attacks.

No read at a fixed displacement through `DS:19C5` or `DS:1429` takes the five bytes at `0x31`;
the saving throw roll reads them at a computed displacement (FND-PARTY-099).

## Alternatives

- Replaces FND-PARTY-092, which recorded these observations but concluded that no code but the
  setter reads the five bytes at `0x31`. The searches cover only fixed displacements; overlay 179
  `+2A46` reads them as the byte at `0x30` plus a save number added to the record's address
  (FND-PARTY-099).
- What the rate counts, and how the doubling and the later argument choose it, was not read; that
  its values 2 to 5 and the thri-kreen's 8 count half attacks per round rests on the AD&D rules
  the game follows, circumstantial.
- The reads of `0x24` and `0x25` at the other sites listed above were not followed.

## How to reproduce

From the commit that added FND-PARTY-092, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `field_reads.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es --table 19c5 24 25 27 31 32 33 34 35`, the
same with `--table 1429`, and the same without `--table` for `31 32 33 34 35`;
`overlay_listing.py <dsun> 197 869A4 86A40`, `173 5CE80 5CFB0`, `173 5D052 5D0E9` and `173 5D37D
5D3BA`; `direct_callers.py <dsun> 197+35A4`; and `field_offsets.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --table 19c5 --table 1429 31 32 33 34 35`, with
`--table 19c5 1` as the control.
