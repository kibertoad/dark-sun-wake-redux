---
id: FND-UI-068
title: The pointer hit search continues after APFM matches and reads region rectangles after a ten-byte header
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0EB8..3D72:1157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:1157..3D72:11F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0BF8..4237:0C42
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4400:0062..4400:007F
tool: tools/research/exec-census/resident_listing.py at d63747b7; Capstone 5.0.9
environment: null
---

## Observation

Resident 3D72:0EB8 accepts two point words and two near output offsets.
It accesses the outputs through DS:SI and DS:DI, clears the far-pointer
output and DS:A121, and initializes the word output to FFFF. Its window
loop uses signed comparisons against word DS:A103, starts at far
DS:A105 and follows each window's far link +EE. Before advancing past
a window, bit 0100 in its +9E word ends the search. Bit 0004 skips the
current window. Guard effects and validity of this graph remain conditions.

The search calls 4237:0BF8 for the point against the window's region
at +0C and against DS:9F06. That helper reads an unsigned count word
at region+0, then tests eight-byte rectangles at region+0A+8*i for
indices below the count. The rectangles do not immediately follow the
count word. Each point test is delegated to 4072:010E; the region helper
returns one on a child result equal to one, otherwise zero after exhaustion.
This reading establishes its offsets and gates, not the producers or
capacity of either region buffer.

If both region results are zero, the search skips the window. Otherwise
it stores the current window pointer in DS:A121, its local saved-window
slot and the far-pointer output before scanning children. Child i is
at window offset +105 plus the zero-extended byte +F2 plus 30*i, with
16-bit offset arithmetic. The loop compares i against unsigned word
+F3. It skips a null child pointer, flag 8000 at child-entry+1C or
entry tag ACCL. The BUTN mask gates at payload+58 test bits four and
two as FND-UI-006 records.

A MENU entry when the DS:9F06 region result equals one calls
3BA6:08BF with the child pointer and both output offsets. Returned AX
FFFF selects an FFFF return from this search. Other returns compare
the possibly changed outputs with the saved window pointer and the
initial FFFF word; a difference ends the search with AX zero. With no
difference the child loop continues. This does not establish the MENU
callee's own effects or the native presence of such a child.

The ordinary child path requires a nonzero window-region result. It
passes the child payload to 3D72:1157 for a local rectangle, offsets
that rectangle by window words +96 and +98 through 4400:0062, then
requires a nonzero point-test result. The rectangle helper reads the
payload tag for a three-case dispatch; this is a separate read from
the entry tag used by the search. An EBOX entry also requires bit two
at payload+96. Further calls test region overlap, form an intersection
and test the point in that region. Their nonzero results gate selection;
their detailed geometry and storage admission are not established here.

After those gates, 111A..1124 copies the entry's current far pointer
to the pointer output. The comparison at 1124 tests the entry tag
against APFM. The unequal branch at 112D goes to the zero-return
tail at 1151. Equality falls through to the index increment at 112F
and the next child-count test. Thus an APFM match continues scanning;
a non-APFM match exits. The ordinary selection path has no own write
of the selected child index through the word-output offset. A later
qualifying window can overwrite the pointer output with its own window
pointer before any child match. The outputs therefore cannot be treated
as an immutable first APFM hit and its child number.

The offset helper 4400:0062 adds its first word argument to rectangle
words +0 and +4 and its second to words +2 and +6, in word arithmetic,
then restores DS. This retains the screen-offset and runtime-field
observations used by FMT-UI-001 and FMT-UI-002. File bytes and shipped
child kinds are independently recorded in FND-UI-002 and FND-UI-003.

## Interpretation

The hit-search record is a live window/child-table result under region,
mask and ordering gates. An APFM directory tag alone neither terminates
the search nor freezes the pointer subsequently consumed by the caller.
FND-CONFIG-095 records the later assignment to A125; Q-COMBAT-051
retains the actual graph producers, payload identity, storage lifetime
and intervening writes needed to bind it to a shipped frame.

The supported loaded-window offsets remain +0C for the region object,
+96/+98 for translation, +9E for flags, +EE for the window link,
+F2 for the child-table offset adjustment and +F3 for the count.
No complete-reading or native dispatch claim follows from these local gates.

## Alternatives

This replaces FND-UI-011 in full. Its final child-loop condition was
reversed: APFM equality continues, while inequality exits. Its region
description also omitted the header space before the rectangles, and
its wording about both outputs did not distinguish the ordinary path
from the MENU callee. The unchanged offsets and helper operations are
retained above, rather than discarding that valid evidence.

Untraced graph mutations, aliases, stack-limit guard effects, geometry
helper results and MENU behavior remain conditions. The search can
leave a window pointer or a previously selected child in its output;
this finding does not classify the payload at every later read. It does
not show that a numbered-load state reaches any particular match.

## How to reproduce

Run resident_listing.py on the licensed installed DSUN.EXE with
3D72:0EB8..3D72:1157, 3D72:1157..3D72:11F8,
4237:0BF8..4237:0C42 and 4400:0062..4400:007F. The tool checks
the build length and XXH3-128 and applies MZ relocations at load
segment 1000. Decode continuously from each named entry. Trace
both outcomes of 112D to the child increment and return tail, the
window-pointer overwrite at 0F82 and every own store through SI/DI.
For the region helper, follow the count test and the addition of 0A
before the point-test call. Keep child-entry tags and payload tags
separate. No original process, input, screenshot or emulated call is used.
