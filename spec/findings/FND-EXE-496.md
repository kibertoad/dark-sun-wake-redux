---
id: FND-EXE-496
title: Disc Miles drivers request no program execution and transfer out only to host callbacks and resident programs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:ADLIB.ADV
    offset: 0x000031DC..0x00003224
  - build: BLD-GOG-EN-1.1
    file: CD:GF1MIDI.ADV
    offset: 0x0000041D..0x00000459
  - build: BLD-GOG-EN-1.1
    file: CD:PASDIG.ADV
    offset: 0x00000C01..0x00000C34
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00001A45..0x00001A83
  - build: BLD-GOG-EN-1.1
    file: CD:SBPDIG.ADV
    offset: 0x00000CE4..0x00000D22
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

Each of FND-EXE-492's 19 disc driver files starts with a word that points
to its function table: pairs of a function number and an offset, ending
at number FFFF. The tables hold 38 functions in the thirteen music
drivers and 17 or 19 in the six digital-audio drivers. Decoding was started at
every table offset and followed through direct jumps, calls and
fall-through. Three more kinds of start were added:

- handlers the decoded code passes as far pointers (`mov ax,handler;
  push cs; push ax`): ALGDIG.ADV 0857 and 08C5, ARIADIG.ADV 0A7C and 0A96,
  PASDIG.ADV 0539 and 05CA, SBDIG.ADV 0568, 058F and 05F9, SBPDIG.ADV
  0612, 0639 and 06A3;
- GF1DIGI.ADV 0A73, stored at CS:0A0F beside CS at CS:0A11;
- SBAWE32.ADV's two near tables, by their bounds below.

The decode covered between 2659 (GF1DIGI.ADV) and 12787 (SBAWE32.ADV)
bytes per file.

In that code, the interrupt instructions are:

| File | Site | Selector |
|---|---|---|
| ARIADIG.ADV | 0x0FED | int 21, AH=62 |
| GF1DIGI.ADV | 0x0FAC, 0x0FC8 | int 21, AH=35 |
| GF1MIDI.ADV | 0x042F, 0x044B | int 21, AH=35 |
| PASDIG.ADV | 0x0C07, 0x0C19, 0x0C28, 0x0CA4 | int 2F, AX=BC00, BC03, BC02, BC04 |
| PASFM.ADV | 0x12B8, 0x12CD, 0x143E | int 2F, AX=BC00, BC03, BC07 |
| PASOPL.ADV | 0x142F, 0x1444 | int 2F, AX=BC00, BC03 |
| ARIATSR.ADV | 0x0599 | int 65, AL from the caller's argument, DX=0330 |

No decoded instruction has an immediate with a CD byte, and no `ret` or
`retf` follows a `push` directly.

The far transfers fall into three groups:

