---
id: FND-PARTY-024
title: The view-mode word at DS:0DAB and the word at DS:0D9C start at 0, and the routines that can make them nonzero first
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0D9C..57E0:0DAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B1B:0002..2B1B:0015
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069FF8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A0A6..0x0006A409
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A0B..0x00059C45
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of the resident image and overlays 172 and 182; Python 3.14.7 scans of DSUN.EXE's raw bytes for direct-address operands
environment: null
---

## Observation

Both words are in the data segment `57E0` (FND-CONFIG-005). In the load image the word at
`DS:0DAB` (file offset `0x0004E1AB`) and the word at `DS:0D9C` (file offset `0x0004E19C`) are 0.

**Direct accesses.** Every place in the file where the two bytes of the offset follow a byte that
decodes, with them, as an instruction with that direct operand was decoded. For `DS:0DAB` there
are 42 such instructions; the stores are:

| File offset | Where | Store | Routine |
|---|---|---|---|
| `0x000203C0` | `2B1B:0010` | the routine's word argument | `2B1B:000A` (file `0x000203BA`), a setter |
| `0x00069C6E` | overlay 182 `0x141E` | 1 | overlay 182 `0x1167` |
| `0x00069E1F` | overlay 182 `0x15CF` | 0 | overlay 182 `0x1525` |
| `0x00069E5C` | overlay 182 `0x160C` | 4 | overlay 182 `0x15D7` |
| `0x00069FA9` | overlay 182 `0x1759` | 1 | overlay 182 `0x15D7` |
| `0x0006A233` | overlay 182 `0x19E3` | 2 | overlay 182 `0x1856` |
| `0x0006A352` | overlay 182 `0x1B02` | 1 | overlay 182 `0x19F8` |

The other 35 compare it or, at `2B1B:0005` (file `0x000203B5`), load it into `ax` in a getter
that returns it. For `DS:0D9C` the stores are 1 at overlay 182 `0x1233` (in `0x1167`, before the
gate at `0x12DD`), 0 at `0x158D` (in `0x1525`), 0 at `0x1672` and 1 at `0x1745` (both in
`0x15D7`), 0 at `0x19B0` (in `0x1856`) and 1 at `0x1AD5` (in `0x19F8`); the other three
accesses are compares.

**Overlay 182 `0x1856`** (trampoline `56BD:00B6`) returns at once, at `0x19F6`, when `DS:0DAB`
is 0 or 2. Every call and store it makes comes after that test.

**Overlay 182 `0x19F8`** (trampoline `56BD:00BB`) stores 0 to the bytes at `DS:0DA4` and
`DS:0DA3`, and then, unless `DS:0DAB` is 2, skips to `0x1A79`; there, unless the word is 2 or 3,
it jumps to its return at `0x1BB8`. Its store of 1 to `DS:0DAB`, its store of 1 to `DS:0D9C` and
all its far calls lie on the path where the word is 2 or 3.

**The setter `2B1B:000A`** is called only from overlay 204, at `DSUN.EXE+0x0008D08D` and
`0x0008D0D9`, in the routines at overlay 204 offsets `0x12B4` and `0x12F4`; a second search of
the whole file for every `9A` far call and every near call whose target is the setter, independent
of function boundaries, finds no others.

**Overlay 172 `0x05DB`** (trampoline `566A:002A`), which shows a message, calls the getter
`28C9:2522` (the same code as `2B1B:0002`); unless it returns 2, it sets the byte at offset 6 of
the segment whose segment-table word is `0x0300` to 1 and calls `0x1856`. Its closing routine at
overlay 172 `0x0746` calls `0x19F8` when that byte is 1.

## Interpretation

While `DS:0DAB` is 0, neither `0x1856` nor `0x19F8` changes it or `DS:0D9C`, so a message shown
before any of the other writers has run leaves both words at 0. The first routine to make
`DS:0DAB` nonzero is therefore the setter (from overlay 204), overlay 182's `0x15D7`, or the gate
routine `0x1167` itself after its gate; the first to make `DS:0D9C` nonzero is `0x15D7` or
`0x1167`. The values 1, 2 and 4 name states of the game's display that this finding does not
identify, so the word keeps a neutral description.

## Alternatives

- `DS:0DAB` or `DS:0D9C` is written in some other way: not ruled out for stores through a base
  or index register, string instructions or a segment register other than DS holding `57E0`,
  which this scan does not see.
- `0x1856` or `0x19F8` changes the words when `DS:0DAB` is 0: ruled out; both test the word
  before any store or call that could.

## How to reproduce

Read the words at file offsets `0x0004E1AB` and `0x0004E19C`. Search the file for `AB 0D` and
`9C 0D`, and decode an instruction starting one to four bytes before each hit, keeping those
whose operand is the direct address. Disassemble overlay 182 from `DSUN.EXE+0x0006A0A6` to
`0x0006A409` and overlay 172 from `0x00059A0B` to `0x00059C45` as 16-bit code, and the resident
bytes from `0x000203B2` to `0x000203C5`.
