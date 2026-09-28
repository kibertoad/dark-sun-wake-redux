---
id: FND-CONFIG-043
title: Ammo and broken-item branches call the shared message routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5671:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Three direct calls in overlay 173 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035):

| Call file offset | Passed text | Local condition |
|---|---|---|
| `0x0005D041` | `DS:05BA`, `Out of ammo` | `[BP-10]` exceeds `[BP-8]`, so it is clamped to `[BP-8]`; that second word is zero. |
| `0x0005D36E` | `DS:05BA` | The indexed record's flag byte has bit `0x08` set and the word at `[BP-8]` is no greater than `[BP-10]`. |
| `0x0005E4CB` | A stack buffer formatted from `DS:05C6`, `%s is broken !` | Two local random-number scaling comparisons each fall below the threshold held in `DI`; the routine calls a local helper with the selected record before passing the text. |

Each site passes a far text pointer and removes four argument bytes after
the call. The third site's item name comes from either a pointer table or
a copied local string, depending on the selected record number; the
message formatting and the local helper call are in the same bounded
routine. The helper's effects and the full item-rule semantics are not established by this
message-call reading.

## Interpretation

These branches enter the shared message routine with ammo or item
feedback. Their local conditions are more specific than the syntactic
call-site inventory. The later `WIND/10501` acquisition still decides
whether the shared routine reaches its message-delay wait
(FND-CONFIG-018).

## Alternatives

The inputs to the ammo calculations, the selected item, the random
generator's distribution and the local helper's effects were not completely
read here. The call sites do not establish how often these branches
occur or whether the message window and wait succeed in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0005CFC0..0x0005D052`, `0x0005D345..0x0005D37D`, and
`0x0005E3BF..0x0005E4D7`. Resolve the calls at `0x0005D041`,
`0x0005D36E` and `0x0005E4CB` through their `0x0560` FBOV fixups
to overlay 172's `566A:002A` entry. Read the short strings at
`57E0:05BA` and `57E0:05C6` without copying neighboring content.
