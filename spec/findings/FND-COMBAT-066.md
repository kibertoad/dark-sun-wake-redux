---
id: FND-COMBAT-066
title: The to-hit and damage terms of an attack are a Strength or Dexterity table entry, nine effect-block terms, a -3, -1 or -2 to hit and -2 or -1 damage for an unenchanted item whose type nibble is 0 to 3, and a two-weapon term that adds the negated Dexterity entry when it is positive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008599E..0x00085BA7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000863B2..0x0008640D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DE34..0x0005DE8A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:02A7..27E5:030B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:03B1..27E5:0423
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:042B..27E5:0498
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001D473..0x0001D47B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001D4E8..0x0001D4F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D818..0x0004D880
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of the eight bytes at file 0x1D473 and at 0x1D4E8 and the 104 bytes at 0x4D818
environment: null
---

## Observation

Overlay 197's code starts at file `0x83400`. `27E5:04A0(slot, k)` returns combatant byte
`+0x19 + k`, 9 for a slot without a loaded character (FND-MAGIC-013). A slot's effect block is
the 16 bytes overlay 193 `+1BA3` fills for it. Bits are named by their mask.

**Ability tables.** `27E5:02A7` and `27E5:02C0` index the signed bytes at `DS:0818` and `DS:0832`
by `27E5:04A0(slot, 0)`, Strength; `27E5:02D9` and `27E5:02F2` index `DS:084C` and `DS:0866` by
`27E5:04A0(slot, 1)`, Dexterity. None tests the index. The 26 bytes of each table, for scores 0
to 25, are:

| Table | Values |
| --- | --- |
| `DS:0818` | 0, -4, -2, -1, -1, -1, 0 (6 to 15), 1, 1, 4, 7, 8, 9, 10, 11, 12, 14 |
| `DS:0832` | 0, -5, -3, -3, -2, -2, -1, -1, 0 (8 to 16), 1, 2, 3, 3, 4, 4, 5, 6, 7 |
| `DS:084C` | 0, -6, -4, -3, -2, -1, 0 (6 to 15), 1, 2, 2, 3, 3, 4, 4, 4, 5, 5 |
| `DS:0866` | 0, 5, 5, 4, 3, 2, 1, 0 (7 to 14), -1, -2, -3, -4, -4, -4, -5, -5, -5, -6, -6 |

**Item terms.** `27E5:03B1(item, type)` and `27E5:042B(item, type)` return 0 when the item or
type is 9999 or more, when type byte `+4` has bit 0x80, or when the item's byte `+0x16` is not
0; otherwise they jump on the low four bits of type byte `+4`, returning 0 above 3. The tables at
`27E5:0423` (file `0x1D473`) and `27E5:0498` (file `0x1D4E8`) send 0, 1, 2 and 3 to code that
returns -3, -1, -2 and -2 for `03B1`, and -2, -1, -1 and -1 for `042B`.

**Two-weapon term, overlay 173 `+3514(n, slot)`.** When bit 0x200 of the slot's details word
`+0x10` is clear and `n` is above 1, it returns `-27E5:02D9(slot)` when that is positive, else 0;
otherwise 0. The attack routine passes `+2E1B`'s count of melee weapons as `n` (FND-COMBAT-065).

**Attacker term, overlay 197 `+259E(target, attacker, list, r)`** (`0x8599E..0x85BA7`), from 0:

- -2 for each count in byte `+0x255` of the target's row of effect 0x3B, when `3143:00A8` finds
  one (`+25C7..+25F6`);
- from the attacker's block: +1 for byte 1 bit 2, -1 for byte 2 bit 0x40, -4 for byte 11 bit 8,
  -2 for byte 6 bit 4, +1 for byte 13 bit 4, -2 for byte 5 bit 0x10, and for byte 12 bit 0x40
  `L - 1 - (L - 1) / 3` with `L` = `27E5:00C1(attacker, 11)` (`+25F8..+2647`);
- -2 for byte 4 bit 4 when `r` is 1 or less and the attacker's row of effect 0x22 has word
  `+0x251` 0xBB (`+2649..+2675`);
- -2 for the target's block byte 1 bit 0x20 (`+2678..+267E`);
- for the attacker's block byte 9 bit 1, +1 or -1 as the row of effect 0x48 that `3143:0061`
  finds for the attacker comes from a slot with the same combatant byte `+0x15` or not
  (`+2681..+26FB`);
- +1 for an attacker whose details byte `+0x12` is 6, when `r` is above 0x1C, the list is not
  null, and type byte `+0` of the entry it reads has bit 0x10, or bit 2 without bit 8; the entry
  is the one `+2E1B` returns for the current index -1, which is always -1, so it is the 10 bytes
  before the list (`+26FC..+2789`);
- `27E5:02C0(attacker)` when `r` is 1 or less, otherwise `27E5:02D9(attacker)` (`+278A..+279F`).

**Melee damage term, overlay 197 `+2FB2(attacker, d)`** (`0x863B2..0x8640D`) returns `d +
27E5:02A7(attacker)`, and when the attacker's block byte 4 has bit 1, that sum times `100 + 10 *
overlay 193 +1A2D(attacker, 0x20)`, divided by 100 (signed 32-bit).

## Interpretation

`DS:0832` and `DS:0818` are Strength's to-hit and damage adjustments and `DS:084C` Dexterity's
missile adjustment: a melee attack adds the Strength to-hit entry and a ranged one the Dexterity
entry, and a melee hit adds the Strength damage entry. `DS:0866` is Dexterity's armour class
adjustment (FND-COMBAT-068). An unenchanted item whose type nibble is 0 to 3 hits at -3, -1, -2
or -2 and does 2, 1, 1 or 1 less damage; these are the nibbles RULE-COMBAT-011 lets break, which
fits weapons of wood, bone and stone in the AD&D Dark Sun rules (circumstantial).

The two-weapon term is never a penalty: for a non-ranger holding two melee weapons it is 0 at
Dexterity 6 or more and adds 1 to 6 to the bonus below that. No fixed penalty for the second
weapon appears in the attack routine. The AD&D rule the manual paraphrases is a penalty reduced by
Dexterity, so the sign may be a slip, but nothing here decides it.

## Alternatives

- What the effect codes 0x22, 0x3B and 0x48 and the block bits are was not read; FND-PARTY-167
  reads the byte `+0x255` of an effect row as a count. The terms are given by bit, not by name.
- Details byte `+0x12` 6 is the halfling origin (FMT-PARTY-001); what `r` above 0x1C means for a
  distance was not settled, and the entry before the list that the term reads was not traced.
- The tables were read from the file; a write to them in play would change the values. No search
  for writers was made.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 197 8599E 85BA7`, `197 863B2 8640D`, `197 85B42 85B47`,
`197 863DF 863E4` and `173 5DE34 5DE8A`; `resident_listing.py <dsun> 27E5:02A7..27E5:030B
27E5:03B1..27E5:04A0`. Read the eight bytes at file `0x1D473` and at `0x1D4E8` as four words each,
and 26 signed bytes at each of `0x4D818`, `0x4D832`, `0x4D84C` and `0x4D866`.
