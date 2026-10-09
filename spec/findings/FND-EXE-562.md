---
id: FND-EXE-562
title: The INT 3Fh handler loads an overlay and returns into its trampolines, which the manager switches between INT 3Fh and far-jump forms
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:0193
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:04F4..4AE5:0637
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0672..4AE5:06E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0753..4AE5:0785
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_manager_fields.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with DS `55CE` (FND-EXE-560):

- Vector swap, far routine `4AE5:0140`. It reads the vector numbered by byte `[0x111]`, `0x3F`
  in the file, sets it to the far pointer at `[0x0002]`, `4AE5:04F4` after relocation, and
  stores the old vector there. When `[0x128]` is not 0 it then closes that handle and clears
  it; otherwise it opens the name at `55CE:008C` with the mode in byte `[0x0006]` and keeps the
  handle in `[0x128]`. Startup calls it at `4AE5:00F3` after closing the file, and the far
  routine at `4AE5:0193` calls it at `4AE5:01A5` when `[0x128]` is not 0.
- Handler `4AE5:04F4`. After `push bp` and `mov bp, sp`, an odd BP jumps far to `1000:02DF`.
  Otherwise it saves AX, BX, CX, DX, SI, DI, DS and ES, sets DS to `55CE`, enables interrupts,
  and loads ES:BX from the interrupt's return address at `BP+2`. ES is therefore the segment of
  the `INT 3Fh`. It pushes the word at ES:BX, the two bytes after the `INT 3Fh`, and subtracts 2
  from the saved return offset, so that it names the `INT 3Fh` itself.
  - When the result is 0 (the `INT 3Fh` at header `+0`), it calls `4AE5:05A4`.
  - Otherwise (a trampoline), it adds 6 to BP and exchanges the words at BP and BP-6 (the INT's
    saved flags and the handler's saved BP). It calls `4AE5:05A4` and exchanges them back.
  It then pops the pushed word into BX, takes bit 3 of header byte `+0x1A` into AX (0 or 8),
  clears that bit, and calls the far pointer at `[0x0086]` (`1000:02FC`). It restores the
  registers and returns with `IRET` to the `INT 3Fh` instruction.
- Entry, `4AE5:05A4`. When header `+0x10` is not 0, it sets byte `+0x1B` to 1 and bit 2 of
  byte `+0x1A`. Otherwise it sets bit 3 of `+0x1A`, places and loads the overlay through
  `4AE5:055A` and the near routine in `+0x18` (FND-EXE-520), and jumps far to `1000:02DF` when
  the load returns carry. On both paths it calls `4AE5:0672` and adds bits 0 and 1 of `+0x1A`
  to `+0x1B`.
- Jump form, `4AE5:0672`. It returns at once when `+0x0C` is 0 or the first trampoline's first
  byte is `0xEA`. When header word `+0x02` is not 0, it calls `4AE5:0753` with AX the load
  segment `+0x10`, DX the header's segment and CX that word. It then writes each trampoline,
  from `+0x20` with stride 5, as `EA`, the trampoline's old word `+2`, and the load segment:
  a far jump to that offset in the loaded code.
- Trap form, `4AE5:06B1`, called at `4AE5:060C` and from `4AE5:061F`, which then clears
  `+0x10`. It returns at once when the first trampoline's first byte is `0xCD`. Otherwise it
  calls `4AE5:0753` with AX the header's segment, DX the load segment and CX 0, stores the CX
  that call returns in header word `+0x02`, and writes each trampoline back as the word in
  `[0x110]` (`CD 3F`), its jump offset (the word at `+1`) and a byte 0.
- Frame walk, `4AE5:0753`. Through `4AE5:075F` it follows the chain of saved BP words from the
  current BP. A saved word with bit 0 set is followed without a comparison. For each other
  frame, when the word at `+4` equals DX it is replaced with AX, and BX keeps the first such
  frame. When BX is not 0, the word at `SS:BX+2` is exchanged with CX.

## Interpretation

A far call to a trampoline of an overlay that is not in the jump form runs `INT 3Fh`. The
handler loads the overlay when it is not loaded, rewrites every trampoline of that overlay as
a far jump into the loaded code, and returns to the trampoline, which now jumps to `target`.
Later calls jump without the interrupt.

When an overlay is unloaded, its trampolines go back to `INT 3Fh` form. Frames that would return
into its code get the header's segment instead. The innermost of them gets return offset 0,
the header's own `INT 3Fh`, and its old return offset is kept in header word `+0x02`. Returning
there runs the handler on the direct path. The reload's jump-form rewrite then puts the new
load segment in place of the header's segment in the frames the walk compares, and gives the
first of them the kept offset. Whether that first frame is the `INT 3Fh`'s own, so that `IRET`
resumes the interrupted return, depends on the saved BP words the walk meets; this reading
does not settle it.

So header `+0x02` holds a saved return offset while its overlay is unloaded. A trampoline's
fifth byte is 0 in the `INT 3Fh` form and the high byte of the load segment in the jump form.

## Alternatives

The far routines at `1000:02DF` and `1000:02FC` and the near routine at `[0x80]` were not read,
so what the handler's error exit and its after-load call do is not known. Neither is the
effect of the exchange of the flags and saved BP on the trampoline path beyond the words it
moves. Which frames the walk reaches depends on every routine on the stack keeping BP as a
frame chain; that was not checked. The disassembly is linear over the cited ranges.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_manager_fields.py <install dir>/DSUN.EXE`
from the commit that adds this finding. It prints the words at `55CE:0002`, `55CE:0080` to
`55CE:0088` and `55CE:0110` after relocation and disassembles `4AE5:0140..4AE5:0193`,
`4AE5:04F4..4AE5:061F`, `4AE5:061F..4AE5:0637`, `4AE5:0672..4AE5:06E4` and
`4AE5:0753..4AE5:0785`.
