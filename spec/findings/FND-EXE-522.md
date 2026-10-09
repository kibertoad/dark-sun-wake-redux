---
id: FND-EXE-522
title: No near transfer from the four owning entries reaches FND-EXE-173's code-or-image anomalous spans
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
    kind: file-data
    offset: 0x0004BEC0..0x0004BFCE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004BFD0..0x0004C054
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00095F30..0x00098020
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x0005A840..0x0005E9AA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004CD80..0x0004CDF5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004CE50..0x0004CEA2
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004B820..0x0004B921
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00055519..0x0005553D
tool: Capstone 5.0.7 16-bit recursive decoding and xxhash 4.0.1
environment: null
---

## Observation

The installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`) and `CD:DSUN.EXE` (634,704 bytes,
XXH3-128 `318cd5ec0559901add3780097162a919`, read from the root directory
of `game.gog`) match the build manifest. FND-EXE-173 gives four anomalous
spans that are not wholly fixups or padding, each assigned by the analyzer
to the function at an owning entry. Each owning entry is a trampoline
target of its descriptor's stub:

| File | Owning entry | Descriptor, code block | Block offset (trampoline) | Span | Where the span lies |
|---|---|---|---|---|---|
| `DSUN.EXE` | `0x0006B581` | 183, `0x0006AFE0..0x0006CE93` | `0x05A1` (42nd of 46) | `0x0006D090..0x0006D150` | descriptor 184's code, offsets `0x0000..0x00C0` |
| `CD:DSUN.EXE` | `0x0009674B` | 211, `0x00095F30..0x00098020` | `0x081B` (13th of 17) | `0x00095F30..0x000961E5` | descriptor 211's own code, offsets `0x0000..0x02B5` |
| `CD:DSUN.EXE` | `0x00097BED` | 211, `0x00095F30..0x00098020` | `0x1CBD` (11th of 17) | `0x00099380..0x000995C3` | descriptor 213's code, offsets `0x0000..0x0243` |
| `CD:DSUN.EXE` | `0x0005E2ED` | 173, `0x0005A840..0x0005E9AA` | `0x3AAD` (38th of 45) | `0x00055519..0x0005553D` | resident load image, 36 zero bytes |

Descriptors 184, 211 and 213 each have a trampoline whose target is offset
`0x0000`, and descriptor 211 also has one at `0x02B5`, the end of its span.

A recursive decoding from each owning entry, with offset 0 at the first byte
of the entry's code block, follows near jumps, conditional jumps and near
calls, continues after far calls, and stops at returns. Each table jump is
`jmp cs:[bx + table]` after an unsigned `cmp bx, N` that skips the table
when BX is larger (directly with `ja`, or with `jbe` over a `jmp`), then
`shl bx, 1`; the walk follows all `N + 1` words:

| Owning entry | Reached | Table jumps (block offset: table, slots) |
|---|---|---|
| `0x0006B581` | 518 instructions, 1,447 bytes | `0x05B8`: `0x07C1`, 6 |
| `0x0009674B` | 1,164 instructions, 3,446 bytes | `0x089C`: `0x10CA`, 21; `0x1CFA`: `0x1DAC`, 8 |
| `0x00097BED` | 214 instructions, 583 bytes | `0x1CFA`: `0x1DAC`, 8 |
| `0x0005E2ED` | 158 instructions, 455 bytes | `0x3B27`: `0x3BC5`, 4 |

Every table word is below its block's code size. No walk meets an
interrupt, a computed transfer without such a bound, an undecodable byte
or a near target past its code size, and no reached instruction lies in
the corresponding span. The walk from `0x0009674B` stays within block
offsets `0x073D..0x20F0`.

## Interpretation

None of the four spans belongs to the body the analyzer gives it.
FND-EXE-520 shows that an overlay's segment holds only its own code block
and fixups at offset 0, so near transfers from descriptors 183 and 173
cannot reach descriptor 184's code, descriptor 213's code or the resident
image, and the walks above find none. The span inside descriptor 211 is not
reached either: it runs from the target of descriptor 211's offset-0
trampoline to the target of its `0x02B5` trampoline. The spans in
descriptors 184 and 213 likewise begin at their descriptors' offset-0
trampoline targets. The resident span is zero bytes, not code. This settles
Q-EXE-021.

## Alternatives

The walk takes more than a single procedure: it follows near calls into
callees and continues after every far call, including calls that might not
return, so it can only add instructions to what the entry reaches. A span
that a smaller walk reached would be in this one. A far call into another
overlay's trampoline enters that overlay as its own entry, not as part of
the caller's body. Entry by a route other than a trampoline, and code
written at run time, are outside this reading, as in FND-EXE-520.

## How to reproduce

Run `python -I tools/research/exec-census/anomalous_span_reach.py <install
dir>/DSUN.EXE <install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml`
from the commit that adds this finding, with the locked evidence Python
(Capstone 5.0.7, xxhash 4.0.1). It checks both files against the manifest,
walks from the four owning entries over the code blocks in the table above,
and prints each walk's size, table jumps, far calls and span hits. The stub
trampoline words are read at the stub offsets in this finding's locations:
target words at stub offset `0x22` with stride 5. Nothing is written or
executed.
