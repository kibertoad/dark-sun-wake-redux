---
id: FND-SOUND-069
title: FM acquisition selects sequence tags but local load failure does not set the caller-tested flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:01C4..47E5:02B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:0500..47E5:05D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:00E2..4A32:0185
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:0310..4A32:0338
tool: resident_listing.py and window_operands.py at db6e5beb; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-013/068, FND-PARTY-268/269/321 and FND-CONFIG-012
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-013's non-disc song branch clears 4E71:0B56,
calls 47E5:01C4 and tests that flag for exactly one before
continuing to 4A32:0310 and volume application. This reading
supplies the connected acquisition and cleanup contract, reusing
the existing generic resource-interface findings.

### Tag selection and publication

01C4 holds its word song argument in SI, clears word 4E71:0B56
and, when byte 0B50 is nonzero, calls cleanup 4A32:00E2.
It then tests and repeatedly reloads configuration pointer DS:3411.
Under an unchanged word +32 the known choices are:

| Configuration word | Requested tag through 0552 |
|---|---|
| 0 | PSEQ |
| 1 or 2 | FSEQ |
| 3 | LSEQ |
| 4 | GSEQ |
| Other | No local tag request before final sequence setup |

The song word is sign-extended to a dword resource number.
The tests are successive, not one captured switch value; changes
across a child can admit more than one tag branch. FND-PARTY-268
already identifies these non-DATA request bindings. No new claim
about other parameterized callers or resource interpretation follows.

A zero result from a selected 0552 call requests 49DE:000F with
error word 17, selector 006A and third word 0B, then jumps to the
common return. Reuse FND-PARTY-321's distinction between that
selector and fatal 0069. Actual error-callback/frame effects remain
conditions; ordinary return alone does not imply an error flag update.

After the selected branches, 01C4 passes zero and the current far
pointer DS:344A to 47B9:0002, stores AX at DS:3440 and writes
byte 4E71:0B50 one. It does not test a success result or locally
normalize AX on all exits. Unknown configuration values can reach
this setup with no new 0552 request. Actual source/sequence-handle
correspondence, lifetime and setup effects remain open.

### Reused resource helper and distinct pointer fields

0552 clears 0B56, queries resource size through 38FF:05B5
into an SS-local dword and returns zero for a nonzero query result.
When 4E71:0C0F is one, it also returns zero if dword 0B5A is
less than the queried size unsigned. Otherwise it requests the whole
resource through 38FF:04AB with output-pointer address DS:3458.
Nonzero reader result returns zero; zero returns one. These local
failure exits do not set 0B56 one. FND-PARTY-268 supplies the
parameterized interface; its broader source and buffer obligations
are not replaced by an assumption of successful I/O.

Final sequence setup above uses DS:344A, whereas this resource
reader is given DS:3458. The local body contains no copy between
those fields. Their correspondence must come from actual state,
other children or bindings; equal purpose cannot be inferred from
both being far pointers.

### Cleanup and start-forwarding children

00E2 queries 4842:0D06 with DS:343E/DS:3440. Result one
requests 0CFA with those words. All ordinary continuations then
request 0CC4 and clear 4E71:0B50. If DS:3452 is nonzero,
it similarly queries DS:343E/DS:3442, conditionally requests
0CFA for result one, requests 0CC4 and clears 3452. A nonnull
DS:344E is then passed to 4AB9:0043 and replaced by returned
DX:AX. No local child-success test is performed. This does not
clear DS:3440 or establish release of a particular backing buffer.

0310 returns immediately for configuration word +8 equal to 71.
Otherwise it requests 4842:0CF4 with DS:343E/DS:3440 and copies
current 3440 to DS:3464 after return without inspecting success.
FND-CONFIG-012 records the subsequent volume-helper contract.
Actual sequence/driver and release effects remain Q-SOUND-004.

### Failure-flag census component

The controlled 0B55..0B59 operand query reports thirteen listed
operands: twelve accesses to 0B56 in five sound routines and one
adjacent immediate in overlay 189. The caller clear/read and loader
clear are independent positive controls. The only listed one-valued
store is 47E5:053B in the separate generic 0500 helper, which sets
one for a null local output pointer. FND-PARTY-268/269 already
retain its absence of known direct/nearby relocated incoming bindings
without treating it as unreachable.

The fifteen raw candidates remain unpromoted, including prefix
interiors and unrelated operands; computed/aliased/overlapping writes
and external changes are not excluded. Consequently no native flag
invariant is claimed. The bounded conclusion is that 01C4/0552's
own selected-load failure route does not assign the caller-tested one.

## Interpretation

Q-SOUND-004 can remove the unread local FM acquisition, prior cleanup
and start-forwarding contracts. Its remaining evidence names actual
tag/configuration, failure-flag producers, DS:3458 versus DS:344A
binding, sequence setup, driver/volume/release effects and output.
A load failure alone is not proof the caller's flag becomes one.
Q-SOUND-011 can reuse these concrete write targets and child interfaces
where they affect music-table lifetime, retaining actual aliases. No
complete reading, whole queue closure or native playback fault follows.

## Alternatives

- Every configuration value fetches a sequence: unrecognized values
  reach final setup without a local tag request.
- Resource-load failure necessarily sets the caller-tested flag:
  the known zero returns do not make that assignment.
- Acquisition and setup use the same pointer field: their encoded
  fields are 3458 and 344A, with no local copy between them.

## How to reproduce

Read the four location ranges with resident_listing.py at the tool
revision above as music-fm-acquisition-contract.txt. Run
window_operands.py on the approved executable and DSUN inventory
for 0B55..0B59 as music-load-failure-flag-window.txt. Retain both
under GAME_DIR/analysis/handler-census-20261106/. Reuse the existing
resource-interface, incoming-form exclusions, error-selector and volume
findings; preserve all raw candidates and actual state/alias conditions.
No original game, emulator session or emulated call supplied evidence.
