---
id: FND-EXE-498
title: SBAWE32.ADV runs its code above file 0x1A80 at the driver's base, so its four switches read code bytes as targets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00000195..0x000001DA
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00001206..0x000012F2
  - build: BLD-GOG-EN-1.1
    file: CD:SBAWE32.ADV
    offset: 0x00002C28..0x00002D41
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

Decoding SBAWE32.ADV from FND-EXE-496's starts, the code at or above
file 0x1A80 is entered only through near transfers that keep CS:

- near calls from the code below 0x1A80, at 0x0195 (to 0x389C), 0x01C4
  (to 0x1A84), 0x01C7 (to 0x1A94), 0x01D4 (to 0x1AA2) and 0x01D7 (to
  0x1A92);
- the near calls in the seven entries of the table at DS:3B42 (to 0x1AEC,
  0x1AB2, 0x1BE2, 0x1B22, 0x1B60, 0x1C1C and 0x1B92), which the far
  routine at 0x1A45 reads after setting DS to CS;
- the near call through DS:4428, whose 16 distinct targets from 33D8 to
  3764 begin, at the driver's base, with `push bp; mov bp,sp` (or, at
  33D8 and 33DC, `xor ax,ax; cwd; ret`). The same offsets plus 0x1A80
  hold other bytes.

No decoded instruction has immediate 01A8 or a far immediate. The file
has 30 bytes 9A or EA followed by a segment word below 1000, 22 of them
naming 0078:157E or 00FE:157E between 0x66EE and 0x7809, and no decoded
instruction starts at any of them. No decoded instruction writes file
0x1206..0x12F2.

The routine at 0x2C28 has one near caller, in 0x27FA, which calls it
only when its third argument is nonzero. 0x27FA is called from 0x1AB2
(reached from entry 1, status 90..9F), 0x1AEC (from entry 0, status
80..8F) and 0x2745. 0x2C28 takes its first argument as a channel and, for
channel 9 only, reads the word at DS:423E + 9 x 1E (file 0x434C, shipped
as 0000). It jumps through CS:1206 when that word is 0081, CS:1230 for
0082, CS:1264 for 0083 and CS:12C2 for 0087 (FND-EXE-496), indexed by its
second argument minus 29 or 24. Values 0084 and 0086 go through chains
of comparisons, and other values skip the switches. At the driver's base those
operands read these words at file 0x1206..0x12F2, which lie in the code
of functions 0068, 0096, 00BD, 00BE and 0097:

| Jump | Index range | Words read |
|---|---|---|
| 0x2C81 | 0..9 | 76FF FF0A 0876 5550 EC8B 46C7 0002 5D00 E80E F044 |
| 0x2CAB | 0..14 | 1F5E E58B CB5D 8B55 1EEC 5756 FA9C 08B8 8002 00CF E80E FFFA 5E5F 5D1F 55CB |
| 0x2CDF | 0..16 | A806 8003 00CF E80E FFFA 5E5F 5D1F 55CB EC8B 561E 9C57 2EFA 06C7 03A6 0000 C72E A806 |
| 0x2D3C | 0..23 | 76FF 0E08 C3E8 83F1 06C4 FA83 7500 E903 00F6 C28E F88B 46C7 0CFC C700 FA46 0000 76C5 2E0E B789 037E 8C2E 809F C703 0244 |

Two other words equal to 2C28 lie inside repeating 16-byte records at
file 0x3EB0 and 0x9586. Neither decoded table reaches them: DS:3B42 is
read only up to 0x3B50, and DS:4428 only up to 0x4628 even for an index
of 255.

## Interpretation

The code above file 0x1A80 runs with the driver's own CS, the same as
the code below it. Its DS-relative function table (DS:4428) was linked
for that base, but the operands and contents of its four CS-relative
switch tables were linked for a segment starting at file 0x1A80. At run
time, each of these switches takes its target from code bytes. Many of
those words are past the file's end (0x9BB3) or inside instructions.
Taken with an index in range, a switch sends control to a byte the
driver never meant as a target. That needs a call into 0x27FA with a
nonzero third argument on channel 9 while the channel word is 0081,
0082, 0083 or 0087, and the word ships as zero. This settles Q-EXE-019.
Whether the sound utility's calls can produce such a call, and the
writers of the channel word, belong to Q-EXE-018.

## Alternatives

A CS based at file 0x1A80 would make the switch tables consistent, but
no decoded instruction builds that segment, every entry into the code
is a near transfer from the driver's base, and the DS:4428 targets are
function starts only at the driver's base. A run-time copy that moves
the tables into place would need a decoded write to 0x1206..0x12F2,
and there is none. Stack-built transfers and code written at run time
are not covered.

## How to reproduce

Run `python -I tools/research/exec-census/disc_adv_flow_scan.py
<install dir> spec/builds/BLD-GOG-EN-1.1.files.yaml` from the commit that
adds this finding, with the locked evidence Python (Capstone 5.0.7,
xxhash 4.0.1). For SBAWE32.ADV it also prints the near transfers from
below file 0x1A80 to code at or above it, the first bytes of each DS:4428
target at the driver's base and at +0x1A80, decoded operands with
immediate 01A8, bytes 9A or EA with a segment word below 1000 and whether
a decoded instruction starts there, decoded writes into 0x1206..0x12F2,
the near callers of
0x2C28 and 0x27FA, and the words each of the four switches reads at the
driver's base for its bounded index range. Nothing is extracted to disk
and nothing is executed.
