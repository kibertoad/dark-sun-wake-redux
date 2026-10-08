---
id: FND-EXE-226
title: Initial resident handler candidate has direct loader leads and an unresolved far callback before interrupt return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:04F4..4AE5:055A
tool: executable-reader 2.5.0, engine 13.6.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

Starting at FND-EXE-176's initial relocated handler target `4AE5:04F4`
gives a 47-instruction, 102-byte conditional source traversal through
`4AE5:055A` exclusively. Its two listed exits are the far tail transfer
at `4AE5:04FD` to `1000:02DF` and the interrupt return at `4AE5:0559`.
The latter adds no argument cleanup beyond the interrupt-return operation.

| Call site | Target | Discovery assumption |
|---|---|---|
| `4AE5:051C` | `4AE5:05A4` | Callee returns to the next instruction |
| `4AE5:052D` | `4AE5:05A4` | Callee returns to the next instruction |
| `4AE5:054C` | Unresolved computed far target | Callee returns to the next instruction |

The bounded saved-listing reading shows a frame-base alignment test after
saving BP, before
the tail-transfer arm. The other arm saves registers, enables interrupts,
loads a far pointer from words relative to BP, and changes BP-relative
words around the two direct calls. Before the far call it clears bit three
of an ES-relative byte at offset `0x001A`, loads DS from CS-relative word
five, and calls through a far memory operand at offset `0x0086`.
Register restoration and the interrupt return follow that call.

The source traversal reports complete discovery with no CFG gaps, but
explicitly retains all three callee-return assumptions, including the
unresolved far call. It does not traverse those callees or establish their
effects. The saved-listing window also prints later instructions; they are
not part of this handler observation.

## Interpretation

This identifies the resident direct callee and unresolved callback that must
be read before attributing the saved-frame changes to an overlay transfer.
The far callback is a live dependency even though discovery reports complete.
The native installed vector, incoming interrupt frame, effective segments,
callee preservation and resumed target remain unresolved in Q-EXE-001 and
Q-EXE-010. Enabling interrupts also prevents treating this as isolated
state preservation. No native run, complete_reading declaration or inventory
replacement follows.

## Alternatives

The initial stored vector pointer is not proof that this handler remains
installed. A closed local CFG does not resolve the computed callback or
its return effects. BP-relative words are not identified as native interrupt
frame fields until entry and frame admission are established. Rendered MOV
operand direction is not used to establish the earlier DS initialization.

## How to reproduce

At revision `59408bf`, run x86-bounds on the installed original DSUN.EXE
with sourceKind mz and XXH3 `e296af55ba2ecde7e77f555c90f33d0b`.
Set entry `0x00040544` and one resident region: start `0x00040050`,
exclusive end `0x00041050`, segment `0x4AE5`, ip zero, entries containing
only `0x00040544`. Name it resident-handler-candidate and give FND-EXE-176's
initial relocated pointer as evidence, retaining unresolved live vector
and caller admission. Supply no seeds, indirect declarations, callee
summaries or interrupt models. Use FND-EXE-175's recorded default limits.
Retain assumedContinuations, calls, exits and complete together.

Read the original resident Ghidra snapshot with analysis disabled and
read-only mode. Run engine 13.6.0 ReportInstructionWindow at `4AE5:04F4`,
count 55, restricting this observation to the handler interval above.
The later printed suffix is not folded into its body. Source, configuration,
listing and reports stay in GAME_DIR and are not committed.
