---
id: FND-EXE-409
title: Indexed record lookup publishes an early output and writes tags after an untested transfer result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:060B..1425:076B
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-408 calls this helper with an incoming word at SS:BP+6
and far output pointers at +8 and +0C. The helper saves BP,
SI and DI and allocates six local bytes. It derives local BP-6
as the incoming word shifted right twelve plus one, DI as its
low twelve bits shifted right six, and an output word as its
low six bits. Thus the three values are locally one..sixteen,
zero..63 and zero..63 respectively. It writes the low-six-bit word
through the +0C far pointer before age updates or any calls.

Using current DS and stride 0108, it visits age fields 39CA
for record indices one through four. FFFF remains unchanged; every other
word increments once, including FFFE becoming FFFF. It then searches
records zero through four for both tag fields 39C6 and 39C8
equal to local BP-6 and DI. The first match clears that record's
39CA age, writes its index through the +8 far pointer and returns
AX zero without a callback. All accesses and comparisons use current fields.

If no tag pair matches, it searches 39C6 for FFFF at indices
one through four, replacing SI on every match. Thus the highest such
index is selected. If none matches, it starts with index one and
selects a later record only when its unsigned age is strictly greater
than the current selection's age. Equal maxima retain the earlier index.
These local selection loops are bounded independently of later callback behavior.

It starts DX zero and checks the selected record's 39C4 word.
Nonzero calls 0591 with SI and copies returned AX into DX;
zero skips that call. A nonzero DX skips the subsequent transfer and
tag writes but still writes current SI through the +8 output pointer
and returns DX in AX. The early +0C output and age updates
are not undone. FND-EXE-407 reads the 0591 callback path and
its conditional preservation.

After zero DX it reads the word at current DS:39CC indexed
by local BP-6 times four, splits its high four bits into local
BP-2 and low twelve bits into BP-4, and prepares a different
slot callback. It pushes word 0100, current DS, offset 39CC
plus SI times 0108, DI shifted left eight, local BP-4 and
the slot argument at BP-2 times fourteen plus 3F4E. It calls
the far pair indexed from 3F42/3F44 and removes twelve bytes.
The slot index is locally zero..fifteen; lookup extent and pointer validity
are not checked by this bit split.

Returned AX is copied to DX. Without testing that result, it writes
local BP-6 to current DS:39C6 indexed by current SI, current
DI to indexed 39C8, and zero to indexed 39CA. Only afterward
does it write current SI through the +8 output pointer and return
DX in AX. Thus a nonzero final transfer result still precedes tag
and age publication. These stores are not a successful transfer predicate.

The helper does not locally save SI, DI or DS around that
indirect call. Return-side indices and segments depend on the target, and
local output-pointer aliases can change tags, local values or saved frames.
The final restoration of saved DI/SI does not restore them before the
publication stores. No capacity, disjointness or unchanged-frame contract follows.

The complete body is 352 bytes in 133 instructions, ending with
the far return at 076A. The ordinary indexed arithmetic is word-width.
No original native request is executed or admitted by this local reading.

## Interpretation

This supplies FND-EXE-408's direct helper, its concrete output producers,
record-selection gates, slot choice and different failure prefixes. Q-EXE-007
retains complete callers/writers, DS/input/extent/alias admission, slot-target
preservation and native transfer effects. Publishing matching tags after a nonzero
transfer result does not establish that such tags imply valid record contents.
No complete reading, reachable failure outcome or original bug is declared.

## Alternatives

Treating both outputs as success-only erases the early low-six-bit store and
the failing 0591 path's index output. Selecting the first unused record
contradicts replacing SI on every match. Choosing the last equal-age maximum
contradicts the strict unsigned comparison. Skipping tag publication on final
nonzero AX inserts a missing test. Calling final register restores proof
of preserved publication indices changes their order.

## How to reproduce

At revision c5dd5163 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 0x060B through exclusive 0x076B in sixteen-bit mode.
Require 352-byte coverage and 133 instructions. Track incoming word splits,
early output, saturating ages, first tag match, last unused selection and
strict maximum-age replacement. Follow the selected flag through 0591,
its nonzero early exit, both words of the final slot target and all
outgoing arguments. Trace returned AX into DX, the untested tag stores,
output pointer reload and returned AX on every path. Use FND-EXE-407/408
for connected behavior. Preserve callback and storage gaps; licensed bytes
remain outside Git. No game, DOSBox or emulated call runs.
