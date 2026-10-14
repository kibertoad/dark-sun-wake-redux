---
id: FND-EXE-591
title: ADV resources 10 and 20 of the installed RESOURCE.GFF request no program execution; the DIGPAK image in ADV 10 installs its own interrupt 66 handler and calls back only into itself or DSUN.EXE's 4842:105F and 4842:107E
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008133F..0x00081DDF
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000914D7..0x00092BFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:014F..47E5:017D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45CD:0000..45CD:001E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0ADF..4842:0AF1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1045..4842:10B2
tool: Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resource_adv_census.py); scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) (reach_config.py with q007_dsun_digpak_census.json, dsun_interrupt_census.py)
environment: null
---

## Observation

ADV 10 (2,720 bytes at `RESOURCE.GFF` offset `0x8133F`, `DIGPAK` at `+3`) and ADV 20 (5,926 bytes at
`0x914D7`, `Copy` at `+2`) are the two `ADV ` resources a `SOUND.INI` record can select that equal no
disc driver (FND-EXE-590). Offsets inside a resource below are the offsets its code runs at.

**Where the DIGPAK image runs.** `47E5:0074` allocates the resource's size plus 16 bytes, adds
`16 - (offset AND 0F)` to the pointer (`47E5:014F..0168`) and passes it to `45CD:0000`, which returns
it with the offset's high twelve bits moved into the segment, so the offset is 0. The install
routine keeps `ES - 10` and `DI + 0100` at `CS:0E28` and `CS:0E26` (`4842:0ADF..0AF1`), so file offset
`f` runs at offset `f + 0100` of its segment. `4842:13FB` and `4842:14EA` call offsets `0200` and
`0203` (FND-EXE-585).

**ADV 10.** `resource_adv_census.py` decodes from `0200` and `0203`, the handlers the decoded code
hands out as far pointers, and the 16 rows of the jump table at `020C` that `0264` indexes with AX
`0688..0697` after `0248..0250` bound it (else it goes to `0269`). The starts are:

| Start | How it is handed out |
|---|---|
| `0248` | the interrupt 66 handler: `0200` jumps to `0958`, which stores `0066`, `0248` and CS at `[085E]`, `[0860]`, `[0862]` and calls `0A05`; that reads the old vector with interrupt 21 AH=35, keeps it at `[0248 - 0A]` and `[0248 - 08]` (`023E`, `0240`), and writes `0248` and CS into vector 66 with two `stosw` through ES = 0 (`0A30`, `0A33`) |
| `0401` | stored with CS as the callback at `CS:0232` by `03CD..03D4` |
| `0481` | passed in DX:BX with DX = CS to interrupt 66 AX=068E at `0369` |
| `0654` | written with CS into vector 8 at `0686..068D`, ES = 0 from `0674..0676`; the old vector is kept at `[066F]` and restored by the stores at `06A7` and `06AE`, DS = 0 |

The walk decodes 1,343 bytes. Its interrupt instructions are `int 66` with AX `068F` (`0354`, `0A4A`),
`068E` (`0369`), `068B` (`048F`), `0689` (`0A4F`) and `0692` (`0A58`), and `int 21` with AH `35`
(`0A1C`), `25` (`0A82`) and `49` (`0A8F`, `0A94`). Its transfers through memory are:

- `jmp [CS:022C]` at `0264`, the jump table above;
- `ljmp CS:[023E]` at `0277`, reached only for AX outside `0688..0697`: the vector interrupt 66
  held before `0A05` ran;
- `lcall CS:[0232]` at `044A`, skipped when `CS:0230` is 0. `0232..0236` is written at `02F2..02FA`
  (zero, when interrupt 66 AX=068E passes DX:BX = 0), at `0301..0310` (the DX:BX of an AX=068E
  request, with the caller's DS at `0236`), and at `03C6..03D9` (`0401` with CS).

No `ret` or `retf` follows a push. The image's other 20 `CD` bytes lie on the path from `0100`, the
image's start as a `.COM` program, which DSUN.EXE never calls: it decodes 403 more bytes, with
`int 21` AH=09 and AH=4C, `int 27` and `int 66` AX=0701, and two `CD` bytes at `0915` and `091A` that
lie in the 12 bytes after `jmp 0929` at `090D`, which nothing decoded reaches.

**The requests that reach the handler.** DSUN.EXE's six `int 66` sites (FND-EXE-585) select AX
`068A` (`4842:0FE2`, `103E`), `068E` (`4842:1058`), `0688` or `068B` (`4842:13D1`), `068C`
(`4842:1403`) and `068F` (`4842:18DE`). The site at `4842:1058` is in `4842:1045`, which passes its
far pointer argument in DX:BX with DS = CS; its callers `4842:11BA` and `1228` pass `4842:105F`,
`4842:12B2` passes `4842:107E`, and `4842:18D5` passes `0000:0000`.

**The walk with the callbacks.** The query `q007_dsun_digpak_census.json` is FND-EXE-590's query
with `4842:105F` and `4842:107E` as two more starts. The walk reaches 2,199 routines and decodes
167,199 instructions in 2,405 ranges; the two new ranges are `4842:105F..107E` and
`4842:107E..10B2`. It reaches no interrupt instruction that FND-EXE-590's walk did not and leaves the
same 49 transfers unresolved.

**ADV 20.** The census decodes from its 17 function table entries (numbers `0064` to `0068` without
`0067`, and `0078` to `0084`) and the two handlers it pushes as far pointers, `0ADB` (at `1217`) and
`0AF5` (at `0EF7`): 5,315 bytes. It holds one interrupt instruction, `int 21` AH=62 at `104B`, and no
far transfer or transfer through memory. With DS = 0 it writes a far pointer argument into a
hardware interrupt vector at `0A3B` and `0A3D`, keeping the old one at `CS:015A`, and puts the old one
back at `0AC4` and `0AC6`. No `ret` or `retf` follows a push, and its one `CD` byte is the decoded
interrupt.

## Interpretation

Neither resource requests program execution from the entries DSUN.EXE uses. ADV 20 never transfers
out of its own code. The interrupt 66 handler a `DIGPAK` driver uses is ADV 10's own `0248`,
installed by the `0200` entry `4842:13FB` calls before the AX=068C request at `4842:1403`. Every
request DSUN.EXE and the image make is in `0688..0697`, so the chain to the earlier interrupt 66
handler at `0277` does not run for them, and the image's callback goes to `0401`, `0481` or the
routines DSUN.EXE registers, `4842:105F` and `4842:107E`, whose walk adds no interrupt site or
transfer. So whichever `SOUND.INI` record setup writes to `SOUND.CFG`, no sound driver the game can
load starts a batch helper.

## Alternatives

- DSUN.EXE could issue an `int 66` before `4842:13FB` installs the image's handler; that request
  would go to whatever handler was installed before the game, which is not part of the shipped files.
- The census follows the entries listed above; a transfer reached only from elsewhere in a resource
  is not covered. The `CD` byte classification is the control: every `CD` byte of ADV 20 and all but
  two of ADV 10 are decoded interrupts, the two left lying in bytes no decoded path reaches.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, in
`tools/research/exec-census/`: run
`resource_adv_census.py <install dir> ../../../spec/builds/BLD-GOG-EN-1.1.files.yaml`, which prints
both censuses. `callback_args.py <dsun> 1045 6 3E7DA 3E848 3E8D2 3EEF5` reads the arguments the four
callers of `4842:1045` pass (the fourth is built by `4842:18C0..18D3`). Then run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_digpak_census.json`,
`python -I -m scientific_method_engine reach <config> > <report>` and
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
