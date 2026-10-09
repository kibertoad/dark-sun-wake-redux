---
id: FND-EXE-568
title: The overlay manager reads its overlays from the running program's own file and allocates a buffer of twice the largest overlay
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0029..4AE5:0050
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:01B5..4AE5:028B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D27..4AE5:0D82
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_file_buffer.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with DS `55CE` in the manager (FND-EXE-560):

- Buffer setup, far routine `4AE5:0D27`. It runs the descriptor walk (`4AE5:029B`, FND-EXE-561),
  then takes `[0x11A]`. When that is below the word at `57E0:3570` it uses that word, otherwise
  twice `[0x11A]`, and adds 1. It calls `1000:15A5` with that many paragraphs times 16 as a
  doubleword. When the far pointer returned is null it jumps to `1000:02AD`. Otherwise, with
  DX the pointer's segment plus 1, it calls the startup routine `4AE5:0010` with the words 0, 0,
  DX plus the paragraph count, and DX: a null far pointer for the file name, then the end and
  the start of the buffer (`[bp+0x0A]` and `[bp+0x0C]`, FND-EXE-566). When the startup returns
  a nonzero AX it jumps to `1000:02AD`.
- The word at `57E0:3570` is 0 in the file, and `[0x114]`, `[0x116]` and `[0x11A]` are 0. The
  only bytes `70 35` in the load image are the operands of `4AE5:0D40` and `4AE5:0D47`, the two
  reads above, and the FBOV pack holds no such pair. The largest overlay by the walk's measure is
  descriptor 173, 1,095 paragraphs, so `[0x11A]` is 1,097 and the routine asks for 2,195
  paragraphs (35,120 bytes).
- Opening the file, in the startup routine. `4AE5:0029` calls `4AE5:01C1`. On DOS 3 or later
  (service `30h`), it sets the open mode at `[0x06]` to `0x20`, follows `57E0:008C` to a segment,
  skips its NUL-terminated strings to the empty one and a word after it, and copies the next
  string to `55CE:008C`, keeping the position after its last `\`. `4AE5:0263` then copies up to
  12 characters of the name the far pointer at `[bp+6]` gives over that position, when the
  pointer is not null, and opens the result with service `3Dh` and the mode at `[0x06]`. When
  that fails and the pointer is null, the routine sets it to `55E8:0728` (`55CE:08C8`), which
  holds `dsmall.exe` and a NUL. `4AE5:01B5` opens that name as it is. When that fails,
  `4AE5:0206` looks in the same strings for one starting with `PATH=` (the five bytes at
  `4AE5:0009`) and tries each `;`-separated directory with a `\` added when it does not end in
  `:` or `\`, followed by the name. When all fail the startup returns `0xFFFE`.

## Interpretation

When `4AE5:0D27` runs, it sizes the overlay buffer at twice the largest overlay, since nothing
changes the 0 in the word at `57E0:3570`, and takes the buffer from the heap. The manager
then opens the running program's own file, whose full path DOS 3 and later keep after the
environment strings, because `4AE5:0D27` passes no file name. The overlays are read from
`DSUN.EXE` itself. `dsmall.exe`, looked for in the current directory and then along `PATH`, is
used only when that open fails or on DOS 2.

## Alternatives

What reaches `4AE5:0D27` was not read: the load image holds a far pointer to it at `5B79:0006`
(FND-EXE-564), after the bytes `01 01`. Neither were `1000:15A5`, taken here to be the run time's
far allocator from its argument and result, and `1000:02AD`, taken to be a fatal-error exit, nor
what the segment at `57E0:008C` is beyond the strings read from it.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_file_buffer.py <install dir>/DSUN.EXE` from the
commit that adds this finding, with the locked evidence Python. It checks the file, applies the
MZ relocations, disassembles the three ranges, prints the default name, the `PATH=` bytes and the
data words, lists every `70 35` pair with the instruction holding it, and computes the buffer
size.
