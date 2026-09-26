---
id: FND-SOUND-008
title: The speech routine plays INTR files from the disc below 50 and SPCH files from the installation or the disc from 50 up
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0AFA..2C5F:0C8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:00F2..56EF:00FC
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080
environment: null
---

## Observation

`DS` is the data segment `57E0`. The far routine at `2C5F:0AFA` takes a word, `n`, and returns a
word, 1 unless noted. In order it:

1. returns 0 when any of the bytes at `DS:13F7`, `DS:14E3`, `DS:14E4` and `DS:1439` is 0, or when
   the word at offset 8 of the record at the far pointer `DS:3411` is `0x71` (113);
2. calls `56EF:00F7`, the entry of overlay 187 at offset `0x2B6C`;
3. when `n` is below 50, formats `%c:\INTR\INTR%u.VOC` (`DS:0F83`) with the letter `'A'` plus the
   byte at `4E71:0033`, and `n`, into the buffer at `DS:43FB`;
4. otherwise formats `%sSPCH%u.VOC` (`DS:0F97`) with the text at `DS:44F2` and `n`; when
   `56BD:0034` returns 0 for that name, formats `%c:\SPEECH\SPCH%u.VOC` (`DS:0FA4`) with the drive
   letter and `n` instead, and sets the byte at `DS:4275` to 1;
5. returns 0 when `56BD:0034` returns 0 for the name it has;
6. calls `49E9:0142` when `49E9:00FD` returns a non-zero byte, and, when `DS:14E3` is not 0,
   `4611:03A5` when `4611:0051` returns a non-zero byte;
7. calls `4611:0177` with the name, and will return 0 when that returns 0;
8. when the word at `DS:0D9C` is not 0 and the 32-bit value at `DS:6554` is 0:
   - while `DS:14E3` is not 0, calls `4611:0051` until it returns 0;
   - when the byte at `DS:14E8` is not 0, formats `%c:\RESOURCE.GFF` (`DS:0FBA`) with the drive
     letter, opens it through `44DE:0086` with mode 1, and closes it through `44DE:003A` when the
     handle is not -1;
   - calls `56EF:00F2`, the entry of overlay 187 at offset `0x2B91`, with 3 when the word
     `combat_state` at `4C10:0019` is not 0 and 2 when it is.

The overlay 187 routine at offset `0x2B6C`, `DSUN.EXE+0x0007299C`, returns 0 when `DS:13F7` is
0; otherwise it calls `4A32:0185` and then `2834:0001` with 0, and returns 1. The routine at
offset `0x2B91`, `DSUN.EXE+0x000729C1`, sets the byte at `DS:4263` to 1, calls `2834:0001` with its argument, sets `DS:4275` to 0 and returns 1.
`2834:0001` stores its argument in the word at `DS:08BC` (FND-SOUND-012).

`2C5F:0AFA` has no far caller in the load image and two in overlays: offset `0x20DC` of overlay
187 and offset `0x22F2` of overlay 204.

## Interpretation

This is the speech player. `DS:1439` is the setting saved as `unk_08` of `PREF`
(FMT-CONFIG-003), which reads as the voice setting. Numbers below 50 are introduction lines, kept
only on the disc (FND-SOUND-001); others are played from the installed copy when there is one and
from the disc otherwise, and `DS:4275` records that the disc copy was used. `4E71:0033` holds the
disc's drive number counted from 0 for `A:`. The routine stops the music and sets the music mode
to 0 before speaking. When `DS:0D9C` is set it waits for the line to end, touches
`RESOURCE.GFF` on the disc when `DS:14E8` is set, and sets the music mode to 3 in combat and 2
outside it.

## Alternatives

What `DS:0D9C`, `DS:6554`, `DS:4263` and `DS:14E8` stand for was not read, nor why the disc's
`RESOURCE.GFF` is opened and closed at once; it may keep the drive awake. The caller in overlay
187 and the one in overlay 204 were not read, so which numbers are spoken when is not known.

## How to reproduce

Disassemble `2C5F:0AFA` to `2C5F:0C8D` and overlay 187 from file offset `0x7299C` (its code starts
at `0x6FE30`), and search the overlays for `9A FA 0A B8 00`.
