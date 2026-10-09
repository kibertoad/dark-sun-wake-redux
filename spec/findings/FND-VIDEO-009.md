---
id: FND-VIDEO-009
title: The game sets BIOS mode 0x13 and unchains it for play, uses plain mode 0x13 for cinematics and mode 3 on exit, while the INT 10h wrapper at 1000:1136 serves text services
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2973..1BF3:2A4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2A4A..1BF3:2A61
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1136..1000:11BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the load image for INT 10h and near calls; Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportReferences)
environment: null
---

## Observation

The load image holds `CD 10`, the `INT 10h` instruction, eleven times: nine inside the routine at
`1000:1136` and two at `1000:E907` and `1000:E989`. `1000:E8A3` is the same address as
`1BF3:2973`, and `1000:E97A` the same as `1BF3:2A4A`.

The far routine at `1BF3:2973` takes a word `mode` and keeps it in the word at `1BF3:295F`. For
mode 7 it programs the monochrome adapter's ports `3BFh`, `3B8h` and `3B4h` and clears 16,384
words at `B000:0000`. Otherwise it sets bits 4 and 5 of the BIOS byte at `0040:0010` to 2, or 3
when the low three bits of `mode` are 7, and calls `INT 10h` with `AH` 0 and `AL` the low byte of
`mode`. When `mode` is `0x100` or more it then sets bit 5 of sequencer register 1 (`3C4h`),
clears bit 3 and sets bit 2 of sequencer register 4, clears bit 4 of graphics register 5 and bit 1
of graphics register 6 (`3CEh`), clears bit 6 of CRTC register `0x14` and sets bit 6 of CRTC
register `0x17` (`3D4h`), writes `0x0F` to the map mask, clears 32,768 words at `A000:0000`,
clears bit 5 of sequencer register 1 again and writes 1 to the map mask. The far routine at `1BF3:2A4A` returns the word at
`1BF3:295F`, or the BIOS's current mode through `INT 10h` with `AH` `0x0F` when that word is
`0xFFFF`.

Calls to `1BF3:2973` are found only in overlays. With the header of overlay 180 at segment `56B2`
and its code at file offset `0x671E0`:

| Caller | File offset | Argument |
|---|---|---|
| overlay 180, offset `0x48D` | `0x6766D` | `0x113`, after keeping the result of `1BF3:2A4A` at `4E71:0001` |
| overlay 180, offsets `0x12`, `0x879`, `0x8B5` | `0x671F2`, `0x67A59`, `0x67A95` | 3, when `4E71:0001` is not `0xFFFF`; then `4E71:0001` becomes `0xFFFF` |
| overlay 180, offsets `0x88D`, `0x8C9` | `0x67A6D`, `0x67AA9` | 3 |
| overlay 187, offset `0x27B2` | `0x725E2` | `0x113`, after a cinematic (FND-VIDEO-004) |
| overlay 196, offset `0x279` | `0x831E9` | `0x13`, before an FLI plays (FND-VIDEO-008) |

`1000:1136` loads `DS` with `0x40`. It passes any `AH` other than 0 and `0x0F` straight to
`INT 10h`. For `AH` 0 with modes 2 and 3 it checks for a VGA through `INT 10h` `AX` `0x1A00` and
sets the cursor shape; for mode `0x40` it asks for the EGA information, loads the 8x8 font and
selects the alternate print-screen routine. For `AH` `0x0F` it returns the current mode and, for
modes 2 and 3, calls `1000:1128`. A scan of segment `1000` finds 16 near calls to `1000:1136`, at
`1000:0FFD`, `1061`, `1072`, `10CA`, `112C`, `11C3`, `11CF`, `11E9`, `11FB`, `1200`, `257D`,
`2607`, `2634`, `2651`, `286C` and `2E5E`, and no far call. An earlier Ghidra reading counted 14
direct calls from seven recovered functions, among them service values `02h` and `09h` and a
`0Fh` then `00h` sequence, and did not place `1000:E907` and `1000:E989` inside a function.

## Interpretation

`1BF3:2973` is a set-mode routine of the game's graphics library, and a mode above `0xFF` asks for
the BIOS mode in its low byte with the VGA unchained into four planes afterwards. The game keeps
the starting mode at startup, switches to `0x113`, BIOS mode `0x13` unchained, and goes back to
text mode 3 on the way out. The FLI player uses mode `0x13` itself, where the 64,000 bytes at
`A000:0000` are the whole 320x200 screen, and the game switches back to `0x113` afterwards.
`1000:1136` is the C runtime's video helper for text-mode console output: its callers are
runtime code in segment `1000`, and it plays no part in the game's graphics.

## Alternatives

Which of overlay 180's routines run at startup and which on exit was read from their shape, not
from their callers. The routines the game draws with after the switch were not read, so how it
lays out the four planes is not shown here.

This replaces FND-VIDEO-003, whose first two locations ended at `1BF3:2A49` and `1BF3:2A60`, the
closing `retf` of each routine, instead of the byte after it. The documentation check found this
when the `DSUN.EXE` inventory, rebuilt from a single import that sees calls from overlay code,
placed functions at `1BF3:2973` and `1BF3:2A4A`. The ends now give the byte after each `retf`.
Its other observations are unchanged.

## How to reproduce

Search the load image for `CD 10`; disassemble `1BF3:2973` to `1BF3:2A61` and `1000:1136` to
`1000:11BE`; scan segment `1000` for `E8` calls reaching `1000:1136`; search the overlays for
`9A 73 29 58 00`, a far call to `1BF3:2973` through descriptor `0x58`.
