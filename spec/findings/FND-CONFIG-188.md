---
id: FND-CONFIG-188
title: The pointer replacement service has null and nonnull state paths and returns zero after word-gated services
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:12ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0B84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0942
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and header-derived MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-187 passes current far DS:45A6 and word
zero to resident 3D72:12ED after its resource request,
without a transfer-result or own pointer-null gate.
FND-CONFIG-179 and FND-CONFIG-186 name other guarded
calls to this service. Its complete span is
`0x00033C0D..0x00033C85`, ending with far return at
`0x00033C84`. Its inputs are a far pointer at BP+6
and a word at BP+0A. After its stack-limit guard through
1000:2E48, it tests the full stacked far pointer.
DS-relative fields below mean DS at each instruction.

The null-pointer branch first calls local far 0B84.
It then tests current far field DS:A149, and, only when
that is nonzero, current far field DS:A14D. Both nonzero
pass the latter pointer to 444C:0092. The returned
DX:AX is ignored. After that returning call or a skipped
release, it copies current far DS:A145 into DS:A14D,
clears double word DS:A149 and clears double word
DS:A151. The latter clear includes words A151 and
A153. The release wrapper's explicit zero after a
returning runtime call is bounded in FND-CONFIG-184;
a cleared field does not prove a successful release or
accepted default pointer.

The nonnull branch also first calls local far 0B84.
It then stores the current stacked far input at DS:A14D,
clears double word DS:A149 and stores a zero-extension
of the current stacked word argument at double word
DS:A151. This branch does not perform the null branch's
own old-pointer release. It neither validates source contents
nor takes a destination-capacity argument. Stack and state
preservation through the earlier call remain conditions on
the stores retaining the originally supplied values.

Both branches then call local far 0942 and explicitly
return AX zero if it returns. The two local service
results and any release result provide no failure gate.
There is no own 0DAB, 1440, 0FCC or 0FCF store.
Own SI/DI preservation is not supplied by this wrapper;
its guard, local services and possible release effects
remain conditions on those caller registers. Its zero
result is a local normalization, not proof of performed
presentation, accepted bytes, completed release or stable
callback state.

The complete 0B84 span is
`0x000334A4..0x000336A3`; 0942 spans
`0x00033262..0x0003347F`. Each saves SI/DI, has a
stack-limit guard, then uses a word gate: 0B84 skips
its active work for nonzero current word DS:332E;
0942 skips for nonzero current word DS:332C.
FND-CONFIG-187's bracket helpers write only bytes at
332E and 332C, leaving neighboring bytes 332F and
332D untouched by those own writes. Their zero low
bytes do not establish zero words. For example, low
byte zero and high byte one leaves a nonzero word and
skips the service; two zero bytes admit its following
mode/handle work. These are conditional width cases,
not observations of the original neighboring bytes.
FND-CONFIG-189 reads both active service bodies and
their callback, handle and state-commit gates.

All resident external call operands above were verified
through header-derived MZ relocations. The local far
calls use push-CS/near-call frames. The null/nonnull
branch is chosen before 0B84, while the later state
and stacked inputs are read after that returning call;
therefore caller inputs alone do not freeze the complete
subsequent state. No native or emulated outcome is claimed.

## Interpretation

The replacement wrapper separates a null reset path from
a nonnull assignment path, then normalizes returning
completion to zero regardless of the local service results.
Mixed byte writes and word guards make adjacent-byte
provenance necessary. The caller's low-byte bracket and a
zero returned AX cannot by themselves establish that either
active service ran or accepted the replacement pointer.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
stack/pointer/default/marker/index and byte/neighboring-byte
producers, SI/DI/DS preservation, guard and runtime effects,
capacities, aliases, callbacks and actual presentation/I/O.
One reading supplies zero high bytes and admitted service
state; another has a nonzero high byte so active work is
skipped despite a cleared low byte. Complete field producers
and native state evidence would distinguish their reachability.

A reading that null always requests an old-pointer release
is ruled out by its two nonzero-field gates. A reading that
nonnull always releases the earlier pointer is ruled out by
that separate branch. A reading that AX zero confirms
accepted replacement is unsupported: own normalization follows
unchecked services and active-work bypasses. Complete callers
and state effects remain open; no rendered identity is assigned.

## How to reproduce

Read resident 3D72:12ED through 1364 from its entry.
Verify the guard and release MZ segments and local far
frames. Map BP+6/+0A, follow the pre-call pointer split,
post-call field/input reads, conditional release, double-word
clears and zero-extension, then the unchecked 0942 call
and explicit AX zero. Compare 0B84's word 332E gate
and 0942's word 332C gate against FND-CONFIG-187's
byte stores. Keep both neighboring bytes, mode/handle
conditions, aliases, register/stack preservation and actual
release/presentation outcomes separate from the zero result.
