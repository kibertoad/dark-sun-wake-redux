---
id: FND-COMBAT-050
title: View-bound inputs have distinct reset, restore and packed script producers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C322..0x0008C3E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C3FD..0x0008C417
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C481..0x0008C487
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007111B..0x0007114D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA4F..0x0007DA67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A1C6..0x0007A209
tool: tools/research/exec-census/immediate_search.py, overlay_listing.py, trampoline_target.py and direct_callers.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-COMBAT-049 reads the four bound words DS:14D7, 14D9, 14DB and
14DD. An encoded immediate/displacement search for these four values
finds candidate stores in descriptors 187, 190, 192 and 204, beside
reads and overlapping decodes. This is a candidate search, not a
complete writer census: computed, aliased and bulk writes are outside
its contract. The independently known bound reads in 2C5F:01F3 and
the reset stores of FND-CONFIG-044 are positive controls.

### The script request producer has its own reached selector

Trampoline 5787:0066 resolves to descriptor 204 +0169 at file 8BF29.
FND-RNG-022 and FND-VIDEO-010 separately identify this dispatcher and
its script input route. Its current request word is decremented,
accepted only when the unsigned result is at most 52, doubled and
used to select one of 53 code-segment word targets at local 06C7,
file 8C487. A fresh bounded table query gives request one -> +02B1
as an independent known control, request 33 -> +0562 and request
38 -> +063D.

Request 33 starts at file 8C322. Let A and B denote the signed
doublewords read from SS:BP+0A and SS:BP+0E. Its sequence is:

1. Divide A by signed 1000 and store the quotient's low word at
   DS:14D7. Divide A again and store the remainder's low word at
   DS:14D9.
2. Divide B by signed 1000 and store the quotient's low word at
   DS:14DB. Divide B again and store the remainder's low word at
   DS:14DD.
3. Add the current 14D7 word to 14DB, and current 14D9 to 14DD,
   retaining each low word.
4. Multiply each current word by signed 16 in order 14D7, 14D9,
   14DB, 14DD, storing only the low product word.
5. Using signed word comparisons, replace each horizontal word with
   2048 when it is at least 2048, and each vertical word with 1568
   when it is at least 1568. Smaller signed values remain unchanged.

The branch imposes these upper limits but no local lower limit,
rectangle-order check or minimum width/height test. Division is signed
with truncation toward zero and signed remainder. Narrowing, additions
and products occur before the later comparisons; the original
doublewords are not simply clamped in a larger integer type.

There are no child calls on this selected branch. It joins the
dispatcher epilogue at +06C1, which returns DI as AX. DI was initialized
to zero in the dispatcher prologue and is not changed on this path.
Its returned zero therefore does not certify a well-ordered or
screen-sized rectangle.

Request 38 is a separate selected branch. It shifts the low word
of A and the low word of B left four bits, retaining low words, then
passes those x/y words and mode word one to 2C5F:01F3. The caller
cleans six argument bytes and returns its unchanged DI zero through
the same epilogue. It does not propagate the view helper's AX as the
dispatcher result. This is an additional local coordinate producer,
not proof that every shipped script invokes it with admitted inputs.

### Reset and restore do not force every later input to be the reset

The region-transition entry descriptor 187 +1132, exported through
56EF:0093 and read in FND-COMBAT-044, reaches a continuation at
+12EB. Its four successive stores set DS:14D7/14D9 to zero,
DS:14DB to 2048 and DS:14DD to 1568. Later it requests adjustment
for the current slot from 4C13:0369 with mode zero. Other preceding
transition and following helper effects remain dependencies; this
does not establish successful native region loading.

The diagnostic-gated reset branch of the keyboard dispatcher already
identified in FND-CONFIG-044 writes the same four values. Its local
gates test bits zero/one of the returned word from 44B6:0011 and a
nonzero byte DS:143C; otherwise it calls the separate descriptor-201
entry. The reset stores follow its message call, so that call's
return and storage preservation remain conditions.

The load continuation read in FND-SAVE-005 instead takes the four
words from its local GREQ destination at SS:BP-14 through BP-0E,
and writes them to these globals when its reader result is zero.
The instructions between that result test and the four stores add
no order or minimum-extent validation. Actual reader/archive outcomes,
source lengths and buffer admission are separate; this observation
does not rely on the neighboring tenth byte discussed in that older
finding or claim a complete save-format contract.

## Interpretation

This answers the identified local producer branches and script packing
part of Q-COMBAT-014. Fixed reset values do not by themselves admit
every later input to the view calculation: the script and restoration
routes are distinct. Complete writer/input coverage, actual shipped
request domains, restoration admission, aliases and state lifetime
through later callbacks remain open. No native rectangle or display
result was observed.

## Alternatives

- Assuming reset values always remain current omits the selected
  request-33 stores and conditional restore stores.
- Assuming the script producer guarantees nonnegative bounds or a
  rectangle at least 320 by 200 adds tests this branch does not make.
- Returning dispatcher zero does not propagate rendering success or
  validate the four words; both reviewed branches retain DI zero.
- Raw search hits decoded inside other instructions are candidates,
  not additional qualified mutation sites or evidence of absence.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the project evidence Python.
Run immediate_search.py with hexadecimal 14D7 14D9 14DB 14DD,
retaining the known predicate reads and diagnostic reset as controls.
Resolve 5787:0066 and 56EF:0093 with trampoline_target.py.

Run report.mjs table with that executable/hash, start 8C487 hexadecimal,
count/limit 53, stride two, and width-two target at field offset zero.
The decrement/unsigned-52 guard is the count evidence. Check request
one as the known control before identifying requests 33 and 38.
Read overlay_listing.py descriptor 204, 8BF29..8BF4E,
8C322..8C3E3, 8C3FD..8C417 and 8C481..8C487, hexadecimal,
half-open. Read descriptor 187 from its established +1132 entry at
70F62 through 7114D; descriptor 190 at reached +1E86,
7A1C6..7A209; descriptor 192 from its established +05ED entry
at 7D8ED through the stores ending at 7DA67. Follow the entry and
branch provenance in FND-COMBAT-044, FND-CONFIG-044 and FND-SAVE-005;
do not treat arbitrary-offset disassembly as reachability proof.
Inspect signed division widths, low-word stores, multiply truncation,
signed upper comparisons and the common DI-to-AX return. Local query
configuration and reports stay in GAME_DIR/analysis/combat-actions/.
These static queries execute no original function.
