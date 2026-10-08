---
id: FND-EXE-194
title: Shared callback dispatch continuations reach the failure import while direct setter searches remain bounded
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD140..0x005FD14D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD0FD..0x005FD105
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006020C0..0x006020C6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035854C..0x00358550
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.4.0
environment: null
---

## Observation

FND-EXE-193's stack-argument callback call at `0x005FD145` is followed
by a direct call to `0x005FD120` at `0x005FD148`. On a normal return
from the first callback, execution therefore proceeds into the consumer
of the published field +4 slot. This is conditional on the callback's
return and frame preservation, not proof those effects occur.

In that consumer's callee, `0x005FD0FD` calls through its own first
stack argument at `EBP + 8`. Its following instruction at `0x005FD100`
calls `0x006020C0`. That target jumps through import slot `0x0243194C`.
The fingerprinted PE's import lookup/address-table correspondence names
this slot `abort` in `msvcrt.dll`; its shipped address-table dword maps
to file offset `0x0035854C`. FND-EXE-190's newly allocated shared record
uses the same local trampoline as its initial field +4 value. The
import identity is not proof of the loaded library's effects, non-return,
stack behavior, or the continued validity of an adopted record.

The analyzer reference query for FND-EXE-193's setter at `0x005FD170`
returned no references. A separate scan of all decoded instruction text,
independent of function ownership, returned no instruction containing
the literal `0x005fd170`. It did return the independently located
direct-call controls `0x005FD148` to `0x005FD120` and `0x005FD15E` to
`0x005FD140`, among other positive calls to those two targets. The
combined query did not reach its 256-match cap. These controls cover
rendered direct calls only. They do not control stored pointer data,
indirect calls, computed targets, undecoded bytes, interior entry points,
or target spellings that do not contain the queried literal.

A source-byte scan of five-byte E8/E9 transfers in physically backed
executable sections found no candidate entering `0x005FD170..0x005FD182`.
Its independent control intervals at `0x005FD120..0x005FD121` and
`0x005FD140..0x005FD141` produced ten and two candidates respectively.
The 128-candidate limit was not reached. The scan does not establish
instruction boundaries or reachability; it excludes non-executable sections,
raw padding, virtual-only bytes, cross-region encodings, rel8/rel16,
conditional, far, indirect, computed and runtime-written transfers.

## Interpretation

This closes the concrete normal-return continuation between the two
published callback consumers and identifies the local failure trampoline
by its shipped import slot. Q-EXE-001 retains callback preservation,
loaded failure effects, indirect setter callers and argument provenance,
other slot writers and record lifetime. The two empty setter results
are restricted search observations, not a declaration that it has no
callers or that its mutation path is unreachable. No complete reading
follows.

## Alternatives

Stopping at the field +8 call would omit its field +4 continuation.
Calling the failure target an ordinary local cleanup routine would ignore
its import-table transfer. Conversely, the import's name alone cannot
prove failure termination or establish object ownership and teardown.
Two empty searches with the same omitted indirect domain do not close
that domain.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved PE project, read-only with analysis disabled, run
ReportReferences at `0x005FD170` (reference limit 200). Run the committed
ReportInstructionText with tokens `0x005fd170`, `0x005fd140` and
`0x005fd120` (combined match limit 256). Check both named positive
direct-call controls and retain the exclusions above; no absence claim
about other reference kinds is admitted.

Run ReportInstructionWindow at `0x005FD140`, count 10; at
`0x005FD0FD`, count 2; and at `0x006020C0`, count 1. Restrict the
continuation claims to the declared half-open intervals, excluding any
neighboring instructions printed after a gap. Repeat FND-EXE-187's
source-fingerprinted `x86-imports` query with controls slot `0x024319D0`,
dll `MSVCRT.DLL`, name `malloc`, and slot `0x024319E4`, dll `MSVCRT.DLL`,
name `memset`. Both controls match. Read the descriptor/lookup correspondence
for slot `0x0243194C`, including its source file offset and import name.
Rich instruction and import reports remain in the local licensed-source
store, outside Git. No original-game execution is involved.

Run the committed `pe-transfers` command with sourceKind `pe32`, the
same fingerprinted source, target `{start: 0x005FD170, end: 0x005FD182}`,
controls `{start: 0x005FD120, end: 0x005FD121}` and
`{start: 0x005FD140, end: 0x005FD141}`, and limit 128. Inspect target
indices separately from control indices; control hits do not count as
setter candidates. Retain every exclusion listed above.
