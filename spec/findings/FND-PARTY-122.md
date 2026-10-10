---
id: FND-PARTY-122
title: Of the hit routine's computed item arguments, overlay 197 +1202 repeats the current hit's number, overlay 176 passes 235 to 268, and overlay 204 +19AF passes a script argument modulo 1000 for function 46 of opcode 0x22
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E795..0x0007E7D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x0006501D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065157..0x000651A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x00084568
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000845AC..0x0008460A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F299..0x0005F2D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000805E8..0x0008060C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000616D6..0x00061703
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000618D2..0x00061902
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006190D..0x00061925
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000619A9..0x000619CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061A41..0x00061A45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF3B..0x0008BF49
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C01E..0x0008C045
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D68D..0x0008D781
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008C487..0x0008C4F1
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-107 lists the callers that compute the third argument of overlay 193
`+0000` or overlay 179 `+12B1`, which reaches overlay 193 `+003C`. **Overlay 193 `+003C`** keeps
that argument in `DI` and passes it to the hit routine overlay 179 `+0D2F` (trampoline
`56A7:006B`) as its third argument, after the slot and before the target's word at `+0x1A`
(`+04A5..+04E3`). `+0D2F` loads its third argument into `SI` (`+0D2F..+0D3D`) and writes `SI` no
more before `+0EC0`.

**Overlay 197 `+1202`.** `trampoline_target.py <dsun> 575A:009D` gives overlay 197 `+111A`, which
`+1202` lies in. `+111A` takes an object `di` and a number `si` from its word at `bp+0xC`; when the
number is below -1 it negates it and sets its flag at `bp-4` to 1 (`+111A..+1168`). It calls
overlay 179 `+12B1` at `+1202` with the word at `DS:426D`, `di`, `si` and a byte only when the flag
is 0, `si` is not -1 and the tests on the object's records pass (`+11AC..+120A`).
`direct_callers.py <dsun> 197+111A` finds two far callers. Overlay 179 `+0EC0`, in the hit
routine, passes its own `SI` as the number (`+0E77..+0EC5`). Overlay 174 `+04D2` passes the
negated word at its `bp+8`, the object it hands `2C5F:0003` (`+0499..+04D7`), so the flag is set
for an object of 2 or more, the number is -1 for object 1, and it is 0 only for object 0.

**Overlay 176.** `direct_callers.py <dsun> 176+0196` finds one far caller, the dispatcher at overlay
193 `+2317`, which for `d` from 235 to 268 passes `d` less 235 (`+22F8..+231C`). `+0196` passes
that number on, and `+03BD` adds 235 back to it (`+0196..+01C3`, `+0392..+03C2`). Overlay 176
`+048A`, in `+03CD`, passes 235 plus `DI`, which `+03CD` sets to 0 and raises by 1 up to 0x21
(`+03CD..+03E5`, `+0469..+048F`, `+0501..+0505`).

**Overlay 204 `+19AF`**, in `+18CD` (trampoline `5787:0093`), passes as the number the remainder
of its dword argument at `bp+0xC` divided by 1000 (`+18CD..+19C1`). `direct_callers.py <dsun>
204+18CD` finds one caller, the near call at `+027F`. It lies in the case at `+025E` of overlay 204
`+0169`'s switch on its first argument less 1 through the 53 words at `cs:06C7`
(`+017B..+0189`), whose word 46 is 0x025E; the case calls `+18CD` with -1 and the dwords at
`bp+0xA` and `bp+0xE` when its second argument is 9999 (`+025E..+0285`). FND-PARTY-108 shows
`+0169` is reached from the handler of script opcode `0x22`, with the script's arguments.

## Interpretation

Overlay 197 `+1202` queues the number of the hit that is running, or 0, so it carries 104 or 225
only after another source has. Overlay 176 passes psionic powers 235 to 268. The one computed
argument that can be any `DATA` number is overlay 204's: function 46 of script opcode `0x22`,
with 9999 as its second argument, casts the number its fourth argument gives modulo 1000 with no
caster. Whether the shipped scripts do that with 104 or 225 needs the script instruction layouts.

## Alternatives

- Overlay 179 `+0EC0` was read for stores to `SI` from the start of `+0D2F` only along the listing;
  a jump into the routine past `+0D3D` from elsewhere was not looked for.
- What the dwords at `bp+0xA` and `bp+0xE` hold beyond the script's arguments was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `trampoline_target.py <dsun> 56A7:006B
575A:009D`; `direct_callers.py <dsun> 197+111A 176+0196 204+18CD 193+0000`; read the 53 words at
file `0x0008C487`; and `overlay_listing.py` over the ranges above.
