---
id: FND-PARTY-291
title: Transfer wrappers leave DI unchanged locally while the final handle wrapper restores its saved value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0002..44DE:0014
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0064..44DE:0086
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:022C..44DE:02A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:02F8..44DE:0321
tool: tools/research/exec-census/resident_listing.py at 992824ae; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-CONFIG-152, FND-COMBAT-124 and FND-PARTY-289/290
environment: null
---

## Observation

Q-PARTY-134 retains FND-PARTY-290's transfer-side DI contracts.
Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The four complete local
wrappers below have these distinct preservation mechanisms.

### Pointer normalization is a leaf

44DE:0002 saves BX, normalizes offset AX and segment DX with
word-width arithmetic, restores BX and near-returns. It has no
DI/EDI operand, child call, interrupt or stored-data write other than
its stack push. Its ordinary execution preserves DI intrinsically.
FND-CONFIG-152 already establishes the pointer transformation;
the register result does not add a buffer-validity claim.

### Read, write and seek leave an interrupt condition

44DE:0064 sets AH=42 and loads handle, displacement and origin
arguments before INT 21h at 0077. Its return branches update
AX/DX or error storage and restore BX/CX/BP; none writes or
restores DI. Thus the wrapper's own instructions preserve DI,
but its complete register contract depends on that interrupt.

44DE:022C has no own DI/EDI operand. It calls only the normalization
leaf before issuing AH=40 at 0264 on its chunk path and 0285
on its final word-count path. Its common return restores saved
CX/BX/DS and its frame, not DI. A zero count can skip both
interrupt paths; such an ordinary path preserves DI locally.
For paths reaching a write request, the interrupt's DI effect remains.

FND-CONFIG-152 and FND-PARTY-289/290 already read the parallel
44DE:01AF body. It likewise has no own DI operand, calls the
normalization leaf and issues AH=3F at 01E7 or 0208 when a
read request is reached. Its zero-count path does not require
an interrupt result. These no-request results require normal
distinct frame/argument storage; they do not assume a skipped
request proves a successful caller operation.

The remaining live-register interface question is specifically DI
across reached INT 21h services AH=3F, 40 and 42. Their own
wrapper code supplies no DI restoration. No ABI convention or
successful I/O return is substituted for that missing evidence.
Memory writes from reads or error fields remain separate alias
conditions even if the interrupt's live DI behavior is settled.

### The AH=68 wrapper has its own saved DI

44DE:02F8 saves DS, ES and DX, then DI at 02FE to SS:BP-8,
then SI. It issues INT 21h with AH=68 at 0305. Carry clear
sets AX zero; carry set stores the returned error and sets AX
FFFF. Both own branches converge at 0319, then restore SI,
DI at 031B, DX, ES, DS and BP before far return.

Consequently an interrupt's live DI change alone does not defeat
this wrapper's incoming-DI restoration. The remaining condition is
preservation of SS:BP-8, the stack/return state and normal continuation.
FND-COMBAT-124 already describes its request/result behavior;
this adds a specific saved-word binding, not a file-update outcome.

## Interpretation

The four original transfer-wrapper interface readings are complete
for this DI question. Q-PARTY-134 no longer needs the normalization
register question or an assumption that AH=68 preserves live DI.
It retains the named saved word and the AH=3F/40/42 interrupt
contracts, along with FND-PARTY-289/290's other saved-memory,
guard and alias conditions. The external behavior is not decided
by the original wrapper's instructions alone.

No whole getter site or queue item closes. Eighty direct sites
under Q-PARTY-124 and three under Q-PARTY-134 remain 83 total,
plus repeated-hook preservation and indirect/bypass inputs.
FMT-PARTY-007 stays supported; no complete reading is declared.

## Alternatives

- A wrapper with no DI assignment can still expose a reached
  interrupt's DI change; local absence and whole-call preservation
  are separate claims.
- A saved DI word can protect against live register changes while
  remaining vulnerable to a changed frame or overlapping memory write.
- The no-request path is not the same as a successful file transfer.

## How to reproduce

Run resident_listing.py on the fingerprinted DSUN.EXE with the four
half-open resident ranges in locations. The retained report is
GAME_DIR/analysis/class-table-closure-20261106/di-transfer-boundary.txt.
Reuse di-miss-services.txt for the complete read wrapper and
FND-CONFIG-152/FND-COMBAT-124 for existing request meanings.
Check the actual interrupt AH values, zero-count branches and saved
DI displacement rather than treating all transfer services alike.
No original game ran.
