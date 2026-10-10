---
id: FND-PARTY-115
title: Of the block copies in DSUN.EXE, only the CHAR load and character generation's store copy whole combatant records into the table at DS:19C9, and generation's working record holds 0 in bytes +0x0E and +0x0F unless a record copied into it held another value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A..1000:3F7E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:406D..1000:4092
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:4152..1000:4183
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3150:000E..3150:0080
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073071..0x00073098
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000732B1..0x000732CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E72C..0x0006E739
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E760..0x0006E79D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EE5E..0x0006EE8C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004375B..0x0004378C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005857E..0x00058593
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078BC0..0x00078BFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078A4A..0x00078A72
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078AE9..0x00078AF9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000933F7..0x00093422
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000938FC..0x00093925
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093AFA..0x00093B19
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093C46..0x00093C51
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/block_copies.py, table_flow.py, callee_copies.py, direct_callers.py, immediate_search.py, store_values.py, overlay_listing.py, resident_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The FMT-COMBAT-001 records are 0x31 bytes each, at the far pointer `DS:19C9`.

**The table.** Overlay 188 `+01D1` calls `444C:0008` with the dwords 1 and 0x80A, stores the far
pointer it returns in `DS:19CB` and `DS:19C9`, and copies it to the dword at `DS:6157`
(`+01D1..+01F8`); `+0411` passes it, when it is not 0, to `444C:0092` and stores that call's result there
(`+0411..+042D`). `immediate_search.py <dsun> 19C9 19CB` finds no other store to those words. The
table is a block of 0x80A bytes, 42 records.

**The copy routines.** `1000:0452` copies `CX` bytes from the far pointer at `[bp + 6]` to the far
pointer at `[bp + 0x0A]`, so the destination is the pointer pushed first, and returns with
`retf 8`. `1000:3F5A` copies the word at `[bp + 0x0E]` bytes from `[bp + 0x0A]` to `[bp + 6]`, so
the destination is the pointer pushed last. `1000:406D` copies the string at `[bp + 0x0A]` with its
terminator to `[bp + 6]`; `1000:4152` copies at most the word at `[bp + 0x0E]` bytes of the string
at `[bp + 0x0A]` to `[bp + 6]` and fills the rest of that many bytes with 0; `1000:411A` compares
two strings and writes nothing.

**The census.** `block_copies.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 48` lists 55 far calls
of `1000:0452`, 49 of `1000:3F5A` and 135 `movsb` or `movsw` instructions outside those two
routines. 23 of the calls and 48 of the moves lie in bytes no inventory row covers, among them the
`CHAR` load's copy at overlay 187 `+0478`. The 104 calls are the 104 far-call sites
`direct_callers.py <dsun> 1000:0452 1000:3F5A` finds.

**Where the table's segment goes.** `table_flow.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`
follows the segment word `DS:19CB` through straight-line runs and finds 96 pushes of it, before
calls of 30 targets, 26 stores of it to memory, and no string move while ES holds it.

- Two pushes go to `1000:3F5A`: overlay 187 `+0473`, the `CHAR` load (FND-PARTY-049), and
  overlay 184 `+1703`, which copies 0x31 bytes from `4E4F:006B` to the record of index `si`
  (`+16D0..+170D`); the path at `+169C` pushes the same source and joins it with the index from
  `DS:112E`. Before each, 0x42 bytes go from `4E4F:0029` to the record of the same index in the
  table at `DS:19C5`.
- Three go to the string routines: overlay 171 `+037E` passes the record's name (`+0x21`) as the
  source of `1000:406D`; overlay 190 `+0880` passes it to the compare `1000:411A`; and `+08B2`
  passes it as the destination of `1000:4152` with a count of 15, which writes bytes `+0x21` to
  `+0x2F`.
- The other 25 targets are `3150:000E`, overlay 211 `+02B5` (also through the trampoline
  `57C1:003E`), 212 `+061C` (also `57C9:0025`), 202 `+0287` (also `577D:0020`), 183 `+189B` and
  `+191D`, `4228:002B`, 186 `+00D4`, 184 `+1DCE`, `2C5F:09F3`, `2C5F:03B7`, 209 `+07D0`,
  `28C9:254C`, 190 `+06B4`, `+079F` and `+36D2`, 172 `+018F`, 195 `+0000`, 206 `+0240`,
  213 `+02D5`, 212 `+0ECF` and `444C:0092`, and six pushes have no call after them in the run.
  `callee_copies.py` lists one copy site in these routines, `3150:007B`, which copies 14 bytes to
  the routine's own stack buffer at `ss:[bp - 0x230]`. Their calls of routines that copy to a
  pointer argument are three: overlay 190 `+06B4` and `+079F` call `2D40:3DC2` and `3F96:01A6`
  with the stack buffers `ss:[bp - 0x28]`, `ss:[bp - 8]` and `ss:[bp - 0x16]` as destinations
  (`+070A..+0732`, `+07A9..+07B9`), and overlay 209 `+07D0` calls `1000:406D` with the far pointers
  at `DS:9C18` and `DS:9C14` as destinations (`+099A..+09B9`). Overlay 209 sets those two pointers
  to blocks of 0x50 bytes from `444C:0008` (`+0297..+02C2`), frees them (`+079C..+07C5`) and clears
  them (`+0AE6..+0AF1`).
