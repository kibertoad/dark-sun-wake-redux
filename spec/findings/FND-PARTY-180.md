---
id: FND-PARTY-180
title: 28C9:2A81 with a nonzero argument clears the word at DS:0D9E, sets the bytes at DS:2FB8 and DS:33BA to 1 and calls 4328:0B4C
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2A81..28C9:2ABD
tool: Capstone 5.0.9 16-bit disassembly of the load image based at segment 1000
environment: null
---

## Observation

Addresses are those of the repository's load image, based at segment `1000`; `28C9:2A81` is file
offset `0x20911`. Far call segment words in the file are 0x1000 below the segment they name.

`28C9:2A81` takes a byte argument `[bp + 6]`. When it is not 0 it stores 0 to the word at
`DS:0D9E` (`28C9:2A84..28C9:2A8A`). Then, when that word is above 0, it decrements it
(`28C9:2A90..28C9:2A97`). When the word is 0 after that, it calls `28C9:2AAE` (`push cs; call`)
and `4328:0B4C` (`28C9:2A9B..28C9:2AA7`). It returns at `28C9:2AAD`.

`28C9:2AAE` stores 1 to the bytes at `DS:2FB8` and `DS:33BA` and returns
(`28C9:2AAE..28C9:2ABD`). FND-CONFIG-171 reads `4328:0B4C`: it goes on only when the words at
`DS:2FB8` and `DS:33BA` are not 0 and the words at `DS:33B6` and `DS:33B8` differ, and then
calls `409B:1675`, a `3D72` routine chosen by `DS:3330`, `1BF3:5814` with `0xFFFF`, `4400:000E`
when `DS:A2B2` is not 0, clears `DS:2FB8`, and calls another `3D72` routine.

## Interpretation

With 1, as overlay 182 `+1D3E` passes it after a key (FND-PARTY-179), `28C9:2A81` runs
`4328:0B4C` at once, with the two flags it tests set; with 0 it runs it only once a count at
`DS:0D9E` runs down. So a leader change by key runs `4328:0B4C` right away, after the view
redraw `2C5F:0139` (FND-PARTY-130), whenever `DS:33B6` and `DS:33B8` differ. The high bytes of
the words at `DS:2FB8` and `DS:33BA` are not written here.

## Alternatives

What `4328:0B4C`'s callees change, and what `DS:33B6`, `DS:33B8` and `DS:0D9E` hold, were not
read; FND-CONFIG-171 does not name an effect either.

## How to reproduce

From the commit that adds this finding, disassemble the installed `DSUN.EXE` from file offset
`0x20911` to `0x2094D` as 16-bit code with origin `2A81`.
