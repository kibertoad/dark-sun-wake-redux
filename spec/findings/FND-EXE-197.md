---
id: FND-EXE-197
title: The nested record's shipped metadata prefix gives a five-byte conditional reader path
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
    offset: 0x002EDC24..0x002EDC29
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

FND-EXE-165 writes `0x006EE824` to its nested record's offset-28 field.
The fingerprinted shipped PE maps that address to file offset 3071012,
`0x002EDC24`. Its first five bytes supply a first-marker bypass value
255, second marker zero, a terminating offset payload 13, third marker
one, and a terminating offset payload four. The two offset bytes have
their continuation bit clear. These are bounded data observations, not
proof that the selected record or runtime source retains them.

Compose those inputs with FND-EXE-060, FND-EXE-092 and FND-EXE-059,
assuming the reader receives this address as cursor B, valid output
storage disjoint from the source and its stack, unchanged input bytes,
admitted context and ordinary callee/frame preservation. The initial
output-zero store and optional constant-zero context query occur first.
First marker 255 then stores zero at output +4 without the modifier or
typed reader. Second marker zero is stored at output +20 and selects
the first offset decode. That decoder reads one byte, returns cursor
B +3 and writes value 13 to its output local. The metadata reader stores
B +16 at output +12, retaining B +3 as its parsing cursor.

The third marker one is stored at output +21. Its following decoder
reads one byte, returns B +5 and writes value four to its output local.
The metadata reader stores B +9 at output +16 and returns B +5. All
address additions use 32-bit width. Its direct full-word outputs on this
path are zero at offsets zero and four, B +16 at twelve, and B +9 at
sixteen. Byte outputs at twenty and twenty-one are zero and one. There
is no direct output +8 store or initialization of the bytes adjacent to
the two markers. The source read interval is exactly B..B +5; neither
calculated target is dereferenced on this path.

Under those conditions each decoder reads one terminating byte, so this
particular path needs no typed-reader dispatch and no continuation-byte
loop re-entry. This bounds this prefix's consumption, not the later
matching loop or the extent and meaning of either calculated target.
FND-EXE-196 separately records the caller's returned-cursor retention
and its later marker-byte use.

## Interpretation

This supplies concrete source values behind one offset-28 writer and a
finite conditional read extent for that source. Q-EXE-009 retains selected
record identity, field/source writers, setup preservation, DS/SS and pointer
admission, aliases, lifetime and downstream target/cursor consumers. The
published address does not prove this nested record is selected, nor that
every selected record has these bytes. No complete metadata schema or
complete_reading declaration follows.

## Alternatives

Treating the first bypass marker as the end of the whole input would omit
four subsequent reads. Advancing the parser to B +16 or B +9 would conflate
stored relative targets with the returned cursor. Conversely, proving the
five-byte source prefix does not admit either target's storage or the
runtime record that exposes this pointer.

## How to reproduce

Verify shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Use the pinned PE32 section mapping to map `0x006EE824` to file offset
3071012; inspect exactly five source bytes for the marker and terminating
payload facts above. In the saved project, read-only with analysis disabled,
ReportDataBytes at `0x006EE824`, count 16, supplies a bounded cross-check;
restrict this finding's data interpretation to its first five bytes and
retain no raw payload report in Git.

Repeat FND-EXE-060/092's reader windows and FND-EXE-059's decoder reading.
ReportInstructionWindow at `0x005F4F40`, count 22, independently checks
the bypass marker store, cursor advancement and common final-decode path.
Follow both one-byte terminating decodes, each output-local last writer,
the source cursor retained apart from the calculated targets and the
ordinary restoration/return through the cited end. The condition that
source bytes are unchanged and stores do not alias later reads is explicit,
not established by the file mapping. Keep rich reports in the local
licensed-source store and execute no original program.
