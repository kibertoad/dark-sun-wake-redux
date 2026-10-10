---
id: FND-PARTY-078
title: The word at offset 0x12 of a window event is the mouse driver's event bits from the mouse packet, so the generation screen's step buttons go forward for the left button and back for the right or middle one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0053..44D0:009E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0382..4464:041F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:064A..39D1:071F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:071F..39D1:0752
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0752..39D1:0854
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0854..39D1:0913
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D..39D1:0BD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006AFE0..0x0006AFF3
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. A window handler takes the 24-byte event as its
arguments, so its `[bp + 6]` is the event's offset 0 and `[bp + 0x18]` its offset `0x12`.

**The mouse packet.** The mouse event handler `44D0:0053` (FND-INPUT-005) copies `AX`, the event
bits the driver passes, into `DI`, loads `AX` with `0x57E0` for `DS`, and builds seven words on
its stack: 2, 14, 0, `CX`, `DX`, `AX` (now `0x57E0`) and `DI`, so the driver's event bits are at
offset `0x0C` of the 14-byte packet (`+005F..+0088`). It passes the packet to `4464:0230`. The
keyboard hook's packet is 10 bytes: 1, 10, 0, the key word and the shift flags (FND-INPUT-005).

**Reading a packet.** `4464:0382` takes a far buffer. When the queue is not empty it copies the
packet at its head into the buffer, byte by byte, as many bytes as the packet's second word
gives, and advances the head by that number (`+039A..+0400`).

**Building the event.** `39D1:097D` takes a window and a far pointer to a 24-byte event. It stores
2 at offset 0, 0 in the dword at `0x14` and in the words at 2 and 4, calls `4464:0382` with a
14-byte buffer on its stack, and copies the 14 bytes of that buffer to offset 6 of the event with
`1000:0452` (`+099B..+09DE`). For a mouse packet (kind 2) it stores at offset 2 the result of
`3D72:0009` for the buffer and goes on only when that is nonzero, then stores the word at `DS:A193`
at offset 4 (`+0B9F..+0BC8`); it leaves offset `0x12` as copied. For a key packet (kind 1) it
either leaves the event kind at 2 with the button number in `DS:A101` at offset 2 and a double word
from `DS:A17F` at offset `0x14`, when `409B:0D48` matches the key (`+0A22..+0A86`), or makes the
kind 6 (`+0B09..+0B1A`); in both the 14 bytes copied end with the four bytes `4464:0382` did not
write for the 10-byte packet.

**Delivering it.** `39D1:064A` calls `39D1:0854` with a 24-byte buffer on its stack
(`+06CA..+06D7`); `39D1:0854` takes a packet with `4464:02FE`, passes it with the event pointer to
`39D1:097D` (`+08B2..+08BC`), and passes the event by value to `39D1:071F` (`+08EF..+08FE`), which
passes it by value to `39D1:0752`; that routine calls the window's handler at `+0xF5` of the
window record, or the handler at `DS:A0F1`, with the 24 bytes (`+07D2..+0802`, `+0822..+083B`).
`39D1:071F`'s other direct callers are in overlays 172, 188, 199 and 209 and `27D0:0006`.

So for a mouse press the event's word at offset `0x0C` is the pointer's horizontal position, the
key word the handler 184 `+0DA7` tests for event 6 (FND-PARTY-070), and the word at `0x12` the
driver's event bits.

**The step.** Overlay 183 `+0000` returns 1 when the word at `[bp + 0x18]` is below 8 (unsigned)
and -1 otherwise (`+0003..+0011`, FND-PARTY-070).

## Interpretation

The word overlay 183 `+0000` tests is the event mask the mouse driver passes to a handler
registered through interrupt `33h` service `0Ch`: bit 0 for movement, bits 1 and 2 for the left
button going down and up, bits 3 and 4 for the right button and bits 5 and 6 for the middle one
(the game registers with the mask `0x7F`, FND-INPUT-005). A value below 8 has only movement or left
button bits, so the left button steps the portrait, score, hit point and alignment buttons
forward and the right or middle button steps them back.

A button pressed through a key gets in that word two bytes the key packet does not carry, left in
the buffer from before, so its step depends on what that stack space held.

## Alternatives

- The meaning of the bits comes from the mouse driver's interface, not from the game's code; a
  capture of a score after a left and after a right press would confirm it.
- Whether any of the generation screen's step buttons can be pressed by a key, through the table
  `409B:0D48` reads from `DS:A17F`, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun> 44D0:0053..44D0:00A0
4464:0382..4464:041F 39D1:064A..39D1:0752 39D1:0752..39D1:0854 39D1:0854..39D1:0960
39D1:097D..39D1:0BD3`; `overlay_listing.py <dsun> 183 6AFE0 6AFF3`; and `direct_callers.py <dsun>
39D1:0752 39D1:071F`.
