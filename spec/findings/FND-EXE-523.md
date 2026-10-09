---
id: FND-EXE-523
title: No owning body, trampoline or direct near branch reaches FND-EXE-173's fixup-table and padding spans
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006AFE0..0x0006CE93
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078340..0x0007BE16
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087310..0x000878CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A4A0..0x0008B12B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00067140..0x00067CA6
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x0006D080..0x0006F3A0
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00078310..0x0007BDE1
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000873E0..0x0008799C
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000879D0..0x00088FEB
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000919F0..0x000931D5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00095F30..0x00098020
tool: Capstone 5.0.7 16-bit recursive decoding and xxhash 4.0.1
environment: null
---

## Observation

The installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`) and `CD:DSUN.EXE` (634,704 bytes,
XXH3-128 `318cd5ec0559901add3780097162a919`, from the root directory of
`game.gog`) match the build manifest. Their stubs give 57 and 59 overlays;
for each, the code runs from the stub's file position plus the FBOV payload
start for the code size, the fixup table follows for the fixup size, and
the bytes up to the next overlay's code are zero padding. FND-EXE-173
assigns 13 fixup-table spans and 6 padding spans to the functions at 14
owning entries:

| File | Owning entry (descriptor, block offset) | Span | Holder |
|---|---|---|---|
| `DSUN.EXE` | `0x0006B581` (183, `0x05A1`) | `0x0006D081..0x0006D090` | 183's padding |
| `DSUN.EXE` | `0x0006C01D` (183, `0x103D`) | `0x00067D99..0x00067DCF` | 180's fixups |
| `DSUN.EXE` | `0x0007A5A7` (190, `0x2267`) | `0x0008171E..0x0008173E` | 194's fixups |
| `DSUN.EXE` | `0x00087459` (198, `0x0149`) | `0x00094E46..0x00094E49` | 209's fixups |
| `DSUN.EXE` | `0x0008772D` (198, `0x041D`) | `0x00094EB8..0x00094F3B`, `0x00094F3B..0x00094F5E` | 209's fixups, 209's padding |
| `DSUN.EXE` | `0x0008AA6A` (202, `0x05CA`) | `0x0008BD04..0x0008BD90`, `0x0008BD90..0x0008BDB3` | 203's fixups, 203's padding |
| `CD:DSUN.EXE` | `0x000677FC` (180, `0x06BC`) | `0x0007485E..0x00074890` | 188's fixups |
| `CD:DSUN.EXE` | `0x0006EA00` (184, `0x1980`) | `0x0007497D..0x00074988`, `0x00074988..0x000749AB` | 188's fixups, 188's padding |
| `CD:DSUN.EXE` | `0x0007A577` (190, `0x2267`) | `0x00081642..0x000816AE` | 194's fixups |
| `CD:DSUN.EXE` | `0x000877FD` (198, `0x041D`) | `0x000870C4..0x0008710E` | 197's fixups |
| `CD:DSUN.EXE` | `0x00088A01` (199, `0x1031`) | `0x00087278..0x00087285` | 197's fixups |
| `CD:DSUN.EXE` | `0x00092C03` (208, `0x1213`) | `0x0008913A..0x00089168` | 199's fixups |
| `CD:DSUN.EXE` | `0x0009674B` (211, `0x081B`) | `0x00095EE2..0x00095F1A`, `0x00095F1A..0x00095F30` | 210's fixups, 210's padding |
| `CD:DSUN.EXE` | `0x00097BED` (211, `0x1CBD`) | `0x0009933E..0x00099371`, `0x00099371..0x00099380` | 212's fixups, 212's padding |

Every owning entry except `0x00092C03` is one of its descriptor's
trampoline targets. Only the first span lies in its owner's own descriptor,
and it is padding.

A recursive decoding from each owning entry over its own code block, with
offset 0 at the block's first byte, follows near jumps, conditional jumps
and near calls, steps over far calls and stops at returns, as in
FND-EXE-522. Its table jumps take two forms. In the first, an unsigned
`cmp bx, N` skips the table for larger BX, and the walk follows all
`N + 1` words. In the second, at `CD:DSUN.EXE` descriptor 199 offset
`0x1077` and descriptor 208 offset `0x0F6E`, `mov cx, N; mov bx, K` starts
a loop that compares the word at CS:BX with a key, adds 2 to BX and runs at
most N times, leaving on a match to `jmp cs:[bx + D]`. Here N, K and D are
13, `0x1423`, `0x1A` and 15, `0x10FB`, `0x1E`, and the walk follows the N
words from K + D. No walk meets an interrupt, an unbounded computed
transfer, an undecodable byte or a near target past its code size, and no
reached instruction overlaps any span.

Every trampoline target of each holder (descriptors 180, 183, 194, 203 and
209 installed; 188, 194, 197, 199, 210 and 212 disc) is below that holder's
code size. A scan of every byte offset in the code of each fixup holder for
a direct near branch (`E8`, `E9`, `EB`, `70`..`7F`, `E0`..`E3`) whose
target falls in the holder's own fixup table finds none.

## Interpretation

FND-EXE-520's loader reads code size plus fixup size into an overlay's
segment, so padding bytes are never loaded. A fixup table is loaded only
into its own overlay's segment, after the code. Near transfers from an
owning body stay in the owner's segment, which holds none of the other
descriptors' spans, and the walks above reach none of them. Far entry into
a holder goes through its trampolines, whose targets all lie in its code.
No direct near branch inside a holder enters its fixup table. The spans
FND-EXE-173 assigns to these bodies are therefore not part of them. This
settles Q-EXE-022 for the bodies the analyzer assigned the spans to.

## Alternatives

The walks over-approximate the bodies: they follow callees and continue
after far calls that may not return, so a smaller body cannot reach more.
The byte-offset scan accepts any byte that would be a branch opcode,
including bytes inside other instructions, so it can only add candidates.
A computed near transfer inside a holder's own code, a far transfer built
at run time rather than through a trampoline, and code written at run time
are not covered here.

## How to reproduce

Run `python -I tools/research/exec-census/fixup_span_reach.py <install
dir>/DSUN.EXE <install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml`
from the commit that adds this finding, with the locked evidence Python
(Capstone 5.0.7, xxhash 4.0.1). It checks both files against the manifest,
lists the overlays from their stubs, walks from the 14 owning entries in
the table above, prints each span's holder and whether its trampoline
targets lie below its code size, then prints the byte-offset scan of each
fixup holder. Its overlay numbers count stubs in file order; this finding
names descriptors by the build's Code ranges rows. Nothing is written or
executed.
