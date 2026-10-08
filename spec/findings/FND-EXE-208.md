---
id: FND-EXE-208
title: Physical near-transfer and decoded-flow searches agree on the selected-record reader's direct caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5136..0x005F513B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5187..0x005F518C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005BE76F..0x005BE774
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.4.0
environment: null
---

## Observation

A physical scan of five-byte E8/E9 near transfers in the shipped PE's
mapped executable bytes finds one candidate entering the complete reader
interval `0x00600A80..0x00600A8D`: the call at `0x005F5136` targets its
entry `0x00600A80`. There is no candidate entering an interior byte in
this search domain. A separate enumeration of every decoded instruction's
resolved call and jump targets finds that same call and no other flow into
the interval, independently of function ownership.

The independently read call control at `0x005F5187` targets
`0x00600A50`, as recorded in FND-EXE-166. The independently identified
free trampoline in FND-EXE-024 supplies a separate jump control:
the physical candidate at `0x005BE76F` decodes as a jump to `0x00601D00`.
The physical control intervals produce one and 240 candidates respectively.
Including the reader candidate, the scan reports 242 candidates in one
mapped executable region, below its 4096 limit. The other jump-control
candidates are raw encodings, not 239 additional proved callers.

The physical search considers every eligible byte as a possible start,
without relying on analyzer instruction or function boundaries. It excludes
non-executable sections, unmapped raw padding, virtual-only bytes,
cross-region encodings, rel8/rel16, conditional, far, indirect, computed
and runtime-written transfers. The decoded search excludes undecoded
instructions and unresolved targets. Agreement does not close those domains.

## Interpretation

This supplements FND-EXE-166's physical absolute-address-word search with
an independent physical near-transfer check across the reader's full body.
The single candidate is checked to decode as the known call. Q-EXE-009
still requires excluded target representations, selected-local admission,
field writers and setup preservation. No complete-reading declaration or
format-status promotion follows.

## Alternatives

A second E8/E9 candidate entering the body would require a separate decoded
instruction-boundary check before being called a caller. None is present
in the stated physical domain. Treating this as proof of no indirect or
runtime-created caller would extend the result beyond both searches.

## How to reproduce

Use the shipped PE of size 3802624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Run the committed
`tools/evidence/report.mjs pe-transfers` adapter with sourceKind `pe32`,
target `{start: 0x00600A80, end: 0x00600A8D}`, controls
`{start: 0x00600A50, end: 0x00600A51}` and
`{start: 0x00601D00, end: 0x00601D01}`, and limit 4096.
The mapped executable region spans shipped file offsets
`0x00000400..0x002EEBF4`, preferred addresses
`0x00401000..0x006EF7F4`. The adapter checks E8/E9 plus a signed
32-bit displacement wholly inside the region, computing the destination
from the address after those five bytes. Inspect target and control indices
separately and retain all exclusions above.

In the saved PE project, read-only with analysis disabled, run
ReportCallsToRange with `0x00600A80`, `0x00600A8C`, `all`; this
reporter's final address is inclusive. Its 50-result cap is not reached.
Compare the physical reader candidate with its decoded call and
FND-EXE-166's independently read call control. Run ReportInstructionWindow
at `0x005BE76F`, count 1, to check the separate jump control. Keep
rich reports in the licensed-source store, outside Git. Execute no original
program.
