---
id: FND-COMBAT-107
title: GPL 119's request 9 has a conditional trigger-to-script-reset route before its local selector
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C0F9..0x0008C17C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D571..0x0008D63C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:03CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:01D1..1695:0218
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:00A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0F84..172C:0FC2
tool: overlay_listing.py, resident_listing.py and report.mjs table at c4af8aa6; locked evidence Python; retained script_decode.py index of FND-COMBAT-100
environment: null
---

## Observation

FND-COMBAT-105 identifies GPL/119's conditional request 9 at 0x0500,
before the common local-number selector at 0x0547. Its encoded arguments
are request 9, target -49, the result of nested instruction 0x1E with
operand -214, and zero. This reading connects one concrete native reentry
route; it does not establish that the shipped state takes it.

### Dispatch selects a group replacement path

The request dispatcher initializes its local search cursor to zero, takes
the target's word into SI, and indexes its 53-word table with request minus
one after the unsigned bounds test. Table row 9 is overlay 204 +0339;
row 49 is +01FB, independently matching FND-PARTY-156's field-setter control.
Row 37 is +0657, distinguishing the common request from request 9.

Request 9's positive-target branch calls +17B1 once. Its nonpositive branch
repeatedly calls 2D40:0D94 with the cursor and target, calling +17B1 for
each returned word other than 9999. It subsequently walks the table at
DS:67B7 and renumbers entries that still match the target, as already
recorded by FND-PARTY-135. Encoded target -49 selects the nonpositive
branch under parameter preservation. It does not establish a match.

The opcode-0x22 handler reads four expression parameters and passes two
words followed by two double words to the dispatcher. After its native
call returns, it zero-extends AX into the accumulator and returns. Its own
body has no frame-depth save/restore or explicit trigger-suppression store.
This does not exclude suppression or other effects in its children.

### The replacement helper supplies a trigger-enabling mode bit

The full +17B1 body rejects a negative slot. Otherwise it retains location,
orientation and state information, calls 31E0:05BB with the old record's
entry word and three ones, and calls 31E0:0121 to load the replacement.
Its own mode byte starts at seven; nonzero fourth request argument adds
bit eight. GPL/119's encoded fourth argument is zero, so the supplied
mode is seven under argument/local preservation. Either mode retains bit
four. FND-PARTY-135 supplies the previously recorded unload/load context.

The helper's trailing success store is conditional on the loader returning
a word other than -1: it stores that word in 4C13:0363. This is a further
reason not to presume that named words retain their incoming values through
the prefix. It is not a bound on the loader's actual result.

### A successful loader path can enter the script runner again

In 31E0:0121, capacity/reclamation and the lower load can reject before the
following path. After construction and table insertion, the caller's mode
bit four gates a state-byte test. State two at 4F49:0C33 plus three times
the returned slot admits the branch beginning at 31E0:0245. That branch
calls other services before reaching the trigger work; their effects and
preservation are not assumed away.

The function assigns the returned slot to 4C13:0367 at 31E0:02B8.
When the later word read at DS:0DAB is nonzero, it calls 1695:01D1 with
that named slot, its current cell coordinates and a category chosen from
zero, one or two. Category two is used for slots signed at least four;
the smaller-slot cases distinguish equality with 4C13:0369. There is no
explicit test of the trigger call's return value before the following
service call. Earlier reads of DS:0DAB are distinct checks, not proof of
the later value.

FND-SCRIPT-028 supplies 1695:01D1's cell-then-box walkers: while script
state 4C0E:000B is two, the first matching record runs through 172C:000C.
The record, coordinates, category and script number must actually be
admitted; a call of the walker alone does not prove a script ran.

At 172C:000C the state-two branch calls the local clear before resetting
the stop byte, script depth, frame depth and comparison/conditional depths,
then runs the selected script through 172C:00A1. Its own body does not
save and restore the old depths around that call. This agrees with
FND-SCRIPT-007 and FND-SCRIPT-026. Under FND-SCRIPT-020's clear-byte,
pointer, capacity and alias conditions, the local-number bank is cleared.
Unlike a different-script load alone, this run-entry clear does not first
require a mismatch of current script number and kind.

## Interpretation

A proof that GPL/119's local assignments survive must distinguish a native
helper returning from the original script actually resuming at 0x0547.
This chain can reset interpreter state as well as clear locals. Consequently,
it is not yet a counterexample in which zero is read by that later selector:
the later read itself may cease to be reached from the old frame.

Q-COMBAT-062 takes the independently answerable admission and continuation
of this request-9 trigger route. Q-COMBAT-060 retains the initial bank and
other intervening effects. Actual matches, rejected loads, child results,
record/register/segment preservation and incoming control remain open.
No native run, reachable zero-selector case or complete reading is claimed.

## Alternatives

- A returning request handler need not preserve the caller's interpreter
  frame; the conditional trigger chain reaches the run-entry resets.
- Treating this as an observed local-zero read skips both trigger admission
  and proof that the old script continuation is reached afterward.
- A state-two object and mode bit do not alone admit a script run. The
  later DS:0DAB test, script-system gate and an actual trigger match remain.
- Excluding this particular route would not preserve the other prefix
  calls, storage aliases or independently admitted script entries.

## How to reproduce

Reuse FND-COMBAT-100's executable/archive identities and
FND-COMBAT-104's retained decoded index with SHA-256
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Read GPL/119 0x04E4 through its local return at 0x0552, retaining all
conditional guards and the nested operand at 0x0508.

Use report.mjs table at file 0x8C487, count 53, stride two, one unsigned
two-byte field at offset zero. FND-PARTY-156's dispatcher is the count
control. Require rows 9/37/49 to be 0x0339/0x0657/0x01FB. Read the
overlay ranges in Locations using descriptor 204, retaining actual fixup
resolutions. Follow the parameter pushes, the zero cursor, group-loop
return test and the helper's mode-byte producer, rather than inferring
the function from its table position alone.

Read the four resident ranges in Locations using resident_listing.py
with actual MZ relocations. In the loader keep the earlier rejection and
mode/state guards, the distinct DS:0DAB reads, slot store and trigger call.
Use FND-SCRIPT-028 for the matching walkers, FND-SCRIPT-007 for run entry
and FND-SCRIPT-020/026 for conditional clearing and storage limits. Do not
model these children as side-effect-free or infer outer-frame restoration
from the native call stack returning.

Configs/reports and bounded request-dispatch-prologue, request-9-case,
request-9-target, request-9-loader, request-handler and reentry-controls listings remain in
GAME_DIR/analysis/local-bank-preservation-20261103/. The request-37 case
and zero-child readings there are preparation for another obligation;
their presence does not establish preservation by that request. No
original listing, decoded script or memory is committed.
