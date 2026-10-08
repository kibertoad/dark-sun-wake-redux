---
id: FND-EXE-144
title: Complementary byte-count prefix branch saves count and input while retaining its lookup byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A18E1..0x004A1964
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2073..0x004A20B8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A2233..0x004A2240
tool: Ghidra 12.1.3 PUBLIC bounded complementary byte-count prefix branch reading
environment: null
---

## Observation

FND-EXE-131 grounds selector 34 at `0x004A18E1`. It zero-extends
byte C at `0x01BA29E4`, compares it unsigned with eight and saves
it as one byte at current ESP plus `0x0F`. The store preserves the
comparison flags for the following unsigned branch. C above eight reads
and zero-extends byte N at `0x01BA29E0`, reads current full mask M
at `0x0075B204`, clears mask one and saves N at ESP plus `0x0E`.
It then joins at `0x004A1908`.

For C at or below eight, the alternate at `0x004A207B` reads N,
reloads saved C, computes eight minus C and saves N in the same slot.
It arithmetically shifts a zero-extended full copy of N by that count,
which is bounded zero through eight. Its full sign bit is clear, so the
shift fills with zero. It reads M and sets mask one when the shifted
result's low bit is one, or clears it through `0x004A2233` otherwise.
Both join at `0x004A1908`. C zero shifts by eight and gives extracted
bit zero; C eight selects N bit zero. Counts above eight do not return early.

The join zero-extends byte V at `0x01BA29E8`. V zero sets mask
`0x40`; nonzero clears it through `0x004A2073`. Both join at
`0x004A191A`, reload saved N and replace mask `0x80` from
retained V's bit seven. They XOR zero-extended V and saved N, then
logically shift right seven. The result is zero or one, so mask
`0x800` is set when their bit-seven values differ and cleared through
`0x004A20AC` otherwise. Both join at `0x004A1948`.

The branch clears mask four and indexes the word lookup at `0x006F2B70`
plus twice retained BL, not a fresh V read. FND-EXE-133 grounds the
physical shipped contributions; runtime writers remain unresolved. It ORs
the zero-extended contribution into the mask, then tests saved C at ESP
plus `0x0F` with `0x1F`. The direct transfer at `0x004A195F`
preserves those flags to `0x004A1015`. FND-EXE-137 grounds that
shared gate: zero low five bits clears mask `0x10`, nonzero sets it.
FND-EXE-132 grounds full mask publication, subsequent full selector
clear, register restoration and full published-mask return.

No calls or local ESP changes occur between these saved-byte stores and
reads, and the bounded paths show no intervening writes to either slot.
FND-EXE-131 grounds the enclosing reserved frame. N and C occupy distinct
adjacent bytes; the later reads consume only their respective byte.
Other M bits remain locally retained apart from the stated replacements.
Separate initial N/C/V reads establish no atomicity or producer/lifetime
contract.

## Interpretation

Selector 34's byte-count boundary and saved/retained inputs are explicit.
It differs from FND-EXE-143's word boundary and fresh tail reads.
Q-EXE-009 in FMT-EXE-006 remains open for producers, runtime writers,
other prefix branches and remaining callee effects. No complete helper
reading, named operation model or actual PATH behavior is established.

## Alternatives

- C zero gives extracted bit zero, C one selects N bit seven and C eight
  selects N bit zero. C nine or 33 clears mask one without shifting.
- Above-eight counts still reach V's zero/sign tests and N/V high-bit
  comparison. Mask `0x800` can therefore be set on that path.
- Saved C zero or 32 clears mask `0x10`; one or 33 sets it. The
  final count gate does not test the complementary shift count or reload C.
- Upper N/V storage bytes do not participate. Neither the saved-byte sign
  comparison nor retained V lookup is interchangeable with a fresh global read.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-131's selector slot and frame. Use the saved Ghidra program
with -noanalysis and ReportInstructionWindow.java at `0x004A18E1`
limit 35, `0x004A2073` limit eighteen and `0x004A2233` limit
three. Exclude following unrelated paths. Use FND-EXE-137 for the final
count gate and FND-EXE-132 for publication/return. Track comparison flags
across the count store, unchanged ESP, distinct saved byte slots and their
writers on each path, bounded subtraction, zero-extension, alternate clears,
retained V and final flags transfer. Check counts zero, one, eight, nine,
32 and 33 and each byte high-bit combination as local arithmetic controls,
not native inputs. No native or emulated execution is part of this finding.

The location ranges use exclusive ends, including the final transfers
already described above. Verify the corrected boundaries with additional
windows at `0x004A195F`, `0x004A20B3`, `0x004A223B`,
each with limit two; the following instruction starts are their exclusive ends.
