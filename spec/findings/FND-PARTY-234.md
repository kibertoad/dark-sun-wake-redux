---
id: FND-PARTY-234
title: Every field-path wrapper and 172C setter request takes its field from script operand bytes, except overlay 189's fixed field 89
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:028B..1AA0:051C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:084E..172C:0A47
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:13E0..172C:151C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:284D..172C:299C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007761B..0x00077635
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077893..0x000778B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0009..1425:0044
tool: tools/research/exec-census/direct_callers.py, far_pointer_words.py, resident_listing.py and overlay_listing.py at commit 0352c0bb; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-233 left these generic field-accessor requests with computed
fields: the field-path wrappers 1AA0:028B, 1AA0:02D2 and 1AA0:03D2,
the setter 172C:295B, the CHAR loader's header word, overlay 204's
script setter, near indirect calls inside segment 1AA0 and an
immediate candidate at file 948E. This finding reads the wrappers, the
setter and every direct caller of each, and the last two leftovers.

### Wrappers in segment 1AA0

- 1AA0:028B takes a handle (BP+6), a count (BP+8) and a pointer to a
  word list (BP+0A). It calls the getter 1AA0:0009 with the current
  handle and the next list word, keeps the result as the next handle,
  and repeats while the ordinal is unsigned below the count and
  DS:40C4 is zero (FND-CONFIG-139). Every field it requests is a word
  of the caller's list.
- 1AA0:02D2 takes a handle, a count, a list pointer and a double-word
  value. It calls 028B with the count minus one and the same list,
  then, when DS:40C4 is zero and the result is not 9999, calls the
  setter 1AA0:012A with that result, the list word at index count
  minus one and the value. Every field is a word of the caller's list.
- 1AA0:0321 copies 28 bytes from DS:0425 into a local array, then for
  each index from its BP+0A up to its BP+0C (step two) takes a handle
  (its own BP+6 for index zero, otherwise the result of 1AA0:0209)
  and, unless that is 9999, calls 03D2 with the handle, its BP+8 list
  pointer, the local array, its BP+12 double word and its BP+10 word.
- 1AA0:03D2 keeps the list pointer (BP+8) unchanged through its loop.
  With BP+10 zero it calls the getter with list word 0; with BP+10
  positive it calls 028B with BP+10 as the count; with BP+10 negative
  it calls the getter with list word 0, compares the result with its
  BP+0C double word, and on a match calls the getter with list word 1
  and takes list word 2 as a value. Its other field reads (DS:60ED
  plus a traversal-list word) come from 1AA0:0566, which
  FND-PARTY-233 already reads.

The direct-call search finds these callers and no others:

| Routine | Direct callers |
|---|---|
| 1AA0:028B | 172C:288F (far), 1AA0:02E9 in 02D2, 1AA0:0429 in 03D2 |
| 1AA0:02D2 | 172C:2990 (far) in 295B |
| 1AA0:0321 | 172C:09A5 (far), overlay 189 +2EFD (far) |
| 1AA0:03D2 | 1AA0:0371 in 0321 |
| 172C:295B | 172C:1439, 1489 and 14FD in 13E0, 172C:2952 in 2914 |

The far-pointer word search for 028B, 02D2, 0321 and 03D2 finds only
the three far calls above as relocated or fixed-up forms; the word at
file D8B1 is the offset of `mov es:[0321], ax` with segment 4C13 and
does not form 1AA0:0321. A linear decode of segment 1AA0's whole
descriptor span, 1AA0:0009..1AA0:06BB, whose inventory rows are
contiguous and each end in `retf` before the next, holds no indirect
call or jump (register or memory operand, near or far). The same
pattern finds the three `jmp word ptr cs:[bx+...]` table jumps at
172C:08CF, 09C3 and 29F8 (positive controls).

### Callers in segment 172C

These are routines of the script interpreter. 172C:2805 reads the next
script code byte and 172C:27EF two of them (FND-SCRIPT-006).

- 172C:284D (expression byte B1, FND-SCRIPT-010) and 172C:2914
  (assignment, FND-SCRIPT-009) fill a local word list through parser
  172C:299C, which reads a count byte and that many bytes from the
  script and stores each zero-extended as a list word
  (FND-CONFIG-139). 284D passes the handle, count and list to 028B;
  2914 passes them with its value argument to 295B.
