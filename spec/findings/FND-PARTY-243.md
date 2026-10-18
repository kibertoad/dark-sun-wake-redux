---
id: FND-PARTY-243
title: The party's last-cast table, overlay 211's chosen spell and overlay 213's item power take only list numbers below 269 or item powers, so no party cast passes DATA 313 except from overlay 211's handler with a mode byte of 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A788..0x0007AA38
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A051..0x0005A21C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096F04..0x0009755A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009785F..0x00097902
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097BCC..0x00097C50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062500..0x00062592
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093DE4..0x00093E5A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000995AF..0x00099E5B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004D655..0x0004D670
    kind: file-data
tool: tools/research/exec-census/immediate_search.py, direct_callers.py, overlay_listing.py and cast_list_bounds.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`), whose data segment is
`57E0`. FND-PARTY-242 traces the party's numbers for the hit routine to the last-cast table at
`DS:445E`, which overlay 190 `+2503` fills from `DS:6273`, to `DS:43F5`, stored at overlay 211
`+16ED` and overlay 213 `+0AE2`, and to the `SI` overlay 211 `+166E` passes.

**`DS:6273`.** `immediate_search.py <dsun> 6273` finds the word named only in overlay 190
`+2448..+26F8`; no other instruction has 0x6273 as an immediate or displacement. Two
instructions store it: overlay 190 `+2448` stores its first argument in `DS:6276` (the member),
its second in `DS:6275` (a kind byte) and its third in `DS:6273`; `+2503` stores 9999 there and
0xFF in `DS:6275` as it ends. `+2503` writes the table only when `DS:6275` is neither 0xFF nor
3: unless one of the member's six entries already holds the number plus 1, it moves the first
five one place on and stores `DS:6273` plus 1 in the first. With kind 3 it treats `DS:6273` as an
item's index and may call overlay 191 `+0B8C` with it; with another kind it calls overlay 197
`+3060` with the number. With 0xFF it writes nothing. The only other store into the table, overlay 190 `+26F8`, clears a member's six entries
(`immediate_search.py <dsun> 445E`; overlay 209 `+12DF` pushes the word 0x445E as an argument
without a segment, and overlay 172's `mov di, 0x445E` at file `0x00059D0F` starts inside the
instruction at `0x00059D0E`).

`direct_callers.py <dsun> 190+2448` finds seven far calls, through trampoline `571F:00D4`:

- overlay 172 `+0D7E` passes `SI` with a kind of 0 for numbers below 115, 2 for 235 to 268 and
  1 otherwise. `SI` is the member's table entry less 1, loaded at `+0C72..+0C8A`, and the routine
  `+0C21` does not set it elsewhere before the call;
- overlay 173 `+2965` and overlay 213 `+0A64`, `+0AD7` and `+0B3E` pass kind 3 with an item's
  index;
- overlay 211 `+1899` passes kind 0xFF and the number 306;
- overlay 211 `+15FA` passes the `SI` of the routine `+10F4`, with the kind in `[bp-1]`.

**Overlay 211 `+10F4`** returns at once unless `DS:9D79` is above 0 and its index `di`, the
control number less 0x2BCD, is below it. It then reads the member's mode byte `DS:43E8 + member`:
for 1 it loads `SI` from the word list at `DS:9C20 + 2 * di` with kind 0, for 2 the same with
kind 1, and for 3 the same with kind 2 and adds 235 at `+13BC`. For any other mode it goes on
with kind 1 and does not set `SI`, which keeps its caller's value. `+166E` passes this `SI` to
the spell dispatcher and `+16ED` stores it in `DS:43F5`. `direct_callers.py <dsun> 211+10F4`
finds no direct call: it is reached through its trampoline `57C1:0061` only.

`immediate_search.py <dsun> 9C20 9D79` finds `DS:9D79` stored only by overlay 211 `+1AEF`, from
the count overlay 211 `+1A4F` builds, and cleared at `+0600` and `+0AFD`. `+1A4F` sets the count
to 0 and, for mode 1, 2 or 3, fills the list at `DS:9C20`: modes 1 and 2 call overlay 177 `+0400`
with list kind 1 or 2 and the member's level byte, mode 3 calls overlay 211 `+1DBC` with the
member's discipline byte. Overlay 177 `+0400` lists numbers from the byte at `DS:0655 + level`
(odd kind) or `DS:0660 + level` (even kind) up to the next byte, or 0 to 234 for level -1, at most 21 numbers (235 for level
-1).
Overlay 211 `+1DBC` lists the indices from the byte at `DS:066C + d` up to `DS:066D + d`, for a
discipline `d` from 0 to 2 (others give -1). Every bound is a byte, so any level gives numbers below 256.
`cast_list_bounds.py` prints those bytes: wizard levels 0 to 10 give 0 to 114, priest levels 0
to 10 give 115 to 234, and the three disciplines the indices 0 to 5, 6
to 19 and 20 to 33, which are `DATA` 235 to 268. The only other stores into the list, overlay 209
`+0CE5` and `+0CF4`, write indices below 34 and a closing 0. The bytes `DS:066C..DS:066F` are
named by no store: `immediate_search.py <dsun> 066C 066D 066E 066F` finds only overlay 211's two
reads, two resident reads and, in overlay 170, a hit at file `0x00057F92` that starts inside the
instruction at `0x00057F8E`.

`immediate_search.py <dsun> 43E8` finds the mode byte stored only with the immediates 0, 1, 2 and
3, in overlay 190 `+284A..+28CA` and overlay 211 `+0C02..+0C49`.

**Overlay 213 `+034F`.** Its `[bp-4]` is stored at `+0430`, `+04EE` and `+076A`. The paths from
`+0430` and `+04EE` leave through `+0BEA` and `+05D7`, which jump to the routine's end; only the
path from `+076A`, which stores an item's word at `+0x10` less 1, reaches `+0A4F` and `+0AE2`.
The routine has no indirect jump.

## Interpretation

The last-cast table holds a number plus 1 only when overlay 211 `+10F4` or overlay 172 `+0C21`
set `DS:6273` with a kind other than 3 and 0xFF, and overlay 172 only repeats an entry of the
table. So the table, the `SI` of overlay 211's handler and its store in `DS:43F5` hold a
number below 256 from a wizard or priest list (0 to 114 and 115 to 234 for levels 0 to 10) or a
psionic number from 235 to 268, as long as the member's mode byte is 1, 2 or 3. Overlay 213 stores in `DS:43F5` and passes to the
dispatcher an item's power less 1, which FND-PARTY-242 found to be 313 for no shipped item. No
party cast or item power therefore passes `DATA` 313, unless overlay 211 `+10F4` runs with a mode
byte of 0 while `DS:9D79` is above 0, when the number is whatever its caller left in `SI`.

## Alternatives

- The mode 0 case could be impossible if `+1A4F` runs again whenever a member's mode byte becomes
  0, or if the window that sends `+10F4` its events is closed then; neither was read.
- A level byte of -1 lists 235 numbers into a list with room for 171 words before `DS:9D76`; the
  numbers are below 235, but the words past the list were not followed.
- Stores into `DS:6273`, the table, the list or the bounds through a pointer formed from a value
  other than a literal (a block copy, a saved-game load) are outside the immediate search, as are
  item records built while the game runs.
- `+10F4` compares `di` with `DS:9D79` signed, so a control number below 0x2BCD would read words
  before the list; which controls send `+10F4` events was not read.
- The resident hits for `DS:9C20` at `5819:000D` and `5819:000E` lie in the data segment's span,
  not in code.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `immediate_search.py <dsun> 6273`, `445E`, `9C20 9D79`, `43E8` and `066C 066D 066E
066F`; `direct_callers.py <dsun> 190+2448 190+2503` and `211+1A4F 211+10F4 190+271A`;
`cast_list_bounds.py <dsun>`. List with `overlay_listing.py <dsun>` overlay 190 `7A700 7AC20`,
overlay 172 `59F30 5A240`, overlay 173 `5D200 5D300`, overlay 211 `96D00 97720`, `969C0 96AD0`,
`97850 97910` and `97BCC 97C60`, overlay 177 `62500 62640`, overlay 209 `93D90 93EA0` and
`94400 94470`, and overlay 213 `99500 99E60`. The outputs are retained under
`GAME_DIR/analysis/hit-item-313-20261018/`. No original function or game process was run.
