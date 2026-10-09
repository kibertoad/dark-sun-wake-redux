---
id: FND-EXE-372
title: Sound utility growth helpers distinguish preliminary alignment and later failure checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1AF5..1000:1BB3
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-371's initial growth branch calls near 1AF5 with encoded units
in AX. The helper holds that word on the stack, reloads DS from CS:1992
and calls 1E73 with two zero words. It removes four argument bytes and
masks returned AX with 000F. A nonzero nibble produces another call with
high word zero and low word sixteen minus that nibble. DS is reloaded from
CS:1992 before this call. Neither preliminary result is tested against FFFF.
The second preliminary result is discarded before the held unit word is popped.

It pushes the restored unit word again and constructs a double-word quantity
equal to units times sixteen: BX receives the original high byte shifted right
four, and AX is shifted left four. After reloading DS from CS:1992 it pushes
BX then AX and calls 1E73. After four-byte argument cleanup it pops the held
unit word into BX. Returned AX equal to FFFF selects AX/DX zero and a near
return. Otherwise it publishes returned DX to both CS:198C and CS:198E,
loads DS from DX, stores held units to word zero and DX to word two, and
returns AX=0004 with DX retained. These stores follow the request result test.

The alternate growth helper at 1B59 similarly holds units, constructs units
times sixteen, reloads DS from CS:1992 and calls 1E73. It restores units
into BX after argument cleanup. AX equal to FFFF returns a zero pair.
Otherwise AX is masked with 000F. A zero nibble goes directly to publication:
CX receives old CS:198E, CS:198E receives returned DX, DS receives DX,
word zero receives held units and word two receives the old segment in CX.
It then returns AX=0004 and the selected DX segment.

For a nonzero nibble, 1B59 pushes held units and the first returned DX,
then calls 1E73 with high word zero and low word sixteen minus the nibble.
There is no intervening reload of DS on this path. After four-byte argument
cleanup it pops the saved first DX and held units, then tests the second
returned AX against FFFF. Equality returns a zero pair. Otherwise it increments
the saved first DX at word width and enters the same publication sequence.
The second returned DX is discarded by the stack restoration. Segment increment
wraps at word width and has no local extent check.

The local bodies contain no interrupt. The only calls are to 1E73. Their
ordinary returns do not clean the caller's incoming arguments. The extensions
in the zero-pair suffixes are sixteen-bit sign extensions of zero. DS after
successful publication is the chosen segment; its restoration belongs to the
outer helper read in FND-EXE-371.

## Interpretation

Both growth branches pass a quantity sixteen times the encoded unit count,
but the lower helper's units, state changes and result contract remain open.
The initial branch's preliminary calls are not guarded by a failure-sentinel
test. The alternate branch tests its first result before alignment and its
second result before publication, while retaining the first segment as the
source of the eventual return. A second failure therefore does not prove the
first call left no state change. A nonzero low nibble is not rejected locally.

These sequences establish local arithmetic, result selection and ordered
publication only. Writable capacity, alignment validity and allocation success
require 1E73 and its callees, incoming DS and register preservation, shared
CS-state writers, field extents, aliases and re-entry admission under Q-EXE-007.
No native result or complete reading is claimed.

## Alternatives

Treating every request as checked ignores the two preliminary calls at 1AF5.
Treating the second returned segment as the aligned segment ignores the saved
first DX restoration and its increment. Treating a zero-pair result as rollback
assumes the unread lower helper reverses prior changes. Treating multiplied units
as writable capacity assumes a lower contract not established by this reading.

## How to reproduce

At revision 4d8ea8e require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00002EF5..0x00002FB3 at IP 1AF5,
modeled CS 1000 and MZ header size 1400, with locked Capstone 5.0.7
in sixteen-bit mode. Follow both entries, their held-unit stack words,
argument cleanup, nibble tests, DS reloads, sentinel comparisons, restored
first segment and publication order. The instructions at 1B57 and 1BB1
are sixteen-bit extensions despite the decoder's wider mnemonic.
Original bytes stay outside Git. No original process, DOSBox, interrupt
thunk or emulated call is executed.
