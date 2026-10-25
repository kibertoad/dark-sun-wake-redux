---
id: FND-RNG-078
title: Overlay 173 +059A stores through its pointer only at +07CF, always 9999 and only after +0729, and at +07FA, never 9999
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005B175
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 173 code starts at file offset `0x5A920`. In `+059A..+0855`, `DI` is loaded from the pointer
argument at `+05A2` and changed only at `+083C`, after the last store through it; the only
instructions that store through `DI` are `mov [di], si` at `+07CF` and at `+07FA`.

- `+07CF` follows `+078D`, which sets `SI` to 9999, and is reached only from `+0729`; FND-PARTY-089
  lists the two routes into `+0729`.
- `+07FA` is reached from `+06DD`, after `+06C4..+06C8` jumps away when `SI` is 9999, and from
  `+07F5`, after `+07F1` jumps away when `SI` is 9999. On the second route `SI` comes from
  `+06EA` (what `+1098` returns) or from `+07DC` (what `2D40:2F34` returns, below `0x30` there,
  since `+07DE..+07EE` sets 9999 for a result of `0x30` or more).

`SI` starts as the word at the pointer (`+05A9`). The state word's cases at `+05DC` and `+06C1` set
it to 9999 (`+0656`, `+06C1`), the case at `+0660` sets it to 9999 or leaves it, and the case at
`+0690` leaves it.

## Interpretation

`+059A` writes 9999 through its pointer only on its end-of-combat path, and otherwise writes either
the word that was already there or a slot that `+1098` or `2D40:2F34` returned.

## Alternatives

`+1098` returns 9999 or a slot below `0x30` (FND-RNG-042), so a value above 9999 reaches `+07FA`
only when the word at the pointer already held it.

## How to reproduce

`overlay_listing.py <dsun> 173 5AEBA 5B175`, and search the listing for stores through `DI` and
for writes to `DI`.
