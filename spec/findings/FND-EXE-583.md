---
id: FND-EXE-583
title: Eight of DSUN.EXE's guarded calls through DGROUP far pointers go to routines in segment 2660, segment 49D2 or overlay 180, or never run
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:01D9..2660:0209
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49D2:0009..49D2:0015
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49DE:00A7..49DE:00B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AB9:0007..4AB9:001C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AB9:001C..4AB9:006A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067255..0x00067260
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005046D..0x00050496
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_pointers_census.json, dsun_interrupt_census.py, far_pointer_words.py, store_values.py, trampoline_target.py)
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-582, whose walk leaves 65 computed transfers
unresolved. Eight of them are far calls through doublewords of DGROUP (`57E0`) in `DS:346D..3496`.
Each of those doublewords is 0 at load and carries no MZ relocation, and each call is skipped by a
`cmp dword [pointer], 0` and `je` just before it. A search of every byte offset of the regions for
a direct store to each word of the range finds these writers:

| Calls | Pointer | Writer | Value |
|---|---|---|---|
| `4734:0199` | `3486` | `2660:01D9..01E5` | `2660:052D` |
| `4734:05C7` | `348A` | `2660:01E5..01F1` | `2660:05CA` |
| `4A32:005D` | `348E` | `2660:01F1..01FD` | `2660:0250` |
| `4A32:01CD` | `3492` | `2660:01FD..0209` | `2660:0447` |
| `49DE:0091` | `3479` | `49D2:0009..0015` | `49D2:0017` |
| `49DE:0027` | `3475` | `49DE:00A7`, from its argument | overlay 180 `+0000` |
| `4AB9:002B` | `3471` | `4AB9:0007`, from its argument | none |
| `4AB9:0052` | `346D` | `4AB9:0007`, from its argument | none |

The writers in `2660` and `49D2` store immediate offsets beside segment words (`1660`, `39D2`) that
carry MZ relocations, and each target starts with `push bp` and an inventory row.

`49DE:00A7` has one caller: the far call at overlay 180 `+007B`, whose segment word is a fixup for
`49DE`, and which pushes `05A0`, a fixup word for descriptor 180, and `0048`. That is overlay 180's
trampoline 8, whose target is the overlay's code offset `+0000`. No word of the file equal to
`00A7` is another far call to `49DE` or a pointer to it.

`4AB9:0007` has no caller: the words of the file equal to `0007` give no far call or immediate
beside segment word `4AB9`, only relocated pointers into four other segments, and no near call in
the 64 KiB frame of segment `4AB9` names it. So `3471` and `346D` stay 0, and the two calls through
them never run.

**The walk with these targets.** The query `q007_dsun_pointers_census.json` is FND-EXE-582's query
with exhaustive `indirectCalls` declarations for the first six rows. The walk reaches 1,428
routines and decodes 107,667 instructions in 1,544 ranges. It leaves 59 transfers unresolved, the
65 less the six declared, and adds none. Every interrupt site keeps the reading of FND-EXE-582.

## Interpretation

Six of these calls go to routines in the file: four in segment `2660`, the module that issues the
CD-ROM extension requests (FND-SOUND-013), one in `49D2` and one in overlay 180. The other two
never run, because nothing sets their pointers. None of them leaves the file, and the code they
reach adds no interrupt site.

## Alternatives

- The store search finds direct stores only, so an indexed, string or block store into `346D..3496`
  is not excluded.
- The near-call search for `4AB9:0007` covers `E8` and `E9` at every byte offset of its frame. A
  computed call to it is not excluded.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_pointers_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`store_values.py <dsun>` with each word from `346d` to `3494` lists the direct stores,
`far_pointer_words.py <dsun> 49DE:00A7 4AB9:0007` the words equal to each setter's offset, and
`trampoline_target.py <dsun> 56B2:0048` resolves the overlay 180 pointer.
