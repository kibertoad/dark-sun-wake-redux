---
id: FND-RNG-074
title: With DS:426D at 9999 and combat off, 28C9:1EDB stores nothing while record 9999 holds its text and its type byte is not 2, and once that record is filled with 0xFF it calls overlay 173 +059A with combat word 0 and the address of DS:426D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1E41..28C9:2322
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:197A..2D40:19ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005AF00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AFE4..0x0005B03B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004C7F0..0x0004C7F3
tool: Capstone 5.0.9 over the load image; tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static. Record 9999 of the 37-byte records at `DS:67BB` is at `DS:0CE6`
(FND-RNG-068), and its type entry `4F49:0C33 + 3 × 9999` is at `4F49:8160`, which holds `2E 00 CD`
in the file (offset `0x4C7F0`).

`28C9:1EDB` with `DS:426D` at 9999 and the combat word `4C10:0019` at 0 passes the first test
(`1EEB..1F00`) and then:

- leaves for `2127` when record 9999's word `+0x01` (`DS:0CE7`) is -1 (`1F0A..1F11`);
- otherwise stores into record 9999 (`1F58`, `1F96`, `1FD3`) only when its type byte is 2, its
  byte `+0` has bit `0x20` set and bit 1 clear (`1F14..1F51`). With the type byte `0x2E` it goes
  to `1FD7`, where a clear bit `0x20` leads to `1FE8` and the type byte, not 2, to `230B`, which
  returns without a store.
- With a type byte of 2 it would read the 0x31-byte table `DS:19C9` into `[bp-3]`, reach `21D1`
  with combat off and call `28C9:1E41`, then leave at `2224` because `DS:426D` is above 4 and
  combat is off.

From `2127` with combat off it reaches `2146`, calls overlay 173 `+059A` (trampoline
`5671:0039`) with the combat word and the near address `0x426D`, and stores the result in
`4C10:0019` (`2160`). When that is 0 it stores `DS:0DA5` or `4C13:0369` in `DS:426D` and `0xFFFF`
in `DS:0DA5` when `DS:0DA5` is not -1 (`2171..21C1`), and calls `2C5F:097E` and `4699:0043`
(`21C7..21CC`). It then calls `28C9:1E41` and leaves at `21F7` or `2224` as above.

`28C9:1E41(p)` calls `2D40:197A(9999)`, stores 8 at `p` and walks the slot list from `DS:5AF5`
through byte `+0x12` of the 19-byte slot records at `4F49:08A3`, calling `28C9:19A1` with each slot
and storing a record's byte `4F49:0374 + 0x1C × DS:426D` at `p` only for the slot equal to
`DS:426D`. `2D40:197A` takes an argument of 9999 or more as all slots from the head
`4F49:0000` below `0x30`, and uses it as no index.

Overlay 173 `+059A` with a first argument of 0 skips the jump table at `+0855` (`+05C9..+05D2`),
and with `SI` at 9999 goes to `+06E0`, the search for a next combatant; a slot found goes through
`2D40:2F34` and is stored through the pointer (`+07D5..+07FA`), and none found leads to the end of
combat at `+071B` (FND-RNG-070).

## Interpretation

While record 9999 holds the help text, `28C9:1EDB` reads it and the type entry and returns with no
store. After FND-RNG-068's step 4 fills record 9999 with `0xFF`, every pass with combat off calls
overlay 173 `+059A(0, &DS:426D)`, which can start combat with a found slot, or end it again, and
store the result in `DS:426D` and the combat word.

## Alternatives

What overlay 173 `+059A` writes on its search and end paths, and the callees `2C5F:097E`,
`4699:0043` and `28C9:19A1`, were not read here. The type byte at `4F49:8160` is read from the
file; FND-RNG-075 reads it in the running game.

## How to reproduce

Disassemble `28C9:1E41` to `28C9:2322` and `2D40:197A` to `2D40:19ED` from the load image;
`overlay_listing.py <dsun> 173 5AEBA 5AF00` and `... 173 5AFE4 5B03B`; read 3 bytes at file offset
`0x4C7F0`.
