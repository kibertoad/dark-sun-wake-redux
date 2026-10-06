---
id: FND-PARTY-022
title: The placed-object count starts at 0, and eight direct stores in five routines change it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:264E..57E0:2650
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:044C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070B4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070F62..0x0007118B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089242..0x000895CF
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of overlays 182, 187 and 200; Python 3.14.7 scans of DSUN.EXE's raw bytes, MZ relocation table and FBOV fixup lists
environment: null
---

## Observation

The word at `DS:264E`, which FND-ACTOR-004 reads as the number of entries in use in the
placed-object table at `DS:67B7`, is `57E0:264E` (DS is `57E0`, FND-CONFIG-005). Its two bytes in
the load image, at file offset `0x0004F64E`, are `00 00`.

**Direct stores.** Every place in the file where the bytes `4E 26` follow a ModRM byte with the
direct-address form (`mod` 00, `r/m` 110) was decoded with the opcode before it, and the
`A3 4E 26` form was searched for too. Eight are stores:

| File offset | Instruction | Where | Routine |
|---|---|---|---|
| `0x000271E3` | `inc word [264E]` | `31E0:01E3` | `31E0:0121`, which appends an entry (FND-ACTOR-004) |
| `0x00027404` | `inc word [264E]` | `31E0:0404` | `31E0:03CA` |
| `0x000709B1` | `mov word [264E], 0` | overlay 187, offset `0x0B81` | overlay 187 `0x0A6C` |
| `0x000709C5` | `inc word [264E]` | overlay 187, offset `0x0B95` | overlay 187 `0x0A6C` |
| `0x000710A0` | `mov word [264E], 0` | overlay 187, offset `0x1270` | overlay 187 `0x1132` |
| `0x000710A8` | `inc word [264E]` | overlay 187, offset `0x1278` | overlay 187 `0x1132` |
| `0x00089297` | `mov word [264E], 0` | overlay 200, offset `0x0137` | overlay 200 `0x00E2` |
| `0x000892D0` | `inc word [264E]` | overlay 200, offset `0x0170` | overlay 200 `0x00E2` |

The other hits are compares. The scan does not see stores through a base or index register, a
string instruction, or a segment other than DS that holds `57E0`.

**Overlay 200 `0x00E2`** (trampoline `5773:0034`) does nothing unless the byte at `DS:265B` is nonzero and its argument is a region number
from 1 to 99. It then loads the
`ETAB` resource of that number into the table at `DS:67B7`, sets the count to 0 and adds one per
entry until an entry's word at `0x6` is 0 or the count reaches 1,500. Its only far references in
the MZ relocations and the FBOV fixups are the two calls from overlay 182's region routine at
`0x0365` (FND-PARTY-021), at overlay 182 offsets `0x0539` and `0x0456`.

**Overlay 187 `0x1132`** (trampoline `56EF:0093`) is referenced once, by a far call at overlay
188 offset `0x0A05` (`DSUN.EXE+0x000738A5`). **Overlay 187 `0x0A6C`** (trampoline `56EF:007F`)
has no far reference; it is called by `push cs; call near` at overlay 187 offsets `0x0F2C` (in
the routine at `0x0EBE`) and `0x121B` (in `0x1132`).

**The START GAME path up to the gate.** Of the routines FND-PARTY-021 puts between the Start Game
branch and the gate at overlay 182 offset `0x12DD`, three were read to their returns: overlay 187
`0x2424` (looks up a `DATA` resource), overlay 187 `0x206E` (loads `BMP ` and `PAL ` resources and
records the far pointer it is given at `DS:444E`), and overlay 200 `0x0386`, which the routine at
`0x1167` calls through `0x0640:0x0025`. None of the three stores to the count; each calls further
routines that were not followed.

## Interpretation

On a fresh start of the program the count is 0, and the only routine that fills the table from a
region's `ETAB` resource is reached through the region routine of FND-PARTY-021, on the path
that also decides whether to load the supplied party. Unless one of the other writers runs
between program start and the gate (the two resident appenders, or overlay 187's region-change
routines and the overlay 188 call into them), START GAME finds the count at 0 and loads
characters 40 to 43. This reading does not show which routines run before the start window, nor
what the unfollowed callees on the START GAME path do.

## Alternatives

- The count starts nonzero: ruled out for the load image; nothing before the program's own
  startup code can change it.
- Overlay 200's `ETAB` loader runs before the gate by another route: ruled out for direct far
  references; computed calls were not searched.

## How to reproduce

Read the two bytes at file offset `0x0004F64E`. Search the file for `4E 26`, keep hits whose
preceding byte is a direct-address ModRM (`b & 0xC7 == 0x06`) and decode the opcode before it,
and search for `A3 4E 26`. Map overlays 187, 188 and 200 with
`tools/ghidra/ReportFbovOverlayMap.ps1`, and read every MZ relocation and FBOV fixup for the
segment words `0x46EF` and `0x4773` (resident) or `0x05D8` and `0x0640` (fixups) paired with
offsets `0x007F`, `0x0093` and `0x0034`. Disassemble overlay 200 from `DSUN.EXE+0x00089242` to
`0x00089339` and from `0x000894E6` to `0x000895CF`, and overlay 187 from `0x00072254` to
`0x000722C6` and from `0x00071E9E` to `0x0007201A`.
