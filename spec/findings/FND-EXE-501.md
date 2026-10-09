---
id: FND-EXE-501
title: For the sound utility's four driver functions, SBAWE32.ADV dispatches only through code table entries
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00000DAF..0x00000E84
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x000010F8..0x00001195
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00000263..0x0000027E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

SBAWE32.ADV's function table maps 0064 to 0x0217, 0065 to 0x0149, 0066
to 0x1077 and 0067 to 0x0DAF. Decoding from each of these alone:

- 0064 (31 bytes) and 0065 (536 bytes) reach no call to 0x01ED, the only
  direct caller of the MIDI routine 0x1A45 (FND-EXE-496), and no
  transfer through memory.
- 0067 returns at once unless the word at CS:03A2 is zero. It then
  increments CS:03A2, loads CX from CS:039E and, when CX is zero, jumps
  to 0x0E78, which decrements CS:03A2 and returns. Decoding from 0x0DAF
  without following the jump at 0x0DF8 into the loop at 0x0DFD visits
  only 0x0DAF..0x0DC4, 0x0DE2..0x0DFD and 0x0E78..0x0E84, which hold no
  call. The 14 calls to 0x01ED that 0067 reaches are reached only through
  that loop, which runs CX times.
- CS:039E ships as 0000. The decoded instructions naming it are reads at
  0x066E, 0x0DEE and 0x11C0, the store of 0000 at 0x1087 in function
  0066, the increment at 0x13B6, reached only from function 0097, and
  the decrement at 0x140E, reached from 0067 (inside the loop), 0068 and
  0098.
- 0066 calls 0x01ED at three sites. At 0x1126 it sends status B0 plus
  channel, for channels 1..9, with the controller from the byte at
  CS:0263 + n and the value from CS:026C + n for n = 0..8, skipping a
  value of FF. At 0x1162 it sends status E0 plus channel with data 00
  and 40. At 0x1189 it sends status C0 plus channel with the program
  from CS:0274 + channel, skipping FF. The bytes at 0x0263..0x026B ship
  as 07 01 0A 0B 40 72 6E 6F 70, and those at 0x026C..0x0274 as 7F 00 40
  7F 00 00 00 00 00. No decoded instruction stores into 0x0263..0x026B,
  and no decoded block store starts at or below 0x026B.

0x1A45 selects entry 3 of DS:3B42 for status B0..BF, entry 4 for C0..CF
and entry 6 for E0..EF. Entry 3 reaches the dispatch through DS:4428
indexed by the controller, and the nine controllers select rows 01
(359C), 07 (3468), 0A (344C), 40 (3538), and 0B, 6E, 6F, 70 and 72
(33D8). Decoding from 0066 together with entries 3, 4 and 6 and those
rows reaches neither 0x27FA, 0x2745 nor 0x2C28, and no other transfer
through memory.

## Interpretation

The sound utility asks SBAWE32.ADV only for functions 0064 to 0067
(FND-EXE-497). Function 0097 is the only code that raises the sequence
count, so in the utility the count stays zero, and 0067 returns without
sending anything. The only MIDI messages the driver sends for the
utility are 0066's control changes, pitch bends and program changes.
Their controller numbers are all below 80, so the DS:4428 dispatch takes
only code entries, and no message reaches the note routine 0x2C28 whose
switches read code bytes (FND-EXE-498). This settles Q-EXE-018: in the
utility's use, SBAWE32.ADV sends control only to code entries.

## Alternatives

Reading the controller bytes as changed before 0066 sends them would
need a store into 0x0263..0x026B, and no decoded instruction or block
store makes one. Reading the sequence count as raised without function
0097 would need another writer of CS:039E; none is decoded. Stores
through a computed address that happens to equal 039E or 0263..026B,
stack-built transfers and code written at run time are not covered.

## How to reproduce

Run `python -I tools/research/exec-census/sbawe32_utility_paths.py
<install dir>/game.gog spec/builds/BLD-GOG-EN-1.1.files.yaml` from the
commit that adds this finding, with the locked evidence Python (Capstone
5.0.7, xxhash 4.0.1). It reads SBAWE32.ADV through FND-EXE-492's disc
reader and manifest check, decodes from functions 0064..0067 one at a
time, and prints the calls to 0x01ED with their argument setup, whether
0x1A45, 0x27FA, 0x2745 and 0x2C28 are reached, what 0067 visits when
the jump at 0x0DF8 is not followed, the shipped table bytes
and count, every decoded instruction naming 039E with the functions
that reach it, stores and block stores into 0x0263..0x026B, and the
DS:4428 rows the controllers select. Nothing is extracted to disk and
nothing is executed.
