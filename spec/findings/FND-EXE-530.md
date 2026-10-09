---
id: FND-EXE-530
title: Game early startup scans fixed slots and writes segment-selected metadata before bounded quantity calculation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:029B..4AE5:031B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:029A..4AD6:031A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07AD..4AE5:07C5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 4AD6:07AC..4AD6:07C4
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-529's first near startup callee sets ES from an immediate,
reads ES word 0090, adds 0010 at word width and stores the
result into CS word 000E installed or 000D on disc. It then
sets ES from a second immediate, zeros BX and DI, and starts SI
at 01A0. It does not save incoming BX, DI, SI or ES.

At each slot it tests DS word SI+4 for bit two. Clear skips
the slot; set additionally requires DS word SI+2 nonzero. Admission
loads AX from DS word SI, pushes current ES, stores AX at
that ES word 0012 and replaces ES with AX. It reads the selected
segment's byte 001A. FF restores ES from the stack, clears restored
ES word 0012 and advances to the next slot.

Every other byte instead pops the old ES into AX, leaving ES as
the selected segment. It stores word 04C6 installed or 04C5 on
disc at selected ES:0018. It then reads DS words 0114 and
0116 into AX and DX, adds AX into selected ES word four
and adds DX with carry into selected ES word six. These are ordered
word stores, with no local admission of segment storage or its relationship
to the table, stack or code. The next slot begins with this selected ES,
rather than unconditionally resetting ES to the second immediate segment.

It calls near 07AD installed or 07AC on disc. That helper
sets CL to four, reads ES word eight into AX, adds 0011 at
word width and shifts AX right logically by four. It reads ES word
ten into DX, adds 000F at word width, shifts DX right logically
by four and adds AX into DX at word width. Both additions before
shifting wrap without carry tests. Each shifted term is at most 4095,
so returned DX is at most 8190 independently of the input words.
This is not an unconditional unbounded rounding-up formula. The helper
returns near without cleanup, changes AX/DX/CL and does not change
BX, SI, DI or any segment register; it makes no calls or interrupts.

The caller compares held BX unsigned with returned DX. Larger DX
replaces BX by exchange; otherwise BX remains. Every slot then advances
SI by eight at word width and repeats while SI is unsigned below
08C8. Starting at 01A0, intact normal execution visits 229 slots
and finishes at SI 08C8. Its only callee preserves the scan registers;
no native call or locally unbounded callee alters that numerical traversal.
This count does not prove accessible table or selected-segment extents.

At the end it clears AX, adds two to BX, stores BX at DS:011A
and returns near without incoming cleanup. On admitted normal return,
BX is two plus the largest computed DX among admitted non-FF slots,
or two if there were none; its bound is 8192. DS is not locally
changed. DI remains zero and SI is not restored. ES is the last
retained selected segment, or the initial second segment if no selection
was retained. Writable aliases and intact return-frame assumptions remain
unadmitted, so these are local execution contracts rather than storage proofs.

MZ relocation index 4426 installed or 4413 on disc identifies the
first immediate word at relative 3AE5:029C or 3AD6:029B,
with shipped values 47E0 or 47D7. Index 4427 or 4414
identifies the second at 3AE5:02AC or 3AD6:02AB, with
values 45DF or 45D0. With modeled load segment 1000 these
become ES 57E0/57D7 first and 55DF/55D0 second.
The per-slot ES comes from the DS table word, not either immediate.
Both editions share the local control flow with the listed operand differences.

## Interpretation

This resolves the first immediate startup helper and its sole arithmetic
callee, including a concrete producer of the DS:011A word consumed
by FND-EXE-529. It establishes local traversal and output bounds without
claiming units, valid segment descriptors or writable extents. Q-EXE-007
retains table/field producers and consumers, actual DS and per-slot segments,
aliases and lifetime, the other startup callees and their cleanup contracts,
native dependencies and broader launch coverage. No complete startup or
launch-exclusion claim is made.

## Alternatives

Restoring ES after every slot contradicts the admitted branch's pop into
AX. Calling the arithmetic ordinary rounding up ignores addition wrap.
Using fixed traversal as storage admission ignores segment-selected writes.
Assuming saved scan registers contradicts their initialization and absent saves.
Treating DS:011A as an unconstrained output ignores the helper's independent
term bounds and the caller's maximum and final addition.

## How to reproduce

At revision 535c9c9 require both DSUN.EXE identities from FND-EXE-350.
Use header size 5200 and modeled load segment 1000. Decode relative
segments 3AE5/3AD6 at the offsets in Locations in sixteen-bit mode.
Read the MZ relocation table from header word 0018, count at 0006,
and four-byte offset/segment records; check the two indices and operand
words above. Follow both admission tests, every ES push/pop destination,
ordered metadata stores, the sole near callee and its register writes,
word-width pre-shift additions, unsigned maximum, scan endpoint and final
store. Use FND-EXE-529 for the returning consumer. Keep storage and
segment admission explicit. Licensed bytes remain outside Git; no original
process, DOSBox or emulated call runs.
