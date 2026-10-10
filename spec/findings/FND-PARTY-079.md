---
id: FND-PARTY-079
title: No key presses a button of the generation window, since no button record ever gets a hotkey byte and the window has no menu or accelerator child, so its step buttons take presses only from the mouse
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0A04..39D1:0B1D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0D48..409B:0D83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0E42..409B:0E6E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00036B52..0x00036BBE
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0BFD..3EBE:0CC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0CC1..3EBE:0D24
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00020AF2..0x00020E8B
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, direct_callers.py, field_stores.py, immediate_search.py, segment_references.py, store_values.py), and a Python 3.14.7 reading of RESOURCE.GFF
environment: null
---

## Observation

Event building and delivery are as FND-PARTY-078 gives: `39D1:097D` starts each event as kind 2
with 0 at offset 2, and for a key packet (kind 1) goes on at `+0A04`.

**Keys with an edit box active.** When `DS:A179` is nonzero, `39D1:097D` passes the key word to
`409B:0D48` with the edit box record at `DS:A17F`; when that returns other than -1 and `DS:A101`
is nonzero it puts `DS:A101` at offset 2 of the event, keeping kind 2 (`+0A22..+0A86`).
`409B:0D48` clears `DS:A101` (`+0D60`) and looks the key up among the 27 words at `409B:0FA2`
(file `0x36B52`), jumping through the word 27 words after the match (`+0D69..+0D7F`). Only the
case for `0x1C0D` (Enter), at `+0E42`, stores into `DS:A101`: the word at `+0x18` of the edit box
record, when the record's word at `+0x96` has bit 8 and that word is nonzero (`+0E42..+0E68`).
`store_values.py` finds no other store to `DS:A101` except a byte store that decodes from inside
other bytes (file `0xE783`).

**Keys otherwise.** With no edit box active and a window record at `DS:A11D`, `39D1:097D` calls
`3BA6:13A4`, then `4066:0003` when the window's word at `+0x9E` has bit `0x80`, then `3EBE:0BFD`
when it has bit `0x40`, and ends the event there when one returns 1; otherwise it makes the event
kind 6 (`+0A89..+0B1A`). `3BA6:13A4` compares its child's tag with `MENU` (`+13E5`).
`4066:0003` goes over the window's children for the tag `ACCL` and, for an entry of that child
whose word at `+0x0F` equals the key's low byte, puts the entry's word at `+0x11` at offset 2 of
the event (`+0054..+00A7`). `3EBE:0BFD` folds the key's low byte
to capitals, and for each child of the window whose tag is `BUTN` and whose record has a nonzero
byte at `+0x6C` equal to the key, folded the same way, puts the record's number at `+0x5A` at
offset 2 of the event and returns 1 (`+0C0F..+0CAF`).

**The hotkey byte.** `3EBE:0CC1` finds a window's `BUTN` child by number and stores its third
argument at `+0x6C` (`+0CDA..+0D1C`). `direct_callers.py` finds no call of it, near or far,
`immediate_search.py` finds no instruction with the word `0x0CC1` as an immediate, and
`segment_references.py` finds no word naming segment `3EBE` outside far calls other than its
segment-table descriptor. `field_stores.py` finds two stores with displacement `0x6C` and a base
register in inventoried code outside segment `4842`: `3EBE:0D1C` and `3F96:0599`, in `3F96:0556`,
which looks its record up by the tag `APFM` (`+0572`). The stores in segment `4842` go through
`CS`. The other hits are additions, ands and moves that the decoder finds at bytes inside other
instructions or data. FMT-UI-003 gives the five bytes at `0x68` of every `BUTN` resource as 0.

**The window.** `WIND` 19503 (`0x4C2F`), the generation window overlay 184 `+07D8` opens
through trampoline `56BD:0048` (`+08C2..+08D3`), is 921 bytes at file `0x20AF2` of the installed `RESOURCE.GFF`. Its word at
`0x9E` is 0 and it has 22 children: `BUTN` 2001 to 2010, 2027, 18302, 19304 and 2011 to 2018, and
`EBOX` 4003 (`0xFA3`). FMT-UI-002 gives `BUTN`, `APFM` and `EBOX` as the only child tags in the
shipped windows, with no `ACCL` child in any.

## Interpretation

A key never fills in a button number for a `BUTN`: the hotkey byte `3EBE:0BFD` matches is 0 in
every button resource and nothing that runs writes it, and the window has no `MENU` or `ACCL` child for
the other two routines. The only key that makes an event 2 for the generation window is Enter in its name box,
which carries the edit box's own number. The portrait, score, hit point and alignment buttons
therefore take presses only from the mouse, and their step is the mouse button's (FND-PARTY-078);
the stale bytes a key packet would leave in the event never reach them.

## Alternatives

- A block copy writes `+0x6C` of a loaded `BUTN` record: the search covers stores with a
  displacement only. The records are the resources as loaded (FMT-UI-003), and no copy into them
  was found, but none was searched for.
- The loaded window's word at `+0x9E` differs from the file's: this matters only for the menu and
  hotkey routines, which find nothing to match in this window either way.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun> 39D1:097D..39D1:0BD3
409B:0D48..409B:0EA8 3EBE:0BFD..3EBE:0D24 3BA6:13A4..3BA6:1428 4066:0003..4066:00C9 3F96:0556..3F96:05A0`;
`direct_callers.py <dsun> 3EBE:0CC1`; `immediate_search.py <dsun> 0CC1`; `segment_references.py
<dsun> 3EBE`; `store_values.py <dsun> A101`; and `field_stores.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv 6C` and the same with `--es`. Read the 27 words at
file `0x36B52` and the 27 after them. In the installed `RESOURCE.GFF` read `WIND` 19503: its word
at `0x9E`, its child count at `0xF3`, and the tag and number of each 30-byte child from `0x105`.
