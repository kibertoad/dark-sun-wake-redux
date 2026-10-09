---
id: FND-EXE-503
title: Game diagnostic flush updates record state before writes and ignores per-record results in its aggregate path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2F13..1000:2FCE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2F13..1000:2FCE
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-500's far helper 2F13 saves BP, SI and DI and holds its
incoming record offset from SS:BP+6 in DI. Zero calls local far-returning
2F94 without arguments, then discards that helper's AX and returns zero.
Nonzero first compares record word fourteen with the record offset itself.
Mismatch returns FFFF without the later record-state stores.

With that equality, it tests record word zero signed. A nonnegative value
tests flags word two for bit 0008. When that bit is clear and record
word ten differs from record offset plus five at word width, it returns
zero without clearing word zero. Otherwise it clears word zero, compares
current word ten with record offset plus five, and replaces word ten with
word eight only on equality. It then returns zero. These are word-offset
tests, not checks of allocated storage or pointer ownership.

A negative word zero computes word six plus word zero plus one at
word width and holds that quantity in SI. It subtracts the quantity from
record word zero, holds the quantity as an outgoing argument, reloads word
eight and stores it into word ten before calling 3DCC. The other outgoing
arguments are that word-eight source offset and the sign-extended record
byte four. It removes six argument bytes. Full AX equal to held SI
returns zero. A mismatch also returns zero when current flags word two
has bit 0200. Otherwise it sets flag 0010 and returns FFFF.
Neither mismatch path restores the count or pointer stores made before
the call. SI, DI and BP are restored on every local return, which is far
without incoming cleanup. Callee preservation and writable aliases remain
unadmitted; the comparison relies on SI surviving the call.

The local aggregate helper 2F94 saves BP, SI and DI, reserves two
local bytes and initializes its local count to zero. It captures a word
limit from DS:37C2 installed or DS:3736 on disc into DI and starts
SI at record offset 3682 installed or 35F6 on disc. Its loop tests
the old DI for zero while decrementing DI at word width. Zero skips
the body. Each nonzero old value tests current record flags word two
for any bit in 0003. Any such bit calls 2F13 with SI, removes
two argument bytes and increments the local count regardless of returned
AX. Every iteration advances SI by 0010 at word width.

Under admitted unchanged frame, segments and callee preservation, initial
limit N produces N iterations and a count of selected records, not a
count of successful flushes. Initial zero still decrements DI to FFFF
before exiting. The captured limit is not reloaded each iteration, and
record-offset advancement performs no segment adjustment or extent check.
The helper returns the local count in AX, restores DI and SI, resets
SP to BP, restores BP and returns far without incoming cleanup. The
zero-record caller then replaces that count with zero. Admitted traversal
storage, record writers, shared limit and segment identity remain open.

Both editions have the same local control flow; only the aggregate limit
and starting-record offsets differ in these bodies. No interrupt or
program-execution request occurs locally. The negative-count branch's 3DCC
operation and its downstream/native effects are not completed by this reading.

## Interpretation

The selected diagnostic flush path includes pre-call count and pointer
changes, a failure bypass flag and an aggregate route that drops every
per-record result. A zero return alone cannot prove all pending bytes
were delivered or that failed writes left record state unchanged.
Q-EXE-007 retains 3DCC and its result/preservation contract, record and
table producers, actual DS, extents, aliases and lifetime, other counted-byte
branches, surrounding callers and broader game launch-capability coverage.
This bounded reading does not establish successful output or a whole-game
launch exclusion.

## Alternatives

Interpreting the aggregate count as successful operations ignores unconditional
increments after selected calls. Interpreting the zero-record return as that
count ignores its replacement with zero. Treating the negative-count path
as transactional ignores the stores before 3DCC. Treating the captured
word limit as storage admission ignores unchecked word-offset traversal.

## How to reproduce

At revision 52d4914 require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode, header size 5200 and modeled
load segment 1000. Decode shipped 8113..81CE at corresponding
1000:2F13..1000:2FCE in both editions. Verify opcode 98 at
2F73 as byte-to-word sign extension. Track the self-offset test, signed
count branch, word-width arithmetic, stores before the call, flag 0200
bypass and cleanup. Follow the aggregate loop's captured limit, unconditional
selected-call count and discarded AX into the zero-record caller. Keep the
edition-specific globals and record starts distinct. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
