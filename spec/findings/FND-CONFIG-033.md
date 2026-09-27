---
id: FND-CONFIG-033
title: Initialized display bounds admit the message window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident routines
environment: null
---

## Observation

The resident graphics initializer at `39D1:0009` calls a mode query and
dispatches its result through a four-entry table. It writes these pairs to
the bounds words at `DS:A039` and `DS:A03B`:

| Mode result | Maximum x | Maximum y |
|---|---:|---:|
| `0x201`, `0x202` or `0x203` | 639 | 399 |
| `0x10` | 649 | 349 |
| Any other result | 319 | 199 |

The window registration routine at `3A8E:02B3` fails its bounds check if
`x + width` exceeds `maximum x + 1`, or if `y + height` exceeds
`maximum y + 1` (FND-CONFIG-030). The message caller supplies (66, 90),
and installed `WIND/10501` is 192 by 28 (FND-CONFIG-030). Its far edges
are therefore 258 and 118, inside even the smallest initialized limits
of 320 and 200.

## Interpretation

If the bounds words retain one of the graphics initializer's values, the
message window's position/bounds check succeeds. Other registration
failures, including a missing resource pointer, are separate
(FND-CONFIG-030, FND-CONFIG-032).

## Alternatives

This reading does not prove that the bounds words are unchanged at every
message call. The initializer's callers, possible indirect writes, and
resource acquisition outcomes are not completely read. It does not show
which text-message calls reach the wait.

## How to reproduce

Disassemble shipped `DSUN.EXE` file offsets `0x0002EF19..0x0002EFB2`
(`39D1:0009` onward) and `0x0002FD93..0x0002FDE9`
(`3A8E:02B3` onward) in 16-bit mode. Resolve the graphics dispatch
table at `39D1:0316`, list the three assignments to `DS:A039` and
`DS:A03B`, and apply the registration comparison to the position and
dimensions documented in FND-CONFIG-030.
