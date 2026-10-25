---
id: FND-RNG-073
title: No direct store puts 9999 in DS:426D; its direct stores give 0 to 3, a slot from DS:0DA5 or 4C13:0369, neither of which a direct store sets to 9999
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1EDB..28C9:2146
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078C50..0x00078CAC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007939B..0x000793F6
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding, a direct-operand search over the function inventory's rows with an optional segment immediate; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static. The direct-operand search finds 15 stores to `DS:426D`:

- `28C9:0CFF` stores the constants 0, 1, 2 and 3.
- `28C9:1EDB` and `28C9:305F` each store `DS:0DA5` when the byte `+0x14` of its 0x31-byte record
  is 1, and otherwise `4C13:0369`.
- Overlay 182 stores `4C13:0369` at four sites (file offsets `0x69AE1`, `0x69FC2`, `0x6A34F` and
  `0x6A5F5`).
- Overlay 190 `+0969` stores its argument less `0x2C2D` after using that value to index the
  0x42-byte table at `DS:19C5` and the 0x31-byte table at `DS:19C9`; `+1090` stores the loop
  index `DX`, which runs from 0 below 4; `+10AB` stores `4C13:0369`.

`DS:0DA5` is stored only at `28C9:1EDB` (`4C13:0369` at `28C9:213D`, and `0xFFFF`) and at
`28C9:305F` (`0xFFFF`). `4C13:0369` is stored directly only at `2D40:3DB4`, with a value below
9999 (FND-RNG-071), and at overlay 188 `+0DFA` (file offset `0x73C9A`), in the initializer `+0D26` that
FND-PARTY-119 reads, with 0. The file holds 0 at
`DS:426D` and at `4C13:0369`, and `0xFFFF` at `DS:0DA5`.

`28C9:1EDB` leaves at once when `DS:426D` is 9999 or more and combat is on; with combat off it
goes on and indexes record `DS:426D` of `DS:67BB` and the entry `4F49:0C33 + 3 × DS:426D`.

## Interpretation

9999 reaches `DS:426D` only through overlay 173 `+059A`'s pointer (FND-RNG-070), through
`31E0:2D6E`'s (FND-RNG-072), or through `4C13:0369` written by something other than a direct
store, such as a block copy or a file read. `28C9:1EDB` is a second routine that uses 9999 as a
record when combat is off.

## Alternatives

Stores whose address is computed, block copies and file reads into the data segment or segment
`4C13` are outside the search.

## How to reproduce

Search the inventory rows for operands `[0x426d]`, `[0xda5]` and, with segment word `0x3C13` or
`0x02E8`, `[0x369]`; list overlay 190 from `0x78C50` to `0x78CAC` and from `0x7939B` to `0x793F6`;
disassemble `28C9:1EDB` to `28C9:2146`; read the three words in the file.
