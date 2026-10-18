---
id: FND-PARTY-244
title: The fight runs a slot's trigger only for a combatant with combat_mark 1 whose byte 0x15 is not 1, and object 161, whose trigger reaches opcode 0x24, is placed only in region 50
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B585..0x0005B9A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B278..0x0005B2E1
tool: tools/research/exec-census/overlay_listing.py and etab_objects.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). Overlay 173 `+0C65` is
the routine of the fight that calls the trigger run `2D40:207E` (FND-PARTY-097, FND-PARTY-098).

**Overlay 173 `+0C65`** calls `+0958` once and keeps its result, then takes each slot `s` from 0
to 47 (`+0CB1`, `+106A..+106E`). It calls `2D40:207E` with `s` at `+0DEA` only when all of these
hold:

1. the slot's kind byte at `4F49:0C33 + 3 * s` is 2 (`+0CB6..+0CC8`);
2. the combatant record (FMT-COMBAT-001) numbered by the word at `4F49:0C34 + 3 * s` has
   `combat_mark` 1 and its byte at `+0x15` is not 1 (`+0CD5..+0D01`);
3. `+0958`'s result is not 0 (`+0D04..+0D0A`);
4. for a slot of 5 or more, overlay 173 `+0000`, given the slot's position from its 37-byte record
   at `DS:67C5` shifted right by 4 and a mask built from the record's byte `+0x15`, returns a
   value other than 0 (`+0D0D..+0DE6`); slots 0 to 4 skip this test.

`+0958` walks the slots with the same kind byte 2 and `combat_mark` 1, keeping a mask of the
`+0x15` bytes it has seen (`+0960..+09C1`).

**Object 161.** `etab_objects.py` over the 26 installed archives (20 `ETAB` resources, 13,559
records) finds object 161 placed once, in `RGN032.GFF` `ETAB` 50, record 170, at x 168, y 239,
flags 6, and object 142, which the branch places through `0x25`, once, in `RGN0FF.GFF` `ETAB`
255. In the decoded scripts, `GPL/144` registers offset 281 for object -161 twice: as the
trigger at `0x0023` (`0x1B`, radius 5) and through `0x6E` at `0x002D`.

## Interpretation

A trigger runs during a fight only for an object that is in the fight as a combatant: its slot
has kind 2, its record is marked for the fight, and its byte `+0x15` is not 1. So `GPL/144`'s trigger for object
161 can reach its dialog during a fight only in region 50, the region a new game starts in
(FND-UI-050), and only in a fight in which object 161 itself takes part. Whether such a fight
happens, and what the kind-9 variable 32, `varD[374]`, `varD[331]`, `varE[7]` and the menu row's
condition hold then, depend on the course of play, which the code does not decide.

## Alternatives

- The byte `+0x15` may name a combatant's side, with 1 for the party, but its stores were not
  read; the slot kind byte 2 and the meaning of `+0958`'s mask and `+0000`'s test were not read
  further.
- Region 50's other resources, or an object made while the game runs, could place object 161
  elsewhere; the search covers the shipped `ETAB` records only.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 173 5B585 5B720`, `173 5B8D9 5B9B0` and `173 5B278 5B2F0`;
`etab_objects.py <install> 161 -161 142 -142`. The script lines come from the decode of
FND-SCRIPT-024 (`script_decode.py GPLDATA.GFF OBJEX.GFF --all`), `GPL/144` offsets `0x0000` to
`0x0042`. The outputs are retained under `GAME_DIR/analysis/party-trigger-fight-20261018/`. No
original function or game process was run.
