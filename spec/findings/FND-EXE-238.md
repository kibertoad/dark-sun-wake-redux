---
id: FND-EXE-238
title: Priority cleanup path gates a far callback before a full-width segment product
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BCF..4AE5:0BFE
tool: executable-reader 2.5.0, Capstone 5.0.7 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-237's priority callee at `4AE5:0BCF` decodes from the shipped source
as 24 instructions and 47 bytes, ending exclusively at `4AE5:0BFE`.
Its saved resident listing has no instruction at that start; the missing
window does not establish absent source code.

It loads AX with a relocated segment immediate and calls `4AE5:0C2E`.
The immediate's shipped-file word at `0x00040C20` has an MZ relocation;
under load segment `0x1000` it resolves to `55E1:0000`, shipped-file offset
`0x0004B010`. This gives the operand's source mapping, not the callee's
interpretation or admission of the referenced storage. A returned carry
set branches directly to the near return at `4AE5:0BFD`.

Otherwise the path saves ES, AX, BX, CX and DX, loads AX with one, and calls
through the far pointer at current DS-relative offset `0x0086` at
`4AE5:0BDF`. Both pointer words and their writers remain unresolved here.
It then pops DX, CX, BX, AX and ES in reverse order. Balanced saves do not
establish unchanged saved slots or returned DS across the unread callback.

Next it saves CX, reads current ES-relative word `0x0010` into AX, sets DX
to sixteen and performs unsigned sixteen-bit multiplication. The product
occupies the full DX:AX pair; it is not a truncated sixteen-bit shift.
It restores CX and calls `4AE5:11A9` at `4AE5:0BF3`. A returned carry clear
branches to the near return; carry set instead takes a far tail transfer
at `4AE5:0BF8`. Its segment word at file `0x00040C4B` is MZ-relocated and,
with target offset `0x02DF`, resolves to `1000:02DF`, shipped-file offset
`0x000054DF`. The near return has no extra argument cleanup.

The bounded CFG lists both direct calls, the unresolved computed far call,
the near return and the far tail transfer. It assumes all three calls return.
The two carry tests consume different callees' results; neither proves the
meaning of an error nor that all paths return normally to the cleanup caller.

## Interpretation

This supplies the priority cleanup path's explicit gate, callback, save/restore,
product width and error-transfer order. It does not establish allocation or
release semantics, effective storage identity, saved-slot preservation, the
callee return conventions beyond the caller's carry tests, or preservation
of ES before FND-EXE-235's later clear.

Q-EXE-001 and Q-EXE-010 retain native caller and DS/ES admission, both direct
callee effects, the far pointer's complete writers and target, stack aliases,
input-field writers, error-target behavior and interrupt-enabled changes.
No complete_reading or replacement inventory is established.

## Alternatives

Unconditional far-callback invocation is contradicted by the initial carry
gate. A sixteen-bit-only product ignores unsigned multiplication's high
word. Guaranteed normal return ignores the far tail transfer. Balanced pops
cannot prove preserved stack contents across an unresolved callback.

## How to reproduce

At revision `65e7a1a`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, setting entry and sole entries value to
`0x00040C1F` (`4AE5:0BCF`), with no seeds or summaries. Check all 47 bytes,
24 instructions, three returning-call assumptions and both exits.

Independently hash-check the source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Decode file interval
`0x00040C1F..0x00040C4E` using Capstone 5.0.7 in x86 sixteen-bit mode with
initial IP `0x0BCF`; inspect the instructions above. Using the committed
operand command on the same source, sourceKind mz, resolve site `0x00040C20`
with targetOffset zero and site `0x00040C4B` with targetOffset `0x02DF`.
Both must identify MZ relocations and the loaded addresses above.

The resident snapshot, read-only with analysis disabled, reports an
undisassembled start for ReportInstructionWindow at `4AE5:0BCF`, count 28.
Keep that limitation separate from source decoding. Sources, configurations,
listings and reports remain in GAME_DIR.
