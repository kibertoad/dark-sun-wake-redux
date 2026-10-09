---
id: FND-EXE-508
title: Both game editions ship the diagnostic record with handle one and line-flush and failure-bypass flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050692..0x000506A2
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00050576..0x00050586
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000507C2..0x000507CE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000506A6..0x000506B2
tool: Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-499's diagnostic passes record offset 3692 installed or 3606
on disc. Under modeled DS 57E0 installed or 57D7 on disc, the
sixteen shipped bytes at that offset are at file 50692 or 50576.
Read as eight little-endian words, they have the following values:

| Record byte offset | Installed | Disc |
| --- | --- | --- |
| 0 | 0000 | 0000 |
| 2 | 020A | 020A |
| 4 | 0001 | 0001 |
| 6 | 0000 | 0000 |
| 8 | 0000 | 0000 |
| 10 | 0000 | 0000 |
| 12 | 0000 | 0000 |
| 14 | 3692 | 3606 |

The corresponding shipped limit word at installed 507C2 or disc 506A6
is 0014. The first five adjacent little-endian flag words are 6001,
6002, 6002, A004 and A002 in both editions. Under the modeled
segments these are DS:37C2/37C4 installed or DS:3736/3738 on
disc, the limit and table used by FND-EXE-500, FND-EXE-502 and
FND-EXE-504. These reads establish shipped values only, not later writers
or an initialized runtime table extent.

## Interpretation

If the diagnostic reaches this record with these bytes unchanged and the
modeled DS admitted, its flags include 0002, 0008 and 0200;
record byte four is one, word six is zero and word fourteen equals
the record offset. FND-EXE-500's flag-eight counted-byte route therefore
applies to this conditional state. Its character helper uses the slow path,
admits flag 0002 and bypasses a failed or short write when flag 0200
remains set. The handle-one table word 6002 has bit 0800 clear,
so its optional positioning call is not selected in this conditional state.
These implications rely on the recorded code, not on inferred field names.

The bytes do not prove that DS has this value at the diagnostic, that
the record and table remain unchanged, that aliases cannot modify them or
that native output succeeds. Q-EXE-007 retains startup and intervening segment
writers, record/table writers and lifetime, admitted storage, native effects,
surrounding callers and broader game launch coverage. No runtime-state admission
or whole-game launch exclusion is claimed.

## Alternatives

Treating matching shipped records as live initialization ignores intervening
writers and segment identity. Treating flag 0200 as proof of a successful
write ignores its recorded failure bypass. Treating the observed first five
flag words as the complete runtime table ignores its producers and extent.

## How to reproduce

At revision ec14c78 require both DSUN.EXE identities from FND-EXE-350.
Read eight little-endian words at shipped 50692 installed and 50576
on disc; read six words at 507C2 installed and 506A6 on disc.
Separate each latter read into the limit and five following flag words.
Use header size 5200, modeled load segment 1000 and relative data
segments 47E0/47D7 to check the conditional segment-to-file mapping.
Compare each record word and the code consumers cited above. Licensed bytes
stay outside Git; no game process, DOSBox or emulated call runs.
