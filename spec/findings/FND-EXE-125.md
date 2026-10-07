---
id: FND-EXE-125
title: Full-width mapped reader separates one four-byte access from independently mapped boundary bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F6640..0x004F673B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F6740..0x004F6792
tool: Ghidra 12.1.3 PUBLIC bounded full-width mapped-reader reading
environment: null
---

## Observation

FND-EXE-124 records two calls to `0x004F6740`, storing each full
returned EAX without testing failure. The reader reserves twelve bytes and
loads its full first input A from current ESP plus sixteen. It tests A's
low twelve bits against 4092, unsigned. Low offsets zero through 4092
select one four-byte read; 4093 through 4095 instead call the separate
byte-assembly helper `0x004F6640` with retained A. This is a different
width and threshold from FND-EXE-094's sixteen-bit reader.

The single-access path logically shifts A right twelve to form J, then
reads the full direct entry at `0x0075B6D0` plus four times J. A
nonzero entry D supplies a full read at D plus full A, with wrapping
address arithmetic, and returns that full value. It does not substitute
A's low twelve bits for A. FND-EXE-100 records mapping bias publication,
without establishing every entry's extent or lifetime.

A zero direct entry instead loads full object O at `0x00F5B6D0`
plus four times J, reads O's first full word as a target table, and calls
its target at offset sixteen with O and full A in two outgoing slots.
After an ordinary return it restores the local reservation and retains
the full callee EAX. There is no local object/target-null guard or
normalization to a smaller width. The reader's low-offset gate decides
between these paths before any direct or object entry is read; backing
array validity and indirect effects remain unresolved.

The byte-assembly helper reserves twenty-eight bytes, saves three registers
in its local frame and retains original full A. It performs four reads in
ascending input order, at wrapped A, A plus one, A plus two and A plus
three. For each address it independently computes that address shifted
right twelve and freshly reads its direct entry at `0x0075B6D0`.
A nonzero entry supplies one byte at that entry plus the full current
address, zero-extended. A zero direct entry freshly selects the parallel
object at `0x00F5B6D0`, reads its current first-word target table,
and calls target offset eight with object and current full address. Only
returned AL is then retained and zero-extended. There is no local
object/target-null guard for any of the four calls.

The helper retains the first byte, shifts the second byte by eight and
combines it, shifts the third by sixteen and combines it, then shifts
the fourth by twenty-four and combines it. The full ordinary return is
b0 OR (b1 shifted eight) OR (b2 shifted sixteen) OR (b3 shifted twenty-four).
The helper restores its saved registers before returning; its caller keeps
that full EAX. All four reads occur on ordinary completion regardless of
byte values. A zero byte does not end assembly, and an indirect return's
upper twenty-four bits cannot contribute. An exceptional indirect exit
need not reach later reads or the final assembly.

For low offsets 4093, 4094 and 4095, respectively, the first three, two
and one addresses lie on the initial page before the next address crosses
the 4096-byte boundary. At full A equal to `0xFFFFFFFF`, A plus one
wraps to zero; the later indices are recomputed from wrapped addresses.
Even reads within one page select the mapping afresh: a prior indirect
call may have changed state. The helper does not retain one mapping as
admission for the remaining bytes. It has no local table/slot stores,
while its unread callees may modify those or other state. The twenty-bit
page index bounds index arithmetic, not independently established backing
storage or permissions.

## Interpretation

These are concrete local width, mapping-selection and boundary-assembly
contracts for the reader used by FND-EXE-124. A one-read path keeps a full
indirect return, whereas the boundary path truncates each separate return
to one byte before assembling a little-endian full value. Q-EXE-009 in
FMT-EXE-006 remains open for concrete offset-sixteen targets, byte-method
effects, mapping producers and lifetime, input admission and the larger
selected callee's remaining branches. This is not a complete mapped-memory
model or an established description of actual PATH behavior.

## Alternatives

- Offset 4092 permits a single four-byte access; the three larger low
  offsets use independently selected bytes. The sixteen-bit threshold
  from FND-EXE-094 does not apply to this reader.
- Direct addressing uses the full input plus the entry, consistent with
  a bias contract; a low-twelve-bit-only addition is not performed locally.
- Boundary assembly neither reuses its initial mapping nor stops on zero,
  and each virtual byte return discards its upper bits.
- The ordinary full-width virtual return is not truncated or tested for
  failure, and a page-index arithmetic bound does not establish storage.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's six physical
controls. FND-EXE-124 supplies the direct reader target. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x004F6740` limit 55, restricting observations through
`0x004F6792` and excluding the following writer. Follow its direct
helper target with a window at `0x004F6640` limit 72, then complete
the last indirect byte branch at `0x004F6725` limit seven. Restrict
the helper to its declared range. Track frame-relative inputs, unsigned
low-offset threshold, full versus byte returns, each freshly selected
mapping and target, outgoing-slot last writers, wrapped addresses, shifts,
OR assembly and register/frame restoration. Check the local arithmetic
for low offsets 4092 through 4095 and full all-ones input; these are
instruction-reading controls, not evidence those inputs occur in a native
run. Keep concrete targets, array extents and indirect effects conditional.
No native or emulated execution is part of this observation.
