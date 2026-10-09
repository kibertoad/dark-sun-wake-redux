---
id: FND-EXE-490
title: Sound utility load image selects no DOS program-execution service at any interrupt 21 site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00001400..0x0001E5E0
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001E5E0..0x00031F31
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:20DF..1000:21A9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0318..1BD4:0336
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The MZ header of SOUND_DS.EXE gives a 0x1400-byte header and a load image
ending at file offset 0x1E5E0, with 958 relocations. The remaining 80209
bytes begin `FB 52` and contain symbol names such as `_int86x`, `_fopen`,
`_getenv` and `autoexecPath`. They lie past the load image the header
describes, so loading the program does not place them in memory; this
census treats them as appended data.

The load image holds 69 occurrences of the bytes `CD 21`. Each decodes as
an aligned `int 21` instruction, and in each case the nearest preceding
aligned instruction that writes AX or AH loads an immediate, one to five
instructions earlier. The instructions in between write only AL, BX, CX,
DX, SI, DI, DS or ES, or push registers. The AH values and their counts
are 07 (1), 0B (1), 0E (1), 19 (1), 1A (1), 1C (2), 25 (6), 2F (1),
30 (2), 35 (5), 39 (1), 3A (1), 3B (1), 3C (2), 3D (2), 3E (2), 3F (3),
40 (7), 41 (2), 42 (5), 43 (5), 44 (4), 47 (1), 48 (1), 49 (1), 4A (2),
4C (1), 4E (1), 4F (1), 56 (1), 58 (2), 67 (1) and 68 (1). No site
loads AH=4B. The image contains no `CD 2E` and no `CD 2F` byte pair, and
no `B4 4B` (`mov ah,4B`) pair.

A whole-image search treated every byte as a possible start of a short
or near branch (70..7F, E0..E3, EB, E8, E9) or a far immediate (9A, EA)
and collected those whose target lies after an AH writer and at or before
its interrupt. It found six candidates, at file 0x3B74, 0xF19D, 0xF28D,
0xF290, 0xF4E0 and 0xF59B. Each byte lies inside another aligned
instruction: `sub bl,30`, the displacement of `loop`, two `push [bp+..]`
operands, `and cx,27` and `shr bx,1`, respectively. No aligned direct
branch enters a writer-to-interrupt gap.

The image also executes interrupt 21 outside these 69 sites through
FND-EXE-353's wrapper. 1000:2110 writes `55 CD nn 5D CB` into its stack
frame, taking nn from its selector argument, loads AX from word zero of
the caller's input record, and far-calls the stack copy. 1000:2110 has one
caller, the near call from 1000:2106 inside 1000:20DF. Every far call or
far jump in the image whose segment:offset resolves to 1000:20DF was
collected; there are nine, each `9A DF 20 00 00` with a relocated segment
word, and those nine are the only words equal to 20DF in the image. No
word equals 2110. Each call pushes an immediate selector after building
its input record in the same frame:

| Call at file | Selector | Input word zero |
|---|---|---|
| 0x091DE | 2F | BC04, after a far call clears sixteen record bytes |
| 0x0BC0C | 33 | 000A |
| 0x0BC31 | 33 | 0003 |
| 0x0BC5E | 33 | 0003 |
| 0x0BC90 | 33 | 0000 |
| 0x0BCC5 | 33 | 0003 |
| 0x0BD44 | 33 | 0001 |
| 0x0BD75 | 33 | 0002 |
| 0x0D471 | 21 | 3305 |

The last call is at 1BD4:0331, inside FND-EXE-360's stack pathname
constructor at 1BD4:0318.

Every byte offset of the image was also decoded as an instruction start
to find immediates containing byte CD and displacements 0084..0087.
Besides 2110's own store at file 0x3526, the immediate hits are
`mov ax,CD42` at 0x5EF1, pushed with DS as a far pointer argument to
090F:0000; `mov word [DB9C],3CCD` at 0x4CB2; a loop at 0x0AE9B that
fills stack bytes `[bp+si-68]` with CD; and bytes inside the aligned
`mov ax,35xx` and `mov al,[bp+4]` instructions next to the interrupt 21
sites at 0x1571..0x15A7. The two `B8 0E 4B` occurrences, at 0xC124 and
0xC1DF, are aligned `mov ax,4B0E` instructions pushed with DS as a far
pointer argument to 090F:0000. Displacements 0084 and 0087 are read
after 1000:16DE sets DS to 0040 and after 1000:17D9 sets ES to 0040.
1000:1E34 and 1000:1EBA read DS:0087/0089, which FND-EXE-369 records as
heap state. The hit at 0xE22A decodes inside data. No instruction reads
the interrupt 21 vector at 0000:0084..0000:0088. The generic
vector-read helper at load-image offset 0xDFA8 (file 0xF3A8; `mov ah,35`,
AL from its argument) and the vector-write helper at offset 0xDF93 (file
0xF393) have no direct near or far callers and no word equal to either
offset.

