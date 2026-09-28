---
id: FND-CONFIG-070
title: Overlay fixups to the message overlay are direct calls, not stored caller pointers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0020..566A:002A
tool: Python 3.14.7 bounded FBOV fixup-table inspection; Capstone 5.0.7 16-bit instruction checks
environment: null
---

## Observation

The shipped `DSUN.EXE` has 80 overlay-code fixup words whose encoded
descriptor index is 172. Sixteen of those fixups lie in other overlays and
target overlay 172 entries other than the message entry: two direct far calls
target `566A:0020`, and fourteen target `566A:0025`. The 56 direct far calls
to the `566A:002A` message entry are the sites in FND-CONFIG-035. Thus all 72
fixups to descriptor 172 in *other* overlays are segment operands of `0x9A`
far-call instructions. None is an address-taking fixup that stores a pointer
to the message entry.

The remaining eight fixups lie within overlay 172 itself. Each occurs in an
immediate segment push, not a far call to the message entry. This count is
limited to declared overlay-code fixups; it does not inventory resident
relocations, unrelocated values, or computed pointers.

## Interpretation

The overlay-to-overlay fixup table contributes no indirect caller of the
message entry beyond the 56 direct sites already catalogued. FND-UI-037 and
FND-SAVE-010 separately identify the Save Game UI callback and keyboard
route into one of those sites (FND-CONFIG-046). This does not establish the
incoming paths of the other shared sinks.

## Alternatives

Resident code or a pointer assembled at runtime may still call the message
entry indirectly. The eight self-fixups may serve other overlay 172 entries;
their downstream use is not established by this classification.

## How to reproduce

In the approved `DSUN.EXE`, decode the FBOV descriptor table and each
overlay's declared code and fixup table as FMT-EXE-002 and FMT-EXE-003
describe. Select fixup words whose encoded descriptor index (`word >> 3`) is
172. For each, inspect the preceding opcode and offset operand in its
declared code range. Group the other-overlay `0x9A` calls by target offset
`0x0020`, `0x0025`, and `0x002A`, and inspect the eight fixups in overlay
172 separately. FND-CONFIG-035 gives the 56 physical message-call offsets.
