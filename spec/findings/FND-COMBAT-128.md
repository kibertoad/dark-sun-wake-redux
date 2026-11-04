---
id: FND-COMBAT-128
title: Numbered loading can request region-handle closure after working-archive selection without clearing its reuse flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069FD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A0A6..0x0006A248
tool: tools/research/exec-census/overlay_listing.py at af348061; existing phase readings FND-COMBAT-079/080 and close contract FND-COMBAT-096
environment: null
---

## Observation

Q-COMBAT-064 needs working-record lifetime through the selector-zero
numbered load. FND-COMBAT-079/080 describe region setup and cleanup;
FND-COMBAT-110 distinguishes new opening from reuse. This composes
their explicit archive-close placement and field stores, without
assuming child preservation or an actual runtime handle identity.
Overlay offsets below are hexadecimal.

### There is an intervening explicit close site

After an admitted nonzero file-copy byte, overlay 187 +0EBE calls
overlay 182 +1856, then local +1BE7. The latter's returned AL is saved
at BP-1; neither value is a gate before the next region-preparation
call. The wrapper calls 182 +15D7 with (current DS:140C, 0, 1, 0),
then calls the numbered reader +0A6C with selector zero without testing
the region-preparation result. FND-COMBAT-043/084 supplies the later
reader-result and cleanup handling.

On this +15D7 call, its third byte argument is one. Current DS:0DAB
zero takes the own no-child branch: store the supplied region at
DS:140C and return AL one. It skips even 3D72:0B84 because that
call is conditional on a zero third argument.

Current DS:0DAB nonzero instead stores four there. It conditionally
requests release of DS:29EF, then tests current dword DS:145A.
Nonnull passes that numeric handle to 38FF:02B5 at overlay 182
+163B, file 69E8B. Regardless of the close result, or when the input
was null, the own continuation writes dword zero to DS:145A at +1643.
There is no AX/AL test between close and that clear, and no branch on
the close result before returning to the numbered-load wrapper.

Subsequent calls remove/install null callbacks and release panel or
other storage as mapped in FND-COMBAT-079/052 and FND-CONFIG-184.
Those children are not assumed read-only. The pre-read region branch
does not itself call the working-archive close helper +1C26 and has
no own write to DS:167A or DS:144E. Its concrete archive-close input
is current DS:145A, not necessarily the selected record's handle.

### The earlier cleanup does not make every case identical

182 +1856 returns without a child when its entry DS:0DAB is zero or
two. On its other branch it conditionally closes current DS:145A,
then clears that field without testing the close result. Later it
stores two to DS:0DAB before its final children (FND-COMBAT-080).

Consequently, with stated intervening field preservation, the local
cases differ:

| Cleanup entry state | Earlier own region-handle action | Pre-read +15D7 branch |
| --- | --- | --- |
| Zero | None; cleanup has no child. | State zero skips the later close too. |
| Two | None; cleanup has no child. | State two reaches the later current-handle test. |
| Other nonzero | Conditional close followed by DS:145A clear. | Preserved state two reaches the test, but a preserved cleared handle skips close. |

The last case requires preservation after the earlier clear, including
cleanup's remaining children and the working-open helper. The table is
a conditional composition of own stores, not a complete reachable-state
classification. Aliases, diagnostics, allocator effects and callbacks
can invalidate the stated preservation conditions.

### Handle identity determines the lifetime consequence

FND-CONFIG-067 separates numeric handles from record pointers.
FND-COMBAT-096 supplies close's resolver, preparation and unlink order.
If the later DS:145A handle resolves to the working record and close
reaches its admitted unlink, that record is removed before +0A6C,
while this branch has no own reuse-flag clear. If it resolves to a
different record, graph coherence and close's active-pointer condition
determine whether the selected working record stays selected. If close
rejects before unlink, its failure still does not suppress the outer
DS:145A clear or the later reader call on a returning continuation.

None of these conditional cases asserts that the two saved handles
are equal in a native load. Their current values, resolver membership,
graph identity and child preservation are precisely the remaining
admission questions. FND-COMBAT-127's word counter and lack of an own
uniqueness gate cannot establish or disprove a particular equality.

## Interpretation

A successful working open is followed by a concrete independent close
site before the numbered reader. The reuse flag alone does not certify
continued list membership or active selection at SAVE/24 and SAVE/26.
Nor does the later region-handle clear certify successful closure.

This removes pre-read close placement and its state-dependent own
gates from unread work in Q-COMBAT-064. Reuse Q-COMBAT-054 for shared
graph/producers, Q-COMBAT-063 for copied input admission and
Q-COMBAT-066 for reached child effects. Actual state, handle identity,
aliases and external outcomes remain; no queue closes or status changes.

## Alternatives

- Reuse-flag one is not an archive membership check.
- An earlier cleanup can skip its close on entry state two.
- Clearing a saved numeric handle is not evidence that close succeeded.
- An explicit intervening close need not target the working archive.
- An ignored region result does not establish successful preparation.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. overlay_listing.py verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 187 at 70CEE..70DE7
and descriptor 182 at 69E27..69FD3 and 6A0A6..6A248. Follow the
third-byte argument, initial state tests, DS:145A close/clear order
and absence of a close-result gate. Compare FND-COMBAT-079/080/096/110
and FND-CONFIG-041/067 rather than assuming disjoint saved handles.
Keep restore-pre-read-archive-lifetime.txt only under
GAME_DIR/analysis/hit-incoming-20261104/. No original listing or native
save is committed, executed or modified by this reading.
