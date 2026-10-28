---
id: FND-COMBAT-080
title: The pre-load cleanup skips states zero and two, releases interface and region buffers, then stores state two
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A0A6..0x0006A248
tool: tools/research/exec-census/overlay_listing.py at caf95e29; Capstone 5.0.9
environment: null
---

## Observation

Overlay 182 starts at file 0x68850. Its +1856 helper returns at +19F6
without calling a child when DS:0DAB equals 0 or 2. Otherwise it:

- sets DS:0FCC to -1 if either pointer at 4E71:0A59 or 4E71:0A5D
  is nonnull;
- calls +059C with 0x4AA6 and 1 when DS:4454 is zero, otherwise
  3D72:0B84;
- conditionally releases each nonnull pointer at 4E71:0A59,
  4E71:0A5D and DS:9BE8 through 444C:0092, storing returned DX:AX
  back at that pointer;
- stores 1 in DS:143F, then calls 28C9:2ABD, 2C5F:0139,
  28C9:2A25, 2C5F:06CB(1, 1), overlay 190 +3169(1),
  28C9:2AAE and overlay 200 +046F in that order;
- conditionally releases DS:29EF through 444C:0092 and stores
  returned DX:AX there;
- closes a nonnull DS:145A through 38FF:02B5 and clears the pointer;
- calls +01EF with the window at 4E28:00D7, 0x4B01 and a null callback,
  calls 39D1:0448 with a null callback, clears DS:0D9C, calls
  overlay 190 +11A1 with a null callback, and calls 3A8E:0CDC
  with that window and a null callback;
- calls 28C9:24EA(0), +088B, stores 2 in DS:0DAB, calls
  overlay 206 +0B49, then 28C9:2A44(0), and returns.

Every conditional release above is tested separately in execution order;
there is no branch on the release or close return value. The helper
itself contains no table traversal of DS:19C5 or DS:19C9 and no explicit
hit-point store. This does not describe the complete effects of its children.

FND-SAVE-022 places this call before opening DARKRUN.GFF, the first
+15D7 call, and the saved-table reader. If this helper's active path
returns with its state store preserved by its last children, the next
+15D7 call takes its nonzero-state branch and writes 4 (FND-COMBAT-079).

## Interpretation

The cleanup's own control flow distinguishes already-clean states from
an active interface. Its state-two store precedes saved-record restoration.
The load wrapper does not need this helper to return a success flag.

## Alternatives

The effects and preservation of the reached children remain obligations
of Q-COMBAT-044, including the two calls after the state-two store. A local
absence of table writes is not proof that the entire call graph preserves
combatant or details data. FND-EXE-602 describes overlay 206 +0B49's two
conditional buffer releases; allocation and runtime effects still need their
existing bounds. No native observation or complete-reading status is claimed.

## How to reproduce

Use overlay_listing.py with the installed licensed DSUN.EXE, descriptor
182, file range 6A0A6 6A248. Follow both entry state tests to the common
return and each separate pointer test, preserving the call order around
the state store at file 0x6A233. Bind the caller order from FND-SAVE-022.