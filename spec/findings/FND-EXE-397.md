---
id: FND-EXE-397
title: Game type-three published callees retain request-field writes on zero work and stage odd bytes through mutable records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:06B6..15F3:076B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:06B6..15F3:076B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:076B..15F3:0851
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:076B..15F3:0851
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000B1A8..0x0000B1E8
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0000B1A8..0x0000B1E8
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-396's two wrappers reach far 15F3:06B6 and 15F3:076B
with six words in their separately recorded orders. Both callees save BP,
BX, CX, DX, SI, DI, ES and DS, and assign DS from CS.
They load the count into CX from SS:BP+000C and ES:BX from the
far pointer at +000E/+0010. They obtain the word at +0006 in DX.
06B6 writes that word to DS:007C and DS:009C; 076B writes it to
DS:0092, DS:00B2 and DS:009C. These writes precede every work-count
guard and every indirect call.

They then clear DX, load DI from SS:BP+0008, and twice shift DI
right with its low bit rotated into DX. They add the incoming word at
SS:BP+000A to DX without propagating the addition's carry into DI.
Thus DI is the first word shifted right two, while DX is that word's
low two bits shifted left fourteen plus the second word, modulo 10000.
Inputs 0003/3FFF give DI:DX 0000:FFFF; 0003/4000 give 0000:0000,
not 0001:0000. Inputs FFFF/FFFF give 3FFF:BFFF, not 4000:BFFF.
These are local arithmetic cases, not admitted native address bounds.

An even count skips the odd-byte staging. If CX is zero at the later
count guard, each routine clears AX and returns with the initial field writes
retained and no indirect call. A nonzero count selects the bulk request
described below. Neither routine locally tests the supplied +0006 word or
checks that the far target is installed before a reached call.

### Shipped request records and their later writes

The shipped sixteen-byte regions corresponding to CS offsets 0078 and 0088
contain only zero words. At 0098 the initial first word is two and the
word at record offset 000C is 0076; at 00A8 the initial first word
is two and the word at record offset 0006 is 0076. The other words
in those two regions are initially zero. These are shipped-file observations,
not an assertion that a later request sees those defaults.

FND-EXE-394's probe writes current CS to offsets 00A6 and 00B0
before its native requests. These are the high words paired with the two
initial 0076 offsets. Their complete writer and runtime-storage contracts remain
open. Neither published callee invokes that probe or resets every record word.

For a nonzero bulk count, 06B6 writes current CX to DS:0078,
BX and ES to DS:0084/0086, and DX and DI to DS:007E/0080.
It selects SI 0078 and AH 0B and calls the far pointer through
current DS:00B8/00BA. For 076B the corresponding writes are CX to
DS:0088, BX/ES to DS:008E/0090 and DX/DI to DS:0094/0096;
it selects SI 0088 and AH 0B before the same default-DS lookup.
They write only the low count word, not the adjacent high word. Shipped
zero high words or other untouched fields do not establish later zero values.

Every reached indirect call is admitted only when its full returned AX
is exactly one: the routine decrements AX and tests the zero flag. A
failed test maps to AX eight. A passing bulk test leaves AX zero and
returns. Both routines restore their saved registers on final exit and return
far without incoming cleanup. No local rollback restores their request records,
scratch bytes or explicit buffer writes on AX eight.

### Callee 06B6 odd-byte staging

An odd incoming count first decrements CX, leaving the even bulk count.
For odd DX it decrements DX, writes DX/DI to DS:009E/00A0,
selects SI 0098 and AH 0B, saves BX and calls the indirect target.
After restoring BX it tests the exact-one result. A passing result reads
the word at current DS:0076 and writes its high byte through current
ES:BX. It increments BX and adds two to current DX without a carry
into DI, then reaches the bulk count guard.

For even DX it adds CX to DX, writes that sum to DS:009E,
subtracts CX back from DX, and writes DI to DS:00A0. It makes
the same SI-0098 request with BX saved/restored and tests exact one.
A passing result reads current DS:0076 and writes its low byte through
ES at BX plus current CX, using two exchanges of CX and DI to
form the address. It then reaches the bulk count guard. The temporary
sum and final buffer address are word-width operations; neither carries into
an adjacent segment/high word.

### Callee 076B odd-byte staging

