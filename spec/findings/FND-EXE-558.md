---
id: FND-EXE-558
title: Game caller registers a relocated cleanup pair and tests the full registration result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:0029..167B:003F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 167B:0029..167B:003F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:00F4..167B:0138
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 167B:00F4..167B:0138
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

At 167B:011A both game editions push a shipped segment immediate
067B, then offset 0029, and call far through the immediate pair
0000:030B at 0120. These are MZ relocation operands, not final
runtime segments. Relocation index 59 at 067B:011B changes the pushed
segment to modeled 167B; index 58 at 067B:0123 changes the
call segment to modeled 1000, with load segment 1000. Both editions
therefore supply FND-EXE-557's registration helper the pair 167B:0029.
Its SS:BP+6 low word comes from the second push, and its
SS:BP+8 high word from the first, under the intact far-call frame.
No instruction between these pushes and the call rewrites the outgoing words.

The caller removes four outgoing bytes at 0125, then tests the whole
AX word at 0128. Zero reaches 0135, which restores SI,
uses LEAVE to discard the local frame and returns far without incoming
cleanup. Nonzero pushes word 0257 and calls 4448:002C installed
or 443D:0032 on disc. Relocation index 57 at 067B:0132
supplies that call's segment from shipped 3448/343D. On return the
caller removes two outgoing bytes by popping CX, then takes the same
SI/frame/far-return cleanup. It does not test the latter callee's returned
AX. Its effects and return contract remain unread; reaching shared cleanup
after that call is conditional on its return and preservation.

The immediate predecessor at 00FB calls 1425:13FE with three
outgoing words: current SS, the offset formed as BP-019C, and
current DX, pushed in that order. It removes six bytes before testing
the returned AX word. Zero branches directly to registration at 011A.
Nonzero first manufactures a far return and calls local 000B, then
pushes the doubleword currently at SS:BP-4 and word 023C and
calls 4448:0002 installed or 443D:0008 on disc. It removes
six bytes and falls through to registration if that call returns. Relocation
index 60 at 067B:0115 supplies the latter segment. This is the
local route into registration, not admission of the earlier source/frame
contents or the three preceding callees' behavior.

The registered callback at 167B:0029 saves BP and sets its frame,
then calls far to 1425:1684. Relocation index 66 at 067B:002F
supplies the segment from shipped 0425. It does not test returned AX.
It next pushes current DS, then word 4022 installed or 3F96
on disc, and calls far to 44DE:02A9 installed or 44D3:02AF
on disc. Relocation index 65 at 067B:0038 supplies the segment
from shipped 34DE/34D3. The outgoing segment word is the current
DS after the first call; the callback does not save or reload DS
between calls. It removes four bytes, restores BP and returns far without
incoming cleanup. The two callees, the pointed-to storage, actual segments and
preservation remain unadmitted. FND-EXE-555's general cleanup consumer does not
test this callback's result before retesting its current count.

## Interpretation

This supplies a concrete registration caller, its relocated pointer inputs and
word-width result consumption, plus the registered callback's ordered outgoing
calls. Q-EXE-007 retains all other callers and count/table writers,
aliases and storage admission, the preceding helper contracts, both callback
callees and the nonzero-result callee. No complete caller census, callback
effects, registration invariant or game launch exclusion is claimed.

## Alternatives

Treating the pushed 067B or called segment zero as final runtime segments
ignores the relocation entries. Reading the registration result as only AL
contradicts the word test. Treating nonzero registration as locally undoing an
earlier operation ignores the absence of rollback on that branch. Giving the
callback's second callee its incoming DS ignores the preceding call and lack
of local DS restoration. A physical immediate-call hit alone would not prove
the connected sequence; the outgoing pushes, relocation operands and return
consumption are read here.

## How to reproduce

At revision 75cd998f require both DSUN.EXE identities from FND-EXE-350.
With MZ header size 5200, relative source segment 067B and modeled
load segment 1000, decode both ranges in Locations in sixteen-bit mode.
Their source base is file B9B0 in both editions. Read header relocation
count at 0006 and table offset at 0018; check indices 57,
58, 59, 60, 65 and 66 and the operands/values above.
Follow the zero/nonzero predecessor and registration return branches, outgoing
word/doubleword widths, each cleanup and the callback's current-DS push after
its first call. Use FND-EXE-557 for registration and FND-EXE-555 for
its consuming dispatch. No negative search or complete caller/writer census is
claimed. Licensed bytes stay outside Git; no game process, DOSBox or emulated
call runs.
