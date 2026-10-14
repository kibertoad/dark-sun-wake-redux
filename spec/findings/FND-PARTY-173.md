---
id: FND-PARTY-173
title: The only count of filled party slots in DSUN.EXE is 27E5:0283, which the kill experience share and one store to DS:1425 that nothing reads by its displacement call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:0283..27E5:02A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DA1C..0x0005DA21
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E4CA..0x0006E4D2
tool: Capstone 5.0.9 (byte scans and bounded listings); tools/research/exec-census/direct_callers.py, immediate_search.py
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The count.** `27E5:0283` counts the slots 0 to 3 whose kind byte at `4F49:0C33 + 3 * slot` is not
0 and returns the count in AX (`27E5:0286..02A6`). `direct_callers.py <dsun> 27E5:0283` finds two
far callers:

- overlay 173 `+30FC`, which divides the details dword at `+0x04` of the dead combatant's record
  by the count (`+3101..+3119`), the kill experience share of FND-PARTY-086;
- overlay 184 `+143A`, which stores the count to the word at `DS:1425` (`+143F`).
  `immediate_search.py <dsun> 1421 1422 1423 1424 1425 1426` finds no other instruction with the
  displacement `1425`, and no word or dword access at `1423` or `1424` that would cover it.

**The census.** Three scans list every decoded instruction that reads a slot's occupancy, then the
next instructions, and keep those followed within six instructions by an increment of a counter (a
`[bp-N]` local, a direct memory word or byte, or AX, CX or BP):

| Field read | Sites | Followed by a counter |
| --- | --- | --- |
| the kind byte, any instruction with displacement `0C33` | 213 | `27E5:0294`, the count above, and `2D40:2398`, the object walk of FND-PARTY-172 |
| `+0x0E` of a box record, after `les` from `DS:19C5` | 47 | none |
| `+0x06` of a combatant record, after `les` from `DS:19C9` | 21 | none |

## Interpretation

No code found branches on how many party slots are filled, except the kill experience share,
which divides by that number (RULE-PARTY-015). Code that loops over the four slots and acts on
each filled one treats a smaller party as fewer members under the same rules. The count stored at
`DS:1425` is not read by any instruction found.

## Alternatives

- A count kept another way, such as a counter increment further than six instructions from the
  test, a test on another field, or a read of `DS:1425` through a base register or a block copy, is
  not covered by these scans.
- A rule can still depend on a single slot being empty, such as the leader's slot (FND-PARTY-171);
  that is a question about the slot, not the party's size.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: list `27E5:0283..27E5:02A7` and overlay 173
`+30CC..+312A` and overlay 184 `+140A..+1446`; run `direct_callers.py <dsun> 27E5:0283` and
`immediate_search.py <dsun> 1421 1422 1423 1424 1425 1426`. For the census, decode at each
occurrence of the bytes `33 0C` from two to five bytes before them, and at each `les` whose
displacement is `19C5` or `19C9` and the 15 instructions after it, and apply the counter test above.
