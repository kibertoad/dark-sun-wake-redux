---
id: FND-EXE-356
title: Sound utility configuration helper returns a post-interrupt DX bit without status checking
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0519..1000:052A
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-355's continuation loads selected record byte four, extends
it to a word, pushes it and reaches this helper through pushed CS
and a near call. The helper forms BP, sets AX=4400, loads BX from
SS:BP+06 and executes interrupt 21. On ordinary continuation it
exchanges DX and AX, masks AX with 0080, restores BP and far-returns
without incoming argument cleanup. It makes no carry, error or returned
AX test and has no error-helper call. It does not locally save DS, ES
or general registers other than BP.

The caller removes the argument word and tests full returned AX against
zero. The local mask permits exactly zero or 0080, rather than a normalized
zero/one result. A nonzero result causes the caller to set bit 0200 in
selected record word two before its next configuration call. At this caller's
ordinary continuation, the loaded record byte has already passed a signed
nonnegative test; its word extension therefore supplies BX in 0000..007F.
That encoded input range does not establish a valid native service argument.

## Interpretation

This resolves the local 0519 obligation in FND-EXE-355. The helper's
returned AX comes from a bit of post-interrupt DX, while outgoing DX
contains post-interrupt AX after the exchange. Neither value establishes
successful native execution. In particular a status indicated by carry or
AX is not checked before the returned DX bit changes the record flags.

Q-EXE-007 retains interrupt results and preservation, admitted record-byte
meaning and writers, aliases and lifetime, and configuration/cleanup callees
1000:37F1 and 1000:2873. The reached AX selector is direct instruction
evidence; this finding assigns no external service semantics or native result.
No complete-reading promotion or execution exclusion follows.

## Alternatives

Treating returned AX as the interrupt's original status word ignores the
exchange. Treating nonzero as one ignores the 0080 mask. Treating the
record-bit update as conditional on a checked successful service ignores
the absent carry/error test. Treating the caller's byte range as native
argument admission confuses an encoded local bound with an external contract.

## How to reproduce

At revision fc4235c require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00001919..0x0000192A at IP 0519, modeled CS 1000 and
MZ header size 1400. Compare FND-EXE-355's argument construction,
sixteen-bit byte extension, argument cleanup and full-word result test.
Follow the DX/AX exchange and mask independently of any external service
name. Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
