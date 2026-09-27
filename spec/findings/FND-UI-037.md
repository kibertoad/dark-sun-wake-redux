---
id: FND-UI-037
title: Save and Load window callback selects rows and branches to the matching file operation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:002F
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 192; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

Overlay 192's `5736:002F` trampoline targets the callback at
`DSUN.EXE+0x0007DA85`. For event value 2, a 13-entry table compares the
control ID with `BUTN/18301`, `BUTN/18302`, `BUTN/18304` through `/18313`,
and `EBOX/18400` of the shared `WIND/18500` graph (FND-UI-036):

| Control | Branch file offset | Bounded effect |
|---|---|---|
| Ten row buttons | `0x0007DD58` | Subtracts 18304 from the ID and stores that row index. In Load mode it calls the row-to-name-box routine; in Save mode it passes the selected record's name field to the name box and calls another control routine. |
| Action button 18301 or name box 18400 | `0x0007DE21` | Passes the selected record's name field and `EBOX/18400` to a control routine, redraws the row, releases the window, then calls the Load Game routine at `0x0007D8ED` when the mode byte is one or the Save Game routine at `0x0007D6FB` when it is zero (FND-SAVE-004, FND-SAVE-005). |
| EXIT button 18302 | `0x0007DF7C` | Redraws EXIT, releases the window and record buffer, then follows a separate return path without a direct call to the save or load routines. |

For event value 6, a four-entry table compares the input word with
`0x011B`, `0x1C0D`, `0x4800` and `0x5000`. The first reaches the EXIT
branch, the second the action branch, and the latter two enter opposite
row-selection paths. The callback does not identify the event source or
prove how the UI framework maps a physical key to these words.

## Interpretation

The LOAD and SAVE action art corresponds to distinct executable file
operations, selected by the same mode byte that changes the title and
action images. A row click changes the selected slot before either
operation. The EXIT branch leaves this window without directly calling
either file operation. The manual's OKAY action name is generic; the shipped
interface and callback use LOAD or SAVE for the same action control.

## Alternatives

The control routines that transfer name text, the condition under which an
empty row is selectable, and the later transition after closing the window
remain unread. The event-6 input words resemble DOS key codes, but this
finding does not establish a physical keyboard mapping. An indirect callee
could have effects beyond those named in the bounded branches.

## How to reproduce

Map overlay 192 from its `5736` header and read the callback trampoline at
`5736:002F`. Disassemble `0x0007DA85..0x0007E013` in bounded 16-bit
windows. Decode the event-2 ID table at `0x0007E014` and its 13 targets at
`0x0007E02E`, then the event-6 word table at `0x0007E048` and its four
targets at `0x0007E050`. Follow the selected-index write in
`0x0007DDA3..0x0007DDB2` and the mode branch at
`0x0007DEC7..0x0007DEF2`; compare
the two target routines with FND-SAVE-004 and FND-SAVE-005.
