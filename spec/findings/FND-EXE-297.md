---
id: FND-EXE-297
title: Smaller-existing-count helper copies by old header count before unchecked release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1622..1000:169E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-295 selects this near helper when the existing header count
is unsigned-smaller than the converted request. It pushes incoming BX,
reads CS:1363 into SI and pushes it, then reads CS:1365 into SI
and pushes that. It calls 1000:15A5 through pushed CS and a near
call, then removes four argument bytes. Thus the allocator's request
comes from the shared high/low words, not a private saved request.

It tests returned DX alone. Zero pops saved original BX and near-returns
without copying, releasing or rewriting AX. Nonzero pops the original
BX into DS, loads ES from returned DX, and pushes ES, DS and the
allocator's current BX. The first pushed word retains the new segment
for the final result; the latter two words remain as outgoing arguments
to a later call. The current BX need not equal the original BX.

It reads word DS:0000 into DX, clears the direction flag and decrements
DX at word width. With SI=DI=0004 and CX=0006 it copies six words
forward from DS to ES. Zero decremented DX bypasses the remaining copy.
Otherwise it increments both segment words by one, clears SI and DI,
sets CX to the unsigned minimum of DX and 1000, shifts CX left three
and copies that many words forward. It subtracts 1000 from DX; unsigned
below-or-equal finishes. Otherwise it adds 1000 to both segment words
at word width, clears SI/DI again and repeats the chunk selection.
There is no overlap test, segment-wrap rejection or local extent validation.

For a stable header word H and ordinary unmodified loop execution, let
N be H minus one modulo 65536. The copy performs twelve prefix bytes,
then sixteen times N bytes in chunks of at most 65,536 bytes. Its
total is therefore 12 + 16N. H=0001 copies twelve bytes; H=0000
wraps N to FFFF and copies 1,048,572 bytes. Across all header words
this is the maximum local byte count. This arithmetic bound is not a
proof that the source or destination has that extent, or that repeated
segment selections designate distinct physical storage.

After copying it reloads DS from shared CS:1361, then calls
1000:149B through pushed CS and a near call. The retained outgoing
high word is the original segment saved before copying; the low word
is the allocator-return BX pushed at that time. FND-CONFIG-167's
dispatcher reads the high word and ignores the low one. The helper
removes four argument bytes, pops the retained new segment into DX,
sets AX=0004 and near-returns without testing the release result.
It does not restore the direction flag locally.

## Interpretation

This resolves FND-EXE-295's second unread helper and follows its request,
copy and release as distinct steps. A nonzero returned segment selects
copying even without a separate AX validity test. Copy size follows the
old header word, not the new request size. The final encoded new pointer
is returned even where the release dispatcher returns a rejection.

Q-EXE-010 retains shared request/DS writers and aliases, actual header
admission, source and destination extents, overlap and segment wrap,
interrupt effects and lifetime, including effects on retained stack words.
FND-EXE-295 and FND-EXE-296 supply the surrounding local branches,
not complete state or caller coverage. No valid-copy, successful-release
or complete-reading claim follows.

## Alternatives

Treating failure as a two-word null test ignores the DX-only branch.
Treating the copy as request-sized ignores its header read and decrement.
Treating zero count as no copy ignores the wrapped decrement and prefix.
Treating the release's AX as the final result ignores the overwrite with
four. Treating consecutive segment chunks as validated distinct storage
ignores aliases and word-width wrapping.

## How to reproduce

At revision 649989c require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With locked Capstone
5.0.7 in sixteen-bit x86 mode decode shipped half-open
0x00006822..0x0000689E at IP 1622, modeled CS 1000. Track all
retained stack words, each segment writer, the direction flag, the unsigned
chunk comparisons and both calls' result continuations. Check the formula
for every H from 0 through 65535, including 0, 1, 4097 and 65535;
their totals are respectively 1,048,572, 12, 65,548 and 1,048,556.
Compare FND-CONFIG-167's dispatcher argument reads. Source bytes and
reports remain outside Git. No original execution or copy emulation is
involved; the formula check is arithmetic over the stated local reading.
