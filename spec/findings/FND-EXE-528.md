---
id: FND-EXE-528
title: Game record positioning resets state before native positioning and sign-extends its buffered adjustment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2FCE..1000:3093
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2FCE..1000:3093
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The far helper 302B saves BP, SI and DI, reads its incoming record
offset from SS:BP+6 into SI and mode from SS:BP+12 into DI.
It passes SI to far-returning 2F13 and removes two argument bytes.
Any nonzero AX returns FFFF immediately. FND-EXE-503 records that
flush helper's branches and effects; its failure need not leave state intact.

Zero AX continues. Mode exactly one and current record word zero
strictly positive signed calls near 2FCE with the record offset. That
helper removes its two-byte argument. Opcode 99 sign-extends its returned
AX into DX; 302B subtracts AX from incoming low word SS:BP+8
and subtracts DX with borrow from incoming high word SS:BP+10.
This changes the incoming frame words at word width, with no range check.
The adjustment is signed extension of the returned word, not unconditional
zero extension of a byte count. Other modes or nonpositive current record
word zero skip that adjustment.

Every continuing path masks record flags word two with FE5F, clears
record word zero and stores freshly read word eight into word ten.
These stores precede the next call. It pushes mode, current incoming high
and low words, then sign-extended record byte four, into far-returning
07B0 and removes eight argument bytes. FND-EXE-502 records that
helper's handle-flag store and native positioning request. Only returned
DX and AX both FFFF produce FFFF here; any other pair produces
AX zero. No local rollback restores record or argument changes on failure.
All local paths restore DI, SI and BP and return far without incoming
argument cleanup. Callee/native frame and register preservation remain conditions.

Near 2FCE saves BP, SI and DI and reads its incoming record
offset from SS:BP+4 into SI. Negative signed record word zero
forms DX as word six plus word zero plus one at word width and
copies it into DI. Nonnegative instead reads word zero into AX,
uses opcode 99 to sign-extend, then XOR/subtract with DX to form
the word magnitude and copies it into DX and DI. For an unchanged
nonnegative input this is the input itself.

Record flag 0040 set skips byte scanning and returns DI in AX.
Otherwise it holds record word ten in CX and reads record word zero
again to choose direction. Negative chooses backward scanning: each visited
byte first decrements CX, uses BX=CX and reads DS:BX. Nonnegative
chooses forward scanning: each visited byte uses BX=CX, then increments
CX before reading DS:BX. Every byte equal to 0A increments DI at
word width. Neither path writes those scanned bytes.

Each loop tests old DX for zero while decrementing DX. Zero skips
the byte access and finishes with DX FFFF. A captured initial quantity
N visits N bytes, at most 65535, conditional on intact local execution.
The returned DI is initial quantity plus the number of visited 0A bytes,
modulo 65536. This does not prove an admitted buffer extent or a count
representable as a nonnegative signed word. Offset progression wraps at
word width without segment adjustment or a local extent check.

2FCE returns DI in AX, restores DI, SI and BP and near-returns
with two-byte incoming cleanup. It makes no calls or interrupts and does
not change DS or SS locally. Both editions share the instructions in
these bodies. The decoder's displayed CDQ/CWDE mnemonics for opcodes
99/98 denote word CWD/CBW in this sixteen-bit context.

FND-EXE-512's 364E caller supplies mode one and two zero offset
words, calls 302B only when its record word zero is nonzero, removes
eight argument bytes and ignores AX. The adjustment condition is checked
after flushing, so the caller's pre-flush nonzero test alone does not
establish whether 2FCE runs. That caller continues record cleanup even
when positioning returns FFFF.

## Interpretation

This resolves 364E's immediate positioning dependency and its byte-count
adjustment helper. State changes and argument arithmetic precede native
failure, and the result is discarded by the recorded caller. Q-EXE-007
retains actual record/buffer producers, extents, aliases and segments, native
I/O contracts, other callers/writers and earlier startup/launch coverage.
No complete stream, initialization or launch contract is claimed.

## Alternatives

Treating failure as transactional ignores stores before 07B0. Treating
the adjustment as unsigned ignores CWD before the two-word subtraction.
Assuming the adjustment always runs ignores its post-flush count test.
Using the scan bound as storage validation ignores offset wrap and absent
extent checks. Treating every nonzero native pair as failure contradicts
the exact FFFF:FFFF comparison.

## How to reproduce

At revision 7975477 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
81CE..8293 in sixteen-bit mode. Track both frame layouts, manufactured
far calls, cleanup widths, pre/post-flush reads, opcode 99 and ordered
record stores. Follow each scan's old-count test, decrement, byte address
and output increment, including zero and wrapped quantities. Trace the
two-word result test and 364E's result disposal using FND-EXE-512;
use FND-EXE-502/503 for the immediate callees. Keep native and storage
admission explicit. Licensed bytes remain outside Git; no original process,
DOSBox or emulated call runs.
