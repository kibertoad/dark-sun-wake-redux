---
id: FND-CONFIG-096
title: The frame-event caller ignores the pre-dispatch window routine's failure return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0364..3D72:039E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0D9A..3A8E:0F73
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

On its input-bit-4-or-16 path, the pointer routine passes the containing
window at DS:A121 to resident 3A8E:0D9A at file offset
`0x00032C98`. After removing that argument, it immediately tests
DS:A125 for a nonzero selected control; it does not test the callee's
return in AX (FND-CONFIG-094).

The callee starts at `0x0003087A`. It compares its window argument
with DS:A11D. If they differ and the old pointer is nonzero, it calls
local 3A8E:0F7F with the old pointer. A nonzero return causes this
routine to return `0xFFFF` without replacing DS:A11D. Otherwise it
stores the argument at DS:A11D. A null argument returns zero.
For a nonnull accepted argument it clears bit `0x0004` of the window's
word at `+0x9E`, then calls two resident routines with its position
words and region pointer at `+0x0C`.

The subsequent child loop uses the window's count at `+0xF3`,
30-byte entries at `+0x105`, and byte adjustment at `+0xF2`.
Its two-tag table contains MENU and EBOX only, targeting
`0x00030971` and `0x00030958` respectively. Any other child tag
skips those branches and advances the loop. The loop ends with
return zero; the handled branches have additional helper-dependent
paths to the `0xFFFF` return.

The shipped WIND/13501 graph consists entirely of seven APFM children
(FND-CONFIG-093). On that unchanged graph, none selects the callee's
MENU or EBOX branch. This is separate from the APFM callback dispatch
that follows in the pointer routine.

## Interpretation

A failure result from this intervening call is not itself an explicit
guard against the later APFM dispatch: the caller ignores it. The
current shipped window also selects neither of the callee's two child
handling branches. Pointer state and nested helper effects still matter;
this reading does not make the call harmless in every live state.

## Alternatives

FND-CONFIG-098 shows the old-window helper skips an unchanged
APFM-only prior graph. FND-CONFIG-099 bounds the coordinate and
region callees' write ranges. Other prior graphs and runtime changes
may still affect shared state. An ignored failure return therefore does not
prove that DS:A125, the containing window or frame callback remains
valid. Reading those effects and their writers would distinguish an
unchanged-state path from a state-changing one (Q-CONFIG-008).
No visible message or successful message-window setup is established.

## How to reproduce

Map resident 3A8E to file base `0x0002FAE0`. Inspect the caller at
`0x00032C84..0x00032CDE`, the callee's initial window comparison
and failure path at `0x0003087A..0x000308CF`, and its window flag
clear and two argument sequences at `0x000308CF..0x00030909`.
Inspect child-address construction and tag dispatch at
`0x00030909..0x00030958`, decode exactly two tag words and two
target words at `0x00030A53..0x00030A5F`, and inspect the loop exit
and returns at `0x00030A40..0x00030A53`. Compare the seven child
tags from FND-CONFIG-093 with that two-tag table. Keep the callee's
nested helper effects separate from the caller's ignored return.
