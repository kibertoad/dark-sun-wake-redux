---
id: FND-SCRIPT-008
title: 172C:0388 loads a GPL or MAS resource into a 16-slot script cache and appends a 0x31 byte
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-SCRIPT-019]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0388..172C:07BB
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Superseded by FND-SCRIPT-019. The early wrapper paths bypass aging, and the size-query output is a double word. The original description below is retained as history.

The cache has 16 slots, each described by one element of five arrays in segment `4C13`: the
script number at `01D9`, the selector at `01B9`, the start offset in the script buffer at `0219`
(`0xFFFF` for a free slot), the end offset at `01F9`, and a signed age byte at `0239`.

`172C:0388(number, selector)` returns 0 at once when the stop byte `4C13:0326` is 1, and 1 when
the number and selector equal the words at `4C0E:0012` and `4C0E:0014`. Otherwise it calls
`172C:31ED` and then `172C:043F(number, selector)`, and when that returns 0, `172C:04CF(number,
selector)`. When either returns 1, it stores the number and selector at `4C0E:0012` and
`4C0E:0014`. In every case it then adds 1 to the age of each slot whose age is from 0 to 126, and
returns the result.

`172C:043F` looks through all 16 slots for one with that number and that selector. For a match
it stores the number at `4C13:0197`, the selector at `4C13:0195`, the slot's start offset at
`4C13:0193`, and 0 in the slot's age, and returns 1.

`172C:04CF`:

1. returns 0 when the number is `0xFFFF` or the selector is neither 1 nor 2;
2. takes the tag `GPL ` (`0x204C5047`) for selector 1 and `MAS ` (`0x2053414D`) for selector 2;
3. takes the first slot whose start offset is `0xFFFF`, or, when there is none, the slot
   `172C:07BB` returns;
4. when that slot's number already equals the number, skips to step 9;
5. asks the resource entry `38FF:05B5` for the size of the resource with that tag and number,
   and far-calls `5702:00B1` when it returns nonzero;
6. calls `172C:0698` with the size plus 1, which returns an offset in the script buffer or
   far-calls `5702:00B1` when the size is not below the buffer size at `4C13:0313`; returns 0
   when the stop byte is then 1;
7. stores the offset as the slot's start and the offset plus the size plus 1 as its end, and adds
   1 to the age of each slot whose age is from 0 to 126;
8. reads the resource into the script buffer at that offset through `38FF:04AB`, far-calls
   `5702:00B1` when that returns nonzero, and otherwise stores `0x31` in the byte after the data
   and the number, the selector and age 0 in the slot;
9. stores the number at `4C13:0197`, the selector at `4C13:0195` and the slot's start at
   `4C13:0193`, and returns 1.

`38FF:04AB` has the offset and arguments of the read entry of FND-PARTY-012: a 32-bit tag, a
32-bit number and a far pointer to the destination. `38FF:05B5` takes the same arguments with a
far pointer to a word that receives the size.

## Interpretation

A script is loaded once into a shared buffer and kept while its slot is not reused; running code
reads from the slot's start (FND-SCRIPT-006). Selector 1 loads a `GPL ` resource and selector 2 a
`MAS ` resource. The byte after the data is the stop instruction `0x31` (FND-SCRIPT-009), so a
script that runs off its end stops. The age counts loads since the slot was last used, up to 127,
and presumably guides `07BB` in picking a slot to reuse.

## Alternatives

That `38FF:05B5` gives the resource's size rests on its use here: the result sizes the
allocation and the read. `172C:07BB`, `172C:0698` and `172C:31ED` were not read, so how a slot is chosen for reuse and how
buffer space is found are open. Step 4 compares the number alone; a slot holding `GPL ` resource n
taken for `MAS ` resource n would be used without loading, but whether a free or reused slot can
hold a matching number was not traced. Which open archive the resource entries search is not
shown here; the game opens `GPLDATA.GFF` at start (FND-SCRIPT-003).

## How to reproduce

Disassemble `172C:0388` to `172C:07BB`.
