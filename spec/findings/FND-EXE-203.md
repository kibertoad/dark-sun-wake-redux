---
id: FND-EXE-203
title: Gate-neighbor allocation helper forwards one full request without a local index-register write or retry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005BE710..0x005BE724
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CF0..0x00601CF6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005B860B..0x005B8622
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-202's first allocation call reaches `0x005BE710`.
Its complete direct body saves EBP, establishes its conventional frame,
reserves twenty bytes, reads the full dword at frame offset eight,
pushes that word and calls `0x00601CF0`. After normal return it adds
sixteen to ESP, restores its frame with LEAVE and returns near with
no immediate argument removal. There is no local size conversion,
zero-request substitution, null test, retry, indirect local callback
or store through the returned pointer. It returns the import's EAX
word unchanged by subsequent local instructions.

The six-byte target thunk jumps through `0x024319D0`, identified by
FND-EXE-024's physical import reading as msvcrt.dll malloc. This is
the same imported slot used by the separate construction/retry wrapper;
that wrapper's setup, zero substitution and retry paths are not part
of this small helper. An import identity does not identify the loaded
CRT implementation or establish its effects.

Let E be ESP at this helper's entry. Its saved frame is E -4 and its
first argument's storage is E +4. The twenty-byte reservation leaves
ESP at E -24; the outgoing argument push leaves E -28, and the call
places the import return address at E -32. Under normal return with
no callee argument removal, ESP is E -28 afterward. The sixteen-byte
addition puts it at E -12; LEAVE resets it to the saved frame and
restores EBP, making ESP E before the near return. The remaining local
reservation is discarded by that reset, rather than by a second ADD.
The caller's own outgoing argument is not removed by this helper.

No direct instruction in this helper or thunk writes ESI or EBX.
Neither is saved locally, so their preservation rests on the imported
execution, not on a local restore. Its frame and argument identity,
return, EAX result and stack trace likewise require valid stack storage
and the imported call's admitted effects. SRC-WIN32-X86-ABI describes
ordinary external register-preservation and caller-cleanup contracts;
it does not observe this historic loaded library or exclude exceptional
or external writes.

For FND-EXE-202's guarded table publication, the direct caller pushes
516, the helper forwards those same four bytes without local scaling,
and the caller stores the full returned EAX word using ESI after
removing twelve outgoing bytes. Conditional on the import preserving
ESI and the ordinary frame/argument contract, the checked index
therefore survives this local wrapper. FND-EXE-202's disjoint table
interval then applies to that store. There is no local recheck if those
conditions fail, and no native register change or gate corruption is
claimed. Allocation unit, returned storage, lifetime and aliases of
later pointed-to writes remain separate obligations.

## Interpretation

This resolves the intervening helper's direct register, argument,
return and cleanup behavior. It narrows the remaining index-preservation
boundary to the import and admitted stack/storage model on this route.
Q-EXE-009 retains loaded-library effects, alternate entries, indirect
writes, destination contracts and the gate's actual initialization.
No complete_reading declaration follows from the small local body.

## Alternatives

Substituting FND-EXE-024's larger retry wrapper would introduce setup
and request mutation absent here. Treating the sixteen-byte ADD as
complete removal of the twenty-byte reservation plus the push would
miscount ESP; LEAVE performs the remaining reset. The absence of a
local ESI write is not proof that the import preserves it.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x005BE710`, count nine; at
`0x00601CF0`, count one; and at `0x005B8600`, count nine.
Restrict claims to the cited spans. Check ReportCitationBoundaries for
`005BE710..005BE724:return` and `00601CF0..00601CF6`.
Use FND-EXE-024 for the independently mapped import slot.

Track ESP from entry through the saved frame, reservation, outgoing
push, call, normal return, partial ADD and LEAVE. Compare the full
input read and push with the caller's last-written request and keep
ESI preservation at the import boundary distinct from direct local
writers. Endpoint controls do not establish caller completeness,
source identity, imported effects or valid storage. Keep rich reports
local and execute no original program.
