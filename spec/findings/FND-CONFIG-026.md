---
id: FND-CONFIG-026
title: Literal difficulty-word writers are in load, Preferences and a guarded overlay branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: bounded physical-byte search and Capstone 5.0.7 16-bit disassembly with Python 3.14.7; Ghidra 12.1.3 ReportReferences.java
environment: null
---

## Observation

A physical search of the approved `DSUN.EXE` for the two-byte displacement
`3A 14` of `DS:143A` finds 19 candidates. Bounded instruction contexts
identify six direct writes to that difficulty word:

| File offset | Write | Path |
|---|---|---|
| `0x00058F18` | literal 3 | Guarded branch in overlay 171 (FND-CONFIG-120). |
| `0x0007D981` | `AX` from a local save-data buffer | Load Game after a successful `PREF/100` read (FND-SAVE-005). |
| `0x0008B610`, `0x0008B615` | add 2, then subtract 1 | Preferences harder-arrow branch (FND-CONFIG-010). |
| `0x0008B620`, `0x0008B62D` | literals 3 and 0 | Signed upper and lower clamps in the same Preferences dispatcher (FND-CONFIG-010). |

The other physical candidates are reads or do not encode a direct write to
`DS:143A` in their bounded contexts. Ghidra's mapped reference query for
`5000:923A` recognizes the guarded overlay write and the Preferences
dispatcher write, but does not supply a complete writer inventory by
itself.

## Interpretation

Among physical instructions that encode this exact displacement, no
separate new-game initialization write was found. This narrows the
new-game default question to the executable's loaded image, an indirect
or computed write, a block copy, or the guarded overlay path if that path
runs with its write condition met.

## Alternatives

The absence of another literal displacement write is not evidence that a
new game keeps the loaded-image value zero. This search cannot rule out
whole-structure initialization, a write through a register or pointer,
or a call into the guarded overlay branch through indirect dispatch.
The caller and input state of that branch remain untraced (Q-CONFIG-002).

## How to reproduce

Search the approved `DSUN.EXE` for physical bytes `3A 14`. Inspect each
of the 19 hits in bounded 16-bit instruction context, distinguishing
operands that read from those that write to `DS:143A`. Confirm the six
write instructions at the file offsets above. Compare the load, click
and guarded branch contexts with FND-SAVE-005, FND-CONFIG-010 and
FND-CONFIG-120. A mapped-image `ReportReferences.java` query for
`5000:923A` is an additional cross-check, not an absence proof.
