---
id: FND-EXE-510
title: Game startup chooses priority entries and marks each before near or far dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050994..0x000509BE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00050878..0x000508A2
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-509's startup calls near 0220 with ES loaded from saved
CS:02C4, SI 3994 installed or 3908 on disc, and DI
39BE installed or 3932 on disc. The helper initializes priority word
AX to 0100, candidate offset DX to DI and scan offset BX to
SI. It advances BX by six at word width until BX equals DI.
Each entry whose ES byte zero is FF is skipped. Every other entry's
byte one is zero-extended and compared unsigned with current AX. Only a
strictly smaller value replaces AX and candidate DX. Equal priorities keep
the earlier selected entry. There is no extent check or iteration bound
other than the scan-offset equality.

If candidate DX remains equal to DI, the helper returns near without
incoming cleanup. Otherwise it loads BX from DX, compares ES byte zero
with zero and stores FF into that byte before dispatch. The store and
the following ES push preserve the comparison flags. Original byte zero
selects a near indirect call through ES word two; every other non-FF
value selects a far indirect call through ES words two and four. After
either call it pops ES and jumps to the helper's beginning. It does not
save SI or DI around the calls or test any returned result.

Under admitted stable bounds, segments and callee preservation, the shipped
42-byte table contains seven six-byte entries. The static entry fields are:

| Index | Selector byte | Priority byte | Installed target offset | Disc target offset | Installed relative segment | Disc relative segment |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | 01 | 01 | 0D27 | 0D26 | 3AE5 | 3AD6 |
| 1 | 00 | 02 | 0886 | 0886 | 0000 | 0000 |
| 2 | 00 | 10 | 11C1 | 11C1 | 0000 | 0000 |
| 3 | 01 | 10 | 12D9 | 12D9 | 0000 | 0000 |
| 4 | 00 | 10 | 2877 | 2877 | 0000 | 0000 |
| 5 | 00 | 10 | 29AA | 29AA | 0000 | 0000 |
| 6 | 01 | 1E | 3B56 | 3B56 | 0000 | 0000 |

The segment words of entries zero, three and six have MZ relocation
records. Installed relocation indices are 4445, 4457 and 4473 at
shipped words 50998, 509AA and 509BC. Disc indices are 4432,
4444 and 4460 at words 5087C, 5088E and 508A0. With
modeled load segment 1000, the first far target therefore becomes
4AE5:0D27 installed or 4AD6:0D26 on disc, and the other
two far targets become 1000:12D9 and 1000:3B56. Near targets
use current CS and ignore the fourth word. The zero relative segment
in a far entry does not mean a zero loaded segment.

If callees preserve SI/DI, ES stack restoration and table contents except
the marks, priority order follows the table order above, including the four
priority-10 entries. These conditions are not established by reading this
dispatcher. A callee can change bounds, marks, priorities, targets or segment
state, and the next scan reads them again. Marking before dispatch also
precedes any callee failure or nonreturn; there is no local rollback.

## Interpretation

This identifies concrete startup callees that stand between early segment
setup and the later game diagnostic. Q-EXE-007 retains their bodies and
returned preservation contracts, actual segment/table admission, writable aliases,
runtime-created targets, later record writers and broader launch coverage. The
bounded dispatcher and shipped table do not establish a complete startup
reading or exclude launches within its callees.

## Alternatives

Treating equal priorities as arbitrary ordering ignores the strict replacement
comparison. Treating every zero segment word as a null target ignores MZ
relocation and the separate near/far selector. Treating the marked entries as
successful calls ignores the pre-call store. Treating seven shipped entries as
a runtime bound ignores unproved SI/DI preservation and later table writers.

## How to reproduce

At revision 8dbf1be require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
5420..5464 in sixteen-bit mode. Track six-byte stepping, unsigned priority
comparison, preserved zero-test flags, pre-call mark and ES-only save/restore.
Read seven records as byte, byte, little-endian word, little-endian word at
50994 installed and 50878 on disc. Scan every MZ relocation record
from the header's relocation-table offset and count; compare its mapped shipped
word with each entry's segment word and verify the indices above. Apply
load-segment addition only to relocated words. Licensed bytes stay outside Git;
no game process, DOSBox or emulated call runs.
