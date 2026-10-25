---
id: FND-RNG-092
title: The copy, fill and string routines other than 1000:0452 and 1000:3F5A, and the inline moves block_copies.py lists, write DS:426D and 4C13:0369 at no classified site; script instruction 0x0A can reach 4C13:0369 but no shipped script does
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:00D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0B2E..172C:0B50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2150..172C:246A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00041330..0x00041370
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:05D5..277B:0610
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DC2..2D40:3E64
tool: tools/research/exec-census/direct_callers.py, block_copies.py, resident_listing.py, overlay_listing.py, immediate_search.py, segment_offset_refs.py and script_decode.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. The words are `DS:426D` (linear
`0x5C06D`) and `4C13:0369` (linear `0x4C499`); 9999 is the byte pair `0F 27`.

**Startup.** `1000:0000..00D8` sets `SS` to `DS`, places the stack above `DS:A4EC` and clears
`DS:39C4..A4EC`, so frame locals and the near heap lie above both words.

**Callers.** `direct_callers.py <dsun> 1000:3220 1000:323F 1000:3265 1000:3280 1000:32C9 1000:374E
1000:376B 1000:37B6 1000:37F2 1000:384D 1000:3F7E 1000:3FA2 1000:3FC1 1000:406D 1000:40F6 1000:4152
1000:4235 1000:0716 1000:0ECC 1000:18BC 1000:0699 1000:32E3 1000:3726 2D40:3DC2 2D40:3E64` finds
370 calls, 101 of them of the stack routine `1000:0699`. The first argument of each other call
falls in one of these classes:

- a frame local or the stack;
- a fixed address above `DS:426D` written forward: `DS:43FB`, `DS:44F2`, `DS:4542`, `DS:6323`,
  `DS:6356 + 0x33 × b` for a byte `b`, `DS:9DB2`, `DS:A4E4`, `DS:A4E8`;
- a fixed address in a segment between the two words, with its length read: `4E28:00DB` (256
  bytes) and `4E71:0A71` (limits `0x28` to `0x50`);
- a fixed address below `DS:426D` with its length read: `DS:4226`, which `277B:05D5` fills with
  `sprintf` and three `2D40:3DC2` appends limited to `0x28`, ending by `DS:424E`; `DS:420A`, one
  word that overlay 173 `+1256`'s call of `2D40:3E64` stores; `DS:0A61`, which overlay 186 `+05DD`
  fills with the format `%u Default`, at most 14 bytes; `DS:4079`, which `172C:31B7` fills with a
  4-character prefix and a 16-byte name, ending by `DS:408E`;
- a far pointer variable that is 0 in the file and stored only from `444C:0008` or a release:
  `DS:19C9`, `DS:40C8`, `DS:6278`, `DS:6554`, `DS:9C14`, `DS:9C18`, `DS:67B7`, `4C13:0251` and
  the script tables `4C13:032F` to `4C13:034B`.

The 69 calls of `1000:3FA2` fill with 0 (55), `0xFF` (12), `0xFE` (1) or `DL` (1, over
`4E28:00DB` for 256 bytes).

**Script instruction `0x0A`** (`172C:0B47`) copies a string with `1000:406D` to the address of its
first operand, which the operand decoder (`172C:2150..246A`) forms. Kind 9 takes a far pointer
from the table at `4C13:0000 + 4 × (n - 0x20)`, whose entry `0x25` is `4C13:0369` and entries
`0x26` to `0x2C` the words below it (file `0x41330..0x41370`). `script_decode.py
"<game>/GPLDATA.GFF" "<game>/OBJEX.GFF" --all` lists 331 instructions `0x0A`: 319 write a kind-1
and 12 a kind-6 string variable, from a literal of at most 39 characters (318) or an object field
(13).

**Inline moves.** `block_copies.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 24` lists
138 `movs` sites: 21 inside the routines above or the near heap's resize; 43 that are not
instructions (9 trampoline bytes, 34 data); 11 that write the `1BF3` library's own code segment;
2 in `44DE:0619` and `0637`, which copy the interrupt vector table to `4DE7:0002` and back; and
heap, EMS, video-page and ring-buffer copies (`289B`, `2707`, `4464`, `15F3:025C`), each read at
its callers. Not classified: `1B2E:0AA8` (its outer caller `1B2E:094F` has no direct caller), 22
page and bitmap copies in `1BF3`, `45CD:00CB` and `0102` (no direct caller), overlay 170 `+050D`,
the pool `[DS:95FF]` (`31E0:39EB`), near arrays copied at overlay 173 `+031C`, 179 `+0886` and
208 `+035A`, the FLI decoders of overlays 214, 216 and 217, the overlay manager's three sites and
`15F3:020F`.

## Interpretation

No classified copy, fill or string write reaches `DS:426D` or `4C13:0369`, and no fill writes the
pair `0F 27`. A script could copy a string over `4C13:0369` through kind-9 variable `0x25`, but
every shipped instruction `0x0A` writes a string table in the heap.

## Alternatives

The 153 inline `stos` sites were not classified, except overlay 175 `+0917`, which writes `0xFFFF`
to `DS:4218..4224`; byte loops without string instructions were not searched. `172C:31B7` appends
from address 0 when its slot is not in state 2, which was taken to stop within the interrupt
vectors without reading them. The video-page and overlay-manager destinations rest on FND-EXE-566
and FND-EXE-568. Script resources outside `GPLDATA.GFF` and `OBJEX.GFF` were not decoded, and
`GPL/37` has an undecoded stretch at `0x0D39..0x0D58`. Other script instructions that write
through kind-9 variables, such as `0x06`, are outside this census.

## How to reproduce

From `tools/research/exec-census/`: the `direct_callers.py` and `block_copies.py` commands above,
with the pushes before each call read through `resident_listing.py` and `overlay_listing.py`;
`immediate_search.py <dsun>` for each far pointer variable named; `resident_listing.py <dsun>`
over the locations; read 64 bytes at file `0x41330`; the `script_decode.py` command above, keeping
the instructions with opcode `0A`.
