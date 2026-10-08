---
id: FND-EXE-231
title: Segment-state writer switches DS before ordered header-word stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0735..4AE5:0753
tool: Ghidra 12.1.3 PUBLIC, p-code segment-output reporter and executable-reader 2.5.0
environment: null
---

## Observation

FND-EXE-230's state-writing procedure contains twelve instructions and thirty
bytes, ending exclusively at `4AE5:0753`. Its only call is the arithmetic
helper at `4AE5:07A1`; it returns near at `4AE5:0752` without extra argument
cleanup. The bounded CFG has one conditional branch and assumes that call
returns. The helper's explicit effects are read in FND-EXE-230.

After adding the returned increment to current DS-relative word `0x0120`,
the procedure saves DS, loads AX with a relocated segment word and loads DS
from AX. The immediate occupies shipped-file word `0x0004078E`; the MZ
relocation table marks it for relocation. Its shipped value is `0x45DF`,
giving `55DF:0000` under load segment `0x1000`, corresponding to shipped-file
offset `0x0004AFF0`. This is a different segment base from FND-EXE-176's
candidate state segment `55CE:0000`; it does not identify the caller's live DS.

The segment load at `4AE5:0740` is rendered with the operands reversed.
Its decoded p-code explicitly writes DS, supporting the load direction.
The procedure reads the newly DS-relative word at `0x001C` into AX and
tests it at sixteen-bit width. If zero, it first stores ES into that
DS-relative word. Both paths then store the retained AX to current
ES-relative word `0x001C`, restore saved DS and return. The DS restore at
`4AE5:0751` also has a decoded DS output. No further calls intervene between
the read and these stores.

Thus a zero input word takes the first store and then writes zero through
ES; a nonzero input skips the first store and writes that old word through
ES. Their order is explicit. Equality of the two effective destinations
has not been excluded: if they refer to the same storage, the later store
can overwrite the earlier one. The same offset in two segment accesses is
not evidence that either destination is the caller's earlier state word.

## Interpretation

This completes the bounded explicit-instruction reading of one writer found
in FND-EXE-230, separating the pre-switch arithmetic state access from the
post-switch accesses and preserving both paths' store order. The segment
relocation establishes a source-derived segment candidate, not its live
contents, an admitted linked-list structure or alias-free storage.

Q-EXE-001 and Q-EXE-010 retain native entry, caller DS/ES and header admission,
writers of the relocated-segment word and accessed fields, effective-address
alias checks, stack-slot preservation, interrupt-enabled changes and other
publisher callees. No complete_reading, allocator contract or replacement
inventory is established.

## Alternatives

Treating all accesses as one unchanged DS-relative state structure is
contradicted by the segment load between them. Treating the displayed
segment-MOV direction as authoritative is contradicted by its DS output.
An alias-free chain insertion remains a possible interpretation, but its
storage admission and distinctness are not established by this procedure.

## How to reproduce

At revision `c6a345c`, use FND-EXE-226's source hash, original-source region
and default x86-bounds limits, setting entry and sole entries value to
`0x00040785` (`4AE5:0735`). Supply no seeds or summaries. Inspect all thirty
covered bytes and the single returning-call assumption. In the resident
Ghidra snapshot, read-only with analysis disabled, run ReportInstructionWindow
at `4AE5:0735`, count 38, restricting this procedure to the interval above.

Against the same hash-guarded source, run the committed operand command with
sourceKind `mz`, site `0x0004078E` and targetOffset zero. Check the MZ relocation,
raw word, loaded address and shipped-file offset above. Run
tools/ghidra/ReportSegmentWrites.java at revision `c6a345c` with names DS and
limit 10000. Check positive outputs at `4AE5:0740` and `4AE5:0751`. The scan
reports 74571 decoded instructions and 592 DS matches, all emitted without
truncation. Its opaque-site output is capped; no absence or interrupt-time
preservation claim follows. Sources and reports remain in GAME_DIR.
