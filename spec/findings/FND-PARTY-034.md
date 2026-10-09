---
id: FND-PARTY-034
title: The party loader opens CHARSAVE.GFF from the program's own directory before each character load, and a failed open prints a message and ends the program
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FC72..0x0006FCAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068EE0..0x00068F07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068850..0x000688A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000673F1..0x00067430
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A83..0x00067AC5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4448:0002..4448:002C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277D:025D..277D:0265
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations and FBOV fixups applied (tools/research/exec-census/immediate_search.py, overlay_listing.py, trampoline_target.py, resident_listing.py, gff_tag_numbers.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with `DS` the data segment `57E0`:

- **References to the name.** Decoding every byte of the load image and of each overlay's code,
  the only instructions with `0x1388` or `0x1395` as an immediate or displacement are `push 0x1388`
  at `0x0006FC87` and `push 0x1395` at `0x0006FC9A`, both in overlay 186, and three that compare
  or store the number 5,000 (`2684:0001`, `2684:0002` and overlay 194 at `0x000812FF` to
  `0x00081301`). `DS:1388` holds `CHARSAVE.GFF` and `DS:1395` the message that it was not found
  (FND-PARTY-008).
- **The open routine** at `0x0006FC72` (overlay 186, code offset `0x0572`, trampoline
  `56E9:0052`). When the double word at `DS:144A` is not 0 it returns 0 in `AL`. Otherwise it
  calls `56BD:002F` with the near name `DS:1388`, the far address `DS:144A`, and the words
  `0x000A` and `0x09C4`. When that returns 0 in `AL`, it calls `56B2:0039` with `DS:1395` and
  the far pointer `DS:44F2`. It then returns 1 in `AL`.
- **The archive-open wrapper** `56BD:002F` (overlay 182 at `0x00068850`, FND-CONFIG-039) copies
  the string at `DS:44F2` into a local buffer through `1000:406D`, appends the name
  through `2D40:3DC2` with the limit `0x50`, calls `38FF:0066` with that buffer and the other
  arguments, and returns 0 in `AL` when that returns `0xFFFF`, 1 otherwise.
- **`DS:44F2`** is empty in the file (FND-SAVE-006). Overlay 180's startup routine, entered
  through `56B2:0020`, copies the far string at `DS:[bp+6]` into `DS:44F2` through `1000:406D`,
  finds the last `\` in it through `1000:381E`, or else the last `:`, and stores a NUL after it,
  or at its first byte when it has neither. Its only caller is the far call at `277D:025F`, in
  the routine at `277D:0004`, which pushes the word at `[di]` with `di` loaded from `[bp+8]`,
  that routine's second argument; its first, `[bp+6]`, is compared with 1 at `277D:0014`. No
  other instruction names `0x44F2` except `mov si, 0x44F2` at `0x0006742A` in the same routine
  and pushes of it as an argument.
- **The message entry** `56B2:0039` (trampoline 5 of overlay 180, code at `0x00067A83`) tests the
  word at `4E71:0001`, calls `1BF3:2973` with 3 (twice when that word is not `0xFFFF`, setting it
  to `0xFFFF` in between), clears the byte at `DS:1462`, and calls `4448:0002` with its first
  argument and the far pointer after it. `4448:0002` calls `1000:3DB6` with the format and the
  address of the arguments after it, then `1000:03DF` with 1. FND-CONFIG-062 reads
  `1000:03DF` as reaching the run time's cleanup and the DOS terminate-process request.
- **The callers.** The open routine's trampoline `56E9:0052` is the target of nine far calls in
  overlay code. One is at `0x00068EE0` in the party loader of FND-PARTY-013, the instruction
  before the slot's arguments to `2D40:000A` (the call at `0x00068F02`, FND-PARTY-023). The loader
  does not test the open routine's result. Overlay 186's routine at `0x0006FCAD` (trampoline
  `56E9:0057`) closes the archive at `DS:144A` through `38FF:02B5` and clears it; it is the
  target of nine far calls, one at `0x00068FF7` in the same loader.
- **The records.** Of the 26 installed `.GFF` archives, only `CHARSAVE.GFF` has a `CHAR` table,
  with resources 29 to 43 and 50 to 53.

## Interpretation

`DS:44F2` holds the directory part of the routine `277D:0004`'s `argv[0]`: that routine is the
program's main routine, with the argument count and list as its arguments, and the C run time
passes the program's path as the first argument. The game therefore opens `CHARSAVE.GFF`, like
`RESOURCE.GFF` (FND-CONFIG-039), from the directory `DSUN.EXE` was started from, not from a
search path or the CD drive. Under GOG's launch configuration, which starts the game from `C:`
(FND-EXE-010), that is the installed copy, or a copy GOG's overlay of `cloud_saves` on `C:` puts
in its place.

On START GAME, the party loader opens the archive before loading the first slot's character. When
the open fails, the open routine prints the not-found message with the directory and ends the
program with status 1, so the loader never runs `2D40:000A`. When the archive is open but a
record is missing, the lookup searches every open archive (FND-CONFIG-151), finds no `CHAR`
table outside `CHARSAVE.GFF`, and fails, so `2D40:000A` returns `0xFFFF` for that slot.

## Alternatives

- `CHARSAVE.GFF` is read from the disc: ruled out for the lookup of the characters; the only open
  of the name prefixes the program's directory. Which copy `cloud_saves` holds, if any, depends
  on what earlier sessions wrote.
- A failed open is reported and play continues: ruled out unless `1000:03DF`'s terminate request
  returns; the operating system's handling of it was not observed (FND-CONFIG-062).
- `argv[0]` holds the full path of the program only from DOS 3 on and as the C run time builds
  it; that construction was not read here. With an `argv[0]` that has no `\` or `:`,
  `DS:44F2` is empty and the name is opened in the current directory.
- The other eight callers of each overlay 186 routine were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 1388
1395` and `immediate_search.py <dsun> 44F2`; `overlay_listing.py <dsun> 186 0x6FC40 0x6FCF0`,
`overlay_listing.py <dsun> 182 0x68850 0x688A6`, `overlay_listing.py <dsun> 182 0x68EC0
0x68F20`, `overlay_listing.py <dsun> 180 0x673D0 0x67460` and `overlay_listing.py <dsun> 180
0x67A47 0x67AC5`; `trampoline_target.py <dsun> 56B2:0020 56B2:0034 56B2:0039 56BD:002F 56E9:0052
56E9:0057`; `resident_listing.py <dsun> 4448:0002..4448:0048 277D:0004..277D:0040
277D:0240..277D:0275`; and `gff_tag_numbers.py <install dir> CHAR`. The far calls to the
trampolines are the five-byte sequences `9A`, offset, segment word: `0x05D0` in overlay code and
`0x46E9` in the load image for overlay 186's, `0x05A0` and `0x46B2` for overlay 180's. Each of the
18 overlay-code matches for `56E9:0052` and `56E9:0057` has its segment word in its overlay's
fixup list; the load image has no match for them, and one, at `277D:025F`, for `56B2:0020`.
