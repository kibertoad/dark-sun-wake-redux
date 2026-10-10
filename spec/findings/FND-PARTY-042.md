---
id: FND-PARTY-042
title: GPLDATA.GFF is registered before the MAS 99 call and nothing on the way unregisters it, so the load can fail only on a file call's result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x0006769D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0615..38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:08FB..37FC:09C1
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, store_values.py, direct_callers.py, trampoline_target.py, gff_tag_numbers.py, script_listing.py); scientific-method-engine 15.0.0 `reach` (tools/research/exec-census/reach_config.py with q016_archive_round.json)
environment: null
---

## Observation

**The order.** The only direct call of overlay 188 `+046C` (trampoline `5702:00C0`), which runs
`MAS` 99 through overlay 169 `+0000` (FND-CONFIG-149, FND-CONFIG-156, FND-CONFIG-160), is at
overlay 180 `0x00067698`, in the routine that starts at `+0141` (`0x00067321`). From that entry
every path passes, in this order:

1. `38FF:000E` at `0x00067489`, the archive initializer, and `37ED:00B8` with 2 at `0x00067490`,
   which sets the traversal mode at `DS:9D9B` to 2 (FND-CONFIG-040);
2. the open of `GPLDATA.GFF` at `0x000674A9`, through overlay 182 `+0000`, which calls
   `38FF:0066` (FND-SCRIPT-003); when it returns 0 the routine formats the not-found message and
   calls its `+0867`, which passes it to `4448:002C`, and so to `1000:03DF` with status 1
   (FND-CONFIG-062);
3. the `DARKSAVE.GFF` and `DARKRUN.GFF` paths built through `2D40:3DC2` and passed to
   `4544:0000`, then the opens of `RGN0FF.GFF`, `OBJEX.GFF`, and `RESOURCE.GFF` or `RESFLOP.GFF`, each with the same failure
   call;
4. `1BF3:2A4A`, `1BF3:2973`, `1BF3:271B`, then `+0026` when the byte at `DS:13F7` is nonzero or
   `4842:06CF` when it is 0, then setup `5702:00D4` (overlay 188 `+0CD7`), then the call at
   `0x00067698`.

The routine's conditional jumps before `0x00067698` skip a failure message or choose a name or a
helper, and all of them go forward to a point before `0x00067698`.

**The archive-list writers.** The stores to `DS:9D9B`, `DS:9D9D`, `DS:9D9F` and `DS:9DA3`
(the traversal mode, the open count, the active archive and the list head, FND-CONFIG-037,
FND-CONFIG-040) are at:

| Routine | Stores |
| --- | --- |
| `37ED:0064` | `37ED:00B0` |
| `37ED:00B8`, the mode setter | `37ED:00DE` |
| `37FC:077C` | `37FC:089C`, `37FC:08AE` |
| `38FF:000E`, the initializer | `38FF:002E`, `38FF:0037`, `38FF:003F`, `38FF:0054` |
| `38FF:0066`, the open | `38FF:027B`, `38FF:027F`, `38FF:02AB`, on its success path |
| `38FF:02B5`, the close | `38FF:034E`, `38FF:0357`, `38FF:0374`, `38FF:03F4`, `38FF:03F8` |

**The reach run.** `q016_archive_round.json` starts from the routines called in steps 2 to 4
after the `GPLDATA.GFF` open (overlay 182 `+0000`, `3150:000E`, overlay 180 `+0867`,
`1000:406D`, `2D40:3DC2`, `4544:0000`, `1BF3:2A4A`, `1BF3:2973`, `1BF3:271B`, overlay 180
`+0026`, `4842:06CF`, overlay 188 `+0CD7`) and from the values FND-PARTY-039 gives for the
pointers their indirect calls read (overlay 180 `+0848`, `1000:0FB5`, `1000:345B`, `1000:3726`,
`1000:129F`, `2660:052D`, `2660:05CA`, `2660:0250`, `2660:0447`, overlay 180 `+0000` and
`49D2:0017`). Its one leaf is `1000:0388`, the exit worker `1000:03DF` and the other exit entries
call (FND-SAVE-012, FND-PARTY-032). Its targets are the stores of the table other than the
open's, and the open's store at `38FF:027B` as a positive control; the call at overlay 182
`0x0006888D` to `38FF:0066` is a control call site.

