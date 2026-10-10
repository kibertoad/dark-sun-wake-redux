---
id: FND-PARTY-139
title: On the DUAL path the event loops hand events to the message window's handler overlay 172 +0816, the DUAL window's overlay 209 +169C and the handler slot DS:A0F5, which holds only null or overlay 190 +0092; of these only overlay 209 +169C reaches the unload 31E0:05BB other than through overlay 187 +1B67, by way of the object placement 31E0:0121
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0752..39D1:0854
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D..39D1:0BD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A0B..0x00059A65
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009440A..0x00094447
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000946C4..0x00094705
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A1F5..0x0006A20E
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, immediate_search.py, trampoline_target.py, resident_listing.py, overlay_listing.py, q026_dual_event_reads_reach.json, q026_message_handler_reach.json, q026_mode2_handlers_release_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `39D1:0418`, `39D1:0430` and `39D1:0448` store their far pointer argument in the
handler slots `DS:A0F1`, `DS:A0F5` and `DS:A0F9` (FND-PARTY-133).

**`39D1:0752`**, which `39D1:071F` calls with the event it is given, calls the handler in
`DS:A0F5` for an event of kind 3 and returns -1 for kind 1. For other kinds it calls the handler
in `DS:A0F1` when the word at `DS:A103` is 0; otherwise it calls the handler at `+0xF5` of the
object whose far pointer is at `DS:A0FD`, and when that pointer or handler is null it stores 0 at
`DS:A0FD` and calls the handler in `DS:A0F1`. It skips a null handler slot (`39D1:0752..39D1:0854`).

**`39D1:097D`** reads one event for the window its first argument names. A key event goes to
`3BA6:13A4`, `4066:0003` and `3EBE:0BFD` only when the window at `DS:A11D` is that window; a
mouse event takes its number from `3D72:0009`; an event of kind 0xF is passed to the handler in
`DS:A0F1` when that is not null. The rest are returned to the caller (`39D1:097D..39D1:0BD3`).

`direct_callers.py <dsun> 39D1:0430` lists 18 calls. The two words pushed before each are 0 for
12, and for 6 they are overlay 190's segment word 0x05F0 and offset 0x0057 (overlay 190 `+111A`, `+1185` and `+38F2`, overlay 202 `+051A`, overlay
211 `+070B` and overlay 212 `+08BB`); `trampoline_target.py` resolves `571F:0057` to overlay 190
`+0092`. `immediate_search.py <dsun> A0F5` finds no other store but one of 0.

**Overlay 182 `+1856`** passes a null pointer to `39D1:0448` and to overlay 190 `+11A1`, which
stores it through `39D1:0418` (`+19A5..+19BE`, FND-PARTY-127).

**Overlay 209 `+123E`**, the DUAL routine, registers `57B0:0048` through overlay 190 `+11A1` and
opens a window with the same handler through overlay 182 `+016B` (`+12AA..+12E7`);
`trampoline_target.py` resolves it to overlay 209 `+169C`. At `+1584` it reads events for the
window at `55BD:0004` with `39D1:097D` and passes each to `39D1:071F` (`+1564..+15A5`). **Overlay
172 `+05DB`**, the message window, first loops while the window at `4C4D:0007` is open and the
word at `DS:40CD` is not 0, reading that window's events with `39D1:097D` and passing each to its
own `+0816` (`+05DB..+0635`), and opens its window
with `566A:004D` (`+0687..+0696`), which `trampoline_target.py` resolves to overlay 172 `+0816`.

`q026_dual_event_reads_reach.json` is FND-PARTY-138's DUAL query with the ten calls of
`39D1:097D`, `39D1:071F` and `39D1:0752` as targets. The walk reaches the calls at overlay 172
`+05FF`, overlay 209 `+0090`, `+0A52` and `+1584`, and `39D1:08BC`, and `39D1:071F` and
`39D1:0752`, the last two directly from overlay 209 `+123E`. It does not reach those in the load
image, overlay 172 `+0BAC`, overlay 188 `+13F7` or overlay 199.

`q026_message_handler_reach.json` starts at overlay 172 `+0816`, with FND-PARTY-138's cut and the
releases, `31E0:05BB` and overlay 171 `+0D15` as targets. The walk reads 148 routines and 13,428
instructions, leaves 6 computed transfers unresolved (calls at `+0xF9`, `+0xFD`, `+0x5A` and
`+0x5E` of an object, and two through `DS:A119`), and reaches none of the targets. FND-PARTY-138's
single-start walks reach neither `31E0:0556` nor overlay 171 `+0DC9` from overlay 190 `+0092`, and
reach `31E0:0556` from overlay 209 `+169C`. With overlay 187 `+1B67` cut as well, the walk from
overlay 209 `+169C` still reaches `31E0:05BB`, through overlay 209 `+17DC`, overlay 184 `+2107`,
overlay 191 `+05DB`, the object placement `31E0:0121`, overlay 193 `+0DC7`, `+12A8` and `+13EC`,
overlay 173 `+24C7` and overlay 206 `+0E1E`.

## Interpretation

On the DUAL path, the handlers that the event loops reach are the message window's overlay 172
`+0816`, the DUAL window's overlay 209 `+169C` through `DS:A0F1` until a message window nulls it,
and overlay 190 `+0092` through `DS:A0F5` when a screen has registered it. The windows below them,
such as the View Character window that overlay 190 `+3767` opens with overlay 189 `+0559`, do not
get key events while their window is not the one `39D1:097D` is asked about. Of the three
handlers, only overlay 209 `+169C` reaches the unload `31E0:05BB` other than through the loop that
starts at slot 5, and that route places an object through `31E0:0121`, which runs the object's
scripts (FND-PARTY-133), and goes on through the hit routine overlay 193 `+13EC` to overlay 206.
Whether that route unloads a party member's entry depends on the scripts and effects it runs.

## Alternatives

- The object handlers at `+0xF5` called through `DS:A0FD`, and the callbacks at `+0xF9`, `+0xFD`,
  `+0x5A` and `+0x5E` and through `DS:A119`, are left unresolved; a button's callback may run code
  the walks do not read.
- The route from overlay 209 `+169C` may run only for a choice the player cannot make on this
  screen; the conditions along it were not read.
- Overlay 206 `+0E1E` and the other callers of `31E0:05BB` were not read for the entry they pass.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 39D1:0418 39D1:0430
39D1:0448 39D1:0752 39D1:097D 182+016B`, `immediate_search.py <dsun> A0FD A103 A0F5`,
`trampoline_target.py <dsun> 571F:0057 57B0:0048 566A:004D 5713:004D`, `resident_listing.py
<dsun> 39D1:0752..39D1:0854` and `39D1:097D..39D1:0BD3`, and `overlay_listing.py` over the
overlay ranges above. In the repository root run `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
<query>` and `python -I -m scientific_method_engine reach <config>` for
`q026_dual_event_reads_reach.json` and `q026_message_handler_reach.json`, then for
`q026_mode2_handlers_release_reach.json` with `209+169C` as its only start, no control, and
`187+1B67` added to `leaves` and `noReturn`.
