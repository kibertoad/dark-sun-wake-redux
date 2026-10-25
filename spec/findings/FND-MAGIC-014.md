---
id: FND-MAGIC-014
title: The walker sweep's store is the only write of 4F49:0002, made only when the test passes; no save, load, copy or fill reaches it, so it stays 0 until the clock 4C13:035B passes 0x80000000, and the shipped writers of the clock never take it below the stored time otherwise
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:19AD..2D40:19ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1F9B..2D40:1FA8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:03D3..277B:03E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:111C..172C:1127
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B5AB..0x0005B5B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067826..0x00067835
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082C85..0x00082C9F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00044692..0x00044696
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B408..0x0004B40C
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, segment_offset_refs.py, segment_references.py, block_copies.py and script_decode.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short pass that decodes 15 instructions from each site segment_references.py prints for 4F49; short reads of the bytes at file 0x44692 and 0x4B408
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x5A920 + x` for overlay 173, `0x671E0 + x` for 180, `0x81770 +
x` for 195 and `0x6FE30 + x` for 187. `4F49:0002` is at file `0x44692`.

**The sweep.** `2D40:197A` loads the double word `4F49:0002`, subtracts the clock `4C13:035B` and
compares the difference with 60 as a signed number; when it is above 60 it sets the flag
`[bp-0x18]` and stores the clock in `4F49:0002`, and otherwise it skips both (`2D40:19B5..+19E8`).
In its loop over slots 0 to 3 it calls `2D40:37C1` with each slot when the flag is set
(`2D40:1F9B..+1FA8`). `direct_callers.py <dsun> 2D40:37C1 2D40:197A` finds the near call at
`2D40:1FA4` and one far call of the sweep, at `28C9:1E4C` (FND-COMBAT-062).

**Direct writes of `4F49:0002`.** `segment_offset_refs.py <dsun> 4F49 OFF` for `OFF` 0 to 6 finds
the sweep's load (`2D40:19BA`) and store (`2D40:19E3`), the sweep's word at `4F49:0000` (read at
`2D40:1999` and `2D40:1D65`, stored at `2D40:1D45` and `2D40:1D75`), and a double word at
`4F49:0006` that `2D40:3E34` reads at `2D40:3E41` and stores at `2D40:3E5C`. The word at 0 covers
bytes 0 and 1 and the double word at 6 bytes 6 to 9, so neither overlaps bytes 2 to 5. The same search for the offsets
that put linear `0x4F492` in the segments `4F38` (`0112`, `0110`), `4E71` (`0D82`, `0D80`), `4E4F`
(`0FA2`), `4D62` (`1932`), `4C13` (`3362`) and `4C10` (`3392`) finds nothing.

**Every segment word for `4F49`.** `segment_references.py <dsun> 4F49` prints 799 sites. Decoding
15 instructions from each, the only memory operands without a base register at offsets 0 to 5 of
`4F49` are the ones above. The far pointers the sites build are `push 0x3F49` or `push 0x388`
(overlay form) followed by an offset: 0xE (four sites), 0xFF0, 0xC33 and 0x8A3 as constants, and
24 register offsets, each of which the listing forms as `0xC33 + 3 * n`, `0x8A3 + 0x13 * n` or
`0x363 + 0x1C * n` just before. Five words name `4F49` outside code: the `SAVE` table's far
pointers at `DS:1584`, `DS:1588`, `DS:158C` and `DS:15C8` (offsets `0x08A3`, `0x0C33`, `0x0363`
and `0x0330`, FND-SAVE-015), and file `0x4B408`, a row of the 8-byte table at `0x4B400` that pairs
the segment word `0x3F49` with `0x1257`, which reads as the segment's size.

`block_copies.py <dsun> <inventory> 24` finds 15 copy sites whose context names `4F49`; their
`4F49` addresses are `0xC33 + 3 * n`, `0x8A3 + 0x13 * n`, `0xFF0` or a pointer to `DS:9C02` into a
stack local, none at offsets 0 to 5. The call of the fill routine `1000:3FA2` at `2D40:03ED` clears 0x13 bytes at
`4F49:08A3 + 0x13 * si`.

**Save and load.** The `SAVE` entries in segment `4F49` cover `0330..0363`, `0363..08A3`,
`08A3..0C33` and `0C33..1257` (FND-SAVE-015); none covers `4F49:0002`. The clock is `SAVE` 40, so a
load sets it to the saved value and leaves `4F49:0002` as it was. The double word is 0 in the file
(`0x44692..0x44696`).

**Writers of the clock.** `segment_offset_refs.py <dsun> 4C13 035B` finds these stores:

| Site | Store | In |
| --- | --- | --- |
| `277B:03D8` | 0 | `277B:0024`, whose one direct caller is `1000:0158` |
| overlay 180 `+064B` | 0 | `+0141`, called through `56B2:0020` from `277B:027F` |
| `31E0:31EF` | add 1 | `31E0:31A7`, which has no direct caller; overlay 200 pushes its offset at `+034C` (file `0x894AC`) before a far call, and FND-RNG-077 saw the clock rise by 1 every 667 ms |
| overlay 173 `+0C90` | add 60 | the round step `+0C65` (FND-MAGIC-008) |
| overlay 195 `+1527` | add `60 * n` cut to 16 bits and sign-extended | `+1515` (FND-EXE-619) |

`direct_callers.py` finds the callers of `195 +1515` at `2D40:0493` (in `2D40:048D`, which passes
its argument on) and overlay 204 `+129D` (the rest's end, with the rest's hours, FND-MAGIC-005), and
the one caller of `2D40:048D` at `172C:1120`, in `172C:111C`, the handler of script instruction
0x30 (FND-SCRIPT-005), which passes the value of an expression read by `172C:3278`.
`script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` finds one instruction 0x30, GPL 47 at `049A`,
with the expression `8 * 1`; its `--sweep` decodes 349 of the 350 resources to the end, and GPL 37
stops at `0x0D39` and resumes at `0x0D58`. FND-MAGIC-005 reads the rest's hours as 8 from overlay
182 `+0C23` and as the low word of an operand that is 0 in every shipped script instruction.

## Interpretation

`4F49:0002` is the time the sweep last ended the paid powers. Only the sweep writes it, and only
on the path where it has just found the stored time more than 60 above the clock; no save, load,
copy or fill touches it, so it is 0 from start-up until that path first runs.

With the stored time 0 the test passes only when 0 less the clock, as a signed double word, is
above 60: when the clock lies from `0x80000001` to `0xFFFFFFC3`. The shipped writers only raise the
clock or set it to 0, and the amounts the shipped scripts and the rest pass to `195 +1515` (8 and
0 hours) give 480 and 0. So the sweep sets the flag, and `2D40:37C1` ends the paid powers through
it, only once the clock has passed `0x7FFFFFFF`. At the rates FND-RNG-077 saw, 60 every 57 to 71
ms, that is 2^31 / 60 passes, about 23.6 to 29.4 days of play in one game (the clock is saved with
the game); at 1 per 667 ms alone it is about 45 years. After that first time the stored time holds
the clock's value, and the sweep runs `2D40:37C1` again only if the clock is then set below that
value less 60, as a load of an earlier save of the same game would do. In practice the paid powers
are never ended by the sweep; they end at the fight's end, at a rest, or when upkeep runs out
(RULE-MAGIC-003).

## Alternatives

- A write through a far pointer read from memory, other than the `SAVE` table's, was not traced;
  the 799 segment words for `4F49` were checked only for what the next 15 instructions form.
- A table index out of its range (`0x8A3 + 0x13 * n` or `0xC33 + 3 * n` wrapping to offset 2)
  was not ruled out; the bounds of those indexes were not read.
- A DOS read or the overlay loader writing into segment `4F49` was not searched for apart from
  `38FF:04AB` through the `SAVE` table.
- The script instruction counts come from `GPLDATA.GFF` and `OBJEX.GFF`; scripts in the region
  archives, if any, were not decoded, and GPL 37's 31 undecoded bytes could hold an instruction
  0x30.
- A run that patches the clock to `0x80000000` and watches `2D40:1FA4` would confirm the reading.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `resident_listing.py <dsun>
2D40:197A..2D40:1A10 2D40:1F90..2D40:1FBA 2D40:0440..2D40:04A0 277B:0380..277B:03F0
31E0:31D0..31E0:3200 172C:111C..172C:1127`; `overlay_listing.py <dsun> 173 5B5A0 5B5BC`, `180 67321
67360`, `180 677F0 67840` and `195 82C85 82C9F`; `segment_offset_refs.py <dsun> 4F49` with each
offset from `0000` to `0006`, `4C13 035B`, `4C13 035D`, `4F38 0112`, `4F38 0110`, `4E71 0D82`,
`4E71 0D80`, `4E4F 0FA2`, `4D62 1932`, `4C13 3362` and `4C10 3392`; `segment_references.py <dsun>
4F49`, decoding 15 instructions from the instruction that holds each printed word;
`block_copies.py <dsun> <inventory> 24`, keeping the sites whose context names `0x3f49` or
`0x388`; `direct_callers.py <dsun> 2D40:37C1 2D40:197A 2D40:048D 195+1515 180+0141 277B:0024
31E0:31A7`; `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` and `--sweep`. Read the four bytes
at file `0x44692` and at `0x4B408`.
