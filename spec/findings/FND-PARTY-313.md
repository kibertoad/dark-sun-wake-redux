---
id: FND-PARTY-313
title: Cursor-number overlap and record-pointer censuses retain one literal publisher and distinguish selector snapshots from later reloads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000929DA..0x00092A73
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092A83..0x00092E92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x00092F2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092F81..0x00092FF1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009304D..0x00093088
tool: window_operands.py and overlay_listing.py at 3f3d9461; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-PARTY-289/302, FND-RNG-095 and FND-CONFIG-109/110
environment: null
---

## Observation

Q-PARTY-134 retains DS:9BE2's actual number history for the two known
208+05B2 consumers. This assesses concrete overlapping stores,
exposed record-pointer uses and the consumers' separate reloads.
DSUN.EXE is the licensed installed build, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

### One literal publisher after overlap classification

The current-inventory operand census for 9BDF..9BE4 returns eighteen
established operands in five routines. The window includes starts
of ordinary writes up to four bytes wide which could overlap the
word 9BE2..9BE4. All matches are direct DS operands: nine reference
the number word and nine the adjacent caster word at 9BE0.

There is one number write, 208+1655's word store from DI. The other
write is +1665's two-byte caster store at 9BE0, which ends exactly
before the number. The other sixteen operands read their respective
fields. The two raw outside-start candidates are reads, not stores;
neither supplies a new writer. None of the matched routines contains
a string store. This strengthens the exact-operand observation in
FND-RNG-095 without claiming that other displacements, adjusted
pointers, copied/aliased storage, wider operations or unowned code
cannot reach the field.

At the sole literal publication, the value is current DI, initially
loaded from the third setup argument at +161A. FND-PARTY-289's
child-free equal-key getter path preserves DI; FND-RNG-095's copy
primitive restores its saved DI after its byte-counted transfer.
With those actual getter/copy paths, unchanged segment/frame backing
and ordinary returns admitted, the publication equals that argument.
The getter's miss route retains its explicit reached-child contracts
under Q-PARTY-136; a decompiler expression assigning param_3 does
not independently establish preservation on that route.

The 73-byte own copy targets 9B86..9BCF and does not overlap 9BE2.
FND-RNG-095 already proves its count and odd-byte branch; no new
copy-length interpretation is needed. Initial/restored field state,
an invocation before setup and subsequent setup/republication order
remain distinct from this store-point contract.

### Known record-pointer exposure supplies type reads

The census for pointer slots 9BCF..9BD3 has seven established
operands: setup's two returned-pointer stores and its two local-buffer
rebindings, plus three LES reads. There are no outside-start
candidates. A separate 9B86 window finds only setup's destination
push and local-pointer rebind, with no outside-start candidate.

The three own LES uses are 208+1162, +1244 and +17C5. Each reads
the bound record's byte +0C. At +116D and +124F the pointer register
BX is replaced by the type-derived dispatch index; at +17D0 the
continuing branch replaces BX with the caster word. A failing type
branch in the last helper returns. No own store through those
specific loaded bindings precedes replacement or return. Reached
children, different pointer constructions and segment/frame alias
conditions remain outside that local footprint. Thus these three
known exposures are not unclassified writes to number field +5C
relative to the local buffer base.

### Consumer calls take separate snapshots

FND-PARTY-302 binds both +129E and +170F to current DS:9BE2.
The first pushes the number at +1295, then caster and calls +05B2.
Its earlier +122A number read already crossed another helper before
that push. Later own branches reload the field at +1372, +13A6
and +13BA; the complete body has no own number write. Actual
equality across those calls still needs the same field/segment and
reached-child preservation.

The second consumer +16F1 pushes the number at +1706, calls +05B2
at +170F and tests its returned signed word. A passing result later
reloads DS:9BE2 at +171C for 2C5F:0003, then reads byte +0C of
that returned resource. The second getter argument is not restored
from the first call's outgoing number slot. Under unchanged field
and DS those values agree; an intervening relevant write, changed
segment or reentrant setup can make them differ. No such native
change is asserted by the existence of the reload.

## Interpretation

Q-PARTY-134 no longer needs an unclassified literal/wider-start
publisher or a writer reading of the three known local record-pointer
uses within these census bounds. Remaining number admission is
actual setup argument/DI history, initial/restored or pre-setup state,
subsequent publication/order and writes through the excluded concrete
access forms. The two cursor consumers are field snapshots, not
independent number producers. Their equality with later reloads is
a preservation condition retained by Q-PARTY-136.

The six known setup producer families remain FND-RNG-095 and
FND-PARTY-303's source-domain work. This finding neither excludes a
whole client nor supplies all missing histories. FMT-PARTY-007 stays
supported, without a complete reading or queue closure.

## Alternatives

- The adjacent caster store overwrites number bytes: its two-byte
  destination ends at 9BE2 and does not overlap the number.
- The fixed spell-prefix copy reaches the number: its proven
  half-open destination ends at 9BCF.
- A later getter necessarily uses the number already checked by
  +05B2: it rereads the current global instead of the outgoing slot.
- The decompiler's param_3 publication proves every getter path
  preserves DI: the actual miss-path children remain required.

## How to reproduce

Run window_operands.py with current inventory and windows
9BDF..9BE4, 9BCF..9BD3 and 9B86..9B87. Positive controls are
FND-RNG-095's number store, pointer stores and destination literal,
and FND-PARTY-302's two number pushes. Retain raw candidates,
operand widths and excluded mechanisms separately.

Read descriptor 208 file 929DA..92A73, 92A83..92E92,
92EA2..92F2E, 92F81..92FF1 and 9304D..93088. Reuse the getter
and copy contracts above; do not infer their effects solely from a
caller's decompiler variables. Bounded Ghidra matches at
fbov208::09dca2 and fbov208::09dd81 recover the number store and
consumer/reload controls. Instructions decide DI liveness, two-byte
stores and both distinct loads.

Retain cursor-number-overlap-census, cursor-record-pointer-census,
cursor-record-input-root, cursor-record-repeat-root,
cursor-number-first-root-complete and cursor-number-ghidra reports
in GAME_DIR/analysis/handler-census-20261106/, with the reused
word-selector208 setup/consumer reports. No original game, emulator
session or emulated call ran.
