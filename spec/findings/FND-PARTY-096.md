---
id: FND-PARTY-096
title: Overlay 195 runs the level drain of overlay 210 +0B66 for the effect code 59 on a party slot, and overlay 179 sends code 59 to the slot struck by an item of DATA number 313 when the attack roll is 20
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082269..0x000822A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082330..0x00082342
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F95F..0x0007F9E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x0006508E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065653..0x000656F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065AE7..0x00065B0A
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, gff_tag_numbers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Combat slots 0 to 3 are the party's (FND-PARTY-081).

**Overlay 195 `+0AF9`** (trampoline `574E:0043`) takes a slot `s`, a second slot and a code `c`.
It looks `c` up in a table of 19 words at `cs:0FB4` (file `0x82724`) with 19 jump targets after
them, and jumps to the matching target (`+0B18..+0B34`). For `c` = 59 the target is `+0BC0`,
which calls overlay 210 `+0B66` (trampoline `57B9:0070`) with `s` when `s` is below 4 and does
nothing else (`+0BC0..+0BD2`). The other keys are 9, 16, 18, 20, 21, 22, 23, 24, 35, 42, 51, 52,
65, 66, 69, 91, 101 and 108.

**Overlay 193 `+166F`** (trampoline `573B:0039`) takes `s`, a second slot, a word `n`, a signed
byte `c` and further arguments. When `c` is 0 it returns; when `c` is negative it calls `+192F`
with `s` and `-c` and returns (`+167A..+1697`). Otherwise, unless `DS:2100` is `0xC0`, it calls
`2C5F:0003` with `n`, which requests the `DATA` resource numbered `n` (the dword `0x41544144`),
calls `+192F` with `s` and `c` when that record's word at `+7` is not `-1`, and then calls
overlay 195 `+0AF9` with `s`, the second slot and `c` sign-extended (`+169F..+16D1`).
`direct_callers.py` lists 38 far calls of it, from overlays 179, 195, 197 and 204, and one near
call at overlay 193 `+1861`.

**Overlay 179 `+0D2F`** (trampoline `56A7:006B`) takes a slot `d`, a second slot and an item
number `n` (`-1` for none), and at `+0D9C` passes `d` and a damage amount to overlay 173 `+22C4`,
which takes a combat slot and an amount of damage (FND-PARTY-086). Its near call at `+1134` calls
`+1373` with `d`, the second slot and `n`. **`+1373`** looks `n` up in a table of 81 words at
`cs:28C1` (file `0x66BA1`), the 81 jump targets following at `cs:2963`, and goes to `+2A32` for
any other `n` (`+13F0..+1411`). For `n` = 313 the target is `+1807`: when `DS:420E` is 20 it calls
overlay 193 `+166F` with `d`, the second slot, `n`, the code 59, the dword `-9999` and 0, and sets
bit 2 of the byte its caller passed by pointer (`+1807..+182D`). Overlay 173 `+2560` stores its
attack roll at `DS:420E` (FND-PARTY-091). Of the 38 far calls of `+166F`, this is the only one
whose pushed code is the immediate 59.

`gff_tag_numbers.py <install> DATA` finds `DATA` resources only in `RESOURCE.GFF`, numbered 0 to
319 and 1000 to 1002.

## Interpretation

The level drain of FND-PARTY-082 runs when overlay 195's effect handler receives code 59 for a
party slot; on any other slot nothing happens. One sender is fixed: a combatant struck by an item
whose `DATA` number is 313 loses a level when the attack roll was a natural 20. Overlay 193
`+166F` is the general way to apply an effect code to a slot, and its far callers pass many other
codes.

## Alternatives

- A caller passing a code it computed: overlay 179 `+24BB` pushes a local byte, overlay 193
  `+1861` passes its own argument, overlay 204 (`0x8DE37`) pushes an argument, and the calls at
  `0x81B3E` and `0x82972` in overlay 195 push values loaded from memory. Any of them may pass 59;
  not read.
- The near callers of `+0D2F` at overlay 179 `+0D1A` and `+1299`, the far caller at overlay 193
  `+04DE`, and the other caller of `+1373` (trampoline `56A7:008E`) were not each read, so which
  attacks pass the attacker's weapon as `n` rests on `+0D2F` looking `n` up as an item and on the
  roll being the attack roll.
- What item type 313 is was not read from `DATA` 313.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 195 82269 822A4`,
`195 82326 82345`, `193 7F95F 7F9E6`, `179 6500F 650A0`, `179 65653 656F1` and
`179 65AC0 65B10`; `resident_listing.py <dsun> 2C5F:0003..2C5F:0040`; `direct_callers.py <dsun>
193+166F 179+1373 179+0D2F`, checking the 24 bytes before each call for `6A 3B`; read the 81 key
words at file `0x66BA1` and the targets at `0x66C43`; and `gff_tag_numbers.py <install> DATA`.
