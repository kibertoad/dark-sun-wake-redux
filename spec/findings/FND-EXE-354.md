---
id: FND-EXE-354
title: Sound utility pointer selector advances before testing its derived record bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2BC7..1000:2C11
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-360's file-interface wrapper calls this near helper and retains
its returned DX:AX. The helper forms an SS-relative BP frame with four
local bytes. It stores incoming DS as the local high pointer word and
DBA8 as its low word. It loads that pair into ES:BX and compares byte
ES:BX+04 with zero as signed. A negative byte branches to the final
test without advancing the pointer.

For a nonnegative byte it loads the current local offset into AX, adds
0014 to the local offset at word width and pushes the old AX. It reads
current DS:DD38 into AX, performs signed word multiplication by 0014,
then adds DBA8 to low AX at word width. The high product word in DX
is discarded by popping the old offset into DX. It compares that old
offset with the derived low-word bound unsigned. Strictly below loops
to reload and test the already advanced local pointer; equality or above
continues to the final test. The bound word is reread on every such
iteration, not copied once into a private immutable value.

The final test reloads ES:BX from the current local pointer and again
compares byte ES:BX+04 with zero as signed. A nonnegative byte
returns DX:AX zero. A negative byte returns the local segment and
offset. It restores SP from BP, pops BP and near-returns. There are
no calls or interrupts in this body, and no local store marks the selected
record as used. ES and BX are scratch results, not locally restored.

The final pointer can differ from the offset used in the bound comparison.
For example, if current DS:DD38 is zero and the first tested byte is
nonnegative, the derived bound is DBA8. Old offset DBA8 is not below
it, but the final test reads the advanced offset DBBC. This conditional
instruction path does not require a native observation to distinguish the
old comparison operand from the new access operand.

## Interpretation

This resolves one near callee left open by FND-EXE-360. It selects an
encoded pointer using a signed byte gate and a stride of twenty, rather
than making an allocation request or proving the selected record's extent.
The caller's zero-pair test distinguishes only this local result shape.

Q-EXE-007 retains the record/count writers, initial DS and storage
admission, segment/offset wrap, aliases and lifetime, the other near callee
1000:2AF6 and later pathname operations. The signed multiply does not
admit the count word as nonnegative, and retaining only its low product
does not establish a linear capacity bound. A mutable or aliased bound
must not be used as an independent traversal or writable-extent proof.
No complete-reading promotion, native result or execution exclusion follows.

## Alternatives

Treating the compared offset as the final accessed offset ignores the
preceding local increment. Treating the byte gate as unsigned ignores the
signed branch. Treating a returned pointer as a reserved record ignores
the absence of a marking store. Treating the derived bound as a checked
full-width size ignores discarded product bits and word-width addition.

## How to reproduce

At revision 910c4ff require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003FC7..0x00004011 at IP 2BC7, modeled CS 1000,
MZ header size 1400. Use FND-EXE-360's local call as the incoming
frame relation. Track the saved old offset, updated local pointer, signed
byte branches, full multiply versus retained low word, final reload and
both returned pairs. Trace count zero with a nonnegative first gate byte
as the stated conditional example; do not assert that native state occurs.
Source bytes and reports stay outside Git. No original process, DOSBox
or emulated call is executed.
