---
id: FND-PARTY-076
title: The generation screen's working details record keeps its constitution byte from the last character finished or copied in, starting from 10, so the hit die floor reads that value instead of the score being set
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D868..0x0006D92C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E10C..0x0006E128
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E181..0x0006E1A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E6D8..0x0006E7A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EE5E..0x0006EE8F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097ACD..0x00097AE2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00043719..0x0004375B
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, segment_references.py, immediate_search.py, store_values.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E4F` starts at file `0x436F0`. The generation screen's working details
record is the 66 bytes at `4E4F:0029` and its combatant record the 49 bytes at `4E4F:006B`
(FND-PARTY-073); the details record's six score bytes are at `+0x15..+0x1A`, constitution at
`+0x17` (FND-PARTY-070). `1000:3F5A` copies the number of bytes given by its third argument from
its second far pointer argument to its first.

**The file.** The six score bytes of the working details record, file `0x4372E..0x43734`, are
10 each.

**The pointer.** `store_values.py` and `immediate_search.py` find the far pointer at `DS:1429`
set to `4E4F:0029` only at overlay 184 `+07E9..+07F5`. The other stores to it are in overlays
190 (`0x78972`, `0x7BA49`), 202 (`0x8A72F`), 209 (`0x933A2`, `0x93C0E`), 211 (`0x96139`) and 212
(`0x98831`), and store slot records or their own arguments. Outside overlays 183, 184 and 190 the
only other instruction naming `DS:1429` is the load at overlay 211 `+1CC0`, which reads the
record's words at `+0x0E` and `+0x10` and stores nothing through it (`+1CC4..+1CDC`).

**Writes into the record.** `segment_references.py` finds 56 references to segment `4E4F`: its
segment-table word in the load image and 6 in overlay 183, 33 in 184, 5 in 209, 10 in 210 and 1
in 212. Those in 183, 209 and 210 read tables at `4E4F:00C0` and beyond, or push the record's
address for overlay 186 `+0421` (trampoline `56E9:0048`), which prints the record's experience
figure (FND-PARTY-058); the one in 212 does the same. In 184 the stores through the segment
write `4E4F:0000..0005`, the words at `4E4F:0037` and `4E4F:0071`, and the class bytes at
`4E4F:0044` plus a position (FND-PARTY-073), and two routines copy whole records:

- `+1648` copies the working details and combatant records to the slot numbered by `DS:112E`
  when that is not -1, and otherwise to the slot given as its argument (`+1674..+170D`).
- `+1DCE` copies the details and combatant records given as its arguments to `4E4F:0029` and
  `4E4F:006B` (`+1DD7..+1DFC`). Its four callers are listed in FND-PARTY-080: `+1606`, the edit
  path of overlay 190; overlay 209 `+14FF`, the DUAL path; overlay 212 `+0403`; and `+0334`, the
  end of `+015D`, which passes the slot `+1648` has just filled from the working records
  (FND-PARTY-074).

In overlays 183, 184 and 210 the only store with displacement `0x15`, `0x16` or `0x17` and a base
register is overlay 184 `+108E`, in the finish `+1029` with its flag 1, which copies the six
scores at `+0x19` of the combatant record to `+0x15` of the details record (`+107C..+1096`).

**The screen's set-up.** Overlay 184 `+07D8` (trampoline `56DD:003E`) stores its argument at
`4E71:0B44`, sets `DS:1429` to `4E4F:0029` and `DS:142D` to `4E4F:006B`, and when `DS:112E` is -1
stores the combatant word at `+0x10`, and in the details record the origin and gender bytes (1),
the hit point word at `+0x08` (0), the alignment byte (5), the experience dword (0) and the three
class bytes (0); it writes nothing at `+0x15..+0x1A` (`+07DC..+0896`). With the flag 0 the finish
goes to `+10F1`, which, when `DS:112E` is -1, clears the byte at `4F49:0C33` plus three times the
slot and the slot details record's word at `+0x0E`, and writes nothing in segment `4E4F`
(`+10F1..+1119`).

## Interpretation

While a character is made, the working details record's constitution byte at `+0x17` changes only
at DONE, which copies the scores, and when `+1DCE` copies a slot's records in. Each roll first
copies the working records into the slot (FND-PARTY-074), so overlay 210 `+0000` reads as its
floor constitution the value the working record held when the screen opened: 10 if no character
has been finished or copied into the screen since the program started, and otherwise the
constitution of the character finished or copied in last. For a new character that is the
previous character's constitution; for a character being edited, its own constitution as stored.
The scores the player rolls and steps on the screen do not reach the floor until DONE.

## Alternatives

- A loaded saved game writes segment `4E4F`: no reference to the segment other than those listed
  exists, so a write would need a block read or copy whose destination is formed from another
  segment value; none was found, and no search for such a read was made.
- A routine given the record's address writes `+0x17` through a pointer with an index register
  only: the routines that receive it (overlay 183's sheet and roll tail routines, overlay 184
  `+015D`, overlay 186 `+0421`) were read only by the displacement search above.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `segment_references.py <dsun> 4E4F`;
`immediate_search.py <dsun> 1429`; `store_values.py <dsun> 1429 142B`; `direct_callers.py <dsun>
184+1DCE 184+07D8`; `trampoline_target.py <dsun> 56E9:0048`; `overlay_listing.py <dsun> 184
6D868 6D92C`, `184 6E10C 6E1A9`, `184 6E6D8 6E7A0`, `184 6EE5E 6EE8F`, `211 97ACD 97AF0`, `211
96120 96145`, `212 985F0 9862A` and `209 9466C 94684`; and the whole of overlays 183, 184 and 210,
searched for stores with displacement `0x15`, `0x16` or `0x17`. Read the 66 bytes at file
`0x43719`.
