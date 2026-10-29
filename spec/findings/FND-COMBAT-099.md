---
id: FND-COMBAT-099
title: The script region-change route forwards three low words after a full-width sentinel test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1A43..172C:1B30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073890..0x000738AD
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at 3c878c68; Capstone 5.0.9
environment: null
---

## Observation

FND-UI-051 identifies instruction 0x5E's handler at 172C:1A43 and
its independent route through overlay 188 +09F0 to overlay 187 +1132.
This reading resolves the widths at that call boundary, rather than
extending the earlier direct-caller census or establishing script admission.

After requesting five parameters, the handler compares the full double
word at 4C13:00B4 with 32766. Equality takes the region-change branch.
There it pushes only the words at 4C13:00C0, 00BC and 00B8, in that
order, before calling 5702:00AC at 172C:1B16. FND-UI-051 maps that
trampoline to overlay 188 +09F0. The high words of these three operands
are not forwarded by this call. The fifth parameter is not pushed on
this branch.

Overlay 188 +09F0 reads its first word argument into DI and compares
that word with current DS:140C. Equality bypasses the call to overlay
187 +1132. Inequality forwards the same first word and the following
two word arguments to 56EF:0093 at +0A05. It performs no additional
numeric range test before this call. The inherited setup and later
continuations remain as described in FND-COMBAT-044.

## Interpretation

FND-COMBAT-044 establishes that +1132 copies its first word argument
to DS:140C and passes it to the numbered reader +0A6C, after its
preparation gates. Together with FND-COMBAT-035, this gives a bounded
condition: on this script route, reaching the reader's zero-selector
61-source branch requires the second script operand's low word to be
zero, the previous DS:140C to differ from zero, and the intervening
preparation to admit the read. A zero low word with DS:140C already
zero instead takes the earlier equality branch. Other admitted low
words select the reader's 17-source branch.

The sentinel comparison and argument narrowing are different operations.
A first operand with low word 32766 and a nonzero high word does not
satisfy this full-width equality. A hypothetical second operand with a
nonzero high word and zero low word still forwards selector zero.
These are conditional width consequences, not observations that such
operands occur in a shipped script or at runtime.

## Alternatives

- The parameter decoder's actual values and preservation, executed script
  identity, current region producers, preparation outcomes and other incoming
  transfers remain Q-COMBAT-055. This does not close the reader's admission.
- FND-UI-051's census of literal sentinel instructions does not establish
  the values of their other operands at execution, or exclude a variable
  first operand becoming the sentinel. No new exhaustive census is claimed.
- FND-COMBAT-044 retains the read-result and later setup dependencies.
  No callback, restored-health result or native transition is established.

## How to reproduce

Use the locked evidence Python and supported DSUN.EXE, size 634416,
XXH3 e296af55ba2ecde7e77f555c90f33d0b. Read resident_listing.py
172C:1A43..172C:1B30 and overlay_listing.py descriptor 188,
73890..738AD (hexadecimal, half-open). Follow the full-width sentinel
comparison separately from the three word pushes and the word equality
gate. Reuse FND-UI-051 for the trampoline and caller census,
FND-COMBAT-044 for +1132 and FND-COMBAT-035 for +0A6C's bounds.
No original code, bytes or decoded listings are committed.
