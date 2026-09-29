---
id: FND-CONFIG-133
title: The rest caller iterator selects negative-word records and has a separate stored-index branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0776
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0020
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and FBOV fixup mapping
environment: null
---

## Observation

Resident 2D40:0776 begins at file offset `0x00022D76` and
returns at `0x00022DDD`. It receives one word at BP+06 and
initializes the result to sentinel 9999. Input 9999 starts at
index zero; any other input is incremented as a 16-bit word to
form the starting index. The starting index is retained separately
from the scan index.

While the scan index is signed less than four and the result is
still 9999, the routine selects a 49-byte record through DS:19C9.
A signed-negative word at that record's offset six selects the
current index as the result; a nonnegative word skips it. The scan
index increments after either comparison. With input 9999 or
zero through two, this selects the first qualifying record from
the starting index through three, or returns 9999 if none qualifies.
There is no lower-bound or valid-index check for other inputs.

After this scan, a separate branch tests whether the retained
starting index equals four. Only then, and only when byte
4F49:000A is nonzero, it reads DS:1A32 as an index into a
three-byte-stride table at 4F49:0C33. A nonzero byte there
replaces the result with DS:1A32. No range or signedness check
is applied to that returned stored index. Both segment operands
are declared MZ relocations: file offsets `0x00022DB5` and
`0x00022DC9` hold raw 3F49, mapped to 4F49 with load segment
1000. The complete routine has no call and no stored-data write.

For ordinary inputs zero through three, the special branch is
therefore reachable only from input three. Starting at sentinel
9999 and finding no qualifying record does not reach that branch:
the retained starting index remains zero even after scanning four
records. A returned index three followed by another call reaches
the special branch; its failed byte checks return 9999.

Overlay 204 entry 5787:0020 supplies sentinel 9999 before its
first pass at `0x0008CAD3`, entering the call at `0x0008CD0E`.
Its first-pass continuation pushes SI before that same call and
sets SI from the returned AX. Sentinel ends the first pass.
It supplies sentinel again at `0x0008CD1F`, entering the
second-pass call at `0x0008D040`. The continuation at
`0x0008D03F` pushes SI before that call; its returned AX becomes
SI, and sentinel ends the second pass. Both call sites
have declared FBOV fixups naming descriptor 25 and mapped
resident segment 2D40. FND-CONFIG-132 describes the work between
the calls; this reading does not inventory all other iterator callers.

## Interpretation

The ordinary scan is a bounded search of record words, rather
than an unconditional enumeration of four indices. Its special
stored-index return is a distinct path whose eligibility includes
the previous input being three. The caller's two passes can select
different records if their stored words or the special-branch
inputs change between calls.

The iterator's own body does not change those inputs. Nevertheless,
its arbitrary stored-index return does not prove that the caller's
iterations terminate: a stored index of three with both byte checks
still passing returns three again on the next identical call.
Other stored indices can change which ordinary scan follows.
This is a conditional reading of the routine, not evidence that the
shipped game reaches a repeated index or hangs.

## Alternatives

FND-CONFIG-134 reads one stored-index assignment and table clear;
FND-CONFIG-135 reads a registered flag producer and another clear.
Q-CONFIG-008 retains their upstream paths, later or other writes,
allowed values, the 49-byte record's offset-six word producers,
intervening helper effects and reachable inputs.
They may rule out the conditional repeated-index case; no range
invariant has been established here. Assigning these records a
party, actor or other gameplay identity needs separate evidence.
A scan with no match and a special-branch return are not equivalent:
the retained starting-index check distinguishes them directly.

## How to reproduce

Read the complete bounded routine `0x00022D76..0x00022DDE`.
Track the retained starting index separately from the incremented
scan index, the signed word comparison and signed upper-bound
check, sentinel result, two byte checks and unbounded stored-index
return. Check the two listed segment operands against the MZ
relocation list before applying the resident load segment.
For the caller, read `0x0008CAD3..0x0008CAD9`,
`0x0008CD0D..0x0008CD25` and
`0x0008D03F..0x0008D051`; verify the declared FBOV fixups
and descriptor resolution. Compare FND-CONFIG-132 for the
first- and second-pass processing blocks without assuming their
unread helpers preserve the iterator's inputs.
