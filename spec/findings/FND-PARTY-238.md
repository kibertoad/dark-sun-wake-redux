---
id: FND-PARTY-238
title: No direct generic field-accessor call with an immediate, type-gated, traversal-list or transfer field selects field six, and overlay 204's setter call at +18B5 passes the immediate field 77
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:012A..1AA0:0209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0566..1AA0:05DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072F7B..0x00072FF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2588..2D40:25F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2689..2D40:271D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:27B7..2D40:2848
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:288C..2D40:2945
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:29F8..2D40:2B5D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D646..0x0008D67A
tool: tools/research/exec-census/direct_callers.py, far_pointer_words.py, immediate_search.py, candidate_starts.py, resident_listing.py and overlay_listing.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1; the overlay 204 listing with overlay_listing.py at commit e6ca37e0, Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

FND-PARTY-230 binds shipped kind-1 field six to attached-record word
+0C, the bitmap-number cache. This finding classifies the field
argument at every direct call of the generic setter 1AA0:012A and
getter 1AA0:0009. FND-PARTY-051 and FND-PARTY-216 describe both
accessors: the handle is the word at BP+6 and the field the word at
BP+8. The setter takes the field's type from DS:60ED plus the whole
field word and its offset from 2D40:06C4.

### Call census

The direct-call search finds 14 setter sites and 14 getter sites.
Positive controls located by earlier findings are among them: the
CHAR loader's setter call at 2D40:01F5 (FND-PARTY-051) and overlay
188's field-one calls at +0764 and +079D (FND-PARTY-216). The
far-pointer word search finds no relocated or fixed-up segment word
that forms 1AA0:012A or 1AA0:0009 with a stored offset; its 32 far
calls are the controls for that kind of reference. Near indirect
calls inside segment 1AA0, calls through a computed segment and
jumps are not searched.

| Field source | Setter sites | Getter sites |
|---|---|---|
| Immediate | 2D40:091F (77), 2D40:2A23 (2), 2D40:2B58 (10), 188 +0764 and +079D (1), 204 +18B5 (77) | 2D40:0813, 0AF2, 0DE5 and 188 +059F (0), 2D40:2A02 (2), 2D40:2F0E (9), 188 +05BB (1) |
| Field whose type byte was tested | 2D40:26B8 (type 8), 2D40:2718 (type 7) | none |
| Descendant graph entry word +6 | 2D40:25F1, 2D40:2B46 | 2D40:2843 |
| Transfer argument at BP+14 of 2D40:288C | 2D40:2940 | 2D40:2916 |
| Per-kind field list at DS:5ECB | none | 1AA0:05B5 |
| Other computed source | 2D40:01F5, 1AA0:0317, file EE3F (172C:297F) | 1AA0:02A7, 1AA0:040D, 0439 and 0456 |

Overlay 204's routine at +1886 (file 8D646) calls overlay 188
through trampoline 7 with its three word arguments, 6 and 0, and only
when that returns a nonzero byte pushes its word at BP+0C sign-extended
as the value, the immediate 77 (+18A9) as the field and the word at
4C13:0363 as the handle, and calls the setter at +18B5.

No immediate is six. The two type-gated setter calls are reached only
after the byte at DS:60ED plus the field held in BP-4 compared equal to
7 or 8, with no write to BP-4 between the test and the push. Shipped
FNFO 2 gives field six type 2 (FND-PARTY-230).

### Traversal field lists

1AA0:0566 reads the word at DS:5ECB plus 34 times the kind plus twice
SI, with SI starting at the count word at DS:5EEB plus 34 times the
kind (or at one when its word argument at BP+A is positive) and
counting down through zero with a signed test; a zero word is skipped. The executable's initial bytes
at DS:5ECB through six 34-byte rows are all zero. Overlay 188, in its
FNFO loader (+00DB..+0157), clears sixteen words of the rows for kinds
1 to 5 and then stores:

| Kind | Index 0 | Index 1 | Index 2 | Index 3 | Index 4 | Count |
|---|---|---|---|---|---|---|
| 1 | 5 | 2 | 4 | 0 | 0 | 2 |
| 2 | 15 | 0 | 16 | 17 | 4 | 4 |
| 5 | 0 | 2 | 0 | 0 | 0 | 1 |

