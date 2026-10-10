---
id: FND-PARTY-104
title: A new Preserver level opens a window that sets one bit of the character's SPST entry, the spell it learns, and a new Psionicist level opens one that learns or enhances one or two psionic powers, whose ranks are bits 1 to 7 of the character's PSST bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093160..0x00093378
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009353C..0x000938B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093B62..0x00093BE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093DA6..0x00093E50
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093F8D..0x000942C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094C7C..0x00094D09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097DC5..0x00097F00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061540..0x0006159B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061AC6..0x00061B36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061B8E..0x00061BB4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D656..0x0004D666
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 210 `+0740` calls overlay 209 `+0000` with the slot for a new greatest level
in class code 11 and overlay 209 `+0A02` for one in class code 12 (FND-PARTY-081). `27E5:00C1`
with a slot and a class code returns that class's level (FND-PARTY-081). The table at
`4D62:039C + 15 * slot` holds the slot's `SPST` resource and the one at `4E71:09CD + 34 * slot` its
`PSST` resource (FND-PARTY-012; overlay 186 `+01EE` and `+0230` push the segment words of
descriptors 100 and 110). Both windows below are built at `55BD:0000` and run their events through
`39D1:097D` and `39D1:071F` until that pointer is 0; each button callback switches on an event code
through four words at `cs:0612` and `cs:122E`, both 2, 4, 0x20 and 0x80.

**Spells.** `+1B1C` takes a slot `s`, a far pointer `list` and a byte `k`. For each spell `n` from
the byte at `DS:0655 + k` up to, but not including, the byte at `DS:0656 + k`, it passes over `n`
when bit `n & 7` of the byte at `4D62:039C + 15 * s + (n >> 3)` is set; otherwise it counts `n`
and, when `list` is not 0, stores `n` as the next word of `list`, returning 19 when a twentieth
would be stored. It returns the count (`+1B24..+1BA8`). The bytes at `DS:0656` to `DS:0665` are
0, 12, 25, 39, 56, 68, 81, 92, 102, 111, 115, 115, 127, 143, 165 and 189, so `k` from 1 to 15
covers the spells 0 to 11, 12 to 24, 25 to 38, 39 to 55, 56 to 67, 68 to 80, 81 to 91, 92 to
101, 102 to 110, 111 to 114, none, 115 to 126, 127 to 142, 143 to 164 and 165 to 188.

`+0000` stores the slot in `4E71:0B44`, takes `L`, the level of class code 11, and returns when
`L` is not above 0 or the byte at `+0x14` of the slot's combatant record is 7 or more. For `k`
from `L` down to 1 it calls `+1B1C` with the slot, `4E71:0007` and `k`, stopping at the first
nonzero count; when there was one it calls `+0218` and runs the window's events (`+0007..+00C1`).
`+0218` makes the slot current (FND-PARTY-084), builds the window (handler `+0622`, filled by
`+00C2`), sets `DS:43EC` to overlay 211 `+208C` of the slot and 1, and shows that number with the
text `LEVEL %d` (`+021E..+03DB`). `+00C2` calls `+1B1C` with `4E71:0B44`, `4E71:0007` and
`DS:43EC` and gives each listed spell a button `0x2BCD + i` with button callback `+03DC`
(`+00CA..+0217`). In `+03DC`, event 0x20 on button `0x2BCD + i` takes the word `n` at
`4E71:0007 + 2 * i`, sets bit `n & 7` of the byte at `4D62:039C + 15 * 4E71:0B44 + (n >> 3)`,
shows the spell's name with `IS LEARNED!`, calls overlay 186 `+00B8` with 100, closes the window
(`+0729`) and returns -1 (`+051C..+05A6`). In `+0622`, event 2 on `0x4394` adds 1 to `DS:43EC`,
back to 1 when it passes overlay 211 `+208C` of the slot and 1, and refills the list; event 2 on
`0x4395` asks to confirm with `EXIT` and `CANCEL` and closes the window on `EXIT`; event 1 closes
it (`+0628..+0728`).

