---
id: FND-EXE-095
title: PATH final output helper separates replacement, alias copies and pointer returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D5750..0x006D5829
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-093 supplies output object, local record source and full byte count
to `0x006D5750`. This helper saves those three inputs and compares count
unsigned with `0x3FFFFFFC` before dereferencing the object. Greater counts
call `0x005F77D0` with `0x0075447C`; FND-EXE-036 records that failure
construction boundary. Its unexpected normal return joins the object-read
route without revising the count. Native failure outcomes remain conditional.

The ordinary route reads payload through the object and signed full word
four bytes before payload. A positive word selects replacement. Otherwise
source less than payload unsigned also selects replacement. Remaining paths
form payload plus preceding length word at payload minus twelve, with
32-bit wrap. Source above that computed end unsigned selects replacement;
source equal to either endpoint remains locally admitted. These pointer
comparisons do not prove allocation identity or source-plus-count bounds.

Replacement calls `0x006D69A0` with saved object, zero, the freshly read
preceding length and requested count; FND-EXE-096 records its ordinary
replacement/publication branches. On normal return a count of zero skips
copying. Count one freshly reads the object's current payload and copies
one source byte directly. Other nonzero counts call memcpy thunk
`0x00601CB0` with fresh payload, source and requested count in its first
three slots, plus an auxiliary current-EAX word. FND-EXE-034 identifies
the actual slot `0x024319DC` as memcpy. This caller does not test its return.
These routes restore the frame and return the saved object pointer in EAX,
not a Boolean, copied length or payload pointer.

For the locally admitted alias range, it computes source minus payload
at 32-bit width. A displacement at least requested count unsigned selects
memcpy with payload, source and count, plus the displacement in an auxiliary
slot. A nonzero displacement below count selects memmove thunk
`0x00601D40` with the same first three slots and displacement auxiliary.
FND-EXE-021 identifies its exact import slot `0x024319E0` as memmove.
Each normal call then freshly reloads payload through the object. A zero
displacement skips both calls and retains the previously read payload.
Imported effects and aliases remain conditional; no local return test occurs.

All these alias routes write full zero at payload minus four, requested
count at payload minus twelve, then a zero byte at payload plus count.
They return the saved object pointer. These writes occur after any selected
copy/move and before return. There is no independent local guard proving
the final terminator fits or that the source region contains count bytes.
An equal source and payload is not a no-op: it still publishes the two
preceding words and terminator. The direct helper has no stored-handler
setup of its own; its callees retain their separate exceptional boundaries.

## Interpretation

The final PATH output call chooses replacement or alias handling, then
ordinarily returns an object pointer. FND-EXE-093 discards that return and
supplies its own one, so truthy caller success does not come from a tested
copy status. Q-EXE-009 retains object/payload producers, preceding-word
meaning and lifetime, valid source/count ranges, replacement handlers and
CRT/alias effects. No actual PATH string or complete safe output contract
is established by these local decisions.

## Alternatives

Returning a normalized success flag, validating source-plus-count from the
source endpoint comparison, always using memcpy on an overlapping source,
or omitting publication for identical source and payload is ruled out or
unsupported. The count-one direct copy applies only after replacement;
it does not replace the alias branch's displacement admission. Greater-than-
limit failure does not establish a returning-null result or rollback.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-093's final call inputs.
Read sixty-five instructions at `006D5750` and seventy at `006D57E8`,
restricting claims through `006D5829` before the next function. Read two
at each copy thunk, retaining its first jump; use FND-EXE-034/021 for exact
physical import provenance and FND-EXE-036 for failure construction.
Trace full signed preceding-word gate, unsigned pointer/count comparisons,
wrapped endpoint, each normal reload, explicit versus auxiliary slots,
store order and restored pointer return. Keep imported results and
replacement admission conditional. Keep reports local and run no original.
