---
id: FND-CONFIG-087
title: The class-choice message branch skips callback registration and the registered handler can restore its predecessor
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57B0:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57B0:129A..57B0:17DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 209's loop records whether its local helper finds any of the tested
values 1 through 17 (FND-CONFIG-050). When none was found, it calls overlay
172's `566A:002A` message entry at file offset `0x000943FA` with the
no-other-classes text at `DS:2B1B`, then jumps past the callback registration.
When the loop found one, the path at `0x00094405` gets the current global
callback through `571F:0039`, saves it at `0300:000F`, and registers
`57B0:0048` through `571F:0034`.

The `57B0:0048` trampoline targets overlay 209 code at file offset
`0x000947FC`. Its event dispatch has these bounded exits:

| Event record condition | Local path |
|---|---|
| First word 1 | Calls the cleanup routine at `0x000948DB` and returns `0xFFFF`. |
| First word 6 and event word at offset 12 (`[BP+0x12]`) equals `0x011B` | Calls the same cleanup routine, then continues through the callback's default handler. |
| First word 2 and control number `0x4396` | Jumps to that cleanup routine. |
| First word 2 and control number `0x4397`, `0x4398` or `0x439D` | Enters the branch with a direct call to overlay 172's separate `566A:0025` entry at `0x00094878`; its later return and loop can reach cleanup. |
| First word 2 and control number `0x4399..0x439C` or `0x439E` | Uses a local selection branch that can reach cleanup without that direct `0025` call. |

The cleanup routine at `0x000948DB` passes the saved far pointer from
`0300:000F` to `571F:0034` at `0x000948EE` before other cleanup calls.
The event-two table does not accept a control number outside
`0x4396..0x439E`; that path reaches the default handler.

## Interpretation

The no-other-classes call to the shared `002A` message entry precedes and
excludes this registration path. When the registered handler later receives
one of the listed terminating event records, it can restore the pointer
saved before registration. This narrows one incoming message path and one
callback lifetime, without establishing their live ordering relative to
the stored-character list or the message window's acquisition gate.

## Alternatives

The earlier loop's inputs and the registered window's complete event
production were not read. The `0025` entry is distinct from the `002A`
message-delay entry; this finding does not claim its dialog uses that delay.
Other exits and cleanup callers may change the global pointer, and a
computed event route may add callback inputs.

## How to reproduce

Resolve overlay 209 stub `57B0:0048` at its header offset `0x0048` to code
offset `0x169C`, file offset `0x000947FC`. Disassemble bounded windows at
`0x000943DD..0x0009442B`, `0x000947FC..0x000948C8` and
`0x000948DB..0x0009493C`. Read the nine-entry event-two jump table at code
offset `0x1769`, mapping targets for control numbers `0x4396..0x439E`.
Resolve the far calls at `0x000943FA`, `0x00094878` and `0x000948EE`
through their declared FBOV fixups.