Overlay 211 `+208C` with a slot and 1 takes `x`, overlay 211 `+1FB5` of the slot and spell 0, and
returns `(x + 1) >> 1` when `x` is below 10 and `x >> 1` otherwise. `+1FB5` returns, for a spell
below 235, the greatest level among the class codes 0 to 19 that overlay 177 `+0A45` accepts for
the spell, less 7 for codes 13 to 16 (`+1FBD..+1FFF`, `+208F..+20EF`).

**Psionic powers.** Overlay 176 `+0033` returns bits 1 to 7 of the `PSST` byte at
`4E71:09CD + 34 * slot + p`, the rank of power `p`, and `+0000` returns 1 when that rank is not 0
(for a slot of 4 or more, 1); `+064E` sets the rank to 1 and `+0586`, when the rank is not 0 and
below 30, adds 1 to it, both keeping bit 0 (`+0003..+005A`, `+0591..+05F5`, `+0651..+0673`).

`+0A02` takes `L`, the level of class code 12, and when it is not 0 sets `DS:43EC` to 1, plus 1
when `L` is 4, plus 1 when `L` is odd, calls `+0A84` and runs the window's events
(`+0A09..+0A83`). `+0A84` makes the slot current and builds the window (handler `+0E2D`), showing
`DS:43EC` and the name of group `DS:9D7B + 4E71:0B44` (`+0A92..+0C45`). Its fill routine `+0C84`
lists each power `p` from 0 to 33 whose group, from `+0C46`, equals that byte and whose rank is 0
or below 30, with button callback `+0EF0` (`+0C8C..+0CF0`). `+0C46` maps `p` from 1 to 5 to group
0, 6 to 19 to group 1 and 20 to 33 to group 2, and 0 to -1, so power 0 is never listed. In
`+0EF0`, event 0x20 calls overlay 176 `+0586` (`IS ENHANCED`) for a power of rank above 0 and
`+064E` (`IS LEARNED`) for one of rank 0, takes 1 from `DS:43EC`, and closes the window when it
reaches 0 (`+10CB..+116A`). In `+0E2D`, event 2 on `0x2C38` steps the group byte through 0, 1 and
2, and event 2 on `0x4396` asks to confirm with `EXIT` and `CANCEL` (`+0E30..+0EEF`).

## Interpretation

`SPST` is the set of spells a character knows: bit `n & 7` of byte `n >> 3` for spell `n`. Each
new greatest Preserver level offers the unknown spells of one spell level at a time, starting at
the highest the character can cast, and the player learns one, or leaves with none. `PSST` holds
a rank for each psionic power 1 to 33 in bits 1 to 7. Each new greatest Psionicist level lets the
player learn a power at rank 1 or raise a known one by 1, up to 30, twice at odd levels and at
level 4 and once otherwise.

`+0000` decides whether to open the window from the spell ranges of `k` = `L` down to 1, the class
level, while the window lists the spell levels up to overlay 211 `+208C`, at most 7 for a level up
to 15. The window can open with only spells of a level the character cannot yet cast unknown, and
then lists none. For `L` of 12 or more the test also reads spells 115 to 188, and the bits of
those from 120 lie past the slot's 15 bytes, in the next slot's entry.

## Alternatives

- Which class codes overlay 177 `+0A45` accepts for spell 0, and so which classes' levels set the
  highest spell level, was not read.
- Bit 0 of a `PSST` byte is kept by these writers, and what sets or reads it was not read.
- What happens to a choice when the player leaves with `EXIT` was read only for these windows:
  the spell is not learned and the remaining power picks are lost.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 209 93160 93380`,
`209 9351D 93930`, `209 93B62 93F00`, `209 93F16 94300` and `209 94C7C 94D09`; `211 97DC5 97F00`;
`176 61540 615A0`, `176 61AC6 61BB4` and `186 6F8B6 6F982`; `trampoline_target.py <dsun>
57B0:0052 57B0:005C 57B0:006B 57B0:0070 57B0:0075 57B0:007A`; and read the four words and four
targets at file `0x00093772` and `0x0009438E`, and the bytes at file `0x0004D656..0x0004D666`.
