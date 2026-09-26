---
id: FND-UI-006
title: The frame and child dispatchers test the word at 0x58 of APFM and BUTN records and at 0x96 of EBOX records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515..3D72:059E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0FD4..3D72:1019
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:10B0..3D72:10CA
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

`DSUN.EXE` is 634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

The far routine at `3D72:0515` compares its first argument with the tag `APFM` (`0x4D465041`) at
`3D72:0529` and returns unless they are equal. It then requires the far pointer it was passed,
and the 32-bit value at `0x62` of the record it points at, to be nonzero. At `3D72:054D` it tests
the word at `0x58` of the record against a word argument (`test`, then `jle`), and returns when
the result is zero or has bit 15 set. Otherwise it builds a local record holding the value 5, the
argument word and the word at `0x8` of the record, and makes a far call through the pointer at
`0x62` of the record at `3D72:0590`.

In the child dispatcher at `3D72:0EB8`, for a child whose tag is `BUTN` (`0x4E545542`), the
branch at `3D72:0FEA` tests the word at `0x58` of the child's record with 4 and the branch at
`3D72:100E` tests it with 2; either set bit sends the child to the end of the loop. For a child
whose tag is `EBOX` (`0x584F4245`), `3D72:10C1` tests the word at `0x96` of its record with 2 and
skips the child when the bit is clear.

## Interpretation

The word at `0x58` of an `APFM` or `BUTN` record, and at `0x96` of an `EBOX` record, is a mask of
event bits (RULE-UI-001). A frame is handed an event when its mask shares a bit with the event's
bits. In the search for the control under the pointer, a button with the value 4 or 2 in its mask
is passed over, and an edit box is considered only when its mask has the value 2. A frame's
handler is a far pointer the game stores at `0x62` of the loaded record, in bytes that are 0 in
the file.

## Alternatives

The file values alone would allow `0x58` of an `APFM` record to be an appearance setting or a
resource number; this code rules that out. What event each bit stands for, and who calls
`3D72:0515` with which bits, are not known. Earlier notes read the button and edit box tests as
the same shared-bit match as the frame's; for buttons the test works the other way.

## How to reproduce

In Ghidra, search for the scalar `0x4D465041` and follow it to `3D72:0529`; read `3D72:0515` to
its return at `3D72:059D`. Read `3D72:0FD4` to `3D72:1019` and `3D72:10B0` to `3D72:10CA` in the
routine at `3D72:0EB8`. A 16-bit disassembly of the same ranges, with the MZ relocations applied
for a load image at segment `0x1000`, shows the same instructions.
