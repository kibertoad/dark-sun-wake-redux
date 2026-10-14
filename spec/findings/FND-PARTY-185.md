---
id: FND-PARTY-185
title: Overlay 173 +3CDA marks every party combatant with combat_mark 1 to 3 as 1, raising a fallen one to 1 hit point, then makes the lowest such slot the leader when one of them was standing, and otherwise calls overlay 201 +0410
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E750
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py (Capstone 5.0.9)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, where overlay 173's code `+0000` is at `0x5A920`. Segment words in the code are
resolved through the overlay's fixup list (FMT-EXE-005) as `overlay_listing.py` does: `0x0388`
names segment `4F49`, which holds `object_slot_kinds` and `object_slot_combatants`
(FND-PARTY-171), and `0x02E0` names `4C10`. `DS` is the default data segment `57E0`.

`+3CDA` (`0x0005E5FA..0x0005E750`) takes one argument it does not read here. It stores 1 to the
byte at `DS:13FB` (`+3CE1`), then for `si` from 0 to 3 (`+3CE6`, `+3DB2..+3DB8`):

1. When `object_slot_kinds[si]` is not 2 it goes on to the next slot (`+3CE8..+3CFA`).
2. It calls overlay 193 `+1C95` with `si` and `0x45`, and, when that returns nonzero, overlay 193
   `+194F` with the same arguments (`+3CFD..+3D14`).
3. It stores 0 to the word at `4F49:08A7 + 0x13 * si` and `0xFF` to the byte at
   `4F49:0374 + 0x1C * si` (`+3D17..+3D32`).
4. It takes the slot's FMT-COMBAT-001 record, the one `object_slot_combatants[si]` selects in the
   table at the far pointer `DS:19C9`, 49 bytes each (`+3D38..+3D57`), and reads its byte `+0x14`
   (`+3D5A..+3D61`).
   - For 1, 2 or 3 (`+3D63..+3D70`): when the record's signed word `+0x00` is less than 1 it
     stores 1 there and goes to `+3D91` (`+3D72..+3D80`); otherwise, when the byte is not 3, it
     stores 0 to `DS:13FB` (`+3D82..+3D8C`). In both cases it then stores 1 to the byte `+0x14`
     (`+3D91..+3D94`).
   - Otherwise it sets bits 4 and 6 (`or 0x50`) of byte `+5` of the slot's placed entry, the
     8-byte entry at the far pointer `DS:67B7` that the word at `DS:67BC + 0x25 * si` indexes
     (`+3D9B..+3DAD`).

After the loop, when `DS:13FB` is 0 (`+3DBB..+3DC2`), it walks `si` from 0 to 3 again and, at the
first slot whose `object_slot_kinds` byte is 2 and whose combatant's byte `+0x14` is 1, calls
overlay 182 `+1D3E` (trampoline `56BD:0020`) with `si` and 0 and leaves the walk
(`+3DC4..+3E08`). It then calls `1695:02E7` with the word at `DS:426D` and `0xF461` when the
word at `4C10:0019` is not 0 (`+3E0A..+3E26`). When `DS:13FB` is not 0 it instead calls overlay
201 `+0410` (`+3E28`). It returns at `+3E2F`.

`direct_callers.py <dsun> 173+3CDA` finds near calls at `+07A1` and `+2514` in overlay 173, and
overlay 188 `+14A3` through the trampoline `5671:00E3`.

## Interpretation

The combatant's word `+0x00` is its hit points (FND-PARTY-050). `+3CDA` brings back every filled
slot marked 1 to 3 with mark 1 and at least 1 hit point, and flags the slots with another mark in
their placed entries. If one of those slots had hit points and a mark of 1 or 2, it makes the
lowest slot marked 1 to 3 the leader through `+1D3E`, whose guards (FND-PARTY-171) leave a leader
that is already that slot unchanged; that slot need not be the one that was standing. Otherwise,
with no member standing, it calls overlay 201 `+0410` and sets no leader. FND-PARTY-089 reads the
same routine as the end of a fight and agrees with this; FND-PARTY-174 shows START GAME reaching
it through `+07A1` with a created party.

## Alternatives

- What `combat_mark` 2 and 3 mean is not read here; the reading above does not depend on it.
- Overlay 193 `+1C95` and `+194F`, `1695:02E7` and overlay 201 `+0410` were not read, nor the
  contexts of the callers other than `+07A1`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and the installed
`DSUN.EXE`, run `tools/research/exec-census/overlay_listing.py <dsun> 173 5E5FA 5E750` and
`tools/research/exec-census/direct_callers.py <dsun> 173+3CDA`.
