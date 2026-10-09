---
id: FND-EXE-515
title: Game allocator block producers publish headers after a sentinel-tested request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:21D2..1000:223B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:21D2..1000:223B
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-513's near helper 21D2 holds incoming internal size AX
on the stack, then calls near 0F68 with two zero words, removing
four argument bytes into BX. It masks returned AX with one. Nonzero
calls 0F68 again with push order zero, one and removes four bytes;
that result is ignored. It restores the held size into AX, holds it
again and calls 0F68 with push order zero, size, removing four bytes.
Only full returned AX FFFF takes the failure path, which drops held
size into BX and returns AX zero.

Every other result becomes BX and is stored in shared words DS:390C
and DS:390E installed or DS:3880 and DS:3882 on disc.
The held size is restored, incremented at word width and stored at
DS:BX. BX advances by four at word width and is returned in AX.
Zero is not rejected as a request result, and the returned offset can
wrap. The shared stores precede the header store. No allocation extent,
unit, segment or alias check occurs locally; preliminary request effects are
not rolled back on final failure.

The adjacent near helper 2212 holds incoming AX and calls 0F68
with push order zero, size, removing four argument bytes into BX.
Returned FFFF drops held size and returns zero. Other results become BX.
It reloads current shared word DS:390E installed or DS:3882 on
disc into AX, stores it at DS:BX+2, then publishes BX into that
shared word. It restores the held size, increments it and stores it at
DS:BX before returning BX+4 in AX at word width. These stores
also lack local bounds or alias checks. It does not locally update the
other shared word that 21D2 writes.

Both helpers return near without incoming cleanup, modify BX and AX and
rely on 0F68 for native or backing-storage effects and preservation of
other state. Their bodies contain no interrupt themselves. Both editions have
the same local control flow with the distinct shared-word offsets above.

## Interpretation

This resolves the wrapper's two remaining immediate block producers, including
the exact sentinel test, preliminary result disposal and ordered publication.
Q-EXE-007 retains 0F68's argument/return and storage contract, shared-state
and block writers, units and extents, actual DS/SS, aliases and lifetime,
214F callers, other initialization helpers and earlier startup/launch coverage.
No valid allocation or complete initialization contract is claimed.

## Alternatives

Treating every non-FFFF result as valid storage ignores zero and wrapped
offsets. Treating failure as transactional ignores preliminary request effects.
Treating shared-state publication as proof of a complete header ignores its
ordering before later stores and unadmitted aliases.

## How to reproduce

At revision d1af308 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
73D2..743B in sixteen-bit mode. Track held size through each push/pop,
the low-bit preliminary test, ignored second result, full FFFF sentinel,
edition-specific shared stores and later header/link stores. Keep request
results zero, FFFF and other offsets separate from admitted storage.
Licensed bytes stay outside Git; no game process, DOSBox or emulated call runs.
