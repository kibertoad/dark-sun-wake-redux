---
id: FND-CONFIG-167
title: Runtime pointer dispatch can return a local rejection result that its caller discards
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:149B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1367
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:17F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:298E
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-165's runtime callee 1000:149B occupies file
span `0x0000669B..0x000066C4`, ending with far return
at `0x000066C3`. It saves SI and DI, writes incoming DS
to the shared word at CS:1361 and reads only the supplied
pointer's segment word at BP+8. The offset word at BP+6
is not read by this dispatcher. Segment zero skips both
local helpers. Nonzero equal to CS:135D calls local
1367; other nonzero segments call local 13CA. The common
return reloads DS from CS:1361 and restores the saved
registers and frame. It does not normalize AX to a common
result. The DS restoration uses that shared word, not a
stack-saved value; intervening writes to it remain a
provenance condition.

Local 1367's span is `0x00006567..0x000065CA`. Depending
on comparisons with CS:135B and linked segment fields, it
can change CS:135B, 135D and 135F or call local 143B
before a common request to 17F2. These are preceding
state changes, not a transactional rollback on a rejected
request. It passes a segment word and offset zero to
17F2 and returns its AX after removing the arguments.
The detailed metadata invariants and all producers of
these CS fields remain open.

The complete near 17F2 body occupies
`0x000069F2..0x00006A31`. It compares the supplied far
address first with current DS:009E/00A0, then with
DS:00A6/00A8, using local 07F0. The lower test rejects
strictly below; the upper rejects strictly above. Both
rejects return AX FFFF without calling 177C. Equality
passes the respective test. Local 07F0, file span
`0x000059F0..0x00005A11`, normalizes each offset into a
segment increment and low nibble, in word arithmetic,
then compares segments unsigned and low nibbles when the
segments tie. This is not an unrestricted linear-address
comparison across segment wrap.

An address within those two bounds calls local 177C.
17F2 converts a zero result from 177C to FFFF, and a
nonzero result to zero. Its own bounds rejection therefore
provides a concrete error result without an interrupt.
That result can return through 1367 and 149B without an
AX rewrite. FND-CONFIG-165's outer wrapper then explicitly
sets DX:AX to zero, so its caller does not retain this
runtime result. This local branch contract does not prove
that a named ordinary invocation supplies rejected bounds.

The complete 177C span `0x0000697C..0x000069F2` forms
a word value from its segment argument, current DS:0090
and word arithmetic, rounds it using a right shift by
six, and compares it with DS:3908. Equality stores the
supplied address at DS:00A2/00A4 and returns one without
an interrupt call. Inequality derives another word from
those fields, limits it using DS:00A8 and forwards two
words to local far 298E. A returned FFFF updates DS:3908
and takes the same address-store/one-return continuation.
Any other result updates DS:00A8, clears DS:00A6 and
returns zero. These fields are runtime state; no complete
heap or successful-release contract is assigned here.

The complete far 298E body occupies
`0x00007B8E..0x00007BAA`. It loads AH with 4A, ES with
its first word argument and BX with its second, then
issues interrupt 21h. Returned carry clear produces AX
FFFF. Returned carry set saves returned BX, forwards the
interrupt's AX to local 06BA, then restores that saved
BX into AX before returning. Thus the carry-set branch
does not simply return 06BA's FFFF result. The complete
06BA body, `0x000058BA..0x000058F3`, writes DS-relative
error fields and returns with two argument bytes removed;
its table/input provenance and semantic error mapping
are not established here. Its unprefixed 98 instruction
at 06D5 is CBW in this 16-bit body.

Local 13CA, 143B and 1464 were inspected for continuation
and dependencies. They modify metadata through supplied
and linked segments and CS:135F; one local path temporarily
changes SS under saved flags and disabled interrupts.
They contain no direct interrupt request. Their complete
metadata layout, valid links and input coverage are outside
this bounded dispatcher/result finding. No absence of
indirect or aliased effects is inferred from that local
call inventory.

## Interpretation

The runtime dispatcher routes by segment and restores DS
from a shared field. One route has a bounded address check
whose FFFF result can reach the outer wrapper and be
discarded by its unconditional returned zero. Other routes
and interrupt outcomes have different state and result
contracts. A cleared caller pointer is not proof of a
successful runtime operation or preserved earlier state.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain shared-CS writers,
valid metadata/links, bound and runtime-field producers,
all callers, interrupt responses and preservation of
state/registers across them. One reading uses ordinary
valid runtime state and accepted bounds; another changes
bounds, segment metadata or the interrupt result. The
local encoding distinctions are known, but actual native
reachability and operation outcomes remain unobserved.

A saved DS slot can restore the incoming value when it
stays unchanged through the helper calls. Whether other
writers or reentry can alter CS:1361 is an open provenance
question, not a claim that such reentry occurs. Segment
zero is a dispatcher bypass, but does not undo the outer
wrapper's earlier metadata read in FND-CONFIG-165.

Resident emulation can exercise the interrupt-free bounds
predicate after the harness and supported field layouts
exist (Q-SCRIPT-007). It cannot establish the interrupt
response, full runtime invariants or overlay caller effects.
No native or emulated result is claimed here.

## How to reproduce

Read 149B through 14C3 and follow both helper targets;
keep the shared DS slot and segment-only input separate
from the outer wrapper's pointer access. Read 1367 through
13C9, 17F2 through 1830, and comparator 07F0 through
0810 from their entries. Verify lower/upper branch flags,
word normalization and the returned FFFF/zero encodings.
Read 177C through 17EF and 298E through 29A9, following
both returned-carry paths and the saved BX around 06BA.
Read 06BA through its RET 2 and inspect the unprefixed
98 instruction in 16-bit context. Compare the outer
DX:AX clear in FND-CONFIG-165. Keep metadata invariants,
shared-slot writers and interrupt outcomes separate.
