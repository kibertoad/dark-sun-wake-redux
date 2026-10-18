---
id: FND-SCRIPT-024
title: Every script instruction's handler reads a fixed operand layout, apart from 0x33, 0x40 and 0x48, and 349 of the 350 shipped scripts decode with these layouts to their last byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0A69..172C:20B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:084E..172C:0A47
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:016E..172C:018F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0C34..172C:0C5F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2794..172C:2A6B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3197..172C:3278
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0002031C..0x0002037E
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00202969..0x0020296B
tool: tools/research/exec-census/resident_listing.py, direct_callers.py, overlay_listing.py and script_decode.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Addresses are in the resident load image at segment `0x1000`. "E" is one call of the
expression reader `172C:3278` (RULE-SCRIPT-004's `read_number`), "Pn" one call of
`172C:367B(n)`, which calls it n times, and "S" one call of the string reader `172C:25D6`
(`read_string`). Every handler of the 129-word table at `57E0:030A` (FND-SCRIPT-005) was
read from its first instruction to its return, with the near routines it calls, and reads
these operands in this order before or while it does its work:

| Operands | Opcodes |
|---|---|
| none | `0x00`, `0x0D`, `0x0E`, `0x15`, `0x19`, `0x2A`, `0x2B`, `0x31`, `0x35`, `0x38`, `0x47`, `0x51`, `0x61`, `0x67`, and the fifteen that stop the script (FND-SCRIPT-005) |
| E | `0x08`, `0x09`, `0x0C`, `0x0F`, `0x12`, `0x13`, `0x17`, `0x18`, `0x1D`, `0x1E`, `0x1F`, `0x24`, `0x29`, `0x2E`, `0x30`, `0x34`, `0x36`, `0x3D`, `0x41`, `0x45`, `0x46`, `0x52`, `0x54`, `0x5D`, `0x5F`, `0x62`, `0x64` |
| P1 | `0x02` to `0x07`, `0x3B`, `0x3E`, `0x3F`, `0x42`, `0x43`, `0x44`, `0x63` |
| P2 | `0x01`, `0x0A`, `0x0B`, `0x11`, `0x14`, `0x1A`, `0x20`, `0x21`, `0x27`, `0x2D`, `0x32`, `0x37`, `0x3A`, `0x49`, `0x4F`, `0x50`, `0x5A`, `0x5B`, `0x76` to `0x80` |
| P3 | `0x10`, `0x2F`, `0x3C`, `0x4B`, `0x58`, `0x59`, `0x65`, `0x66`, `0x6C` to `0x6F` |
| P4 | `0x1B`, `0x1C`, `0x22`, `0x39`, `0x5C`, `0x70` |
| P5 | `0x5E`, `0x68`, `0x69` |
| P6 | `0x25` |
| P7 | `0x6A`, `0x6B` |
| S | `0x23` (`172C:016E`), `0x2C` |
| E, then an assignment target | `0x16` (`172C:2C1A`, FND-SCRIPT-009) |
| E, E, S | `0x28` (`172C:2C05`, RULE-SCRIPT-009) |
| the menu of RULE-TALK-001 | `0x48` (`172C:2CF8`) |
| see below | `0x33` (`172C:084E`), `0x40` (`172C:13E0`) |

`0x0B` and `0x2D` share `172C:0B5E`, which reads P2 and calls `172C:0C34`; `0x0D` loops on
the far routine `5702:007F` with stack buffers and reads no script byte. The routines the
handlers call after their operands (`172C:0C34`, `2DB9`, `3197` and the trigger routines
`2A8F`, `2AD5`, `2B29`, `2B89`, `2DF8`, `2EA1`, `2F46` and `3026`) call no routine that reads
script code. The opcodes `0x01` to `0x07`, `0x11` and `0x76` to `0x7F` operate on the
address `4C13:00D4` that P leaves for the first parameter.

A field path is what `172C:299C` reads: a word, high byte first; when it is below `0x8000`, or
its low 15 bits are one of `0x25`, `0x26`, `0x27`, `0x28`, `0x2B` and `0x2C` (the table at
`172C:2A6B`), a count byte and that many bytes follow, and otherwise nothing more is read
and the routine returns 0. The expression byte `0xB1` (`172C:284D`) and an assignment
target whose code, after the reader removes bit 7 and bit 6, is `0x10` or more
(`172C:2C1A` to `172C:2914`) are each followed by one field path.

`0x40` peeks the next word with `172C:2794`, which reads two bytes through `172C:27B0`
without moving. When the word is exactly `0x8000` (unsigned comparisons at `13F8` and
`14A7`) the handler reads nothing; otherwise it reads a field path and then E, on each of
its three paths.

`0x33` reads E, two bytes, and then rounds: when the next byte is `0x53` it is skipped
(`172C:20B3`); a field byte and an operator byte follow; when the operator is 4, 5 or 6 the
handler looks up the byte at `DS:60ED` plus the field and, through the ten words at
`172C:0A55`, reads E when that byte is 0 to 5 or 9, and nothing when it is 6, 7, 8 or above
9. Another round follows while the next byte is `0x53`.

A string of kind 2 is read as 299 bytes: `172C:325F` returns `(b << 2) & 0xFF` for each
byte `b` (its other term, `(b & 3) >> 6`, is always 0), which is never 3, so the loop runs to
its 299-byte limit.

All reads of script code go through two routines: `172C:281B`, which reads the byte at the
current offset, and `172C:27B0`, which reads one a given count ahead. A search of the load
image and every overlay for instructions whose 16-bit displacement is `0x0255` or `0x0257`
(the far pointer to the script buffer, `4C13:0255`) finds those two, the loader's copy at
`172C:05E0..061B` and overlay 169's allocation and release (`+0381`, `+0386`, `+0500`,
`+0514`, `+0519`). `172C:281B` is called from `013D` (the trace instructions), `0888` and
`0901` (`0x33`), `25D6` and `2713` (the string reader), `280C` (`172C:2805`, which reads
and moves on) and `2D7E` (the menu); `172C:27B0` from `2797` and `27A4` (`172C:2794`) and
`3554` (the expression reader). `172C:2805` and `172C:27EF` are called only from the
routines above and the handlers. The only far calls into segment `172C` are nine of
`172C:000C` (FND-SCRIPT-013); no relocated segment word or overlay fixup names `172C`
outside a far call, apart from its segment-table entry.

