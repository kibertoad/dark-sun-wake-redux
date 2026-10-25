---
id: FND-SCRIPT-028
title: The trigger walkers of segment 1695 run the first record of a list whose cell, box, key or key pair matches, after storing the trigger's actors and cell in the script variables, only while the script state 4C0E:000B is 2; a region change frees the cell and box triggers and those not registered by MAS 99
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1695:0000..1695:0970
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E6..2D40:0747
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0AE5..2D40:0B00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:00A1
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/resident_listing.py, direct_callers.py, immediate_search.py and segment_offset_refs.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000. A record is FMT-SCRIPT-004, at the far pointer
`57E0:40C0`; the list heads are the words `4C10:0005` to `4C10:0017` (FND-SCRIPT-027). "Run"
means `172C:000C(word +2, word +0, 1)`, the interpreter's entry. `number(x)` is `2D40:0AE5(x)`:
`x` itself when negative, otherwise field 0 of object `x` through `1AA0:0009(x, 0)`.
`2D40:06E6(id)` stores the cell of the object `id` names (its pixel words `+0x0A`, `+0x0C` shifted
right by 4) in `4C10:001B` and `4C10:001D` and 0 in `4C10:001F`.

Each walker below returns 0 at once unless the word `4C0E:000B` is 2. Each runs only the first
record that passes and returns 1, or 0 when none does.

- **Cell, `1695:0435(head, slot, x, y, kind)`**: stores `x`, `y`, `kind` in `4C0D:0005`, `0003`,
  `0001` and `slot` in `4C13:0367`, `0365` and `0363`, calls `2D40:06E6(slot)`, then `1695:04A9`
  runs the first record with word `+4` = `x`, word `+6` = `y` and byte `+8` not below `kind`
  (signed compare of the zero-extended byte).
- **Box, `1695:0573`**: the same, with `1695:05E7` testing `x` from word `+4` to word `+4` + byte
  `+8` - 1 and `y` from word `+6` to word `+6` + byte `+9` - 1 (signed, inclusive) and byte `+0x0A`
  not below `kind`.
- **`1695:01D1(slot, x, y, kind)`**: the cell walker on head 5, then, when it returned 0, the box
  walker on head 9; it returns the last one's result.
- **`1695:0235(slot, x, y, kind)`**: clears `4C13:0325`, runs the cell walker on head 7 and then the
  box walker on head `0x0B` when nothing ran, and returns `4C13:0325`.
- **One key, `1695:06FF(head, a, b)`**: stores `number(b)` in `4C0D:0009`, `a` in `4C13:0367` and
  `b` in `4C13:0365` and `0363`, calls `2D40:06E6(a)`, and `1695:075D` runs the first record whose
  word `+4` equals `4C0D:0009`. `1695:0218`, `0290`, `02AD`, `02CA` and `02E7`, each `(a, b)`, call
  it with heads `0x0F`, `0x0D`, `0x11`, `0x13` and `0x15`.
- **Key pair, `1695:0304(a, b, c)`**: stores `number(b)` in `4C0D:0009` and `number(c)` in
  `4C0D:0007`, `a`, `b` and `c` in `4C13:0367`, `0365` and `0363`, calls `2D40:06E6(a)`, and
  `1695:037E` runs the first record of head `0x17` whose words `+4`, `+6` equal those two numbers in
  either order.
- **`1695:0834(object)`**, with no state test, stores `number(object)` in `4C0D:0009` and returns a
  mask with bit `0x01` for head `0x0D`, `0x02` for `0x11`, `0x04` for `0x0F`, `0x08` for `0x13`,
  `0x10` for `0x15` and `0x20` for `0x17` when `1695:08DB` finds a record keyed by it. `1695:08DB`
  compares word `+4`, and word `+6` as well for the record that is the head of list `0x17` only.
- **`1695:000B(region)`** stores `region` in `4C10:0021`. When the state is 2 it frees every record
  of heads 5, 7, 9 and `0x0B`, the records of heads `0x0D`, `0x0F`, `0x11`, `0x13` and `0x15` whose
  byte `+6` is 0 (`1695:010F`) and those of head `0x17` whose byte `+8` is 0 (`1695:0170`), putting
  each on the free list `4C13:032D` (`1695:07DD`), then calls `172C:000C(region, 0, 2)` and clears
  `4C13:0326`.

`direct_callers.py` finds no direct call of `1695:0235`, and neither a far pointer to it nor its
offset pushed was found. `172C:000C` clears `4C13:0325` at `172C:0049` on every run; only `0x47`
sets it (FND-SCRIPT-027). `segment_offset_refs.py <dsun> 4C0E B` finds the stores to `4C0E:000B`
only in overlay 169: -1 at `+0028` and `+015C`, 1 at `+0113`, `+0260` and
`+0521`, the inner set-up's result at `+0136`, 0 at `+0226` and 2 at `+040F`, the end of the inner
set-up `+023B`. Outside overlay 169 it is only compared with 2, by `172C:000C` and by `1695:000B`,
`0304`, `0435`, `0573` and `06FF`.

## Interpretation

A cell or box trigger fires for an actor of kind 0 (the leader), 1 (another party member) or 2
(anyone else) when the trigger's byte allows that kind: 0 the leader only, 1 the party, 2 anyone.
When a trigger fires, the script sees the one who set it off in `4C13:0367` and the other object in
`4C13:0365` and `4C13:0363`, the actor's cell in `4C10:001B` and `4C10:001D`, and the tile trigger's
cell and kind in `4C0D`. Triggers fire only once the script system is set up (state 2). At a region
change the new region's `MAS ` script runs and registers its own triggers; those registered by
`MAS ` 99 stay. The walker `1695:0235` for heads 7 and `0x0B` is never reached by a direct call.

## Alternatives

That field 0 of an object is its number rests on the shipped keys, which are negative literals,
and on overlay 213 testing item numbers before it calls `1695:02E7`; the per-kind field offsets
at `DS:5A33` are not in the file's initial data. Only the first record of list `0x17` is tested on
both keys in `1695:08DB`; whether that is what the mask's callers expect was not checked. A computed
far call to `1695:0235` is not excluded, since computed transfers in resident code remain
unresolved (FND-PARTY-129).

## How to reproduce

`resident_listing.py <dsun> 1695:0000..1695:0970 1695:096B..1695:09A0`, `... 2D40:06C4..2D40:0760
2D40:0AE5..2D40:0B20 1AA0:0009..1AA0:0130 172C:000C..172C:00A1`; `direct_callers.py <dsun>` with
each entry of segment `1695` above; `immediate_search.py <dsun> 0325 0326 0235`; `segment_offset_refs.py <dsun> 4C0E B`.
