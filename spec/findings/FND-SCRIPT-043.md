---
id: FND-SCRIPT-043
title: GPL37 has conditional stop and menu-return routes into the rejected-atom postamble
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
tool: retained script_decode.py all-script and sweep reports from 211ea53c; RULE-SCRIPT-003/004 and RULE-TALK-001 control-flow interfaces reviewed at 7aac4cce
environment: null
---

## Observation

This connected control-flow reading narrows Q-SCRIPT-024 after
FND-SCRIPT-042 establishes GPL/37's rejected atom and conditional native
continuation. It reuses the unchanged GPLDATA.GFF (2191945 bytes,
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca) and OBJEX.GFF
(6816516 bytes, XXH3-128 e08d3cac153c547f6804ca27b155915d)
reports from FND-SOUND-077. The whole-file location names that retained
resource census scope; the local blocks discussed below are in GPL/37.

### Enclosing routine has a stop branch and a postamble branch

The decoded local routine beginning 0A41 first shows a portrait and
updates flags from field-path tests, then initializes local flag eleven
to one at 0A72. At 0A77 it evaluates kind-nine variable number 32
not equal to zero. The following conditional branch at 0A7D targets
0AFA when that expression is false (RULE-SCRIPT-003). The true branch
performs its field test and output instructions and reaches stop opcode
31 at 0AF9. That stop is before the malformed expression.

The false branch continues after the closing marker at 0AFA to the
global-flag-24 test at 0AFB. When that flag is nonzero, the test
`flag == 0` is false and the branch at 0B01 targets 0D29, skipping
the intervening dialogue/menu body. Under ordinary unchanged frame and
returning child conditions, this is a direct local route to the postamble;
it does not require any menu choice.

The postamble at 0D29 closes the preceding condition, calls the local
subroutine at 0D86 from 0D2A, then has two output instructions at 0D2D
and 0D32. The next instruction is the known opcode 77 at 0D37.
Subroutine 0D86 has output and a local-flag-eleven condition, sets local
flag one at 0DDD and returns locally at 0DE2. No decoded jump in this
postamble skips 0D37. Child effects, callback routing, stop/frame/cursor
changes and actual admission of this routine remain conditions.

### The menu is a returning call, not a jump past the gap

With global flag 24 zero, the body initializes local flag one to zero
at 0C17, then loops while that flag is zero. The loop-exit instruction
at 0C22 targets 0D29. Its seven-row menu at 0C25 supplies these local
targets in row order: 0DE3, 0DE3, 0DE3, 0D58, 0E42, 0E76 and 0EAF.
The first six conditions are local flags; the seventh is literal one.
Under intact menu inputs this admits at least that last response and
has at most seven offered rows, below the target-array overlap threshold
recorded by RULE-TALK-001. Actual menu input/selection and frame admission
are not proved by these constants.

RULE-TALK-001 and RULE-SCRIPT-003 establish that a chosen target runs
in a new local frame and a local return resumes after the menu. In this
resource the post-menu instruction is 0D08. The fourth response's target
0D58 clears local flag eleven, performs output and returns at 0D85.
Its address after the undecoded interval therefore does not by itself
bypass the enclosing routine's later postamble.

The unconditional seventh response, 0EAF, performs output, sets local
flag one at 0EBE and returns at 0EC3. With that flag retained, the
post-menu condition at 0D08 may also set it and global flag 24; the
loop-back at 0D26 targets 0C1C. The next loop test then exits to 0D29.
This supplies another concrete conditional route to the postamble and
0D37 after child returns, rather than a guarantee of native selection.

## Interpretation

The GPL/37 gap is not excluded merely by a later menu target or by the
existence of an earlier stop. The routine has distinct conditional paths:
a kind-nine-value-nonzero stop path and zero-value paths through a direct
flag-based skip or a returning menu/loop exit. Q-SCRIPT-024 can now focus
on actual routine entry, the relevant variable/flag values, menu and
child-return admission, frame/cursor preservation and FND-SCRIPT-042's
shared error continuation. These routes neither establish native execution
of the rejected atom nor decode the bytes the controlled decoder rejected.

## Alternatives

Treating menu target 0D58 as an unconditional jump over the bad instruction
ignores local return to 0D08 and the enclosing loop/postamble. Treating
the stop at 0AF9 as covering every route ignores the false conditional
branch to 0AFA. Treating a syntactically possible route as an observed
shipped-game bug would omit actual entry, state, callback and error effects.

## How to reproduce

Reuse music-health-all-scripts.txt and music-health-script-sweep.txt under
GAME_DIR/analysis/handler-census-20261106/, generated by committed
script_decode.py at 211ea53c with --all and --sweep. Verify the source
fingerprints above as in FND-SOUND-077. Read GPL/37 ranges
0A41..0B04, 0C17..0D37 and 0D58..0EC4 in the retained report.
Interpret transfer operands as decimal low-word offsets: 2810 is 0AFA,
3369 is 0D29, 3462 is 0D86, and menu targets 3555/3416/3650/3702/3759
are 0DE3/0D58/0E42/0E76/0EAF. RULE-SCRIPT-003 and RULE-TALK-001
supply the independently read branch/local-return semantics. Preserve the
GPL/37 decoder stop rather than replacing its rejected atom. No original
game or emulated call supplied the result.
