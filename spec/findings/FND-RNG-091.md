---
id: FND-RNG-091
title: Of the file reads FND-RNG-090 leaves open, only the SAVE load writes 4C13:0369, restoring the saved word, and none writes DS:426D; the others read into heap blocks, frame locals or fixed buffers that miss both words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:0215
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4544:0000..4544:016B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:032A..4611:03A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:0140
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:031B..4AE5:0421
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0009..1425:0044
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2F03..1000:2F13
tool: tools/research/exec-census/direct_callers.py, resident_listing.py, overlay_listing.py, immediate_search.py, segment_offset_refs.py, far_pointer_words.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. The two words are `DS:426D`
(linear `0x5C06D`) and `4C13:0369` (linear `0x4C499`, the last word of segment `4C13`). Call
sites are the file offsets of FND-RNG-090's table.

**`44DE:01AF`** reads the high word of its count as pairs of `0x8000`-byte reads, adding `0x800`
to the buffer segment after each, then the low word in one read; it only moves forward.

**The eight calls through stored far pointers:**

| Call | Buffer |
|---|---|
| `37FC:085A` (`0x2DA1A`) | the block `444C:0008` allocated at `37FC:0800`, plus `0x31` |
| `38FF:021B` (`0x2E40B`) | the block allocated at `38FF:0130`, plus `0x31`; flag 4, which would use the caller's buffer, is passed by none of the caller sites `direct_callers.py` finds (overlay 192 `+0052`, overlay 182 `+003D` with the flags of its seven callers: `0xA`, 0, 0, 0, 0, `0xA`, 2) |
| `38FF:09A2` (`0x2EB92`) in `38FF:07B1` | the caller's pointer variable, or, when it is null, a block `444C:0008` allocates and stores there (`38FF:0906..093D`); `38FF:0438` passes a local set to 0, `38FF:04AB` its caller's variable, and `47E5:04B8` has no caller found |
| `4544:00F3` (`0x3A733`) | the frame local `[bp-0x8A]`: all five callers of `4544:0000` push a null buffer |
| `4611:034C`, `4611:037E` (`0x3B65C`, `0x3B68E`) | `DS:342C` and `DS:3430`, set at `4611:0028..004B` to the block `DS:3420` and its second half; `DS:3420` holds 0, `4AB9:001C`'s result or a release's 0 |
| `47B9:01C8` (`0x3CF58`) | the block `4AB9:001C` allocated at `47B9:0186`, plus 2 |
| overlay 196 `+014B` (`0x830BB`) | its one caller, `+0288`, passes `DS:6299`, for `0x80` bytes |

**The overlay manager.** `4AE5:0293` reads `0x14` and `0x10` bytes into the frame of
`4AE5:0010`. `4AE5:040C` reads into the overlay buffer between `[0x124]` and `[0x126]` of its
segment, which FND-EXE-566 and FND-EXE-568 place in a block of `1000:15A5`.

**`15F3:0183`.** Its routine `15F3:0170` has one caller, `1425:0009`, reached through the table at
`DS:3F42` (FND-EXE-584) from `1425:0733` (buffers `DS:39CC + 0x108 × i`, `i` 1 to 4, `0x100`
bytes), `1425:0C4A` (`DS:3EEC + 0xE × k`, `k` 0 to 5, or a frame local, 8 bytes) and `1425:1AA1`
(a block `444C:0008` allocated by overlay 190 `+34B3` or `+3544`, or a cache record's field
`+0xA` from `31E0:3277`). The highest of these fixed buffers ends at `DS:3F38`.

**`1000:2F0F`** is in `1000:2F03`, whose one caller, `1000:1E3C`, passes `AH` = `0x54`.

**The 110 direct calls of `38FF:04AB`**, which reads a whole resource into the caller's pointer
variable or a new block: each passes a variable that is 0 at the call, a block allocated just
before, a frame local, or a far pointer variable that is 0 in the file and is stored only from an
allocation, a release or `38FF:07B1`, except three that read into pools based at the far pointers
`4C13:0255` and `DS:95FF` (`172C:0601`, `31E0:36DB`, `31E0:3277`) and nine that read into fixed
addresses:

| Call | Destination |
|---|---|
| `2C5F:0037` | `DS:4277`, a `DATA` resource |
| overlay 188 `+0050`, `+009C` | `DS:5AF7`, `DS:60ED` (`FNFO` 1 and 2) |
| overlay 179 `+09C2` | `4DA1:0060` (`VECT` 1, 1,024 bytes) |
| overlay 186 `+02B1`, `+02E3`, `+030E` | `4D62:039C`, `4E71:09CD`, `4E71:0A55` plus a party slot's offset (`SPST`, `PSST`, `PSIN`, at most 34 bytes) |
| overlay 187 `+08CE` | `DS:67BB + 0x25 × slot` |
| overlay 187 `+0B18` | each non-null entry of the `SAVE` table (FND-SAVE-015); entry 46 is `4C13:0369` |

## Interpretation

A file read writes `4C13:0369` only when a game is loaded or the region changes, and then with the
word a save took from it. No file read writes `DS:426D`: the nearest fixed buffer starts 8 bytes
after it and reads forward. A save file edited to hold a `SAVE` resource longer than the table's
length for entry 26 (`DS:2100`) or 29 (`DS:2A6B`) would reach `DS:426D`, since the read takes the
whole resource.

## Alternatives

The pool allocators `172C:0698` and `31E0:3C02`, and the stores at `31E0:37EA` and `31E0:3E0F`
into the table `DS:95FF` uses, were not read in full; that the pools lie in heap blocks rests on
their bases. Calls of `38FF:04AB` and `38FF:0438` through stored far pointers were not searched.
The rows with a variable 0 at the call rest on a backward scan of up to `0x300` bytes before each call, which
can miss a store reached by a jump from further away. The size of the resources overlay 187
`+08CE` reads was not measured; one longer than `0x4D91` bytes for the highest slot would wrap
inside a segment.

## How to reproduce

`direct_callers.py <dsun> 44DE:01AF 15F3:0170 1000:2F03 38FF:04AB 38FF:07B1 38FF:0066 4544:0000
1425:0009 196+0122`; `resident_listing.py <dsun>` over the locations and over `37FC:077C..37FC:08FB`,
`47B9:008B..47B9:0236`, `4611:0000..4611:0110`, `1425:060B..1425:076B`, `1425:0C04..1425:0CDD`,
`1425:1954..1425:1B0B`; `overlay_listing.py <dsun> 196 83070 830D0` and `... 196 831B0 83200`;
`immediate_search.py <dsun>` for each far pointer variable named; `segment_offset_refs.py <dsun>
4C13 0255`; `gff_tag_numbers.py <install>` for `VECT`, `SPST`, `PSST` and `PSIN`; read the word at
`DS:1632`.
