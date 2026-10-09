---
id: FND-EXE-481
title: Sound utility formatter flush callback advances destination without a capacity check
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0F25..1000:0F3F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0F4C..1000:0FD8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1398..1000:13B8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:390E..1000:394E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-480's 394E wrapper supplies the near callback offset 390E
and a far descriptor naming its destination argument on the wrapper's stack.
The formatter at 0F25 establishes BP, reserves 0096 bytes and saves SI
and DI. It initializes accumulated word SS:BP-12 and failure word
SS:BP-16 to zero, and word SS:BP-14 to 0050. Its entry continuation
saves ES, clears direction and places the output cursor at SS:BP-96.
It reads format bytes from the incoming pair at SS:BP+6. Zero branches
to 1398; an ordinary non-percent byte is stored through SS:DI, advances
DI and decrements byte SS:BP-14. Positive signed remaining byte loops;
otherwise it calls the local flush at 0F55. A percent begins conversion
handling outside this reading. The local output helper at 0F4C similarly
stores a byte and advances DI, but flushes when its decremented byte is zero.

The flush saves BX, CX, DX and ES. It subtracts offset BP-96 from
current DI at word width and passes source SS:BP-96, that difference as
count, and the incoming descriptor pair from SS:BP+0A to the near target
in word SS:BP+0E. Full AX zero sets failure word BP-16 to one;
nonzero does not clear a previous failure. It resets word BP-14 to 0050,
adds current DI to accumulated word BP-12 and resets DI to BP-96.
It restores ES, DX, CX and BX and near-returns. There is no local branch
that stops formatting on callback failure. Retained DI and argument state
require callback preservation and nonaliased frames; no output total bound
is established merely by the local chunk size.

Callback 390E saves BP and SI, retains its incoming count in SI and
passes count, incoming source pair and the current pair in the incoming
descriptor to FND-EXE-361's 30EB copy helper. Its local push-CS/near-call
sequence gives that far-returning helper the current CS. After removing
ten argument bytes it reloads the descriptor, adds SI to the destination
offset at word width without updating its segment, reloads the resulting
pair and writes a zero byte there. It returns AX equal to SI, restores
SI and BP, and near-returns while removing ten incoming bytes. It tests
no extent, overlap or copy result. The callback writes both destination
data and descriptor state; aliases with its own or its caller's frame are
not admitted by this local reading.

At 1398 the formatter compares byte BP-14 signed against 50 and
flushes when it is less. It restores entry ES, returns AX FFFF when
failure word BP-16 is nonzero, otherwise returns accumulated word BP-12.
It restores DI and SI, resets SP from BP, restores BP and near-returns
while removing twelve incoming bytes. This accounts for the wrapper's
six pushes before its near call. The wrapper's zero destination store is
not proof of later capacity or successful terminated output.

## Interpretation

This binds one supplied callback to the local formatter flush and accounts
for nested argument cleanup. Under admitted state, 30EB preserves the
callback's retained count and DI, and the callback advances the output
descriptor before appending zero. Its count return is not an independent
copy-success signal. Q-EXE-007 retains conversion dispatch, input and
descriptor producers, source and destination extents, aliases, total output
bounds and all other formatter callers. No complete formatter reading,
successful configuration or launch exclusion is claimed.

## Alternatives

Treating the 80-byte local chunk as a destination capacity confuses two
allocations. Treating nonzero callback AX as validated output ignores its
unconditional count return. Treating recorded callback failure as an immediate
stop contradicts the local reset and continuation. Treating descriptor offset
addition as linear far-pointer progress ignores unchanged segment and wrap.

## How to reproduce

At revision 176f8e3 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped half-open ranges
2325..233F at IP 0F25, 234C..23D8 at IP 0F4C,
2798..27B8 at IP 1398 and 4D0E..4D4E at IP 390E,
modeled CS 1000. Recheck FND-EXE-361's 30EB copy for preserved
SI/DI and its incoming cleanup contract. Track each nested push, near/far
return, byte-versus-word state access, descriptor reload and offset addition.
Licensed bytes stay outside Git; no original process, DOSBox or emulated call runs.
