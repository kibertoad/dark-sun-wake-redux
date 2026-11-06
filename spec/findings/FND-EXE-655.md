---
id: FND-EXE-655
title: The runtime-error decode gaps cross inline metadata and a separately entered overlay-error routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02AD..1000:02C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000054C4..0x000054DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02DF..1000:02FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:0400
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:02A5..1000:02AD
tool: Pinned Python 3.12.10 and Capstone module 5.0.7 (distribution 5.0.9), resident_listing.py at c43c6a15; retained EXE-238/261/509/551/555/578/647, CONFIG-163/224 and PARTY-343
environment: null
---

## Observation

This qualifies Q-EXE-033's retained five gap sites and four contested
starts without changing the model that produced them. FND-EXE-647's
current report has no noReturn declarations. FND-EXE-578 and the
historical survey now retained by FND-PARTY-343 locate the overlap at the
runtime error path. FND-CONFIG-224 already supplies the conditional
exit/fallback contract; it is reused rather than reread as an unknown helper.

### Independent entries and inline span

The current installed inventory has one row at 1000:02AD, size eighty,
range 02AD..02FD. It has no separate row or current declaration for
1000:02DF. This is inventory ownership, not evidence that every byte
of the eighty is an instruction belonging to the first entry.

The established abort entry 02AD sets a thirty-byte diagnostic request
and joins 02B3. That join loads DS from the saved word at CS:02C4,
then calls 02A5, which requests an AH=40 write to handle two and returns
near. The caller then pushes status three and uses a push-CS/near-call
pair at 02C1 to enter far wrapper 03EE. It does not test the output result
before the exit request. FND-EXE-509/551 retain the saved-segment producer
and selected cleanup path.

The instruction after this call would be 02C4 if 03EE actually returned
there. Word 02C4..02C6 is instead the saved-segment storage that the
startup and abort path use. The next word at 02C6..02C8 is not assigned
a new meaning here. The span 02C8..02DF is twenty-three diagnostic bytes:
entry 02DF selects that exact start/count, loads DS from CS and requests
an AH=40 write to handle two. No diagnostic text or bytes are copied here.

02DF is independently established by the relocated far tail transfers
retained in FND-EXE-238/261. Its first instruction spans 02DF..02E2;
02E1 is inside that instruction on this stream. FND-PARTY-343 replaces
FND-PARTY-044's incorrect label for this routine while preserving that
survey's historical callback/reach observations and limits. The correction
does not claim that every possible alternate interior entry is excluded.

After the overlay diagnostic write, this second entry pushes status four,
selects relocated game-data DS and calls 03EE at 02F9 through another
push-CS/near-call pair. A RETF is encoded at 02FC. Its existence does not
prove an ordinary return: it lies after the same conditional exit request,
and the caller-supplied status word has not been explicitly discarded.
No valid cleanup-caller return is inferred from that byte alone.

The eighty bytes therefore have the following entry-aware partition:

| Span | Bytes | Established role |
| --- | ---: | --- |
| 02AD..02C4 | 23 | Abort path and shared diagnostic/exit join |
| 02C4..02DF | 27 | Saved segment word, two unclassified adjacent bytes and 23-byte diagnostic payload |
| 02DF..02FD | 30 | Independently entered overlay-error path and encoded trailing return |

The existing row conflates these roles. No inventory, declared-entry,
Ghidra database, code region or ownership-plan hash is changed by this
research record. Export repair needs its own supported tooling batch;
its instruction/data/body comparison must preserve every unaffected row.
An inventory repair alone would not change the reach model's assumed returns.

### What the gap cohort means

The retained report's gap offsets map to 1000:02E9, 02EB, 02EC, 02ED
and 02EE. Its contested starts map to 02F1, 02F2, 02F5 and 02F7.
On the independently established 02DF stream these bytes lie in the
handle-two load, write interrupt, status preparation and data-segment
reload before 03EE. The linear decode that continues through the payload
from 02C4 instead treats diagnostic bytes as operations and branches,
including a branch into this later stream. The overlap does not by itself
establish a second actual instruction stream reached by the game.

