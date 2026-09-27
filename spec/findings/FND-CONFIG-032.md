---
id: FND-CONFIG-032
title: Shipped message controls bypass image registration failures
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0006
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0008
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:000B
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident routines; DarkSunWakeRedux.Inspect ui-catalog
environment: null
---

## Observation

The common resident routine at `3CFA:0006` examines two image-number
fields at offsets `0x20` and `0x28` of the record pointer supplied to it.
It requests a `BMP ` only when a field is nonzero. A missing or rejected
image makes it return `0xFFFF`; when both fields are zero it reaches its
zero return without requesting an image.

The button registration routine at `3EBE:0008` sets its position and calls
that routine with a pointer to button offset `0x0C`. It propagates a nonzero
result as `0xFFFF`. It separately requests an `ICON` only when its field at
button offset `0x64` is nonzero. Its other branches end at a zero return.
The application-frame registration routine at `3F96:000B` likewise sets
its position, calls `3CFA:0006` with a pointer to frame offset `0x0C`,
and maps a nonzero result to `0xFFFF` and zero to zero.

The installed `WIND/10501` has image number zero and children `BUTN/10309`
and `APFM/11270` (FND-CONFIG-030, FND-CONFIG-031). Its button has image
number zero, no tail and mask zero; the frame is 4 by 4 with mask zero
(FND-UI-004, FND-UI-005). For these three records, the bytes corresponding
to the common routine's two image-number fields are zero in the shipped
resource (FND-UI-002, FND-UI-004, FND-UI-005).

## Interpretation

With these shipped records loaded intact, the window and its two children
do not require images during registration. The two child registration
helpers return zero on that path. The registration routine can still fail
earlier if a window or child resource cannot be acquired, or if the
runtime position/bounds test fails (FND-CONFIG-030, FND-CONFIG-031).

## Alternatives

This reading does not establish whether all three resources are acquired
in every live state or what values the runtime bounds globals hold then.
It does not establish which text-message callers reach the acquisition
path. An unexpected modification of the loaded records before
registration would also require a separate reading; no such modification
was identified in these bounded paths.

## How to reproduce

For the approved `DSUN.EXE`, disassemble file offsets
`0x000321A6..0x00032279` (`3CFA:0006`),
`0x00033DE8..0x0003420A` (`3EBE:0008`) and
`0x00034B6B..0x00034BC7` (`3F96:000B`) in 16-bit mode. Follow their
nonzero return branches and the image-field tests. From the approved
`RESOURCE.GFF`, read `WIND/10501`, `BUTN/10309` and `APFM/11270`
through their documented layouts and compare the image fields with those
tests. The read-only UI catalog reports the three identities and the
button's zero image number.
