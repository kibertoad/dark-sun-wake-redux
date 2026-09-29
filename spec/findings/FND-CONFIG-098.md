---
id: FND-CONFIG-098
title: The old-window helper skips the shipped item window APFM children
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0F7F..3A8E:103C
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

Before replacing its remembered window, resident 3A8E:0D9A can call
local 3A8E:0F7F with the old window pointer (FND-CONFIG-096).
The helper starts at file offset `0x00030A5F`. A null argument goes
to its zero-result exit. For a nonnull window it loops over children
using byte adjustment `+0xF2`, count `+0xF3`, 30-byte child width
and child-list start `+0x105`.

Its explicit two-entry dispatch table contains:

| Child tag | Branch file offset |
|---|---|
| MENU | `0x00030AEC` |
| EBOX | `0x00030AD4` |

Any other tag skips the branches and advances the child index.
The EBOX branch passes its resolved child pointer to 409B:12A4;
a nonzero result selects return `0xFFFF`. The MENU branch has
additional resident calls, including one with DS:A11D; its nonzero
result also selects `0xFFFF`. Exhausting the loop returns zero.
The effects of those handled-tag callees are not established here.

The shipped WIND/13501 has seven APFM children and no MENU or EBOX
child (FND-CONFIG-093). If that unchanged graph is the old window,
every child skips both branches and the helper returns zero on the
ordinary path. This premise concerns the old window: opening the item
window from a different window does not establish it.

## Interpretation

The old-window helper's child handling does not by itself obstruct a
transition away from the unchanged shipped item-window graph. A
transition from a window with handled tags can have additional effects
or a nonzero result. The pre-dispatch caller's ignored return remains
a separate fact (FND-CONFIG-096).

## Alternatives

The actual prior window and runtime additions or changed tags remain
open (Q-CONFIG-008). The two handled-tag branches' helper effects
require further readings if that prior graph contains them. This
finding does not prove that the item window is current or that its
frame callback remains registered in every live state.

## How to reproduce

Map resident 3A8E to file base `0x0002FAE0`. Inspect the null check
and child addressing at `0x00030A5F..0x00030AB3`, two-tag dispatch
at `0x00030AB3..0x00030AD4`, and the two bounded branches and loop
exit through return `0x00030B1B`. Decode exactly two tag words and
two target words at `0x00030B1C..0x00030B28`. Compare the seven
child tags from FND-CONFIG-093. Read the caller in FND-CONFIG-096
to distinguish its old window argument from the new requested window.
