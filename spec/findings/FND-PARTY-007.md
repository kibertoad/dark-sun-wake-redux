---
id: FND-PARTY-007
title: DSUN.EXE holds no run of four consecutive character numbers other than inside its hex-digit strings
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:A366..5000:A388
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole of `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`),
the MZ header, the resident load image `1000:0000..6237:0000` and the FBOV overlay pack alike,
for the character numbers 40, 41, 42, 43; 50, 51, 52, 53; and 29, 30, 31, 32 in order, each as
four consecutive bytes, four consecutive 16-bit little-endian words and four consecutive 32-bit
little-endian words, finds only two matches, both of the bytes `32 33 34 35` (50 to 53): at file
offset `0x4F568`, `5000:A368`, and `0x4F579`, `5000:A379`. These lie inside the two strings of
hexadecimal digits at `5000:A366` and `5000:A377`, the upper-case and the lower-case set, where
`2345` is part of the digit run.

An earlier search in Ghidra of the load image alone, for four-byte and eight-byte little-endian
runs of 40 to 43 and 50 to 53, gave the same two matches.

## Interpretation

The executable does not hold either disc character set, or the first four installed characters,
as a plain table of consecutive numbers. The two matches are digits of text.

## Alternatives

A party list could still be held another way: numbers that are not consecutive or not in order,
a single first number and a count, numbers computed at run time, or a list in a data file. The
game computes them: overlay 182 loads `CHAR` 40 plus the slot for slots 0 to 3
(FND-PARTY-013). Ghidra also found no function that uses all four of 29 to 32, or all four of 40
to 43, as scalar operands (FND-PARTY-008).

## How to reproduce

Search the file for each of the nine patterns (three runs in three widths) and compare the
matches with the strings around them. The hexadecimal digit strings are the only printable
matches. Ghidra's `ReportBytePattern` script gives the same result on the load image.
