---
id: FND-SCRIPT-041
title: A preserved public-entry local reset bounds the variable radius for every counter value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:00A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:31ED..172C:325F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:207E..2D40:2196
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: Composition of FND-SCRIPT-007/020/026/038/039/040, FND-PARTY-098 and FND-CONFIG-144; retained indexed source at 6a464ec5
environment: null
---

## Observation

FND-SCRIPT-040 leaves the unmatched-counter path with its incoming local
word value. The public interpreter entry and existing local-reset reading
identify a conditional producer for that incoming value, avoiding an
unnecessary census of global counter writers for this class.

FND-SCRIPT-007 reads public entry 172C:000C. Status word 4C0E:000B other
than two returns without running a script. Status two calls local reset
31ED before clearing the stop/frame state and entering the requested script.
FND-SCRIPT-020 reads that reset's complete body: flag byte 4C0E:0000
equal to one requests zero fills of the four local buffers, including
64 bytes through the far pointer at 4C13:0337. Another flag value skips
the fills. FND-SCRIPT-026/010 bind local-number word zero to the first
two bytes of that same pointed-to buffer. FND-CONFIG-144 supplies the
fill helper's byte-counted zero-write contract.

The known second-entry runner 2D40:207E invokes public entry 000C,
not the nested-script entry 0299. Thus the recorded call route reaches
the public reset gate before entering callback 1D32. Reaching a gate is
distinct from proving its flag, pointer, capacity or successful execution.

### Reset-preserving class

Require status two, reset flag one, successful returning fills into valid
nonwrapping disjoint local buffers, and the same local-number buffer at
the callback's uses. Preserve the code/frame/compare and local contents
through loader and intervening handlers except for the direct assignments
in FND-SCRIPT-040. Registration additionally requires its recorded pointer
and parameter/index bindings. These are conditional prerequisites, not
new native observations or a general alias exclusion.

The incoming local word is then zero. For every possible captured counter
value C, the complete source cases and positive guard give:

| Counter class | Local value at the guard | Variable registration |
|---|---:|---|
| C=0 | 5 | Radius five |
| C=1 | 4 | Radius four |
| C=2 | 2 | Radius two |
| C=3 | 0 | Skipped |
| Every other C | Retained reset zero | Skipped |

In particular, the no-default case does not require a global counter
range proof once this reset and preservation are admitted. The setup's
literal radius six and its immediate fall-through remain as recorded in
FND-SCRIPT-040. Neither the literal nor variable input sets bit seven in
this class; the setter can still retain an old high bit.

## Interpretation

Q-SCRIPT-023's remaining uncertainty is whether an admitted route bypasses
or changes this effective reset/preservation: other entry representations,
flag/pointer/capacity state, local restore or alias writes, changed stream
and handler/callback effects. An out-of-range counter alone is insufficient
when the local starts at preserved zero. This removes the full counter
domain from the ordinary reset-preserving route's value proof, without
claiming that every native route meets its prerequisites.

Shared reset and interpreter-state admission remain Q-SCRIPT-003 and
Q-CONFIG-008; Q-SCRIPT-017 retains the record/caller lifetime requirements.
No queue closure, confidence/parity promotion or complete-reading claim
follows from this composition. No original function ran.

## Alternatives

A reset flag other than one skips the clear; a replaced/aliased buffer or
intervening restore can break the zero-value premise. A direct incoming
entry after the reset also needs its own evidence. Those mechanisms remain
open. Merely choosing a counter outside zero through three does not break
the premise or introduce a high radius on the reset-preserving route.

## How to reproduce

Reuse the complete entry/reset readings and exact source identities in
FND-SCRIPT-007/020/026 and the bounded GPL/223 source block in
FND-SCRIPT-040. Verify the public runner's 000C call in
GAME_DIR/analysis/walk-script-20261104/entry-runners.txt. Compare the reset's
64-byte pointer with the local-number accessor binding rather than relying
on a variable name alone. Follow all four case mismatch targets and the
positive guard with initial local zero, retaining the shared-state conditions.
The indexed source SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
No original code, listing or fixture is committed.
