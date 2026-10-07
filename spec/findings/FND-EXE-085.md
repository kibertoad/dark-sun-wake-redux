---
id: FND-EXE-085
title: Third cleanup target visits twenty-six slots in reverse and retains their direct pointer fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8590..0x004A8625
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8630..0x004A863E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8640..0x004A864B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB320..0x002EB323
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with physical constructor and cleanup target provenance
environment: null
---

## Observation

FND-EXE-082 physically resolves cleanup slot two to `0x004A8640`. The
wrapper sets EDX 65535, clears EAX to zero and tail-jumps to `0x004A8590`.
Independently, FND-EXE-080's constructor slot four at virtual `0x006EBF20`,
shipped offset `0x002EB320`, stores `0x004A8630`. That wrapper supplies
EDX 65535 and EAX one to the same helper. No new return address or arguments
are added by either tail jump.

The helper saves incoming EDX in ECX and incoming EAX in EBX. Its first
gate combines byte results from full comparisons of EDX with 65535 and
EAX with one. Its later gate separately compares the saved full EDX with
65535 and tests the saved full EAX for zero, combining those equality
results as bytes. These are exact fixed-input selection gates, not tests
of the original low bytes alone.

### Cleanup wrapper's fixed path

The zero/65535 inputs bypass the first group and admit the second. The
working slot pointer starts at `0x01BA4C40`, the exclusive end of the
half-open interval beginning `0x01BA4AA0`. Each iteration compares the
working pointer with the start, subtracts sixteen if it is not at the start,
then loads that slot's first full word. Zero skips the call and returns to
the start comparison. Nonzero supplies the loaded word in the first
outgoing slot and calls `0x005F9910`, FND-EXE-024's direct-pointer free wrapper.
The returned word is not tested. The post-call start comparison decides
whether to subtract for another slot.

Under normal returning calls that preserve the working register, the
interval spans 26 sixteen-byte slots. The first read is `0x01BA4C30`, then
addresses decrease by sixteen through `0x01BA4AA0`, which is included.
Zero slots do not end the scan. No runtime item count, signed bound,
per-slot length or field at offset four, eight or twelve admits a release.
The loop reads only each first pointer word for that decision.

The caller itself writes no slot to zero and does not restore a released
pointer on failure. The contents may nevertheless change through callees,
aliases or other actors; no all-writer or immutable-table proof is supplied.
Repeated cleanup with identical nonzero contents would reach the same release
boundary again, but neither repeat admission nor valid ownership is established.
The pointers must meet FND-EXE-024's unread external free contract. Reaching
a known free thunk is not proof that every release succeeds safely.

The final return is not normalized: if the last slot is zero, its loaded
zero remains EAX; if nonzero, the last normally returning free boundary's
raw EAX remains. FND-EXE-082's parent ignores this result by reloading its
cursor. Its next callback therefore is not admitted by a successful-release
predicate.

### Independently admitted constructor path

One/65535 inputs admit the first group. Starting at `0x01BA4AA0`, the helper
writes full zeros at offsets zero, four and eight, then advances sixteen.
Its work counter begins at 25, decrements before those latter two stores,
and loops until the decremented value equals all ones. This performs 26
iterations, not 25. The last three stores are at `0x01BA4C30`,
`0x01BA4C34` and `0x01BA4C38`. Offset twelve in every slot is untouched by
the direct loop, including `0x01BA4C3C` in the last slot. The second gate
then rejects cleanup because the saved original input is one.

The selected wrappers and complete helper have no local allocation,
capacity negotiation, table publication or slot ownership check. All slot
words fit FND-EXE-078's virtual-only BSS interval; they have no physically
backed initializer bytes. These source paths do not prove actual initial
contents, initialization order, populated slots or a constructor/cleanup
lifecycle for every actor.

## Interpretation

The third concrete cleanup callback now has an exact reverse-slot admission
and direct release boundary, distinct from FND-EXE-084's shared counter gate.
Its separately admitted constructor initializes three words per slot without
covering the fourth. Q-EXE-009 retains population, pointer ownership, all writers,
callee/alias effects, actual lifecycle admission and remaining callbacks.

## Alternatives

Scanning forward, excluding the first slot, stopping on the first zero,
iterating only 25 slots, clearing all four words per slot on initialization,
clearing each released slot locally, testing a release return to continue,
or returning a local success Boolean is ruled out by the direct helper.
The fixed source interval is not proof of its pointer contents or valid releases.

## How to reproduce

Verify FND-EXE-011's source identity. Resolve the cleanup target from
FND-EXE-082's slot two and the constructor wrapper from FND-EXE-080's
29-word table, checking the full word at `0x002EB320`. Read three instructions
at `004A8640` and `004A8630` and 43 at `004A8590`, restricting each wrapper
to its own tail jump and the helper to `004A8590..004A8625`. Recheck
FND-EXE-024's `005F9910` direct pointer/free boundary. Trace saved full inputs,
byte predicates, decrement-before-final-counter test, sixteen-byte stride,
exclusive end, inclusive start, zero-slot continuation, preserved working
register assumption and final EAX provenance. Recover only the physically
stored wrappers and their tail-reached helper; export start/body-size columns
and do not treat body byte counts as contiguous spans. Keep reports local;
do not call a callback, free a source pointer or run the original.
