---
id: FND-EXE-472
title: Sound utility lower read transforms bytes and copies trailing-request slot without result test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3720..1000:37F1
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:06FF..1000:072E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-379 and FND-EXE-471 call far-returning 3720 with a first word,
destination pair and requested count. This helper reserves four local bytes
and saves SI/DI. The first word must be unsigned below current DS:DD38;
otherwise it passes error word 0006 to 04CE and returns that helper's FFFF AX.
FND-EXE-362 reads the mapper. Count zero or FFFF selects AX zero via
a wrapped increment and unsigned comparison with two. For other counts,
indexed word bit 0200 also selects AX zero. Index formation doubles the
first word at word width and uses current DS displacement DD3A.

The continuing route calls 06FF with the incoming first word, destination
and count, removes eight argument bytes and saves AX to SS:BP-02.
Returned zero or FFFF, or indexed bit 4000 clear, returns that saved word.
No comparison verifies that a different returned count fits the requested
count or destination. Bit 4000 set with returned AX from one through
FFFE selects byte transformation using that returned count in CX.

The transform loads the destination pair into ES:SI and sets DI/BX to
its starting offset. With forward direction it reads bytes through ES:SI.
Ordinary bytes are written through ES:DI and count down CX. Byte 0D
is discarded while CX remains nonzero. On a last-byte 0D, it holds ES
and starting BX and calls 06FF for one byte into SS:BP-03. After
eight-byte argument cleanup it restores BX/ES, reloads the local byte
and writes it through current ES:DI. It does not test this request's
AX, locally initialize that byte beforehand, or apply the 0D/1A filters
to the copied byte. SI/DI preservation across the native request remains
necessary for identifying current cursors.

After a block or the extra-byte route, DI equal to starting BX repeats
the full incoming request from 3756. No independent iteration cap or
progress test exists. Inequality returns DI minus starting BX at word
width. Offset wrap and destination extent are not checked. Under admitted
nonwrapping cursors and ordinary preservation, the transformed output
contains at most the returned count of the selected full request, but its
bytes may include the extra request's slot. This conditional local bound
is separate from the unverified native returned count and requested extent.

Byte 1A selects another path before the loop decrements its current CX.
The helper calls 05CC with incoming first word, mode one and a negative
double-word quantity formed by negating remaining CX and extending its
borrow into the high word. Thus on the ordinary positive remaining-count
path the quantity is minus that remaining count, including the selected
byte's position in the count. FND-EXE-376 reads that wrapper. The result
is ignored; current DS's indexed word receives bit 0200, then the helper
returns current DI minus held starting BX. It performs no restoration
result test or local rollback, and native DI/DS preservation is open.

The 06FF wrapper has no independent first-word range check. Indexed
bit 0002 set passes error word 0005 to 04CE instead of executing an interrupt.
Otherwise it saves DS, sets AH=3F without separately initializing AL,
loads BX from the first word, CX from count and DS:DX from destination,
then executes interrupt 21. It restores DS before checking carry. Carry
set passes returned AX to 04CE; carry clear retains AX. It restores BP
and returns far without incoming cleanup. It does not save SI, DI or ES,
establish whether bytes were written, or normalize a DX result pair.

The outer helper restores SI/DI/SP/BP and returns far without incoming
argument cleanup. Its local slot, indexed flags, destination bytes and
downstream position state have separate last-writer and preservation
requirements. No native output guarantee is assigned by this reading.

## Interpretation

This resolves the lower read called by FND-EXE-379/471. Accepted native
count, transformed count, extra-request result and copied local byte are
different values. The trailing-byte path copies a slot regardless of the
extra result; a zero or failure result alone does not identify the slot's
contents. The 1A route likewise publishes a flag after an ignored position
result. Caller refill publication does not supply the missing native bounds.

Q-EXE-007 retains limit/index/flag producers, actual DS, native arguments,
results and preservation, destination/frame extent and aliases, local-slot
writers, repeat-entry state and later pathname consumers. The auxiliary
27FC contract remains open. No successful read, capacity or complete-reading
claim follows.

## Alternatives

Treating returned count as requested capacity assumes a native guarantee
not checked here. Treating every copied byte as a successful read ignores
the unchecked extra result and slot lacking local initialization. Treating 1A handling
as a tested position update ignores the discarded result. Treating finite
block count as total loop termination overlooks the empty-output retry.

## How to reproduce

At revision 2ba7545 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00004B20..0x00004BF1 at IP 3720
and 0x00001AFF..0x00001B2E at IP 06FF, modeled CS 1000,
MZ header size 1400, using locked Capstone 5.0.7 in sixteen-bit mode.
Track wrapped count gates, saved returned count, ES-prefixed source reads,
CX before its loop decrement, last-0D extra arguments and local-slot use,
empty-output re-entry, 1A's negative quantity and ignored result, DS save
and native preservation limits. Original bytes remain outside Git; no
original process, DOSBox, interrupt thunk or emulated call is executed.
