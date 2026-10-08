---
id: FND-EXE-206
title: Shared numeric operand search reproduces the corrected gate-neighbor domain with independent reference-kind controls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0060086B..0x00600870
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCD56..0x005FCD5B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCE58..0x005FCE5E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B861B..0x005B8622
tool: scientific-method-engine 13.5.0 ReportScalarConstants and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-202's first padded text query omitted known immediate and
indexed operands. The installed shared numeric scalar reporter was
run for every integer from `0x0242C900` through `0x0242C9FF`, inclusive,
plus independent control values `0x02427E40` and `0x02427E50`, with
both operand kinds enabled. It completed with 46 operand matches,
below its 300-match cap, and explicitly reported that it covered
every disassembled instruction. Function ownership did not select
which instructions were searched.

Controls recovered the immediate address-taking instruction at
`0x005FCD56` from FND-EXE-075, the bitmap read/write family from
FND-EXE-025/073, and the indexed displacement at `0x005B861B`
from FND-EXE-202. Both padded absolute-memory display and unpadded
immediate/indexed display forms matched by numeric value. The reporter
labels memory and immediate operands; operand direction and indexed
store admission still come from the bounded readings, not those labels.

Mechanically comparing unique instruction addresses gives 46 for
this numeric report and 46 for FND-EXE-202's corrected unpadded text
report, with no address difference. The numeric report contains 17
exact gate-value operands, all in the known full-word load family
recorded by FND-EXE-076/077 and FND-EXE-200. It supplies no additional
decoded gate store or address-taking candidate within this queried
value set. FND-EXE-204/205 classify the other fixed publications;
FND-EXE-202/203 retain the indexed publication's preservation boundary.

## Interpretation

The shared reporter provides a tested numeric query for this domain
without requiring a new local reporter or reliance on hexadecimal
padding. Q-EXE-009 retains computed and encoded addresses, bases
outside the queried interval, stores through pointers, symbolic forms
not represented as numeric operand objects, undecoded or runtime-created
instructions, external writers and actual gate initialization. A
matching numeric base is not an effective-address or storage-alias
proof. No complete writer set or complete reading follows.

## Alternatives

The failed padded text control cannot exclude immediate or indexed
references. Conversely, numeric operand matching does not recover an
address that is constructed from other values or written indirectly.
An immediate/memory label does not establish whether an operand is
read, written or used only to form a pointer. The agreement between
two decoded-listing queries does not validate undecoded file bytes.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run the installed
ReportScalarConstants with all 256 integers in the inclusive interval
`0x0242C900..0x0242C9FF` explicitly enumerated as separate arguments,
then the two controls `0x02427E40` and `0x02427E50`. Do not supply an
operand-kind filter. The installed script's cap is 300 matches.
Require the completion marker and all control kinds, not exit status
alone. Keep numeric matching separate from the reported classification.

Repeat ReportInstructionText with `242c9`, `2427e40` and `2427e50`,
combined cap 256. Extract the instruction address from each match,
sort unique addresses, and compare the sets. Both sets have 46 addresses
and their difference is empty; the numeric gate value has 17 matches.
Use FND-EXE-075, FND-EXE-025/073 and FND-EXE-202 for independently
located controls and their operand readings. Keep rich reports local
and execute no original program.
