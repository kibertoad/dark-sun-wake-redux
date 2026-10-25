---
id: FND-VIDEO-011
title: Read with the chunk layouts of the FLI decoders, every chunk of the five installed FLI files ends where its data ends, and the run-length chunk of 5.FLI's first record, stated one byte longer than its record, is decoded to the record's last byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: 1.FLI
    offset: 0x80..0x57244E
  - build: BLD-GOG-EN-1.1
    file: 2.FLI
    offset: 0x80..0x13BCD6
  - build: BLD-GOG-EN-1.1
    file: 3.FLI
    offset: 0x80..0x61E11C
  - build: BLD-GOG-EN-1.1
    file: 4.FLI
    offset: 0x80..0x4F0E4
  - build: BLD-GOG-EN-1.1
    file: 5.FLI
    offset: 0x80..0x57D1E7
tool: tools/research/exec-census/fli_chunks.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

`fli_chunks.py` follows the records and chunks of each installed FLI file (FMT-VIDEO-001) and
reads the data of every colour (`0x0B`), line delta (`0x0C`) and run-length (`0x0F`) chunk as the
decoders of overlays 214 to 217 read it (FND-EXE-613), without writing pixels.

| File | Records | `0x0B` | Colour packets | `0x0C` | `0x0F` | `0x10` |
|---|---|---|---|---|---|---|
| `1.FLI` | 1176 | 17 | 35 | 578 | 1 | 1 |
| `2.FLI` | 374 | 3 | 3 | 266 | 1 | 1 |
| `3.FLI` | 576 | 4 | 4 | 429 | 1 | 0 |
| `4.FLI` | 285 | 1 | 1 | 270 | 1 | 0 |
| `5.FLI` | 1399 | 6 | 25 | 554 | 1 | 0 |

No file has a `0x0D` chunk. In every file each decoded chunk ends at its stated end or before it,
and none reads past its record's end. No colour chunk has a 256-colour packet followed by another
packet, and no line delta chunk has a line count of 0.

The first record of each file holds a colour chunk at `0x90` ending at `0x39A` and a run-length
chunk from `0x39A`. In `1.FLI` to `4.FLI` the run-length chunk's stated end and its record's end
are both `0x918`, and its 200 lines end there. In `5.FLI` the run-length chunk states its end as
`0x1520`, the record ends at `0x151F`, and its 200 lines end at `0x151F`.

## Interpretation

The chunk layouts read from the decoders account for every byte of the installed files' colour,
line delta and run-length chunks. The run-length decoder takes 200 lines whatever the chunk size
says, so the extra byte that the run-length chunk of `5.FLI`'s first record states is never read:
the next record is found from the record size, and the decoder stops at the record's last byte.
The two colour-routine edge cases of FND-EXE-613 do not occur in the shipped files.

## Alternatives

The walk reads the bytes as the decoders would and does not run them; a decoder reading a byte
the listing in FND-EXE-613 does not show would change the stopping points. The copies on the disc
were not walked.

## How to reproduce

`python -I tools/research/exec-census/fli_chunks.py <install> 1.FLI 2.FLI 3.FLI 4.FLI 5.FLI`, with
`<install>` the installed BLD-GOG-EN-1.1 directory.
