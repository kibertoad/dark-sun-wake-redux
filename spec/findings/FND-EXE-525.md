---
id: FND-EXE-525
title: The corrected-snapshot anomalous spans that FND-EXE-522 and FND-EXE-523 do not cover are not their owners' code either
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000749B0..0x00077F3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078340..0x0007BE16
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083400..0x00086F67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087310..0x000878CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A4A0..0x0008B12B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091890..0x00093088
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093160..0x00094D09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000499DC..0x000499FE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00068790..0x0006ACAD
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x0006AF90..0x0006CE88
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x0006FE20..0x00072B6E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000749B0..0x00077F0F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00078310..0x0007BDE1
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000873E0..0x0008799C
tool: Capstone 5.0.7 16-bit recursive decoding and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-174's corrected snapshots of the installed `DSUN.EXE` (XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`) and `CD:DSUN.EXE` (XXH3-128
`318cd5ec0559901add3780097162a919`) have anomalous spans that FND-EXE-173's
earlier table does not. Its pairs at `0x0006B581`, `0x0006C01D`,
`0x0005E2ED`, `0x0006EA00`, `0x0007A577`, `0x00088A01`, `0x0009674B` and
`0x00097BED` are covered by FND-EXE-522 and FND-EXE-523. The nine other
pairs, read here with the walk and byte-offset scan of FND-EXE-523:

| File | Owning entry (descriptor, block offset) | Span | Holder | Reached instructions |
|---|---|---|---|---|
| `DSUN.EXE` | `0x000796DB` (190, `0x139B`) | `0x00077F40..0x00077F41` | 189's fixups | 1,326 |
| `DSUN.EXE` | `0x00087859` (198, `0x0549`) | `0x0008716E..0x000871B7` | 197's fixups | 30 |
| `DSUN.EXE` | `0x0008AA6A` (202, `0x05CA`) | `0x000870C4..0x00087143` | 197's fixups | 88 |
| `DSUN.EXE` | `0x00092A83` (208, `0x11F3`) | `0x000499DC..0x000499FE` | resident load image, 34 zero bytes | 2,036 |
| `DSUN.EXE` | `0x0009439E` (209, `0x123E`) | `0x00094E46..0x00094E49` | 209's own fixups | 510 |
| `CD:DSUN.EXE` | `0x00069F69` (182, `0x17D9`) | `0x0006AD00..0x0006AD70` | 182's own fixups | 121 |
| `CD:DSUN.EXE` | `0x0006B9C8` (183, `0x0A38`) | `0x00072D6B..0x00072E18`, `0x00072E18..0x00072E3B` | 187's fixups, 187's padding | 577 |
| `CD:DSUN.EXE` | `0x000796AB` (190, `0x139B`) | `0x00077F35..0x00077F36` | 189's fixups | 1,326 |
| `CD:DSUN.EXE` | `0x00087929` (198, `0x0549`) | `0x000870C4..0x0008710E` | 197's fixups | 30 |

Every owning entry is a trampoline target of its descriptor. Every computed
jump the walks reach is a bounded table or a key scan, and every table word
lies in its overlay's code. No walk meets an interrupt, a computed near
call, an undecodable byte or a near target past its code size, and no
reached instruction overlaps its span.

Every trampoline target of each overlay holder is below its code size. The
byte-offset scan for direct near branches into a holder's own fixup table
finds one candidate: installed descriptor 189 offset `0x3548`, a `7D` byte
with target `0x35AB`. The walk from all 30 of descriptor 189's trampoline
targets reaches the instruction at `0x3546`, `les si, [0x617D]`, whose
address bytes include `0x3548`, so the candidate is not an instruction
start on any reached path.

The holders that FND-EXE-524 did not walk (installed 189 and 197, disc
182, 187 and 189) were walked from all their trampoline targets. Their
reached computed jumps are key scans: installed 189 at `0x05ED` (10
slots), `0x071E` (9) and `0x0BFB` (5); installed 197 at `0x1078` (6),
`0x1F6A` (9) and `0x3358` (15); disc 182 at `0x03B0` (7); disc 189 the
same three as installed 189. Disc 187 reaches none. They also take 11
bounded `cmp bx, N` table jumps. Every table word lies in the overlay's
code. Disc 197 and installed 209 are covered by FND-EXE-524.

## Interpretation

As in FND-EXE-523, no owning body, trampoline, or direct or computed near
transfer reached from a trampoline enters any of these spans. The two
spans in their owner's own fixup table (`0x00094E46` for 209 and
`0x0006AD00` for 182) are loaded into the owner's segment after its code,
but nothing reached from its trampolines targets them. With FND-EXE-522 to
FND-EXE-524, none of the anomalous spans in either corrected snapshot
belongs to the body the analyzer gives it.

## Alternatives

The walks over-approximate as described in FND-EXE-523. Bytes not reached
from any trampoline, a far transfer built at run time and code written at
run time are outside this reading, as in FND-EXE-520 and FND-EXE-524.

## How to reproduce

Run `python -I tools/research/exec-census/corrected_span_reach.py <install
dir>/DSUN.EXE <install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml`
from the commit that adds this finding, with the locked evidence Python
(Capstone 5.0.7, xxhash 4.0.1). It prints, for the nine pairs above, the
walk from each owning entry and each span's holder, then the byte-offset
scan of each fixup holder, then the walks of the five holders from their
trampoline targets. Nothing is written or executed.
