---
id: FND-ITEM-054
title: The inventory transfer wrapper subtracts four from logical position and partitions target list fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077247..0x000773B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076A4C..0x00076A5F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076D49..0x00076D52
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:288C..2D40:2B6A
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 380f3f29; composition with FND-ITEM-044/048/051 and FND-PARTY-221/238
environment: null
---

## Observation

Q-ITEM-003 retains the placement action/transfer contract; Q-ITEM-006
retains producers of attached position values six and nine. This reads the
complete inventory wrapper 189 +2897 and its accepted empty-target entry
route. It does not infer a native event or completed mutation from a successful
placement query.

### Target field and graph entry

+2897 takes logical position L, initializes target-entry index K to zero and
calls projection +1688 with current character 4E71:0B44. For admitted L in
4..17 it looks up the per-position category at 506F:0000 plus L-4. Category
five or twelve reads the proposed item's type through graph 5072:0040,
current DS:1A34 and the attached/shared tables. With shared type byte-zero
mask 04 hex clear it selects target field 17; with that bit set it selects
field 16. All other body categories select field 16 without that type read.
L at least 18 selects field four.

FND-EXE-616's category table makes the local body partition explicit: logical
positions 6,7,14 have category twelve/five/five. They select field 17 when
the proposed type's mask is clear, or field 16 when set. Other positions
4..17 select field 16. These are this wrapper's requests, not universal
names for the three character list heads.

For L at least 36, if either DS:6187 or DS:6188 is nonzero, the wrapper
replaces K with a map word. It rereads DS:6187: nonzero selects the map
position DS:6183, otherwise DS:6185. It loads that position's word through
DS:617D as K. Thus the first flag has priority when both remain set. Without
this branch K stays zero. The code adds no independent K/map-capacity or
container-kind check at this point.

### Source rebuild and exact transfer arguments

The wrapper now constructs the DS:1A32 source graph at 5072:0040. Starting
at zero it scans signed indices below the returned count for the first
kind-one entry. It still proceeds if the scan reaches the count without
finding one. Call +2992 requests 2D40:288C with these callee arguments:

| Frame field | Supplied value |
|---|---|
| BP+6, far pointer | Source graph 5072:0040 |
| BP+0A, word | Newly scanned first-kind-one index, or stopping index |
| BP+0C, word | Low word of L minus four |
| BP+0E, far pointer | Current character graph through DS:6189 plus four times 4E71:0B44 |
| BP+12, word | K selected above |
| BP+14, word | Field 4,16 or 17 selected above |

For admitted L in 4..47, requested position P is 0..43 without word wrap.
This wrapper never supplies the automatic-search value -2 in that class.

The proposed item used for field selection can precede the source rebuild:
field selection reads current DS:1A34, whereas the transfer uses a fresh
first-kind-one scan. This wrapper does not compare those two entry indices
or prove that both reads identify the same record. Stable source selection
and graph/state preservation remain necessary to describe them as one item.
A missing kind-one source is not locally rejected before the transfer call.

FND-PARTY-238 independently identifies +2992's field choices in the generic
field census. Entry-based decoding of 2D40:288C confirms that BP+14 supplies
its target field getter/setter requests; it is not a packed mode combined
with K. With a kind-two target root, canonical fields 16,17,4 address words
+08,+0A,+0C respectively (FND-PARTY-052). A container target can have a
different kind/descriptor; field four does not universally mean a character
record offset.

### Accepted empty-target route and position six/nine

FND-ITEM-051's +1F14 prefix calls the message-enabled placement gate at
+2005. Nonzero AL reaches +209C. If the subsequently reread map word L is
FFFF, +239C calls +2897 with L. An occupied target takes the other action
branches, whose complete exchange/use effects are not established here.
No stable empty-slot state is inferred across the intervening projection or
other children.

Identifiers 2BD6 and 2BD9 hex become logical L=10 and L=13 under the
FND-ITEM-051 transformation. They therefore request P=6 and P=9. The
category table at those positions is ten and one, so both choose field 16
and K zero, independently of the incoming type's shield bit. FND-PARTY-223
records the corresponding body-frame identifiers; actual event delivery,
item category/eligibility and current-character state remain conditions.

Under FND-ITEM-044's admitted target graph without a same-position collision,
the position helper can return these requested words unchanged. A continuing
generic transfer reaching 2D40:2B4E sign-extends that output and requests field
ten on its retained DI handle at +2B58. It then returns AL one without testing
setter status. With a kind-one target handle, canonical descriptors and
retained width one, FND-PARTY-221 binds that request to the attached record's
position byte +13 hex. This is a conditional producer route for six/nine,
not evidence that the native game reached those input/store conditions.

### Return and later effects

The wrapper tests the generic transfer's AL. Zero returns without its own
later clear of DS:1A34; it does not roll back partial callee effects. Nonzero
for a body position calls projection again, rereads its mapped record and
calls local +326A with character and record. With DS:1A30 equal to 9999 it
also calls 207 +0F79. All nonzero routes then clear DS:1A34 to FFFF. Those
children's complete effects and preservation remain open; clearing selection
or AL one is not independent proof that a particular field store succeeded.

## Interpretation

This removes the local logical-to-stored-position transformation and target
field/entry selection from Q-ITEM-003, and supplies explicit static UI
identifier routes for Q-ITEM-006's positions six/nine. Existing conditional
placement and final-byte-width findings no longer need an arbitrary request
word to connect those producers.

Actual dispatch, target/source selection across rebuilds, descriptor/width
retention, DS/SS near-storage identity, graph/map and frame admission, generic
splice/copy/child effects and post-transfer calls remain required. Q-ITEM-004
and Q-ITEM-013 retain their related storage/lifetime contracts. No native
parity, confidence or complete-reading promotion is claimed.

## Alternatives

Passing L directly would shift stored positions by four. Selecting a field
from the proposed type category instead of the fixed slot category changes
which branch the wrapper reads. Treating the old selected index and new
scan result as identical omits a local missing comparison. Treating a
successful placement query as a completed byte store omits subsequent
projection, transfer rejection and ignored setter status.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 189 range 77247..773B4
with overlay_listing.py. Reuse FND-ITEM-051's +1F14 prefix, then read
76A4C..76A5F and 76D49..76D52 for the empty-target continuation. Decode
2D40:288C..2B6A from its entry with resident_listing.py; verify target-field
pushes at 290F/2939 and final output/field-ten request at 2B4E..2B5D.
Do not treat a linear decode started at 2B00, inside a preceding instruction,
as instruction-boundary evidence.

Private placement-target-transfer.txt, placement-empty-target.txt,
placement-success-continuation.txt and placement-generic-transfer-full.txt
remain under GAME_DIR/analysis/item-removal-20261104/.
placement-generic-transfer-bindings.txt is the earlier partial-boundary
diagnostic; the full entry-based report replaces it for accepted boundaries.
FND-ITEM-055 subsequently records placement-list-transfer.txt's local exchange
sequence and conditional temporary-slot bootstrap; native admission remains. No original code,
script, resource or fixture is committed or executed.
