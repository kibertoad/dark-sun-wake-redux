---
id: FND-EXE-589
title: Every computed transfer DSUN.EXE's census walk leaves unresolved stays in the file, never runs, or goes to the XMS driver, a handler installed before the program, or sound driver code loaded from ADV resources
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2A24..1000:2A28
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:00B8..44B6:00BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:051A..4842:051F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 55CE:018F..55CE:0194
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0498..4842:049D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03F4..4842:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:13FB..4842:1400
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:14EA..4842:14EF
ends_on_last_byte: [4842:051A..4842:051F]
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_apfm_census.json, dsun_interrupt_census.py); Capstone 5.0.9 for the return search
environment: null
---

## Observation

FND-EXE-588's walk of the installed `DSUN.EXE` from its established entries reaches 2,195 routines
and lists 49 computed transfers it cannot resolve. Of the near and far returns in its decoded
ranges, the far return at `4842:040A` is the only one whose previous instruction is a push. Each of
these 50 is classified by an earlier finding:

| Transfers | Count | Where they go | Finding |
|---|---|---|---|
| far jumps through tables at `2707:0188`, `02FA`, `03D5`, `04D0`, `058C` | 5 | cases of the video routines in `2707`, given to the walk as starts | FND-EXE-586 |
| `jmp [398C]` at `1000:252C` | 1 | `1000:129F` | FND-EXE-579 |
| `jmp [3916]` at `1000:298A` | 1 | the return address that `1000:2877` saves on entry | FND-EXE-580 |
| `call far [bp-4]` at `1000:1F10` and `1FBC` | 2 | the interrupt routines the program builds on its stack, called for CD-ROM extension and version services | FND-EXE-578 |
| `1000:029D`, `1000:1D8D`, `4AB9:002B`, `4AB9:0052`, `1BF3:5D0E`, `3D72:012A`, `3D72:09C7`, `3D72:0BD7`, `3D72:0DA1`, `3D72:0DD0` | 10 | never run: the pointer or record each calls through is never set, or the record kind it needs is never present | FND-EXE-580, FND-EXE-579, FND-EXE-583, FND-PARTY-043, FND-EXE-588 |
| `4AE5:0F55`, `0F60`, `0F6D`, `0F7B`, `0FA5`, `0FAF`, `10CB`, `10D9`, `1172`, `117C`, `1220`; `15F3:05D4`, `0629`, `066C`, `0699`, `0701`, `0727`, `0754`, `07C2`, `07D7`, `07FF`, `0817`, `083A` | 23 | the XMS driver's entry, which interrupt 2F AX=4310 returns, with AH 00 or 08 to 0D | FND-EXE-581, FND-EXE-582, FND-EXE-584 |
| `1000:2A24`, `4842:051A`, `44B6:00B8`, `55CE:018F` | 4 | the handlers of interrupts 5, 8, 9 and 19 that were installed before the program started | FND-EXE-575 |
| `4842:0498` | 1 | `4654:09A9`, `196+040B`, `31E0:31A7`, or function `0067` of a driver loaded from an `ADV ` resource | FND-EXE-585 |
| the far return at `4842:040A`, and `4842:13FB` and `4842:14EA` | 3 | the driver functions in segment `4842` for a `DIGPAK` driver, or a `Copy` driver's functions; the `DIGPAK` image's code | FND-EXE-585 |

The walk reaches 83 interrupt 21 sites (FND-EXE-578, FND-EXE-579, FND-EXE-584), none selecting
AH=4B, and the file holds no interrupt 2E (FND-EXE-578).
It reaches six `int 66` instructions, whose handler the program does not install (FND-EXE-585).
FND-EXE-576 gives a control for each kind of site the census searched for.

## Interpretation

`DSUN.EXE`'s own code never asks DOS to run a program and holds no transfer to code outside the
file other than these. So it can start a batch helper only if code outside the file does. The XMS
calls ask only for memory functions, and the four saved handlers are the system's own, so the only
code outside the file left unread is the sound driver code loaded from `ADV ` resources and
whatever handles interrupt 66 (Q-EXE-027).

## Alternatives

- The game starts a batch helper through its own code. Ruled out for every site the walk reaches
  and every transfer it leaves, as far as each classification above holds; each finding cited names
  its own limits.
- Code the walk does not reach, such as the 17 interrupt 21 candidates of FND-EXE-578 (none of which
  selects AH=4B) or code reached only through one of the ten calls that never run, is not covered.
- A handler of interrupt 5, 8, 9 or 19 that another program installed before the game could do
  anything; it is not part of the shipped files.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_apfm_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`,
whose `unresolved` lines list the 49 transfers. The return search decodes each of the report's
decoded ranges and keeps each `ret` or `retf` whose previous instruction is a `push`.
