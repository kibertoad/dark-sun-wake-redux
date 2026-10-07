---
id: FND-EXE-109
title: Callback consumer has dedicated word dispatch and clears six fields only after its value-two call returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A47C0..0x005A47FD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A48DC..0x005A496B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4970..0x005A4972
tool: Ghidra 12.1.3 PUBLIC bounded callback-consumer dispatch and value-two reading
environment: null
---

## Observation

FND-EXE-108 forwards an object and zero-extended shifted word to
`0x005A47C0`. This consumer pushes ESI and EBX and reserves thirty-six
stack bytes, making its first full input O current ESP plus forty-eight
and its second input's low word plus fifty-two. It zero-extends that word
before comparing full values. Value one selects `0x005A4800`; values
at most one under the signed comparison select `0x005A4970`, where
zero continues and nonzero goes to the default path. Since the source was
zero-extended, this comparison does not admit negative values. Value two
selects `0x005A48DC`; value seven selects `0x005A4A00`. All other
word values take the default path.

Default reads O's full first word as a table pointer and reads its full
offset-eight target. It writes O and the zero-extended word into the original
first and second argument slots, releases its local stack, restores both
saved registers and tail-transfers to the retained target. It does not bound
the target, normalize its result or establish a valid table/object lifetime.
Its entry return address is retained. Dedicated zero, one and seven branches
are not fully described by this finding.

Value two reads six full fields from O at offsets `0x013C`, `0x0134`,
`0x0138`, `0x0130`, `0x012C` and `0x0128`. The outgoing call to
`0x00520320` has first full argument `0x0073BF04`, second O's
current full `0x010C` field incremented modulo thirty-two bits, then
fields `0x0128`, `0x012C`, `0x0130`, `0x0138`, `0x0134` and
`0x013C` in that order. Each field is read before the call; no clearing
of those fields precedes it.

After an ordinary return the path publishes full zero to O plus `0x012C`
and `0x0128`, byte zero to O plus `0x0126`, then full zero to
`0x0130`, `0x0134`, `0x0138` and `0x013C`, in that order. It restores
the local stack and saved registers and returns full EAX zero from its
explicit zeroing. The callee's result is discarded. No local result test
or branch guards this normal-return clearing. The callee can have effects
before those stores; a nonreturn or exceptional transfer does not establish
that this clearing occurs. Object aliases, field meanings and callee effects
remain unread contracts.

## Interpretation

The fixed callback's downstream consumer distinguishes four dedicated inputs
from virtual fallback and has a concrete value-two call/publication sequence.
Its normal-return clear is not evidence of successful callee behavior.
Q-EXE-009 in FMT-EXE-006 still requires the dedicated zero/one/seven paths,
virtual targets, object-field producers, callee contracts and caller admission.
No complete consumer reading, format promotion or actual PATH outcome is claimed.

## Alternatives

- Signed comparison after zero-extension does not classify a high input word
  as negative; its full value remains zero through 65535.
- Default forwards a zero-extended word rather than the incoming full slot.
- Value-two clearing follows the call, not a success predicate or prior reset.
- The outgoing field order differs from ascending offsets and must not be
  inferred from their adjacency.

## How to reproduce

Verify FND-EXE-011's executable length/hash and FND-EXE-099's physical
controls. FND-EXE-108 independently supplies the direct call target. In the
saved Ghidra program with -noanalysis, use ReportInstructionWindow.java at
`0x005A47C0` limit 110 and at `0x005A4970` limit 150. Restrict the
observations to the three declared ranges; other branch readings are not
claimed here. Track both saved registers, local reservation, word load,
zero-extension before signed comparison, original argument slots on default
exit, every outgoing value-two field, post-call stores and explicit return
zero. Keep reports in GAME_DIR/analysis/exe-batches; commit no original
listings or bytes and execute neither the interpreter nor the game.