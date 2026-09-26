---
id: FND-COMBAT-001
title: No function of the load image holds all six scan codes of the manual's combat keys, and the six do not occur as one byte run
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

The six keyboard scan codes of the manual's combat keys (SRC-MANUAL-1994, page 77), `0x22` (`G`),
`0x31` (`N`), `0x19` (`P`), `0x10` (`Q`), `0x11` (`W`) and `0x39` (Space), do not occur as the
byte run `22 31 19 10 11 39` anywhere in `DSUN.EXE`. No decoded function of the load image has all
six among its instruction operands.

## Interpretation

The combat keys are not dispatched from a table that lists their scan codes in the manual's order,
nor compared by bare scan code in one function.

## Alternatives

The game compares keys as BIOS key words, scan code in the high byte and character in the low
byte, in a table of 33 words in overlay 190 (FND-COMBAT-025); a function-level search for bare
scan codes cannot see those. FND-INPUT-008 records the same negative for the character codes.

## How to reproduce

Search the whole file for the six bytes. In a Ghidra project of `DSUN.EXE` with the MZ loader at
segment `0x1000`, run `ReportFunctionScalarIntersection` with the six values.
