---
id: FND-PARTY-027
title: Startup holds a full-screen reservation of the video-memory pool, which leaves 1,067 paragraphs for later reservations
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067673..0x0006785F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x000679AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:0337
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1038:0008..1038:009A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:13FE..57E0:13FF
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of overlay 180; Python 3.14.7 scans of DSUN.EXE for direct-address operands and for far and near calls at every byte offset; the call graph of FND-PARTY-026
environment: null
---

## Observation

Overlay 180's routine at `0x0141`, which the program's main routine calls before the start
window (FND-PARTY-026), calls the pool setup `1BF3:271B` (FND-PARTY-025) at overlay 180 offset
`0x0499` (`DSUN.EXE+0x00067679`). Later in the same routine, at `0x066D`
(`DSUN.EXE+0x0006784D`), it reserves the rectangle (0, 0, 319, 199) with `1BF3:27A8`: 80 × 200 =
16,000 bytes, 1,000 paragraphs, the largest size the routine accepts. It stores the entry number
at `DS:13FE`, and when the result is `0xFFFF` it returns 0 at once.

**Release.** The only direct store to `DS:13FE` other than that one is at overlay 180 `0x07C5`,
which writes `0xFFFF` after passing the entry to the release routine `1BF3:28C5`, in the routine
at overlay 180 `0x0762` (trampoline `56B2:0025`). A search of every byte offset of the file for a
far or near call to that routine finds none. The routine at `0x0141` instead pushes the segment
word `0x05A0` (a fixup, overlay 180's header) and `0x0025` at its start and calls `1000:030B`,
which appends the far pointer to a table of 32 far pointers at `DS:A446`, counting them in the
word at `DS:3572`, and returns 1 without storing when the count is already 32.

That table is read only by `1038:0008` (file `0x00005588`), which calls its entries from the last
to the first at `0x000055A1`, then the far pointers at `DS:3676`, `DS:367A` and `DS:367E`, and
returns only when its middle argument is nonzero; otherwise it ends with the call to file
`0x0000539E`, which terminates the program through `int 21h` with `AH` = `4Ch`. Of its four
entry wrappers, the routines reachable by direct calls before the gate (FND-PARTY-026) reach
only the two at file `0x000055DF` and `0x000055EE`, and both pass 0 as that argument.

The other accesses to `DS:13FE` push it as an argument: in five resident routines at
`362C:0000` to `3677:000F`, in overlay 190 at `0x3170` to `0x31AA`, and in overlay 193 at
`0x214E` and `0x2232`.

**Arithmetic.** After the setup and this reservation, 2,067 − 1,000 = 1,067 paragraphs and 253
free entries remain (entries 0 and 1 are the setup's own). The gate routine's two reservations
need 7 + 54 = 61 paragraphs and two entries (FND-PARTY-025).

**Other reservations before the gate.** In the routines reachable by direct calls before the
gate (FND-PARTY-026), `1BF3:27A8` is also called at `DSUN.EXE+0x000166EB`, `0x00016755` and
`0x000167B8`, in the resident routine at file offset `0x000164E4`, with rectangles read from
tables at `DS:10F2`, `DS:1104`, `DS:1116` and `DS:1128`; at `0x00033411`, in `3E06:0002`, which
releases the entries it keeps at `DS:A167` and `DS:A169` before it reserves again; and at
`0x00037156`, in `41E1:000B`, with a rectangle one to four pixels wide and `DS:A189` rows high.
Their sizes and whether each is still held at the gate were not read.

## Interpretation

The full-screen reservation made at startup is released only by an exit handler, which runs
only as the program ends, so it is still held when START GAME reaches the gate. The four calls
of `1038:0008` that FND-PARTY-026 leaves unresolved are on those ending paths too, so they cannot
run a writer before the gate. The gate's two reservations fail only when the reservations other
than the startup one that are held at that moment take more than 1,006 paragraphs or 251
entries. Since one reservation holds at most 1,000 paragraphs, that needs at least two of them
held together.

## Alternatives

- The startup reservation is released before the gate: ruled out for the exit table, which runs
  only on paths that end the program; not ruled out for the other unresolved indirect calls of
  FND-PARTY-026.
- The other reservations listed are small or released before the gate: not read; their sizes
  depend on table contents and on `DS:A189`.

## How to reproduce

Disassemble overlay 180 from `DSUN.EXE+0x00067673` to `0x0006785F` and from `0x00067942` to
`0x000679AB`, and the resident bytes from `0x0000539E` to `0x000053AE`, `0x0000550B` to
`0x00005537` and `0x00005588` to `0x0000561A`, as 16-bit code. Search the file for `FE 13` after a direct-address ModRM or the
`A3`/`A1` forms. Search every byte offset for far and near calls resolving to overlay 180's
`0x0762` (file `0x00067942`), and the relocations and fixups for `56B2:0025`. Take the
reservation sites from the closure of FND-PARTY-026 and disassemble the instructions before each
call to read its arguments.
