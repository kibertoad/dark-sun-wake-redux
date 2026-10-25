---
id: FND-COMBAT-068
title: Overlay 173 +2C00 computes the armour class from the base, worn armour and shields and their magic, a gladiator's level over 5, and overlay 197 +27A7's Dexterity entry, caps and effect terms; from the rear the shield and Dexterity do not count, and one cap comparison reads two locals no path but one writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D520..0x0005D6F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085BA7..0x00085DE0
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

**Overlay 173 `+2C00(t, s, rear)`** (`0x5D520..0x5D6F0`). For a slot `t` in state 1 it returns 5,
in a state other than 1 or 2 it returns -99. For state 2 it starts `A` from details byte `+0x21`
and fills an item list for `t` through `2D40:2196`. For each entry of kind 1 for which `+2DD0`
does not return 4, whose type is below 9999 and whose type byte `+0x0B` has bit 0x80: it skips
the entry when type byte `+0` has bit 4 and `rear` is set; otherwise it adds `-(item byte +0x16)
- type byte +0x0E` to `A`, or only `-type byte +0x0E` when overlay 193 `+1C95(t, 4)` is nonzero
(`+2C88..+2D66`). When `t`'s details word `+0x10` has bit 0x40 it subtracts `27E5:00C1(t, 10) / 5`
(`+2D6E..+2DAC`). It adds overlay 197 `+27A7(t, s, rear, &A)` and returns the sum
(`+2DAF..+2DCF`). `direct_callers.py` lists callers at `+2087` and in overlays 183, 184, 195,
197 (twice), 207 and 212.

**Overlay 197 `+27A7(t, s, rear, &A)`** (`0x85BA7..0x85DE0`) fills the blocks of `t` and `s` and
returns a term `T` from 0, changing `A` through the pointer. The bytes named are of `t`'s
block, and bits are named by their mask:

- unless `rear`, `T` adds `27E5:02F2(t)`, the `DS:0866` entry of FND-COMBAT-066;
- for `t`'s block byte 13 bit 1, with `x` = overlay 193 `+1A2D(t, 0x68)`, the locals `bp-4` = `5 -
  (x > 5 ? (x - 5) / 3 : 0)` and `bp-2` = `T`; no other path writes `bp-4` or `bp-2`;
- `A` is held to at most 4 for byte 11 bit 0x40, else to at most 6 for byte 0 bit 0x20; to at
  most `6 - +1A2D(t, 6) / 4` for byte 0 bit 0x40; to at most 4 for byte 11 bit 2 unless `rear`;
- when `A + T` is above `bp-4 + bp-2`, `A` becomes `bp-4` and `T` becomes `bp-2` (`+287D..+2890`);
- `A` is held to at most `10 - +1A2D(t, 0x28)` for byte 5 bit 1;
- `T` -2 for byte 3 bit 0x20; -7 for byte 3 bit 0x10 when `s`'s details byte `+0x12` is 0x0E;
  +4 for byte 1 bit 4; +4 for byte 11 bit 8; -2 for byte 9 bit 8 when `s`'s details byte
  `+0x14` is 3, 6 or 9; -4 when overlay 197 `+2F52` returns nonzero; -1 for byte 1 bit 1; +4 for
  byte 2 bit 0x80; -4 for byte 12 bit 0x40; +8 for byte 6 bit 0x10.

## Interpretation

The armour class is the character's base (10 for most, 5 for most thri-kreen, FND-PARTY-145)
less each worn armour piece's value and magic bonus, a shield not counting against a rear attack,
less one point per five gladiator levels, plus the Dexterity entry (-1 at 15 down to -6 at 24
and 25, +1 to +5 below 7) except from the rear, with effect caps and terms. Effect 4 on the target
removes the magic bonus of its armour, as it does the attacker's weapon bonus (FND-COMBAT-067).
Details byte `+0x14` 3, 6 and 9 are the evil alignments (FMT-PARTY-001), so byte 9 bit 8 reads as
a protection against evil attackers.

The comparison at `+287D` reads `bp-4` and `bp-2` even when byte 13 bit 1 is clear; they then hold
whatever the stack held, and whether `A + T` is replaced depends on it.

## Alternatives

- Location 4 as a carried-not-worn place, type byte `+0x0B` bit 0x80 as armour and byte `+0`
  bit 4 as a shield rest on the tests, not on a reading of their writers.
- The uninitialised pair may in practice always hold large values left by earlier calls, so the
  replacement never happens; a run or a reading of the callers' stack use would settle it.
- The block bits and effect codes 0x06, 0x20, 0x28 and 0x68 were not named.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 173 5D520 5D73B` and `197 85BA7 85E40`;
`direct_callers.py <dsun> 173+2C00`.
