---
id: FND-EXE-619
title: The 2 routines of overlay 195 that no entry placed pass every loaded object slot's 16-byte flag block to overlay 197, and add 60 times a count to the double word at 4C13:035B
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082C45..0x00082C9F
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 2 rows of overlay 195 (header segment `574E`, 14
trampolines, code from file offset `0x81770`, 5,690 bytes) that no spec location covered, 90
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x81770` plus it, and `DS` is `57E0`. Both routines have a trampoline in `574E`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A slot's state byte is at `4F49:0C33 + 3 * slot`, and overlay 193
`+1BA3` (trampoline `573B:0025`) fills a 16-byte flag block for a slot (FND-PARTY-112).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+14D5` | `0x82C45..0x82C85` | `574E:0052` | For each slot 0 to `0x13F` whose state byte is 2, fills a 16-byte stack block for it through overlay 193 `+1BA3` (slot, near block) and calls overlay 197 `+29E0` (trampoline `575A:00CA`) with the slot and the block's far address. Called from overlay 173 `+0795`. |
| `+1515` | `0x82C85..0x82C9F` | `574E:0057` | Takes `n` and adds `60 * n`, the product cut to 16 bits and sign-extended, to the double word at `4C13:035B`. Called from the resident routine `2D40:048D` (at `2D40:0493`) and overlay 204 `+129D`. |

## Interpretation

`+14D5` runs one overlay 197 routine over every loaded object with its current flags, which fits a
per-round pass over the effects on each combatant from overlay 173's fight routine. `+1515`
advances the same double word that overlay 173 `+0C65` advances by 60 when the byte at `DS:054E`
is 0 (FND-RNG-042), so `n` reads as a count of the steps that code takes and the double word as a
clock. The
names stay `fn_195_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Overlay 197 `+29E0`, `2D40:048D` and overlay 204 `+129D` are
not read here, so the per-round reading of `+14D5` and the clock reading of `4C13:035B` rest only
on the callers named and on FND-RNG-042. For `n` above 546 the 16-bit product wraps, and the
amount added is negative from 547.

## How to reproduce

Run `overlay_listing.py` for overlay 195 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`195+<offset>`), map the resident caller to
its routine with the inventory, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
