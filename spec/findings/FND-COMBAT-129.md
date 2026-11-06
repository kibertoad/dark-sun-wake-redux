---
id: FND-COMBAT-129
title: Load action recomputes the selected path after window closure and releases the save list only after loading returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D8ED..0x0007DA85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DE21..0x0007DF0F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D6AC..0x0007D6FB
tool: tools/research/exec-census/overlay_listing.py at 7c8db0b3; existing action and record mapping FND-UI-037, FND-SAVE-005/022 and FND-EXE-642
environment: null
---

## Observation

Q-COMBAT-063 retains the selected-save path passed to the copy routine.
FND-SAVE-005/022 already identifies the selected 125-byte record as
the source. This resolves the exact read times, arithmetic and own
list lifetime on the action branch, separately from UI event admission,
record contents and child effects. Offsets below are hexadecimal.

### Displayed description and load argument are separate reads

FND-UI-037 identifies overlay 192 +0B21 as the shared action branch.
Its description-transfer request forms the record address using current
word 4C4C:0002 plus current word 4C4C:0000, multiplies by 7D at word
width, adds current DS:6278's offset and then 50 at word width. It
passes that offset with segment DS:627A and length 2C to 4228:00A9.
This addresses the selected record's description, not its path.

After redraw/interface calls, the branch calls local +03AC. That helper
conditionally passes window 50AF:0000 and word one to overlay 182
+0128, then clears 50AF:0000 regardless of the result. It calls
overlay 190 +319E and 3D72:0D83 and handles two UI bytes. Its own body
has no save-list release, DS:6278 replacement or 4C4C row/top store;
the complete effects of window destruction and the other children
are not inferred from that absence.

Only after +03AC returns does the action branch reread mode byte
50AF:0004. Nonzero rereads selected row word 4C4C:0000 and passes it
to local +05ED. Zero instead passes the current selected row to the
save routine +03FB. There is no saved pre-close row/mode snapshot or
own range/nonnull-path check on this action-to-load continuation.
UI dispatch, disabled controls and event reachability remain separate;
this is not evidence that a player can load an empty or invalid row.

### The source path is formed after load preparation

+05ED saves its incoming row word in SI. After its DS:459B test it
shows the loading message and calls overlay 204 +284D, overlay 187
+2424 with current DS:140C, then +206E with the returned double word.
Only after those calls does it read current top word 4C4C:0002 and
current list pointer DS:6278/627A to form the copy caller's argument.

Let B:O be that far list pointer, T the current top word and S the
preserved SI word. The supplied path pointer is:

`B : ((O + low16(125 * low16(T + S))) mod 65536)`.

The segment is not adjusted for offset carry. This own calculation
has no row-count comparison, sign rejection, list-null test, record
extent check or path-terminator scan before calling overlay 187 +0EBE
at +0654. SI preservation and stable frame/segments across the earlier
children remain conditions on S. Actual path termination/capacity and
normalization remain Q-COMBAT-063 with FND-COMBAT-113/114/115.

FND-EXE-642 gives the ordinary ten-record allocation. If T+S is admitted
as an ordinary index 0..9, the path begins at one of offsets 0,125,...,
1125 from that allocation's base. That conditional bound does not prove
T+S is in range, that the base addition does not wrap, that any row is
populated or that the path terminates inside its first 80 bytes. In
particular, builder/input admission cannot be replaced by this formula.

### Own list release comes after the load

The action branch's own DS:6278 release is at +0BFB, after +05ED
or +03FB returns. It tests the current pointer for nonnull, passes it
to 444C:0092 and writes returned DX:AX back into DS:6278/627A.
FND-CONFIG-165 qualifies that wrapper's forced-zero normal return;
the store does not prove successful release.

+05ED's body calls +0EBE with the path pointer, then continues to
CHARSAVE/settings work without testing that call's result, as already
recorded in FND-SAVE-022. Its own body does not release DS:6278.
Thus the explicit action-branch list release is after the file-copy
call returns, rather than before the pathname is consumed. Child or
aliased releases are not excluded by that local ordering.

## Interpretation

The path's own producer and explicit list lifetime are now bounded for
this action route. The displayed description, selected-row argument and
eventual source pointer are formed at different times with intervening
children and fresh global reads. A displayed row is insufficient evidence
that the same admitted record reaches the copy routine.

This removes the selected-path arithmetic, read timing and own premature-
list-release question from unread work under Q-COMBAT-063. Actual row/top
values, buffer identity/lifetime, child preservation, valid terminated
contents, directory/character state and external copy outcomes remain.
No queue item closes or rule/parity status changes.

## Alternatives

- Window release and save-list release are different operations.
- The action branch rereads mode and selection after window closure.
- The load routine rereads top and base after preparation calls.
- A ten-record allocation does not validate the selected arithmetic index.
- The explicit later release does not exclude child or alias effects.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. overlay_listing.py verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 192 over all listed
half-open ranges. +0B21 is the established action target in FND-UI-037;
this finding does not decode tables as instructions or assign new event
targets. Track each row/top/base reread separately, word multiplication
and addition, the +0654 path push and the later +0BFB release. Compare
FND-SAVE-005/022 and FND-EXE-642 instead of rediscovering their records.
Retain restore-selected-path-lifetime.txt only under
GAME_DIR/analysis/hit-incoming-20261104/. No original path, save or listing
is committed, and no original program is executed.
