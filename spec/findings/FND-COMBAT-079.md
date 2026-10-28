---
id: FND-COMBAT-079
title: Numbered-load region setup uses zero flags to skip the object-rebuild block and optional region restore
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069FD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068BB5..0x00068DA3
tool: tools/research/exec-census/overlay_listing.py at 13adb0f3; Capstone 5.0.9
environment: null
---

## Observation

Overlay 182's code starts at file 0x68850. The numbered-load wrapper calls
+15D7 with (region, 0, 1, 0) before the saved-table reader and (region, 0,
0, 0) afterward (FND-SAVE-022).

+15D7 reads its third argument as a byte. When DS:0DAB is zero it stores
the region at DS:140C and returns 1, calling 3D72:0B84 first only when
that third argument is zero. With DS:0DAB nonzero and third argument
nonzero it stores 4 at DS:0DAB, performs its release and interface calls,
and returns without calling +0365. With a zero third argument it does
nothing further unless DS:0DAB equals 4.

In that state-4 continuation it calls +0365 with the region, its second
argument, zero, and its second argument again (file 0x69F0E..0x69F1D).
Both load call sites pass zero as that second argument, so +0365 receives
(region, 0, 0, 0). The next optional call, to overlay 187 +146C, is guarded
by +15D7's fourth byte argument; both numbered-load calls pass zero and
skip it. The continuation later sets DS:0DAB to 1 and copies 4C13:0369
to DS:426D. These are control-flow observations, not a complete reading
of all children of this continuation.

+0365 first calls +0297 and 362C:01FA and selects a region-specific
3796:0387 call, or 3796:03BF. At file 0x68C3E it tests its second byte
argument. Zero jumps directly to 0x68D9A, which clears DS:4459 and returns.
This skips the whole object-rebuild block, including overlay 200 +00E2,
the party-entry preservation and recreation loop through 31E0:0121, and
the later optional call at +0547. In the nonzero-second-argument block,
that latter call is separately guarded by the third byte argument.

## Interpretation

The numbered-load state-4 continuation does not execute +0365's object
rebuilding or +15D7's optional overlay 187 +146C call. Neither skipped
path can be the cause of rescaling restored creatures on these calls.
The earlier +15D7 call establishes the state-4 branch only when its
incoming DS:0DAB is nonzero.

## Alternatives

Q-COMBAT-044 retains the reached children before these flag tests,
other reader children and later continuations, failed or partial restore
paths, and restored combatant-table admission. No claim that all load
children preserve hit points follows. Q-COMBAT-045 separately retains
the temporary-slot admission question.

## How to reproduce

Run overlay_listing.py on the installed licensed DSUN.EXE with descriptor
182 and ranges 69E27 69FD3 and 68BB5 68DA3. Use FND-SAVE-022's two
numbered-load call sites to bind the four arguments; trace each byte test
at the width read and the explicit pushes at 0x69F0E..0x69F1D.