`tools/research/exec-census/script_decode.py` decodes every `GPL ` and `MAS ` resource of
`GPLDATA.GFF` from offset 0 with these layouts, taking the 98 bytes at `DS:60ED` from
`OBJEX.GFF`'s `FNFO/2` (FND-CONFIG-153). 349 of the 350 resources decode to their last
byte. All 26,003 literal code offsets their jumps, `if`, `else`, `while`, `compare` cases,
local calls and menu rows name fall on instruction starts, as do all 797 literal starts of
another script that `0x14` names. `GPL/37` stops at offset `0x0D38` (file `0x0020296A`),
where instruction `0x77` at `0x0D37` begins its first parameter with the byte `0xB3`, which
the expression reader does not accept; decoding resumes at `0x0D58`, a literal jump target,
and reaches the end. Changing one layout makes the decode fail: with `0x22` reading three
parameters 83 resources decode, with `0x2A` reading one 268, with `0x25` reading five 203,
and with field type 9 reading no expression in `0x33` 258.

Over the decoded instructions, opcode `0x4F` occurs 22,881 times, `0x16` 17,360 and `0x22`
3,203; 55 opcodes, `0x12` and `0x4B` among them, never begin an instruction at top level.
111 strings have kind 1 and the others kind 5; none has kind 2 or another kind byte. `0x33` occurs twice at top level and otherwise
nested in expressions; its rounds name fields 0, 1 and 3, and operators 4 to 6 occur only on
field 0, whose `FNFO/2` byte is 9.

## Interpretation

A script can be decoded instruction by instruction from its first byte: apart from `0x33`,
`0x40` and the menu, each opcode fixes the shape of its operands, and every operand is an
expression, an assignment target, a string or a field path whose own bytes say how long it
is. `0x33` depends on the type bytes the game loads from `FNFO/2`, and `0x40` reads nothing
for the word `0x8000`. The shipped scripts are one stream of instructions with no data in
between: every literal target lands on an instruction start, and a wrong layout breaks the
decode. The byte `0xB3` in `GPL/37` reaches the reader's error routine if `0x0D37` runs; the
neighbouring bytes `80 27 01 19` read as a field path after `0xB1`, so it may be a mistake
for `0xB1`. Kind-2 strings cannot end before 299 characters, and no shipped script uses them.

## Alternatives

- The far routine at `57E0:02F6`, which sees each opcode before its handler (FND-SCRIPT-005),
  could move the offset; it could reach the readers only through `172C:000C`, which starts a
  new run, and what sets it is open (Q-SCRIPT-003).
- The menu loop also ends after 25 offered rows (RULE-TALK-001), which depends on the rows'
  conditions at run time; the decoder reads to the `0x4A` byte, and every shipped menu
  decodes that way.
- `0x33` reads its type bytes through `DS`; the decode assumes the game has loaded `FNFO/2`
  there, which Q-CONFIG-008 still holds.
- Whether instruction `0x0D37` of `GPL/37` is reached was not read.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`),
`GPLDATA.GFF` (2,191,945 bytes, `2bd1d2b989eef2dcb0545feb95975fca`) and `OBJEX.GFF`
(6,816,516 bytes, `e08d3cac153c547f6804ca27b155915d`) with the locked evidence Python. In
`tools/research/exec-census/`, list with `resident_listing.py` the ranges
`172C:0A69..172C:20B3` (one range per handler start), `172C:084E..172C:0A47`,
`172C:016E..172C:018F`, `172C:0C34..172C:0C5F`, `172C:2794..172C:2A6B`,
`172C:2DB9..172C:2DF8`, `172C:2A8F..172C:2BF6` and `172C:3197..172C:3278`, and read the
words at `172C:0A47` (7), `172C:0A55` (10) and the double words and words at `172C:2A6B`
(6 and 6). Run `direct_callers.py` for `172C:2805`, `27EF`, `281B`, `27B0`, `20B3`, `3278`,
`367B`, `25D6`, `2C1A`, `299C`, `2794`, `284D`, `2914`, `084E`, `2DB9` and `3197`; search the
load image and overlays for far calls whose segment is `172C` (raw `0x072C`), `1695` (raw
`0x0695`) or overlay segment word `0x0028`, for relocated segment words and overlay fixups
with those values outside far calls, and for ModRM or `moffs` operands with displacement
`0x0255`, `0x0257` or `0x0295`. Then run `script_decode.py GPLDATA.GFF OBJEX.GFF --sweep`,
and repeat it with each layout change named above. Retained listings, the sweep output and
the controls are under `GAME_DIR/analysis/script-instructions-20261018/`. No original
function or game process was run.
