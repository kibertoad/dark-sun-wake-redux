---
id: FND-EXE-195
title: Controlled shipped-file literal search finds no stored dword equal to the callback setter address
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0060068B..0x00600699
tool: ReportPhysicalBytePattern.ps1 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The fingerprinted shipped interpreter has 3802624 bytes. A search at
every possible four-byte start across that complete physical file found
no contiguous little-endian dword equal to `0x005FD170`, the setter
identified in FND-EXE-193. This is a byte-pattern observation, not an
instruction or pointer-use interpretation.

Two independently identified stored-target controls each matched once.
The dword `0x006020C0` occurs at file offset 2095758 and
`0x00600520` at file offset 2095765. Bounded instruction reading identifies
them as the immediate operands of the record +4 store at `0x0060068B`
and record +8 store at `0x00600692`, respectively, already described
by FND-EXE-190. Both controls exercise the contiguous four-byte
absolute-target representation searched here. The 128-match limit was
not reached in any of the three searches.

## Interpretation

Together with FND-EXE-194's decoded and physical direct-transfer searches,
this removes another specific shipped-file representation from the
setter-caller investigation. It does not prove the setter is unreachable
or has no callers. Q-EXE-001 retains relative or relocated pointer forms,
split/encoded addresses, targets calculated from other values, interior
entry points, runtime-created pointers and code, and indirect state
writers. Native pointer validity, callback effects and record lifetime
are not established by literal searches. No complete reading follows.

## Alternatives

An analyzer reference list could omit an undecoded stored dword; this
physical search does not depend on that list or function boundaries.
Conversely, a target absent in its contiguous absolute representation
can still be constructed or reached in another representation. The
two controls validate this search kind only, not those excluded kinds.

## How to reproduce

First verify shipped interpreter XXH3-128
`09861838aa3018346f9f15c9a4f5925c` with the pinned executable reader's
sourceXxh3 API. Run the committed ReportPhysicalBytePattern.ps1 with
SourcePath naming that shipped file and MaximumMatches 128. Derive
the four-byte Pattern by little-endian encoding of each named dword:
query `0x005FD170`, controls `0x006020C0` and `0x00600520`. The tool
scans every start with four remaining bytes, including unaligned starts,
headers, section contents and trailing physical bytes; it does not map
or decode them. Missing controls or an exceeded match limit would make
the negative query unusable.

In the saved PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x00600687`, count five, restricting the
control interpretation to `0x0060068B..0x00600699`. Keep rich reports
in the local licensed-source store, outside Git. No original-game
execution is involved.
