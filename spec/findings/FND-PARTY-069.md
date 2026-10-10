---
id: FND-PARTY-069
title: Segment 4E68 is a 140-byte data segment that only overlays 183 and 184 name, and none of their 54 references writes its first 16 bytes, the origin class table; the only writes go to two 10-word arrays at 0x64 and 0x78
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004B3E0..0x0004B3F0
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DA72..0x0006DB39
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E22D..0x0006E279
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DB92..0x0006DB9F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D6B1..0x0006D6C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B756..0x0006B75E
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/segment_references.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E68` holds the origin class table in its first 16 bytes (FND-PARTY-068).

**Its size.** The rows of 8 bytes at file `0x4B3E0` and `0x4B3E8` name segments `0x3E4F` and
`0x3E68` (that is `4E4F` and `4E68` less `0x1000`) with sizes 398 and 140 bytes; segment `4E4F`
ends at its offset `0x18E`, before `4E68` begins at `4E4F:0190`.

**Who names it.** `segment_references.py` with `4E68` finds 55 words naming the segment other
than far calls and jumps: the row at `0x4B3E8` in the load image and 54 FBOV fixups, 25 in overlay
183 and 29 in overlay 184, each the word `0x0368` of a `mov ax, 0x368` or `mov dx, 0x368`. The
read in overlay 183 `+0AF7` (FND-PARTY-068) is among them. In each of the 54 the next instruction
or the one after it moves the register into ES, and no fixup moves it into DS or SS or pushes it.

**What they write.** Going over up to 26 instructions from each of the 54, and stopping at a
`ret`, `retf` or `jmp`, the instructions that write through ES are:

- `mov word ptr es:[bx + 0x64], ax` and `mov word ptr es:[bx + 0x78], ax` in overlay 184 at
  `+0A2C` and `+0A77`, in a loop with `bx` twice `si` and `si` from 0 while less than 10
  (`+09E2..+0AA6`);
- the same two stores at `+11BC` and `+11DE`, in a loop with `bx` twice `di` and `di` from 0
  while less than 10 (`+119D..+11E7`);
- `mov byte ptr es:[bx + 0x21]` at overlay 184 `+0625` and `+0631` and `mov word ptr es:[bx +
  2]` at overlay 183 `+077A`, each after `les bx` loads ES from `DS:1429` or `DS:142D`
  (overlay 184 `+0621`, overlay 183 `+0776`).

Every other access of the 54 reads: `push word ptr es:[N]`, `push word ptr es:[bx + N]`, `mov al,
byte ptr es:[si + N]`, `mov al, byte ptr es:[bx + 0x16]` and `cmp word ptr es:[bx + N], -1`.

## Interpretation

Nothing in the executable writes the origin class table at `4E68:0000`: the only stores into the
segment go to offsets `0x64` to `0x8B`, the two arrays of ten words that end the segment's 140
bytes. The generation screen therefore offers each origin the classes the shipped table gives.

## Alternatives

- The search covers references by segment word: MZ relocations, FBOV fixups, and the
  instructions after each. It does not cover a store through another segment with an offset past
  that segment's end, such as `4E4F:0190`, or a segment value computed at run time; no such access
  was seen, and the 398-byte size of `4E4F` puts its own fields below `0x18E`.
- A write more than 26 instructions after a fixup, with ES still holding `4E68`, would be missed;
  every access found reloads ES right before it.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `segment_references.py <dsun> 4E68`;
`overlay_listing.py <dsun> 184 6DA40 6DBB8`, `184 6E1F0 6E2A0`, `184 6D660 6D6C8` and `183 6B6F0
6B780`. Read the 8-byte rows at file `0x4B3E0` and `0x4B3E8` as four words each. For each fixup
`segment_references.py` prints, decode from the instruction holding the word up to 26
instructions, stopping at `ret`, `retf` or `jmp`, and list each instruction whose destination
has an `es:` override or that is a string store.
