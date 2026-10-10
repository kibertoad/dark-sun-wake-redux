---
id: FND-PARTY-114
title: CHARTRAN.EXE stores 0 in bytes +0x0E and +0x0F of each of its four combatant records before it writes a transferred character's CHAR resource, and no other code in it indexes those records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0159..13D8:0202
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0BA1..13D8:164B
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:164B..13D8:17D4
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0734..13D8:0797
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:05C3..13D8:0734
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:07CA..13D8:0A3E
tool: Capstone 5.0.9 16-bit disassembly with Python 3.14.7 of the file unpacked by tools/DarkSunWakeRedux.Inspect unlzexe, swept from the load image to the data segment and restarted one byte on after an undecodable byte
environment: null
---

## Observation

Addresses are in the unpacked `CHARTRAN.EXE` (FND-PARTY-010), 70,288 bytes with XXH3-128
`a2804715759141397dca547934213843`; far call segments are written relocated to a load image at
segment `0x1000`.

**The records.** `13D8:164B` stores, among other words, 0x17, 0x31 and 0x42 in the words at
`DS:14C2`, `DS:14C4` and `DS:14C6`, and the far pointer `1896:0129` in the dword at `DS:1490`
(`13D8:164B..13D8:17D4`). `13D8:0734` takes an object number `o`, reads its type `t` from the byte
at `1896:635C + 3 * o` and its index from the word at `1896:635D + 3 * o`, and returns the far
pointer at `DS:1488 + 4 * t` plus the index times the word at `DS:14C0 + 2 * t`
(`13D8:0734..13D8:0797`). For type 2 that is `1896:0129 + 0x31 * index`.

**The copy.** `13D8:0BA1`, for `di` from 0 to 3, copies words and bytes of the transfer slot at
`1896:0939 + 0x3A * di` into the record at `1896:0129 + 0x31 * di`, and stores constants in
others. It stores the byte 0 at `1896:0137 + 0x31 * di` and at `1896:0138 + 0x31 * di`, which
are bytes `+0x0E` and `+0x0F` of the record (`13D8:0CA1` and `13D8:0CB1`). Its block copies
through `1000:339B` have the destinations `1896:0142 + 0x31 * di` (6 bytes, record bytes `+0x19`
to `+0x1E`) and addresses in the 0x42-byte records at `1896:12B7`, and the one through
`1000:33BF` the destination `1896:014A + 0x31 * di` (record byte `+0x21` on)
(`13D8:0BA1..13D8:164B`).

**The order.** `13D8:0159` opens the archive, and when `13D8:021F` returns nonzero calls
`13D8:0BA1` and then, for each slot from 0 to 3, `13D8:046C` (`13D8:0159..13D8:0202`), which
writes the `CHAR` resource through `13D8:05C3` (FND-PARTY-011). `13D8:05C3` lists the object's
parts through `13D8:07CA`, which stores each part's type and index, and writes each part's
10-byte header and the bytes at the pointer `13D8:0734` gives, the size taken from the word at
`DS:14C0 + 2 * type` (`13D8:05C3..13D8:0734`, `13D8:07CA..13D8:0A3E`).

**The searches.** In the swept listing, the 23 instructions that multiply by 0x31 all lie in
`13D8:0BA1..13D8:164B`; the only stores to `es:[bx + 0x137]` or `es:[bx + 0x138]` are the two
above; and the stores through ES at displacement `0x0E` or `0x0F` are three, at `1000:6B98`,
`1000:7698` and `1000:76C8`, each to word `+0x0E` of a node reached through the far pointer at
`+8` of another node, beside a store to its word `+0x0C`.

## Interpretation

The combatant record of a character that `CHARTRAN.EXE` transfers holds 0 in bytes `+0x0E` and
`+0x0F` when the utility writes its `CHAR` resource, since the copy stores 0 there for all four
slots before any slot is written and nothing else in the utility indexes the records. A party
member loaded from a transferred character's `CHAR` therefore has type 0, as the shipped ones do
(FND-PARTY-113).

## Alternatives

- That each transferred character's type 2 part has an index from 0 to 3 was not read from the
  code that fills `1896:635C`; an index of 4 or more would read memory after the four records.
- The three stores at displacement `0x0E` are taken to be list links from their shape (a pair of
  words forming a far pointer, reached through the link at `+8`); a linear sweep can also
  misdecode bytes, so a store hidden by a wrong alignment is not ruled out.
- A store through a pointer formed other than by multiplying by 0x31 or through `13D8:0734`
  would be outside the searches.

## How to reproduce

From the commit that adds this finding, run `tools/DarkSunWakeRedux.Inspect unlzexe
<install>/CHARTRAN.EXE <out>` and check the size and hash above. Disassemble `<out>` with
Capstone in 16-bit mode from the load image (file offset `0x990`) to file offset `0x92F0`,
restarting one byte on after an undecodable byte. In that listing, read the six ranges above,
search for `imul` with 0x31, for the displacements `0x137` and `0x138`, and for stores through
ES at displacement `0x0E` and `0x0F`, and read the destinations pushed before each call of
`1000:339B` and `1000:33BF` in `13D8:0BA1..13D8:164B`.
