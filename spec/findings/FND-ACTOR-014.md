---
id: FND-ACTOR-014
title: Script request 8 reads MONR resource 1 as 42-byte records of a key and ten pairs of words, and picks a pair by the party's levels or divides by a pair's second word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF3B..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C495..0x0008C497
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C0E6..0x0008C0F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D307..0x0008D4E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D4E9..0x0008D571
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:05B5..38FF:0615
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x56FBCB..0x570039
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, script_decode.py and monr_records.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `+offset` in overlay 204, whose code starts at file `0x8BDC0` of the
installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`).

**The route.** The request dispatcher `+0169`, entered through trampoline `5787:0066`, takes the
request number less 1, rejects values above 52 unsigned, and jumps through the 53 words at
`+06C7` (file `0x8C487`) (FND-COMBAT-050, FND-RNG-022). Word 8, at file `0x8C495`, is `+0326`.
That branch pushes the low byte of its fourth argument (the doubleword at `[bp+0x0E]`), then the
low byte of its third (`[bp+0x0A]`), near-calls `+1547` and returns the result as the
dispatcher's. Script opcode `0x22` calls `5787:0066` with its four parameters (FND-VIDEO-010);
a caller search finds one far call of the trampoline, at file `0xD46B` in the opcode's routine
`172C:0F84`, and no other caller of `+1547` than `+0330`. Below, A is the first byte argument of
`+1547` and B the second.

**The resource.** `+1547` calls `38FF:05B5` with the tag `MONR` (`0x524E4F4D`), the number 1 and
the address of a local doubleword. `38FF:05B5` stores 0 in all four bytes of that doubleword
before it looks the resource up, and on success stores a doubleword there
(`38FF:05D5..38FF:05E0`, `38FF:0541..38FF:054C`), the size the other callers of the routine
use as the resource's size (FND-SCRIPT-008). `+1547` divides it by 42 as a signed doubleword and
keeps the quotient as the record count. It then calls `38FF:04AB` with `MONR`, 1 and the
address of a far pointer. When the pointer is 0 it returns 0 at once. Otherwise it reads the
records and, at the end, passes the pointer to `444C:0092`, as the icon consumer of
FND-COMBAT-038 does with its resource pointer after use, and returns the result word.

**The key.** The key is 255 when B is not 0 and the word `current_region` (`DS:140C`) otherwise.
For each record `r` from 0 while `r` is below the count, the word at record offset 0 is compared
with the key; a record whose word differs is skipped. For a record that matches, `+1547` calls
`+1729` for a score and then does one of two things.

**The score** (`+1729`). For each party slot 0 to 3 whose `combatants` record has a
`character_id` other than 0, it takes the three bytes at offset `0x1E` of the slot's
`combatant_details` record, the largest of them (0 when all are 0) and the number of them that
are not 0, and adds the largest plus the number less 1 to a 16-bit total. A present slot whose
three bytes are all 0 adds -1. The total is the score.

**A not 0.** A local index starts at -1 once per call. A loop runs `i` from 0 while `i` is
below 10 and the index is still -1: when the word at record offset `2 + 4 * i` equals the word
at `549B:00C0`, the index becomes `i`; `i` is then increased. When the index is still -1 the
result is 0. Otherwise the result is the score times 2 divided, as signed words, by the word at
record offset `4 + 4 * i`, where `i` is the loop's value on leaving it: one past the matched
pair. A quotient below 1 becomes 1 and one above 6 becomes 6. With a match at pair 9 the divisor
is read from offset 44, which is offset 2 of the next record. A divisor of 0 is not tested for.

**A is 0.** The best distance starts at 999 once per call. Three times, it draws a pair number
from the random number generator (FND-RNG-001), as the draw times 10 divided by 32,768, drawing
again for as long as the drawn pair's word at offset `2 + 4 * n` is 0. When the absolute
difference between the pair's word at offset `4 + 4 * n` and the score is below the best
distance, the result becomes the pair's first word, that word is stored at `549B:00C0`, and the
best distance becomes the difference. A record whose ten first words are all 0 never leaves the
drawing loop.

**The shipped resource.** `RESOURCE.GFF#MONR/1` is 1,134 bytes, 27 records with nothing left
over. Its keys are 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 18, 20, 26, 27, 30, 31, 33, 34,
35, 39, 40, 43, 44 and 45, each once; 255 is not among them. Every record has 7 to 9 pairs whose
first word is not 0, at pair numbers 0 upward, and its last pair (number 9) is 0 and 0. The
first words range from 70 to 279; the second words of the records keyed 2 and 3 are all 0, and
the others range from 6 to 96.

**Uses.** No instruction of the shipped GPL and MAS scripts gives opcode `0x22` the literal
request 8. One, at `GPL/27` offset `0x01C8`, takes its request number from the variable
`var7[8]`. The decoder stopped once, inside `GPL/37` between `0x0D39` and `0x0D58`.

## Interpretation

`MONR` resource 1 is a table of 42-byte records: a key that is a region number (or 255), and ten
pairs of words, of which the first word is a value request 8 stores at `549B:00C0` and looks up
again, and the second is compared with, or divides, the party's score. Request 8 with A equal to
0 picks, for the party's region, the best of three random non-empty pairs, the one whose second
word is nearest the score; with A not 0 it finds the pair stored last and returns a number from
1 to 6 from the score and the second word of the pair after it.

## Alternatives

- That the size `38FF:05B5` gives is the resource's size in bytes rests on its uses; the routine
  `38FF:04D5` it calls was read only as far as the stores above.
- Whether request 8 runs at all in play is not shown: no script uses it with a literal number,
  and what `var7[8]` holds when `GPL/27` reaches `0x01C8` was not traced. Overlay code calling
  `+1547` through the trampoline `5787:003E` was not found by the caller search, which looks for
  far calls with a fixed-up segment word only.
- What `549B:00C0` means, and which other code reads or writes it, was not searched; nor what the
  first words of the pairs name. That the three bytes at `0x1E` of `combatant_details` are class
  levels is not shown here.
- Using the pair after the matched one may be an error in the original's loop; the reading
  records what the instructions do.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<dir>` the installation: `overlay_listing.py <dsun> 204 8BDC0 8E660`;
`resident_listing.py <dsun> 38FF:04AB..38FF:0615 444C:0092..444C:00C0`;
`direct_callers.py <dsun> 204+1547 204+1729 204+0169`; read the word at file `0x8C495`;
`script_decode.py <dir>/GPLDATA.GFF <dir>/OBJEX.GFF --all` and search for opcode `22` with first
parameter 8; `monr_records.py <dir>/RESOURCE.GFF`. The listings are retained under
`GAME_DIR/analysis/actor-monr-20261018/`. No original function or game process was run.
