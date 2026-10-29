---
id: FND-COMBAT-081
title: The numbered-load caller routes two event controls through load mode, ignores the loader result, and performs cleanup before its independent exit test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA85..0x0007DAC3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DD38..0x0007DD58
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DE21..0x0007DF7C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E014..0x0007E048
    kind: file-data
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py at 0949ba74; Capstone 5.0.9; bounded little-endian word-table inspection
environment: null
---

## Observation

Overlay 192 begins at file 0x7D300. The direct-call candidate search for
192+05ED returns one near-call candidate at file 0x7DEDF (+0BDF), preceded
by push CS. Reading the containing +0785 routine confirms that instruction
calls +05ED. This is a candidate census with a checked positive site, not
an exhaustive indirect-call or jump census.

At +0785 the first argument equal to 2 selects a 13-key word lookup using
the second argument. The keys occupy file 0x7E014..0x7E02E and corresponding
target words 0x7E02E..0x7E048. Keys 0x477D and 0x47E0 both select +0B21;
0x477E selects the separate +0C7C branch. An unmatched event-two key goes
to the common zero-result path. Event 6 uses a different four-key table;
other events take the local -1-result path.

The +0B21 branch calls interface helpers, copies the text control's current
content into the selected 125-byte list record's description, updates the
selected entry, changes control states and calls +03AC. It then reads
50AF:0004. Nonzero passes word 4C4C:0000 to +05ED at +0BDF; zero passes
that same word to the save routine +03FB at +0BEF. Both branches join at
+0BF2, which removes the stacked word. No test of AX or AL from either
operation precedes that join or the following cleanup.

It conditionally releases the list pointer DS:6278 through 444C:0092,
storing the returned pointer. The subsequent independent test of DS:627C
returns -1 after clearing DS:1462 when that byte is nonzero. Otherwise
it calls overlay 182 +19F8 and tests the full AX result of 28C9:2522
against 1. Unequal calls overlay 182 +1167(0). Equal conditionally calls
overlay 204 +284D in load mode, then 2C5F:018C with 4C13:0369 and 1.
The common continuation calls overlay 204 +1D2E(5), stores DS:143F = 1,
calls 2C5F:0139 and 28C9:2A81(1), and takes the common zero-result path.
The loader's result therefore does not select these continuation branches.

## Interpretation

For the checked direct caller, attempted loading is followed by cleanup
and independent global-state branches even if a lower-level load fails.
It does not propagate a load success flag through its own return. This
extends FND-SAVE-022's observation that the inner archive-copy result is
ignored, without assuming the global exit byte or child return domains.

## Alternatives

Q-COMBAT-044 retains initialization and writers of the mode, selected-slot
and exit fields, admitted event delivery, interface-child effects and later
continuations. The raw-byte direct search excludes indirect calls, jumps
and unsupported transfer forms; the table reading does not admit every
runtime control value. No complete-reading declaration or whole-load health
preservation claim follows. BUG-SAVE-001 separately records the stale exit
byte behavior.

## How to reproduce

Run direct_callers.py on the installed licensed DSUN.EXE for 192+05ED.
Read descriptor 192 with overlay_listing.py ranges 7DA85 7DAC3,
7DD38 7DD58 and 7DE21 7DF7C. Read the two adjacent arrays as 13
little-endian words each, pairing equal indexes; retain only the table's
bounded keys and targets. FND-SAVE-022 identifies the numbered-load body.