An odd incoming count also decrements CX. For odd DX it decrements
DX and writes that low word to DS:009E and DS:00B4 and DI
to DS:00A0 and DS:00B6. It makes the SI-0098 request with
BX saved/restored and tests exact one. A passing result reads a byte
through current ES:BX and writes it to current DS:0077. It next
makes an AH-0B request with SI 00A8, again saving/restoring BX,
and again requires exactly returned AX one. A passing result increments BX
and adds two to current DX without advancing DI before the bulk guard.

For even DX it adds CX to DX and publishes the resulting low word
to DS:009E and DS:00B4, then subtracts CX from DX and writes
DI to DS:00A0 and DS:00B6. After a passing SI-0098 request
it reads the byte at current ES:(BX+CX), using the two register exchanges,
and stores it at current DS:0076. It makes the SI-00A8 request
and tests exact one before reaching the bulk count guard. A later failed
request does not undo that explicit scratch-byte write.

### Per-call inputs and preservation limits

The routines set AH 0B for each request, but do not supply one
uniform AL. With modeled CS 15F3, the first reached call in either
routine has AL F3 from its initial CS-to-AX assignment. In 06B6,
after a passing scratch request, the later bulk call's AL comes from the
low byte of the subsequently read current DS:0076 word. In 076B's
odd-DX path, the first successful result decrement makes AL zero; its
next scratch request retains that zero while the buffer byte is held in AH
and stored. In its even-DX path, the next scratch request instead retains
the newly read buffer byte in AL. A passing second request decrements AX
to zero, so a subsequent bulk call in either 076B odd-count branch has
AL zero. AH selection alone does not establish the native contract for AL.

Only BX is saved/restored around the staging calls. CX, DX, DI, DS
and ES used by the following arithmetic, field writes, pointer lookup and
explicit byte access are the then-current registers. No CS-to-DS reload occurs
after those calls. The outer saves restore the caller's registers only on
final exit and are not intermediate preservation guarantees. A count-one input
leaves CX zero before the first staging request; reaching the final no-bulk
path afterward depends on native CX preservation. Even when that path is
reached, the earlier requests and explicit byte write remain distinct effects.

There is no local count-to-buffer extent check, pointer-range validation,
segment-carry correction, or guard against aliases between the buffer, records,
scratch pair and saved frame. Whether the target performs a transfer or
preserves neighboring scratch bytes is not established by requesting it.
The initial published words, the two record layouts and the current far
target's full identity remain separate obligations.

Both editions have identical bytes across both complete bodies and the
inspected shipped record regions. The 06B6 body contains 81 instructions
ending in a far return at 076A; 076B contains 100 ending at 0850.

## Interpretation

This supplies both remaining type-three published callee bodies, including
zero-work prefix writes, word-width address construction, exact-one returns,
odd-byte staging and the distinct lower-byte inputs of their native requests.
It does not establish native transfer semantics or producer-reachable storage.
Q-EXE-007 retains native pointer identity and register/record effects, complete
callers and writers, buffer/record aliases and bounds, and type four. No
complete-reading declaration or game launch exclusion is made.

## Alternatives

Calling the zero-work path side-effect-free would erase its initial record
stores. Replacing the address construction with a widened multiply/add would
retain a carry the body drops. Treating the records as freshly initialized
for each request would supply writes to untouched fields. Treating the first
successful scratch result as proof of the later bulk result would erase a
separate exact-one test. Assuming all AH-0B requests share AL zero would
lose the recorded lower-byte producers. Treating the outer register saves as
per-call preservation would replace current native outputs with stale values.

## How to reproduce

At revision 452a3e1e require both identities from FND-EXE-350. Use MZ
header size 5200, relative segment 05F3 and modeled load segment 1000.
Decode the two address ranges separately in sixteen-bit mode at shipped-file
base B130 plus their offsets. Verify full byte coverage, instruction counts
and edition equality. Read the file-data range separately as four sixteen-byte
regions beginning at relative offsets 0078, 0088, 0098 and 00A8,
and check only the initialized values described above, not assumed runtime zeros.

Use FND-EXE-396 for the two wrappers' six-word bindings and FND-EXE-394
for the probe's pointer and record-segment writes. Follow the initial stores,
paired shifts, dropped carry, odd/even count and DX branches, every staging
call, BX restore, exact-one test, byte write and later bulk guard. Check
the three stated arithmetic cases at word width and track AH and AL's
last writers separately for every reached call. Distinguish shipped defaults,
current DS/ES storage and native changes. No negative caller/writer census
is claimed. Licensed bytes stay outside Git; no game, DOSBox or emulated
call runs.