- 172C:13E0 fills its list through 299C on each of its three paths
  and passes it, unchanged, with a value from the expression evaluator
  172C:3278, to 295B: once, once per handle of a 2D40:0776 chain, or
  once per handle of a 2D40:07DE chain.
- 172C:295B does nothing when its handle is unsigned 9999 or more or
  signed 3 or less. With count one it calls the setter with list word 0; with
  any other count it calls 02D2 with the count and the list.
- 172C:084E reads script bytes into its list at [BP-3A]: each round
  stores one script byte as the next list word, reads an operator
  byte, and for operators 4 to 6 may evaluate an expression through
  3278 whose value goes after the second and later bytes. A following
  byte 53 repeats the round. List words 0 and 1 are always script
  bytes. Its count word [BP-18] starts at one and drops by one each
  round, so the BP+10 it passes through 0321 to 03D2 is zero or
  negative.

With count zero from the parser, 295B calls 02D2, which calls 028B
with count FFFF and then the setter with the word just before the
list. In the frames of 2914 and 13E0 that word is the high word of
the value they pushed for 295B, itself a script value.

### Overlay 189

Overlay 189's routine at +2C6B stores 59 (89) in its local [BP-0E] and
9999 in [BP-0C] and [BP-0A], with no other write to [BP-0E] before
+2EFD. There it calls 0321 with the handle at 4E71:0B44, the list
[BP-0E], BP+0A zero, BP+0C one, BP+10 zero and BP+12 zero. 0321 then
calls 03D2 once with BP+10 zero, which requests field 89 with the
getter.

### Candidate at file 948E

File 948E is 1425:003E. A linear decode from 1425:0009, the start of
segment-table descriptor 1's span (FND-EXE-571), reaches `mov ax, 5`
at 1425:003D, which covers 003E; the routine's own jumps target only
0018, 001C and 0040, and it ends with `retf` at 0043 before the
inventoried 1425:0044.

## Interpretation

Every generic accessor request made through 1AA0:028B, 02D2, 0321,
03D2 or 172C:295B names either a field read from the running script's
code bytes (172C:084E, 13E0, 284D and 2914) or field 89 (overlay 189
+2EFD). Field 89 is not six. Whether a script names field six at one
of the interpreter sites depends on the shipped scripts' operands,
like overlay 204's setter (FND-PARTY-156), and needs the instruction
layouts. Segment 1AA0 has no indirect transfer that could reach an
accessor with another field, and the 948E candidate is an immediate
inside another instruction. Of FND-PARTY-233's computed sources, the
CHAR loader's header word for CHAR data written at run time is the
only one this finding does not reach.

## Alternatives

- Reading the wrappers' list arguments as fixed tables fails: every
  caller builds its list on its own stack from script bytes, except
  overlay 189's single local word.
- Reading 03D2's positive-count path as reachable from these callers
  fails: overlay 189 passes zero and 172C:084E only zero or less.
- The lists are formed in SS frames and read through DS. This reading
  takes DS and SS as the same storage, as FND-CONFIG-139 does for the
  parser; Q-PARTY-089 holds that correspondence.
- A near call into 1AA0 from a computed target in another segment, or
  an entry into the 172C routines at an offset that skips their list
  setup, is outside the direct-call search.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. Run
direct_callers.py for 1AA0:028B, 1AA0:02D2, 1AA0:0321, 1AA0:03D2,
172C:295B, 172C:284D, 172C:2914 and 172C:299C, and far_pointer_words.py
for 1AA0:028B, 1AA0:02D2, 1AA0:0321 and 1AA0:03D2. List with
resident_listing.py 1AA0:0009..1AA0:06BB, 172C:084E..172C:0A47,
172C:13E0..172C:151C, 172C:284D..172C:2A20 and 1425:0009..1425:0060,
and with overlay_listing.py overlay 189 file 7761B..77A92; search the
listings for `call` or `jmp` with a register or memory operand. Read
the 1AA0 and 1425 spans from descriptors 7 and 1 at file 4B080
(segment, unk_02, flags, unk_06 words). Retained listings are under
GAME_DIR/analysis/party-record-heads/field-paths-20261018/. No original
function or game process was run.
