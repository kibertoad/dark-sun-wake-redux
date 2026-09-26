---
id: FND-INPUT-005
title: The keyboard interrupt hook and the mouse event handler both send packets to 4464:0230
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0011..44B6:015A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0006..44D0:0095
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0230..4464:02B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4201:012C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`44B6:0011` sets `AH` to 2, executes `INT 16h`, clears `AH` and returns: it returns the BIOS
shift-key flags. `44B6:0018` saves the vector of interrupt 9 and installs `44B6:00B5` in its
place. That handler first calls the saved vector. Then, when the byte `DS:33D5` is 1, it reads the
shift flags; if they differ from the byte `DS:33D6` it stores them there and sends a packet built
from the old and new flags; otherwise, while `INT 16h` service 1 reports a waiting key, it reads
the flags with `44B6:0011`, takes the key with service 0 and sends a 10-byte packet of five words:
1, 10, 0, the key word `INT 16h` returned, and the flags.

`44D0:0006` registers `44D0:0053` with the mouse driver through `45B9:0122`, service `0Ch`, with
the event mask `0x7F`, and sets `DS:33D8` to 1. When `DS:33D9` is 1, the handler builds a 14-byte
packet of seven words: 2, 14, 0, `CX`, `DX`, the segment `57E0`, and the event bits the driver
passed in `AX`.

Both send the packet to `4464:0230`, whose third far caller is `4201:012C`, which sends a packet
starting 4, 14, 204. `4464:0230` copies as many bytes as the packet's second word gives into a
buffer when the word `DS:33CC` is 1. The buffer is set up by `4464:014E`, whose one far caller,
`39D1:0173`, passes a far pointer and the size 1,024.

## Interpretation

Keyboard and mouse events go into one queue of packets whose first word gives the kind: 1 for a
key, 2 for a mouse event and 4 for a third kind. The mouse packet carries the pointer position
and the event bits, and the key packet the BIOS key word and the shift flags.

## Alternatives

Earlier notes gave the buffer size as 1,040; the value passed at `39D1:0173` is 1,024. They did
not record the keyboard hook. Which routine reads the queue, and what the third packet kind is,
are not recovered.

## How to reproduce

Disassemble the ranges above with the relocations applied, and search the relocation table for far
calls to `4464:0230` and `4464:014E`.
