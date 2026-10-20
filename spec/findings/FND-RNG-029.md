---
id: FND-RNG-029
title: Overlay 184's five generator calls are the generation screen's opening picture draw and four picks of a Preserver's first spells, one of four each
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D868..0x0006DBB8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DD89..0x0006DE22
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000436F0..0x000436F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E16C..0x0006E171
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EA10..0x0006ED5D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0006ED5D..0x0006ED7D
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py and overlay_listing.py at commit c40653e3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 184's code starts at file `0x6D090`.
Trampolines `56DD:003E` and `56DD:002F` enter `+07D8` and `+1980`. Decoding each from its entry
to its far return shows FND-RNG-028's five overlay-184 generator sites as instructions: `+0AEA`
in `+07D8`, and `+1AE5`, `+1B51`, `+1BCD` and `+1C39` in `+1980`. Each is a far call of
`1000:0822` whose segment word is a descriptor-0 fixup.

### The generation screen's opening draw, `+07D8`

`+07D8` is the generation screen's setup routine (FND-PARTY-073), called from `+1641` and from
overlay 190 `+0EF8`. Every branch between its entry and `+0AE8` lands at or before `+0AE8`, so
each call that returns from its earlier callees reaches the draw. It sign-extends the draw,
multiplies it by 6 and divides by 32768 as signed 32-bit values, giving `i` from 0 to 5, reads
the signed byte at `4E68:0016 + i`, and uses four times it to index the far pointers at
`DS:11C8`, which it passes to `2D40:3BEC` with the other arguments of a picture draw
(`+0AE8..+0B28`). This is the same reduction and byte table as the reroll button's last draw
(FND-PARTY-071). For a new character (`DS:112E` is -1) the routine later calls overlay 186
`+05DD` at `+0B9F`, whose `roll_sum` draw picks the default name (FND-TEXT-008), so the picture
draw comes first.

The new-character block of `+07D8` (`+0810..+089B`, taken when `DS:112E` is -1) stores 0 in
the words at `4E4F:0000`, `4E4F:0002` and `4E4F:0004`. On the edit path, for each class position
0 to 2 of the working record at `DS:1429` (FND-PARTY-073's 1-to-8 numbering), code 5, the
preserver, stores that position's level byte (`+0x1E` plus the position) in `4E4F:0000`, and
code 1 or 2 stores the level in `4E4F:0002` and byte `+0x1D` of the working combatant record in
`4E4F:0004` (`+0CF9..+0D92`). FND-PARTY-076's census of references to segment `4E4F` puts every
store to `4E4F:0000..0005` in overlay 184, and these are the stores to the three words. The file
holds 0 in all three (`0x436F0..0x436F6`).

### The Preserver's first spells, `+1980`

`+1980` has one direct caller, `+10DD` in the DONE path `+1029` (FND-PARTY-070), which passes the
slot in `4E71:0B44`, removes the argument and does not read the return value or the flags. With
`s` that slot it works on the details record at `DS:19C5 + 66 * s` and the 15-byte `SPST` entry
at `4D62:039C + 15 * s`, in which bit `n & 7` of byte `n >> 3` is spell `n` (FND-PARTY-104).

1. For class positions 0, 1 and 2 in turn, stopping at the first whose class byte (`+0x1B` plus
   the position) is 11, the preserver code, it takes that position's level byte as `L` and sets
   a flag when `L` differs from the word at `4E4F:0000` (`+1993..+19DC`).
2. It then scans the positions again for class codes 13 to 16, 1 to 4 and 5 to 8, comparing the
   level with `4E4F:0002` and byte `+0x1D` of the combatant record with `4E4F:0004`, and sets a
   second local flag (`+19DE..+1A6E`). That flag is read only by a comparison just before the far
   return, whose result nothing uses.
3. When the first flag is clear it returns with no draw (`+1A71..+1A77`).
4. Otherwise it clears the bits for spells 0 to 114 (`+1A7A..+1AA9`). When `L` is 7 it sets spell
   43. When `L` is 1 or less (signed) it sets spell 11 and returns.
5. For `L` of 2 or more it sets spells 27 and 29, then makes four draws. Each is sign-extended,
   multiplied by 4 and divided by 32768 as signed 32-bit values, giving 0 to 3, and indexes a
   four-word jump table guarded by an unsigned test against 3, which no draw fails. In order:

| Draw | Site | Table | Spell set for result 0, 1, 2, 3 | Then sets |
|---|---|---|---|---|
| 1 | `+1AE5` | `+1CE5` | 12, 16, 17, 19 | |
| 2 | `+1B51` | `+1CDD` | 14, 18, 22, 24 | 13 |
| 3 | `+1BCD` | `+1CD5` | 0, 9, 7, 4 | |
| 4 | `+1C39` | `+1CCD` | 1, 2, 3, 10 | 8 and 11 |

Each of the 32768 draws maps to one result, 8192 to each, so each listed spell of a pick has
exactly one chance in four. The four jump tables are 16 words at `+1CCD..+1CED`, file
`0x6ED5D..0x6ED7D`, directly after the far return; every target is one of the sixteen
`or`-and-jump blocks listed above.

## Interpretation

The five sites serve two things. `+07D8`'s draw chooses which of six pictures the generation
screen shows when it opens, the presentation of SCR-UI-004, which does not describe it yet.
`+1980`'s four draws give a character with a preserver class its first spells when DONE stores
it: a fixed set by preserver level and one of four spells from each of four groups. A new
preserver of level 1 or more always meets the level test, since the word it is compared with
is 0; an edited
character meets it when its preserver level differs from the one the screen saved on opening,
and then every spell from 0 to 114 it had learned is cleared before the new set is given.

## Alternatives

- The spell numbers are `SPST` bit numbers, the same numbering as FND-PARTY-104's learning
  window; their names and their levels in the game's tables were not read here.
- The second flag of step 2 does not change any state or result. Reading it as a priest spell
  grant would need a reader; none exists in `+1980` or at its caller.
- `+1029` makes other calls before `+1980` (`+0871` of overlay 183, `+1648`, `+1876`); whether
  any of them draws is not read here, so the full draw order of DONE is not established.
- For an edited character with no preserver position on opening, `4E4F:0000` keeps what the last
  pass that wrote it left: 0 after a new character or at program start, or an earlier edited
  character's preserver level. Whether such a character can gain the preserver class before DONE
  was not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `trampoline_target.py <dsun>
56DD:003E 56DD:002F`; `direct_callers.py <dsun> 184+1980 184+07D8` (one near caller at `+10DD`
for `+1980`; `+1641` and overlay 190 `+0EF8` for `+07D8`); `overlay_listing.py <dsun> 184 6D090
6F197` for the whole overlay, and in it the ranges above. Read the jump tables as little-endian
words at file `0x6ED5D`. Listings are kept under `GAME_DIR/analysis/rng-callers-20261020/`.
Nothing was executed.
