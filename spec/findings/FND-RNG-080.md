---
id: FND-RNG-080
title: The page words that 1BF3:42FB returns hold only segments from A000 to AFFB or page offsets, and the cases of the segment 2707 jump tables write only through ES set from them and their own stack frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:271B..1BF3:295F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42FB..1BF3:43AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0109..2707:06B9
tool: Capstone 5.0.9 over the load image; tools/research/exec-census/immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static. Segment `1BF3`'s routines here set `DS` to `CS`.

**The page tables.** Page `n` has a word at `CS:0004 + 2n` (its segment, or for an alias the byte
offset `2m` of another page), a size in paragraphs at `CS:0204 + 2n`, bounds at `CS:0404`,
`0604`, `0804` and `0A04 + 2n`, and flags at `CS:0C04 + 2n` (`0x80` free, `0x40` alias). A linear
sweep of segment `1BF3` for stores whose displacement lies in `0x0004..0x0204` or
`0x0C04..0x0E04` finds these writers of the page words and flags:

- `1BF3:271B..27A7` stores `0xA000` at `CS:0004` and `0xA400` at `CS:0006`, 0 in their flags, the
  limit `0xAFFB` at `CS:0E4C` and the next free segment `0xA7E8` at `CS:0E4E`, and `0x80` in the
  flags of pages 2 to 255;
- `1BF3:27A8..282C` takes a free page `SI` from `1BF3:2707`, which searches the flags for bit
  `0x80`; it returns -1 unless the new size in paragraphs, added to `CS:0E4E`, stays at or below
  `CS:0E4C`, and otherwise stores the old `CS:0E4E` as the page's segment, advances `CS:0E4E` and
  clears the flags;
- `1BF3:282D..28A8` follows the alias flag from the given page to a page without it, stores that
  page's offset `2m` as the new page's word and sets its flags to `0x40`;
- `1BF3:28C5..295F` sets bit `0x80` of a page's flags, and for a page that is not an alias lowers
  `CS:0E4E` by its size and moves each later page down by that size, storing its new segment at
  `CS:0004 + 2n` (`1BF3:2931`);
- `1BF3:583D..5840` swaps `0xA000` and `0xA400` between `CS:0004` and `CS:0006` (FND-PARTY-195).

`CS:0E4C` is stored only at `1BF3:272B` and `CS:0E4E` only at `1BF3:2783`, `27EE` and `2900`;
`immediate_search.py` finds the other operands `0xE4C` and `0xE4E` with an index register or in
other segments.

`1BF3:42FB(n)` follows the alias flag to a page without it and returns that page's word at
`CS:0004`.

**Segment 2707.** In `2707:0109..06B9`, outside the five tables at `2707:018D`, `02FF`, `03DA`,
`04D5` and `0591`, every `rep movsb` writes `ES:DI`, and `ES` is loaded only from what
`1BF3:42FB` returns (`2707:0137`, `02BB`, `03A8`, `04A1`); `DS` is loaded from it (`2707:0144`) or
popped back after it was pushed, except on the paths of `2707:02A8` and `2707:0395` where
FND-RNG-056 finds `DS` taking the caller's `SI`; after those returns `2707:055A` writes no memory
before `2707:06B6` pops its saved `DS`. The other stores are to the frame words `[bp-2]` to `[bp-8]`
and `neg` of `[bp-6]`, and the routines write the VGA ports `0x3C4`, `0x3C5`, `0x3CE` and `0x3CF`.
The second table's cases only call `2707:0395`, `02A8` and `048E`.

## Interpretation

A page segment is `0xA000`, `0xA400` or a segment from `0xA7E8` to below `0xAFFB`, so the copies
of segment `2707` write only video memory and their own stack frames. They do not write the data
segment's clock windows.

## Alternatives

The sweep is linear from the segment's start and could miss a store hidden by a wrong decoding;
it found every writer the allocator's own reads lead to. The frames are on the stack, which shares
the data segment (FND-RNG-061) and lies above the windows that Q-RNG-005 lists in the runs read so
far (FND-COMBAT-056).

## How to reproduce

Disassemble `1BF3:271B..295F`, `1BF3:42FB..43AE` and `2707:0109..06B9` from the load image;
sweep segment `1BF3` (to `2707:0000`) for stores with those displacements;
`immediate_search.py <dsun> 0E4C` and `... 0E4E`.