- Music-driver callbacks. ADLIB.ADV 0x29C2 through CS:2335, ADLIBG.ADV
  0x2F8C through CS:28FF, ARIATSR.ADV 0x12DA through CS:0C3F, GF1MIDI.ADV
  0x19D0 through CS:1336, MT32MPU.ADV 0x19A7 through CS:131A, PASFM.ADV
  0x2B95 through CS:2508, PASOPL.ADV 0x2FD8 through CS:294B, PCSPKR.ADV
  0x1110 through CS:0A83, SBAWE32.ADV 0x0A33 through CS:03A6, SBFM.ADV
  0x29F4 through CS:2367, SBP1FM.ADV 0x2B8E through CS:2501, SBP2FM.ADV
  0x2FF0 through CS:2963 and TANDY.ADV 0x1248 through CS:0BBB. Each slot
  ships as four zero bytes. Its only decoded writers are 16 bytes into
  the file's function 00BD, which stores the far pointer argument at
  `[bp+8]` (and the caller's DS in the word before the slot), and 8 bytes
  into function 00BE, which stores zero in both words. Each call is skipped, 24 bytes earlier, when both words are
  zero, and loads DS from the word before the slot.
- Gravis resident calls. GF1MIDI.ADV calls through CS:1425 at 0x04A1,
  0x1D71, 0x1E36, 0x2046, 0x2219, 0x2394 and 0x24AD. GF1DIGI.ADV calls
  through CS:0A1C at ten sites from 0x0ED3 to 0x144B. Each slot ships as
  zero. Its only writer stores BX and ES from the AH=35 request for the
  first of interrupts 78..7F whose handler segment holds `ULTRAMID`
  (GF1MIDI.ADV, eight bytes) or `ULTRAMI` (GF1DIGI.ADV, seven bytes) at
  offset 0103.
- Media Vision resident calls. PASDIG.ADV 0x0387 and 0x0396 call through
  the pointer at CS:019A. Its only writer stores BX and DX from int 2F
  AX=BC03, which runs only after AX=BC00 with BX=3F3F returns BX xor CX
  xor DX equal to 4D56. PASFM.ADV 0x12E6 and PASOPL.ADV 0x145D call through
  `es:[di+24]`, with ES:DI set from DX:BX of AX=BC03 in the same routine.
  That call runs only after the same BC00 test, AX=4D56 from BC03 and
  CX at least 000A.

The near transfers through memory are all in SBAWE32.ADV:

- 0x1A78, in the far routine at 0x1A45, calls through DS:3B42 with DS
  set to CS. The index is the status byte's high nibble minus 8, and
  status F0 and above returns before the call, so entries 0..6 are used:
  1A28, 1A2C, 1A30, 1A34, 1A38, 1A3C and 1A40. 0x1A45 has one direct
  caller, 0x01ED, which has 21 near callers.
- 0x1B4A, reached from entry 3 (0x1A34), calls through DS:4428 indexed by
  AH, which 0x1A45 loads from its caller's byte at `[bp+8]` with no
  mask. Entries 0..127 hold 16 distinct offsets. From entry 128 on, the
  words are zeros and other values.
- 0x2C81, 0x2CAB, 0x2CDF and 0x2D3C jump through CS:1206, CS:1230,
  CS:1264 and CS:12C2, with indexes bounded to 0..9, 0..14, 0..16 and
  0..23. The words those operands name begin right after each jump (file
  0x2C86, 0x2CB0, 0x2CE4 and 0x2D42) only if CS is based at file 0x1A80.
  There they name file 0x2D72 and 0x2D7B, which the decode already
  covers. At file 0x1206..0x12F2 the bytes are decoded code of the file's
  functions 0068, 0096, 00BD, 00BE and 0097. The routine holding these jumps (0x2C28) is
  reached by near calls through 0x27FA from 0x1AB2, which entry 1
  (0x1A2C) calls.

No file in the installation (10503 files) or on the disc (264 files)
has a name ending in .SYS or .COM. `ULTRAMID` occurs only in
GF1DIGI.ADV, GF1MIDI.ADV and both copies of RESOURCE.GFF. `MVSOUND`
occurs only in SOUND_DS.EXE (installed and disc copies) and SVIEW.EXE.

## Interpretation

The drivers' decoded code makes no program-execution request: its only
interrupt 21 selectors are AH=35 and AH=62. Control leaves a driver in
three ways. The first is a callback that the host registers through
function 00BD; FND-EXE-497 shows the sound utility never requests that
function, so in the utility these calls are always skipped. The second
is a resident Gravis program found through interrupts 78..7F, and the
third is a resident Media Vision program found through interrupt 2F.
ARIATSR.ADV also hands a byte to whatever program owns interrupt 65.
None of these resident programs ships with the build. What they do
depends on the player's machine, not on the game's files.

Not settled here: SBAWE32.ADV's dispatch through DS:4428 with a caller
byte of 80 or more, and the CS base of its switch code. The jump tables
fit a CS based at file 0x1A80, but the decoded entry path keeps the
driver's base. Bytes outside the decode are not shown to be data.

## Alternatives

FND-EXE-495 recorded far jumps through `[06C7]` at SBPDIG.ADV 0x0D1A,
0x0EF8 and 0x0EFF. Decoding from the function table shows each address
is the last byte of an FFFF immediate. For example, the instruction at
0x0D14 is `mov word cs:[01BA],FFFF`, and decoding from its last byte
reads `FF 2E C7 06` as a far jump. FND-EXE-495 also recorded twelve
SBAWE32.ADV far transfers with register bases, where the decode from
entry points finds only 0x0A33. Its far immediates at PASDIG.ADV 0x006F,
near 0x00AD in four files, and in SBAWE32.ADV lie in no decoded code;
PASDIG.ADV's lies inside its function table. FND-EXE-495 kept
instruction starts by a vote of the 48 preceding start offsets, and that
vote keeps a misaligned start wherever the bytes after it resynchronize.
Its other observation, that no instruction stores an immediate with a CD
byte, holds in this decode and is restated above.

Reading the music-driver callbacks as a way into the sound utility's own
code would need a request for function 00BD, and FND-EXE-497 finds none.
Reading the resident calls as part of the game would need a resident
program among the build's files, and the name and string search finds
none.

## How to reproduce

Run `python -I tools/research/exec-census/disc_adv_flow_scan.py
<install dir> spec/builds/BLD-GOG-EN-1.1.files.yaml` from the commit that
adds this finding, with the locked evidence Python (Capstone 5.0.7,
xxhash 4.0.1). It reuses FND-EXE-492's disc reader and manifest check,
decodes from the starts and table bounds given above, and prints each
file's decoded byte count, interrupt sites with the nearest immediate AX
or AH writer among the 12 instructions before them, far and near
transfers through memory, the zero guard before each callback call, the
writers and shipped bytes of each slot, immediates with a CD byte, and
returns right after a push. It then walks the installation directory and
the disc's ISO 9660 tree for .SYS and .COM names and the strings
`ULTRAMID` and `MVSOUND`. Nothing is extracted to disk and nothing is
executed.
