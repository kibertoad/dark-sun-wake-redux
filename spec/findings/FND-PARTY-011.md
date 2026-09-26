---
id: FND-PARTY-011
title: CHARTRAN.EXE stores each transferred character as CACT, CHAR, SPST, PSST and PSIN resources under one number from 0 to 39
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:040E..13D8:046C
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:046C..13D8:05C3
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0A89..13D8:0B55
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Addresses are in the unpacked `CHARTRAN.EXE` (FND-PARTY-010). The routines push each tag as a
32-bit immediate and pass it, with a resource number, to far routines of segment `162D` and
`1730`:

- `1730:04AC` takes a tag, a number and a far pointer, and returns 0 when it has read the
  resource into the buffer.
- `162D:05C6` takes a tag and a number, and `162D:00E9` a tag, a number, a size and a far
  pointer to the bytes. Both return the error code `0x1E` unless bit 2 of the open archive's flag
  word is set.

`13D8:040E` reads `CACT` resources 1 to 39 in turn and counts those that it reads and that hold a
nonzero word. It returns 1 when the count is 39 and 0 otherwise.

`13D8:046C` takes a slot index. It reads `CACT` resources 0 to 39 in turn. A number whose `CACT`
cannot be read, or holds 0, is noted as free, each later free number replacing the one before. It
stops at the first number whose `CACT` word equals the word at `1896:0939 + slot * 0x3A`, and
uses that number. With no match it uses the last free number it noted, and with no free number
it prints a message with the far string at `1896:095B + slot * 0x3A` and stores nothing. For the
number chosen it calls `162D:05C6` and then `162D:00E9` with `CACT` and the 2-byte word, calls
`13D8:05C3` with `CHAR` and the number to write the character record, and when that returns 1,
calls `13D8:0A89` with the slot and the number.

`13D8:0A89` takes a slot index and a number. For each of `SPST`, `PSST` and `PSIN` in turn it
calls `162D:05C6` with the tag and number, then `162D:00E9` with the tag, the number and:

| Tag | Size | Source |
|---|---|---|
| `SPST` | 9 | `1896:0105 + slot * 9` |
| `PSST` | 34 | `1896:007D + slot * 34` |
| `PSIN` | 1 | `1896:0079 + slot` |

It returns 0 as soon as a write returns `0xFFFF`, and 1 when all three succeed. The `PSIN` table
ends where the `PSST` table starts, four bytes on, and the `PSST` table ends where the `SPST`
table starts, four records on.

## Interpretation

The transfer utility keeps up to four characters in memory, in slots of `0x3A` bytes at `1896:0939`
with a 16-bit identifier at the start of each and a name 34 bytes in. It stores each character under
one resource number from 0 to 39: the number whose `CACT` already holds the character's identifier,
so a character transferred again replaces its old records, or else a free number, the highest free
one in practice. The `CACT` resource holds the identifier. The character's `CHAR`, `SPST`, `PSST`
and `PSIN` resources take the same number. Bit 2 of the flag word marks an archive opened for
writing, `162D:05C6` removes a resource and `162D:00E9` writes one, so each resource is replaced
whole. `13D8:040E` checks whether all 39 numbers from 1 are in use.

The installed `CHARSAVE.GFF` has `CACT` resources 29 to 39 and 9-byte `SPST` resources for
characters 30, 31 and 33, which fits characters stored from the top number down by this code or
by the game's own code of the same shape (FND-PARTY-006, FND-PARTY-012).

## Alternatives

That bit 2 marks writing, that `05C6` removes and that `00E9` writes are read from the calling
pattern and the shared check; their bodies were not read further. What the 2-byte
identifier is, where the utility takes it from in a Dark Sun 1 save, and what `13D8:05C3` writes
into the `CHAR` record have not been read.

## How to reproduce

Unpack `CHARTRAN.EXE` as in FND-PARTY-010, apply its relocations for a load image at segment
`0x1000`, and search the code for the bytes `66 68` followed by each tag; the pushes lie in the
three routines above. Disassemble them from their entry addresses.
