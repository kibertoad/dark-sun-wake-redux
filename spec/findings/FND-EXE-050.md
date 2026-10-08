---
id: FND-EXE-050
title: Field and nested-object callers continue without testing record setup or cleanup returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6C70..0x006D6CEC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6CF0..0x006D6D1F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6D42..0x006D6D63
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE630..0x005FE69E
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-032's field helper forms a stack-local address at frame offset
minus 124 and passes it to FND-EXE-167's setup helper. Before that call it
stores other local record values, including the handler target
`0x006D6D20` at frame offset minus 88. This target is stored, not called
by the direct ordinary path studied here. The caller does not initialize
the first word at the passed record address before setup; FND-EXE-167
describes setup's mode-dependent write to that word.

On normal setup return the very next instruction loads the original source
argument into the return register, then dereferences it. There is no result
test or saved setup result between the call and that overwrite. The source
and destination argument reads described by FND-EXE-032 therefore follow
normal return regardless of the particular returned word. This does not
assert normal return when the helper or an imported operation fails exceptionally.

Both publication paths pass the same local record address to
FND-EXE-049's cleanup helper after their destination store. The nonnegative
fixed-address arm reaches the first cleanup call directly. The other
nonnegative arm calls FND-EXE-033's addition helper and joins that same
publication/cleanup sequence. The negative arm has its own cleanup call.
After either cleanup call, the caller adjusts the outgoing stack region,
restores its saved registers and frame, and returns. It does not locally
test, store or explicitly overwrite the returned register. Consequently,
this local caller does not gate publication or roll it back based on cleanup's
returned word, but it also does not prove that a higher caller ignores it.

FND-EXE-037's nested-object helper similarly passes a stack-local record
at frame offset minus 64 to setup, after storing the handler target
`0x005FE6A0` at frame offset minus 28 and saving the destination elsewhere.
Its direct pre-setup path does not initialize the passed record's first word.
On normal setup return a stack pop overwrites the return register; the next
load replaces it with the saved destination. No setup-result test intervenes.
The caller proceeds with the initial object-word write and field-helper call
recorded by FND-EXE-037.

After that field-helper call, the nested helper replaces the return register
with its local record address and writes that address to the first outgoing
slot for cleanup. Thus the field-helper's returned word is not tested or
retained locally. After cleanup it adjusts the stack and restores the frame
and saved registers without testing, saving or explicitly overwriting the
return register. This leaves the cleanup result available on ordinary return;
its eventual interpretation requires callers above this nested helper.

The local record addresses occupy different frames in the two helpers.
The evidence does not turn their first words into the same storage, or prove
that they cannot alias object or source storage through external effects.
The stored handler targets and local state words need their own consumer
reading before assigning a complete exception-record layout.

## Interpretation

These direct caller paths lack a local success gate for setup and cleanup.
They distinguish immediate overwriting of setup results from unchanged
cleanup-result propagation through an epilogue. Q-EXE-009 retains higher
caller consumption, handler entry, record fields and lifetime, alias admission,
exceptional control transfer and concurrent effects. Ordinary call order is
not proof of transactional publication or a complete record lifecycle.

## Alternatives

A setup-return test before reading the field arguments or writing the object's
initial word is ruled out in these paths. A cleanup-return test that locally
reverses destination publication is also ruled out. Calling cleanup's result
universally discarded is not justified: these epilogues leave its return
register unchanged. A stored handler target is not evidence that the ordinary
path invokes it, and adjacent local stores do not establish every record field.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read seventy instructions from
`0x006D6C70`, eight from `0x006D6D50`, and forty from `0x005FE630`.
Restrict claims to the cited ranges, excluding following functions and the
unread handlers. Follow both passed local addresses, first-word pre-call
writers, setup-return last writers, both field branches and the addition
branch's join, outgoing cleanup slots and the register writes through each
epilogue. Use FND-EXE-032, FND-EXE-033 and FND-EXE-037 for publication,
and FND-EXE-167, FND-EXE-048 and FND-EXE-049 for record/import behavior.
Keep normal return and indirect effects conditional. Keep rich reports local
and execute no interpreter or game.
