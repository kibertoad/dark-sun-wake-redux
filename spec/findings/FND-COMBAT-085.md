---
id: FND-COMBAT-085
title: Load-list navigation searches differently after wrapping and can leave an empty selected row
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA85..0x0007DD38
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D300..0x0007D32A
tool: tools/research/exec-census/overlay_listing.py and segment_offset_refs.py at b024f0e6; Capstone 5.0.9
environment: null
---

## Observation

Overlay 192 +0785 initializes a local byte to one at entry. Its event-six
table sends input word 5000 to +07C6 and 4800 to +08E2
(FND-UI-038). The selected row is word 4C4C:0000; the record marker
tested is byte zero of DS:6278 plus the word-wrapped product of 125
and (4C4C:0002 + selected row). The mode is byte 50AF:0004.
The following paths assume valid record storage and no intervening
changes to mode, selection, base index, markers or relevant pointers.

At +07C6, Save mode or a nonempty current Load record requests control
state five for the old row. An empty current Load record skips that
request. The branch then compares the selected word with nine using a
signed comparison. A value below nine is incremented. In Load mode an
empty resulting record loops back to the comparison; a nonempty record
continues. A value at least nine instead stores zero, without another
search loop at that point. The later Load marker test exits to +08D7
when that row zero is empty. Otherwise the branch requests control state
four for the new row and calls +0DF4 with it before reaching +08D7.

At +08E2 the old-row control request has the same mode/marker gate.
The movement loop decrements when the signed selected word exceeds zero;
Load mode repeats the loop for empty resulting records. A value at most
zero stores nine. In Load mode, when row nine is empty and the local
byte is still one, it clears that byte and repeats the movement loop.
On the next empty row-nine wrap it leaves the byte zero and continues.
The later marker test skips the new-row control request and +0DF4 when
the result is empty. A nonempty Load result, or Save mode, makes those
requests. Both branches reach +08D7's 28C9:2A81(1), then the callback's
common return path. They do not restore the previous selected word.

For an initial selected row within zero through nine, stable Load mode
and ten empty markers, the first branch therefore finishes with row zero
and the second with row nine. Neither requests the new-row state or calls
+0DF4 on that empty-result exit. With initial row five and only row two
nonempty, the first branch still stops at empty row zero; the second
reaches row two. With only row seven nonempty, the second can find it
after its first wrap. These are static branch derivations, not native
keyboard or rendered-selection observations. Save mode takes only one
movement or wrap and does not search the markers.

The no-argument entry +0000 explicitly stores mode one; +0015 stores
zero. Both call shared setup +024B. FND-UI-036 and FND-EXE-621 already
identify these mode producers; FND-UI-051 supplies the setup and handler
registration census. The segment-field candidate search for 50AF:0004
returns sixteen windows: these two store windows and fourteen read/test
windows. The two stores are independently checked from their entries.
This candidate search is not proof of absent computed or aliased writes.

## Interpretation

The selection word is not guaranteed to identify a populated save merely
because a Load-mode navigation callback returned. The two directions
also differ when the initial row lies after a valid row and row zero is
empty. Their local selection behavior cannot be replaced by a symmetric
cyclic search for the next populated row. FND-COMBAT-081's action branch
later passes the selected word to the loader; event delivery and intervening
state changes still decide whether that action is admitted.

## Alternatives

FND-UI-038 leaves the empty-list retry result open; this reading resolves
the local loops under the stated storage and preservation conditions.
A symmetric wrap-and-search reading is contradicted by the first branch's
direct row-zero marker exit and the second branch's one-byte retry gate.
No original bug or native empty-list action is claimed. Control helpers
can have further effects; the selected word inherited from overlay 171
need not initially lie within zero through nine. Q-COMBAT-046 retains
those producers, lifetimes, event admission and restored handler routes.

The field search scans literal segment-load candidates and at most six
following instructions for an unindexed literal offset. It excludes LES
origins, pushed far pointers, computed offsets and more distant uses;
arbitrary-byte candidates are not an exhaustive instruction census.
The two entry stores supply independent positive controls for the form.

## How to reproduce

Read descriptor 192 ranges 7DA85 7DBE2 and 7DBE2 7DD38 with
overlay_listing.py on the licensed installed DSUN.EXE. Follow each
signed row test, mode reread, marker test, wrap and the single local-byte
clear from the callback entry. Compare the paired event table of
FND-UI-038. Derive the named cases with initial row five and the specified
stable markers, and separately with all ten markers empty. Read 7D300
7D32A for both mode entries. Run segment_offset_refs.py with 50AF 4,
retaining its candidate and addressing exclusions; do not infer an
all-writer proof from the sixteen returned windows.
