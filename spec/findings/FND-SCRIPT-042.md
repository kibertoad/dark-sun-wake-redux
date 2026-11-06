---
id: FND-SCRIPT-042
title: Rejected expression atoms retain an uninitialized scratch value and returning error calls do not locally suppress opcode77 mutation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3278..172C:332C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:33A8..172C:344F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3523..172C:3585
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:367B..172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1DCF..172C:1DF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0000FA63..0x0000FB29
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00202969..0x0020296B
tool: resident_listing.py and bounded table reporter at d5ded4d5; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SCRIPT-010/023/024 interface reuse
environment: null
---

## Observation

This connected reading follows the known GPL/37 rejected atom in
FND-SCRIPT-024. DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. GPLDATA.GFF is 2191945 bytes,
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca. No game execution is used.

The existing decoder stops while reading the first parameter of opcode
77 at script offset 0D37 because its atom at 0D38 is B3. The native
expression reader at 172C:3278 does not accept B3 as a field path.
Its 99-word table at 35A3 maps B3 to the error call at 3416. B2 has
the same target; B1 instead reaches 33A8, which calls the field-path
reader 284D. Controls 80 and E2 select the accumulator and opening-
parenthesis paths already read by FND-SCRIPT-010. The preceding index
range check and normalization retain B3 as this distinct rejected atom.
No byte substitution is justified by that table.

At entry, the reader clears 32 bytes beginning SS:BP-3A for values
and 16 bytes beginning SS:BP-1A for pending operators. Neither fill
covers the four-byte scratch value at SS:BP-6. The reader does not
initialize that scratch at entry or at the start of each iteration.
Recognized value branches assign it, but the default error branch
3416 calls 5702:00B1 then falls through to 341B without such an
assignment. Under an intact returning continuation its value is thus
retained from an earlier assignment in that call, or unspecified stack
contents if no branch has assigned it. It is not an implicit zero.

With the ordinary initial level and pending operator, the common join
can copy this scratch value into the expression's result slot. The
reader's returned result can therefore remain undetermined for a first
rejected atom even if the error call returns. Changed locals, segments,
cursor state or nonreturning error children are separate conditions.
FND-SCRIPT-023 records the error entry's eventual stop-byte publication
only after its helper and optional message return; this finding does not
assume those returns or their other effects.

The recognized string branch explicitly assigns scratch zero, as does a
nonoverflowing opening-parenthesis branch; their previous zero semantics
must remain explicit when removing the artificial per-iteration initializer.
Parenthesis overflow and underflow take a different error route: 33EA
calls the error entry and, on return, jumps directly to the expression
return at 3577. They do not fall through the rejected-atom common join.
The rule now keeps these distinct returning continuations as well.

The common join, parameter reader and opcode-77 body do not themselves
test that stop byte before their next local operations. Parameter reader
367B loops over the requested count, stores each expression value and
its far operand address, and proceeds to the next index. Opcode 77 at
1DCF requests two parameters and then subtracts the low byte of the
second from the byte at the first parameter's stored address. Its local
body contains no stop test between the parameter call and that store.
Thus error reporting is not a local early-return guard for this handler.
Whether the native path reaches those operations remains conditional
on the called error/helper, fetch, state and frame continuations.

The expression reader initially points its operand-address field at
4C13:031B, the accumulator. The rejected atom does not itself replace
that address with a field-path address. If that field survives the error
continuation unchanged, the first parameter address remains the default
accumulator address. This is not a claim about an observed write or the
actual returned parameter values in the shipped game.

## Interpretation

RULE-SCRIPT-004's former per-iteration zero scratch initialization was
not supported by the original reader on rejected atoms. Its description
must retain the unspecified/previous scratch state and conditional error
continuation. The accepted-atom layouts and the decoder's rejection of
GPL/37 remain intact. Q-SCRIPT-024 owns the remaining reachability and
error/handler continuation question; shared error-entry effects remain
Q-SCRIPT-003. The script census cannot treat the gap as decoded ordinary
instructions or assume that an error call instantly aborts the current
handler. No complete reading or native bug outcome is established.

## Alternatives

Treating B3 as B1 is contradicted by the distinct native table targets.
Treating an invalid first atom as zero is contradicted by the untouched
scratch slot. Treating the stop setter as a guaranteed immediate abort
omits both its preceding dependencies and the unchecked caller operations.
Reaching GPL/37 0D37, a changed cursor, an error child that does not return,
and effects of the unknown initial scratch contents remain distinct cases.

## How to reproduce

Use tools/research/exec-census/resident_listing.py at d5ded4d5 for
172C:3278..3339, 33A8..3455, 367B..36CE and 1DCF..1DF7,
retaining gpl37-expression-error-continuation.txt under
GAME_DIR/analysis/handler-census-20261106/. Ranges in the command share
segment 172C. Read 3523..3585 and 2805..281B separately as
gpl37-expression-return-and-fetch.txt. The shared accepted-reader and
error-entry bodies are FND-SCRIPT-010 and FND-SCRIPT-023.

Run tools/evidence/report.mjs table with gpl37-expression-dispatch-config.json:
source DSUN.EXE and fingerprint above, start FA63, count 99, stride two,
unsigned width-two target at offset zero. Count evidence is the native
80..E2 dispatch bound; FND-SCRIPT-010 independently identifies the table.
Output gpl37-expression-dispatch-report.json retains all rows and the
80/B1/E2 controls. Reuse FND-SCRIPT-024's source location and the retained
all-script report for GPL/37; no source bytes were changed.
