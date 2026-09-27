---
id: FND-CONFIG-021
title: Sound initialization passes the first configuration block and tests more flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:00B0
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident-image ranges
environment: null
---

## Observation

The initialization routine called after `SOUND.CFG` is loaded (FND-CONFIG-019,
FND-CONFIG-020) continues to use the buffer at `DS:3411`:

| File offset | Direct operation |
|---|---|
| `0x0003C7AB` | Compares word `0x08` with `0x69`; writes `0x5A` to a runtime byte on equality and `0x64` otherwise. |
| `0x0003C88E` | Pushes words `0x06`, `0x04`, `0x02`, `0x00`, then a library word, to a far routine. A zero return takes an error branch. |
| `0x0003C8B6` | Pushes the same five words to another far routine. |
| `0x0003C8E6` | Tests bit `0x01` of word `0x14`. When set, calls another far routine; a nonzero returned byte takes an error branch. |
| `0x0003CA29`, `0x0003CA88` | Tests word `0x32` for 1 or 2 before string and file operations. The second branch also requires nonzero `DS:3444`. |

The first four words are passed in their on-disk order as a group, while
the library's preceding bounded path uses word `0x0A` as a port base for
two values of word `0x08` (FND-CONFIG-020).

## Interpretation

The two ten-byte blocks are used differently during initialization: the
first block's four settings are passed as one group to two calls, and the
second block's first word can select I/O ports. This does not yet identify
which block belongs to music or digital sound. The value at `0x32` affects
later string and file calls, as well as the music-level cap already
recorded in FND-CONFIG-012.

## Alternatives

The two receiving far routines, the runtime byte changed for ID `0x69`,
the bit-`0x01` callback, and the string/file operations controlled by
`0x32` were not read to completion. Their exact effects and the remaining
`SOUND.CFG` consumers stay open. The tail at `0x34..0x3A` is not accessed
in these bounded windows; that is not evidence that it is unused elsewhere.

## How to reproduce

Disassemble only the installed `DSUN.EXE` resident windows
`0x0003C7AB..0x0003C7D3`, `0x0003C87D..0x0003C906`, and
`0x0003CA29..0x0003CABA` in 16-bit mode. At each access, follow the
load of the buffer pointer from `DS:3411` and the immediate branch.
