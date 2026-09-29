---
id: FND-CONFIG-155
title: The zero-mode pre-setup helper resets code-segment storage and restores flags through an internal return target
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:06CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0710
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit reads from the entry and its explicit internal call target
environment: null
---

## Observation

FND-CONFIG-149's DS:13F7 zero branch calls resident
4842:06CF before setup. Its complete local path begins at
`0x0003DCEF` and ends with far return at
`0x0003DD39`. It retains DS, SI and DI, saves flags,
and clears the interrupt-enable flag before its stores.

It clears words in its own code segment at offsets two,
four and 0E22. It then sets ES to CS and fills these spans:

| Code-segment span, half-open | Written value |
|---|---|
| 0126..0166 | 32 zero words |
| 0166..0186 | 16 FFFF words |
| 0186..01A6 | 16 zero words |

There is no stored-data write through DS and no write to
DS:193E or the three-byte table used by the iterator. The
code-segment fields remain unnamed; no gameplay or device
identity is inferred from their reset pattern.

At `0x0003DD2F`, the ordinary linear entry path decodes
an operation leaving BH's value unchanged but changing flags.
It then pushes CS and near-calls internal offset 0710 at
`0x0003DD33`. That explicit target is file offset
`0x0003DD30`, inside the preceding linear instruction's
bytes. Decoding from the target yields a one-byte IRET.

The stack at that target contains the call's return offset,
the just-pushed CS and the flags saved before the reset. The
16-bit real-mode interrupt return therefore restores that local
continuation and the saved flags. It does not invoke an external
interrupt handler. The continuation pops DI, SI and DS and
far-returns. A linear decode alone does not describe this
internal call path; both reachable starts are needed.

## Interpretation

This concrete zero-mode pre-setup helper does not itself
populate FNFO buffers or assign the setup gate byte. Its local
stores are in code-segment storage and it restores the saved
DS and flags. This rules out that direct local producer on
FND-CONFIG-149's zero-mode branch, while leaving earlier
state and the other branch's helper open. The helper's call
is not evidence of an OBJEX.GFF registration.

The verified internal call also shows why overlapping decodes
cannot be rejected solely because another entry path starts
an instruction earlier. The target is supported by an explicit
control-flow edge and matching stack continuation; arbitrary
locally decodable overlaps in FND-CONFIG-148 remain rejected.

## Alternatives

Q-CONFIG-008 retains earlier gate producers, the nonzero
DS:13F7 helper, archive registration and later state. This
bounded helper reading cannot establish the byte's value when
it is entered, which caller path is reached, or the effect of
external asynchronous activity. No native execution or complete
startup invariant is claimed.

## How to reproduce

Read 4842:06CF from `0x0003DCEF` through
`0x0003DD3A`. Follow the explicit CS stores, repeated
word spans, saved registers and flags. Resolve the near call
at `0x0003DD33` to 4842:0710 and decode that target
separately, checking the return-offset, CS and saved-flags
stack order and the following pops. Do not force a single
linear instruction-boundary set onto both reachable paths.
Compare the local branch join in FND-CONFIG-149 and retain
its earlier-state and nonzero-branch dependencies.
