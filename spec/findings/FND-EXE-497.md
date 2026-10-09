---
id: FND-EXE-497
title: Sound utility asks installed drivers only for functions 0064 to 0067
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:03BE..1C08:040E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0B75..1C08:0B7B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0C59..1C08:0D80
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-491's driver lookup 1C08:03BE takes a driver slot in BX and a
function number in AX, walks the slot's number/offset table, and returns
the matching far pointer or 0000:0000. The dispatcher 1C08:03F6 takes the
slot from the stack, calls the lookup, and jumps to the result with
`push dx; push ax; retf` when it is not 0000:0000.

The load image holds 50 stubs of the form `mov ax,number; jmp
1C08:03F6`, at 1C08:0B75 for function 0065 and from 1C08:0C59 to
1C08:0D7A for 0068, 0078..0086, 0096..009F, 00AA, 00AB, 00AD..00B7 and
00B9..00C2.
The stub for 0065 has two far callers, at load offsets 0x3EE2 and
0x4065. No other stub has a far or near caller. No stored seg:off pair
resolves to the stubs for 00BD or 00BE.

Besides the stubs, the dispatcher has two near callers: 1C08:0B6D after
`mov ax,0064` and 1C08:0BFB after `mov ax,0066`. The lookup has two near
callers: the dispatcher at 1C08:03FD and 1C08:0BB1 after `mov ax,0067`.
Neither the dispatcher nor the lookup has a far caller. The one stored
pair that resolves to the lookup's address, at load offset 0x440D, is
the bytes of `push ds; mov ax,00C2` in segment 1000.

The only instructions in the image with immediate 00BD or 00BE are the
two stubs' own `mov ax` at 1C08:0D5C and 1C08:0D62.

## Interpretation

The sound utility asks an installed driver for functions 0064, 0065,
0066 and 0067 only. It never asks for function 00BD, the music drivers'
callback registration in FND-EXE-496, so their callback slots keep the
zero bytes they ship with, and every call through those slots is
skipped. The other stubs are present but unreachable through any
reference this search covers.

## Alternatives

Reading the 00BD or 00BE stub as reachable through a far pointer stored
in data needs a seg:off pair resolving to it, and none does. Reading AX at
the dispatcher as computed ignores that every branch into the dispatcher
and the lookup follows an immediate `mov ax`. Transfers built on the
stack, or code written at run time, are not covered by this search.

## How to reproduce

Run `python -I tools/research/exec-census/sound_ds_driver_function_census.py
<install dir>/SOUND_DS.EXE` from the commit that adds this finding, with
the locked evidence Python (Capstone 5.0.7, xxhash 4.0.1). It requires
FND-EXE-350's identity (length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57) and searches the load image (file
0x1400..0x1E5E0; segment 1C08 starts at load offset 0xC080) for the
stubs, for far calls whose seg:off resolves to each stub, near calls
inside segment 1C08, stored seg:off pairs, branches to load offsets
0xC476 and 0xC43E, and instructions whose last operand is 00BD or 00BE.
Original bytes stay outside Git; no original process, DOSBox or emulated
call runs.
