---
id: FND-PARTY-201
title: Overlay 193's per-target loop calls overlay 179 +0D2F for every target, and sets the fight state to 1 at +0533 only for a target numbered above 4 when its record's flag 0x4000 is set
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E763..0x0007E849
---

## Observation

Overlay 193's code starts at file `0x7E2F0`. In the routine overlay 193 `+003C`, the loop at
`+0473..+054B` runs a counter `[bp-0x20]` from 0 while it is below `[bp-0x18]`. For each value it
takes two words from a local list of four-byte entries at `[bp-0x16A]` (into `[bp-0x1A]`) and
`[bp-0x16C]` (into `[bp-0x1C]`) (`+0483..+04A2`), calls the near routine at `+1F88` with `di`,
the byte `[bp+0xC]` and the address of `[bp-0x16]` and keeps its result in `[bp-0x14]`
(`+04A5..+04B6`), and calls overlay 179 `+0D2F` through trampoline `56A7:006B` with `[bp-0x1A]`,
`si`, `di`, the word at `+0x1A` of the far record at `[bp-0x12]`, the address of `[bp-0x14]`,
`[bp-0x16]`, 0, 0, `[bp-0x1C]`, 0x63, `[bp+0xC]` and `[bp-0x1D]` (`+04B9..+04DE`), ORing the low
byte of its result into `[bp-0x22]`. No test comes between the start of the iteration and that
call.

Then, when bit `0x4000` of the word at `+0x11` of the record at `[bp-0x12]` is set
(`+04EB..+04F4`), and the word at `4C10:0019` is 0 or 1 (`+04F6..+050B`), and `[bp-0x1A]` is
above 4 (`+050D`), and `si` is from 0 to 3 (`+0513..+051B`), it calls `28C9:2E0E` with
`[bp-0x1A]` and `[bp-0x1D]` and stores 1 at `4C10:0019` (`+051D..+0533`). When `si` is not
negative it calls `1695:02AD` with `si` and `[bp-0x1A]` (`+053A..+0548`).

## Interpretation

The routine applies something to each entry of a list, through overlay 179 `+0D2F`, and starts a
fight (FND-PARTY-089) only when the entry's number is above 4, which a party slot's is not
(FND-PARTY-013 puts the party in combat slots 0 to 3), and the record carries flag `0x4000`. So on
the USE window's route (FND-PARTY-198) a fight starts only when there is a target outside the
party, and a party member that is a target reaches `179 +0D2F` whatever the flag.

## Alternatives

- That `[bp-0x1A]` is a target, `si` the caster and the record at `[bp-0x12]` a spell is not read;
  the reading rests on the overlay's place on the route from the USE window (FND-PARTY-197) and on
  the call of `179 +0D2F`, which leads to the damage routine.
- How the list at `[bp-0x16A]` is filled, and whether it can hold a number above 4 on Create
  Characters, is not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run `tools/research/exec-census/overlay_listing.py <dsun> 193 7E760 7E850`.
