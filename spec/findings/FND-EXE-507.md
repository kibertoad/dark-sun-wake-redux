---
id: FND-EXE-507
title: Game counted-byte dispatch selects direct copying or writes with overflow-sensitive buffer tests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:349D..1000:3603
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:349D..1000:3603
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3220..1000:323F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:3220..1000:323F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:32FA..1000:3313
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:32FA..1000:3313
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-500 records near 345B's frame, held original quantity and
flag-eight branch. With bit 0008 clear, both editions test record flags
word two for bit 0040. Set takes the branches below; clear takes
the later byte-loop or 3DCC branch. Record offset is held in DI,
quantity is SS:BP+6 and source offset is SS:BP+8.

With bit 0040 set and record word six nonzero, word six is compared
unsigned with incoming quantity. A smaller value first calls 2F13 if
record word zero is nonzero; nonzero returned AX takes the shared zero
return. Otherwise it sign-extends record byte four, doubles that word and
tests indexed flag 0800 at DS:37C4 installed or DS:3738 on disc.
Set calls 07B0 with that sign-extended handle, zero pair and mode two,
removes eight argument bytes and ignores the returned pair. It then calls
3EDA with freshly sign-extended record byte four, source offset and full
incoming quantity, removing six argument bytes. Equality between full AX
and current incoming quantity returns the held original quantity; mismatch
returns zero. With record word six zero, the same optional positioning
and direct 3EDA request occur without the preliminary flush.

When nonzero record word six is at least incoming quantity, it adds
record word zero and incoming quantity at word width and takes JL
directly from ADD's flags. A taken branch proceeds to copying. Otherwise
record word zero equal to zero is replaced by FFFF minus current word
six, then proceeds to copying; a nonzero word zero calls 2F13 and
requires returned AX zero. This JL uses SF different from OF, not
simply the wrapped sum's top bit. For example, 7FFF plus one yields
8000 with SF and OF both set, so does not take JL; 8000
plus FFFF yields 7FFF with OF set and SF clear, so does.

Copying passes current record word ten as destination, incoming source and
incoming quantity to 3220, removes six argument bytes and ignores its
AX. It reloads record word zero, adds current incoming quantity and stores
the result there, then adds current incoming quantity to record word ten.
It returns the held original quantity. There is no local destination extent
check, rollback or validation of the copy helper's result.

With bit 0040 clear and record word six nonzero, the helper tests
the old incoming quantity for zero while decrementing that word. Each
nonzero iteration increments record word zero and tests JGE from INC's
flags. When JGE is not taken, it retains and advances record word ten,
retains and advances the incoming source offset, copies one byte through
current DS and clears AH to return that byte word to the local test.
For JGE taken, it advances the incoming source offset, loads its old
byte into AL and passes the existing AX word and record offset to
32FA, removing four argument bytes. Only full returned AX FFFF takes
the zero return; every other result continues. Exhaustion returns the held
original quantity. INC's overflow matters: old count 7FFF becomes 8000
with SF equal to OF and takes JGE. Old count 8000 becomes
8001 with SF different from OF and takes the direct-byte branch.

The fallback's outgoing AH remains the high byte of the old incoming
quantity loaded into AX at the preceding loop test; the subsequent count
increment and source-byte load do not replace it. Its immediate callee
32FA reads only the low byte at SS:BP+6,
so that AH does not become the character argument to 3313. It saves
BP and SI, loads record offset at SS:BP+8, decrements record word
zero before calling 3313 with the sign-extended low byte and record
offset, removes four argument bytes, restores SI and BP and returns far
without incoming cleanup. It does not roll back that decrement after failure.
AX is the result of the call, without a local replacement.

With bit 0040 clear and record word six zero, 345B calls 3DCC
with sign-extended record byte four, incoming source and quantity, removes
six argument bytes and requires full returned AX equal to current quantity.
Mismatch returns zero; equality returns the held original quantity. Its shared
suffix restores DI and SI, resets SP to BP, restores BP and near-returns
with six-byte incoming cleanup. The common zero-return path also uses this
cleanup; it does not locally reverse record or source-offset updates.

The far copy helper 3220 saves BP, SI and DI, sets ES to current
DS and loads destination, source and quantity from SS:BP+6, +8 and
+10. It shifts the quantity right one, clears direction and copies that
many words forward from DS:SI to ES:DI. The shift's carry selects
one further byte when the original quantity was odd; intervening moves do
not change carry. It returns the original destination offset in AX, restores
DI, SI and BP and returns far without incoming cleanup. ES remains set
to DS. There is no overlap, allocation or termination check; source and
destination offsets advance at word width without segment adjustment. Forward
copy does not provide general overlapping-copy semantics.

Both editions have the same local branches and helper bodies, apart from
the indexed flag-table offsets above. Callee/native preservation, actual DS/SS,
record/table writers, source and destination extents, aliases and lifetime remain
unadmitted. These bodies contain no local interrupt or execution request.

## Interpretation

This completes the local branch enumeration left outside FND-EXE-500's
selected flag-eight reading, and resolves its additional copy and byte-fallback
helpers. Success-like quantity returns can follow memory copying or buffered
character processing, not necessarily native output. Q-EXE-007 retains state
and segment admission, callee/native effects, surrounding game callers and
whole-game launch-capability coverage. No complete output contract or whole-game
launch exclusion follows from this bounded local reading.

## Alternatives

Testing only the wrapped count sign loses ADD and INC overflow behavior.
Treating every fallback word as a newly sign-extended byte ignores its outgoing
AH, while treating that AH as character data ignores the immediate callee's
low-byte read. Treating failure as transactional ignores pre-call count and
offset updates. Treating a returned original quantity as delivered output ignores
the memory-copy and buffered branches. Treating 3220 as an overlap-safe
copy ignores its unconditional forward direction.

## How to reproduce

At revision 7f8eb24 require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode, header size 5200 and modeled
load segment 1000. Decode shipped 869D..8803, 8420..843F and
84FA..8513 at their corresponding 1000 addresses in both editions.
Follow flag 0040, unsigned word-six comparison, ADD/JL and INC/JGE
using both SF and OF, full-word call-result tests and exact argument cleanup.
Track every outgoing byte into 32FA and its low-byte consumer; verify
opcode 98 at 3307 as byte-to-word sign extension. Follow SHR's carry
through the forward word copy and optional byte. Keep actual segment and
storage admission separate from numerical offsets. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
