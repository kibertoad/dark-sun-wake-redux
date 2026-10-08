---
id: FND-EXE-210
title: Ordinary record metadata prefixes take distinct finite bypass paths through the metadata reader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EDC68..0x002EDC6D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EEAD3..0x002EEAD7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4EA0..0x005F4F78
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4D30..0x005F4D66
tool: executable-reader 2.4.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-209's first metadata address `0x006EE868` maps to shipped
file offset `0x002EDC68`. Its five-byte prefix supplies first marker 255,
second marker zero, terminating offset payload thirteen, third marker one
and terminating offset payload four. These are the same prefix values
as FND-EXE-197's distinct source address, not the same storage.

The second metadata address `0x006EF6D3` maps to shipped file offset
`0x002EEAD3`. Its four-byte prefix supplies first marker 255, second
marker 255, third marker one and terminating offset payload two. The
offset byte has its continuation bit clear. Neither bounded prefix reading
establishes the contents or meaning of the following stream.

Compose these inputs with FND-EXE-060, FND-EXE-092 and FND-EXE-059,
requiring unchanged source, valid disjoint output and stack storage, admitted
context and ordinary callee/frame preservation. Let B be the selected
metadata address. Both paths first store zero at output offset zero,
including after the optional constant-zero context query, and first marker
255 stores zero at output offset four without modifier or typed decoding.

For the first source, second marker zero is stored at output offset 20.
The first offset decoder reads one terminating byte, writes thirteen to
its local output and returns B +3. The caller stores B +16 at output
offset twelve but keeps B +3 as its parsing cursor. Third marker one is
stored at output offset 21. The second decoder reads one terminating
byte, writes four and returns B +5. The caller stores B +9 at output
offset sixteen and returns B +5. Its source read extent is B..B +5.

For the second source, second marker 255 is stored at output offset 20
and selects the bypass: output offset twelve receives zero without the
first offset decoder. Third marker one is read at B +2 and stored at
output offset 21. The remaining decoder reads the terminating byte at
B +3, writes two to its local output and returns B +4. The caller
stores B +6 at output offset sixteen and returns B +4. Its source
read extent is B..B +4. Neither calculated target is dereferenced here.

All cursor and target additions use 32-bit width. Both paths directly
initialize full words at output offsets zero, four, twelve and sixteen,
and only individual bytes at twenty and twenty-one. They do not directly
initialize offset eight or the adjacent bytes of the marker fields.
Each invoked offset decoder terminates after one byte on these unchanged
inputs; neither path enters typed-reader dispatch or a continuation loop.

## Interpretation

This closes the conditional metadata-prefix read extent for both concrete
ordinary producers, while distinguishing their returned cursors and outputs.
Q-EXE-009 retains actual record selection, source and field preservation,
segment identity, output admission, aliases and downstream stream/target
consumption. A finite prefix read is not a complete metadata schema or
complete reading of the selected-record lifecycle.

## Alternatives

Applying the first source's five-byte path to the second would incorrectly
invoke an offset decoder skipped by its second marker. Treating a calculated
target as the returned parsing cursor would likewise change both paths.
Equal first-source and nested-prefix values do not establish storage identity
or admit either source as the runtime reader's input.

## How to reproduce

Hash-check the shipped interpreter against XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Use the pinned PE32 section
mapping to map `0x006EE868` and `0x006EF6D3` to the respective cited
file offsets and inspect exactly five and four source bytes. In the saved
project, read-only with analysis disabled, run ReportDataBytes at both
addresses, count sixteen each, restricting interpretation to those prefixes.
Compose FND-EXE-060/092's caller and FND-EXE-059's decoder readings:
track both independent bypass markers, local output last writers, cursor
retention and target calculation separately. Retain the unchanged-source,
disjoint-output and callee conditions; inspect no downstream target by
inference from its displacement. Keep rich reports in the licensed-source
store, outside Git. Execute no original program.
