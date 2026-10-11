---
id: FND-PARTY-157
title: Combatant byte 0x0E selects a monster special attack among the MONS DATA records, which overlay 198 sends through the hit routine on a hit, and byte 0x0F indexes the immunity words at DS:2588
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087310..0x0008753C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x00065090
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AA4..0x00062AC5
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, overlay_listing.py; scratch scripts that print the name at +0x20 of DATA resources read through the GFF directory as details_chunks.py reads a directory, and that evaluate overlay 198 +0149 over its jump tables)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A `CHAR` load copies bytes `0x18` and `0x19` to bytes `+0x0E` and `+0x0F` of the slot's
FMT-COMBAT-001 record (FND-PARTY-113), whose table is at the far pointer `DS:19C9`.

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c9 e f` finds reads of `+0x0E` in
overlays 173, 174, 197 and 198 and of `+0x0F` in resident `25AF:02FA` and overlays 179 and 197,
with four more outside the inventory (overlay 179 `+1D71`, 197 `+2291`, and 204 `+23E2` and
`+23F6`, the field getter's fields 18 and 19).

**DATA names.** Overlay 177 `+09A4` returns, for a number from 0 to 234, `2C5F:0003` of the number
plus 0x20 (`+09A7..+09C3`), the name a spell window shows. Read that way, the `DATA` resources 269
to 319 of `RESOURCE.GFF` are named `MONS269` to `MONS319`; `DATA` 268 is `THOUGHT SHIELD` and
`DATA` 27 `FIREBALL`.

**The special attack.** Overlay 198 `+0149` takes a kind `k` and a mode, and returns -1 or a `DATA`
number through four jump tables at `cs:028A`, `cs:0266`, `cs:0242` and `cs:022C`, and a last test
for mode 4 (`+0149..+022B`). Evaluated for every `k` from 1 to 20:

| `k` | Mode 0 | Mode 1 | Mode 2 | Mode 3 | Mode 4 |
|---|---|---|---|---|---|
| 1 to 4 | 289 to 292 | 286 | 287, or 288 when `DS:420E` is above 19 | -1 | -1 |
| 6 | 293 | -1 | -1 | -1 | -1 |
| 7 | 288 | -1 | -1 | -1 | -1 |
| 8 | -1 | 294 | -1 | -1 | -1 |
| 9 | 295 | -1 | -1 | -1 | -1 |
| 10 | 298 | 297 | 297 | 297 | 284 |
| 11 | -1 | 270 | 270 | 270 | -1 |
| 12 | 300 | 301 | 301 | 301 | -1 |
| 13 | -1 | -1 | 288 when `DS:420E` is 18 or more, otherwise -1 | -1 | -1 |
| 14 | -1 | -1 | 303 | -1 | -1 |
| 15 | 304 | -1 | -1 | -1 | -1 |
| 16 | -1 | 318 | -1 | -1 | -1 |
| 17 | 27 | -1 | -1 | -1 | -1 |
| 18 | -1 | 286 | 287, or 288 when `DS:420E` is above 19 | -1 | -1 |
| 19 | 319 | -1 | -1 | -1 | -1 |
| 20 | 288 | 297 | 297 | 297 | -1 |

`k` of 5 gives -1 in every mode. `DS:420E` holds the attack roll from 1 to 20 (FND-PARTY-091).

Overlay 198 `+0000` takes two slots `a` (`bp+6`) and `b` (`bp+8`) and finds both with
`2D40:3E64`. When `b`'s byte `+0x0E` is not 0 and its argument at `bp+0x12` is not 0, it passes
that byte to `+0149` with mode 1 when its argument at `bp+0xC` is 0, 2 when that equals the one at
`bp+0x10`, and 3 otherwise. A result of 0x11F stores the argument at `bp+0xA` in `DS:631E`; a
result of 0x120 becomes -1 when one of the 48 effects `3143:00A8` returns for effect 0x1E has `b`
at `4E71:024F + 10 * e`. A result other than -1 goes to overlay 179 `+12B1` with `a` first and `b`
second. Then, when `a`'s byte `+0x0E` is not 0 and the same argument is not 0, it passes `a`'s
byte with mode 4 to `+0149` and a result other than -1 to `+12B1` with `b` first and `a` second
(`+0008..+0148`). `+12B1` queues the call for overlay 193 `+003C` (FND-PARTY-107). Overlay 174
`+07ED` passes the byte with mode 0 for the enemy spell choice (FND-PARTY-121), and overlay 197
`+111A` ends a hit by `DATA i` on `t` when `t`'s byte `+0x0E` is not 0 and `+0149` of it with mode
0, 1 or 4 is `i` (FND-PARTY-112).

**The immunity type.** Overlay 179 `+0D2F` reads byte `+0x0F` of the record of its first
argument's slot and ends at once when the word at `DS:2588 + 2 * byte` is 0xFFFF
(`+0D45..+0D6E`). Overlay 197 `+1C52` returns 0, no effect, when that word shares a bit with the
hit's mask, and 100 or 50 otherwise (FND-PARTY-112). The 18 words are listed in FND-PARTY-112;
only type 16 has 0xFFFF. Overlay 197 `+0192` and `+01CF` compare the byte with 2 and 0x0C before
calling `2D40:35EF`.

## Interpretation

CHAR byte `0x18`, combatant `+0x0E`, is a monster's special attack: the kind names, by mode,
which `MONS` record the creature uses as a spell of its own (mode 0), which it applies when the
exchange in overlay 198 `+0000` involves it (modes 1 to 4), and which it is immune to. CHAR byte `0x19`, combatant `+0x0F`, is an
immunity class: it picks which hit masks the creature ignores, and class 16 ignores every hit. Both are 0
in the shipped `CHAR` records (FND-PARTY-113), so a party member has no special attack and
class 0, whose word is 0 and blocks nothing.

## Alternatives

- What each `MONS` record does was not read; the names are placeholders.
- Which of the two slots overlay 198 `+0000` receives is the attacker, what its arguments at
  `bp+0xC`, `bp+0x10` and `bp+0x12` stand for, and so when modes 1 to 4 apply, was not read.
- What overlay 197 `+0192` and `+01CF`, `25AF:02FA` and the reads in overlays 173 and 174 do with
  the bytes was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c9 e f`,
`overlay_listing.py <dsun> 198 87310 8753C`, `overlay_listing.py <dsun> 179 6500F 65090` and
`overlay_listing.py <dsun> 177 62AA4 62AC5`. Read the 20, 18, 18 and 11 words at `cs:028A`,
`cs:0266`, `cs:0242` and `cs:022C` of overlay 198 (file `0x8759A`, `0x87576`, `0x87552` and
`0x8753C`) and follow each case to the value it stores in `DX`. Print the 32 bytes at `+0x20` of
`DATA` 27 and 268 to 319 of `RESOURCE.GFF`.
