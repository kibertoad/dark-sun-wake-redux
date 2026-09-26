---
id: FND-SOUND-007
title: The sound-effect routine plays a BVOC resource when one exists and otherwise the installed SOUND file of that number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0A22..2C5F:0AFA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0B00..2D40:0B0F
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080
environment: null
---

## Observation

`DS` is the data segment `57E0`. The far routine at `2C5F:0A22` takes one byte, `n`, and returns
nothing. In order it:

1. returns when the byte at `DS:13F7` is 0, when `n` is `0xFF`, or when the byte at `DS:1435` is 0;
2. returns when the word at offset 8 of the record at the far pointer `DS:3411` is `0x71` (113);
3. when the byte at `DS:14E3` is not 0 and the far routine at `4611:0051` returns a non-zero byte,
   calls `4611:03A5`;
4. when `49E9:00FD` returns a non-zero byte, calls `49E9:0142`;
5. calls `38FF:05B5` with the tag `BVOC` (`0x434F5642`), `n` widened to 32 bits and the address of
   a local;
6. when that returns 0, calls `4654:04FE` with `n`, `0x1770` (6,000) and `0x1771` (6,001), and
   returns;
7. otherwise copies the text at `DS:44F2` into a local, formats `%sSOUND%03u.VOC` (`DS:0F73`) with
   it and `n` into the buffer at `DS:43FB`, calls `56BD:0034` with that buffer, and when the low
   byte of the result is not 0 calls `4611:0177` with the buffer's offset.

It has two far callers in the load image: `2D40:0B07`, in the routine at `2D40:0B00`, which passes
the low byte of its word argument, and one at file offset `0x28F55`. It has 18 far callers in
overlays: offset `0x283B` of overlay 173; `0x1061`, `0x1087` and `0x1178` of overlay 179; `0x018A`,
`0x0198`, `0x01B0`, `0x027F`, `0x0354` and `0x2AD8` of overlay 193; `0x0748`, `0x0802` and
`0x0B1C` of overlay 201; `0x053E` of overlay 203; and `0x0118`, `0x01D3`, `0x0C36` and `0x0E7E` of
overlay 206.

## Interpretation

This is the game's sound-effect player. `DS:13F7` turns all sound on or off (FND-SOUND-010),
`DS:1435` is the sound-effects setting saved as `unk_05` of `PREF` (FMT-CONFIG-003), and card 113
in `SOUND.CFG` is the setup program's "No Sound" (FMT-CONFIG-001). Before a new effect the routine
stops one still playing. It looks for resource `n` of type `BVOC` first and plays it from memory;
only when there is none does it play the loose file `SOUND` and `n` in three digits from the
installation directory at `DS:44F2`, when that file exists. The number space is one: an effect
number is a `BVOC` number or a `SOUND` file number (FND-SOUND-001).

## Alternatives

`38FF:05B5`, `4654:04FE`, `56BD:0034`, `4611:0177` and the two stop checks were not read; that
`38FF:05B5` returns 0 when the resource exists, that `56BD:0034` tests whether a file exists, and
that the others play and stop samples are read from how this routine uses them. What 6,000 and
6,001 mean is not known.

## How to reproduce

Disassemble `2C5F:0A22` to `2C5F:0AFA`, search the load image for far calls to it, and search the
overlays for `9A 22 0A B8 00`, a far call whose fixup word `0x00B8` names the segment `2C5F`.
