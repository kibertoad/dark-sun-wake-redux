---
id: FND-CONFIG-024
title: ADV selector pairs each resource number with one settings block
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49EC:015F
tool: Ghidra 12.1.3 ReportReferences.java and ReportInstructionContext.java, corroborated by Capstone 5.0.7 16-bit disassembly with Python 3.14.7
environment: null
---

## Observation

The routine at `47B9:0334` (`DSUN.EXE+0x0003D0C4`) copies its word
argument to the local selector used to choose `SOUND.CFG` offset `0x38`
for zero or `0x36` for one before requesting an `ADV ` resource
(FND-CONFIG-023). Two recognized direct resident callers supply literal
values:

| Caller file offset | Argument | Nearby use |
|---|---|---|
| `0x0003C7DC` | 0 | In sound initialization, guarded by nonzero `DS:3444`; its returned far pointer is kept at `DS:340D`. The path then passes `SOUND.CFG` words `0x00`, `0x02`, `0x04`, `0x06` to two far routines (FND-CONFIG-021). |
| `0x0003F22D` | 1 | Its returned far pointer is kept at `DS:3409`. This routine passes words `0x0A`, `0x0C`, `0x0E`, `0x10` to those same two far routines. |

The second caller's routine begins at `DSUN.EXE+0x0003F21F`. A recognized
direct call to it occurs at `0x0003C8F2`, reached only when bit `0x01` of
`SOUND.CFG` word `0x14` is set. A nonzero byte returned by that call takes
an error branch (FND-CONFIG-021).

## Interpretation

Selector zero pairs the `0x38` `ADV ` number with the first ten-byte
settings block; selector one pairs the `0x36` number with the second block.
Bit `0x01` of the `0x14` word gates the second pair's setup in this caller.

## Alternatives

The device identity of each pair, the meaning of `DS:3444`, and the two
receiving far routines' effects remain unread. The installed values match
music and digital driver chunk numbers (FND-CONFIG-003), but that match
does not alone establish a device label for either selector. Recognized
direct references do not exclude indirect or overlay callers.

## How to reproduce

In the mapped image of the approved `DSUN.EXE`, query references to
`47B9:0334` and `49EC:015F` with `ReportReferences.java`, then inspect
their two caller contexts with `ReportInstructionContext.java`. In the
physical file, disassemble bounded windows `0x0003D0C4..0x0003D140`,
`0x0003C7D3..0x0003C81B`, `0x0003C8E6..0x0003C906`, and
`0x0003F21F..0x0003F2BE` in 16-bit mode. Follow the word argument and
the four field pushes on each path.
