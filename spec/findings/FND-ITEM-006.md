---
id: FND-ITEM-006
title: CHARTRAN.EXE loads ITEMS.BIN whole and looks up each Dark Sun 1 item's number in its first column
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0295..13D8:02DE
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:0300..13D8:038D
  - build: BLD-GOG-EN-1.1
    file: CHARTRAN.EXE
    address: 13D8:1314..13D8:164B
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Addresses are in the unpacked `CHARTRAN.EXE` (FND-PARTY-010). The item routines use the data
segment at paragraph `1F2F`, which is the same memory as `1896`: `1F2F:x` is `1896:x+0x6990`.

The loader at `13D8:0300`, called from `13D8:011E` during start-up:

1. gets the size of the file named at `1F2F:04BF` (`items.bin`, FND-ITEM-005) through the routine
   at `13D8:02DE`;
2. allocates the size plus one byte and keeps the pointer in the word at `1F2F:0E26`; when the
   allocation fails it prints the message at `1F2F:04C9` with the size;
3. opens the file with mode `rb`; when that fails it prints the message at `1F2F:04FB`;
4. after either failure, calls the routine at `1000:03F7` with 0;
5. otherwise reads the whole file into the buffer, one byte per item, stores the count of bytes
   read in the word at `1F2F:0E24`, and closes the file;
6. halves the word at `1F2F:0E24`, rounding towards zero. For the shipped file this is 468.

The lookup at `13D8:0295` takes one signed 16-bit argument:

1. When the argument is 0 it returns 0.
2. Otherwise it takes the argument's absolute value, and for a counter from 0 while the counter is
   below the word at `1F2F:0E24`, reads two words from the buffer, advancing four bytes each
   time. When the first word equals the value it returns the second word.
3. When no pair matches it returns 900 (`0x384`).

The counter's limit is the number of 16-bit words in the file (468), while each step reads a pair
of words, so a value that matches no pair makes the lookup read 1,872 bytes from the start of a
936-byte buffer.

The caller at `13D8:1314` runs a counter k from 0 to 399. For each k it:

1. calls the lookup with the word at `1896:3FC0 + k * 21`;
2. when the byte at `1F2F:0286` is not zero, prints `OLD: %d, NEW: %d` (FND-ITEM-005) with that
   word and the result;
3. when the result is 0, adds one to the count of items not translated and moves to the next k;
4. otherwise adds one to the count of items translated, stores the result in the word at
   `1896:1BD0 + k * 23`, and reads the `RDFF` resource of that number through `1730:04AC`
   (FND-PARTY-011);
5. when that read fails, reads `RDFF` 900 instead, stores 900 in the word at
   `1896:1BD0 + k * 23`, and, when that read also fails, prints the money-bag message at
   `1F2F:0674` (`1896:7004`);
6. copies further words of the 21-byte record at `1896:3FC0 + k * 21` into the 23-byte record at
   `1896:1BD0 + k * 23`.

After the loop, when the word at `1F2F:0282` is not zero, it prints the two totals, and then the
message that items have been translated.

## Interpretation

The records at `1896:3FC0` are up to 400 Dark Sun 1 items, 21 bytes each, and the records at
`1896:1BD0` are the Dark Sun 2 items the utility writes, 23 bytes each. `ITEMS.BIN` maps the
number of a Dark Sun 1 item to the number of a Dark Sun 2 `RDFF` resource of `OBJEX.GFF`. A Dark
Sun 1 item with no pair becomes item 900, and so does one whose pair names an `RDFF` resource the
file lacks; from the message, 900 is a money bag. A Dark Sun 1 item number of 0 means an empty
record. The routine at `1000:03F7` is the C library's exit. The table is sorted (FND-ITEM-001),
but the lookup scans it from the start and does not rely on the order.

## Alternatives

Where the records at `1896:3FC0` come from, and what their other fields hold, is not traced here.
The over-read on an unmatched value compares whatever follows the buffer in memory; a match there
would return a word from outside the table.

## How to reproduce

Unpack the file as FND-PARTY-010 says, apply its MZ relocations for a load image at segment
`0x1000`, and disassemble segment `13D8` from offset `0x0295` to `0x038D` and from `0x1314` to
`0x164B`. Segment `13D8` starts at file offset `0x4710` of the unpacked file.
