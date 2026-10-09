---
id: FND-EXE-524
title: Every computed near jump reached in the ten fixup-holding overlays is bounded and stays in their code
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000671E0..0x00067D64
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081130..0x000816C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008BCC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093160..0x00094D09
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00072E80..0x000747E0
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000810A0..0x00081639
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000835A0..0x00087034
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000879D0..0x00088FEB
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000950B0..0x00095EB6
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00098320..0x0009920F
tool: Capstone 5.0.7 16-bit recursive decoding and xxhash 4.0.1
environment: null
---

## Observation

The ten overlays that hold FND-EXE-523's fixup spans are, in the installed
`DSUN.EXE` (XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`), descriptors 180,
194, 203 and 209, and in `CD:DSUN.EXE` (XXH3-128
`318cd5ec0559901add3780097162a919`), descriptors 188, 194, 197, 199, 210 and
212. For each, a recursive decoding starts at every one of its trampoline
targets, with offset 0 at the first byte of its code block, and uses
FND-EXE-523's walk: near jumps, conditional jumps and near calls followed,
far calls stepped over, returns stopping, and table jumps followed through
the slots their bound admits.

| File | Descriptor, code | Trampolines | Reached bytes | Computed jumps reached (block offset: table, slots) |
|---|---|---:|---|---|
| `DSUN.EXE` | 180, `0x000671E0..0x00067D64` | 13 | 2,936 of 2,948 | none |
| `DSUN.EXE` | 194, `0x00081130..0x000816C9` | 3 | 1,334 of 1,433 | `0x0035`: key scan, `0x0355`, 19 |
| `DSUN.EXE` | 203, `0x0008B240..0x0008BCC8` | 9 | 2,644 of 2,696 | `0x014E`: key scan, `0x050F`, 13 |
| `DSUN.EXE` | 209, `0x00093160..0x00094D09` | 20 | 6,998 of 7,081 | `0x0407`: key scan, `0x061A`, 4; `0x0F1A`: key scan, `0x1236`, 4 |
| `CD:DSUN.EXE` | 188, `0x00072E80..0x000747E0` | 49 | 6,494 of 6,496 | none |
| `CD:DSUN.EXE` | 194, `0x000810A0..0x00081639` | 3 | 1,334 of 1,433 | `0x0035`: key scan, `0x0355`, 19 |
| `CD:DSUN.EXE` | 197, `0x000835A0..0x00087034` | 38 | 14,769 of 14,996 | `0x1071`: key scan, `0x1101`, 6; `0x1F4B`: key scan, `0x22E4`, 9; `0x3285`: key scan, `0x34B3`, 15 |
| `CD:DSUN.EXE` | 199, `0x000879D0..0x00088FEB` | 15 | 5,496 of 5,659 | `0x00C8`: key scan, `0x028E`, 9; `0x0162`: key scan, `0x0274`, 4; `0x1077`: key scan, `0x143D`, 13 |
| `CD:DSUN.EXE` | 210, `0x000950B0..0x00095EB6` | 17 | 3,590 of 3,590 | none |
| `CD:DSUN.EXE` | 212, `0x00098320..0x0009920F` | 8 | 3,783 of 3,823 | `0x09D0`: key scan, `0x0A2C`, 5; `0x0A90`: key scan, `0x0B48`, 4 |

The walk also reports 13 bounded `cmp bx, N` table jumps across these
overlays. A key scan is the form FND-EXE-523 describes: `mov cx, N;
mov bx, K`, a loop of at most N word comparisons at CS:BX that adds 2 to
BX, and on a match `jmp cs:[bx + D]`; the table column gives K + D. Every
word of every table is below its overlay's code size. No walk meets an
interrupt, a computed near call, a computed jump without a bound, an
undecodable byte or a near target at or past its code size.

## Interpretation

Code reached from a holder's trampolines never transfers into the holder's
own fixup table: direct branches were ruled out by FND-EXE-523's byte-offset
scan, and every computed jump reached here names an offset inside the code.
With FND-EXE-523, no native transfer that enters through a trampoline
reaches any of FND-EXE-173's fixup or padding spans. This settles
Q-EXE-023.

## Alternatives

The bytes the walks do not reach (from 0 to 227 per overlay) are not
entered through a trampoline and are not covered. A key scan's loop exits
on a match only with BX equal to K plus twice a count below N, so its jump
cannot read past the N handler words. A far transfer built at run time and
code written at run time are outside this reading, as in FND-EXE-520.

## How to reproduce

Run `python -I tools/research/exec-census/holder_computed_transfers.py
<install dir>/DSUN.EXE <install dir>/game.gog
spec/builds/BLD-GOG-EN-1.1.files.yaml` from the commit that adds this
finding, with the locked evidence Python (Capstone 5.0.7, xxhash 4.0.1). It
checks both files against the manifest, takes the overlays whose code
starts at the offsets in this finding's locations, walks from all their
trampoline targets and prints each computed jump with its table. Nothing is
written or executed.
