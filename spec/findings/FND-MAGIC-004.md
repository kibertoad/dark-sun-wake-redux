---
id: FND-MAGIC-004
title: A priest spell's spheres are the byte at 51F1:07D3 + spell, bits 0x01 to 0x08 for air, earth, fire and water and 0x10 and 0x20 for two grades of Cosmos access, and overlay 177 +0A45 admits it when the byte shares a bit with the class code's mask, so a cleric and a ranger take their element and the Cosmos spells of levels 1 to 3, a druid its element and every Cosmos spell, and a spell of several spheres any caster of one of them
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062B45..0x00062BDA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00062BDA..0x00062C00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062C00..0x00062CC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006260A..0x000626B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062500..0x00062592
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AC5..0x00062B1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047956..0x000479CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D660..0x0004D66C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008486E..0x000848AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084562..0x00084568
tool: tools/research/exec-census/overlay_listing.py, segment_references.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7; a short read of the bytes at file 0x478E3 + n and of the DATA names in RESOURCE.GFF
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x62100 + x` for overlay 177 and `0x83400 + x` for overlay 197.
Segment `51F1` starts at file `0x47110` and holds `0x08BE` bytes (FND-PARTY-106), so
`51F1:07D3 + n` is file `0x478E3 + n`, and spells 115 to 234 take its last 120 bytes,
`51F1:0846..51F1:08BE`, after the 34 three-byte psionic power rows at `51F1:07E0` (FND-RNG-046,
FND-PARTY-210). Class codes are FND-PARTY-057's: 1 to 4 Cleric, 5 to 8 Druid, 13 to 16 Ranger.

**The class masks.** Overlay 177 `+0A45` takes a spell `n` and a class code `c`
(`+0A45..+0ADA`). It returns 0 for `n` below 0 or from 235 (`+0A53..+0A5C`). For `n` below 115 it
returns 1 when `c` is 18 or 11 (`+0A5E..+0A73`). For `n` from 115 to 234 it switches on `c` less 1,
taken as unsigned and above 0x12 sent to a mask of 0, through the 19 words at `cs:0ADA`
(`+0A80..+0A8A`), builds a mask in `cl` and returns 1 when the byte at `51F1:07D3 + n` shares a
bit with it (`+0AC1..+0AD3`). The words send the codes to these masks:

| Codes | Mask |
| --- | --- |
| 1, 2, 3, 4 | 0x11, 0x12, 0x14, 0x18 |
| 5, 6, 7, 8 | 0x31, 0x32, 0x34, 0x38 |
| 13, 14, 15, 16 | 0x11, 0x12, 0x14, 0x18 |
| 19 | 0x3F |
| 0, 9 to 12, 17, 18 and any code above 19 | 0 |

The druid entries fall through `or cl, 0x20`, `or cl, 0x10` and the element bit; the cleric and
ranger entries enter one instruction later, after the 0x20.

**The sphere bytes.** The table below gives, for each spell from 115 to 234, the name in the 32
bytes at `+0x20` of `DATA` resource `n` of `RESOURCE.GFF` (as FND-PARTY-163 reads names), the
level overlay 177 `+09C5` gives it, and the byte at `51F1:07D3 + n`. `+09C5` returns the greatest
`c` from 1 to 10 whose byte at `DS:0660 + c` is not above `n` (`+09DA..+0A19`); those bytes are 115,
127, 143, 165, 189, 208, 216, 231, 232 and 233 (FND-PARTY-110).

| Spell | `DATA` name | Level | Byte |
| --- | --- | --- | --- |
| 115 | `BLESS` | 1 | 0x30 |
| 116 | `CURSE` | 1 | 0x30 |
| 117 | `CURE LIGHT WOUNDS` | 1 | 0x30 |
| 118 | `CAUSE LIGHT WOUNDS` | 1 | 0x30 |
| 119 | `ENTANGLE` | 1 | 0x30 |
| 120 | `INVISIBLE TO UNDEAD` | 1 | 0x30 |
| 121 | `MAGIC STONE` | 1 | 0x02 |
| 122 | `PROTECTION FROM EVIL` | 1 | 0x30 |
| 123 | `REMOVE FEAR` | 1 | 0x30 |
| 124 | `CAUSE FEAR` | 1 | 0x30 |
| 125 | `SANCTUARY` | 1 | 0x30 |
| 126 | `SHILLELAGH` | 1 | 0x30 |
| 127 | `AID` | 2 | 0x30 |
| 128 | `ALTRUISM` | 2 | 0x30 |
| 129 | `BARKSKIN` | 2 | 0x30 |
| 130 | `CHANNEL STENCH` | 2 | 0x01 |
| 131 | `CHARM MAMMAL` | 2 | 0x30 |
| 132 | `DUSTDEVIL` | 2 | 0x03 |
| 133 | `FIND TRAPS` | 2 | 0x30 |
| 134 | `FLAME BLADE` | 2 | 0x04 |
| 135 | `HESITATION` | 2 | 0x30 |
| 136 | `HOLD PERSON` | 2 | 0x30 |
| 137 | `MUSIC OF THE SPHERES` | 2 | 0x30 |
| 138 | `RESIST FIRE` | 2 | 0x04 |
| 139 | `RESIST COLD` | 2 | 0x04 |
| 140 | `SILENCE 15 FEET` | 2 | 0x30 |
| 141 | `SPIRITUAL HAMMER` | 2 | 0x30 |
| 142 | `STEAL BREATH` | 2 | 0x01 |
| 143 | `AIR LENS` | 3 | 0x01 |
| 144 | `BRAMBLESTAFF` | 3 | 0x0A |
| 145 | `CREATE SMOKE` | 3 | 0x05 |
| 146 | `CURE BLINDNESS` | 3 | 0x30 |
| 147 | `CAUSE BLINDNESS` | 3 | 0x30 |
| 148 | `CURE DISEASE` | 3 | 0x30 |
| 149 | `CAUSE DISEASE` | 3 | 0x30 |
| 150 | `DISPEL MAGIC` | 3 | 0x30 |
| 151 | `HEAT EXHAUSTION` | 3 | 0x04 |
| 152 | `MAGIC VESTMENT` | 3 | 0x30 |
| 153 | `NEGATIVE PLANE PROTECTION` | 3 | 0x30 |
| 154 | `PLANT GROWTH` | 3 | 0x30 |
| 155 | `PRAYER` | 3 | 0x30 |
| 156 | `PROTECTION FROM FIRE` | 3 | 0x04 |
| 157 | `REMOVE CURSE` | 3 | 0x30 |
| 158 | `BESTOW CURSE` | 3 | 0x30 |
| 159 | `REMOVE PARALYSIS` | 3 | 0x30 |
| 160 | `SAND SPRAY` | 3 | 0x02 |
| 161 | `SPIKE GROWTH` | 3 | 0x30 |
| 162 | `STONE SHAPE` | 3 | 0x02 |
| 163 | `STRENGTH OF ONE` | 3 | 0x30 |
| 164 | `SUMMON INSECTS` | 3 | 0x30 |
| 165 | `ABJURE` | 4 | 0x20 |
| 166 | `BLOOD FLOW` | 4 | 0x08 |
| 167 | `CLOAK OF BRAVERY` | 4 | 0x20 |
| 168 | `CLOAK OF FEAR` | 4 | 0x20 |
| 169 | `CONDENSE` | 4 | 0x02 |
| 170 | `CURE SERIOUS WOUNDS` | 4 | 0x20 |
| 171 | `CAUSE SERIOUS WOUNDS` | 4 | 0x20 |
| 172 | `DEHYDRATE` | 4 | 0x08 |
| 173 | `DUST CLOUD` | 4 | 0x03 |
| 174 | `FOCUS HEAT` | 4 | 0x04 |
| 175 | `FREE ACTION` | 4 | 0x20 |
| 176 | `LUNGS OF WATER` | 4 | 0x08 |
| 177 | `MAGMA BLADE` | 4 | 0x04 |
| 178 | `NEUTRALIZE POISON` | 4 | 0x20 |
| 179 | `POISON` | 4 | 0x20 |
| 180 | `PRODUCE FIRE` | 4 | 0x04 |
| 181 | `QUENCH FIRE` | 4 | 0x04 |
| 182 | `PROTECTION FROM EVIL 10` | 4 | 0x20 |
| 183 | `PROTECTION FROM LIGHTNING` | 4 | 0x01 |
| 184 | `PROTECTION FROM WEATHER` | 4 | 0x21 |
| 185 | `INTENSIFICATION OF WEATHER` | 4 | 0x21 |
| 186 | `RETURN TO EARTH` | 4 | 0x02 |
| 187 | `SOOTHE` | 4 | 0x08 |
| 188 | `THORNS OF BINDING` | 4 | 0x0A |
| 189 | `CONJURE AIR ELEMENTAL` | 5 | 0x01 |
| 190 | `CONJURE FIRE ELEMENTAL` | 5 | 0x04 |
| 191 | `CONJURE EARTH ELEMENTAL` | 5 | 0x02 |
| 192 | `CONJURE WATER ELEMENTAL` | 5 | 0x08 |
| 193 | `CRUMBLE` | 5 | 0x0A |
| 194 | `CURE CRITICAL WOUNDS` | 5 | 0x20 |
| 195 | `CAUSE CRITICAL WOUNDS` | 5 | 0x20 |
| 196 | `DEFLECTION` | 5 | 0x01 |
| 197 | `DISPEL EVIL` | 5 | 0x20 |
| 198 | `FLAME STRIKE` | 5 | 0x04 |
| 199 | `INSECT PLAGUE` | 5 | 0x01 |
| 200 | `IRON SKIN` | 5 | 0x02 |
| 201 | `RAINBOW` | 5 | 0x20 |
| 202 | `QUICKSAND` | 5 | 0x0A |
| 203 | `RAISE DEAD` | 5 | 0x20 |
| 204 | `SLAY LIVING` | 5 | 0x20 |
| 205 | `SANDSTORM` | 5 | 0x01 |
| 206 | `SPIKE STONES` | 5 | 0x02 |
| 207 | `WALL OF FIRE` | 5 | 0x04 |
| 208 | `BLADE BARRIER` | 6 | 0x20 |
| 209 | `FIRE SEEDS` | 6 | 0x04 |
| 210 | `HEAL` | 6 | 0x20 |
| 211 | `HARM` | 6 | 0x20 |
| 212 | `HEARTSEEKER` | 6 | 0x0A |
| 213 | `SUNSTROKE` | 6 | 0x05 |
| 214 | `WALL OF THORNS` | 6 | 0x20 |
| 215 | `WATERS OF LIFE` | 6 | 0x08 |
| 216 | `CONFUSION` | 7 | 0x20 |
| 217 | `CONJURE GREATER AIR ELEMENTAL` | 7 | 0x01 |
| 218 | `CONJURE GREATER FIRE ELEMENTAL` | 7 | 0x04 |
| 219 | `CONJURE GREATER EARTH ELEMENTAL` | 7 | 0x02 |
| 220 | `CONJURE GREATER WATER ELEMENTAL` | 7 | 0x08 |
| 221 | `CREEPING DOOM` | 7 | 0x20 |
| 222 | `FIRESTORM` | 7 | 0x04 |
| 223 | `GLASS STORM` | 7 | 0x02 |
| 224 | `RESTORE` | 7 | 0x20 |
| 225 | `ENERGY DRAIN` | 7 | 0x20 |
| 226 | `SCIROCCO` | 7 | 0x01 |
| 227 | `SUNRAY` | 7 | 0x25 |
| 228 | `SYMBOL OF HOPELESSNESS` | 7 | 0x20 |
| 229 | `SYMBOL OF PAIN` | 7 | 0x20 |
| 230 | `SYMBOL OF PERSUASION` | 7 | 0x20 |
| 231 | `REVERSE WINDS` | 8 | 0x01 |
| 232 | `DISRUPTION` | 9 | 0x01 |
| 233 | `INSECT HOST` | 10 | 0x01 |
| 234 | `RIFT` | 10 | 0x20 |

The bytes take 11 values: 0x30 for 36 spells, all of levels 1 to 3; 0x20 for 27, all of levels 4
to 7 but spell 234; 0x01 for 13, 0x02 for 10, 0x04 for 15 and 0x08 for 7; 0x03 (Dustdevil, Dust
Cloud) and 0x05 (Create Smoke, Sunstroke) for 2 each; 0x0A for 5 (Bramblestaff, Thorns of
Binding, Crumble, Quicksand, Heartseeker); 0x21 for 2 (Protection from Weather, Intensification
of Weather); and 0x25 for 1 (Sunray). No byte has bit 0x10 without 0x20, no byte of a spell of
level 4 or more has bit 0x10, and none is 0 or has bit 0x40 or 0x80.

**Who reads the bytes.** `segment_references.py <dsun> 51F1` finds 72 words naming the segment.
In the instructions it prints around them, only two use displacement `0x07D3`: overlay 177 `+0AC6`
in `+0A45`, and overlay 197 `+14A3`. The others address `es:[bx]`, `es:[bx + 2]`, `es:[bx + 5]`,
`es:[bx + 0x17]`, `es:[bx + 0x7E0]` to `es:[bx + 0x7E2]`, `es:[di]` and `es:[0x840]`. `direct_callers.py <dsun> 177+0A45` finds near calls at
`+06D3` and `+0718` (in `+05B7`, the pool count of FND-MAGIC-002), `+08BB` (in `+07AF`) and
`+0BAB` (in `+0B00`), and one far call, from overlay 211 `+1FCC` (in `+1FB5`, FND-MAGIC-003).

Overlay 197 `+14A3` lies in `+111A`, which ends a hit when it returns 1 (FND-PARTY-112). When the
byte at `bp-5` is 0x0B, the target's combatant record (numbered by the word at
`4F49:0C34 + 3 * di`) has byte `+0x0E` equal to 0x0A, and the number in `si` is from 115 to 234,
it tests the byte at `51F1:07D3 + si` with 6 and on a shared bit jumps to `+1162`, which returns 1
(`+146E..+14AE`, `+1162..+1168`).

**The Use list.** For a party slot below 4, with `DS:13F8` not 1, overlay 177 `+050A` admits a
spell `n` from 115 to 234 when `+0B00` of `n` and the slot is nonzero and `+09C5` of `n`, as an
unsigned byte, is not above overlay 211 `+208C` of the slot and 2 (`+0525..+0593`). `+0B00` calls
`+0A45` once for each class position 0 to 2 that it does not pass over and returns 1 at the first
nonzero result (`+0B93..+0BB7`); it passes over position 1 or 2 when byte `+0x12` of the details
record numbered by word `+0x04` of the slot's combatant record is 1 and the position's level byte
`+0x1E` is not below position 0's (`+0B11..+0B91`). It reads the class byte at `+0x1B` plus the
position from the details record numbered by the slot itself (`+0B93..+0BA0`). It never tests
the class byte for 0; `+0A45` gives 0 for code 0. For a slot of 4 or more `+050A` asks `+0BC8`
instead and the sphere bytes are not read (`+0511..+0516`, `+0599..+05AC`).

Overlay 177 `+0400` with an even kind and a level `L` from 1 tries each spell from the byte at
`DS:0660 + L` up to, not including, the byte at `DS:0661 + L` and stores those `+050A` admits;
on a 22nd admitted spell it returns 21 without storing it (`+0466..+0483`). `direct_callers.py
<dsun> 177+0400` finds its far callers at overlay 211 `+1AAD` (the Use list, FND-PARTY-110),
overlay 211 `+1F35` (the window's level search, FND-MAGIC-003) and overlay 173 `+14F6`, which
passes kind 3 and level -1, so all spells 0 to 234 (`+14E9..+14FB`).

Counting the table against the masks, the spells `+0A45` admits per spell level 1 to 10:

| Caster | Mask | Levels 1 to 10 | Levels 1 to 7 |
| --- | --- | --- | --- |
| Cleric or ranger, air (1, 13) | 0x11 | 11 13 17 4 4 1 3 1 1 1 | 53 |
| Cleric or ranger, earth (2, 14) | 0x12 | 12 11 18 4 5 1 2 0 0 0 | 53 |
| Cleric or ranger, fire (3, 15) | 0x14 | 11 13 18 4 3 2 3 0 0 0 | 54 |
| Cleric or ranger, water (4, 16) | 0x18 | 11 10 16 5 3 2 1 0 0 0 | 48 |
| Druid, air (5) | 0x31 | 11 13 17 13 10 5 10 1 1 2 | 79 |
| Druid, earth (6) | 0x32 | 12 11 18 15 11 5 10 0 0 1 | 82 |
| Druid, fire (7) | 0x34 | 11 13 18 15 9 6 10 0 0 1 | 82 |
| Druid, water (8) | 0x38 | 11 10 16 16 9 6 9 0 0 1 | 77 |

The largest union of two element masks at one level is 21, earth and fire at level 3 (the 15
spells of 0x30, two of 0x02, two of 0x04, one 0x05 and one 0x0A).

Against the manual's Cleric Spell Summary (`RULE-MAGIC-002.cleric_spells.csv`), matching names
that differ only in spelling or abbreviation (Invisibility to Undead, Magical Stone, Silence 15'
Radius, Cure and Cause Blindness or Deafness, Magical Vestment, Adjure for `ABJURE`, Protect.
from Evil 10' Radius, Spiked Stones, Water of Life, Restoration): every matched spell has the
manual's level, and the element bits equal the manual's elements and a Cosmos spell has 0x20,
except these:

| Spell | Manual | Game |
| --- | --- | --- |
| 131 `CHARM MAMMAL` | Charm Person or Mammal, level 2, air | 0x30 |
| 184 `PROTECTION FROM WEATHER` | level 4, air and water | 0x21 |
| 185 `INTENSIFICATION OF WEATHER` | level 4, air and water | 0x21 |
| 213 `SUNSTROKE` | level 6, Cosmos | 0x05 |
| 227 `SUNRAY` | level 7, Cosmos | 0x25 |

The manual's Conjure Elemental (level 5, all four elements) and Conjure Greater Elemental
(level 7) are four spells each in the game, 189 to 192 and 217 to 220, one element each; its
Symbol is three, 228 to 230, each 0x20. Spell 132 `DUSTDEVIL` (level 2, 0x03) and spells 231 to
234 (levels 8 to 10) are not in the manual. That gives 116 spells of levels 1 to 7 against the
manual's 107.

## Interpretation

The game keeps a priest spell's spheres in one byte per spell in the executable's data, at
`51F1:07D3 + n`, and its level in the spell's number, through the ranges at `DS:0660`. Bits 0x01,
0x02, 0x04 and 0x08 are air, earth, fire and water: the sphere menu adds 0 to 3 to the class code
in that order (FND-PARTY-161, FND-PARTY-073), the masks of codes 1 to 4 give the same bits in the
same order, and the four Conjure Elemental spells carry the bit of the element in their name.
Bits 0x10 and 0x20 are Cosmos access: 0x10 is on the 36 Cosmos spells of levels 1 to 3 and 0x20
on every Cosmos spell, so 0x10 is the cleric's minor access and 0x20 the druid's major access.

A character's sphere is not stored apart from its class: it is the code within the class's four,
`(code - 1) & 3` for codes 1 to 8 and 13 to 16, in the class bytes `+0x1B..+0x1D` of the details
record. Each class position carries its own sphere; character generation gives every class the
one chosen sphere (FND-PARTY-073), and the DUAL window asks a sphere for the new class
(FND-PARTY-161).

So the list builder offers a cleric the spells of the cleric's element and the Cosmos spells of
levels 1 to 3, as the manual says, with the level-3 cut held in the data and not in the code. A
druid gets the druid's element and every Cosmos spell, as the manual says. A ranger gets exactly
a cleric's spells of the same element: its element's spells and the Cosmos spells of levels 1 to
3, which the manual does not give a ranger. Which levels each can reach is the cast window's
limit (FND-MAGIC-003), so a ranger takes those spells from ranger level 8 and its element's
fourth-level spells from 14.

A spell of several spheres is open to a caster of any of them, since the test is a shared bit. A
character with two sphere classes gets the union of both masks, since `+0B00` accepts the first
class position that admits the spell. The 21-spell cap of `+0400` is never reached by a party
member's list: the largest union is 21.

Spells 231 to 234 have levels 8, 9 and 10 from `+09C5`. `+208C` with kind 2 gives 8 only from a
class level of 20 (FND-MAGIC-003), above the 15 a level reaches in play (FMT-PARTY-001), so no
party member is offered them unless `DS:13F8` is 1.

The second reader, overlay 197 `+14A3`, makes `+111A` end a hit by a priest spell with an earth or
fire bit on a target whose byte `+0x0E` is 0x0A, under a condition on its local byte at `bp-5`.
It reads the same bits as spheres, so the bytes are spheres for the hit as well as for the list.

## Alternatives

- That bits 0x10 and 0x20 stand for minor and major Cosmos access is read from where they are set
  and which classes test them; no string names them. Read as "cleric may cast" and "druid may
  cast", they give the same lists.
- The bytes could be written at run time through a pointer formed elsewhere: `segment_references.py`
  finds no store into `51F1` near a load of the segment (FND-PARTY-106 also reads the five local
  far pointers), but a segment computed by arithmetic or a far pointer copied out of those routines
  is not covered.
- Whether `DATA` numbers 115 to 234 are the same numbering as the cast list's spell numbers rests
  on FND-PARTY-110 and FND-PARTY-163, which pass the list's number to the hit routine as a `DATA`
  number; the names fit the levels and spheres throughout.
- `+0B00` reads the class bytes from the details record numbered by the slot and the origin and
  level bytes from the one numbered by the combatant word `+0x04`; for a party slot these are
  taken to be the same record, which was not checked here. Which positions count for a dual-classed
  human is FND-PARTY-110's and FND-PARTY-258's reading and was not re-derived.
- What byte `+0x0E` 0x0A and the byte at `bp-5` of overlay 197 `+111A` stand for was not read.
- What overlay 173 `+14F6` does with its list of all spells was not read.
- Code 19's mask 0x3F would admit every priest spell; no character of FMT-PARTY-001 has code 19.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 177 62500 625B7`, `... 177 6260A 626B7`, `... 177 62AC5
62C00`, `... 177 62C00 62CC8`, `... 197 84800 84960`, `... 197 84555 84570`, `... 197 84AA0
84AB4` and `... 173 5BDE0 5BE30`; `segment_references.py <dsun> 51F1`, keeping the hits with
displacement `0x7D3`; `direct_callers.py <dsun> 177+0A45 177+0B00 177+050A 177+0400`. Read the
19 words at file `0x62BDA`, the 23 bytes at file `0x4D655` and the bytes at file `0x478E3 + n` for
`n` from 115 to 234. For each `n` from 115 to 234, read the text up to the first 0 in the 32 bytes
at `+0x20` of `DATA` resource `n` of `RESOURCE.GFF` (FMT-GFF-001 to FMT-GFF-007). Count, for each
mask of the table and each level from `+09C5`, the bytes that share a bit with the mask, and
compare names, levels and bits with `RULE-MAGIC-002.cleric_spells.csv`.