It reaches 249 routines and 10,427 instructions without stopping at the instruction limit. It
reaches `38FF:027B` through overlay 182 `+0000` and `38FF:0066`, and none of the 13 other
stores. It leaves 11 unresolved calls: `0x5546` (`DS:A4C6`), `0x5C37` (the putter argument),
`0x772C` (`DS:398C`), `0x3C6D9`, `0x3CB07` and `0x3F57D` (`DS:3486`, `348A`, `348E`), `0x3F007`
(`DS:3475`) and `0x3F071` (`DS:3479`), whose values are among the starts; `0x3FDBB` and `0x3FDE2`
(`DS:3471`, `DS:346D`), which are skipped while those words are 0 and nothing reached sets them;
and `0x7110`, the stack stub through which `1000:1EB4` raises interrupt `0x21` or `0x2F`
(FND-PARTY-039). Its five gaps and four contested instructions lie at `1000:02E9` to `1000:02F7`, in the
abnormal-termination code that `1000:129F` jumps to at `1000:02AD` (FND-PARTY-038).

**The catalog.** Of the 26 installed archives, only `GPLDATA.GFF` has a `MAS ` table. The table
is indexed (its type word has the high bit), with number ranges 1 (1), 50 (3), 54 (10), 65 (5)
and 99 (1), in rising order, and `GFFI` resource 3 as its index. Number 99 is entry 19, which
gives offset `0x1FEAA5` and length 411 (FND-PARTY-041).

**The lookup.** For an indexed table the length query calls `38FF:0615` (FND-CONFIG-151). It
walks the table's ranges in memory, stops with 0 when the number is below a range's first
number, finds the archive's `GFFI` table and the index resource through `39A9:0045` and
`39A9:01BC`, and returns 0 when either is missing. It then calls `37FC:08FB` and requires 0,
positions the file through `38FF:0A72` and requires the offset back, reads 8 bytes through
`44DE:01AF` and requires 8. A failed check returns 0, and the positioning and read checks store
error 21. On success it writes the
offset and length and returns 1. `37FC:08FB` returns 0 at once when the archive record's byte
`+0x2C` is 0. When it is not 0, it writes parts of the record to the file through `38FF:0A72`,
`44DE:022C` and `44DE:02F8`, and any unexpected result from those returns `0xFFFF` with error 21. The
transfer `38FF:07B1` searches the same tables, checks that the selected length is positive and
the offset not negative, and then calls `37FC:08FB`, `38FF:0A72` and `44DE:01AF` with the same
checks (FND-CONFIG-151).

## Interpretation

When overlay 188 `+046C` runs, `GPLDATA.GFF` is open and registered, or the program has already
been asked to end. Nothing called between that open and the MAS 99 call reaches a store that
closes an archive, resets the list or changes the traversal mode from 2, so the lookup, which
in mode 2 walks every open archive once (FND-CONFIG-040), reaches `GPLDATA.GFF`. It is the only
archive with a `MAS ` table, and its catalog holds `MAS` 99, so neither catalog search returns a
miss. Of the loader's other failure branches (RULE-SCRIPT-010), `script_stopped` is cleared by
`172C:000C` and the room search cannot reject 412 bytes against a buffer of 10,000
(FND-PARTY-041). What remains is the result of a file positioning, read or header write call on
`GPLDATA.GFF`: the load fails, and `172C:0299` calls `5702:00B1`, only if one of those calls
returns something other than what the installed file gives. The code does not decide that; the
operating system does.

## Alternatives

- A routine on the way corrupts the archive record or the list in memory without storing to
  these four words, through a pointer: the run lists the stores by their displacement, so a
  write through a stale or wrong pointer is not covered.
- An interrupt handler closes the archive: the run does not read interrupt handlers, and the
  game's own handlers (FND-PARTY-039) were not searched for these stores.
- The archive's in-memory catalog differs from the file's: the open reads the catalog from the
  file (FND-CONFIG-037), and this finding compares the lookup with the file's bytes.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 180 67321
676C0` and `67A47 67A83`; `overlay_listing.py <dsun> 182 68850 68900`; `direct_callers.py <dsun>
188+046C`; `store_values.py <dsun> --indexed 9D9B 9D9D 9D9F 9DA1 9DA3 9DA5`; `resident_listing.py
<dsun> 38FF:0066..38FF:02B5 38FF:0615..38FF:07B1 37FC:08FB..37FC:09C4`; `gff_tag_numbers.py
<install dir> "MAS "`; and `script_listing.py <gff> "MAS " 99`. Then, in the repository root,
run `reach_config.py` with `q016_archive_round.json` and `python -I -m scientific_method_engine
reach` as FND-PARTY-040 gives.
