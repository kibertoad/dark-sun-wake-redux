---
id: FND-EXE-353
title: Sound utility prefix-byte wrapper stores returned DX into its stack record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:20DF..1000:21A9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0DEF..1000:0E0F
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-360's prefix-byte producer passes selector 0021 and the same
far stack pointer as input and output to 1000:20DF. This wrapper reserves
eight local bytes and passes their SS-relative address to 1000:0DEF,
using pushed CS before a near call. That helper writes incoming ES,
the return frame's CS, SS and incoming DS into four successive words.
It restores ES, SI and BP and far-returns. The captured CS is the
wrapper's pushed CS, not the prefix producer's earlier calling segment.

20DF removes the helper's four argument bytes and forwards fourteen
bytes to 1000:2110: selector, input far pointer, output far pointer and
its initialized local segment-record pointer. It removes those fourteen
bytes after the call, restores SP/BP and far-returns.

2110 saves SI, DI and DS and constructs a small instruction thunk in
SS-relative local storage. For selector 0021 the ordinary branch calls
that local thunk to execute interrupt 21 and return. Selectors 0025/0026
select a different local construction path; its special interrupt/stack
contract is not established by this selected-path reading.

Before the selected call, 2110 loads the segment-record pointer through
DS:SI, pushes its words zero and six, then loads the input-record pointer
through DS:SI. It loads AX, BX, CX and DX from successive input words
zero, two, four and six, and DI/SI from words ten/eight. It pops the
retained segment-record words into DS and ES, respectively. Thus the
record access segment and the segment registers supplied to the interrupt
have different producers. It calls the local thunk through an SS-relative
far pointer. No original code is executed by this research.

On ordinary continuation it pushes flags twice, SI, DS and ES. It loads
the segment-record pointer and stores the stacked ES and DS into its
words zero and six. It then loads the output-record pointer, pops SI into
word eight, and pops the two saved flags words into words fourteen and
twelve. It masks word twelve with one. It stores DI, DX, CX, BX and
AX into output words ten, six, four, two and zero, in that order, then
restores its saved DS. The flag-mask result controls a subsequent branch:
zero bypasses a call to 1000:04CE; nonzero passes AX through two pushes
and calls that helper. Its effects are not read here. Both local suffixes
restore DI, SI, SP and BP and far-return.

For FND-EXE-360's same input/output pointer, the output word-six store
targets SS-relative producer BP minus ten, because that record starts at
producer BP minus sixteen. The producer reads this word's low byte after
the wrapper returns, then adds 40 at byte width. Initially unwritten input
record fields are still read before the interrupt; these output stores do
not retrospectively initialize those incoming register values.

## Interpretation

This connects the producer's prefix-byte read to an explicit returned-DX
store. It does not identify the native returned byte as an admitted drive
character, prove a usable pathname or establish a launch operation.
The initialized segment record is separate from the partially initialized
input/output record. Local register and output stores do not establish
interrupt effects or preservation through its continuation and error helper.

Q-EXE-007 retains native interrupt-result admission, unwritten input-field
provenance, aliases and lifetime, 1000:04CE, the file-interface's deeper
callees and later pathname consumers. The special selector paths and
other callers are outside this selected-path evidence. No complete-reading
promotion, execution exclusion or native observation follows.

## Alternatives

Treating the prefix byte as merely an untouched local ignores the output
DX store. Treating every input word as initialized ignores the producer's
single recorded word write. Treating DS used to access a stack record
as the interrupt's DS ignores the retained segment-record pops. Treating
the wrapper's output as a verified native service result ignores the
unobserved interrupt and unread error continuation.

## How to reproduce

At revision c6b394d require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x000034DF..0x000035A9 at IP 20DF and
0x000021EF..0x0000220F at IP 0DEF, modeled CS 1000 and
MZ header size 1400. Use FND-EXE-360's selected selector 0021,
same input/output record and producer frame. Track the segment-record
argument, retained segment pops, output store ordering and final word-six
binding; leave special selectors and native interrupt effects explicit.
Original bytes and reports remain outside Git. No original process,
DOSBox, generated thunk or emulated call is executed.
