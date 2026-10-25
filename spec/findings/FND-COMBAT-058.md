---
id: FND-COMBAT-058
title: The map loop calls 28C9:1EDB only while the pointer mode word DS:1440 is 1, or while the combat word is set in a mode other than 5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:23F6..28C9:24B1
tool: Capstone 5.0.9 over the load image; tools/research/exec-census/immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added. `DS:1440` is the word the right button steps over the map frame (FND-INPUT-013), and
`4C10:0019` is the combat word.

At `28C9:23F6` the routine that holds the call `28C9:2429` reads `DS:1440`:

- when it is 1, it stores 0 at `DS:0DB0`; when the combat word is not 0 and `DS:426D` is below 4
  it calls `28C9:31D2` with 9999; then it calls `28C9:1EDB` (`28C9:2429`), keeps the returned
  byte at `[bp - 5]` and calls `371D:0334`;
- when it is 5, it calls `47A6:0070` with 1 and two frame words instead, with the byte it returns
  deciding between `28C9:2A81(1)` with `1C5F:0139` and `28C9:31D2(9999)` (`28C9:2436..2483`), and
  does not call `28C9:1EDB`;
- for any other value it stores 0 at `DS:0DB0`, calls `28C9:31D2` with 9999 when the word is not
  0 and `DS:426D` is below 4, and jumps to the call of `28C9:1EDB` only when the combat word is not
  0 (`28C9:24A1..24AE`).

## Interpretation

Outside combat, `28C9:1EDB`, and with it overlay 173 `+059A` (FND-RNG-074, FND-RNG-075), runs only
while the pointer is in mode 1. With the combat word at 0 and the pointer stepped to another
mode, the loop skips it.

## Alternatives

What modes 0 and 5 are and when they are set was not read here; the other readers and writers of
`DS:1440` that `immediate_search.py` lists were not read.

## How to reproduce

Disassemble `28C9:23F6..24B1` from the load image; `immediate_search.py <dsun> 1440`.
