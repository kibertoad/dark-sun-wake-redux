---
id: FND-CONFIG-013
title: The second saved music byte is the Preferences bar's scaling denominator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV ranges; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The Preferences dispatcher in overlay 203 calls overlay 190's trampoline at
`571F:00CF` to draw either level bar. For the music bar, at
`DSUN.EXE+0x0008B4A4..0x0008B4BD`, it passes zero when music is disabled or
the byte at `DS:26B4` when enabled, then the byte at `DS:26B6`, and then the
word 26. For the sound-effects bar, at
`DSUN.EXE+0x0008B5E7..0x0008B605`, it passes zero when
effects are disabled or the byte at `DS:26B5` when enabled, followed by 127
and the word 45. The remaining arguments are a common word 75 and a far
pointer to the Preferences window.

That trampoline targets overlay 190 code at `DSUN.EXE+0x0007AC83`. The
routine takes the music or effects value as its last word argument and the
preceding word as its limit. It first caps the value to the limit. It
multiplies the capped value by 164 in a 16-bit word; when the limit is not
zero, it divides the result by the limit, then divides by two. The resulting
word is added to a horizontal coordinate based on the window's position
before a drawing call. When the limit is zero it skips the first division.

The music limit `DS:26B6` is copied from the unmodified request `DS:26B4`
when the sound library's returned level is smaller, both after loading and
after a music-enable change (FND-SAVE-005, FND-CONFIG-012). The installed
`PREF/100` holds 255 in both bytes (FND-CONFIG-001).

## Interpretation

`PREF/100` offset `0x04` is the denominator used to scale the Preferences
music-level bar. For a nonzero denominator and ordinary byte inputs, the
calculated horizontal offset is `floor(82 * min(level, denominator) /
denominator)`. The game uses a fixed denominator of 127 for the effects bar.
The byte at offset `0x04` also retains the request when the sound library
caps the level, so the bar can represent the returned level against that
larger request.

## Alternatives

The later drawing calls and native widget appearance were not read or
observed; the calculated offset alone does not establish the exact painted
pixels or colours. It remains open whether another control adjusts the
music-level request, and how the library translates its stored level into
audible output. A zero denominator takes a distinct path whose screen
appearance is not established.

## How to reproduce

Use `tools/ghidra/ReportFbovOverlayMap.ps1` to locate overlays 190 and 203.
Read the target word in trampoline `571F:00CF`, which points to offset
`0x2943` of overlay 190, file offset `0x0007AC83`. Disassemble the bounded
windows `0x0007AC83..0x0007AD65`, `0x0008B4A4..0x0008B4BD` and
`0x0008B5E7..0x0008B605` in 16-bit mode. Follow the value cap and two
divisions, and compare the two callers' argument order.
