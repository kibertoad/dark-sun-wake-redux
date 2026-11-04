---
id: FND-SCRIPT-040
title: The preserved GPL two-twenty-three callback sequence supplies radii five four and two then skips re-registration
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: Retained script_decode.py indexed source of FND-COMBAT-100 and the controlled operand domain of FND-SCRIPT-039; reading at 5a495746 under RULE-SCRIPT-003/004
environment: null
---

## Observation

Q-SCRIPT-023 asks whether GPL/223's sole variable registration radius can
set bit seven. FND-SCRIPT-039 isolates the source site at 1E9C hex. Its
nearby zero assignment is only one case, rather than an unconditional
definition on every source path. This reading follows the complete bounded
callback block from 1D32 through its stop at 1EAA, plus its setup prefix.
The index SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Offsets below are in GPL/223, hexadecimal unless stated otherwise.

### Context and compare cases

Use the unchanged indexed source, current script/frame, continuation/stop
and compare-depth state, with valid disjoint local/global/parameter storage. The named local
word zero, global word twelve, compare snapshot and control/operand storage
remain intact across intervening handlers/callbacks except for the direct
assignments and increment identified below. Script parameter evaluation
and registration preserve those bindings; FND-SCRIPT-038's DS/stack and
record admission conditions remain independent prerequisites. These are
explicit conditional inputs, not new native preservation observations.

The callback begins at 1D32. After two context instructions and a portrait
instruction, compare instruction 17 at 1D3B snapshots global word twelve.
Call that signed word value C. Four case instructions compare the snapshot
with literals zero, one, two and three. Their mismatch targets lead to the
next case, or the compare end. Their local-word assignments are:

| C | Case test | Local word zero assignment | Assignment site |
|---|---|---:|---|
| 0 | 1D3E | 5 | 1D78 |
| 1 | 1D7D | 4 | 1DAB |
| 2 | 1DB0 | 2 | 1DFF |
| 3 | 1E04 | 0 | 1E83 |
| Any other value | All four mismatch | No assignment in these case bodies | None |

Each matched body has its own opcode-4F calls before its assignment; case
three additionally writes a global word's bit combination and clears flag
486. Under the stated preservation conditions these do not change the
selected local value after its assignment. The compare end is at 1E88.
There is no default assignment after the four cases. In particular, 1E83
does not execute when C differs from all four case values.

Two more opcode-4F calls precede the positive-value test at 1E93. With
the local values above preserved, the test admits the registration at
1E9C for C=0,1,2 and skips it for C=3. Registration uses callback offset
7474 decimal (1D32), script 223, object -72 and the local word as radius.
The following opcode 06 increments global word twelve at 1EA6 only on
the registration branch. The false branch targets 1EA9, skipping both
registration and increment. The block then ends the if and stops at 1EAA.

For a different C the local retains its incoming value L on these own
case paths. The subsequent test/register branch then depends on L and
its preservation, not on an implicit default zero. For example, a positive
L whose low byte has bit seven set is not excluded by the source compare
alone; this is an input requirement, not an admitted native example.

### Setup and repeated callback class

The source setup at 1D22 registers that same callback with literal radius
six. At 1D2C, after the registration returns, it assigns zero to global
word twelve. This order matters: the counter reset is not a prerequisite
already satisfied before the registration call. The next indexed instruction
after that reset is 1D32 itself. With ordinary continuation preserved, setup
therefore falls straight into the first body pass; it need not wait for a
later trigger invocation.

Start the following class only after that reset has completed, with each
subsequent callback reaching the compare and retaining the named state
through the increment and next callback's compare. No other writer changes
the counter between body passes. The resulting counter/radius transitions are:

- C=0 registers radius five, then increments the counter to one.
- C=1 registers radius four, then increments it to two.
- C=2 registers radius two, then increments it to three.
- C=3 assigns local zero, skips registration and increment, and leaves
  the counter at three on this own branch.

The fourth case does not itself issue an unregister operation. Skipping
this registration is distinct from clearing a previously stored entry.
External effects and later invocation of the retained callback are not
established by this source reading.

With setup fall-through preserved, the immediate first pass replaces six
with five and leaves the counter at one; later callback passes supply four
and two, then skip re-registration. All radius operands in this class,
including setup six, are below 128.
Under FND-SCRIPT-038's actual byte formula they cannot set the high bit
from the incoming radius. They still preserve an old high bit; other
writers or storage aliases are not excluded.

## Interpretation

This removes C=0..3's case/guard/value uncertainty and the reset-preserving
repeated callback class from Q-SCRIPT-023's remaining source-value work.
It does not prove that this class is the only admitted native one.
Remaining routes include a counter outside zero through three with a
retained local value, entry after or into a case/guard, independent counter
or local writers, changed code/operands and intervening state effects.
The returning setup call's effects precede its counter reset.

Native preservation and phase admission remain separate from the complete
source block's conditional transitions. No script/game execution, parity,
confidence or complete-reading promotion follows.

## Alternatives

Treating the nearest zero assignment as unconditional misses the first
three cases and the no-default path. Conversely, a graph that follows
every case mismatch independently of the captured compare value would
invent bypass histories for C=0..3. RULE-SCRIPT-003's compare relation
keeps those conditions paired. Neither source order nor the small-radius
sequence guarantees memory preservation across arbitrary callbacks.

## How to reproduce

Reuse GAME_DIR/analysis/reader-admission-20261029/scripts.txt with the
verified index hash above, regenerated as FND-COMBAT-100 describes from
GPLDATA.GFF length 2191945, XXH3-128
2bd1d2b989eef2dcb0545feb95975fca and OBJEX.GFF length 6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d.
Read only GPL/223's indexed 1D22..1EAA instructions and follow each
case's literal mismatch target, compare end, positive guard and increment.
Check the source setup target and local operand against the controlled
registration-radius-domain.json of FND-SCRIPT-039 under
GAME_DIR/analysis/walk-script-20261104/. Keep unknown child effects
explicit instead of executing the script or replacing them with no-ops.
No original listing or source-derived fixture is committed.