- The 26 stores put the segment in locals of 22 overlay routines and in `DS:142F`, `DS:6177` and
  `DS:619B`. None of the 23 routines that hold them has a copy site, and none calls a routine that
  copies to a pointer argument. No copy site's destination is read from `DS:142D`, `DS:142F`,
  `DS:6157`, `DS:6159`, `DS:6175`, `DS:6177`, `DS:6199` or `DS:619B`.
- None of the 96 pushes is followed by a call of the seven resident routines FND-PARTY-113 lists
  as storing at `+0x0E` or `+0x0F` through an argument.

**The working record.** The 0x31 bytes at `4E4F:006B` (file `0x0004375B`) hold 0 at `+0x0E` and
`+0x0F`. `store_values.py <dsun> 79 7A` and `--indexed 79 7A` find no store to `4E4F:0079` or
`4E4F:007A`: the direct hits are stores to `DS:0078` and `DS:007A` in start-up code, and the
indexed ones address the stack through `[bp + si + 0x7A]`. Of the 239 copy sites, two have the
segment word 0x0360 of segment `4E4F` as destination: overlay 184 `+1DE3` and `+1DF7`, in the
routine `+1DCE`, which copy 0x42 bytes to `4E4F:0029` and 0x31 bytes to `4E4F:006B` from the far
pointers at `[bp + 6]` and `[bp + 0x0A]` (`+1DCE..+1DFC`). No other copy site's instructions load
0x0360 or `DS:142D`.

## Interpretation

Among the block copies these searches reach, two copy whole records into the table: the `CHAR`
load, whose shipped and transferred records hold 0 at `+0x0E` and `+0x0F` (FND-PARTY-113,
FND-PARTY-114), and generation's store of its working record. That working record starts with 0
there, nothing found stores to those two bytes, and the only copy into it is the edit path's copy
of a record passed to `184+1DCE`, which FND-PARTY-073 reads as bringing back a stored character.
The other copies that take the table's address write only the name at `+0x21` to `+0x2F`, or write
elsewhere. So no block copy found gives a party member a type byte other than 0, and the party
members stay of type 0 for the record tests of FND-PARTY-112.

## Alternatives

- The segment is followed only through straight-line runs. A pointer formed before a branch and
  pushed after it, or reloaded from a local or from `DS:142D`, `DS:6157`, `DS:6177` or `DS:619B`
  and passed to another routine, is outside the search, beyond the two checks above on the routines
  that store it.
- Only one level of calls below the 30 targets was read for copies to an argument; a deeper chain
  is not ruled out.
- Indirect calls and far calls through computed targets were not resolved.
- Reading a file into the table, as a saved game's load may do, is not a block copy and was not
  searched; it would put back what a save wrote.
- The bytes no inventory row covers were decoded linearly from the start of each gap, which can
  misread data or start out of alignment.
- That the blocks `444C:0008` returns for overlay 209 never overlap the table is taken from their
  separate allocation, not read in the allocator.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `block_copies.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 48 > copies.txt` and compare its 104 call sites with
`direct_callers.py <dsun> 1000:0452 1000:3F5A`; `table_flow.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`; `callee_copies.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv copies.txt` with the 30 targets above, and with the
23 routines that hold the stored segment (173 `+3CDA`, `+3F1A`; 178 `+0F1F`; 179 `+1373`,
`+2A46`; 189 `+12FF`; 190 `+05A9`, `+2A97`, `+2C5E`, `+36D2`; 195 `+0789`; 197 `+0000`, `+0206`,
`+1760`, `+399F`; 204 `+0CBA`, `+2204`; 208 `+0F07`; 209 `+0218`, `+0A84`; 210 `+0131`, `+0B66`;
211 `+02B5`), reading each call against the routines that copy to a pointer argument in
`copies.txt`; `immediate_search.py <dsun> 19C9 19CB`; `store_values.py <dsun> 79 7A` and
`--indexed 79 7A`; and `overlay_listing.py` and `resident_listing.py` over the ranges above.
