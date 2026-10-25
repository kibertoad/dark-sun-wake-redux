---
id: FND-RNG-083
title: Blocks from 4AB9:001C come from the far heap, whose base the startup code sets at least 0x200 bytes past DS:A4EC and which 1000:1831 never moves below that base
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:00AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:14C4..1000:1622
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:177C..1000:18BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:00FA..444C:0162
tool: Capstone 5.0.9 over the load image; tools/research/exec-census/direct_callers.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added, and `DS` is the data segment `57E0`.

**The allocation path.** `4AB9:001C` calls the far pointer `DS:3471` when it is not null, and
`444C:00FA` otherwise; FND-EXE-583 finds `DS:3471` staying 0. `444C:00FA` asks `1000:15A5` for the
request plus 1 byte and stores `0x77` in the last byte of the block it gets.

**`1000:15A5`** saves the caller's `DS` at `CS:1361`. For a request it rounds up to paragraphs,
it calls `1000:14C4` when the word `CS:135B` is 0, searches the free list from `CS:135F` when it is
not, and calls `1000:1528` when no free block is large enough. A free block that is larger is
split by `1000:1582`, which returns its upper part, and one that fits is unlinked by `1000:143B`
and returned whole. `1000:14C4` and `1000:1528` get new memory from `1000:1831` (with
`DS` set back from `CS:1361`) and store the block's paragraph count and link in its first four
bytes, returning offset 4 in the segment `1000:1831` returned.

**`1000:1831`** reads the far pointer `DS:00A2` (the current top), adds the request, and refuses
(-1) when the result is above `0xFFFFF`, below the far pointer `DS:009E` or above the far pointer
`DS:00A6`; otherwise it calls `1000:177C` with the new top and, on success, returns the old top.
`1000:177C` stores its argument at `DS:00A2`, after resizing the program's DOS memory block
through `1000:298E` when the top crosses a 1 KiB step, and stores the new end of that block at
`DS:00A6..00A8`. `direct_callers.py`
finds two callers of `1000:177C`: `1000:1821`, in `1000:17F2`, which makes the same two checks
first, and `1000:18AB` in `1000:1831`.

**The base.** `immediate_search.py` finds the words `DS:009E` and `DS:00A0` stored by the load
image only at `1000:00A3` (`DS:00A0`); its other hits, `1000:6623`, `6647`, `66DC` and `6717` and
the matching stores of `0x00A0`, are in routines that set `DS` to `CS` first. At startup
`1000:0000..00AB` sets `DS` to the data segment and computes, in paragraphs, `DI` = (`0xA4EC` plus
the word `DS:384E`, raised to at least `0x200`, plus the word `DS:37EC`) / 16 + 1, ending the
program when that overflows or exceeds the memory DOS gave it (`BP` less `DS`). When `DS:37EC` is 0,
`DI` becomes `0x1000`, or the whole memory when that is smaller, which the
previous check keeps at or above the computed value. It stores `DS + DI` at `DS:00A0` (the base's
segment, with offset `DS:009E` 0 from the image) and at `DS:00A4` (the top's segment).

## Interpretation

Every block `4AB9:001C` returns lies in memory `1000:1831` handed out, at or above the base, which
starts at least `0xA4EC + 0x200 + 0x10` bytes past `DS:0000`. The clock conversion's windows
(Q-RNG-005) end at `DS:A4EC`, so a write through a block's segment at any offset reaches none of
them. The driver images and the sequence blocks of FND-RNG-082 are such blocks.

## Alternatives

The release routine `1000:149B` and its helpers `1000:1367`, `1000:13CA` and `1000:1464` were read
only far enough to see that they link the freed block itself into the list; a freed pointer that
did not come from the heap could break this, and the callers of `1000:149B` were not checked.
`1000:298E` was not read.

## How to reproduce

Disassemble `1000:0000..00AB`, `1000:14C4..1622`, `1000:177C..18BC` and `444C:00FA..0162` from
the load image; `direct_callers.py <dsun> 1000:177C 1000:1831 1000:15A5`;
`immediate_search.py <dsun> 009E`, `00A0`, `00A2`, `00A4`, `00A6` and `00A8`.
