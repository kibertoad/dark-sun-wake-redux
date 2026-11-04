---
id: FND-SAVE-026
title: Shared character-list top indexes a ten-record save list without establishing valid later save pages
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058230..0x00058336
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000585D5..0x00058612
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058615..0x00058AF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D54B..0x0007D6AC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E058..0x0007E160
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000416C0..0x000416C8
tool: tools/research/exec-census/segment_offset_refs.py, segment_references.py and overlay_listing.py at b41b9e0c; existing builder FND-EXE-621 and path lifetime FND-COMBAT-129
environment: null
---

## Observation

This replaces FND-SAVE-024's unconditional writer/page-display reading
with bounded producer and consumer observations. Q-COMBAT-046 retains
shared UI-state/event admission; Q-COMBAT-063 retains the selected load
input and storage. Offsets below are hexadecimal; record counts and
the clamp example are decimal.

### Loaded state and literal producer candidates

Descriptor 95's segment 4C4C has eight loaded bytes at file
416C0..416C8, all zero. Word +0 is the selected row, word +2 the
shared list top, and +4 a window pointer in the stored-character list.
Loaded zero does not establish the later top value.

Repeating segment_offset_refs.py for 4C4C, offset 2 retains the six
previously identified store windows, all in overlay 171:

| Segment-load site | Top-word store site | Own source |
| --- | --- | --- |
| +00F3 / file 58303 | +00FA | Zero in +0020's reset branch. |
| +04FF / file 5870F | +0508 | Current DS:044C after the Down-page clamp. |
| +0540 / file 58750 | +0545 | Zero in the navigation reset branch. |
| +063F / file 5884F | +0648 | Current DS:044C after subtraction/clamp. |
| +06A9 / file 588B9 | +06B2 | Current DS:044C after Page-Up subtraction/clamp. |
| +06F4 / file 58904 | +06FD | Current DS:044C after Page-Down addition/clamp. |

The search decodes at most six instructions after a raw segment-immediate
candidate. segment_references.py independently retains declared MZ/FBOV
segment-word candidates, with diagnostic decoding around them. These
searches exclude computed/LES origins, more distant uses and overlapping
or bulk writes. They are not a proof that only overlay 171 can write the
word. The actual stores above are checked in the known +0020 and +03C5
bodies; no arbitrary candidate start is promoted to a routine entry.

The Page-Down branch at +06CD adds ten to word DS:044C, compares it
signed with word DS:40C6 minus ten and, when too large, assigns that
limit and clamps a negative limit to zero. It then writes DS:044C to
the shared top at +06FD before redraw and +0020 calls. With count 39,
old DS:044C 20 and preserved ordinary arithmetic inputs, this own store
writes 29, not 30. The Down-at-last-row branch has corresponding
addition/clamping before +0508. Subtraction paths clamp negatives to
zero. Hence page steps do not imply that every stored top is a multiple
of ten. Later children can preserve or replace the value; this is not
an observed native page transition.

+0020's scan can reset top and selected row when no eligible stored
entry is found; it does not unconditionally initialize both for every
screen opening. FND-PARTY-012 supplies the stored-character record
context. Its count/input and child preservation remain admission
conditions, rather than a guaranteed live count or page domain.

### Save/Load requests ten records independently of top

Overlay 192 +024B calls 444C:0008 with factors ten and 125 and stores
the returned pointer in DS:6278/627A. It diagnoses null but has an
encoded returning continuation. The requested array extent is 1,250
bytes, independently of the current shared top. Allocation success,
actual capacity and a returning diagnostic are not inferred here.

It requests the selected row's control state, calls builder +00F2 with
count ten, then calls +0D58 for each row 0..9. Finally it calls +0DF4
with the current selected row. Its own body neither resets shared top
nor validates selected row against the array extent. FND-EXE-621 and
FND-COMBAT-115 describe builder clearing and filename-derived writes;
the filename parser's missing upper gate does not allocate extra rows.

+0D58 forms its description pointer from current list base plus the
word-wrapped product 125*(current top + row), then adds 80 at word
width. In Load mode it separately reads byte zero of that indexed
record to select a control-state request. +0DF4 uses the same record
calculation for its name-box text. Neither body validates top+row
against ten. FND-COMBAT-129 supplies the later load-path calculation,
fresh reads and list-release order.

For a retained ordinary ten-record allocation, stable top ten, rows
0..9 and nonwrapping address arithmetic, these consumers index records
10..19, beyond the requested array's records 0..9. The first such
description starts 1,330 bytes from the base. This does not establish
physical inaccessibility, an actual native read failure, a crash or
the bytes displayed; allocator padding, aliases and child effects are
separate questions. It also does not establish that valid saves 11..20
are displayed. Numeric index formation is not resource or text provenance.

## Interpretation

The shared character-list producer is a concrete source of a potentially
nonzero Save/Load index. But an admitted inherited value and its lifetime
must be shown, and a ten-record allocation cannot be described as an
established pageable saved-game list merely because a top word is added.
The loaded initial zero and the known writer windows supply no all-state
zero invariant or complete writer census.

Q-COMBAT-046 retains state/event producers and preservation;
Q-COMBAT-063 retains actual load path/value and storage admission;
Q-UI-005 retains native row paint and selection. No native later-page
behavior is established, no question closes and no evidence promotion
follows from this correction.

## Alternatives

FND-SAVE-024 correctly located the shared storage and six literal store
windows, but overstated them as an all-writer proof and as demonstrated
later saved-game pages. Those interpretations are replaced here. The
fixed page-value set derived from ten-step navigation also omits the
count-minus-ten clamp; the conditional count-39 store is 29. Its older
dialogue/DS:044C observation remains a producer lead, not proof of a
particular later top. The old record is retained as superseded.

One possible execution preserves top zero and admitted ten-record
storage; another preserves a nonzero producer value into the consumers.
The latter's requested index is explicit, while actual source bytes,
displayed content and reachability require the named remaining evidence.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The scripts verify length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run segment_offset_refs.py with
4C4C 2 and segment_references.py with 4C4C. Read descriptor 171 over
58230..58336, 585D5..58612 and 58615..58AF4, and descriptor 192 over 7D54B..7D6AC
and 7E058..7E160. Retain candidate exclusions and compare each known
store against its entry-based listing. Track the signed clamp, ten-by-125
allocation request, ten builder argument and top-relative consumers.
Read the eight file-data bytes at 416C0..416C8 as data, not instructions;
their unsigned sum is zero. No transformed original bytes are committed.

Reports remain only under GAME_DIR/analysis/hit-incoming-20261104/:
save-shared-page-offsets.txt, save-shared-page-segments.txt,
save-shared-page-producers.txt and save-shared-page-consumers.txt.
No original game, invalid-row experiment or emulated call is executed.
