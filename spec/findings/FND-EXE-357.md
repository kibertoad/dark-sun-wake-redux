---
id: FND-EXE-357
title: Sound utility configuration separates early rejection from mutated-state allocation failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:37F1..1000:390E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-355 calls this far-returning helper with a record far pointer,
zero far pointer, zero or one word, and word 0200. The helper saves BP,
SI and DI, loads DI from SS:BP+0E and SI from SS:BP+10, and loads
ES:BX from SS:BP+06. It compares record word eighteen with the incoming
record offset alone; it does not compare a segment. A mismatch, signed
DI greater than two, or unsigned SI greater than 7FFF returns AX=FFFF
through the shared register-restoration suffix. These local tests precede
the following stores and calls. Negative DI is not rejected by that test.

After these tests it conditionally writes one to current DS:DF12 for
incoming offset DBBC with a zero prior word. Otherwise it conditionally
writes one to DS:DF10 for incoming offset DBA8 with a zero prior word.
Neither offset comparison tests the incoming record segment.

If record word zero is nonzero, it calls 1000:2CC8 with the record
pointer, zero pair and word one, then removes ten argument bytes. It
does not test the returned value. It reloads the record pointer; when
record word two has bit four set, it calls 1000:1ACC with the far pair
at record words eight and ten and removes four bytes, again without a
result test. These callees and their preservation contracts remain unread.

After reloading the record pointer it clears bits four and eight in word
two and clears word six. It writes the incoming record segment and the
low-word record offset plus five into both far pairs at words eight/ten
and twelve/fourteen. No segment adjustment follows offset wrap. Current
DI equal to two or current SI zero selects the AX=0000 suffix directly.

Otherwise it writes zero to DS:DB9E and 3CCD to DS:DB9C. It tests
the OR of incoming buffer words SS:BP+0A and SS:BP+0C. A nonzero
pair skips allocation. A zero pair calls 1000:1BD6 with current SI and
removes two bytes. It stores returned AX into SS:BP+0A and returned DX
into SS:BP+0C before testing their OR. A zero result selects the earlier
AX=FFFF suffix without locally undoing the preceding record or global
writes. A nonzero result sets bit four in record word two.

The continuing path publishes the buffer pair into both record pairs,
stores current SI into record word six, and sets bit eight in word two
only when current DI equals one. It returns AX=0000, restores saved DI,
SI and BP, and far-returns without incoming argument cleanup. The caller
in FND-EXE-355 removes twelve bytes and uses nonzero AX to select its
separate 1000:2873 cleanup call. There is no local DX result normalization.

## Interpretation

The early invalid-input return and later allocation-failure return share
AX=FFFF but have different preceding state changes. A failure return
therefore does not imply an unchanged record. The default pair is computed
within one segment using low-word addition; it is not a validated extent.
The word-eighteen equality does not establish far-pointer identity.

The selected caller supplies DI zero or one and SI 0200 initially.
Using those values after the reached helper calls still requires their
register-preservation contracts. This reading establishes local branch
conditions and store order, not successful native operations, ownership
transfer, valid buffer capacity or admitted segment/record state.

Q-EXE-007 retains 1000:2CC8, 1000:1ACC, 1000:1BD6 and caller
cleanup 1000:2873, their results and preservation, incoming state writers,
aliases and lifetime. No complete-reading promotion or launch exclusion
follows from this bounded local reading.

## Alternatives

Treating both FFFF returns as pre-mutation rejection ignores the stores
before the allocation-result test. Treating the offset equality as a full
pointer check ignores the absent segment comparison. Treating a nonzero
buffer pair as a proven allocation assumes a contract this helper does not
test. Treating initial DI/SI as preserved across unread calls assumes their
register behavior rather than tracing it.

## How to reproduce

At revision c5c0dcb require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00004BF1..0x00004D0E at IP 37F1, modeled CS 1000 and
MZ header size 1400. Follow FND-EXE-355's pushed argument order,
the signed and unsigned comparisons, each local store and call in execution
order, the returned-pair OR and both paths into shared restoration.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
