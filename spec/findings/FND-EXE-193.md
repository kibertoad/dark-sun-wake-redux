---
id: FND-EXE-193
title: Published shared-record fields are mutable callback slots with distinct consumers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600520..0x0060052E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD120..0x005FD133
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD140..0x005FD149
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD150..0x005FD163
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD170..0x005FD182
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FAEEA..0x005FAF13
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-190 publishes the shared record's field addresses, not their
contents: `0x0242F630` receives the address of field +4 and `0x0242F650`
the address of field +8. The consumer at `0x005FD120` loads the former
global, reads the dword through it, pushes that value and calls
`0x005FD0C0` at `0x005FD12E`. The consumer at `0x005FD150` loads the
latter global, reads its dword, pushes the value and calls `0x005FD140`
at `0x005FD15E`. The bounded prefix of that callee establishes EBP,
pushes two dwords from ECX, and calls through its first stack argument
at `0x005FD145`. Its continuation is outside this finding's reading.

The procedure at `0x005FD170` establishes EBP, reads the slot address
from `0x0242F630`, reads its old dword into EAX, and writes the incoming
dword at `EBP + 8` through that same slot address. It restores EBP and
returns at `0x005FD181`, with no immediate stack cleanup. There are no
calls in this bounded body. The returned value is the old slot content;
caller consumption and incoming value provenance remain unread.

The procedure at `0x00600520` loads the shared record from `0x0242F640`,
temporarily saves and restores EBP, reads the dword at record +4 into
ECX, and jumps through ECX at `0x0060052C`. It does not dereference the
published field-address global. The saved EBP is removed before the
tail transfer, leaving the incoming return address in place. There is
no local pointer or extent guard. Its target is consequently the current
field value, rather than necessarily FND-EXE-190's initial target.

Another positive consumer at `0x005FAEEA` reads the slot addressed by
`0x0242F650` and stores its current dword at EDX +8; it then reads the
slot addressed by `0x0242F630` and stores its current dword at EDX +12.
This is a bounded copy observation, not admission of EDX's destination
extent or its later consumers. Unrelated intervening field stores are
not interpreted here.

## Interpretation

These consumers distinguish slot addresses, copied targets and tail-call
targets. The explicit setter proves a possible post-publication writer
to record +4. The initial field value alone cannot resolve every later
dispatch. Q-EXE-001 retains setter callers and argument provenance,
other writers/aliases, copied-target consumers, field +8 mutation,
callee continuations, shared-record extent and lifetime. No complete
reading or claim about teardown follows from these positive observations.

## Alternatives

Calling a published global directly would skip a required dereference.
Treating the newly allocated record's initial callback as immutable would
ignore the setter. Conversely, a writable callback slot does not show
that any particular caller replaces it or frees its containing record.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c` in
the saved PE project, read-only with analysis disabled. Run
ReportInstructionWindow at `0x005FD120`, count 12; `0x005FD150`, count
24; `0x005FD140`, count 5; and `0x00600520`, count 8. Restrict each
claim to its declared half-open interval: windows can continue across
gaps into neighboring bodies, and a gap proves neither non-return nor
absence of executable bytes. Run ReportInstructionContext at
`0x005FAEEA` and `0x005FAF09` for the two positive slot copies.
Follow the extra dereference and the setter's EBP-relative argument,
old-value return and cleanup explicitly. Rich instruction reports stay
in the local licensed-source store, outside Git. No original-game
execution is involved.
