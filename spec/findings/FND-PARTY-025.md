---
id: FND-PARTY-025
title: The two calls before the party-loader gate reserve off-screen video memory, and fail only when the reservation table or the pool is full
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2707..1BF3:295E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A9C..0x00069AD4
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; Python 3.14.7 scan of every byte offset of DSUN.EXE for far and near calls
environment: null
---

## Observation

The gate routine of FND-PARTY-021 (overlay 182 offset `0x1167`) makes the far calls at
`DSUN.EXE+0x00069AA5` and `0x00069AC1` to `1BF3:27A8` (file offset `0x000138D8`; the MZ
relocation gives the segment). Both routines below set DS to their own code segment, so the
words they keep are in segment `1BF3`, at the offsets given.

**The table.** It has 256 entries, indexed by twice the entry number. Entry `i` has a start
paragraph at `0x0004 + 2i`, a size in paragraphs at `0x0204 + 2i`, a rectangle (left, top, right,
bottom) at `0x0404`, `0x0604`, `0x0804` and `0x0A04 + 2i`, and a flag word at `0x0C04 + 2i`, in
which `0x80` marks a free entry and `0x40` an entry that shares another entry's memory. The word
at `0x0E4E` holds the next free paragraph and the word at `0x0E4C` the limit.

**Setup, `1BF3:271B`** (one caller, the far call at `DSUN.EXE+0x00067679` in overlay 180). It
sets entry 0 to paragraph `0xA000` and entry 1 to `0xA400`, each `0x3E8` paragraphs with the
rectangle (0, 0, 319, 199) and flags 0, the next free paragraph to `0xA7E8`, the limit to
`0xAFFB`, and the flags of entries 2 to 255 to `0x80`. That leaves `0x813` (2,067) paragraphs
for reservations.

**Reserving, `1BF3:27A8`**, with left, top, right and bottom at `[bp+6]` to `[bp+0Ch]`:

1. `1BF3:2707` scans entries 0 to 255 for the first with flag `0x80` and returns it with the
   carry flag clear, or sets the carry flag when there is none; the routine then returns
   `0xFFFF`.
2. The size in bytes is `((right >> 2) - (left >> 2) + 1) * (bottom - top + 1)`, as an unsigned
   16-bit multiply. When the high word of the product is nonzero or the size is above `0x3E80`
   (16,000), it returns `0xFFFF`.
3. The size in paragraphs is `(size + 15) >> 4`. When the next free paragraph plus that size is
   above the limit (unsigned compare), it returns `0xFFFF`.
4. Otherwise it gives the entry the next free paragraph and the size, stores the rectangle and
   flags 0, advances the next free paragraph by the size, and returns the entry number.

**Releasing, `1BF3:28C5`** (file `0x000139F5`) does nothing for an entry whose flags have `0x80`
or `0x20` set. Otherwise it sets `0x80`; for an entry without `0x40` it subtracts the size from
the next free paragraph and moves every later reservation down by that size, copying video
memory with the VGA graphics controller's mode register (port `0x3CE`, index 5) in write mode 1.

**The two calls.** The first passes (0, 0, 27, 15): 7 × 16 = 112 bytes, 7 paragraphs. The second
passes (0, 0, 99, 33): 25 × 34 = 850 bytes, 54 paragraphs. The gate routine stores the results at
`DS:1400` and `DS:1402` and returns without reaching the gate when either is `0xFFFF`.

## Interpretation

Neither call can fail on its size. Each fails only when all 256 entries are in use or fewer
than 7 (then 54) paragraphs remain between the next free paragraph and the limit, which depends
on the reservations made and released before START GAME. With the pool's whole space free,
both succeed. The sizes read as one plane of a planar 256-colour mode, a quarter of the width,
which fits the addresses at `0xA000`; this reading does not establish the video mode.

## Alternatives

- The calls fail on a new game because earlier reservations fill the pool: not ruled out; the
  reservations and releases that run between program start and the gate were not listed.
- The limit or next-free word changes elsewhere: the only direct stores to `0x0E4E` in segment
  `1BF3` are the setup, the reservation and the release above, and `0x0E4C` is stored only by
  the setup; stores through other registers were not searched.

## How to reproduce

Disassemble `DSUN.EXE` from file offset `0x00013837` to `0x00013A8F` as 16-bit code (segment
`1BF3`, offsets `0x2707` to `0x295E`) and overlay 182 from `0x00069A9C` to `0x00069AD4`. List
callers by decoding a far or near call at every byte offset of the file and keeping those whose
resolved target is `0x0001384B`, `0x000138D8` or `0x000139F5`.
