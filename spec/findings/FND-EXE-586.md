---
id: FND-EXE-586
title: DSUN.EXE's script opcode dispatch, text formatter, block copy, overlay 204 callback and video far jump tables go to routines in the file, and declaring them reaches no further interrupt site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:018F..172C:01C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073BBA..0x00073BC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D2F6..0x0004D2FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D30A..0x0004D40C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:612C..1BF3:61AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000172DE..0x000172FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00012284..0x00012288
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF4E..0x0008BF64
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C049..0x0008C05F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D828..0x0008D93A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0109..2707:018D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C3FD..0x0001C41D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C56F..0x0001C58F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C64A..0x0001C66A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C745..0x0001C755
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C801..0x0001C821
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_local_census.json, dsun_interrupt_census.py, far_pointer_words.py, trampoline_target.py); Capstone 5.0.9 for the store and near-call searches
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-585, whose walk leaves 61 computed transfers
unresolved. This reads eight of them and the ones the walk reaches once they are declared.

**Script opcode dispatch.** `172C:018F` takes an opcode byte and calls the stop routine when it is
above `0x80` (FND-CONFIG-136). Otherwise it calls far through DGROUP (`57E0`) `02F6` when that
doubleword is not 0 (`172C:01A7`), then near through the word at DGROUP `030A` plus twice the byte
(`172C:01B5`). `02F6` is 0 at load. The only instruction start in the walk's decoded ranges and the
inventory's ranges that stores to `02F6` to `02F9` without a base or index register is at overlay
188 `+0D1A..+0D26`, which writes the overlay fixup word `00C8` (descriptor 25, segment `2D40`) and
`37F3`: `2D40:37F3`, an inventory row (FND-CONFIG-135). The 129 words at DGROUP `030A..040C` name 115
distinct offsets in `172C`. Every instruction start of those ranges that writes memory at a
displacement from `030A` to `040B` goes through ES loaded with another segment (`4C13`, `4F49`,
`551D` or `3796`, from relocated immediates or overlay fixup words), or through DS loaded from CS
in segment `1BF3`, or through CS in segment `4842`.

**Text formatter.** `1BF3:612C` loads DS from CS and stores CS and `59C1` (argument 0) or `5E12`
(otherwise) at `CS:1154`, which is 0 at load. For each `%` and letter of its format string it looks
the letter up among the ten bytes `duDUcsHVCP` at `CS:61AE` with `repne scasb` and CX 10, skips it
when none matches, and calls near through the word at `CS:61B8` plus twice the position
(`1BF3:6199`): `624E`, `6227`, `626E`, `628E`, `61CC`, `61DB`, `61F8`, `6207`, `6216` or `62C2`.
Other characters go to `call far [1154]` at `1BF3:61A0`. The stores at `1BF3:615B` and `615E` are
the only instruction starts of those ranges that write `1154` to `1157` by displacement, and no
store names `61B8` to `61CB`. The routines at those ten offsets have no near caller in segment
`1BF3`'s frame; three of them call far through `CS:1154` again (`1BF3:61D3`, `61EC`, `62B6`).

**Text layout.** `call [di+6393]` at `1BF3:654E` goes to `1BF3:659B`, `65DA`, `65BE` or `6600`
(FND-PARTY-043).

**Block copy.** `289B:01DC` keeps a local word at `[bp-2]`, written only at `289B:01E8` (`02AE`) and
`289B:020D` (`02BD`), and calls near through it at `289B:025E` and `0269`. The routine's one near
callee, `289B:02DB`, does not touch the frame.

**Overlay 204 callback.** Overlay 204 `+1A68` calls far through its argument `[bp+0A]` at `+1AAC`,
`+1AED`, `+1B15` and `+1B4D`, and never writes it. Its callers are the near calls at `+01A1` and
`+029C`, the hits of a search of overlay 204's code for `E8` naming `+1A68`. They pass `0648:0048`
(descriptor 201's trampoline 8, target `+09D1`) and `0660:0093` (descriptor 204's trampoline 23,
target `+18CD`); each segment word is in its overlay's fixup list. Trampoline 24, which names
`+1A68`, is the target of no far transfer in the walk configuration's relocation list.

**Video far jump tables.** `2707:0188`, `02FA`, `03D5`, `04D0` and `058C` each jump far through
`CS:[bx + table]` with BX four times an argument bounded just before by `cmp ax, 8` and `jb` (`cmp ax,
4` for `04D0`). Their tables at `2707:018D`, `02FF`, `03DA`, `04D5` and `0591` hold 8, 8, 8, 4 and
8 far pointers, each segment word `1707` carrying an MZ relocation, so `2707`: `01AD`, `01CB`,
`01F1`, `0208`, `0218`, `0224`, `023A`, `026E`; `0390`, `031F`, `0334`, `033C`, `0351`, `0366`,
`036E`; `03FA`, `040A`, `0489`, `041D`, `0430`, `0440`, `0453`; `04E5`, `04FD`, `0510`, `0528`;
and `05B1`, `05C2`, `05EF`, `0600`, `062C`, `063C`, `0668`, `0678`. The engine declares computed
near jumps and computed calls only, so these are given to the walk as starts.

**The walk with these targets.** The query `q007_dsun_local_census.json` is FND-EXE-585's query with
exhaustive `indirectCalls` declarations for `172C:01A7`, `172C:01B5`, `1BF3:6199`, `1BF3:61A0`,
`61D3`, `61EC`, `62B6`, `1BF3:654E`, `289B:025E`, `0269` and overlay 204 `+1AAC`, `+1AED`,
`+1B15` and `+1B4D`, and the 34 distinct jump table rows as starts. The walk reaches 1,884 routines
and decodes 129,266 instructions in 2,037 ranges. It reaches no interrupt instruction that
FND-EXE-585's walk did not, so every interrupt site keeps that finding's reading. It leaves 58
transfers unresolved: the 61 less the six of `172C`, `1BF3:6199`, `1BF3:61A0` and `289B`, plus the
three far jumps `2707:02FA`, `03D5` and `04D0` it now reaches. Each other call it newly reaches is
one declared above.

## Interpretation

These transfers go to routines in the file: the script hook `2D40:37F3`, the opcode handlers, the
formatter's conversions and output routines, the text layout routines, the block copy's two
address normalizers, two overlay routines, and the cases of the video routines. None of them
leaves the file. The far jumps stay in the unresolved list because the walk cannot be told their
rows, but the code they reach is decoded.

## Alternatives

- The store searches decode linearly from each range start, so a store that only a decode from
  another offset finds, a store through a base register loaded with DGROUP and an unrelated
  displacement, or a block copy into these tables or pointers is not excluded.
- `1BF3:5D0E`, the other unresolved call in segment `1BF3`, is not declared: FND-PARTY-043 finds
  the field it calls through is set to 0 once its record is set up and is not reached before.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_local_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
The store searches decode each of the report's decoded ranges and each inventory range linearly
and keep each `mov`, `pop`, `xchg` or arithmetic instruction whose memory destination has a
displacement in `02F6..02F9`, `030A..040B` or `1154..1157`, reading the ES load before each
ES-prefixed one. `trampoline_target.py <dsun> 5778:0048 5787:0093` resolves the overlay callbacks.