Kinds 3 and 4 keep zero words and a zero count. An immediate search
for every word value from 5ECB through 5F96 (the whole six-row table)
gives 189 candidates. A linear decode of the inventory range that
holds each candidate leaves as instruction starts only these sixteen
overlay 188 stores and the two reads in 1AA0:0566 that FND-PARTY-218
located independently (the positive controls). Six candidates lie
outside every inventory range: three reads, two `ret` immediates and
`add ax, 5F00` at file 948E, none of them a store to the table; the
last stays an undecided candidate for a computed pointer.
FND-PARTY-218 shows 2D40:2196 stores the list field it followed in
word +6 of each descendant graph entry; the root entry's word +6 is the
root object instead. The entry-word sites exclude the root: 2D40:25F1
runs only for a nonzero entry index, 2D40:2B46 only for a nonzero
index in SI, and 2D40:2843 only for an entry whose step byte is 2,
which 2196 never writes to the root. Under these lists every such
field is 2, 4, 5, 15, 16 or 17.

### Transfer field argument

2D40:288C never writes BP+14. A negative value skips both its getter
and setter calls (28E3). Its 21 direct callers pass these values:

| Value | Callers |
|---|---|
| -1 | 175 +0E62, +0EC6, +1010; 184 +22ED; 188 +0750; 189 +281E, +2AAD, +2B12; 191 +02A1, +069E; 207 +0B56 |
| 4 | 173 +329E; 184 +2268; 188 +1099; 191 +096E, +0A6E; 175 +0FA1 (local set to 4 at +0F17, its only write) |
| 16 or 17 | 174 +0C9E (AX chosen at +0C7F/+0C84); 184 +2063 (local set at +200B/+2012); 189 +2992 (DI set at +28FF, +2904 or +290B to 16, 17 or 4) |
| 4, 16 or 17 | 188 +129D (local set to 4 at +1194, then 16 or 17 at +11A2/+11B6) |

The 191 +02A1 call is FND-PARTY-229's transfer request. For each
computed value, the containing inventory function has no jump into
the span between the assignment and the call.

## Interpretation

Every direct accessor call whose field is an immediate, a type-gated
local, a descendant graph field or the transfer argument names a
field other than six, under the shipped FNFO 2 types and the lists
overlay 188 installs. None of these calls reads or writes the +0C
cache through the generic accessor. The question of field-six
requests is now limited to the remaining computed sources: the CHAR
loader's header word (shipped CHARSAVE.GFF headers name fields 15,
16, 17 and 4 per FND-PARTY-052, while CHAR data written at run time is
not enumerated), the field-path wrappers 1AA0:028B, 1AA0:02D2 and
1AA0:03D2 with their callers, and the setter in 172C:295B. Overlay
204's setter call writes field 77 of whichever handle 4C13:0363 holds.

## Alternatives

- Treating the absence of a field-six immediate as proof that no
  request names field six omits the computed sources listed above.
- Treating the type-gated calls as possible field-six writers ignores
  their type 7 and 8 tests; a runtime FNFO 2 that gave field six one
  of those types would reopen them.
- Reading the traversal lists from the executable's initial data would
  give no fields at all; overlay 188 writes them at run time. A save
  or block copy into that DS range, or a write through a pointer formed
  from another base, is outside the immediate search.
- Treating the root entry's word +6 as a field number would confuse
  an object handle with a field; the three entry-word sites exclude it.
- FND-PARTY-233, which this finding replaces, listed 204 +18B5 among
  the computed sources and read its field and kind as script operands
  (FND-PARTY-156). The pushes before the call give the immediate 77 as
  the field; only the handle comes from 4C13:0363. Every other
  observation of FND-PARTY-233 is unchanged here.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Run direct_callers.py for 1AA0:012A, 1AA0:0009, 2D40:288C, 1AA0:02D2,
1AA0:028B, 1AA0:03D2 and 172C:295B, and far_pointer_words.py for
1AA0:012A and 1AA0:0009. List each call's pushes with
resident_listing.py or overlay_listing.py from an instruction start
reached in the containing inventory function, and read the field word
at BP+8 (accessors) or BP+14 (2D40:288C) from the pushes' order.
Run immediate_search.py for every value 5ECB through 5F96 and pass
its output, with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv, to
candidate_starts.py (committed with this finding); keep the START
lines. List overlay 188 file 72F7B..72FF7 and overlay 204 file
8D646..8D67A. Read the initial table with
report.mjs table: file 52ECB, six rows of stride 34, seventeen
two-byte words each. Retained reports are under
GAME_DIR/analysis/party-record-heads/field-requests-20261018/.
No original function or game process was run.
