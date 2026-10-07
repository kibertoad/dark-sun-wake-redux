---
id: FND-EXE-013
title: Compiled command dispatcher selects the GOTO record and preserves its two-word call target
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00599120..0x005992BB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005904B0..0x00590563
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003157D0..0x003158E8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00338190..0x003382A6
tool: Ghidra 12.1.3 PUBLIC, pinned bounded reporters, Node.js 24.21.0
environment: null
---

## Observation

The command-record consumer is at `0x00599120`. After the stack probe
already read in FND-EXE-012, its incoming object and line arguments occupy
post-probe stack offsets `0x1040` and `0x1044`. It saves the object in a
local field and sends the line to `0x0058F3A0`. The following description is
conditional on that helper returning a valid NUL-terminated line pointer.

The command token ends at space, slash, tab, equals or NUL. A dot or
backslash first triggers a lookup of the prefix accumulated so far; if no
record matches, that byte becomes part of the token and scanning continues.
An empty completed token returns. The ordinary nonempty token lookup starts
at `0x00716BD0`, compares each name through the imported comparison thunk
identified in FND-EXE-012, tests the full 32-bit result, and advances by 20
bytes on mismatch. Each next name is tested for zero before comparison.
The prefix lookup uses the same table, stride, result width and guard.

The complete table prefix through GOTO contains, in order, DIR, CHDIR,
ATTRIB, CALL, CD, CHOICE, CLS, COPY, DEL, DELETE, ERASE, ECHO, EXIT and
GOTO. Each name pointer is nonzero; the pointed names were read through
NUL, not inferred from record order in SRC-DOSBOX-GOG-0742. Consequently a
comparison matching GOTO after these preceding mismatches reaches record
13, `0x00716CD4`, whose target is recorded in FND-EXE-011.

At the shared match continuation, the dispatcher reads both 32-bit call
words: the first from record offset 8 and the second from offset 12. A
clear low bit in the first word uses that word directly as the target. A
set low bit instead dereferences a pointer at the incoming object plus the
second word, then obtains the target from that pointer plus the first word
minus 1. Both branches pass the incoming object plus the second word as
first argument and the current unconsumed line pointer as second argument.
The indirect call is at `0x0059928C`. The GOTO record has first word
`0x00597BA0` and second word zero: it selects the direct target, passes the
same object and passes the suffix beginning at the token delimiter.

On a nonempty token with no matching record, the dispatcher instead calls
`0x00593BE0` with object, copied token and unconsumed suffix. Its low-byte
result gates a second fallback at `0x00598D40`, whose low-byte result gates
a diagnostic route. FND-EXE-014 reads a conditional branch in the first
fallback; neither fallback is established as a whole here.

There is also a direct parser continuation at `0x0059053E` calling this
dispatcher with its retained object and the pointer returned by its own
trim-helper call. This continuation follows `0x005902A0`: the inspected
path admits a result at most 1 by unsigned comparison and requires both
32-bit output fields at local offsets `0x2C` and `0x30` to be zero. The
helper's input transformation and outputs were not fully read. This is a
positive input path, not complete caller or parser coverage.

## Interpretation

The record found in FND-EXE-011 is consumed by compiled command selection,
not merely present as unused data. Under the stated pointer and comparison
conditions, GOTO selects its recorded target without object adjustment.
The target then retains a trailing colon as FND-EXE-011 describes. The
source-led dispatch reading has direct compiled support for this path;
complete batch-input production and external effects remain Q-EXE-006.

## Alternatives

For this record, a hidden nonzero object adjustment or virtual target is
ruled out by the actual second word and low-bit test. Matching by fixed
record number alone is ruled out by the name comparisons and per-record
zero guards. This does not establish a total table extent, every command's
handler, all callers, safe token capacity for every input, or that an
unread helper preserves the assumed line. No negative caller claim is made.

## How to reproduce

Verify the file identity and preferred PE base from FND-EXE-011. In the
saved static project, search mapped bytes for little-endian pointer value
`0x00716BD0` using `ReportBytePattern.java` query `d06b7100`, with its
100-hit and 20-reference-per-hit limits. Recorded positive text hits were
`0x00597A3F`, `0x00597A65`, `0x00597A95`, `0x00597B06`, `0x00597B2A`,
`0x00597B5B`, `0x00599189`, `0x005991A6`, `0x005991BF`, `0x005991E1`,
`0x005991F6` and `0x0059920B`. The first group is not this dispatcher;
inspect the second group rather than assigning meaning from the pattern.
The automatic analysis timeout left those instructions unowned.

Read 256 bytes at `0x00599060` with `ReportDataBytes.java`; identify the
aligned prologue at `0x00599120` before decoding the hits. Recover that
entry with `RecoverCitedFunctions.java`, summarize it, and read a
160-instruction window from its start. Interpret only
`0x00599120..0x005992BB`, with exclusive end, not the later unrelated
instructions printed after a gap. Verify both call words and the actual
argument writes, not just the decompiler's inferred member-pointer type.

Read table data at `0x00716BD0` (256 bytes) and `0x00716CD0` (48 bytes),
and pointed name data at `0x0073AF90` (256 bytes) and `0x0073B090` (24
bytes). Decode exactly the first 14 records and each name through NUL.
Independently search the shipped file for the 20-byte first record made
from little-endian words `0x0073AF90`, 0, `0x0059A120`, 0,
`0x0073AF94`; the recorded match was `0x003157D0`. Search the ASCII
DIR/NUL followed by SHELL_CMD_DIR_HELP/NUL and GOTO/NUL; recorded offsets
were `0x00338190` and `0x003382A1`. Use
`ReportPhysicalBytePattern.ps1` with maximum 16 matches for each query.
Inspect the parser window at `0x005904B0` (120 instructions), stopping this
claim at `0x00590563`. Rich reports stay local; execute no original program.
