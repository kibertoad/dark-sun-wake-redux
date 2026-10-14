---
id: FND-PARTY-194
title: Overlay 201 +0410, which overlay 173 +3CDA calls when no member is standing, opens window WIND 19502 with no handler, draws BMP number DS:4458 or else BMP 10004 into it, delivers one event and closes the window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py, direct_callers.py, gff_tag_numbers.py)
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089BD0..0x00089D3E
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, where overlay 201's code starts at `0x897C0`. `direct_callers.py` finds one call of
`+0410`, overlay 173 `+3E28` through trampoline `5778:003E` (FND-PARTY-185), and of `+0540` the
near call at `+052D` and the trampoline `5778:0025`.

`+0410` keeps a far pointer in the local dword `[bp - 4]`, set to 0 first (`+0416`).

1. It calls overlay 182 `+1856` (`+041E`), which enters display mode 2 (FND-PARTY-124), and
   `28C9:2ABD` (`+0423`).
2. It calls overlay 182 `+016B` with `0x4C2E` (19502) and a far handler of 0 (`+0428..+0431`),
   which loads and draws a `WIND` resource (FND-PARTY-044), and stores the result at `5499:0000`
   (`+0439..+0443`). `RESOURCE.GFF` has a `WIND` resource numbered 19502.
3. It calls `38FF:04AB` with the tag `BMP `, the byte at `DS:4458` sign-extended as the number and
   the address of `[bp - 4]` (`+0447..+045A`). When that returns 0 it goes to step 5.
4. Otherwise it passes a nonzero `[bp - 4]` to `444C:0092` and stores the result back
   (`+0466..+047C`), and calls `38FF:04AB` again with `BMP ` and 10004 (`0x2714`)
   (`+047F..+0490`). When that returns nonzero and `[bp - 4]` is not 0, it passes `[bp - 4]` to
   `444C:0092` again (`+0498..+04B2`).
5. When `[bp - 4]` is not 0 it calls `2D40:3BEC` with the words 1 and `0x44`, the dword `0x18`,
   `[bp - 4]` and 0 (`+04B5..+04CB`), and then passes `[bp - 4]` to `444C:0092` (`+04D3..+04E9`).
6. It calls `28C9:2A81` with 1 (`+04EC`) and overlay 182 `+059C` with `0x4A9D` (`ICON` 19101,
   FND-PARTY-037) and 1 (`+04F4..+04F9`).
7. While the dword at `5499:0000` is not 0 (`+0530..+053C`), it calls `39D1:0854` with that
   dword and the address of a 24-byte local (`+0503..+0513`), ignores its result, and, when the
   dword is still not 0, calls `+0540` (`+051D..+052D`). It then returns.

`+0540` closes the window at `5499:0000` with overlay 182 `+0128` and stores 0 there when it is not
0 (`+0543..+0568`), then calls overlay 182 `+19F8` (`+0572`), which leaves display mode 2, and
`3D72:0D83` (`+0577`).

`38FF:04AB` returns nonzero when it does not find the resource (FND-COMBAT-004). `gff_tag_numbers.py`
lists `BMP ` 10004 in `RESOURCE.GFF`. The `BMP ` numbers from 1 to 127 that a signed byte can
name are in `OBJEX.GFF`, and none of the other archives has a `BMP ` table but these two.

## Interpretation

When the party restore finds no member standing, the game opens window 19502 with the picture
`BMP` number `DS:4458`, or `BMP` 10004 when that one is missing, calls overlay 182 `+059C` with
`ICON` 19101, takes one event, and closes the window, leaving display mode 2. Nothing in `+0410` or `+0540`
ends the game or reloads one: the routine returns to `+3CDA`, which returns to its caller with no
leader set (FND-PARTY-185).

## Alternatives

- Whether `39D1:0854` waits for input or returns at once when no event is pending was not read
  (FND-PARTY-078 reads it only as passing an event to the window's handler), so whether the
  picture stays until a click is open.
- What `444C:0092` does with an image pointer was not read; the reading of step 4 treats the
  second request as a fallback on the assumption that the first request's nonzero result means
  it failed, as FND-COMBAT-004 reads it.
- Which archive the request searches for a number from `DS:4458`, and what that byte holds when
  `+0410` runs, are not read. FMT-SAVE-002 notes that loading a saved game copies an unwritten
  byte into `DS:4458`.
- What the callers of `+3CDA` do after it returns with no member standing is not read here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 201 89BD0
89D3E`; `direct_callers.py <dsun> 201+0410 201+0540`; and `gff_tag_numbers.py "<install>" "BMP "`
and `gff_tag_numbers.py "<install>" WIND`.
