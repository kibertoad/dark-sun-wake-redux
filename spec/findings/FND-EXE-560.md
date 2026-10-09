---
id: FND-EXE-560
title: The overlay manager's startup reads only the FBOV magic and payload size, and skips other FB blocks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:0140
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:028B..4AE5:029B
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_manager_fields.py)
environment: null
---

## Observation

In the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`), the
far routine at `4AE5:0010` sets DS from `CS:0005`, which holds `55CE` after relocation. It
returns 0 at once when the word at `0x110` is 0; the file holds `CD 3F` there. Otherwise it
opens the file through `4AE5:01C1`, `4AE5:01B5` or `4AE5:0206`, which this finding does not
read further, and keeps the handle in `[0x128]`.

The routine at `4AE5:028B` reads CX bytes with service `3Fh` into the caller's frame at
`BP-0x14` and sets carry when the count returned is below CX. Its only near callers in
`4AE5:0000..4AE5:1293` are `4AE5:0063` and `4AE5:00A2`, both in this routine.

- `4AE5:0063` reads 20 bytes. The word at `BP-0x14` must be `0x5A4D` (`MZ`). The routine forms
  the end of the load image from the page count at `BP-0x10` and the last-page count at
  `BP-0x12` (one page fewer when the last-page count is not 0, times 512, plus the last-page
  count) and rounds it up to a multiple of 16.
- At `4AE5:0094` it seeks to that position (service `4200h`) and `4AE5:00A2` reads 16 bytes. The
  word at `BP-0x14` must be `0x4246` (`FB`). When the word at `BP-0x12` is not `0x564F` (`OV`),
  it adds 16 and the doubleword at `BP-0x10` to the position and repeats the seek and read.
- When it is `OV`, it stores the position after the 16 bytes in `[0x114]` and `[0x116]`, and the
  doubleword at `BP-0x10`, the pack's `payload_size`, in the words at `0x356C` and `0x356E` of
  the segment in `CS:0007`, `57E0` after relocation. It then closes the file.

A failed read or a wrong signature closes the file and returns AX `0xFFFF`, with CX `0xFFFD`
for a short read and `0xFFFC` for a wrong signature. No instruction in `4AE5:0010..4AE5:0140`
reads `BP-0x0C` to `BP-0x05`, where the pack's `segment_table_offset` and `segment_count` land.
The descriptor walk this routine calls at `4AE5:0107` reads a fixed table instead (FND-EXE-561).

## Interpretation

The manager finds the pack by its `FB` signature after the load image, skips any `FB` block
whose second word is not `OV` by that block's own size field, and needs only `magic` and
`payload_size` from the pack header. It does not use `segment_table_offset` or `segment_count`
from the file, so their signedness does not matter to it.

## Alternatives

Other code could read the pack header through its own file reads. This reading covers the
manager's startup only, and the search for callers of `4AE5:028B` is limited to near calls in
segment `4AE5`. The open routines and how the file name is formed were not read.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_manager_fields.py <install dir>/DSUN.EXE`
from the commit that adds this finding, with the locked evidence Python. It checks the file's
size and XXH3-128, applies the MZ relocations for a load image at `1000`, disassembles
`4AE5:0010..4AE5:0140` and `4AE5:028B..4AE5:029B`, and lists the near calls to `4AE5:028B` in
`4AE5:0000..4AE5:1293`.
