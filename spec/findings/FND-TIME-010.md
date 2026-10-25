---
id: FND-TIME-010
title: The game tick DS:145E is written only by the 96 Hz callback's increment, and is read by the walk planner's 32-tick wait, the slot update loop's 4-tick budget, a 24-tick call of 4097:0003, the animation frame timing and overlay 206's 40-tick waits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31AF..31E0:31B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:197A..2D40:1DC0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E34..2D40:3E64
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:0770..371D:07A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F4D1..0x0008F7A0
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/immediate_search.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. `immediate_search.py <dsun> 145E
1460` finds one instruction that writes `DS:145E`: `inc dword ptr [0x145e]` at `31E0:31AF`, in the
timer callback `31E0:31A7` (FND-TIME-008). The other hits read it, or are decodes from inside other
instructions:

| Reader | Use |
| --- | --- |
| `2D40:163B..2D40:1648`, in the walk planner `2D40:10AE` | `DS:145E - DS:19AE` below 32, unsigned (FND-EXPLORE-014 step 5) |
| `2D40:189A` | stores `DS:145E` in `DS:19AE` |
| `2D40:19AD` and `2D40:1D4F..2D40:1D5B`, in `2D40:197A` | keeps the time it started; it goes on to the next slot only while fewer than 4 ticks have passed, and keeps the next slot at `4F49:0000` to resume from |
| `2D40:3E3A..2D40:3E5C`, in `2D40:3E34` | when `DS:145E - 4F49:0006` is at least 24, calls `4097:0003` and stores `DS:145E` in `4F49:0006` |
| `31E0:1F3E`, `31E0:25F3`, `31E0:276A`, `31E0:34EB` | store it in a slot record's `+0x1D` (FND-EXPLORE-019) |
| `31E0:289D`, `31E0:28B7` | the frame timing of `31E0:2837` (FND-EXPLORE-019) |
| overlay 206 `+0DDD..+0DF2` and `+1202..+1217` | loop until `DS:145E` has grown by 40 |

`371D:0770` stores the near pointer `0x145E` at `DS:9C10`, with the byte 4 at `DS:9C0B`. Its two
readers found, `371D:074C` and overlay 193 `+0D3D..+0D52`, read the double word through it and
divide it by that byte, adding 1; neither writes it.

## Interpretation

`DS:145E` counts ticks of the 96 Hz callback, 10.42 ms each, from start-up. In time, the walk
planner's limit is 333 ms, the slot update loop stops after about 42 ms and goes on at its next
call, `4097:0003` runs every 250 ms at most, and overlay 206's waits last 417 ms.

## Alternatives

A write through a block copy, a pointer other than `DS:9C10` or a saved-game load is not excluded
by the search. `4097:0003` was read only to `4097:0040`. What the value `DS:9C10` points to stands
for at its readers was not read.

## How to reproduce

`immediate_search.py <dsun> 145E 1460` and `... 9C10`; `resident_listing.py <dsun>` over
`2D40:1452..2D40:1700`, `2D40:1853..2D40:18E0`, `2D40:197A..2D40:1DC0`, `2D40:3E34..2D40:3E80`,
`31E0:31A7..31E0:31F7` and `371D:0770..371D:07A0`; `overlay_listing.py <dsun> 206 8F4D1 8F7A0`
and `... 206 8FB30 8FB80`.
