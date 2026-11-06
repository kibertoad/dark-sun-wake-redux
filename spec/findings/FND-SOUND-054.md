---
id: FND-SOUND-054
title: File-sound start publishes preparation state before opening its persistent handle
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0177..4611:032A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0407..4611:043E
tool: resident_listing.py at 926ec8ac; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-007/008/051, FND-CONFIG-166 and FND-COMBAT-033/082/113
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-007/008 identify the file-name callers of 4611:0177.
The entry holds its near name offset in DI. It returns AX zero
unless configuration word +14 through DS:3411 has bit zero set
and configuration byte +3A is zero. It has no local null guard.

The admitted path calls 0262 with the name. FFFF returns zero.
Otherwise it passes that returned word and the name to 0211;
zero from 0211 returns zero. It then calls 0415 with the name
and mode one, stores AX at DS:349E and returns zero for FFFF.
After a successful handle result it writes DS:3434 one. It adds
127 to word 4E71:0B68 with word-width wrapping, clamps the
signed result to 0..127, passes that value with DS:3436 to
4842:0CA0 and returns AX one without testing the child result.
The 0B68 producer is bounded by FND-SOUND-051. Unlike the
resource default path, this path adds the fixed baseline 127.
There is no local request to 4842:0C88 in this body.

### Preliminary byte scan

0262 initializes its local word to zero and counter DI to zero.
It opens the name through 0415 with mode one. Open failure
returns FFFF. Otherwise each iteration requests one byte through
44DE:01AF into that local word and retains the double-word
DX:AX result. A result of FFFFFFFF exits; any other result
increments DI and compares the old DI with decimal thirty,
continuing while the old value is less. Under intact frames and
preserved DI this makes at most 31 requests. It does not require
each result to equal one. In particular, zero does not stop it.

It closes the temporary handle through 0407. The retained error
result returns FFFF; otherwise it returns the local word, whose
high byte began zero and whose low byte holds the last value
actually written. No file signature comparison occurs locally.
This is not proof that all 31 bytes arrived or that a VOC header
was validated. Child writes, local storage and file correspondence
remain prerequisites to identifying an actual returned byte.

### Preparation and separate size query

0211 writes its first argument to DS:A432 and zero to DS:A430,
then calls 02D8 with its second argument, the name. It publishes
that double-word result at DS:A438. FFFFFFFF or zero returns
zero, retaining those publications. Otherwise it clears DS:A43C
and invokes fixed-width copy helper 1000:0452 with CX twelve,
source DS:A430 and destination DS:A424, then returns one.
FND-COMBAT-033 supplies the fixed-width copy contract. Thus
preparation can publish fields before the later persistent open
fails; the outer body contains no rollback of these fields.

02D8 opens the name separately with mode one. Failure returns
FFFFFFFF as DX:AX. Otherwise it requests 44DE:0127 with the
handle, saves the returned double word, closes through 0407,
and returns the saved value. Reuse FND-COMBAT-113 for the
file-length request, retaining its actual OS/handle conditions.
The two preliminary opens and final persistent open are distinct;
this code does not prove the file stayed unchanged between them.

0415 forwards a near name as current DS:name plus the supplied
mode to 44DE:0086 and returns its AX handle or FFFF. 0407
forwards a handle to 44DE:003A. FND-CONFIG-166 already records
the stop body's full local release/clear ordering and the status
body's early zero gates; this finding does not rerecord those as
new evidence. No local rollback, audible result or successful close
is inferred from the calls alone.

## Interpretation

Q-SOUND-002 can remove the unread file-start/preparation contracts
and reuse the existing stop/early-status finding. The start result
one promises only that its local admission and handle checks passed
and the parameter-request path returned. The active refill path,
actual file and state histories, buffer bounds/lifetime, child and
interrupt effects, driver units and original output remain open.
No complete reading, whole question closure or status promotion is
claimed. The body alone does not prove a reachable contradiction
of the supported caller-level sound rules.

## Alternatives

- The preliminary scan proves a complete header: it tests only
  FFFFFFFF and the counter, allowing zero/short results.
- Preparation is transactional with persistent open: shared fields
  are written before the later open and are not locally rolled back.
- Returning one proves playback: it is assigned after the parameter
  request, whose result is not checked.

## How to reproduce

Read the two location ranges with resident_listing.py at the tool
revision above. Retain sound-file-start-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Track the old-value
counter comparison, temporary versus persistent handles, low-byte
read destination, outgoing copy arguments and DX:AX preservation.
Reuse FND-CONFIG-166 instead of redoing the stop/early-gate reading.
No game run, emulator session or emulated call supplied evidence.
