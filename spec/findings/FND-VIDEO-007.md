---
id: FND-VIDEO-007
title: Entering regions 0x3E, 0x42, 0x43 and 0x44 copies cinematics 2, 4, 5 and 3 from the disc to the installation ahead of their use
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0110..56EF:0114
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:00E8..56BD:00EC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1078..57E0:1090
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:185B..57E0:18A2
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE; Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportDataBytes, ReportReferences)
environment: null
---

## Observation

`DS` is the data segment `57E0`. The strings ending in `.FLI` in `DSUN.EXE` are:

| File offset | Address | String |
|---|---|---|
| `0x44680` | `4F48:0000` | `1.FLI` |
| `0x4E078` | `DS:1078` | `%s%u.FLI` |
| `0x4E081` | `DS:1081` | `%c:\CINE\%u.FLI` |
| `0x4E85B` | `DS:185B` | `1.FLI` |
| `0x4E861` | `DS:1861` | `2.FLI` |
| `0x4E867` | `DS:1867` | `%c:\CINE\5.FLI` |
| `0x4E876` | `DS:1876` | `%c:\CINE\3.FLI` |
| `0x4E885` | `DS:1885` | `%c:\CINE\4.FLI` |
| `0x4E894` | `DS:1894` | `%c:\CINE\2.FLI` |
| `0x4E8E9` | `DS:18E9` | `%c:\CINE\%d.FLI` |
| `0x4E8F9` | `DS:18F9` | `%s%d.FLI` |

An earlier Ghidra search found the numbered names and the four drive templates and no direct
reference to them; the addresses are pushed as immediate words, which it did not count.

Overlay 187 (header segment `56EF`, code from file offset `0x6FE30`) has its entry `56EF:0110` at
offset `0x1E63`, `DSUN.EXE+0x00071C93`, a routine that takes a word `region`. It is called from
offset `0x12C1` of the same overlay, inside the routine that stores `current_region` at offset
`0x1216` (FND-SOUND-012), with that routine's first argument. In order it:

1. calls offset `0x23E3` (FND-SOUND-006) when the bytes at `DS:55BC` and `DS:14E3` are not 0;
2. calls offset `0x2424` with `region` and offset `0x206E` with the double word it returns;
3. returns when `DS:14E3` is 0;
4. calls `44DE:0569` with a local buffer;
5. for region `0x43`, when bit 1 of `DS:55BC` is set, deletes the installation directory's
   `1.FLI` and `2.FLI` through `44DE:02A9`; then builds the disc path from `DS:1867` and the
   installed path from the directory and `DS:1870`, the `5.FLI` at the end of that string;
6. for region `0x44`, from `DS:1876` and `DS:187F` (`3.FLI`); for `0x42`, from `DS:1885` and
   `DS:188E` (`4.FLI`); for `0x3E`, from `DS:1894` and `DS:1861` (`2.FLI`);
7. opens the installed path through `44DE:0086`. When it opens, it closes it. When it does not,
   and offset `0x2802` (FND-VIDEO-004) returns 1 for the disc path, it calls offset `0x21EA` with
   the disc and installed paths.

For any other region it opens nothing: with its handle still -1 it goes straight to the call of
offset `0x2802`, and possibly `0x21EA`, with path buffers it has not filled.

Overlay 182 (header segment `56BD`, code from file offset `0x68850`) has its entry `56BD:00E8` at
offset `0x236F`, `DSUN.EXE+0x0006ABBF`, called from offset `0x117C` of the same overlay after a
call to `56EF:00F7`. When the bytes at `DS:13F7`, `DS:13F6` and `DS:14E3` are all not 0 it goes
through `n` from 1 to 5: it formats `%s%u.FLI` with the installation directory and `n`, and
`%c:\CINE\%u.FLI` with the disc drive letter and `n`, and for each path that offset `0x56` of the
overlay finds, it stores the file's length through `44DE:0086` and `44DE:0127` in the double word
at `4E28:0005 + 4 * n` for the installed path and `4E28:001D + 4 * n` for the disc path, or 0
when the file is not found.

## Interpretation

The game copies the later cinematics to the hard disk when the party enters the region that
leads up to them, so that playing them does not wait on the disc: region `0x3E` fetches
cinematic 2, `0x42` cinematic 4, `0x44` cinematic 3 and `0x43` cinematic 5, and install types with
bit 1 set drop cinematics 1 and 2 when region `0x43` is entered. Overlay 182 records the length of
each installed and disc copy; offset `0x2BAC` of overlay 187 clears the installed length after a
delete (FND-VIDEO-004). The routine reads other regions' paths from unset buffers, which in the
original most likely fails the open of the disc path.

## Alternatives

Offsets `0x2424`, `0x206E` and `0x21EA` of overlay 187, `44DE:0569`, and the routine of overlay
182 around offset `0x117C` were not read. That offset `0x21EA` copies a file and that the lengths
at `4E28` are read anywhere else is inferred; no reader of them was searched for. What the
unfilled buffers hold, and so what the call for other regions does, was not read.

## How to reproduce

Search `DSUN.EXE` for strings ending in `.FLI` and for pushes of their addresses; disassemble
overlay 187 from file offset `0x71C93` to `0x71E9E` and overlay 182 from `0x6ABBF` to `0x6AD08`,
resolving far calls through the segment table at `0x4B080`; scan both overlays for near calls to
offsets `0x1E63` and `0x236F`.
