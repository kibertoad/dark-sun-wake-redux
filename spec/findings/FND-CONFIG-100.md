---
id: FND-CONFIG-100
title: Overlay 193 routes codes 235 through 268 to overlay 176's conditional feedback entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5691:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 176's resident entry 5691:0043 resolves to file offset
`0x000616D6`, with a separate prologue there and return at
`0x0006190C`. Both shared message calls at `0x000617BE` and
`0x0006182A` lie within this entry (FND-CONFIG-052).

The declared FBOV direct far-call inventory has one call to that entry
from another overlay: overlay 193 at `0x00080607`. The containing
exported entry is 573B:0089, starting at `0x00080580` and returning
at `0x00080616`; no return separates that entry from the call.

The selector reads its third word argument as a code. Its comparisons
are signed. The call at `0x00080607` requires both:

- the code is in the inclusive range 235 through 268;
- the separate byte argument at stack offset `[BP+0x1A]` is zero.

The earlier comparison excludes codes 328 or greater. A nonzero byte
argument or code 269 or greater takes a different local call, while
codes below 235 take a different overlay call. The 235..268 branch
subtracts 235 from the code, producing 0..33, and passes that word as
the third argument to 5691:0043. Its first two word arguments are
forwarded unchanged. The remaining argument slots include forwarded
words and a byte passed in a word slot; this reading does not assign
gameplay roles to those inputs.

At 5691:0043, a nonzero near-pointer argument at `[BP+0x14]` whose
pointed byte is nonzero skips past both message sites. Otherwise the
entry calls local 5691:002A at file offset `0x000616F6`; a zero
byte result also skips past the two sites. The insufficient-points
site additionally requires the unsigned selected-record word at `+2`
to be lower than the computed threshold and the first word argument
to compare signed below four. The other site follows a signed
nonpositive action-helper result and the text-selection guards in
FND-CONFIG-052. The later shared message-window setup still has its
own gates (FND-CONFIG-018).

## Interpretation

The two message sites have a concrete incoming route through a shared
selector with a bounded code range and a separate byte gate. The
235..268 routing and 0..33 transformation identify which selector
inputs can enter this particular entry; they do not establish the
complete psionic action rule or a live feedback message.

## Alternatives

The selector's upstream callers, their code and byte inputs, eligibility
and action helper results, and record state remain unread (Q-CONFIG-008).
FND-CONFIG-101 inventories its declared direct callers. Computed or
unrelocated calls can add routes to either entry. A call into the
selector does not prove that its range/byte branch is taken or that
WIND/10501 acquisition succeeds.

## How to reproduce

Resolve overlay 176 stubs 0043, 002A and 0075 to file offsets
`0x000616D6`, `0x0006159B` and `0x00061D2E`. Resolve overlay
193 stub 0089 to `0x00080580`. Select declared FBOV fixups whose
segment operand names descriptor 176 and whose direct far-call offset
is 0043; the selected call is at `0x00080607`. Inspect the selector
in bounded blocks `0x00080580..0x000805C7` and
`0x000805C7..0x00080617`, including signed comparisons and argument
pushes. Inspect the entry gates at `0x000616D6..0x00061703`,
threshold comparison and action call at `0x00061794..0x000617DA`,
and return at `0x00061907..0x0006190D`. Compare the local message
windows in FND-CONFIG-052. Keep function ownership distinct from
physical adjacency.
