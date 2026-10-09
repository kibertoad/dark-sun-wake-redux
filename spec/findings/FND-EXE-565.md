---
id: FND-EXE-565
title: The disc DSUN.EXE's overlay manager is the installed one's code one byte lower in segment 4AD6
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:1258
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:000F..4AD6:1257
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_manager_editions.py)
environment: null
---

## Observation

The installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`) and
the disc's (634,704 bytes, `318cd5ec0559901add3780097162a919`) were compared over the
`0x1293` bytes from installed file offset `0x40050` (`4AE5:0000`) and disc file offset
`0x3FF5F`, `0xF1` bytes earlier. That disc byte is `4AD5:000F`, so a manager offset X in
segment `4AE5` is offset X - 1 in segment `4AD6`.

Both ranges have MZ relocations at the same 29 offsets. 21 of those words differ: the disc's
segment is lower by `0x0F` for the `55xx` segments (`55CE` becomes `55BF`, `55E8` becomes
`55D9`) and by 9 for `57E0` (`57D7`). Every other differing byte is in one of these:

- an instruction whose operand is a manager code offset or a `CS:` data word, one lower on the
  disc: `cs:[5]`, `cs:[7]` and `cs:[0x0E]` become `cs:[4]`, `cs:[6]` and `cs:[0x0D]`;
  `0x04C6`, `0x0A4B`, `0x0BFE`, `0x0D11`, `0x0D82`, `0x0EA2`, `0x1155` and the 9 in
  `mov di, 9` at `+0x0212` each lose 1;
- the `57E0` words `0x356C`, `0x356E` and `0x3570`, which are `0x34E0`, `0x34E2` and `0x34E4`
  on the disc;
- byte `0x0002`, before the startup routine at `0x0010`, and bytes from `0x1266` to `0x1292`,
  past `0x1257`, the highest code offset the manager's data words name.

In the disc's manager data segment `55BF`, the handler pointer at `+0x0002` is `4AD6:04F3`, the
words at `+0x0080` to `+0x0084` are `1255`, `1256` and `1256`, the far pointer at `+0x0086` is
`1000:02FC`, `+0x0110` holds `CD 3F`, and the 229 records from `+0x01A0` are the segment table
at `55D9:0000` (FMT-EXE-001), with 79 flags 0, 87 flags 1, 49 flags 3 and 14 flags 4.

## Interpretation

The disc's manager is the same code as the installed one, placed one byte lower in its segment
and linked against data that sits at different addresses. FND-EXE-560 to FND-EXE-564 describe
it too, with each manager offset one lower, segment `4AD6` for `4AE5`, `55BF` for `55CE`,
`55D9` for `55E8` and the payload-size words at `57D7:34E0`.

## Alternatives

The comparison is byte by byte over the manager's range. The disc's overlay 180, and whether
its code calls the cache setups as the installed one does (FND-EXE-564), were not compared.
The bytes before `0x0010` and from `0x1258` on belong to neighbouring code and data and were not
interpreted.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_manager_editions.py <install dir>/DSUN.EXE
<local copy of the disc's DSUN.EXE>` from the commit that adds this finding, with the locked
evidence Python. It checks both files, lists the differing relocated words and the instructions
holding each other differing byte, and prints both managers' data words and segment-table flag
counts.
