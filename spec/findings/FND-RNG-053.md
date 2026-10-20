---
id: FND-RNG-053
title: The game reseeds once at startup and then only through overlay 204's offset loop, from scripts, overlay 201 and a cast of DATA 293 with the cast routine's byte 0x1A set; SS changes only at startup and in a balanced swap, and no routine on the seed paths writes DS
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E46C..0x0007E491
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E64A..0x0007E70B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065528..0x00065541
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x00067899
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1464..1000:149B
tool: tools/research/exec-census/direct_callers.py, far_pointer_words.py, overlay_listing.py, resident_listing.py, data_bytes.py and segment_register_writes.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-009 finds the seed setter `1000:0811`
called only at overlay 180 `+065E` and overlay 204 `+001E`, and FND-RNG-045 no stored pointer to
it. `direct_callers.py <dsun> 1000:0963 180+0141 204+0000` gives:

- the clock helper `1000:0963` only the two far calls just before them, overlay 180 `+0657` and
  overlay 204 `+0017`;
- overlay 180 `+0141`, the routine holding `+065E`, one caller, the far call at file `0x0001CC2F`
  in the startup routine `277D:0004` (FND-PARTY-031), through the trampoline `56B2:0020`;
- overlay 204 `+0000`, which reseeds at `+001E` before testing anything (FND-RNG-021), the near
  call at `+1F3D` and the far calls at overlay 193 `+0413` and overlay 201 `+075F` and `+0810`
  (FND-RNG-022), through the trampoline `5787:006B`.

`far_pointer_words.py <dsun> 56B2:0020 5787:006B 1000:0963` finds the offset words of those
addresses with a segment word that names them only in these far calls.

**Overlay 180 `+0141`** calls `+0657` and `+065E` after its loading steps and before its last
calls (`+0641..+0663`); its branches were not read one by one, and FND-RNG-010 observes the seed
it stores on a fresh start.

**Overlay 193 `+0413`** is in the cast routine `+003C`, whose `DATA` number is its word argument at
`bp+0xA` (in `DI`). At `+017C` it tests its byte argument at `bp+0x1A`: when 0 it makes two calls
of `2C5F:0A22` and jumps to `+041B`, past the reseed. Otherwise every path reaches `+035A`. There,
when the record's byte `+0x17` is not `0xFF` it calls `+0617` (FND-RNG-052) and goes on at `+03DD`
when the effect row's byte 2 has low four bits 1, and otherwise to `+05C9`; when byte `+0x17` is
`0xFF` it calls overlay 197 `+3315` if bit 0 of the record's word `+0x11` is set and goes on at
`+03DD`, which then takes it to `+0405`. At `+0405` it calls overlay 204 `+0000` with magnitude 4,
mode 4 and count 4 when the number is 293. `data_bytes.py <resource> 17` gives `DATA` 293 byte
`+0x17` -1, and offsets 11 and 12 give its word `+0x11` as `0x4000`.

The byte at `bp+0x1A` comes from the two callers of `+003C`: overlay 193 `+0000` passes its own
(`+0006..+0029`), and the cast queue overlay 179 `+11DC` passes 1 (`+1248..+125C`). Of `+0000`'s
callers (FND-RNG-051), overlay 176 `+048A` and overlay 204 `+19AF` pass 1, overlay 176 `+03BD` its
byte argument at `bp+0x16`, and overlay 193 `+22CF` its byte argument at `bp+0x1C`; overlay 177
`+04FE` was not read.

**Segment registers.** `segment_register_writes.py <dsun> <coverage>` decodes the 2339 routines of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Writes of `SS` are at `1000:0000` (startup), twice in
`1000:1464`, and ten times in eight routines of the trampoline segments `56EF`, `5702`, `573B`, `575A`,
`5773`, `5799` and `57A6`, which decode the stub bytes after `int 0x3f`, not code. `1000:1464`
loads `SS` from `CS:135F` with interrupts off, copies two words and loads the saved `SS` back
before `popf`. `DS` is written 628 times in 255 routines, none of them overlay 204 `+0000`, overlay
180 `+0141`, `1000:0963`, its date and time wrappers `1000:046E` and `1000:0481`, overlay 193
`+003C`, overlay 201 `+05D5` or overlay 204 `+1F06`.

## Interpretation

The game seeds once at startup, from overlay 180, and afterwards only through overlay 204's offset
loop (RULE-VIDEO-005): for request 27 of script instruction `0x22` (FND-RNG-027), in overlay 201's
removal effect (FND-RNG-050), and for a cast of `DATA` 293 whose byte `bp+0x1A` is not 0, which
every queued cast has. Each call reseeds from the clock helper's low word whatever its count.

The helper's buffers are offsets in its `SS` frame that its wrappers store through `DS`. `SS`
keeps its startup value outside the swap in `1000:1464`, and nothing on the seed paths from the
callers above writes `DS`, so the stores reach the frame whenever `DS` equals `SS` at the entry of
those callers, as FND-RNG-014 measured at startup.

## Alternatives

- `DS` at the entry of the callers is the value their own callers leave; the 255 routines that
  write `DS` were not checked to restore it, so equality on the paths other than startup rests
  on that.
- The census reads only the routines the coverage table lists; code outside them, such as
  interrupt handlers it lacks, could load `SS`.
- Which spells or actions cast `DATA` 293, and what the byte `bp+0x1A` means, were not read; the
  arguments of overlay 176 `+03BD`, overlay 193 `+22CF` and overlay 177 `+04FE` were not followed.
- Indirect calls of overlay 193 `+003C` and `+0000` beyond their trampolines were not searched.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`, `<resource>` the installed `RESOURCE.GFF` and `<coverage>` the repository's
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `direct_callers.py <dsun> 1000:0963 1000:046E 1000:0481
180+0141 204+0000 193+003C`; `far_pointer_words.py <dsun> 56B2:0020 5787:006B 1000:0963`;
`overlay_listing.py <dsun> 176 61540 62067`, `179 642E0 66F95`, `180 671E0 67D64`, `193 7E2F0
80ECE` and `204 8BDC0 8E660`; `resident_listing.py <dsun> 1000:1464..1000:149B`; `data_bytes.py
<resource> 17`, `11` and `12`; `segment_register_writes.py <dsun> <coverage> 0x0008BDC0 1000:0963
1000:046E 1000:0481 0x00067321 0x0007E32C 0x00089D95 0x0008DCC6`. Outputs are kept under
`GAME_DIR/analysis/rng-reseed-20261020/` and `GAME_DIR/analysis/rng-callers-20261020/`. Nothing
was executed.
