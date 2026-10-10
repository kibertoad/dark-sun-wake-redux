---
id: FND-PARTY-067
title: The generation screen's discipline window sets DS:42C0 bits 0x80, 0x40 and 0x20 for buttons 2038 to 2040, whose icons read P.KINESIS, P.METAB and TELEPATHY, and overlay 184 +1876 stores them as bits 0, 1 and 2 of the slot's PSIN byte at 4E71:0A55
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-077]
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
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py), and a Python 3.14.7 reading of RESOURCE.GFF through its directory (FMT-GFF-002, FMT-GFF-003, FMT-GFF-007) decoding ICON frames by RULE-IMAGE-001 and RULE-IMAGE-002
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The window.** Overlay 186 `+0321` closes the window kept in `DS:1134`, opens one with the
handler `56CC:0052` and keeps it in `DS:1134`, prints the string at `DS:136D` (`PSI
DISCIPLINES`) in it, and sets the state of button `0x7F9` to 1 (`+0324..+03AF`).

**Its handler.** `56CC:0052` is overlay 183 `+109D`. For event 2 it takes the button number less
`0x7F6` and, when that is at most 8, jumps through the nine words at `+10ED` (file `0x6C0CD`):
buttons `0x7F6`, `0x7F7` and `0x7F8` call `+0441` with the button and 0x80, 0x40 and 0x20, button
`0x7FE` calls `+0E8B`, the sphere window (FND-PARTY-066), and the others do nothing
(`+10A0..+10E1`).

**Reading the choice.** `+0441` stores the result of `+0CA1` for buttons `0x7F6` to `0x7F8` in
`DS:42C0`, the OR of 0x80, 0x40 and 0x20 for the buttons that are on (FND-PARTY-066). When the
psionicist button `0x7D7` is on (`+0E5E`) and that is nonzero it calls `+0D95` with that value and
then `+0CE3`; when the button is off and the value is nonzero it calls `+0D95` with the pressed
button's bit; when the value is 0 it calls `+0D2C`; and it then stores the result of `+0CA1` in
`DS:42C0` again (`+0444..+04E3`). The psionicist class handler (`+032A`, code 6, FND-PARTY-063)
stores 0xE0 in `DS:42C0` when its button is on (`+0351`), and `+0F5F`, after closing the sphere window,
does the same when button `0x7D7` is on (`+0FD9..+100C`) before it calls `+0CE3` and `+0D95` with it.
Overlay 184 `+07D8` stores 0x80 in `DS:42C0` for a new character (`+084A`, FND-PARTY-063).

**Storing the choice.** Overlay 184 `+1876` (trampoline `56DD:002A`) takes a slot. For `k` from 0 to
2 it sets bit `1 << k` of the byte at `4E71:0A55` plus the slot when `DS:42C0` has bit `0x80 >> k`,
and clears it otherwise (`+1886..+1908`). Its only call found is overlay 184 `+10D6`, right after
the call of `+1648` that stores the character, with the same slot (`+10CC..+10D9`). Overlay 186
writes the slot's `PSIN` resource from the byte at `4E71:0A55` plus the slot (`+0257..+026F`) and
reads it back there (`+02F0..+030E`); FND-PARTY-012 gives the routines.

**The buttons.** `RESOURCE.GFF` holds `BUTN` resources 2038 to 2040 (`0x7F6` to `0x7F8`) with no
text tail and `icon` 2038 to 2040 (FMT-UI-003). The first frame of each of those `ICON`
resources, decoded by RULE-IMAGE-001, shows one word in capitals: P.KINESIS (2038), P.METAB (2039)
and TELEPATHY (2040).

## Interpretation

The discipline window offers psychokinesis, psychometabolism and telepathy in that order. A new
character starts with psychokinesis, a character who is not a psionicist has one discipline at a
time, and choosing Psionicist sets all three. The stored `PSIN` byte (FMT-PARTY-003) holds the
choice: bit 0 psychokinesis, bit 1 psychometabolism, bit 2 telepathy.

## Alternatives

What `+0CE3`, `+0D2C` and `+0D95` do to the buttons was read only from how their results are used,
so whether a psionicist can turn a discipline off in the window is not settled here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 186 6F9F0 6FAC1`,
`186 6F955 6F982`, `183 6C07D 6C0CD`, `183 6B30A 6B4C8`, `183 6BFD9 6C01D`, `184 6E14F 6E16E`
and `184 6E906 6E99A`; `direct_callers.py <dsun> 184+1876 183+109D`; `trampoline_target.py <dsun>
56CC:0052`. Read the nine words at file `0x6C0CD`. Decode the first frame of `ICON` 2038 to 2040
of the installed `RESOURCE.GFF` as FND-PARTY-066 does for 2042 to 2045.
