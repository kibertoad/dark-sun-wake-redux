---
id: FND-PARTY-310
title: The current ES field-writer census reduces graphics callback overlap to two setters, button construction and three fixed-data writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0008..3EBE:00F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:00FF..3EBE:0427
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0211..3F96:02AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B8F1..0x0005B910
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C98C..0x0005C99F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008207D..0x0008209C
tool: field_stores.py, direct_callers.py and overlay_listing.py at e57ba583; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-EXE-588, FND-UI-007, FND-PARTY-309
environment: null
---

## Observation

Q-PARTY-134 retains changed graphics record fields as one possible
route around FND-PARTY-309's zero-field exclusion. This assesses a
finite set of concrete store mechanisms. Installed DSUN.EXE is size
634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

The existing field_stores.py searches the current committed inventory
for its documented first-memory-operand write forms with an explicit
ES override and encoded base/index displacement 57 through 61.
That window includes starts of ordinary word/double-word writes
which can overlap record bytes 5A..61. Operand widths decide actual
overlap. It also includes shorter nonoverlapping mask writes; this
is a candidate census, not an assertion that all matches write APFM.

The established pass returns 26 instructions:

| Cohort | Count | Classification relative to the named record base |
|---|---:|---|
| Word mask writes at displacement 58 | 7 | Four in 3EBE:071F, three in 3F96:02F8. Each ends at 5A and does not touch bytes 5A..61. |
| Word writes at 5C, 5E and 60 in 3EBE:0008 | 14 | The known button constructor from FND-EXE-588, not an unclassified APFM-specific writer. |
| Double-word stores at 5A and 5E | 2 | The known APFM setters 3F96:0211 and 025E assessed in FND-PARTY-309. |
| Byte stores at 5E in overlays 173 and 195 | 3 | Fixup-backed fixed data segment 4C4F, not a pointer loaded from an APFM record argument. |

For the button constructor, fresh direct_callers.py recovers the
known sole encoded far call at file 2FFAE, equivalent to 3A8E:04CE.
FND-EXE-588 binds that call to the window constructor's BUTN tag
case and retains its tag/record provenance. This is not a proof
against a changed tag, misbound record, alternate call or aliased
buffer. The constructor's internal presentation behavior need not
be reread to classify its known caller here.

The three fixed-data writes are 173+0FEA through SI, 173+207A
through BX, and 195+0926 through DI. Each loads ES from declared
overlay fixup word 0308 immediately before the store, resolving to
descriptor 97's segment 4C4F. Two store byte FF; the other stores CL.
Their possible effect on an APFM callback requires actual record
backing/segment alias with that indexed fixed-data address. An equal
displacement alone does not establish that identity. Their index
histories and nonalias admission are not silently assumed settled.

### Raw candidates and search boundary

The second pass returns four raw candidates. Two byte writes at
file 2A7A8 and 2B2B3 have displacements 58 and 59 respectively;
even if admitted, their one-byte widths do not overlap callback
bytes for the same record base. The apparent word writes at 34DB6
and 34E03 start inside the known prefixed double-word setter stores
at 34DB5 and 34E02. They are not additional established writers.
Incoming transfers into their interiors remain outside this result.

Both known setters are positive controls for the write forms; the
constructor call is the independently known far-call control. This
search excludes other segment overrides/default segments, other
base displacements or adjusted pointers, non-enumerated write
instructions, string/block copies, initial/non-resource record
construction, code outside inventory ownership and alternate streams.
It is not a complete alias or buffer-lifetime audit.

## Interpretation

The tested ES displacement window has no remaining unclassified
established writer. Q-PARTY-134's graphics-field admission now names
the two setters' excluded incoming forms, button-tag/record or
fixed-data backing aliases, plus the explicitly excluded write and
construction mechanisms above. Repeating this unchanged census
does not settle those different mechanisms.

FND-PARTY-309's unchanged-shipped-field exclusion remains conditional.
No native callback, whole-client exclusion, queue closure or complete
reading is established. FMT-PARTY-007 remains supported; other
producer and interval obligations remain authoritative in the queue.

## Alternatives

- Every displacement-58 match overlaps a graphics callback: the
  seven established mask stores are only two bytes wide.
- A button constructor store is automatically an APFM store: its
  known caller selects the BUTN case; a different binding needs evidence.
- Fixup-backed data segment identity proves disjoint APFM backing:
  it only identifies the store address family; alias admission remains.

## How to reproduce

Run field_stores.py with the installed DSUN.EXE, its current inventory,
--es and displacements 57 58 59 5A 5B 5C 5D 5E 5F 60 61. Retain
established instructions and raw candidates separately. Run
direct_callers.py for 3EBE:0008, 3EBE:071F and 3F96:02F8; reuse
FND-EXE-588's constructor tag control and FND-PARTY-309's setters.
Use overlay_listing.py for descriptor 173 files 5B8F1..5B910 and
5C98C..5C99F, and descriptor 195 file 8207D..8209C, checking the
fixup and full final-store width.

Reports cast-apfm-overlap-writers, cast-apfm-writer-callers and
cast-apfm-fixed-writer173a/173b/195 remain only in
GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call ran.
