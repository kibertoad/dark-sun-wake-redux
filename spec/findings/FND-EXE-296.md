---
id: FND-EXE-296
title: Larger-existing-count helper publishes headers before unchecked cleanup requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:169E..1000:1702
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-295 reaches this near helper when the word read from ES:0000
is unsigned-greater than the converted request count. Incoming BX is
that ES segment, CX the existing count and AX the request count.
The helper compares BX with CS:135D to choose two separate paths.

For inequality it computes DI=BX+AX and SI=CX-AX at word width,
loads ES=DI and writes SI to ES:0000 and BX to ES:0002. It pushes
this ES, then AX. It reloads ES=BX and writes AX to ES:0000.
It computes DX=BX+CX, loads ES=DX and tests ES:0002. Nonzero
replaces ES:0002 with DI; zero writes DI to ES:0008 instead.
It then copies BX into SI and calls 1000:149B through pushed CS
and a near call. The retained arguments are the remainder segment
DI as the high word and request AX as the low word. FND-CONFIG-167
reads that dispatcher: it consumes the high segment word, not the low
offset word, and saves/restores SI on ordinary continuation. This helper
removes four argument bytes, sets DX from retained SI and sets AX=0004,
discarding the dispatcher's AX result before near-returning.

For equality it first pushes original BX for later restoration into DX.
It loads ES=BX and writes request AX to ES:0000. It adds AX to BX
at word width, pushes that adjusted BX as the outgoing high word,
clears AX and pushes zero as the outgoing low word, then calls
1000:17F2. It removes four argument bytes, restores original BX into
DX and sets AX=0004, without testing the callee's result. The preceding
header write is not locally undone on a rejected request.

Neither path locally checks the segment sums for wrap, validates the
selected headers or restores earlier header publications after its call.
The inequality path's count subtraction is preceded by the outer unsigned
larger-count selection, but storage aliases and changes between reads and
writes remain outside this local arithmetic relation.

## Interpretation

This resolves one unread helper in FND-EXE-295. The helper retains the
original block's encoded segment and returns offset four on ordinary
continuation, even where its final callee returns a rejection. The two
paths publish different headers and call different procedures; neither
can be replaced by a single successful extent-update claim.

FND-CONFIG-167 supplies the callee's bounds rejection and result mapping;
FND-EXE-278 and FND-EXE-279 retain lower publication and interrupt
obligations. Q-EXE-010 still needs admitted headers, state writers,
segment sums, physical aliases, extent lifetime, incoming caller coverage
and the other helper at 1000:1622. No valid returned extent, rollback
guarantee or complete-reading promotion follows.

## Alternatives

Treating returned offset four as successful cleanup ignores the overwritten
AX result. Treating the tail request as the original pointer ignores its
derived remainder segment and the dispatcher's segment-only read.
Treating failure as leaving the old count intact ignores publication before
the call. Treating the final neighboring header store as unconditional
ignores its separate word-two test and word-eight alternative.

## How to reproduce

At revision d06208f require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With locked Capstone
5.0.7 in sixteen-bit x86 mode decode shipped half-open
0x0000689E..0x00006902 at IP 169E, modeled CS 1000. Follow
FND-EXE-295's larger-count incoming branch, every ES selection, the
retained argument words, both cleanup sequences and the overwritten
callee results. Compare FND-CONFIG-167's local callee contracts without
assuming native state admission. Source bytes and reports stay outside
Git. No original execution or complete writer search is claimed.
