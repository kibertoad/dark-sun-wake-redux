---
id: FND-PARTY-239
title: No shipped script names field six in a field path or a 0x33 round, so no generic field-accessor request the game makes selects field six
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:084E..172C:0A47
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:299C..172C:2A6B
tool: tools/research/exec-census/script_decode.py and resident_listing.py at commit e6ca37e0; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-234 shows that the interpreter routines `172C:284D` (expression byte `0xB1`),
`172C:2914` (assignment to a target code of `0x10` or more) and `172C:13E0` (opcode `0x40`)
pass the generic field accessor the bytes of a field path, each byte one field number, and
that `172C:084E` (opcode `0x33`) passes its list words 0 and 1. FND-SCRIPT-024 gives the
layouts with which `script_decode.py` decodes all 350 `GPL ` and `MAS ` resources.

**Field paths.** The decode reads 2,235 field paths: 1,965 after the expression byte `0xB1`,
267 as assignment targets and 3 in instructions `0x40` (`GPL/142` `0x0418` and `0x0513`,
`MAS/50` `0x01EE`; none of the three has the word `0x8000` that makes the handler read
nothing). Every path's word is `0x8025`, `0x8026`, `0x8027`, `0x8028`, `0x802B` or `0x802C`
(2,232 paths), one of the six keys of `172C:2A6B`, or `0x00CC` (3), a handle by number; so
every path has a count byte. 2,004 paths have a count of 1 and 231 a count of 2; none has a
count of 0. The first field byte is 0, 3, 7, 8, 10, 13, 15, 19, 22, 25, 26, 27, 29, 30, 31,
32, 33, 34, 41, 42 or 77; the second, present only after a first byte of 15, is 37, 40, 41,
42, 44, 50, 51, 52, 53, 54, 55 or 76. No field byte is 6.

**Instruction `0x33`.** `172C:084E` reads an expression and two bytes into its locals
`[BP-0A]` and `[BP-0C]`, which it passes, each through `2D40:06C4` with the handle's state
byte, as the index range of `1AA0:0321` (`172C:093D..172C:0983`). Then, per round, it stores
the round's field byte as the next word of its list at `[BP-3A]` (`172C:0892..172C:08A1`) and,
from the second round on, the low word of the round's expression after it
(`172C:08E1..172C:08F2`); the first round's expression goes to `[BP-12]` instead. The list
words 0 and 1 are therefore the field bytes of the first two rounds. The 188 instances of
`0x33` in the decode, two at the top level and 186 inside expressions, have the bytes 16 and
18 (184) or 16 and 17 (4); their rounds name field 0 first (187) or field 0 alone with
operator 3 (1), and a second round names field 1 (29), field 0 (3) or field 3 (1).

The field bytes 77 and 15, which FND-PARTY-238 finds as immediates at other accessor calls
and which FND-PARTY-052 finds in the shipped `CHAR` chunk headers, appear among the path
bytes; they are the positive controls for reading a field number at these sites.

## Interpretation

No shipped script names field six through any interpreter route to the generic field
accessor, whatever kind of handle the path reaches. With FND-PARTY-238 (the direct accessor
calls with an immediate, type-gated, traversal-list or transfer field, and overlay 204's call
with field 77), FND-PARTY-234 (every wrapper and `172C` setter request takes a script operand
byte or field 89) and FND-PARTY-235 (the `CHAR` loader's header word is never six), no request
the game makes through the generic accessor selects field six. The bitmap-number cache at
`+0x0C` of a kind-1 record is never read or written through the generic accessor; what
reaches it are direct accesses, FND-PARTY-224 and the findings after it read some, and the
census of the rest is Q-PARTY-087.

## Alternatives

- A count-zero path, which would take the setter's field from the high word of the assigned
  value (FND-PARTY-234), does not occur in the shipped scripts.
- The decode covers the bytes as shipped. A script resource changed at run time, or a run
  that starts inside an instruction, is outside it; FND-SCRIPT-024 finds every literal jump
  and call target on an instruction start.
- The reading of FND-PARTY-234 takes DS and SS as the same storage for the lists
  (Q-PARTY-089); a computed near call into segment `1AA0` from another segment stays outside
  the direct-call searches.

## How to reproduce

Use the installed `DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`),
`GPLDATA.GFF` (2,191,945 bytes, `2bd1d2b989eef2dcb0545feb95975fca`) and `OBJEX.GFF`
(6,816,516 bytes, `e08d3cac153c547f6804ca27b155915d`) with the locked evidence Python. In
`tools/research/exec-census/`, run `resident_listing.py <dsun> 172C:084E..172C:0A47` and
`script_decode.py GPLDATA.GFF OBJEX.GFF --all`; collect every `path(WORD; BYTES)` and
`path(WORD)` in the output with their bytes, every `: 40 ` instruction, and every `33 `
instruction with its two bytes and its `[field opN ...]` rounds. The listing is retained as
`all-scripts.txt` under `GAME_DIR/analysis/script-instructions-20261018/`. No original
function or game process was run.
