---
id: FND-SCRIPT-026
title: Overlay 169 allocates and clears the eight script variable blocks before MAS/99 runs, and the interpreter clears the four local blocks again whenever another script becomes current
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057580..0x000576F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000577BB..0x000579A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057B1B..0x00057B80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:0022
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0299..172C:0388
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0388..172C:03CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:31ED..172C:325F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, immediate_search.py and segment_displacement_writes.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). Addresses without a
segment are in `4C13`. "Clear(p, n)" is a far call of `1000:3FA2` with the far pointer p, the
fill byte 0 and the count n, as in FND-PARTY-241. Overlay 169's allocator `+0552` returns a far
pointer, or 0 and sets the byte `4C0C:0000` (FND-PARTY-241).

**Globals.** Overlay 169 `+0000` (its only direct call is at overlay 188 `+0481`,
FND-PARTY-040) sets the word `4C0E:000B` to -1 and allocates, in this order (`+0096..+0100`):

| Far pointer | Bytes |
|---|---|
| `034B` | `0x65` (101) |
| `0347` | `0x190` (400) |
| `0343` | `0x50` (80) |
| `033F` | `0x1A4` (420) |

When the failure byte is clear, it stores 1 in `4C0E:000B` and its second argument at `0313`,
and calls `+059B`, which clears each of the four blocks with its full allocated size (`+059B..+05FF`).
Then it near-calls `+023B` (`+012D..+0131`), stores that routine's result in `4C0E:000B`, and
far-calls `172C:000C` with 99, 0 and 2 (`+013A..+0142`).

**Locals.** `+023B` returns at once unless `4C0E:000B` is 1. It allocates 300 bytes for `0251`
and then, in this order (`+0281..+02DC`):

| Far pointer | Bytes |
|---|---|
| `033B` | 9 |
| `0337` | `0x40` (64) |
| `0333` | `0x28` (40) |
| `032F` | `0x1A4` (420) |

and 32 bytes for `0249`. When the failure byte is clear, it clears the four blocks with their
full sizes (`+0304..+0365`), allocates `0255`, sets the words `4C0E:0012` and `4C0E:0014` to 0
and 16 slots of the script cache (FND-SCRIPT-019), stores 1 in the byte `4C0E:0000` (`+0404`)
and 2 in `4C0E:000B` (`+040F`), and returns 2. A failure returns -1. Its only direct call is
the one at `+012E`. The release routines `+0174` and `+0422` free the same blocks and set their
pointers to 0; `+0174` sets `4C0E:000B` to 0 at the end.

**Who sets the eight pointers.** The operand search for the displacements `0x32F` to `0x33D`
finds stores only in overlay 169's allocation and release above; the other hits are reads: the
variable code in `172C` (FND-SCRIPT-009, FND-SCRIPT-010), the clear `172C:31ED` below and the
save table copy below. The stores of `+023B` and `+0422`, read in the listing first, are the
positive control. FND-PARTY-241 gives the same result for `0x347` and `0x349`.

**The local clear.** `172C:31ED` does nothing unless the byte `4C0E:0000` is 1; then it clears
`033B` with 9 bytes, `0337` with `0x40`, `0333` with `0x28` and `032F` with `0x1A4`, the full
sizes `+023B` allocated, and returns. It does not change the byte. Its direct calls are:

- `172C:001F`, at the start of `172C:000C` when the word `4C0E:000B` is 2;
- `172C:03C5`, in `172C:0388(number, kind)`, which returns 0 when the byte `0326` is 1, returns 1
  at once when `number` and `kind` equal the words `4C0E:0012` and `4C0E:0014`, and otherwise
  calls `172C:31ED` before it looks the script up in the cache (`+043F`) or loads it (`+04CF`),
  and on success stores `number` and `kind` in those two words.

`172C:0388` is called by `172C:0299` (the call of another script, FND-SCRIPT-009 opcode `0x14`,
and the start of a run from `172C:000C`) with the called script's number and kind, and by
`172C:031F` (the return, opcode `0x19`) with the number and kind of the script it returns to.

**The byte `4C0E:0000`.** `segment_displacement_writes.py` with segment `4C0E` and displacement
0 lists two writes, both of 1: `172C:25B3` in the variable setter after a write of kind 2, 5 or
`0x0E` (FND-SCRIPT-009), which is the positive control, and overlay 169 `+0404`. No
instruction of the forms the tool lists stores another value there.

