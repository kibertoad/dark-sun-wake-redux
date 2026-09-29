---
id: FND-CONFIG-139
title: The B1 expression path parses selectors and chains lookups before a conditional clear
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:284D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:299C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:028B
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and six-case keyed-table inspection
environment: null
---

## Observation

FND-SCRIPT-010 identifies expression byte B1's call to
172C:284D. Its complete local body occupies
`0x0000ED0D..0x0000EDD4`. It initializes a double-word
result to zero and uses the known fill helper 1000:3FA2
(FND-CONFIG-134) to clear a 64-byte local array. It then
passes output addresses for a seed, count and that array to
local parser 172C:299C. Only return byte one calls lookup
wrapper 1AA0:028B with the seed, count and array address.
Other parser results leave the result zero.

After that optional lookup, a zero double word at DS:40BC
calls overlay entry 5702:00B1, the known common returning
flag-clear path (FND-CONFIG-135). A nonzero double word skips
that clear. With DS:40C4 zero, it uses the last selector
(count minus one) to read a byte at DS:60ED plus that
selector. Byte six or an unsigned byte at least 128 stores
the result as a double word at 4C13:024D. The common
return splits the local result across DX and AX. These
conditions assign no gameplay identity to the result or fields.

Parser 299C, `0x0000EE5C..0x0000EF2B`, first reads a
high-byte-first word through 172C:27EF. Values below 32768
are negated as a double word and their low word is passed to
resident 2D40:0AB6; its result becomes the seed output.
That callee's effects remain unread. For other values, the
low 15 bits must match one of exactly six keys. The match
reads the following word from 4C13 as the seed:

| Low-15-bit key | Seed field |
|---|---|
| 37 | 4C13:0369 |
| 38 | 4C13:0367 |
| 39 | 4C13:0365 |
| 40 | 4C13:0363 |
| 43 | 4C13:035F |
| 44 | 4C13:0361 |

No keyed match returns byte zero. Both successful seed
branches read one zero-extended byte as the count output,
then read that many bytes and store each as a word in the
supplied array. Count zero skips those stores and returns one.
The complete parser has no comparison limiting count to the
caller's 32-word array. It writes its supplied outputs; this
reading does not establish their valid ranges from shipped input.

Word reader 27EF, `0x0000ECAF..0x0000ECC5`, calls byte
reader 2805 twice, combines their zero-extended bytes high byte
first, and has no further call. FND-CONFIG-138 supplies the
byte reader's ordinary and failed-check effects.

Lookup wrapper 1AA0:028B, `0x0000FE8B..0x0000FED2`,
sign-extends its seed word into a local double word and initializes
its ordinal to zero. It calls resident 1AA0:0009 with the
current result's low word and the next selector word. It retains
the returned DX:AX value, advances the selector pointer by two
and increments the ordinal. It then stops if DS:40C4 is nonzero;
otherwise it repeats while the ordinal is unsigned less than the
supplied count. The first call precedes the count comparison,
so count zero does not skip that call. The wrapper has no direct
iterator-flag write, but its callee's effects remain unread.

The root and parser's seven named segment operands are declared MZ
relocations: zero, 0AA0, 47E0, 4702, 3C13, 3C13 and
1D40 map to 1000, 1AA0, 57E0, 5702, 4C13, 4C13
and 2D40. The parser's six keyed segment loads are also
declared relocations mapping 3C13 to 4C13.

## Interpretation

This expression branch is a concrete source of additional
callee effects during parameter evaluation. Its root has no
direct flag assignment; it can clear the flag through its
post-lookup zero-pointer branch, and otherwise depends on the
seed and lookup callees. The chained wrapper alone cannot
establish preserved state merely because it lacks a direct store.
This narrows the dependency boundary in Q-CONFIG-008 without
proving the flag or index at the later rest iterator.

## Alternatives

FND-CONFIG-140 reads the lookup and conditional table writer;
FND-CONFIG-141 reads the seed wrapper and shared-lookup scan.
Q-CONFIG-008 retains stored-field and metadata producers,
reachable B1 expressions, seed and
count ranges, array validity and other intervening state changes.
A reading that the local lookup wrapper invokes no callee when
count is zero is ruled out by its pre-comparison first call.
A reading that the parser bounds the count to the caller's
32-word array is ruled out by its complete byte-count loop.
Neither local shape proves that malformed counts occur in shipped
scripts or that a visible defect follows. No rule or parser is
implemented from this partial reading.

## How to reproduce

Read the complete root, parser, word reader and lookup wrapper
at the stated bounds. Decode exactly six key double words at
172C:2A6B and their six target words at 172C:2A83; follow
each target's field read and return path. Track count zero and
the ordering of lookup calls before the wrapper's unsigned count
test. Verify MZ operands at `0x0000ED2A`,
`0x0000ED52`, `0x0000ED60`, `0x0000ED70`,
`0x0000ED90`, `0x0000EDBA`, `0x0000EE88`,
and the six keyed loads `0x0000EEBD`, `0x0000EEC8`,
`0x0000EED3`, `0x0000EEDE`, `0x0000EEE9`,
`0x0000EEF4`. Apply load segment 1000 before naming mapped
addresses. Keep 1AA0:0009 separate from the following local
routines; the next exported wrapper does not delimit its body.
Compare FND-CONFIG-134, FND-CONFIG-135 and FND-CONFIG-138.
