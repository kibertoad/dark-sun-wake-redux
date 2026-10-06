---
id: FND-PARTY-021
title: START GAME reaches the overlay 182 party loader through two overlay 182 routines, gated on an empty placed-object table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0061..56BD:00B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068BB5..0x00068DA3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069B53
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008126E..0x00081300
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of FBOV overlays 182 and 194; Python 3.14.7 scans of the MZ relocation table, the FBOV headers, trampolines and fixup lists, and the raw bytes of overlay 182; tools/evidence/report.mjs incoming (scientific-method executable reader 2.2.0)
environment: null
---

## Observation

Overlay 182's resident header is at `56BD:0000`; its code is `DSUN.EXE+0x00068850..0x0006AD08`
(9,400 bytes) and it has 41 trampolines. Offsets below marked "overlay offset" are offsets in
that code, so overlay offset `0x063D` is `DSUN.EXE+0x00068E8D`, the party loader of
FND-PARTY-013.

**Trampolines.** Trampoline 20 (`56BD:0084`) targets overlay offset `0x063D`, trampoline 13
(`56BD:0061`) targets `0x0365`, and trampoline 28 (`56BD:00AC`) targets `0x1167`.

**The loader's one caller.** The routine at overlay offset `0x0365` (`DSUN.EXE+0x00068BB5`,
ending with `retf` at `0x0552`) takes four word arguments. Call them A to D in push order from
last to first, so A is at `[bp+6]` and D at `[bp+0Ch]`. A is matched against a seven-word table
at overlay offset `0x0553` (1, `0x37`, `0x3A`, `0x3C`, `0x3D`, `0x3E`, `0x3F`), whose branches
all rejoin at `0x03EE`. There:

1. when the byte of B is 0, the routine jumps to `0x054A` and returns without calling the loader;
2. otherwise, when the word at `DS:264E` is nonzero, it runs a per-slot loop at `0x0414` to
   `0x0536`; when the word is 0 it makes instead the far call `9A 34 00 40 06` at `0x0539`, whose segment
   word `0x0640` is segment-table index 200 shifted left by three;
3. both paths reach `0x053F`: when the byte of C is nonzero, `push cs` and `call 0x063D` at
   overlay offset `0x0546` (`DSUN.EXE+0x00068D96`) call the loader;
4. it clears the byte at `DS:4459` and returns.

**The two callers of `0x0365`.** Both are `push cs; call near` inside overlay 182:

- at overlay offset `0x1300`, in the routine at `0x1167` (trampoline 28, whose common exit is at `0x1522`).
  It pushes D = 1 when the word at `DS:264E` is 4 or less or its byte argument at `[bp+6]` is
  nonzero, otherwise 0; C = 1 when the word at `DS:264E` is 0, otherwise 0; B = 1 when that word is 4 or
  less, otherwise 0; and A = the word at `DS:140C`. Before the call, the routine returns at
  `0x1522` without calling `0x0365` when the word at `DS:0DAB` is nonzero (`0x11BE`) or when
  either of the far calls at `0x1255` and `0x1271` returns `0xFFFF`.
- at overlay offset `0x16C7`, in the routine at `0x15D7`. It pushes C = 0, so this call never
  reaches the loader.

So the loader runs only from the routine at `0x1167`, and only when `DS:264E` holds 0 at
`0x12DD`, which also makes B = 1.

**START GAME reaches `0x1167`.** In overlay 194, the Start Game branch of the start window
callback (FND-UI-035, `DSUN.EXE+0x0008126E`) runs a three-pass loop, calls an overlay 194
routine, stores `0x31` at offset 4 of the segment whose segment-table word is `0x0370`, calls overlay 187's trampolines at its header offsets
`0x00DE` (target `0x2424`) and `0x00E3` (target `0x206E`) with the word at `DS:140C` and the
result, and then, at `DSUN.EXE+0x000812E5`, pushes 0 and makes the far call `9A AC 00 B0 05`.
The fixup list of overlay 194 covers its segment word, which holds `0x05B0`, segment-table
index 182 shifted left by three (FMT-EXE-005), so the call goes to `56BD:00AC` and the routine
at `0x1167` with its byte argument 0.

