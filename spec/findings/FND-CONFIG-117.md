---
id: FND-CONFIG-117
title: Overlay 204 temporarily sets the resident event state to five and restores its saved word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2522
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:252A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0025
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV fixup and trampoline mapping
environment: null
---

## Observation

Resident getter 28C9:2522 begins at file offset `0x000203B2`
and returns at `0x000203B9`. It returns the word at DS:0DAB.
The adjacent setter 28C9:252A begins at `0x000203BA` and
returns at `0x000203C4`. Its only store copies its first word
argument into DS:0DAB; neither entry calls another routine.

Two declared FBOV direct far calls target that setter, both within
overlay 204 exported entry 5787:0025. Its prologue is at
`0x0008D074` and return at `0x0008D0E1`; no preceding return
separates either setter call from the entry.

The entry calls the getter at `0x0008D083` and saves its result
in local word BP-4. It pushes literal five and calls the setter at
`0x0008D08D`. After constructing arguments, it calls overlay
179 entry 0066 at `0x0008D0CE`. On ordinary return from that
call, it passes the saved local word to the setter at
`0x0008D0D9`, then returns. There is no local conditional branch
or early return between saving, setting five and restoring.

Selecting exact MZ relocated direct calls to raw 18C9:252A
finds none. Searching the complete resident segment
`0x0001DE90..0x000217F0` finds no 16-bit relative near-call
candidate to file entry `0x000203BA`. These negatives do not
exclude indirect calls, aliases or other writes to DS:0DAB.

## Interpretation

The stored word tested by FND-CONFIG-113 and FND-CONFIG-115
has an explicit getter/setter pair and a concrete temporary-five
assignment route. This wrapper restores its captured word after the
intervening call, even if that call changed the current state. It
can restore one when its saved word is one, but that does not
establish a state-two/three transition in the separate handler path.

## Alternatives

FND-CONFIG-118 reads overlay 179's wrapper, pending-record drain
and two state-dependent return-region calls, leaving their transitive
effects open. FND-CONFIG-119 traces seven local incoming wrapper
calls in two guarded groups. Earlier caller inputs, timing relative
to the event handler, indirect setter calls, other direct or block
writers, and state initialization remain open
(Q-CONFIG-008). The no-local-branch reading assumes the intervening
call returns normally; it does not establish its termination or
registration behavior. No complete state-value enumeration follows
from this one temporary assignment.

## How to reproduce

Read getter and setter only through their returns,
`0x000203B2..0x000203C5`. Resolve overlay 204 trampoline
0025 with FMT-EXE-002 through FMT-EXE-004 and inspect the
bounded entry `0x0008D074..0x0008D0E2`. Track the getter
result's BP-4 lifetime across the intervening call. Select declared
FBOV fixups with decoded descriptor 22 and preceding far-call
offset 252A using FMT-EXE-005; verify both instruction sites and
literal-five versus saved-word pushes. Check exact relocated MZ
calls and full-segment near-call candidates separately. Do not
interpret those encoding-specific negatives as a whole-program
writer inventory.
