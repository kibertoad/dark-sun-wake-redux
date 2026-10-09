---
id: FND-EXE-564
title: Overlay 180 asks for an EMS overlay cache of up to 64 pages and, when that fails, an extended-memory one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067441..0x0006748E
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_cache.py)
environment: null
---

## Observation

Segment `4AE5` is descriptor 85 of the segment table: its word `+0` is `0x3AE5`. A search of
the 49 overlays' code for far calls (`9A`) to offset `0x08EB` or `0x0AB5` whose segment word is
85 times 8 (`0x02A8`) finds two, both in overlay 180, whose code starts at file `0x671E0`:

- At code offset `0x028D` (file `0x6746D`), after `push 0x00400000` (a 32-bit push) and
  `push 0`, a far call to `4AE5:08EB`. So the routine receives a handle of 0, a first page of 0
  and a page count of `0x0040`. After it, `add sp, 6`, `or ax, ax` and `je` to code offset
  `0x02A9`.
- At code offset `0x029F` (file `0x6747F`), after two 32-bit pushes of 0, a far call to
  `4AE5:0AB5`, followed by `add sp, 8`.

Both segment words, at code offsets `0x0290` and `0x02A2`, are in overlay 180's fixup list, so
the loader turns them into the segment of descriptor 85 (FND-EXE-520). Code offset `0x02A9`
continues with a far call through descriptor 37 (segment word `0x0128`).

No offset and segment pair in the relocated load image, whether a far call's operand or a
stored far pointer, names `4AE5:08EB` or `4AE5:0AB5`. The load image holds far pointers to
`4AE5:0D27` and `4AE5:0193` at `5B79:0006` and `5B7C:0000`.

## Interpretation

The game asks the overlay manager for an EMS cache with the EMS driver's free pages, at most 64
(1 MiB), and asks for an extended-memory cache with default bounds only when the EMS request
returns a nonzero result. With FND-EXE-563, the cache is never larger than the overlay pack.

## Alternatives

How overlay 180 is reached and whether this code runs on every start were not read. The
nearest trampoline targets in overlay 180 are `0x0111` and `0x0141`, before the calls. The
search covers direct far calls with the `9A` opcode only. A call through a stored pointer or a
computed segment would not be found.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_cache.py <install dir>/DSUN.EXE` from the
commit that adds this finding. It finds descriptor 85, searches every overlay's code for the two
far calls, checks each segment word against the overlay's fixup list, and disassembles overlay
180's code offsets `0x0261..0x02BD`. The stored-pointer search over the relocated load image
looks for the offset and segment pair of each manager routine at an address the MZ relocation
table names.
