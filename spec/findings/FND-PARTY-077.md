---
id: FND-PARTY-077
title: The generation screen's discipline window sets DS:42C0 bits 0x80, 0x40 and 0x20 for buttons 2038 to 2040, lets a psionicist turn any of them off and on and anyone else hold one at a time, and overlay 184 +1876 stores them as bits 0, 1 and 2 of the slot's PSIN byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FA21..0x0006FAC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C07D..0x0006C0CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C0CD..0x0006C0DF
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B30A..0x0006B4C8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BC81..0x0006BE6B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BFD9..0x0006C01D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E15C..0x0006E169
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E906..0x0006E99A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F957..0x0006F974
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0686..3EBE:071F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:071F..3EBE:07F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000345D3..0x000345DF
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:091B..3EBE:0991
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0426..3D72:0443
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, immediate_search.py, field_stores.py), and a Python 3.14.7 reading of RESOURCE.GFF through its directory (FMT-GFF-002, FMT-GFF-003, FMT-GFF-007) decoding ICON frames by RULE-IMAGE-001 and RULE-IMAGE-002
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A button's record is its `BUTN` resource as loaded (FMT-UI-003), and `DS:A125` holds
the record of the button being pressed.

**The window.** Overlay 186 `+0321` closes the window kept in `DS:1134`, opens one with the
handler `56CC:0052` and keeps it in `DS:1134`, prints the string at `DS:136D` (`PSI
DISCIPLINES`) in it, and sets button `0x7F9` with code 1 (`+0324..+03AF`).

**Its handler.** `56CC:0052` is overlay 183 `+109D`. For event 2 it takes the button number less
`0x7F6` and, when that is at most 8, jumps through the nine words at `+10ED` (file `0x6C0CD`):
buttons `0x7F6`, `0x7F7` and `0x7F8` call `+0441` with the button and 0x80, 0x40 and 0x20, button
`0x7FE` calls `+0E8B`, the sphere window (FND-PARTY-066), and the others do nothing
(`+10A0..+10E1`).

**Button codes.** `3EBE:071F` takes a window, a button number and a code from 0 to 5, finds the
button's record with `39D1:049C` and the tag `BUTN`, and jumps through the six words at
`3EBE:07F3` (file `0x345D3`): code 0 clears bit 2 of the word at `+0x58` and bit `0x4000` of the
word at `+0x0C`, code 1 sets both, code 2 sets bit `0x8000` of the word at `+0x0C`, code 3
clears it, and codes 4 and 5 set and clear bit 1 of the word at `+0x58` (`+0730..+07C6`).
`3EBE:091B` stores 2 through its pointer argument when bit `0x8000` of the button's word at
`+0x0C` is set and 3 otherwise (`+0970..+098D`). Overlay 183 `+0E5E` returns 1 when that value is
2 and 0 otherwise.

**Pressing a button.** `3EBE:0686`, when the record at `DS:A125` has bit 2 of `+0x58` clear, its
argument is 1 and bit 8 of `+0x58` is clear, flips bit `0x8000` of the record's word at `+0x0C`
through a pointer to that word (`+06A2..+06E2`). `3D72:0426..0443` goes on to `3EBE:0686` with 1
only when bit 2 of the pressed record's `+0x58` is clear, and skips the button otherwise. An
`immediate_search.py` run for `0x8000` and `0x7FFF` finds no other instruction that sets or clears
that bit at `+0x0C`, and `field_stores.py` finds no store to `+0x0C` with a base or index register
in segments `39D1`, `3A8E`, `3D72` or `3EBE`. `direct_callers.py` finds 207 calls of `3EBE:071F`,
all in overlays. `BUTN` 2038 to 2040 hold 0 in the words at `0x0C`, `0x0E` and `0x58`; `BUTN`
2046 holds `0x4000` at `0x0C` and 2 at `0x58`.

**The button routines.** All take the window and a first and last button.

- `+0CA1` returns -1 for a window of 0, and otherwise the OR of `0x80 >> k` for each button first
  plus `k` that `3EBE:091B` reports as 2 (`+0CA8..+0CFD`).
- `+0D03` sets each button with code 0 (`+0D07..+0D27`).
- `+0D2C` sets each button with code 3, calls `2D40:3BEC` with the word at `4E4F:017E` plus twice
  the position, plus 1, and 0, and sets the button with code 0 (`+0D31..+0D8F`).
- `+0D95` takes a mask as well. With a mask of 0 it calls `+0D2C`. Otherwise, going down from bit
  `0x80` one bit per button, it sets a button whose bit is clear in the mask with code 1 and then
  3 and draws it as `+0D2C` does, and sets a button whose bit is set with code 0 and then 2 and
  draws it with 1 for 0 (`+0D9A..+0E57`).

