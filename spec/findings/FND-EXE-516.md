---
id: FND-EXE-516
title: Game allocator request advances a shared offset only below a stack-relative margin
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0F68..1000:0F99
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0F68..1000:0F99
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-515's near request helper 0F68 saves BP and reads incoming
low and high words at SS:BP+4 and SS:BP+6. It adds current
DS:009C to the low word at word width and adds that carry to
the high word at word width. It copies the resulting low word into
CX and rejects when the resulting high word is nonzero.

Otherwise it adds 0200 to CX, rejects carry from that addition,
and compares CX unsigned with current SP. CX at or above SP
rejects. The comparison uses SP after the saved BP, rather than the
caller's pre-call stack pointer. A passing branch exchanges the resulting
low word in AX with current DS:009C: the new offset is stored
and the freshly read old shared word is returned in AX. It restores
BP and returns near without incoming cleanup. The helper makes no
calls or interrupts and does not locally change DS or SS.

Every rejection stores word eight at DS:0094, returns AX FFFF,
restores BP and returns near. It does not locally change DS:009C
on rejection. Aliases between the shared words and incoming frame or
other memory remain unadmitted. The return is not a retained snapshot
of the shared word from the earlier addition; the exchange reads it again.

The high-word carry propagation is itself word-width. An incoming high
FFFF with carry from the low addition becomes zero and can pass
the high-word test. The later margin and SP tests still apply. Thus
the guard is not an unconditional mathematical nonoverflowing unsigned
32-bit sum test. No extent, segment-equality or allocation-unit check is
performed beyond these numerical comparisons.

FND-EXE-515's calls supply high word zero. Its preliminary zero-size
request therefore returns current offset and stores the same value only
when offset plus 0200 does not carry and is below the helper's
current SP; otherwise it returns FFFF. The caller masks that result
with one, so a failure result also selects its one-unit follow-up request.
It ignores that follow-up result before making the final size request.
Each call has its own stack depth and fresh shared-state reads.

For those high-zero calls, a successful final request advances DS:009C
by the supplied internal size and returns its prior offset. The subsequent
block producers write headers through DS at that returned offset. These
local instructions establish neither the initial shared offset nor that its
numerical relation to SP denotes disjoint admitted storage through DS.
Both editions have identical bodies and shared-word offsets here.

## Interpretation

This resolves the immediate request producer below diagnostic buffer allocation.
Its local success is a shared-offset update guarded by a numerical margin
to current SP, rather than an independently verified allocation. Q-EXE-007
retains DS:009C's initialization and other writers, actual DS/SS, stack
and storage extent, aliases and lifetime, caller depth/state admission, the
remaining initialization helpers and earlier startup/launch coverage. No complete
allocator or runtime initialization contract is claimed.

## Alternatives

Treating the guard as full-width nonoverflowing addition ignores wrapped
high-word carry. Treating a zero-size request as infallible ignores its
margin/SP tests. Treating the caller's low-bit result as success ignores
FFFF's low bit. Treating the return as the original shared-value snapshot
ignores the later exchange. Treating a numerical offset below SP as valid
DS storage ignores actual segment and extent admission.

## How to reproduce

At revision c9c3749 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
6168..6199 in sixteen-bit mode. Track SS argument reads, low ADD
carry into word-width ADC, high-word rejection, margin carry, unsigned
SP comparison and the shared-word exchange. Review high FFFF with
low carry, margin overflow and equality with SP as static branches.
Bind FND-EXE-515's zero/high-zero, one/high-zero and final-size calls
separately, including the preliminary failure's low bit. Licensed bytes stay
outside Git; no game process, DOSBox or emulated call runs.
