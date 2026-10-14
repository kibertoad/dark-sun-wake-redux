---
id: FND-PARTY-217
title: Inventory opens WIND 13500 with overlay 189 callbacks and projects attached records by signed position byte 13
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/overlay_listing.py, resident_listing.py, immediate_search.py, direct_callers.py and trampoline_target.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007BA12..0x0007BAE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074D3E..0x00074D4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00075DE2..0x00075E1B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076038..0x00076243
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00076487..0x0007650E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:01F3..27E5:0236
---

## Observation

Overlay 189 starts at shipped offset 749B0; overlay 190 at 78340. Resident
addresses use relocated load segment 1000. FND-PARTY-052 already maps the
combatant's three attached heads; FND-PARTY-218 describes the ten-byte graph
entries built by 2D40:2196.

### Window and current character

The literal operand query for 34BC and 34BD finds a push in overlay 190 at
7BAA4 and another in overlay 175 at 5FD42. From the inventoried entry
190 +36D2, the former is reached on the linear window-open path. It stores
its first argument at 4E71:0B44, uses it to select a 66-byte details record
through DS:19C5 and stores that far pointer at DS:1429, then selects a
49-byte combatant record through DS:19C9 and stores it at DS:142D. It
stores its second argument at DS:1A30, calls several external helpers,
then requests window number 13500 through overlay 182 +016B at +3767.

The passed handler is trampoline 5713:004D, which resolves to overlay 189
+0559. The loader passes this far handler argument to 3A8E:0CDC at its
+01BC. The window result is stored at 5072:003C and DS:1431. The opener
then passes the same handler to its local helper 190 +11A1 (+378B), and
passes trampoline 5713:00AC as a window callback through 3A8E:0D3C at
+37A2. That trampoline resolves to 189 +0000. The subsequent push of
5713:00B1 identifies 189 +001E, but its registration continuation is not
read here. The redraw body at +001E calls 189 +1432 at +039A with the
current word 4E71:0B44.

Thus the code supplies the opener/handler link previously absent from
FND-UI-030's resource interpretation. This is not complete window-lifetime
or input-dispatch coverage.

### Logical position projection

189 +1688 performs three FFFF REP word-store runs through the far pointer
DS:617D, starting at word indices 4, 36 and 42 with counts 26, 6 and 6.
With a clear direction flag they fill words 4..29, 36..41 and 42..47;
with a set direction flag they descend from those starts instead. This
body does not itself clear the direction flag before the stores.
It calls 2D40:2196 with its object argument and the graph-buffer far
pointer in DS:6189 plus four times the current word 4E71:0B44. It keeps
the returned entry count and iterates that graph from index zero.

For entries whose word +2 is kind 1, entry word +4 selects a 23-byte
record through DS:19C1. The body sign-extends that record's byte +13,
adds four and keeps the result as the logical map index.

- For index greater than zero and below 30, it calls 27E5:01F3 with the
  graph and entry index. A nonzero result writes FF to the record's
  byte +13 without writing that entry to the map on this branch. A zero
  result stores the graph entry index in the map word at twice the
  logical position. Later entries may overwrite an earlier map word.
- It then calls 189 +146B with that entry/count when either byte DS:6187
  is nonzero and the logical position equals DS:6183, or byte DS:6188 is
  nonzero and it equals DS:6185. The first matching branch takes priority.
- Other positions are ignored unless they lie from 42 through 47. With
  DS:6187 nonzero, DS:6181 nonzero and the position equal to DS:6183,
  this branch stores the graph entry index in the map and calls
  27E5:01F3. When the map entry selected by DS:6181 equals that helper's
  result, it invokes +146B first for the result and then for the current
  entry. Otherwise those calls are skipped.
- At the remaining 42..47 continuation, both DS:6187 and DS:6188 zero
  cause the record's byte +13 to be written FF. Other values leave it.

27E5:01F3 follows sign-extended parent bytes in graph entries while the
current index is positive and the type byte at DS:60ED plus entry word
+6 is 8. It returns the parent byte of the stopping entry, sign-extended.
It contains no explicit cycle bound. No termination or parent admission
claim follows from this local reading.

### Map consumption

189 +1432 calls +1688, then iterates logical positions 4 through 29. It
calls +1893 for each position and +1AD7 only for map words other than
FFFF. The +1AD7 entry is shipped offset 76487. It keeps the position in
DI, ignores an empty map word, uses the mapped graph index to read the
kind-1 entry's record number, then selects its 23-byte record through
DS:19C1. The first record field read here is word +0C. The image-loading
and final blit continuation is not covered by this reading.

The signed byte calculation has no 16-bit overflow: byte FF becomes -1
and therefore logical position 3. This is outside the main drawing loop's
4..29 positions; it is not an out-of-range negative map index. The source
does not justify calling FF an invalid memory address or assigning physical
equipment names to those logical slots.

## Interpretation

Attached-record byte +13 is an inventory-position code used to project
the object graph into logical display slots. The redraw reads the merged
graph and this position, rather than the three combatant list-head offsets
directly. This settles one additional field under Q-PARTY-020 but does not
identify the heads' separate roles under Q-PARTY-082.

The inventory resource and its handler now have direct code evidence:
190 +36D2 requests WIND 13500 with overlay 189 +0559. Which actions each
event branch performs and every screen state remain separate questions.

## Alternatives

- Graph construction, allocator capacity, current-character/object arguments,
  later pointer writers, direction-flag provenance and recursive +146B effects
  are not complete. Position producers and physical slot names are Q-PARTY-085.
- This reads consumers of +13, not every writer; it makes no universal claim
  about inventory membership, readying, bags, boxes or allowed placements.
- The window loader's external acquisition/drawing outcomes and all caller
  routes were not established. No native runtime observation is claimed.
- A near literal-table filter is not pointer provenance: the prior
  field_reads.py query with --es --table 19c9 and offsets 8, A, C includes
  instructions after another table is loaded. Those candidates alone do not
  establish a read of any combatant head.

## How to reproduce

With the hash-locked evidence Python and installed DSUN.EXE, run
immediate_search.py with 34BC and 34BD. Decode from the canonical opener
start using overlay_listing.py descriptor 190, range 7BA12..7BAE7;
descriptor 182, 689BB..68A3F; descriptor 189, 74D3E..74D4E,
75DE2..75E1B, 76038..76243 and 76487..7650E. Run resident_listing.py
with 27E5:01F3..27E5:0236. Resolve 5713:004D, 00AC and 00B1 with
trampoline_target.py. The query's other window candidate is not a second
inventory opener proven by this finding.