**Reading the choice.** `+0441` stores the result of `+0CA1` for buttons `0x7F6` to `0x7F8` in
`DS:42C0` (`+0444..+0457`). When `+0E5E` reports the psionicist button `0x7D7` on, it calls
`+0D2C` when that value is 0, and otherwise `+0D95` with that value and then `+0D03`
(`+045A..+04B1`). When the psionicist button is off, it calls `+0D2C` when the value is 0 and
`+0D95` with the pressed button's bit otherwise (`+04B3..+04CD`). It then stores the result of
`+0CA1` in `DS:42C0` again (`+04D0..+04E3`). The psionicist class handler (`+032A`, code 6,
FND-PARTY-073) stores 0xE0 in `DS:42C0` when its button is on (`+0351`), and `+0F5F`, after
closing the sphere window, does the same when button `0x7D7` is on (`+0FD9..+100C`) before it
calls `+0D03` and `+0D95` with it. Overlay 184 `+07D8` stores 0x80 in `DS:42C0` for a new
character (`+084A`, FND-PARTY-073).

**Storing the choice.** Overlay 184 `+1876` (trampoline `56DD:002A`) takes a slot. For `k` from 0 to
2 it sets bit `1 << k` of the byte at `4E71:0A55` plus the slot when `DS:42C0` has bit `0x80 >> k`,
and clears it otherwise (`+1886..+1908`). Its only call found is overlay 184 `+10D6`, right after
the call of `+1648` that stores the character, with the same slot (`+10CC..+10D9`). Overlay 186
writes the slot's `PSIN` resource from the byte at `4E71:0A55` plus the slot (`+0257..+026F`) and
reads it back there (`+02F0..+030E`); FND-PARTY-012 gives the routines.

**The buttons' icons.** `RESOURCE.GFF` holds `BUTN` resources 2038 to 2040 (`0x7F6` to `0x7F8`)
with no text tail and `icon` 2038 to 2040 (FMT-UI-003). The first frame of each of those `ICON`
resources, decoded by RULE-IMAGE-001, shows one word in capitals: P.KINESIS (2038), P.METAB (2039)
and TELEPATHY (2040).

## Interpretation

Code 1 makes a button take no presses and code 0 lets it take them again; codes 2 and 3 turn it on
and off. A press of a discipline button turns it over before the handler reads the buttons, so
`+0441` sees the choice after the press.

The discipline window offers psychokinesis, psychometabolism and telepathy in that order. A new
character starts with psychokinesis. For a character who is not a psionicist the window keeps one
discipline on and the other two closed to presses: pressing the one that is on turns it off and
opens all three, and pressing another then turns that one on and closes the rest. For a psionicist
each press turns that discipline off or back on and the three stay open to presses; choosing
Psionicist sets all three. DONE needs all three for a psionicist and one for anyone else
(FND-PARTY-070), so a psionicist who turns one off cannot finish until it is back on. The stored
`PSIN` byte (FMT-PARTY-003) holds the choice: bit 0 psychokinesis, bit 1 psychometabolism, bit 2
telepathy.

## Alternatives

- Code 1 may not stop presses in every path: `3D72:0426..0443` and `3EBE:0686` skip a button with
  bit 2 of `+0x58`, but the rest of the dispatcher that sends event 2 to the handler was not read,
  so whether a closed button still reaches the handler is not shown here. If it does, the same
  handler logic runs and the outcome for a psionicist is unchanged.
- FND-PARTY-067 recorded the same readings but named the routine at `+0D03` as `+0CE3`, the low
  digits of its file offset `0x6BCE3`, and did not read what the three button routines do.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 186 6F9F0 6FAC1`,
`186 6F955 6F982`, `183 6C07D 6C0CD`, `183 6B30A 6B4C8`, `183 6BC81 6BE6B`, `183 6BFD9 6C01D`,
`184 6E14F 6E16E` and `184 6E906 6E99A`; `resident_listing.py <dsun> 3EBE:0686..3EBE:071F
3EBE:071F..3EBE:07F3 3EBE:091B..3EBE:0991 3D72:03F0..3D72:0450`; `direct_callers.py <dsun>
184+1876 183+109D 3EBE:071F`; `trampoline_target.py <dsun> 56CC:0052`; `immediate_search.py <dsun>
8000 7FFF`; and `field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 0C 0D`.
Read the nine words at file `0x6C0CD` and the six at `0x345D3`. In the installed `RESOURCE.GFF`
read the words at `0x0C`, `0x0E` and `0x58` of `BUTN` 2038 to 2040 and 2046, and decode the first
frame of `ICON` 2038 to 2040 as FND-PARTY-066 does for 2042 to 2045.
