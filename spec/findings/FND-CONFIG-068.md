---
id: FND-CONFIG-068
title: Active archive pointer writers include a guarded record-growth path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37ED:0064
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:077C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:000E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5
tool: Python 3.14.7 raw-word inventory of the shipped executable; Capstone 5.0.7 16-bit disassembly of bounded resident windows
environment: null
---

## Observation

The reader's active archive record is the far pointer at `DS:9D9F`
(FND-CONFIG-037). A raw search of the complete shipped `DSUN.EXE` for its
little-endian address word `9F 9D` finds 53 occurrences, all in the
resident MZ image. Six contain direct writes:

| Address-word file offset | Effect |
|---|---|
| `0x2D182` | The handle-selection entry stores a resolved record pointer after its nonzero check. |
| `0x2DA70` | The record-growth helper replaces the active pointer when it names the old record. |
| `0x2E229` | The archive initializer clears the pointer. |
| `0x2E471` | A successful archive open selects the new record. |
| `0x2E540` | Closing the sole record clears the pointer. |
| `0x2E5E6` | Closing the selected record chooses a linked record or the list head. |

One other occurrence, at `0x2D36B`, pushes the address of `DS:9D9F` to
the near helper at physical offset `0x2D93C`. The remaining 46 hits read
the pointer or use its address as a comparison operand; none is another
literal write or address push. The caller reaches that helper only when
its current record's size field at offset `0x2D` is below a required
size. The helper allocates and copies a replacement record, updates list
links, replaces `DS:9D9F` if the old record was active, and on success
writes the replacement pointer through the supplied address as well.

The caller is on a write path that first tests bit 1 of the active
record's option word at offset `0x06`; without it, control returns an
error before the growth call. The archive-open entry copies its option
argument into that word. Overlay 180's startup passes option zero when
opening `RESOURCE.GFF` or `RESFLOP.GFF` (FND-CONFIG-039), so the newly
opened resource record does not pass this write-path gate.

## Interpretation

The literal active-pointer writers and its one literal address-taking
call are bounded. Growing a writable archive can replace its selected
record without closing the archive; the startup resource record's initial
option word excludes that growth path. This does not establish the active
pointer or archive-list contents at every message call.

## Alternatives

A later write to a record's option word, a computed address or alias of
`DS:9D9F`, an indirect call, or code loaded outside the surveyed ranges
could change these conclusions. Selecting or opening another archive can
also change the active pointer while mode 2 still permits traversal of
the resource archive (FND-CONFIG-040).

## How to reproduce

Search the approved `DSUN.EXE` for `9F 9D` and classify all 53 hits.
Disassemble the direct writes near physical offsets `0x2D17C`,
`0x2DA60`, `0x2E21D`, `0x2E467`, `0x2E53D` and `0x2E5C3`.
Follow the address push at `0x2D369..0x2D375` into
`0x2D93C..0x2DABB`, including the conditional direct write and the
output-pointer write. Compare the write gate at `0x2D2F9..0x2D30D`,
the option assignment at `0x2E356..0x2E369`, and the startup pushes at
`0x67606..0x67622`.
