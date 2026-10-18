---
id: FND-TEXT-008
title: TEXT resources give a new character's default name, the item-name table and the version line, each read in its own way
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FCDD..0x0006FDAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073080..0x0007325A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:05D5..277B:06AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:00EB..277B:010F
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, immediate_search.py and gff_tag_numbers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The input is the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`), whose data segment is `57E0`. FND-TEXT-007 locates the three
routines that request `TEXT` resources. `gff_tag_numbers.py` over the installed archives finds
`TEXT` resources 0 to 32, 98, 100 to 118, 200 to 207 and 1,000 in `RESOURCE.GFF` and 99 in
`GPLDATA.GFF`, and none in the other 24.

**Overlay 186 `+05DD`** takes a far destination and two bytes, `a` at `[bp+0x0A]` and `b` at
`[bp+0x0C]`. Its two callers, overlay 183 `+020F` and overlay 184 `+0B9F`, pass the far address
`4E71:0A61` and, from the 66-byte record `DS:1429` points at (FMT-COMBAT-002), the byte at
`+0x13` as `a` and the byte at `+0x12` as `b`; overlay 184's call follows its drawing of the
label `NAME:`. The routine picks the resource number with the roll helper `28C9:391D`
(FND-RNG-005): one roll of 8 plus 199 when `b` is 8, else one roll of 33 less 1 when `a` is 1,
else one roll of 19 plus 99 (`+05F0..+0631`). It requests `TEXT` with that number. On success it
copies bytes from the start of the resource to the destination until it meets a CR (`0x0D`) or
has copied 15, stores a 0 after them and returns 1; on failure it formats the number with
`%u Default` into the destination and returns 0. It then frees the resource (`+0634..+06AC`).

**Overlay 188 `+0000`**, after allocating other tables, asks for the size `n` of `TEXT/1000`
(1 if that fails), allocates `n` bytes at the far pointer `DS:55BE`, points the 256 far entries
at `DS:56F3` at the string `thing` and then entries 0 to 9 at `RESERVED`, and requests
`TEXT/1000` into the buffer (`+0256..+02D7`). If the request fails it keeps those strings. If it
succeeds (`+02E6..+0381`):

1. entry 0 is the buffer's start, and the four bytes at `DS:56EF + 4 * n`, which are entry
   `n - 1` of the table, are set to 0;
2. it walks the buffer with `i` from 0 and the next entry number `k` from 1: at each line start
   it replaces any run of LF and CR bytes with 0, and when `i` is still below `n` points entry
   `k` at byte `i` and adds 1 to `k`; it then moves `i` to the next CR, steps past it, and
   repeats while `i` is below `n`;
3. it stores the low byte of `k` at `DS:55C2`.

The CR that ends each line is left in place, and the LF after it becomes the line's 0, so each
entry from 1 is a line followed by a CR. Entries 0 and 1 both point at line 0, and entry
`m + 1` at line `m`. `TEXT/1000` is 2,403 bytes of 255 lines, so `k` ends at 256, `DS:55C2`
gets 0, and step 1 clears entry 2,402, the four bytes at `DS:7C7B`, beyond the table's 1,024
bytes. `immediate_search.py <dsun> 55C2 56F3` finds the table read, as far string pointers
pushed by index, in overlays 173, 189, 190, 212 and 213, and no instruction naming `DS:55C2`
other than this store.

**`277B:05D5`** formats `1.1:RES ` (`DS:076C`) into the buffer `DS:4226`. When the byte
`DS:143C` is 0 it returns the far address of `VERSION 1.1` (`DS:077B`). Otherwise it requests
`TEXT/98`, puts a 0 at its byte 4 and appends it to the buffer with `2D40:3DC2` and the limit 40,
appends `:GPL ` (`DS:0775`), does the same with `TEXT/99`, frees each resource and returns the
buffer's address. `DS:143C` is set to 1 only at `277B:0107`, when a command-line argument's text
from its third character is `911` (`277B:00EB..277B:010C`). Overlay 203 `+0990` passes the
returned string to overlay 190 `+06B4` after drawing nine lines, and overlay 190 `+1567` passes
it to the message routine overlay 172 `+05DB`.

## Interpretation

`TEXT` resources 0 to 32, 100 to 118 and 200 to 207 are three lists of default names: a new
character's name field gets the first line, cut at 15 characters, of one of them picked at
random, from 200 to 207 when the record's byte `+0x12` is 8, from 0 to 32 when its byte `+0x13`
is 1, and from 100 to 118 otherwise. `TEXT/1000` becomes the item-name table, read by line with
entry 0 repeating line 0, each name keeping its CR. `TEXT/98` and `TEXT/99` give the first four
characters of the resource and script archive versions in the version line, shown only with the
`911` argument.

## Alternatives

- Which character property the bytes `+0x12` and `+0x13` hold was not read; the three name lists
  may follow an origin and a gender.
- How the screens draw a name with its trailing CR, and what lies at `DS:7C7B` that the clear
  overwrites, were not read.
- A request whose tag a script or a table supplies is outside FND-TEXT-007's census, so other
  readers of these resources are not ruled out.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<install>` the installation: `gff_tag_numbers.py <install> TEXT`;
`overlay_listing.py <dsun> 186 6FCC0 6FDAD`, `188 72EA0 73260`, `183 6B1A0 6B200`, `184 6DBE0
6DC40`, `190 79860 798D0` and `203 8BB90 8BC00`; `resident_listing.py <dsun> 2000:7D40..2000:7E80
277B:00C0..277B:0120`; `immediate_search.py <dsun> 55C2 56F3` and `143C`. The strings at
`DS:076C`, `DS:0775`, `DS:077B`, `DS:06BD` and `DS:13E6` are read from the load image. The
outputs are retained under `GAME_DIR/analysis/text-resource-20261018/`. No original function or
game process was run.