The image's indirect far transfers that decode as aligned instructions
are `call far es:[bx+2]` at 1000:0222 and 1000:0263, inside a walker of
six-byte records; `call far [bx-1294]` at 1000:0319, indexed by DS:DA98;
`call far [DB9C]`, `[DBA0]` and `[DBA4]` at 1000:0329..0345; the stack
thunk call at 1000:216C; `call far cs:[si+8]` at file 0xD91A;
`pushf / call far cs:[011C]` at file 0xD99B; and `call far cs:[0E1E]` at
files 0xE87D and 0xE96C. CS:011C/011E receive the words at 0000:0020 and
0000:0022 at file 0xD9AC..0xD9C0, and those are their only writers. The
two CS:0E1E calls first store ES and DI+0100 or DI+0103 into CS:0E20 and
CS:0E1E.

## Interpretation

Within the 69 load-image interrupt 21 instructions and the stack thunk's
nine callers, no request selects AH=4B, the DOS service that loads and
runs a program. The utility also issues no interrupt 2E. So the load
image contains no direct program-execution request, and no batch helper
can be launched through one. This is the reading Q-EXE-007's Settles it
asks for, applied to the sound utility's direct references. The game
editions are not covered.

The CS:011C chain forwards to the interrupt 08 (timer) handler that was
current when the utility saved it. The CS:0E1E calls enter code at a
segment held in ES, and the stack thunk runs bytes built by 2110. The
first two lie outside the load image. Code reached that way, and any
indirect target outside the image, is not covered by this census. The
startup, exit and atexit records, the DS:DB9C..DBA7 hooks and the
CS:[SI+8] table hold far pointers whose values this reading does not
trace. A target inside the load image cannot change the result, since all
of the image's interrupt 21 sites are counted above. A target outside it
would be code the utility loads or inherits, such as an audio driver or
an earlier timer handler.

The appended symbol names include `_int86` and `_int86x` but no `system`,
`spawn`, `exec` or `LoadProg` name, and the `.EXE` texts in the image are
`SW32.EXE` beside `:\autoexec.bat` and the fopen mode `rt`, plus
instructions such as "Run WBMODE.EXE". These are circumstantial and agree
with the census without being evidence for it.

## Alternatives

A program launch through a C library call such as `system` or `spawnl`
would still end in interrupt 21 with AH=4B. The census of every
interrupt 21 instruction and the stack thunk's selectors rules that out
for the load image. A launch through interrupt 2E has no instruction to
use. A selector built at run time in AH would need a non-immediate AH
writer at some site; none exists. Entry between an immediate AH writer and
its interrupt would need an aligned branch into the gap; the whole-image
candidate search finds none. Byte CD stored as an opcode elsewhere would
need a thunk builder besides 2110; the other CD immediates are a data
pointer offset, a function pointer offset, a stack-buffer fill whose
buffer no far call targets, and operand bytes of other instructions.

## How to reproduce

Require FND-EXE-350's SOUND_DS.EXE identity: length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run
`python -I tools/research/exec-census/sound_ds_exec_census.py <SOUND_DS.EXE>`
from the commit that adds this finding with the locked evidence Python
(Capstone 5.0.7 in sixteen-bit mode, xxhash 4.0.1). It reads the header,
lists every `CD 21` in file 0x1400..0x1E5E0 with the nearest immediate
AX/AH writer of the most common aligned decode ending on it (starts one to
48 bytes back), searches for `CD 2E`, `CD 2F`, `B4 4B` and `B8 xx 4B`,
lists far immediates resolving to linear 20DF or 2110 and near rel16
calls to them, and runs the superset CD-immediate, 0084..0087 displacement
and branch-into-gap searches. Read 1000:20DF..1000:21A9, 1000:01ED..0271,
1000:0300..0350 and the file ranges 0xD8F0..0xDA20 and 0xE86E..0xE980
with the same decoder for the transfers named above. Original bytes stay
outside Git; no original process, DOSBox or emulated call runs.
