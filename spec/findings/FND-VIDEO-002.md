---
id: FND-VIDEO-002
title: Overlay 196 plays an FLI file to the screen, one frame record every given number of 1 ms ticks, and stops after the header's frame count
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-VIDEO-008]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5755:0020..5755:004C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:077F..4842:09D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:24E4..57E0:24F7
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`. Overlay 196 has its header at segment `5755` and 1,106 bytes of
code from file offset `0x82F70`. Its entries at `5755:0025`, `5755:002F`, `5755:003E` and
`5755:0043` lead to code offsets `0x122`, `0x24F`, `0x40B` and `0x264`. In the order the play
entry runs them:

- Offset `0x264`, `DSUN.EXE+0x000831D4`, takes a far pointer to a file name, a byte `song`, a byte
  `ticks` and a double word `delay`. It calls offset `0x414`, calls `1BF3:2973` with `0x13`
  (FND-VIDEO-003), and calls offset `0x122` with the name and the 128-byte buffer at `DS:6299`,
  keeping the result in the word at `DS:6319`; when that is negative it returns 2. It calls
  `4A32:0011` with `song` (FND-SOUND-013). When the byte at `DS:1436` is not 0 it calls
  `2660:04F3` until the byte at `4E71:0C4A` is above `song` or 20,000 calls have been made. It
  calls offset `0x2F9` with `ticks` and `delay` and keeps the result, closes the file through
  `44DE:003A`, calls offset `0x443`, which was not read, calls `56EF:00F7`, which stops the music
  (FND-SOUND-008), and returns the kept result.
- Offset `0x122`, `DSUN.EXE+0x00083092`, opens the file through `44DE:0086` with mode 1 and
  returns -3 when that fails. It reads 128 bytes into the buffer; when fewer arrive or the word at
  offset 4 of the buffer is not `0xAF11` (the compare at offset `0x162`, `DSUN.EXE+0x000830D2`)
  it closes the file and returns -5, and otherwise it returns the handle.
- Offset `0x414`, `DSUN.EXE+0x00083384`, passes the far address `5755:003E` to `4842:077F` and
  keeps the result in the word at `DS:21E2`, then calls `4842:0992` with that word and 1,000 and
  `4842:08B1` with that word. Offset `0x40B` adds 1 to the word at `DS:21E0` and returns.
- `4842:077F` takes the first slot whose word at `CS:006C + 2 * slot` is 0, marks it 1, keeps the
  far address in it and sets its double word at `CS:00D2 + 4 * slot` to `0xFFFFFFFF`, and returns
  the slot. `4842:0992` divides 1,000,000 by its second argument and passes the slot and the
  quotient to `4842:093D`, which stores the quotient at `CS:00D2 + 4 * slot` and calls
  `4842:063A`. `4842:063A` is the address `4868:03DA`, the routine of FND-TIME-005 that sets the
  timer chip to the shortest period of the slots in use.
- Offset `0x2F9`, `DSUN.EXE+0x00083269`, the frame loop, takes `ticks`, taken as 1 when it is 0,
  and `delay`. When `delay` is not 0 it sets `DS:21E0` to 0 and waits until the word is at least
  `delay`, giving up after 50,000 passes if the word is still 0. It then reads a record through
  offset `0x24F`; when that fails it calls `1000:1C32` and returns 3. After the first record it
  keeps a count of records read, starting at 1, and a carry, starting at 0, and repeats:
  1. The wait is `ticks` less the carry, and the carry becomes 0; when the carry is larger than
     `ticks`, the wait is 0 and the carry drops by `ticks`.
  2. It sets `DS:21E0` to 0 and waits until the word reaches the wait, sets it to 0 again and
     reads the next record through offset `0x24F`. When that fails it returns 3, without the
     call to `1000:1C32`.
  3. It adds 1 to the count and returns 4 when the count has reached the word at `DS:629F`,
     offset 6 of the header buffer. It adds the word at `DS:21E0`, the ticks the read took, to the
     carry.
  4. It calls `44B6:0011`, which returns the BIOS shift-key flags (FND-INPUT-005); when the
     result has bit 0 or bit 1 set, and the word at `DS:631B` is
     not 0 or the count is above 240, it returns 1.
- Offset `0x24F`, `DSUN.EXE+0x000831BF`, calls offset `0x17B` with its handle, the far address
  `DS:24E4` and 1. Offset `0x17B` reads 16 bytes; when fewer arrive it returns -8, and when the
  word at offset 4 is not `0xF1FA` it returns -6. When the double word at offset 0 is 16 it
  returns 0. Otherwise it allocates that size less 16 through `444C:00FA`; when that fails it
  formats "Can't find %ld bytes" (`DS:24FE`) into a local buffer and returns -2. It reads that
  many bytes, returning -8 when fewer arrive, calls offset `0x000` with `DS:24E4`, the 16 bytes,
  the data and the 1, frees the data and returns 0.
- Offset `0x000`, `DSUN.EXE+0x00082F70`, visits as many chunks as the word at offset 6 of the 16
  bytes, each at the previous one plus the previous one's double word size, and passes the data
  after each chunk's 6-byte head on by the word at offset 4 of the chunk, through the table at
  offset `0x116`:

  | Type | Action |
  |---|---|
  | `0x0B` | when the last argument is not 0, calls `57D7:0020` with the data, then `57D4:0020` with the data and the far pointer at `DS:24F2` |
  | `0x0C` | calls `57DD:0020` with the data and the far pointer at `DS:24EE` |
  | `0x0D` | fills 64,000 bytes at the far pointer at `DS:24EE` with 0 through `1000:3FA2` |
  | `0x0E` | nothing |
  | `0x0F` | calls `57DA:0020` with the data, the far pointer at `DS:24EE` and the word at `DS:24EA` |
  | `0x10` | copies 64,000 bytes from the data to the far pointer at `DS:24EE` through `1000:3F5A` |

  Other types do nothing.

The 20 bytes at `DS:24E4` hold a double word 0, the words 320, 200 and 320, the far pointer
`A000:0000`, a far pointer, and the word 64,000.

In the whole of `DSUN.EXE` the bytes `11 AF` occur three times: at file offset `0x830D6`, inside
the compare above, and at `0x5EB87` and `0x89110`, which lie outside decoded instructions. An
earlier search of the load image alone, the resident code, found none. No instruction of overlay
196 reads the header buffer except through the far pointer at offset 4 and at `DS:629F`.

## Interpretation

Overlay 196 is the game's FLI player. It switches the screen to BIOS mode `0x13`, starts a timer
slot of the library RULE-TIME-002 describes with a period of 1,000 microseconds, so a tick of
`DS:21E0` is 1 ms,
starts song `song`, and when `DS:1436` is set waits for the music to start. It waits `delay`
milliseconds, shows the first frame record, then one record every `ticks` milliseconds, counting
the time spent reading a record against the next wait. It shows as many records as the header's
frame count, so the record past the count (FND-VIDEO-001) is never read. Holding either Shift key
(bit 0 is the right one, bit 1 the left) ends it early when `DS:631B` is set or after 240 records;
no other key is read. The header's speed field is not read: `ticks`
comes from the caller. The chunk types match the published FLI types: `0x0B` sets the palette,
`0x0C` and `0x0F` decode deltas and runs onto the screen, `0x0D` clears it and `0x10` copies a
whole frame. The results are 1 for Shift, 2 when the file cannot be opened, 3 on a read error and
4 at the end.

## Alternatives

The routines at `57D4`, `57D7`, `57DA` and `57DD`, `4842:08B1`, `2660:04F3`,
`1000:1C32` and offset `0x443` were not read, so that they set the palette, decode, start and
remove the timer slot, and wait for the disc audio is inferred from
how they are called. That the library calls the far routine once per period of its slot is read
from FND-TIME-005's account of the slots, not from the interrupt handler. What `DS:1436` and the
far pointer at `DS:24F2` hold was not read.

## How to reproduce

Find overlay 196 through the overlay table, disassemble its 1,106 bytes from file offset
`0x82F70`, resolving far calls through the segment table at `0x4B080`;
disassemble `4842:077F` to `4842:09D2`; read the 20 bytes at file offset `0x4F4E4`; search the whole file for `11 AF`.
