---
id: FND-CONFIG-030
title: Message window setup has two remaining failure gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV and resident calls; DarkSunWakeRedux.Inspect ui-catalog
environment: null
---

## Observation

The overlay 172 message routine passes `WIND/10501`, position words 66
and 90, and a far callback pointer to overlay 182's `56BD:0048` entry
at `DSUN.EXE+0x00059AC6` (FND-CONFIG-018). The read-only UI catalog
reports that `RESOURCE.GFF#WIND/10501` is 192 by 28 and contains a
button and an application frame. At the logical 320 by 200 resolution,
that position and size lie within the screen.

Overlay 182's entry at `0x000689BB` acquires the window, then calls
three setup paths in order, stopping on a nonzero result (FND-CONFIG-018):

| Call | Bounded effect and result |
|---|---|
| Local `0x00068946`, then resident `3A8E:02B3` | Tests for a zero window pointer, compares the supplied position plus window dimensions with global bounds, then registers the window and its children. The zero-pointer and bounds paths return failure; later registration effects were not fully read. |
| Resident `3A8E:0CDC` | When the pointer is nonzero, copies the supplied far callback into the loaded window at offset `0xF5`; returns zero even when the pointer is zero. |
| Resident `3A8E:060D` | Tests for a zero pointer, updates the registered-window list and enters further activation work. It has nonzero return paths that were not fully read. |

The overlay 172 caller stores the returned pointer, makes further calls,
then tests it before sending its text and waiting with `DS:26B7`
(FND-CONFIG-011, FND-CONFIG-018).

## Interpretation

The callback-storage step cannot be the failing one of the three setup
calls. Resource acquisition, position registration and activation can
still fail. The shipped dimensions and supplied position rule out a
simple off-screen placement in a 320 by 200 state, but the runtime
bounds globals and activation state have not been established for every
message call. FND-CONFIG-033 identifies the graphics initializer's three
bounds pairs, all of which admit this window.

## Alternatives

The first setup routine has additional registration branches beyond the
initial bounds checks, and the third activation routine has stateful
effects (followed further in FND-CONFIG-031). This reading does not prove
either succeeds in a live state, that every message call uses this route,
or that the later wait always runs. The UI catalog reports the resource's
fields; it does not prove native paint or control state.

## How to reproduce

Disassemble overlay 172 at `0x00059AB7..0x00059B75` and overlay 182 at
`0x000689BB..0x00068A3F`, following the three calls through local
`0x00068946` to resident `3A8E:02B3`, `3A8E:0CDC` and `3A8E:060D`.
Run the read-only `DarkSunWakeRedux.Inspect ui-catalog` on the approved
installed `RESOURCE.GFF` and select `WIND/10501`; compare its dimensions
with the caller's position arguments. Keep the full resident registration
and activation paths as separate open readings.
