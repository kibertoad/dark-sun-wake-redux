---
id: FND-COMBAT-027
title: The executable holds 113 records of 31 bytes at 4C87:0000, an effect name and a word each, which the status panel's third line indexes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4C87:0000..4C87:0DAF
tool: Python 3.14.7 byte inspection; Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The status panel routine copies its third line from `4C87:0000 + w * 31` (FND-COMBAT-022). In
`DSUN.EXE` that address is file offset `0x41A70`. From there the file holds 113 records of 31
bytes: a NUL-terminated ASCII name padded with NULs to 29 bytes, then a little-endian word. The
3,162 bytes after them, to file offset `0x43479`, are all 0.

The table gives each record's index, name and word, and the key the overlay 190 routine at code
offset `0x2267` returns for that index (FND-COMBAT-022):

| Index | Name | Word | Key |
|---|---|---|---|
| 0 | `None` | `0x3EE6` | 0 |
| 1 | `Acid` | `0x52BB` | 1 |
| 2 | `Adrenalin Control` | `0x52FD` | 1 |
| 3 | `Animal Affinity` | `0x52F9` | 1 |
| 4 | `Anti Magic Shell` | `0x524C` | 1 |
| 5 | `Armor` | `0x5208` | 1 |
| 6 | `Barkskin` | `0x5289` | 1 |
| 7 | `Berserk` | `0x5258` | 1 |
| 8 | `Bio-Feedback` | `0x52FE` | 1 |
| 9 | `Blessed` | `0x527B` | 1 |
| 10 | `Blind` | `0x529B` | 1 |
| 11 | `Blinking` | `0x5221` | 1 |
| 12 | (empty) | `0x0000` | 0 |
| 13 | `Blurry` | `0x5214` | 1 |
| 14 | `Body Weaponry` | `0x52FF` | 1 |
| 15 | `Borrowed Strength` | `0x4AAC` | 1 |
| 16 | `Bound By Thorns` | `0x52C4` | 1 |
| 17 | `Bramble Staff` | `0x5298` | 1 |
| 18 | `Brave` | `0x5283` | 1 |
| 19 | `Burning` | `0x52BC` | 1 |
| 20 | `Controlled` | `0x5307` | 1 |
| 21 | `Confused` | `0x5240` | 1 |
| 22 | `Cursed` | `0x527C` | 1 |
| 23 | `Dancing` | `0x526A` | 1 |
| 24 | `Devouring Brain` | `0x0000` | 1 |
| 25 | `Detect Invisible` | `0x5215` | 1 |
| 26 | `Detect Traps` | `0x528D` | 1 |
| 27 | `Diseased` | `0x529D` | 1 |
| 28 | `Dispel Evil` | `0x52CD` | 1 |
| 29 | `Displaced` | `0x5301` | 1 |
| 30 | `Engulfed` | `0x2B6A` | 1 |
| 31 | `Enhanced Strength` | `0x4AAC` | 1 |
| 32 | `Enlarged` | `0x520D` | 1 |
| 33 | `Entranced` | `0x5291` | 1 |
| 34 | `Extra Hit Points` | `0x4AAE` | 1 |
| 35 | `Afraid` | `0x5231` | 1 |
| 36 | `Cloak of Fear` | `0x52B0` | 1 |
| 37 | `Feebleminded` | `0x5246` | 1 |
| 38 | `Fire Shielded` | `0x5232` | 1 |
| 39 | `Flame Blade` | `0x528E` | 1 |
| 40 | `Flesh Armored` | `0x5303` | 1 |
| 41 | `Free Action` | `0x52B7` | 1 |
| 42 | `Hasted` | `0x5225` | 1 |
| 43 | `Heart Seeker` | `0x52DC` | 1 |
| 44 | `Heat Exhaustion` | `0x529F` | 1 |
| 45 | `Increased Movement` | `0x5225` | 1 |
| 46 | `Mirror Images` | `0x521B` | 1 |
| 47 | `Immune to Missiles` | `0x522B` | 1 |
| 48 | `Immune to Paralysis` | `0x521C` | 1 |
| 49 | `Spell Immunity` | `0x5235` | 1 |
| 50 | `Impeded` | `0x523B` | 1 |
| 51 | `Hasted` | `0x5225` | 1 |
| 52 | `Slowed` | `0x522C` | 1 |
| 53 | `Invisible` | `0x5219` | 1 |
| 54 | `Invisible to Undead` | `0x5280` | 1 |
| 55 | `Spell Invulnerability` | `0x5251` | 1 |
| 56 | `Otiluke's Sphere` | `0x5237` | 1 |
| 57 | `Ironskin` | `0x52D0` | 1 |
| 58 | `Jamming Psionics` | `0x5313` | 1 |
| 59 | `Life Drained` | `0x5270` | 1 |
| 60 | `Lending Strength` | `0x5306` | 1 |
| 61 | `Low Magic Res.` | `0x5248` | 1 |
| 62 | `Magic Plague` | `0x5278` | 1 |
| 63 | `Magma Blade` | `0x52B9` | 1 |
| 64 | `Mind Barred` | `0x5310` | 1 |
| 65 | `No Attacks` | `0x2B6A` | 1 |
| 66 | `Cannot Move` | `0x2B68` | 1 |
| 67 | `Cannot Use Psionics` | `0x2B6C` | 1 |
| 68 | `Can't Cast` | `0x2B6B` | 1 |
| 69 | `Paralyzed` | `0x5290` | 1 |
| 70 | `Petrified` | `0x5257` | 1 |
| 71 | `Poisoned` | `0x52BB` | 1 |
| 72 | `Prayer` | `0x52A3` | 1 |
| 73 | `Protected vs Cold` | `0x5293` | 1 |
| 74 | `Protected vs Energy` | `0x52FA` | 1 |
| 75 | `Protected vs Evil` | `0x5282` | 1 |
| 76 | `Protected vs Evil` | `0x52BE` | 1 |
| 77 | `Protected vs Fire` | `0x5292` | 1 |
| 78 | `Protected vs Lightning` | `0x52BF` | 1 |
| 79 | `Protected vs Life Drain` | `0x52A1` | 1 |
| 80 | `Protected vs Weather` | `0x52C0` | 1 |
| 81 | `Rainbow Bow` | `0x52D1` | 1 |
| 82 | `Weak` | `0x2B72` | 1 |
| 83 | `Lowered Movement` | `0x2B68` | 1 |
| 84 | `Gaze Reflection` | `0x520E` | 1 |
| 85 | `Spell Reflection` | `0x5236` | 1 |
| 86 | `Returning to the Earth` | `0x52C2` | 1 |
| 87 | `Sanctuary` | `0x5285` | 1 |
| 88 | (empty) | `0x0000` | 0 |
| 89 | `Shielded` | `0x5211` | 1 |
| 90 | `Shillelagh` | `0x5286` | 1 |
| 91 | `Slowed` | `0x522C` | 1 |
| 92 | `Absorbing Spells` | `0x2B69` | 1 |
| 93 | `Draining Spells` | `0x0000` | 0 |
| 94 | `Spirit Armored` | `0x522D` | 1 |
| 95 | `Spirit Hammer` | `0x5295` | 1 |
| 96 | `Stone Skin` | `0x523C` | 1 |
| 97 | `Stone Weapon` | `0x0000` | 1 |
| 98 | `Strength` | `0x521F` | 1 |
| 99 | `Strength of One` | `0x4AAC` | 1 |
| 100 | `Suffocating` | `0x2B71` | 1 |
| 101 | `Sunstroke` | `0x52DD` | 1 |
| 102 | `Berserk` | `0x5258` | 1 |
| 103 | `Time Stopped` | `0x5276` | 1 |
| 104 | `Magical Vestments` | `0x52A0` | 1 |
| 105 | `Weak` | `0x2B72` | 1 |
| 106 | `Weapon Grafted` | `0x5304` | 1 |
| 107 | `Vulnerable to Weather` | `0x52C1` | 1 |
| 108 | `Wounded` | `0x2B73` | 1 |
| 109 | `Air Lens` | `0x5297` | 0 |
| 110 | `Bitten` | `0x521D` | 0 |
| 111 | `Biting` | `0x0000` | 0 |
| 112 | `Hesitating` | `0x528F` | 0 |

## Interpretation

The records name the effects a character can be under, from spells, psionic powers, poisons and
wounds. Index 0, `None`, is never shown, since the panel shows `Okay` for 0. The empty records
12 and 88 and `Draining Spells` (93) have key 0, so the panel names one of them only when the
character has no effect with key 1. `Hasted`, `Slowed`, `Weak`, `Berserk` and `Protected vs Evil` each name two
records.

## Alternatives

What the word after each name holds was not read; the values are 0 or lie from `0x2B68` to
`0x5313`, which may be offsets into the data segment. That the list the panel sorts holds indexes into this
table is shown by the panel's use of the result; which routine fills the list was not read.

## How to reproduce

Read 113 records of 31 bytes from file offset `0x41A70` of `DSUN.EXE`, and emulate the key
routine from each target of the jump table at offset `0x236E` of overlay 190, counting the
`inc dx` instructions until `mov ax, dx` at `0x236A`.
