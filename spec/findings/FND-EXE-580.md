---
id: FND-EXE-580
title: DSUN.EXE's startup and exit record walkers, its formatted-output engine and its four signal dispatch calls go to code inside the file, and declaring them reaches no further interrupt site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:02A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050994..0x000509C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:09F4..1000:0A54
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2A07..1000:2C56
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2C56..1000:2DA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005093D..0x00050943
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:023F..277B:026C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2877..1000:2883
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2976..1000:298E
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_dispatch_census.json, dsun_interrupt_census.py, far_pointer_words.py, store_values.py, trampoline_target.py)
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-579, whose walk leaves 77 computed transfers
unresolved. This finding reads eleven more of them, all in segment `1000`.

**Record walkers.** `1000:0220` and `1000:0264` walk six-byte records (a type, a priority and a
pointer), call the one with the lowest (`0220`) or highest (`0264`) priority whose type byte is
not `FF`, set its type byte to `FF` and start again. A type 0 record is called near through
`es:[bx+2]` (`1000:025C`, `1000:029D`), any other far (`1000:0255`, `1000:0296`). The only caller
of `1000:0220`, `1000:0149`, passes `DS:3994..39BE`; the only caller of `1000:0264`, `1000:0170`,
passes `DS:39BE..39C4` (FND-EXE-578). The records hold:

| Record | Type | Pointer |
|---|---|---|
| `DS:3994` | 1 | `4AE5:0D27` |
| `DS:399A` | 0 | `0886` |
| `DS:39A0` | 0 | `11C1` |
| `DS:39A6` | 1 | `1000:12D9` |
| `DS:39AC` | 0 | `2877` |
| `DS:39B2` | 0 | `29AA` |
| `DS:39B8` | 1 | `1000:3B56` |
| `DS:39BE` (exit) | 1 | `4AE5:0193` |

No instruction found by the direct-store search writes a word of `DS:3994..39C4`. The exit table
holds no type 0 record, so the near call `1000:029D` never runs.

**`jmp [3916]`.** `1000:2877`, run by the near record at `DS:39AC`, pops its return address into
`DS:3916` and the two words below it into `DS:3918` and `DS:391A`, and its last instruction,
`1000:298A`, pushes them back and jumps through `DS:3916`. The only writer of `DS:3916` is that
`pop`, and no near call, far call or pointer names `1000:2877`, so the jump returns to
`1000:0260`, after the walker's near call.

**Formatted-output engine.** `1000:0A24` calls the near routine at `[bp+0Ah]` (`1000:0A37`) in the
frame of `1000:09F4`, which takes it as its fourth argument and never writes it (two decodes that
would write it lie inside `cmp word [bp-0Eh], 0` at `1000:0D3B`). `1000:09F4` has five callers,
all near calls, and no word of the file equal to its offset is a relocated far call or pointer:

| Caller | Routine passed |
|---|---|
| `1000:10F9` | `1000:0FB5` |
| `1000:32F5`, `1000:3DC7` | `1000:345B` |
| `1000:3766`, `1000:3782` | `1000:3726` |

**Signal dispatch.** `signal` (`1000:2C77`) finds the signal in the list at `DS:393D` (2, 4, 8, 11,
15, 22), stores the handler it is given in the slot at `DS:3925 + 4 * index`, and installs the
interrupt handlers for that signal (FND-EXE-578). Each interrupt handler reads one slot, exits
through `1000:03EE` when it holds 0 or `FFFF:FFFF`, returns when it holds 1, and otherwise clears
the slot and calls the handler:

| Call | Handler | Slot |
|---|---|---|
| `1000:2A76` | interrupt 5, `1000:2A07` | signal 11, `DS:3931` |
| `1000:2AE7` | interrupt 6, `1000:2A88` | signal 4, `DS:3929` |
| `1000:2B58` | interrupt 0, `1000:2AF9` | signal 8, `DS:392D` |
| `1000:2BC9` | interrupt 4, `1000:2B6A` | signal 8, `DS:392D` |

The slots hold 0 at load. Their writers are `signal` (indexed, from its argument), the handlers'
and `raise`'s stores of 0 (`1000:2DA2`, which no walk reaches). `signal` has three callers, far
calls with relocated segment words, and no near caller: `277B:0247` passes signal 8 with
`5702:00F2`, `277B:0257` signal 4 with `2D40:400C`, and `277B:0267` signal 11 with `5702:00F7`.
`signal` also stores its own address at `DS:A4CA`, which no instruction reads by that address.
`5702:00F2` and `5702:00F7` are overlay 188's trampolines 42 and 43, for code `+17AC` and `+17C7`;
`2D40:400C` starts an inventory row.

**The walk with these targets.** The query `q007_dsun_dispatch_census.json` is FND-EXE-579's query
with eight more exhaustive `indirectCalls` declarations: `1000:0255` to `4AE5:0D27`, `1000:12D9`
and `1000:3B56`; `1000:025C` to `1000:0886`, `1000:11C1`, `1000:2877` and `1000:29AA`; `1000:0296`
to `4AE5:0193`; `1000:0A37` to `1000:0FB5`, `1000:345B` and `1000:3726`; `1000:2A76` to overlay 188
`+17C7`; `1000:2AE7` to `2D40:400C`; and `1000:2B58` and `1000:2BC9` to overlay 188 `+17AC`. The
walk reaches 1,402 routines and decodes 106,852 instructions in 1,519 ranges. It leaves 69
transfers unresolved, the 77 less the eight declared, and adds none. Every interrupt site keeps the
reading of FND-EXE-579: 82 of the 100 interrupt 21 sites reached, 18 candidates, and all 4
interrupt 2F sites reached.

## Interpretation

Eleven of the unresolved transfers go to code inside the file: the startup and exit records, the
return of `1000:2877`, the formatting engine's output routines, and the handlers the game registers
for signals 4, 8 and 11. The exit walker's near call cannot run. None of them leaves the file, and
the code they reach holds no interrupt site that FND-EXE-579's walk did not reach.

## Alternatives

- The record and slot searches find direct stores only; an indexed or block store to the records
  is not excluded. `signal`'s own indexed stores were found by reading it.
- A handler stored by `signal` could also be called through `raise` (`1000:2DA2`), which reads any
  slot; no walk reaches it, and its call goes to the same three registered handlers.
- `jmp [3916]` and `1000:029D` stay in the walk's unresolved list, since the engine takes no
  declaration for a computed jump or for a call that cannot run (toolkit issue #488).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_dispatch_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`store_values.py <dsun>` with the words `3925` to `393B`, `3916`, `3918` and every even word of
`3994..39C4` lists the direct stores, and with `--indexed 3925 3927` the indexed ones.
`far_pointer_words.py <dsun> 1000:09F4 1000:2877 1000:2C77 1000:0220 1000:0264` lists the words
equal to each routine's offset, and `trampoline_target.py <dsun> 5702:00F2 5702:00F7` resolves
the two overlay 188 pointers. The near callers were found by decoding `E8` and `E9` at every byte
offset of file `0x5200..0x15200` and keeping those whose target is the routine.
