---
id: FND-RNG-097
title: Page-copy destinations and local frame stores require separate page and stack admission before excluding clock storage
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
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
tool: frame review with resident_listing.py at d01beea4, Python 3.12.10 and Capstone distribution 5.0.9; FND-RNG-096; historical Capstone 5.0.9 over the load image; tools/research/exec-census/immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

This replacement retains FND-RNG-080's page-table and local store
observations and corrects their blanket destination exclusion after
FND-RNG-096 withdrew universal DS/SS equality. The historical page-word
sweep is not a new exhaustive alias census. The current bounded frame
review uses approved installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, with the pinned static reader.


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

Under the recorded page initialization, allocation and alias histories,
the selected page segments supply the video-copy destinations. Those
histories and ranges are conditions, not an exhaustive runtime writer
proof. In particular, indexed stores and copied/changed allocator state
must not be excluded merely by the literal-displacement sweep.

The BP-relative stores address SS, independently of the selected ES
page segment and of the current DS binding. The reviewed 0109 entry
uses ENTER with eight local bytes; 02A8 uses six. The previously
recorded stores to BP-2 through BP-8 therefore need their actual
entry/frame identities and ranges before excluding clock storage.
The 0109 common epilogue restores its saved registers and uses LEAVE;
that does not itself prove that earlier stack writes missed a target.

The earlier conclusion that all these operations miss the data-segment
clock windows is withdrawn. Equal DS and SS would not suffice anyway:
actual frame offsets, aliases and lifetime also determine overlap.
FND-COMBAT-056's observed frames in selected runs do not establish every
frame admitted by the static roots. Q-RNG-005 and Q-RNG-029 retain the
actual stack/page histories, alias mechanisms and unresolved transfers.
No native overlap, changed clock value or presentation fault is proved.

## Alternatives

- Selecting a video page excludes every store in the function: the
  ordinary frame stores address SS, not ES.
- Segment equality proves frame exclusion: offsets and frame lifetime
  remain necessary even when segments match.
- A literal writer sweep proves an invariant: computed/aliased stores,
  modified state and alternative entries remain outside that proof.

## How to reproduce

Disassemble `1BF3:271B..295F`, `1BF3:42FB..43AE` and `2707:0109..06B9` from the load image;
sweep segment `1BF3` (to `2707:0000`) for stores with those displacements;
`immediate_search.py <dsun> 0E4C` and `... 0E4E`.

For the correction, read the established 2707 entries 0109..02A8,
02A8..0395, 0395..048E, 048E..055A and 055A..06B9 with
resident_listing.py at d01beea4. Retain
graphics-clock-frame-review.txt in
GAME_DIR/analysis/handler-census-20261106/. The embedded jump tables
remain data; sequential listings alone do not establish their bytes as
instructions. Reuse the historical case reading for its bounded local
stores, retaining separate page and SS-frame admission. No original game,
emulator session or emulated call supplied evidence in this correction.
