---
id: FND-EXE-016
title: Compiled count-80 PATH continuation scans for a semicolon without a NUL stop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593645..0x00593706
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction reporter
environment: null
---

## Observation

This is a bounded branch within the lookup in FND-EXE-015, not an observed
runtime failure. The segment-copy counter is reset to zero before the copy
loop. Each copied byte advances the retained input cursor and increases the
32-bit counter. The ordinary copy continuation tests the next byte for
both NUL and semicolon and stops copying when the count exceeds 79.

At `0x00593687` the count is compared with 80. Equality selects
`0x005936E4`; a different count writes NUL at the copied-count index and
continues to the segment-length checks. The count-80 branch instead reads
the retained cursor and compares its byte only with semicolon. While it is
not a semicolon, it increments the cursor and repeats that same comparison
at `0x005936F5`, branching back at `0x005936F9`. There is no call,
additional byte test or other exit in this short loop.

Consequently, if exactly 80 bytes have been copied and the retained cursor
now points at NUL, that byte fails the semicolon test and the loop advances
past it. The normal loop's NUL test does not protect this separate path.
If a semicolon is reached, the branch writes NUL at local buffer offset
`0xEF`, which is copied-byte index 79 relative to its start at `0xA0`,
then enters the segment-length continuation at `0x00593695`. It does not
append a terminator at index 80 on this path.

## Interpretation

The compiled interpreter corroborates the same bounded scan edge in the
bundled source lead. A termination argument based only on the environment
string's NUL terminator is insufficient for the count-80 branch. This
finding does not establish that supported launch inputs can reach that
branch, the following memory bytes, a crash, a hang, or the eventual
selected command. Those input and external-effect questions remain part
of Q-EXE-009.

## Alternatives

Stopping this particular loop at NUL, or reusing the ordinary copy loop's
NUL guard, is ruled out by its complete local branch sequence. A long
segment followed by a semicolon is compatible with the scan's stopping
condition. A terminal long segment has no stopping guarantee from its NUL
alone; no claim about the bytes beyond it is made.

## How to reproduce

Use the verified file and PE base from FND-EXE-011. Read the lookup window
starting at `0x005933F0` (160 instructions) through its counter reset and
ordinary copy guards, then `0x00593660` (200) through the count-80 split.
Independently reread `0x005936E4` with a 12-instruction window. Interpret
only the location above, excluding the later function instructions after
any gap. Trace the cursor's stored increment and both equality outcomes;
check the direct path from the ordinary NUL stop with count 80 into this
branch. Distinguish index 79 from count 80 using the observed buffer start
and terminator-store offsets. These are static path checks, not an executed
synthetic or native experiment. Keep the report local and launch nothing.
