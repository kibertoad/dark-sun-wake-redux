---
id: FND-SCRIPT-027
title: Script instructions 0x66, 0x6C, 0x6D and 0x6E fill one-key trigger lists like 0x65, 0x69 a cell list like 0x68, 0x6A and 0x6B box lists, and 0x70 a two-key list that never replaces a record; 0x6F, 0x1B and 0x1C set an object's entry points instead
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1B30..172C:1DA7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:11D7..172C:1241
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:16AD..172C:16BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2A8F..172C:2C10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2DF8..172C:2EA1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3026..172C:3106
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:09B7..2D40:0BA4
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py and script_decode.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000. `p0`, `p1`, ... are the numbers an instruction
reads with `172C:367B`. A record is FMT-SCRIPT-004. "`MAS ` 99" stands for the test that
`4C0E:0012` is 99 and `4C0E:0014` is 2, which gives 1, otherwise 0. The list heads are words of
segment `4C10`.

| Opcode | Handler | Reads | Record built | Inserter, head |
| --- | --- | --- | --- | --- |
| `0x65` | `172C:1C9D` | 3 | `+0` p0, `+2` p1, `+4` p2, byte `+6` `MAS ` 99 (`172C:2B29`) | `172C:2EA1`, `0x11` |
| `0x66` | `172C:1CCA` | 3 | as `0x65` | `172C:2EA1`, `0x13` |
| `0x6C` | `172C:1C70` | 3 | as `0x65` | `172C:2EA1`, `0x0D` |
| `0x6D` | `172C:1CF7` | 3 | as `0x65` | `172C:2EA1`, `0x15` |
| `0x6E` | `172C:1D24` | 3 | as `0x65` | `172C:2EA1`, `0x0F` |
| `0x68` | `172C:1B88` | 5 | `+0` p2, `+2` p3, `+4` p0, `+6` p1, byte `+8` p4 (`172C:2A8F`) | `172C:2F46`, `0x05` |
| `0x69` | `172C:1BC2` | 5 | as `0x68` | `172C:2F46`, `0x07` |
| `0x6A` | `172C:1BFC` | 7 | `+0` p4, `+2` p5, `+4` p0, `+6` p1, bytes `+8` p2, `+9` p3, `+0x0A` p6 (`172C:2AD5`) | `172C:3026`, `0x09` |
| `0x6B` | `172C:1C36` | 7 | as `0x6A` | `172C:3026`, `0x0B` |
| `0x70` | `172C:1D7A` | 4 | `+0` p2, `+2` p3, `+4` p0, `+6` p1, byte `+8` `MAS ` 99 (`172C:2B89`) | `172C:2DF8`, `0x17` |

`172C:3026` walks its list as `172C:2F46` does (FND-SCRIPT-015), past each record whose word `+4`
and then word `+6` is below the new one's (unsigned), takes a new record unless both words are
equal, and copies 11 bytes. `172C:2DF8` walks the same way with signed compares, always takes a new
record, and copies 9 bytes.

`0x6F` (`172C:1D51`) builds the same 8 bytes as `0x66` and calls `2D40:09D0`, which, for each slot
below 48 that `2D40:0D94` and `2D40:2F34` find for p2, stores the record's start and number in
words `+0x08` and `+0x0C` of the slot's 19-byte record at `4F49:08A3` (FMT-SCRIPT-005) and puts the
record at the front of the list at `DS:5AF5` (`2D40:0B54`). `0x1B` (`172C:11D7`) and `0x1C`
(`172C:120C`) call `2D40:09B7(p3, bytes, 1)` and `(..., 2)`, which store words `+0x0A` and `+0x0E`,
p3 in word `+4` and 1 or 2 in the low 7 bits of byte `+0x10`. `0x47` (`172C:16AD`) reads nothing
and stores 1 in the byte `4C13:0325`.

`script_decode.py --all` over `GPLDATA.GFF` and `OBJEX.GFF` counts the shipped uses: `0x65` 261,
`0x66` 185, `0x68` 150, `0x69` 0, `0x6A` 321, `0x6B` 0, `0x6C` 11, `0x6D` 305, `0x6E` 291, `0x6F`
699, `0x70` 163, `0x47` 0. Byte `+8` of the `0x68` uses is 0 in 87 and 2 in 63; byte `+0x0A` of the
`0x6A` uses is 0 in 293, 1 in 2 and 2 in 26.

## Interpretation

Ten lists hold the triggers: one-key lists at heads `0x0D`, `0x0F`, `0x11`, `0x13` and `0x15`,
cell lists at 5 and 7, box lists at 9 and `0x0B`, and a two-key list at `0x17`. A trigger of a
one-key, cell or box list replaces the one already there with the same key or cell; every `0x70`
adds a record. `0x6F` and `0x1B`/`0x1C` attach entry points to objects instead of lists. The lists
at heads 7 and `0x0B` and the byte `4C13:0325` are not used by the shipped scripts.

## Alternatives

`172C:2EA1` and `172C:2F46` were read in FND-SCRIPT-015 and not again. What the 19-byte records'
entry points run, and when, is outside this finding (FND-SCRIPT-016, FND-SCRIPT-018).

## How to reproduce

`resident_listing.py <dsun>` over the ranges above; `script_decode.py --all` over `GPLDATA.GFF` and
`OBJEX.GFF`, counting each opcode and the fifth or seventh parameter of `0x68` and `0x6A`.
