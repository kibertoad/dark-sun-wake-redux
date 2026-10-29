---
id: FND-COMBAT-091
title: APFM text helpers copy a fixed inline 24-byte region below the parent link without setting the style word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0423..3CFA:046F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0586..3CFA:05D0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:00D6..3F96:012F
tool: tools/research/exec-census/resident_listing.py at 4cc1a32f; Capstone 5.0.9
environment: null
---

## Observation

FND-UI-045 identifies the APFM text operation 3F96:0097 passing its
looked-up frame advanced by 0C to 3CFA:0586 and 0423. The first call
supplies SS:BP-58 as output; the second supplies the text far pointer
from the original argument at BP+0E. The caller tests each helper's
returned AX for zero before continuing. FND-COMBAT-088 binds the same
0C advance used by the shared APFM setup. Offsets below are hexadecimal;
the fixed copy count is 24 decimal bytes (18 hexadecimal).

3CFA:0423 reserves 54 hexadecimal bytes of local storage and has a
stack-limit guard. It copies the source at its far argument BP+0A into
SS:BP-54 through 1000:406D. That primitive copies through a reached
zero without a destination-capacity argument (FND-CONFIG-181 and
FND-CONFIG-182). The helper then forms its first far argument plus 30,
sets DI zero, and copies one byte from the local buffer for each DI
from zero through 17. It returns AX zero after exactly those 24 own
destination writes. It does not write a separate terminator or clear the
remaining bytes of that local buffer before the fixed copy.

For the APFM call form, the destination is therefore frame+3C..frame+54,
an inline 24-byte region, not a stored far pointer to text. An admitted
source shorter than 24 bytes puts its reached terminator into the region;
bytes after it are copied from locals not explicitly initialized by this
helper. An admitted source of at least 24 bytes has only its first 24 bytes stored,
without a forced zero among them. These are static copy cases, not claims
about actual native caption lengths or any observed failure.

3CFA:0586 likewise reserves 54 hexadecimal local bytes and has a
stack-limit guard. It reads exactly 24 bytes from its first far argument
plus 30 into SS:BP-54, then passes that local source and its second far
argument to 1000:406D. It returns AX zero. It adds no terminator before
that second copy. A zero within the stored 24 bytes bounds the copied
text there; the fixed region alone supplies no such guarantee for other
contents. The helper has no own store through the frame pointer, but
its caller-supplied output still needs a valid storage contract.

The named caller supplies a separate local output buffer. With valid
terminated inputs, disjoint frame/stack/output storage, no address wrap
and admitted guard/copy returns, the setter's only frame-write range ends
at 54, immediately before the parent-window far pointer at 54..58.
Neither helper has an own store to the shared style word at frame+0E,
mask at 58, or callback fields beginning at 5A. This bounds these named
text helpers; it does not bound all callees of 3F96:0097.

## Interpretation

The APFM frame has inline text bytes at 3C..53. Together with the two
BMP-number fields and returned-pointer fields at 2C..3B from
FND-COMBAT-088, and its parent link at 54..57, this accounts for the
field roles throughout the previously opaque 2C..57 region. It does not
establish every input value, lifetime or visible result of those fields.

These text-copy helpers do not themselves enable a nonzero rendering
style or install a callback. Their rebased indexed writes stay below
the callback region under the stated storage conditions. That is a
specific alternative-base writer accounted for, not an exhaustive
whole-program no-writer result.

## Alternatives

A text-pointer interpretation is ruled out by the fixed byte-indexed
read and write loops. A padded, always-terminated 24-byte string
interpretation is ruled out by the absent tail fill and absent forced
terminator. The source-copy primitive's larger scan bound is not a
capacity guarantee for either local or caller output storage.

Q-UI-001 retains actual APFM text callers, admitted lengths/termination
and the remaining field meanings. Q-COMBAT-049 retains live record
origins, output/stack aliases, other writes and nonzero rendering paths.
The generic getter cannot be described as frame-preserving for an
arbitrary aliased output, even though the named caller supplies its local
buffer. No native corruption, bug, encoding or rendering outcome is
assigned by this reading.

## How to reproduce

Read 3CFA:0423..3CFA:046F, 3CFA:0586..3CFA:05D0 and
3F96:00D6..3F96:012F with resident_listing.py on the licensed installed
DSUN.EXE. Bind the getter output and setter source from the pushes,
retain the 0C caller advance and 30 helper advance, and follow both
24-byte loops at their actual byte widths. Compare 1000:406D's existing
complete reading in FND-CONFIG-181 and FND-CONFIG-182; do not assume
that its source scan supplies a local destination capacity. Separate
the own frame-write footprint from the source/output alias and termination
conditions and from the caller's later presentation helpers.