**The local backup.** The other reads of the four local pointers are in overlay 204: `+1F45`
far-calls `1000:3F5A` four times to copy `033B` (9 bytes), `0337` (`0x40`), `0333` (`0x28`) and
`032F` (`0x1A4`) to `4E71:0244`, `4E71:0204`, `4E71:01DC` and `4E71:0038`, and `+1FB7` copies
the same four ranges back. Their only direct calls are at `+0521` and `+0527`, the targets of
requests 29 and 30 in the table at overlay 204 `+06C7` that the request dispatcher `+0169`
indexes with the request less 1 after rejecting requests outside 1 to 53 (`+017B..+0189`,
FND-RNG-027). Those four ranges hold zero bytes in the load image. In the decode of
FND-SCRIPT-024, 192 instructions `0x22` have the literal first parameter 29 and 190 have 30;
in `GPL/8` at `0x02F3` a 29 is followed by an `0x14` call and then a 30.

**The save table.** Overlay 187 `+0D1A` copies the far pointers `034B`, `0347`, `0343`,
`033F`, `033B`, `0337`, `0333` and `032F` into entries 29 to 36 of the table at `DS:157E`
(`+0D4D..+0DB9`); the sizes at `4F38:0000` for those entries are `0x65`, `0x190`, `0x50`,
`0x1A4`, 9, `0x40`, `0x28` and `0x1A4`. The save loop writes entries 17 and above only for
selector 0 (FND-COMBAT-034), and the load clears each block before reading its chunk back
(FND-PARTY-241). None of the 61 shipped table entries points into segment `4C0E`.

## Interpretation

With the element sizes of FND-SCRIPT-009 and FND-SCRIPT-010, there are 808 global flags, 200
global numbers, 20 global big numbers, 10 global strings of 42 bytes, 72 local flags, 32 local
numbers, 10 local big numbers and 10 local strings of 42 bytes. When overlay 169 sets up the
script state, before `MAS` 99, the first script, runs, every one of them is 0 and every string
is empty. After that, because the byte `4C0E:0000` is 1 from the end of the set-up on, the four
local blocks are cleared whenever the interpreter makes another script current: on each call of
a different script and on each return to a different script. Local variables therefore last
only while the same script runs, unless the script copies them out with request 29 before a
call and back with request 30 after it, which the shipped scripts do around calls. The global blocks are not cleared by the interpreter; they are
cleared by `277B:035D..277B:03C1` (FND-PARTY-241) and replaced by a selector-0 saved game.

## Alternatives

- A store to `4C0E:0000` through a base or index register, a string instruction or a block copy,
  or with the `ES` load more than 12 bytes before it, is not listed by the tool. No save table
  entry points into the segment, so a saved-game load does not reset it.
- Which saves use selector 0, and so whether a saved game the player makes holds the local
  blocks, follows FND-COMBAT-034 and was not read here.
- Other writes to the backup ranges in segment `4E71`, and where the `GPL/8` call at `0x02FC`
  goes, were not searched for; the backup changes only what request 30 restores.
- The kind and number values 99, 0 and 2 that `+0142` passes are read as `MAS` 99 after
  FND-PARTY-040; this finding does not depend on which script it is.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 169 57580 57B94`, `187 70B4A 70BFD`, `204 8BF21 8BF4E`,
`204 8C2DF 8C2EF` and `204 8DD05 8DDE9`, and read the 53 words at file `0x8C487` (overlay 204
`+06C7`) and the bytes at `4E71:0038..4E71:024D` (file `0x43948`);
`resident_listing.py <dsun> 172C:000C..172C:00A1 172C:0299..172C:0388 172C:0388..172C:043F
172C:31ED..172C:325F 172C:2580..172C:25BC 277B:0330..277B:03C1`; `direct_callers.py <dsun>
169+0000 169+0174 169+023B 169+0422 169+059B 172C:31ED 172C:0388`; `immediate_search.py <dsun>
32f 331 333 335 337 339 33b 33d`; `segment_displacement_writes.py <dsun> 4C0E 0`; and read the
61 double words at file `0x44580` (sizes) and `0x4E57E` (shipped table). The listings are
retained under `GAME_DIR/analysis/script-variables-20261018/`. Count the lines of the
`script_decode.py GPLDATA.GFF OBJEX.GFF --all` listing that hold `22 29, ` or `22 30, ` as an
instruction at the top level or nested. No original function or game process was run.
