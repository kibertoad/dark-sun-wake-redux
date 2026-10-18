---
id: FND-PARTY-240
title: A trigger record's second entry point is set only by script instructions 0x1B and 0x1C, and of the 382 shipped registrations only GPL/144's for object 161 reaches opcode 0x24, inside a dialog branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:11D7..172C:1241
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:09B7..2D40:0AB6
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x000848BF..0x00086BC7
tool: tools/research/exec-census/resident_listing.py, direct_callers.py, immediate_search.py, script_decode.py and script_reach.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The installed `DSUN.EXE` is the input (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`); `GPL/144` is the 8,968 bytes at `0x848BF` of the installed
`GPLDATA.GFF`, and script offsets below are from its start. The trigger records are the 19-byte records of
FMT-SCRIPT-005 at `4F49:08A3`; the fight's trigger run `2D40:207E` runs a record's second entry
point, `entry_script_1` at `+0x0E` and `entry_offset_1` at `+0x0A`, when the word at `+6` is 1 or
2 and the distance test passes (FND-PARTY-098).

**The writers.** `2D40:09E6` takes a structure pointer, a selector, a word and a byte. For each
object slot it finds for the structure's object number, it takes the slot's record number and,
below 48, stores the structure's first word (the code offset) at `+0x08` plus twice the selector
and its second word (the `GPL ` number) at `+0x0C` plus twice the selector; with selector 1 it
also stores the word at `+6` and puts the byte into the low seven bits of `+0x10`
(`2D40:0A32..2D40:0AA1`). It has two callers, the near calls in `2D40:09B7`, which passes selector
1 with its first argument as the word, and in `2D40:09D0`, which passes selector 0. `2D40:09B7` is
called only at `172C:1203`, in opcode `0x1B`'s handler `172C:11D7`, which passes 1, and at
`172C:1238`, in opcode `0x1C`'s handler `172C:120C`, which passes 2; `2D40:09D0` only at
`172C:1D71`, in opcode `0x6F`'s handler. Both handlers read four operands into `4C13:00B4`, and
`172C:2B29` makes the structure from the first three: code offset, `GPL ` number and object
number; the fourth is the byte, the trigger's radius.

The search for the displacements `0x8A9`, `0x8AB`, `0x8AD`, `0x8AF` and `0x8B1` finds, as stores
through a base register into the table, only `2D40:0A45` and `0A5E` above, a store of 0 to `+6`
at `2D40:09A7`, and overlay 187's conversions at files `0x7154A`, `0x71738` and `0x7175C`, which
turn the offsets into `GPLI` entry numbers and back (FND-SCRIPT-017). A byte store to `DS:08B1`
at `10FD:0000` and an `and` through `[bp+si+0x8B1]` in overlay 196 address other memory.

**The registrations.** In the decode of FND-SCRIPT-024, the shipped scripts hold 347 `0x1B` and
35 `0x1C` instructions, and every one has a literal offset and a literal `GPL ` number.
`script_reach.py` follows each of these 382 entry points through the listing, taking the next
instruction, every literal jump and branch target, every menu row's target, the next instruction
after each local call, and every `0x14` call into another script, and reports every transfer
whose target is not a literal or not an instruction start; it reports none. The shipped scripts
hold 13 instructions with opcode `0x24`, in 11 scripts, and none nested in an expression. One is
reached:

- `GPL/144` `0x0023` is `0x1B` with offset 281, `GPL/144`, object -161 and radius 5. From offset
  281 (`0x0119`) the script stops at `0x0122` when the kind-9 variable 32 is not 0, otherwise calls
  the routine at 335 (`0x014F`), which, after tests of `varD[374]`, `varD[331]` and `varE[7]`,
  offers a dialog menu at `0x0211`. Its row whose target is 3510 (`0x0DB6`) prints three lines,
  places object -142 through `0x25` inside the condition at `0x0E95`, and when that condition's
  result is not 0, runs `0x24` with `var9[40]` at `0x0F14`.

The control: the same walk from `MAS/50` offset 0 reaches the `0x22` instruction at `0x05DC`
that FND-UI-050 reads.

## Interpretation

Only script instructions `0x1B` (word 1, run inside the radius) and `0x1C` (word 2, run beyond
it) give a trigger record a second entry point, and the converted forms overlay 187 writes keep
the same entry point. Of the entry points the shipped scripts register, only `GPL/144`'s trigger
for object 161 can reach opcode `0x24`, and only through a dialog: when the conversation it opens
takes the row at 3510 and the placement inside it gives a nonzero result. So a trigger can make a
new record current in a fight only through that dialog, if it runs during a fight.

## Alternatives

- The walk takes both ways at every condition and every menu row; whether the route to
  `0x0F14` runs in a fight depends on the kind-9 variable 32, `varD[374]`, `varD[331]`,
  `varE[7]`, the row's condition and whether a dialog menu works during a fight, none read here.
- A store into the table other than through a base register with these displacements, such as a
  block copy from a saved game, is not covered; a copy keeps entry points some registration set.
- An instruction whose handler writes a variable in place (RULE-SCRIPT-011) does not change a
  registration's literal operands, so it does not add entry points.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `resident_listing.py <dsun> 172C:11D7..172C:1241 172C:1D51..172C:1D7A
2D40:09B7..2D40:0B40`; `direct_callers.py <dsun> 2D40:09B7 2D40:09D0 2D40:09E6 2D40:207E`;
`immediate_search.py <dsun> 8ab 8ad 8af 8b1 8a9`. Then `script_decode.py GPLDATA.GFF OBJEX.GFF
--all > <listing>`, `script_reach.py <listing> 24 --registrations 1B,1C` and, for the control,
`script_reach.py <listing> 22 MAS/50@0`. The listings and reports are retained under
`GAME_DIR/analysis/party-trigger-0x24-20261018/`. No original function or game process was run.