**Other references searched for.** Each search below found the hits listed and no others.

- Far references to trampolines 20, 13 and 28. All 4,703 MZ relocations were read; the 35 whose
  word is `0x46BD` (overlay 182's header segment, load-relative) pair it with other trampoline
  offsets or, in one data pointer, with `0x0000`. All 8,262 fixups of the 49 overlays were read;
  the 292 whose word is `0x05B0` pair it with other offsets, except two at `0x00AC`:
  `DSUN.EXE+0x000812E8` in overlay 194 (the Start Game call above) and `DSUN.EXE+0x0007DF35` in
  overlay 192. No relocation or fixup pairs the segment with `0x0084` or `0x0061`.
  `tools/evidence/report.mjs incoming` with target `0x00068E8D` reports no candidate among all
  declared MZ relocations and FBOV fixups; its positive control, the far call at
  `DSUN.EXE+0x00068F02` inside the loader, resolves to canonical target `0x0002260A`.
- Near references inside overlay 182. Every byte offset of its code was tried as the start of a
  near call or jump (`E8`, `E9`), a two-byte conditional jump, `0F 8x` jump, short jump or loop
  aimed at `0x063D`, `0x0365` or `0x1167`, independent of any function boundaries. The hits are
  the three calls above. The raw words `0x063D` (once) and `0x0365` (four times) occur only
  across instruction boundaries: in `cmp ax, 6` at `0x0919` and in `les bx, [0x6573]` at
  `0x09D1`, `0x0D06`, `0x0F6C` and `0x0F9A`; `0x1167` does not occur.

Not searched: computed calls and jumps, far pointers built at run time, and near references from
outside overlay 182, which cannot reach its code without a far transfer.

**Writers of `DS:264E` seen in passing.** A raw byte scan of the whole file for stores to
`DS:264E` finds `mov word [264E], 0` at `DSUN.EXE+0x000709B1` and `0x000710A0` (overlay 187) and
`0x00089297` (overlay 200), increments at `0x000271E3`, `0x00027404`, `0x000709C5`,
`0x000710A8` and `0x000892D0`, and add or subtract forms at eleven more places. FND-ACTOR-004
reads the word as the number of entries in use in the placed-object table at `DS:67B7`.

## Interpretation

START GAME reaches the party loader: the Start Game branch calls the overlay 182 routine at
`0x1167`, which calls `0x0365` with C set from `DS:264E == 0`, and `0x0365` calls the loader when
C and B are nonzero. The loader therefore runs when the placed-object table is empty at that
point, which fits a game that has not yet placed anything. Whether the count is 0 there on a new
game depends on what overlay 187's entries `0x2424` and `0x206E` and the calls before
`0x12DD` do to it, which this reading does not cover. The overlay 192 call to `0x1167` is on a
path the start window takes for Load Saved Game (FND-UI-035); a loaded game would normally have
placed objects, so the loader would not run there, but that path was not read.

## Alternatives

- The loader is reached by some other route as well: ruled out for every direct far and near
  reference form searched above; computed calls remain unsearched.
- START GAME never reaches the loader: ruled out as a static path; it can still be skipped at run
  time by the early returns of `0x1167` or a nonzero `DS:264E`.

## How to reproduce

Run `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>` to place overlays 182, 187,
192 and 194. Read overlay 182's header at file offset `0x4BDD0` (`56BD:0000`) and its
trampolines. Disassemble overlay 182 code from `DSUN.EXE+0x00068BB5` to `0x00068DA3` and from
`0x000699B7` to `0x00069B53`, and overlay 194 from `0x0008126E` to `0x00081300`, as 16-bit code.
Read the MZ relocation table and each overlay's fixup list (FMT-EXE-005) for segment words
`0x46BD` and `0x05B0`. For the near search, decode a branch at every byte offset of overlay 182's
code and keep those whose target is `0x063D`, `0x0365` or `0x1167`. For the toolkit check, give
`node tools/evidence/report.mjs incoming` a config with target 429709 and control 429826.
