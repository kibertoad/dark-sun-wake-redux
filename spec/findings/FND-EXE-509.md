---
id: FND-EXE-509
title: Game startup saves its relocated data segment and later selects stack and zero-fill segments through separate producers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:00D2
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0000..1000:00D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:01B0..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00000014..0x00000018
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00000014..0x00000018
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000003E..0x00000042
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0000003E..0x00000042
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

Both MZ headers specify initial IP and relative CS zero. The first
relocation record has offset one and segment zero, identifying the startup
immediate word. That word is 47E0 installed or 47D7 on disc.
With modeled load segment 1000, relocation produces 57E0 or 57D7.
Startup loads this word into DX and saves DX through CS at offset
02C4 before requesting interrupt 21 with AH 30.

After that interrupt it reads word two through current DS into BP and
word 002C through current DS into BX, then sets DS from current DX.
It stores returned AX at DS:0092, ES at DS:0090, BX at
DS:008C and BP at DS:00A8 before calling near 01B0. Thus
the saved CS word predates the interrupt, while the DS assignment uses
post-interrupt DX. Their equality requires native preservation evidence.

The 01B0 helper pushes incoming DS, requests interrupt 21 with AX
3500, 3504, 3505 and 3506 in succession, and stores each
returned BX/ES pair through current DS at offsets 0074/0076,
0078/007A, 007C/007E and 0080/0082 respectively. It then
sets AX 2500, loads DX from CS, sets DS from DX, replaces
DX with 01A7 and requests interrupt 21. It pops DS and returns
near without incoming cleanup. Its explicit save/restore covers DS conditional
on an intact stack, but native preservation during the preceding stores and
other registers is not established by this local body.

Back in startup, ES is loaded from current DS:008C. A forward
zero-byte scan starts at ES:0000 with CX 7FFF and counts intervening
strings in BX until an adjacent zero is found. An exhausted count jumps
through 009C to 02AD. The continuation stores a transformed remaining
count at DS:008A and an eight-aligned word-width value formed from
twice BX plus eight at DS:008E. It then loads DX from current DS
and subtracts DX from held BP at word width.

It reads a requested word at DS:384E installed or DS:37C2 on
disc and raises values below 0200 to 0200, storing the replacement.
It adds A4EC installed or A436 on disc, then DS:37EC installed
or DS:3760 on disc, taking the 02AD route on carry from either
addition. The result is shifted right four and incremented; an unsigned
BP smaller than that result takes the same route. Subsequent zero-word
tests select either that quantity or the unsigned smaller of BP and 1000.
These are arithmetic and branch checks, not allocation-success evidence.

The selected quantity in DI plus current DX is stored at DS:00A0
and DS:00A4. Startup loads ES from DS:0090, forms BX by
subtracting that segment from the preceding sum, and requests interrupt 21
with AH 4A, holding DI on the stack across the request. It does
not locally test returned carry before restoring DI, shifting DI left by
current CL, disabling interrupts, loading SS from current DX, setting SP
from DI and enabling interrupts. This SS producer uses post-interrupt DX;
the earlier DS-to-DX copy does not alone admit DS equal to SS here.
The current CL, native effects and available stack extent remain unadmitted.

It then clears AX, loads ES from CS:02C4 and zero-fills forward
from ES:39C4 to before ES:A4EC installed, or ES:3938 to
before ES:A436 on disc. These numerical ranges exclude the shipped
diagnostic record and handle-table locations of FND-EXE-508 under its modeled
mapping. This selected zero-fill uses the saved segment rather than freshly
loading DS or SS. Other writers and later segments remain open.

## Interpretation

This follows startup's separate saved-segment, DS, SS and zero-fill producers.
It supplies a specific early writer exclusion for FND-EXE-508's data, but
does not prove the diagnostic sees unchanged shipped bytes. Q-EXE-007 retains
native register and frame preservation, actual startup memory admission, later
segment and record/table writers, storage extent and lifetime, surrounding callers
and broader launch coverage. No runtime DS/SS equality or complete startup
reading is claimed; the failure target and later continuation remain unread.

## Alternatives

Equating every data-segment use with the relocated immediate ignores the
interrupt before DS is assigned. Equating DS and SS ignores the later
native request before SS is loaded. Treating the zero-fill as a DS-based
write ignores its separately saved ES producer. Treating the AH 4A
request as a checked success ignores the absent carry test in this path.

## How to reproduce

At revision 229f499 require both DSUN.EXE identities from FND-EXE-350.
Read little-endian header IP/CS at shipped 0014 and the first relocation
pair at 003E. With header size 5200 and modeled load segment 1000,
decode shipped 5200..52D2 and 53B0..53F3 in sixteen-bit mode.
Follow the pre-interrupt CS store, post-interrupt DS assignment, vector-helper
stack save/restore, DS-to-DX copy, arithmetic/carry branches, AH 4A
request and post-request SS/SP assignment. Keep saved ES zero-fill distinct
from actual DS/SS and compare its bounds with FND-EXE-508's locations.
Licensed bytes stay outside Git; no game process, DOSBox or emulated call runs.
