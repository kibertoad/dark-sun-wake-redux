---
id: FND-EXE-493
title: Sound utility starts its null timer slot only through a stale driver timer handle
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:074C..1C08:0752
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:081E..1C08:0821
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:084C..1C08:093F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:093F..1C08:0990
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0B8C..1C08:0C60
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:146A..1C08:1502
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-491's timer dispatch calls a slot's far pointer only when the
slot's state word CS:[slot*2 + 006E] equals 2. Registration at 1C08:0781
takes the lowest of slots 0..15 whose state is 0 and sets it to 1. The
state words have these other writers in the image:

| Routine | Effect on the state word |
|---|---|
| 1C08:084C | slot argument FFFF or state 0: no change; otherwise sets 0 |
| 1C08:0895 | calls 1C08:084C for slots 15 down to 0 |
| 1C08:08B3 | sets 2 when the state is 1 |
| 1C08:08DB | calls 1C08:08B3 for slots 15 down to 0 |
| 1C08:08F9 | sets 1 when the state is 2 |
| 1C08:0921 | calls 1C08:08F9 for slots 15 down to 0 |
| 1C08:093F | saves the state, sets 1, updates the slot's period fields, calls 1C08:063C and stores the saved value back |

1C08:08DB has no near or far caller and no word equal to 08DB in the
image. 1C08:08B3 has three callers. The one at 1C08:081E passes 0010,
which no registration returns. The one in 1C08:08DB never runs. The one
at 1C08:0C1B passes the word at CS:01B2 when it is not FFFF.

CS:01B2 is written only at 1C08:0B8C, with FFFF, and at 1C08:0BD1, with
the handle that 1C08:0781 returns for the driver callback that
1C08:03BE finds for function 0067. That registration is skipped when the
lookup returns 0000:0000. The same handle goes to CS:[driver*2 + 0168].
1C08:074C and 1C08:0C53 release the handle held at CS:[driver*2 + 0168]
through 1C08:084C and do not write CS:01B2.

The null registration at 1C08:1485, inside 1C08:146A, stores the
returned handle at CS:0E7A and 0001 at CS:0E7C. CS:0E7A is read only at
1C08:14F1, which passes it to 1C08:084C after the far call through
CS:0E1E in the routine at 1C08:14CF.

## Interpretation

The null slot reaches state 2 only if 1C08:08B3 receives its handle.
Within the image, that can happen only at 1C08:0C1B with a CS:01B2 value
left from a driver timer whose slot was released and then reused by the
null registration, since registration takes the lowest free slot and the
releases leave CS:01B2 unchanged. Whether the utility's call order allows
that sequence is not read here. 1C08:093F restores the state it saved,
so it cannot move a slot into state 2 that was not already there, unless
1C08:063C changes that state while the slot is held at 1; that callee is
not read here.

## Alternatives

Treating the start-all routine as a live path ignores that it has no
caller. Treating 1C08:093F as a starter ignores that it writes back the
value it saved. Treating the handle 0010 as a slot ignores that
registration only hands out slots 0..15. Treating CS:01B2 as always
current ignores that both releases of the driver handle leave it as it
was.

## How to reproduce

Require FND-EXE-350's SOUND_DS.EXE identity: length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. With locked Capstone 5.0.7 in
sixteen-bit mode, decode load-image offsets 0xC7A0..0xC7D8,
0xC801..0xC8C0, 0xC89E..0xC8A1, 0xC8CC..0xCA10, 0xCC0C..0xCCE0 and
0xD4EA..0xD582 (segment 1C08 starts at load offset 0xC080; file offset is
load offset plus 0x1400). Search the image for `CS:`-prefixed
instructions addressing 006E+, 0168+, 01B2 and 0E7A..0E7D, for near and
far calls to load offsets 0xC8CC, 0xC933 and 0xC95B, and for words equal
to 08B3 and 08DB. Original bytes stay outside Git; no original process,
DOSBox or emulated call runs.
