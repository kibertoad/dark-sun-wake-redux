---
id: FND-EXE-495
title: Disc Miles drivers build no interrupt opcode at run time and hold few indirect far transfers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00000000..0x00009BB3
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

Each of FND-EXE-492's 19 disc driver files was decoded from every byte
offset, keeping an instruction only where at least 20 of the 48
preceding start offsets decode to it. No kept instruction in any file
loads or stores an immediate containing byte CD, so no driver writes an
interrupt opcode into memory as an immediate.

The kept far transfers through memory are:

| File | Sites | Operands |
|---|---|---|
| ADLIB.ADV | 0x29C2 | cs:[2335] |
| ADLIBG.ADV | 0x2F8C | cs:[28FF] |
| ARIATSR.ADV | 0x12DA | cs:[0C3F] |
| GF1DIGI.ADV | ten sites from 0x0ED3 to 0x144B | cs:[0A1C] |
| GF1MIDI.ADV | 0x04A1 and six sites from 0x1D71 to 0x24AD; 0x19D0 | cs:[1425]; cs:[1336] |
| MT32MPU.ADV | 0x19A7 | cs:[131A] |
| PASDIG.ADV | 0x0387, 0x0396 | es:[di] |
| PASFM.ADV | 0x12E6; 0x2B95 | es:[di+24]; cs:[2508] |
| PASOPL.ADV | 0x145D; 0x2FD8 | es:[di+24]; cs:[294B] |
| PCSPKR.ADV | 0x1110 | cs:[0A83] |
| SBAWE32.ADV | twelve sites, eleven of them in 0x3B7C..0x6650 | mixed register bases |
| SBFM.ADV | 0x29F4 | cs:[2367] |
| SBP1FM.ADV | 0x2B8E | cs:[2501] |
| SBP2FM.ADV | 0x2FF0 | cs:[2963] |
| SBPDIG.ADV | 0x0D1A, 0x0EF8, 0x0EFF | jmp far [06C7] |
| TANDY.ADV | 0x1248 | cs:[0BBB] |

ALGDIG.ADV, ARIADIG.ADV and SBDIG.ADV hold none. Far immediates are kept
at 0x006F in PASDIG.ADV, near 0x00AD in PASFM, PASOPL, SBP1FM and
SBP2FM.ADV, and at thirteen offsets in SBAWE32.ADV.

## Interpretation

Q-EXE-017's runtime-built requests are ruled out for these files: a
request built like the sound utility's stack thunk needs the CD byte, and
none is stored as an immediate. The listed far transfers are candidates.
The far immediates near 0x00AD and the scattered SBAWE32.ADV sites sit in
regions not yet shown to be code, and the alignment vote does not
separate code from data. Each listed site still needs its pointer's
writers read before the drivers' indirect paths are settled.

## Alternatives

Reading the far immediates as calls into fixed memory assumes the bytes
around them are code; the vote shows only that decoding agrees there. A
CD byte assembled from two registers or copied from data would escape an
immediate search; no `stosb`/`stosw` or byte-copy path writing CD is
claimed or excluded here.

## How to reproduce

Run `python -I tools/research/exec-census/disc_adv_transfer_scan.py
<install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml` from the
commit that adds this finding, with the locked evidence Python
(Capstone 5.0.7, xxhash 4.0.1). It reuses FND-EXE-492's disc reader and
manifest check, keeps instruction starts with at least 20 agreeing starts
among the 48 preceding offsets, and reports FF /3 and FF /5 memory
transfers (with an optional segment prefix), 9A and EA immediates, `iret`
and immediates whose hex ends or begins with CD. Nothing is extracted to
disk and nothing is executed.
