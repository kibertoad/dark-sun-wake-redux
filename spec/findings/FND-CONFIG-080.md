---
id: FND-CONFIG-080
title: Two event-record discriminators reach the stored-character list message branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:002F
tool: Python 3.14.7 FBOV table inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 171's window callback `5664:002F` first branches on the first word
of its 24-byte event argument. For value 2, it searches 17 word values at
code offset `0x08E4`, using the event word at argument offset 2, then jumps
through the adjacent 17-word target table. Value `0x477D` (18,301) selects
code offset `0x0780`, whose first branch tests the stored-character state
word at `4C4C:0000` for `0xFFFF`. That branch calls overlay 172's shared
message entry with the no-available-characters text (FND-CONFIG-051).
`WIND/18501` contains `BUTN/18301` (FND-UI-027).

For event first-word value 6, the callback instead searches four words at
code offset `0x0928`, using the event word at argument offset 12. Value
`0x1C0D` also selects code offset `0x0780`. Other values in either table
select different branches or return an unhandled result. The resident
dispatcher can call this callback for both first-word values (FND-CONFIG-079).

## Interpretation

The list failure message has two bounded event-record routes into its
callback: `(first word 2, word at offset 2 = 18301)` and `(first word 6,
word at offset 12 = 0x1C0D)`. Both require the state word to be `0xFFFF`.
The first discriminator matches a button number in the shipped list window;
FND-CONFIG-215 follows the generic pointer path that can return that number.

## Alternatives

FND-CONFIG-215 establishes a conditional pointer-hit route to the first
record, but not the physical device mapping or whether the list state ever
meets the failure guard in ordinary use. FND-CONFIG-216 shows how a keyboard
packet can carry the second discriminator. FND-CONFIG-083 shows that the
generic event-six fallback can reach this same callback through global
registration, subject to that registration and the list state remaining
active. Neither record alone proves a visible message or delay wait.

## How to reproduce

Map overlay 171's code start to file offset `0x00058210`. Disassemble
`0x000585D5..0x00058612`, `0x00058887..0x000588A7` and
`0x00058990..0x000589AC`. Read the word tables at file offsets
`0x00058AF4` and `0x00058B38` with their adjacent target arrays, mapping
target offset `0x0780` to file offset `0x00058990`. Compare the first
table's `0x477D` with the shipped window graph in FND-UI-027.
