---
id: FND-SCRIPT-004
title: The resident image of DSUN.EXE holds no GPLI tag, and no function there holds the number 135 with the GPL tag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

After Ghidra's full analysis of the loaded image of `DSUN.EXE`:

- `ReportBytePattern` finds no occurrence of `47 50 4C 49`, the tag `GPLI`, in any block;
- `ReportFunctionScalarIntersection` finds no function whose decoded operands hold both the
  scalar 135 and the scalar `0x204C5047`, the tag `GPL ` read as a little-endian 32-bit value;
- the same query finds no function whose operands hold all three of 135, `0x4C50` and `0x2047`,
  the two 16-bit halves of that tag.

## Interpretation

The resident code does not load `GPLI` resources by a literal tag, and no resident function asks
for `GPL ` resource 135 with both numbers written into it. The `GPLI` tag occurs only in overlay
187 (FND-EXE-006), and script numbers reach the loader as arguments (FND-SCRIPT-007,
FND-SCRIPT-019), so neither result bears on which scripts run.

## Alternatives

The searches do not cover the `FBOV` pack after the load image. A tag or number built at run
time, read from data or passed as an argument would not show in them.

## How to reproduce

Run `ReportBytePattern` for `47 50 4C 49` and `ReportFunctionScalarIntersection` for the scalars
above over a fully analysed project of `DSUN.EXE`.
