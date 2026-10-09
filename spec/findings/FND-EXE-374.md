---
id: FND-EXE-374
title: Sound utility neighboring resize entry publishes shared quantity before segment dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1D3D..1000:1DBE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1C5D..1000:1CD9
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

Entry 1D3D forms a BP frame and sets incoming quantity high word DX
to zero. Alternate entry 1D44 forms the frame and loads DX from
SS:BP+0C. Both load low quantity AX from BP+0A and old segment BX
from BP+08; BP+06 is not read locally. After saving SI/DI they store
incoming DS to CS:1992, DX to CS:1994 and AX to CS:1996.
All dispatch and rejection decisions follow those shared stores.

An old segment zero passes the incoming quantity to far-returning 1BE0,
whose local arithmetic FND-EXE-371 reads, and removes four argument
bytes. A nonzero old segment with quantity zero calls 1ACC with segment
BX and offset zero, removes four bytes and returns AX/DX zero without
testing the callee result. FND-EXE-367 reads that segment-only helper.

Otherwise the root applies the same add-nineteen, carry and high-bit gate
and sixteen-unit packing recorded in FND-EXE-371. Rejection returns a
zero pair. It loads ES from old BX and compares ES:0000 unsigned with
the encoded requested units. Equality returns BX:0004. Greater calls
1CD9; smaller calls 1C5D. Both near results pass to the common suffix,
which reloads DS from CS:1992, restores DI/SI/BP and returns far
without incoming argument cleanup. The 1CD9 contract remains open.

The larger-buffer helper 1C5D first holds old BX on the stack, loads
the shared high/low quantity from CS:1994/1996 and calls 1BE0 with
those words. It removes four incoming bytes and tests only returned DX
for zero. Zero restores old BX and returns the allocator pair unchanged.
Nonzero pops the held old segment into DS, loads ES from returned DX
and holds the new segment, old segment and current BX on the stack.
It ignores returned AX as a copy offset.

It loads source DS:0000 into DX, decrements it at word width and copies
six words from source offset four to destination offset four with forward
string direction. If decremented DX is zero it skips the bulk loop.
Otherwise it increments both segment words by one, starts SI/DI at zero
and copies min(current DX, 1000) times eight words. Each chunk therefore
copies at most 8000 words. It subtracts 1000 from DX at word width;
borrow or equality ends the loop. Otherwise it adds 1000 to both segment
words, resets offsets to zero and repeats. No extent is checked locally.

Under admitted noninterfering segments and state, a source word-zero C
therefore selects twelve initial bytes plus sixteen times ((C - 1) modulo
65536) bulk bytes. C=0001 selects only twelve bytes; C=0000 underflows
to FFFF bulk units rather than selecting an empty copy. The destination
extent and interpretation of C are not established by this arithmetic.
Segment increments wrap at word width. The full-size chunk uses 8000
word copies and wraps its string offsets, then explicitly advances segments.

After copying it reloads DS from CS:1992 and calls far-returning 1ACC
with the two held words above the retained new segment. Its segment
argument is the held old DS; the low word is current BX held after 1BE0,
not independently initialized as an offset. The selected callee ignores
that offset under FND-EXE-367. The helper removes four argument bytes,
ignores the result, pops the retained new segment into DX and returns
AX=0004. There is no rollback or copy-result check in this local body.

## Interpretation

This identifies additional direct writers of the shared saved-DS and incoming
quantity words used by the neighboring allocator family. Even local rejection
publishes those words. The larger-buffer path uses source word zero for copy
volume, not the rounded requested quantity or a proven returned capacity.
Its returned new pair follows an unchecked terminal helper call.

Q-EXE-007 retains callers and startup reachability for these entries,
1CD9's contract, all shared-state writers and lifetime, original segment/header
admission, destination extents, aliases and re-entry. No complete writer
census, successful allocation or writable-capacity claim follows.

## Alternatives

Treating rejection as unchanged shared state ignores the initial stores.
Treating a zero source count as zero copy ignores its decrement. Treating
requested units as the copy bound ignores the source-field load. Treating
the final returned pair as a tested release outcome ignores the discarded
callee result. Identifying every caller is outside this local entry reading.

## How to reproduce

At revision fbc9fef require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x0000313D..0x000031BE at IP 1D3D
and 0x0000305D..0x000030D9 at IP 1C5D, modeled CS 1000,
MZ header size 1400, using locked Capstone 5.0.7 in sixteen-bit mode.
Follow shared publications, selected segment, requested-unit comparison,
held stack words, source count decrement, chunk bounds and segment wrapping,
then final held arguments and ignored result. The extension at 1DB4 is
sixteen-bit despite the decoder's wider mnemonic. Keep 1CD9 and caller
admission open. Original bytes stay outside Git; no original process,
DOSBox or emulated call is executed.
