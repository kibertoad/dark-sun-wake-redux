---
id: FND-EXE-350
title: Shipped game and sound utility contain distinct literal batch-name leads
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
    offset: 0x0004D9FF..0x0004DA08
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004D961..0x0004D96A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD1F..0x0000FD2F
tool: PowerShell 7 physical byte-pattern reporter and executable-reader XXH3
environment: null
---

## Observation

Complete physical-byte searches used these manifest-identified sources:

| File | Length | XXH3-128 | Lowercase .bat offset |
|---|---|---|---|
| DSUN.EXE | 634416 | e296af55ba2ecde7e77f555c90f33d0b | 317956 |
| CD:DSUN.EXE | 634704 | 318cd5ec0559901add3780097162a919 | 317798 |
| SOUND_DS.EXE | 204593 | 236c2dc23c071eca421eb5b427caee57 | 64810 |

Each source has exactly one lowercase ASCII `.bat` occurrence and zero
occurrences of the other seven capitalization combinations of those three
letters. Exact ASCII `sound.bat` occurs once at decimal 317951 in the
installed game and once at 317793 in the disc game. Those nine-byte names
end at 317960 and 317802, respectively.

In the sound utility, the exact twelve-byte `autoexec.bat` occurs once
at decimal 64802. Direct inspection of the bounded source interval
64790..64815 shows a colon and backslash immediately before that name,
a zero byte before the colon, and a zero byte after the name. Thus the
shipped text supplies the suffix `:\autoexec.bat`; it does not supply
a literal drive letter immediately before its colon. Exact probes for
`c:\autoexec.bat` and `C:\autoexec.bat` return zero occurrences.

The independent MZ-header control finds ASCII MZ at offset zero in each
file. Its complete raw search additionally finds a match at decimal
262338 in the installed game and 262097 in the disc game, with no extra
match in the sound utility. These extra matches are unclassified bytes,
not claims of embedded executables. Every query completes within the
16384-match cap; no search is truncated.

## Interpretation

The searches provide concrete text targets for Q-EXE-007's executable
consumer reading. They do not establish that either executable launches
a batch helper: a filename in data may be displayed, used as a file path,
or passed to a launcher, and the consumers have not been read here.
The sound utility's zero byte does not establish a constructed drive
letter, its writer or an eventual initialized path. Those require direct
writer and consumer evidence.

The search covers contiguous physical ASCII suffixes only. Extensionless
names, split or encoded strings, runtime construction, commands obtained
from files or arguments, indirect targets and execution paths are excluded.
The CD sound utility has the same manifest length and hash as the installed
one; no separate disc-source read is claimed. Q-EXE-007 remains open and
FMT-EXE-006's status does not change.

## Alternatives

Treating a batch suffix as proof of process execution ignores unread
consumers. Assuming the utility contains a C-drive path conflicts with
the shipped zero byte and both exact-path probes. Treating absence of
other literal suffixes as absence of other launches ignores extensionless
and constructed names. Interpreting a raw MZ match as executable ownership
would require header, mapping and code evidence absent from this query.

## How to reproduce

At revision 495a686, verify each full source with the committed XXH3
helper against the table and build manifest. The disc-game local copy
must match CD:DSUN.EXE's identity, not the installed file's. Run
tools/ghidra/ReportPhysicalBytePattern.ps1 under PowerShell 7 with
MaximumMatches 16384. Search each complete interval 0..634416,
0..634704 or 0..204593, exclusive ends, for the four-byte pattern with
first byte 2E, second byte chosen from 42/62, third from 41/61 and
fourth from 54/74, all eight combinations. Use 4D 5A as the independently
known header control in each source.

For each game source, query 73 6F 75 6E 64 2E 62 61 74. For the utility,
query 61 75 74 6F 65 78 65 63 2E 62 61 74, then both probes whose
first byte is 63 or 43 followed by
3A 5C 61 75 74 6F 65 78 65 63 2E 62 61 74. Inspect only utility
file interval 64790..64815 for the immediate separators and terminators.
No original process, DOSBox or emulated call runs. Licensed-source reports
and byte context remain outside Git; no complete caller search is claimed.
