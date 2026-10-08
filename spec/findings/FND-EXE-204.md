---
id: FND-EXE-204
title: Neighboring cursor helpers read through a shared pointer and publish two fixed cells with different zero-request paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B6770..0x005B67D9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B67E0..0x005B6831
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-202's unpadded neighbor search finds four full-dword direct
stores in these two bodies: each writes `0x0242C960` and then
`0x0242C940`. Both fixed four-byte destinations are disjoint from the
gate at `0x0242C910..0x0242C914`. Reading a complete direct body
separates those stores from its accesses through the loaded pointer;
no local instruction stores through that pointer.

The first helper at `0x005B6770` reads its full first stack argument
and initially clears its prospective return word. A zero argument
branches directly to normal restoration and returns zero without
reading either shared cell or the pointed-to bytes, and without
publishing either shared cell. Its saved-frame and saved-register
stack writes still occur on that path.

For a nonzero argument, it loads the pointer from `0x0242C940` and
the full word from `0x0242C960`. It reads exactly three individual
bytes at pointer offsets zero, one and two, assembling the earlier
bytes into higher positions with zeroed or masked upper register
bytes. It performs full-register shifts, additions and masks, including
a twenty-four-bit mask, before the two publications and final shift.
No local request bound, pointer-null guard or readable-extent guard
precedes those byte reads. The second cell's new full word is the
low three bits of the wrapped sum of the old word and argument.
The pointer publication adds the arithmetic-right-shift-by-three
version of that full sum to the loaded pointer, at 32-bit width.
The final result is returned in EAX without a store through the pointer.

The second helper at `0x005B67E0` loads both shared cells and its full
first stack argument without a zero-argument branch. It reads exactly
two bytes at pointer offsets zero and one, forms a full-register value,
and uses a sixteen-bit mask and register-count shifts. It forms the
same wrapped state-plus-argument sum, publishes its low three bits to
`0x0242C960`, and publishes loaded pointer plus arithmetic-right-shifted
sum to `0x0242C940`. It performs the final logical right shift and
returns the result in EAX. There is no call in either cited body.

A zero request to the second helper therefore does not bypass source
reads or publications. When the old state word is outside zero through
seven, its normalization and arithmetic pointer adjustment can change
those cells even for a zero request. Actual admitted state words and
requests are not supplied by these bodies; this is a local distinction,
not an observed shipped input or a supported malformed-state behavior.

All pointer byte reads use DS and ordinary frame accesses use SS.
Each body restores its three saved general registers and conventional
frame, then returns near without immediate stack-argument removal.
Their non-stack stores are the two fixed publications. Under a valid
stack model whose saved slots are separate from the gate, their direct
writes do not change that gate. A numeric pointer loaded from a nearby
cell does not by itself imply a write through it. Neither fixed-store
separation nor absence of callees proves complete input admission,
segment identity, storage lifetime or all possible entry paths.

## Interpretation

This removes two direct-store candidates from the gate-writer question
within their stated stack and entry model. Q-EXE-009 retains computed
and external writers, alternate entries, stack/storage admission,
actual shared-cell producers and the meaning and extent of cursor
inputs. No format, caller completeness or complete_reading declaration
is established. FND-EXE-078 remains the separate gate-storage evidence.

## Alternatives

Treating a nearby pointer load as a pointer-based write conflates
operand direction. Treating both zero-request paths as no-ops misses
the second helper's reads and publications. Treating the masked new
state as proof that the old state was bounded skips producer evidence.
An arithmetic right shift of the wrapped sum is not an unsigned
quotient for sums whose sign bit is set.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x005B6770`, count 45, and at
`0x005B67E0`, count 35. Restrict claims to the cited spans.
Check ReportCitationBoundaries for `005B6770..005B67D9:return`
and `005B67E0..005B6831:return`. Use FND-EXE-202's unpadded
whole-listing search to locate the stores, not to prove all writers.

Follow the full argument test, each byte-read width and direction,
state-addition width, low-three-bit publications, arithmetic versus
logical shifts, store order and normal restoration. Classify saved
stack slots separately from fixed DS destinations. Keep request and
state producers, alternate entries and valid storage open. Endpoint
controls do not establish caller coverage or complete reading.
Keep rich reports local and execute no original program.
