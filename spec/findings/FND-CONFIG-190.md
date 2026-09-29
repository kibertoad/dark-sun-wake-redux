---
id: FND-CONFIG-190
title: The frame dimension readers return FFFF for an unsigned index failure and otherwise read unbounded frame words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:76C2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:771C
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit readings and header-derived MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-189's admitted nonnull buffer paths pass a
far pointer and word index to resident 1BF3:76C2 and
771C, retaining the returned words and requiring each
to be signed-at-least one. The complete reader spans are
`0x000187F2..0x0001884C` and
`0x0001884C..0x000188A7`. Their arguments are far
source BP+6 and word index BP+0A. Both save DS,
SI and DI and restore them at their normal local return;
neither calls another function, performs port I/O or has
an own pointer-null check.

Each loads DS:SI from the supplied far pointer and
compares the index unsigned with word source+4. Index
greater than or equal to that word returns AX FFFF.
Other indices form a word-wrapped four-times index and
read a double word from source offset plus that result
plus six. The layout corresponds to frame_count and
frame_offsets in supported FMT-IMAGE-001. The code
does not read that layout's resource size field, test a
resource tag or independently bound the table's storage.
Count zero rejects every word index locally; a nonnull
source alone does not prove its header or table valid.

The admitted frame-offset calculation forms a double-word
linear sum of source segment times sixteen, source offset
and the selected table double word, including its word
carry operations. It keeps the sum's low four bits as
SI and assigns DS from the low word of the sum shifted
right four. That representation discards higher segment
bits; there is no separate out-of-range or overflow result.
Table-index and effective-address arithmetic use word
operations. Valid terminated storage, table ranges, offsets
and any native address aliasing remain input conditions.
This is a pointer calculation, not a checked file reader.

76C2 reads the word at the resulting frame pointer;
771C reads its word at +2. Those fields correspond to
width and height in FMT-IMAGE-002. Both are returned
unchanged in AX. A valid-index path sets carry before a
shared return selection, while the invalid-index comparison
leaves carry clear; that distinguishes the frame-word read
from the FFFF result. There is no own signed-positive,
320-by-200, frame-body or capacity test, and no other
own memory write. Native contents and extent validity are
not established by an admitted index.

For valid synthetic storage containing one frame, index
zero returns its raw width or height, while index one
returns FFFF. For a raw dimension word zero, or a
word from 8000 through FFFF hexadecimal, the signed
consumer gate in FND-CONFIG-189 rejects it. Words
one through 7FFF pass that initial consumer gate, before
its later signed/wrapped coordinate adjustment. These are
conditional static cases, not an observed invalid resource
or an assertion that every admitted size can render.
The image format's measured shipped ranges are separate
from a runtime pointer's validity and actual selected bytes.

## Interpretation

The active buffer path has a concrete unsigned frame-index
check and a later signed dimension check, but the dimension
helpers themselves read raw memory without resource bounds.
FFFF from index failure reaches the consumer's early exit;
other raw words can take the same signed-rejection route.
Their normal DS/SI/DI restoration narrows the reader's
own register effects without proving pointer, resource,
primitive or end-to-end service validity.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
source/index/header/table/frame and segment producers,
valid capacities and offsets, aliases, accepted transfers,
subsequent clipping/handle requests and actual native results.
One reading supplies a valid supported image and in-range
index; another supplies an index failure or stale/invalid
memory. Complete producer and accepted-content evidence would
distinguish their reachable outcomes. A file format's observed
ranges do not prove the runtime pointer names that resource.

A reading that every nonnull source yields dimensions is
ruled out by the unsigned count gate. A reading that the
helper bounds every selected frame by resource size is ruled
out by its absent size-field read and capacity argument.
The claimed width/height field correspondence is supported
by FMT-IMAGE-001 and FMT-IMAGE-002, without changing
those entries or expanding this CONFIG/SCRIPT goal's scope.
No native or emulated outcome is claimed.

## How to reproduce

Read 1BF3:76C2 through 771B and 771C through
7776 from their entries. Track BP+6/+0A, the unsigned
count comparison, word-wrapped table index, double-word
linear addition and normalized segment/offset representation.
Check the carry-preserving FFFF/shared selection and exact
frame word/+2 reads, plus saved DS/SI/DI restoration.
Compare the supported image layouts, then follow the signed
at-least-one consumer gates in FND-CONFIG-189. Derive
one-frame, count-zero, index-equality and signed-word cases
under explicit valid storage and extent conditions, without
assuming accepted content or rendered outcome.
