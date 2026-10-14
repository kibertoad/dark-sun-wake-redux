---
id: FND-PARTY-176
title: The keys 1 to 4 set turn_combatant to their slot when that slot's combatant record has a character and the current one's object slot has bit 5 of unk_00 clear, and outside combat then pass the slot to overlay 182 +1D3E
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0E0C..28C9:0EC5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:156C..28C9:1584
tool: Capstone 5.0.9 16-bit disassembly of the load image based at segment 1000
environment: null
---

## Observation

Addresses are those of the repository's load image, based at segment `1000`; `28C9:x` is file
offset `0x1EC90 + x - 0x0E00`. FND-AI-004 gives the key routine `28C9:0CFF`, its tables, and the
handlers of the keys 1 to 4 at `28C9:0E0C`, `0E33`, `0E5A` and `0E81`, each of which ends by
setting `turn_combatant` (`57E0:426D`) and going on at `28C9:0EA7`.

Handler `k` (0 to 3, for the keys 1 to 4):

1. loads the far pointer at `DS:19C9`, the table of 49-byte FMT-COMBAT-001 records, and jumps to
   `28C9:11FC` when the word at offset `0x31 * k + 6` is 0 (`cmp word es:[bx + 6]`, `+ 0x37`,
   `+ 0x68`, `+ 0x99`): the `character_id` of record `k`, the record of that index and not the one
   `object_slot_combatants` names for slot `k`;
2. pushes `turn_combatant` and calls `28C9:156C` (`push cs; call`), and jumps to `28C9:11FC` when
   it returns a byte other than 0;
3. stores `k` to `turn_combatant` and jumps to `28C9:0EA7`.

`28C9:156C` takes a slot and returns 1 when bit 5 of byte 0 of that slot's 37-byte `object_slots`
entry (`DS:67BB + 0x25 * slot`, field `unk_00` of FMT-ACTOR-004) is set, and 0 otherwise.

`28C9:0EA7` jumps to `28C9:11FC` when `combat_state` (`4C10:0019`) is not 0. Otherwise it pushes 1
and `turn_combatant` and calls `46BD:0020` (`56BD:0020` in the segment table's numbering), the
trampoline of overlay 182 `+1D3E` (FND-PARTY-171), then jumps to `28C9:0DFE`.

## Interpretation

Outside combat a key from 1 to 4 asks overlay 182 `+1D3E` to make its slot the leader, which that
routine does unless the slot is already the leader or its combatant's `combat_mark` is above 2
(FND-PARTY-171). The key does nothing when record `k` has no character, or when the current
`turn_combatant`'s object slot has bit 5 of `unk_00` set (which FMT-ACTOR-004 says makes the
movement routine do nothing). In combat it changes `turn_combatant` alone.

## Alternatives

- Record `k` is taken by the key's number. Where `object_slot_combatants[k]` is not `k`, a key
  would test another character's record; in the party loader's layout (FND-PARTY-013) the two are
  equal, which this reading does not show for every party.
- What `+1D3E` does with its second argument, 1 here, beyond FND-PARTY-171's guards, and what
  `28C9:0DFE` and `28C9:11FC` do next, were not read.

## How to reproduce

From the commit that adds this finding, disassemble the installed `DSUN.EXE` from file offset
`0x1EC9C` (`28C9:0E0C`) to `0x1ED55` (`28C9:0EC5`) and from `0x1F3DC` (`28C9:156C`) to `0x1F3F4`
as 16-bit code, with origin `0E0C` and `156C`.
