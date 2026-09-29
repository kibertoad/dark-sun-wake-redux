---
id: FND-CONFIG-102
title: Overlay 172 forwards a queued frame code with the selector byte gate zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0089
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and MZ mapping
environment: null
---

## Observation

The selector call at file offset `0x0005A217` belongs to overlay
172's frame callback 566A:0043, starting at `0x0005A051` and
returning at `0x0005A3EA` (FND-CONFIG-074, FND-CONFIG-101).
The callback's event-five path dispatches its third word argument
through an explicit five-entry table:

| Argument value | Branch file offset |
|---:|---|
| 2 | `0x0005A27D` |
| 4 | `0x0005A2B5` |
| 64 | `0x0005A0A2` |
| 128 | `0x0005A333` |
| 256 | `0x0005A32B` |

The value-64 branch subtracts 11213 from the frame identifier. Its
local data flow loads a word at DS:445E plus DS:445C times 34 plus
twice that identifier difference. A zero word exits; a nonzero word
is decremented to obtain the code kept in SI. Codes 235 through 268
set the local category byte to two. For those codes, a call to overlay
176's 5691:002A with DS:445C and code minus 235 must return nonzero
to pass the earlier psionic-start message branch (FND-CONFIG-073).

Later local guards include an interruption path when both raw word
02E0:0019 and raw byte 0308:[DS:445C+0xB2] are nonzero. A further
record lookup requires byte `+0x0C` to be zero or seven before the
selector argument sequence. Those guards have earlier shared-message
or exit paths; their complete state is not established here.

At `0x0005A1F1` and `0x0005A1F3`, the caller pushes zero for the
two final byte arguments of selector 573B:0089. The byte gate at
that selector's `[BP+0x1A]` is therefore zero. Its third word
argument is the locally retained code, while its first and second
word arguments both come from DS:445C. The local initial word must
be 236 through 269 to produce selector codes 235 through 268 by
that decrement, before considering intervening callees' effects.
FND-CONFIG-100 describes the selector's next range gate and the
conditional overlay 176 message paths.

## Interpretation

This frame callback provides a concrete zero-byte-gate route into the
selector. Its local code source is a nonzero selected-row/frame word
minus one, and only the resulting 235..268 range selects overlay 176.
The queued word's producers, earlier helper results and live window
state are still required to establish a reachable feedback message.

## Alternatives

The frame registration and shipped graph are bounded by FND-CONFIG-074
and FND-CONFIG-075. FND-CONFIG-104 traces a producer of value 64.
The selected-row and queue-word writers, record-lookup effects and
intervening callees remain unread (Q-CONFIG-008). Codes outside the
selector range or a failed earlier guard do not establish entry into
these overlay 176 message sites. No action outcome or visible wait
is established by this local reading.

## How to reproduce

Resolve overlay 172's stub 0043 to `0x0005A051`. Inspect the event
and queue-word path at `0x0005A072..0x0005A0E7`, eligibility branch
at `0x0005A0E7..0x0005A113`, interruption branch at
`0x0005A13A..0x0005A166`, and record guard at
`0x0005A1B6..0x0005A1DA`. Inspect the selector pushes and call at
`0x0005A1F1..0x0005A21F`. Decode exactly five value words and five
target words at `0x0005A3EB..0x0005A3FF`, using overlay code base
`0x00059430`, and verify the return at `0x0005A3EA`. Compare the
selector's byte and range tests in FND-CONFIG-100.
