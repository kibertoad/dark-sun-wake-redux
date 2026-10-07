---
id: FND-EXE-086
title: Resource handler and ordinary setter publish status before distinct onward gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D2191..0x006D21BF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D2211..0x006D2255
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006E7BF0..0x006E7C24
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with explicitly stored handler recovery
environment: null
---

## Observation

FND-EXE-084 identifies the stored target `0x006D2191` in the resource
helper's local record. Recovery of that exact target exposes a handler that
adds twelve to incoming EBP, copies adjusted-frame word -56 to local -76,
and compares full adjusted-frame state -60 with one. Actual handler entry
and identity of this adjusted frame are not established by the stored value.

For a state other than one, it first writes zero to state -60 and calls
`0x005FACB0` (FND-EXE-070). It then supplies saved local -76 to
`0x00600EB0` (FND-EXE-052), writing all ones to state -60 before that call.
If this forwarding helper unexpectedly returns normally, the physical
continuation joins the ordinary combined-status route at `0x006D21C0`.
Its presumed nonreturn does not establish native handler behavior.

For exact state one, it supplies saved local -76 to `0x005FABA0`
(FND-EXE-067), ignoring that normal return. It freshly reads adjusted-frame
local -68, loads its first word and then the adjustment word twelve bytes
before that pointed table, and adds the adjustment to the local pointer
with 32-bit arithmetic. It reads full word 20 of this adjusted object and
ORs in bit zero. It tests bit zero of byte 16 before storing the combined
full word back to offset 20; the store preserves the flags used by the
following branch. Both outcomes therefore have the status store in place.

If that tested bit is clear, it writes all ones to state -60, calls
`0x005FACB0`, and jumps to `0x006D2170`, reusing the local -72 test recorded
in FND-EXE-084. This route does not reset that local word; its value must
not be inferred from ordinary entry initialization. If the tested bit is
set, it writes one to state -60 and calls `0x005FAF40` (FND-EXE-087).
There is no decoded normal continuation before the next function at
`0x006D2260`. Neither arm locally undoes the status store before its call.

The separately called setter `0x006E7BF0` reads its first stack argument
as a pointer and its second as a full requested word. It unconditionally
reads pointer offset 120. If this word is zero, it ORs bit zero into the
requested word; otherwise it keeps the requested word. It stores that
effective full word to pointer offset 20 before reading full mask word
16 and testing overlap with the effective word. Zero overlap returns the
effective word in EAX without Boolean normalization. Nonzero overlap
rewrites the original first argument slot to `0x00754560`, restores EBP
and tail-jumps to `0x005F6F10` (FND-EXE-088), retaining the caller return
address. This direct body reads only the first two arguments; the extra
slots supplied by FND-EXE-084 do not establish additional consumed inputs.

## Interpretation

The ordinary setter and stored handler use different admission widths and
orders: full-word overlap after status publication versus a byte bit test
before the status store whose flags select the later branch. Their local
stores and exact tests are established observations, not a successful
resource operation, complete handler admission, flag meaning or lifetime.
Q-EXE-009 retains those frame, pointer, alias, indirect-target and native
continuation questions. Callee writes can affect later fresh loads.

## Alternatives

Returning only a Boolean, testing overlap before the ordinary status store,
testing every mask bit in the handler, or reverting the status before its
terminal call is ruled out locally. Assuming local -72 remains its ordinary
zero initialization on every handler entry is unsupported. A stored target
alone does not prove that incoming EBP refers to the ordinary helper frame.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-084's record writer.
Use ReportInstructionContext at `006D212C` as the stored-target control.
Recover only `006D2191` with RecoverCitedFunctions. Read windows of twenty
instructions at `006D2191` and `006D2211`, and eighty at `006E7BF0`.
Restrict observations to the cited regions; exclude subsequent functions
and undecoded gaps. Follow state width, adjusted-frame accesses, the last
flag producer across the status store, argument replacement and tail return
provenance. Use FND-EXE-084 for the ordinary continuation, and the cited
callee findings for their separate limits. Keep all reports local; run no
original program and assume no handler admission or valid pointer.
