---
id: FND-EXE-514
title: Game allocator selected-block helpers rewire links and publish split metadata without local extent checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2133..1000:2172
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2133..1000:2172
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:223B..1000:2254
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:223B..1000:2254
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-513's near helper 2133 loads DI from current DS word
BX+6 and compares DI with BX. Equality clears shared head word
DS:3910 installed or DS:3884 on disc and returns near. Inequality
first stores DI into that shared head, then loads SI from current DS
word BX+4, stores SI at DI+4 and stores DI at SI+6,
all through current DS, then returns near. It does not locally modify
BX or AX and makes no calls or interrupts. This resolves BX preservation
for the wrapper's following increment of word BX and formation of BX+4.
It does not admit the links or the backing storage.

Adjacent near helper 214F loads SI from that same shared head.
Nonzero loads DI from SI+6, then stores BX into SI+6 and
DI+4, stores DI into BX+6 and SI into BX+4, in that
order. Zero first stores BX into the shared head, then stores BX
into both BX+4 and BX+6. Both paths return near without incoming
cleanup. BX and AX are locally unchanged. These local stores resemble
link insertion only under admitted disjoint records and valid links; the body
itself checks neither. Its callers remain outside this bounded reading.

The selected-block near helper 223B receives FND-EXE-513's internal
size in AX and selected offset in BX. It subtracts AX from current
word DS:BX, then forms SI as BX plus the freshly read resulting
word. It copies SI into DI and adds the original AX to DI.
It increments AX and stores it at DS:SI, stores BX at DS:SI+2,
then stores SI at DS:DI+2. Finally it adds four to SI, copies
SI into AX and returns near without incoming cleanup. BX is locally
unchanged; SI and DI are changed. All arithmetic is word-width and there
are no carry, underflow, allocation or destination-extent checks locally.

The original block-size store precedes both new metadata and the returned
offset. Where aliases overlap block words, link fields or metadata destinations,
the later reads observe earlier stores. A nonwrapping interpretation as a
split within one block therefore requires admitted size, unit, header, links,
storage and aliases, not merely the wrapper's unsigned comparison. In
particular, FND-EXE-513's size-plus-eight threshold can wrap; this helper
does not repair or independently reject that case.

These three bodies contain no calls or interrupts, perform accesses through
current DS and do not locally change DS or SS. Their near returns
have no incoming cleanup. Both editions share local control flow; only the
shared-head offsets differ in the first two bodies.

## Interpretation

This resolves the two selected-block callees and records the adjacent shared-
head writer needed for later state tracing. It proves local BX preservation,
not valid allocated storage. Q-EXE-007 retains 21D2/2212, shared-head
and block producers, 214F's callers, units and headers, admitted extents,
actual DS/SS, aliases and lifetime, the remaining initialization helpers and
earlier startup effects, and broader launch coverage. No complete allocator or
startup contract is claimed.

## Alternatives

Treating link rewiring as validated removal or insertion ignores unchecked
links and aliases. Treating BX+4 as a writable allocation ignores block
admission. Treating the split as nonwrapping ignores word-width arithmetic and
the wrapper's wrapped threshold. Treating mutation as transactional ignores
the ordered stores with no local rollback.

## How to reproduce

At revision 5e896a7 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
7333..7372 and 743B..7454 in sixteen-bit mode. Track 2133's
equality branch, shared-head store and later link reads; track 214F's
ordered stores on zero/nonzero head paths. Follow 223B's subtraction,
fresh size reload, word-width offset formation, metadata stores and AX return.
Separate local register preservation from admitted records, units and storage.
Licensed bytes stay outside Git; no game process, DOSBox or emulated call runs.