Conversely, calling those bytes payload is not an unconditional global
proof that no execution can ever reach them. The baseline assumes ordinary
returns from 03EE and its children and retains unresolved frame/target/
external effects. Its five gaps/four contested starts remain in the report;
none is deleted, relabeled harmless or used as a successful negative search.

### Exit continuation is conditional, not a return after DOS termination

03EE passes status, zero second argument and one third argument to near
0388. Under preserved arguments/frame bindings the third word bypasses
its optional callback-table/priority path. The two cleanup calls at
01F3 and 0176 remain. At 03BF the still-zero second word selects the
termination branch. FND-EXE-551/555 and FND-CONFIG-163/224 retain those
cleanup, native, target and frame conditions; balanced pushes are not a
proof that their fields remain unchanged.

019E requests DOS termination through AH=4C with the status byte. It has
no own return instruction. If the interrupt returns normally, its actual
successor prepares another diagnostic at 01A7 and jumps to 02B3, which
requests 03EE again. FND-CONFIG-224 already rules out equating that
interrupt return with return through the original diagnostic call under
the preserved frame/argument conditions. Nonreturning cleanup supplies
no such caller continuation either.

A changed second argument can select 0388's returning branch; changed
frames, nonlocal transfers and native/alias effects are not excluded.
This finding does not establish actual native process termination, finite
cleanup or those preservation conditions on every invocation. It supplies
no unconditional noReturn annotation for the whole wrapper.

## Interpretation

The cohort now has an explicit entry/data/exit explanation and a concrete
inventory repair: the separate 02DF entry and inline span are not represented
by the single eighty-byte row. Repeating a linear decode from 02AD would
reproduce the ambiguity rather than settle the actual entry boundary.

Q-EXE-033 retains the source/model and alternate-entry conditions plus
other header-access mechanisms. Q-EXE-037 retains reached target effects,
and Q-EXE-034 the general native/frame-return contract. The source exit
contract can support a later scoped model only with explicit conditions;
it is not permission to prune the unchanged baseline to obtain zero gaps.
No queue closure, complete reading or parity result changes.

## Alternatives

- The whole eighty-byte inventory row is one established instruction body.
  The saved word, counted diagnostic payload and independent 02DF transfer
  establish distinct roles within that range.
- 02E1 is the overlay-error entry. It is inside 02DF's first instruction
  on the independently established stream; the historical label is superseded.
- Returning DOS AH=4C admits the instruction following the original exit call.
  Its actual successor reenters the diagnostic/exit chain under the recorded
  frame/argument conditions, rather than returning from 019E.
- This proves unconditional noReturn or that every contested path is impossible.
  Changed mode/frame/native state and alternate entries remain outside that proof.
- Repairing inventory metadata necessarily removes all reach gaps.
  The reach query's own return assumptions and region model are separate inputs.

## How to reproduce

Require licensed installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Use the committed resident_listing.py
with the pinned interpreter for 1000:02AD..1000:030B,
1000:02DF..1000:02FD, 1000:0388..1000:03DF,
1000:03D1..1000:041A, 1000:019E..1000:01B0 and
1000:02A5..1000:02AD. Start the second stream at its independently
bound 02DF entry, not at the next instruction of the first linear decode.
Reuse the caller-relocation checks in FND-EXE-238/261 and the
exit/cleanup observations in FND-CONFIG-224 and FND-EXE-551/555.

Compare the retained header-pointer reach report's exact gap/contested
arrays with these entry-aware ranges and the installed inventory row.
Keep the original source fingerprint, fifty unresolved transfers, limits,
roots and assumed continuations. Do not alter the report or add an
unsupported nonreturning annotation while reproducing the correspondence.

Local listings are retained under GAME_DIR/analysis/ as
runtime-error-continuation-20261107.txt and
runtime-error-exit-worker-20261107.txt. No original-game run, emulated call
or fresh reach-model run was made for this reading.
