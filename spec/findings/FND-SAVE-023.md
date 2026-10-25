---
id: FND-SAVE-023
title: Saving writes script entry points as GPLI/1 entry numbers, which GPLDATA.GFF maps to an offset and a GPL number, and every load and region change maps them back
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071445..0x00071659
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x00070760
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007086B..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070B4A
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001F8F7F..0x001FAE57
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py, resident_listing.py and gff_tag_numbers.py at the commit that adds this finding; a byte search of DSUN.EXE for the tag; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 187 offsets `+x` are at file `0x6FE30 + x`.

**The table.** `gff_tag_numbers.py` finds a `GPLI` table only in `GPLDATA.GFF`, with resource 1,
among the 26 installed archives, and none in the two archives of `cloud_saves`. `GPLI/1` is 7,896
bytes: 1,316 records of three words `(a, b, c)`. The `a` words are 0 to 1,315, each once; the
`(b, c)` pairs are all different; only record 0, `(0, 0, 0)`, has `b` = 0. Records 1 and 3 are
`(1, 1, 1)` and `(3, 712, 1)`. The tag occurs four times in `DSUN.EXE` (file `0x7145F`, `0x7148C`,
`0x71673`, `0x716A0`), all in the two routines below.

**`+1615(flag)`.** Gets the size of `GPLI/1` through `38FF:05B5`, which looks through the open
archives from `DS:9D9F`, and reads it with `38FF:04AB`; a missing table or a failed allocation or
read shows a message through overlay 180 `+0867` and returns. For each 19-byte record chained from
`DS:5AF5` along its byte `+0x12` until -1, and for each pair `j` of 0 and 1, the first record of
the table in file order whose `b` equals word `+8 + 2j` and whose `c` equals word `+0x0C + 2j`
writes its `a` into word `+8 + 2j`; word `+0x0C + 2j` is kept. When the flag is set, for each of
the 200 trigger records at the far pointer `DS:40C0` whose word `+2` is not 0, the first table
record with `b` = word `+0` and `c` = word `+2` writes `a` into word `+0`. It frees the table.

**`+1829(flag)`.** Loads the table the same way. For each pair of each chained 19-byte record, the
first table record whose `a` equals word `+8 + 2j` writes `b` into it and `c` into word
`+0x0C + 2j`. With the flag, for each trigger record whose word `+2` is not 0, the first table record
whose `a` equals word `+0` writes `b` into word `+0` and `c` into word `+2`. It frees the table.

**The calls.** `direct_callers.py` finds `+1829` called at `+0A61` and `+0CBD` only. The writer
`+08E3(n)` calls `+1615` at `+092E` and `+1829` at `+0A61` with flag 1 when `n` is 0 and 0
otherwise; every path after `+1615` reaches `+0A61`. The reader `+0A6C(n)` calls `+1829` at
`+0CBD` with the same flag, after its resource loop whether the reads succeeded or not
(FND-SAVE-022).

## Interpretation

Words `+8`, `+0x0A` of a 19-byte record and word `+0` of a trigger record are offsets into the
`GPL ` resource named by the word beside them (FND-SCRIPT-027, FND-SCRIPT-028). A saved file holds
these offsets as `GPLI/1` numbers, and memory holds them as offsets again after the save, after a
load and after a region change. The game state never uses the numbered form.

## Alternatives

That the lookup reaches `GPLDATA.GFF` rests on its being the only archive with the table; the order
of the archive chain was not read. Because the reverse runs after a failed read, a region change
into a region with no saved resources applies it to the records already in memory, which are
offsets; an offset from 1 to 1,315 there would be replaced. Whether `182 +15D7` rebuilds those
records afterwards was not read. A trigger at offset 0 with a nonzero `GPL ` number is not
replaced on a save, and the reverse turns it into `(0, 0)`; whether any script registers one was
not checked.

## How to reproduce

`overlay_listing.py <dsun> 187 71445 71659`, `... 187 71659 71869`, `... 187 70713 70760`, `...
187 7086B 7089C`, `... 187 7089C 70B4A`; `direct_callers.py <dsun> 187+1615 187+1829`;
`resident_listing.py <dsun> 38FF:04AB..38FF:0615`; `gff_tag_numbers.py <install> GPLI` and the same
on `<install>\cloud_saves`; read 7,896 bytes of `GPLDATA.GFF` at `0x1F8F7F` as three-word records,
counting.
