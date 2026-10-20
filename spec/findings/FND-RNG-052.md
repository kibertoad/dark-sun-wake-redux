---
id: FND-RNG-052
title: Overlay 193 +0617 samples a cast's area through overlay 179 +0000 by the mode of its DATA record's effect row, rolls the row's dice plus a per-a bonus times the row's word 0x0C, and hands the points to one of two readers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E64D..0x0007E6AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E907..0x0007EAE1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000642E0..0x000647DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D37D..0x0005D3BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047110..0x000478F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:06B9..362C:06C4
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py, resident_listing.py, reach_config.py and effect_rows.py at the commit that adds this finding; scientific_method_engine 19.10.0 reach; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. `direct_callers.py <dsun> 193+0617` finds one
call, the near call at overlay 193 `+03B5` in the cast routine `+003C`, and the trampoline
`573B:008E` with no direct far call through it. `+003C` makes the call only when byte `+0x17` of
its `DATA` record is not `0xFF` (`+035A..+0362`), and pushes, as `+0617`'s nine argument words in
order: the `DATA` number, the byte `a` and the byte `g` of FND-RNG-051, its locals at `bp-4` and
`bp-6`, and its own word arguments at `bp+0xE`, `bp+0x10`, `bp+0x12` and `bp+0x14`
(`+039B..+03B5`).

**`+0617`** (`+0617..+07F0`) copies 73 bytes of the record from `2C5F:0003` into its frame with
`1000:0452` and returns if byte `+0x17` is `0xFF`. Otherwise it takes row `R` at `51F1:0000 + 18 *
INT8(byte +0x17)`, one of FND-PARTY-106's 112 records. It calls overlay 208 `+0933` with the
number, `a` and its six word arguments, passing its words at `bp+0x10` and `bp+0x12` and a local
`ang` by address; then overlay 208 `+03BA` with the number and `+0933`'s result, which fills the
locals `w`, `h`, `c1` and `c2` (`bp-0xE`, `bp-0x10`, `bp-0x12`, `bp-0x14`). It calls overlay 179
`+0000` with:

| Argument | Value |
|---|---|
| mode | bits 2 and 3 of `R` byte 5 |
| scale | bits 4 to 7 of `R` byte 5, times 64 |
| factor | the low four bits of `R` byte 6 read as signed, times 64, plus 256 |
| angle, w, h, c1, c2 | `ang`, `w`, `h`, `c1`, `c2` |
| sub | bits 0 and 1 of `R` byte `0x0A` |
| out, cap | a 1000-byte buffer in its frame (`SS:bp-0x44A`), 250 |

and keeps the returned count `n`. Then (`+0713..+0775`): if `R` word `0x0C` is 30000 the amount is
30000; otherwise it calls overlay 173 `+2A5D` with bits 2 to 7 of `R` byte `0x0A`, the low four bits
of `R` byte `0x0B` and 0, adds the high four bits of byte `0x0B` times `a` at word width, widens
the sum as signed and multiplies it by `R` word `0x0C` read unsigned, in 32 bits. `+2A5D` draws the
generator once per count and returns the sum of `draw * sides / 32768 + 1` plus its third argument
(FND-PARTY-100), the arithmetic of `roll_sum` (RULE-RNG-001) with the third argument added. It calls `362C:06B9`, which stores 8 at `DS:29EB` and returns. When the low four
bits of `R` byte 2 are 1 it calls overlay 193 `+20DA` with its words at `bp+0x10` and `bp+0x12`, the
buffer as a far pointer, `n` and byte `+0x17`; otherwise overlay 193 `+07F1` with those two words,
the buffer's near offset, `n`, `R` word `0x0E`, the amount, `R` word `0x10`, byte `+0x17`, `a` and
`g`.

**Overlay 179 `+0000`** (`+0000..+0180`) sets `x0 = -(w / 2) - c1`, `y0 = -(h / 2) - c2`, `x1 = w /
2 - c1` and `y1 = h / 2 - c2`, halves truncating toward zero, and branches on mode:

| Mode | sub | Call |
|---|---|---|
| 0 | 0 | `+08B5(0, 0, &x0, &y0, 0x40)` and the same for `x1`, `y1`, then `x0` and `x1` swapped through `DS:4256`, `n = +0181(x0, y0, x1, y1, scale, factor, out, cap)` and `angle = (angle - 0x40) & 0xFF` |
| 0 | 1 | `n = +0181(x0, y0, x1, y1, scale, factor, out, cap)` |
| 1 | any | `n = +025D(0, 255, w, h, c1, c2, scale, factor, out, cap)` |
| 2 | any | `r = +09CE(h / 2, w, h, 0)`, then `n = +025D(256 - r, r, h, w, c1, c2, scale, factor, out, cap)` |

Mode 0 with another `sub`, and mode 3, call no sampler and leave `n` at its uninitialized local
`bp-0xA`. Every branch then calls `+0959(0, 0, out, angle, n)` and `+07BD(c1, c2, out, n)` and
returns `n`. The near call targets are `+08B5`, `+0181`, `+025D`, `+09CE`, `+0959` and `+07BD`
(file `0x64B95`, `0x64461`, `0x6453D`, `0x64CAE`, `0x64C39`, `0x64A9D`).

**`+0181`** (`+0181..+025C`), arguments `x0, y0, x1, y1, scale, factor, out, cap`: `step = 25600 /
INT32(INT16(y1 - y0))`, `d = (INT32(scale) * step) >> 8`, `acc = INT32(x0) << 8`, `i = 0`. While `i
< cap` and `acc >> 8 < x1`: `r1 = random_between(0, INT16(d >> 8))`, `r2 = random_between(y0,
y1)`, `r3 = random_between(256, factor)`; word `out[2i] = INT16(acc >> 8) + r1`, word `out[2i + 1]
= INT16((INT32(r2) << 8) / INT32(r3))`; `acc += d`, `i += 1`. It returns `i`. The stores go through
`DS`.

**`+025D`** (`+025D..+04F9`), arguments `a0, a1, w, h, c1, c2, k, factor, out, cap`: `m = (w + h) /
2`; `s = 71680000 / INT32(INT16(3 * m))^2`; `k = INT16((INT32(k) * s) >> 8)`, set to 1 if 0; `fx =
(INT32(w) << 8) / 512`, `fy = (INT32(h) << 8) / 512`; `span = ((a1 - a0) & 0xFF) + 1`; `base =
2D40:382F(0, 0, c1, c2)`; `acc = INT32(a0) << 8`; `last = a0`; `i = 0`. While `span > 0` and `i <
cap`:

1. `b = (acc >> 8) & 0xFF`; if `b != last`, `span -= (b - last) & 0xFF`; `last = b`.
2. `vx = INT16((T[b].x * fx) >> 8)`, `vy = INT16((T[b].y * fy) >> 8)`, `T` the signed word pairs
   at `4DA1:0060`; `r = 2D40:382F(0, 0, vx, vy)`.
3. `d1 = random_between(0, r)`, `d2 = random_between(32, 48)`, `u = r - INT16(INT16(d1 << 5) /
   d2)`; `d3 = random_between(256, factor)`, `u = INT16((INT32(u) << 8) / INT32(d3))`.
4. `f = (INT32(u) << 8) / INT32(r)`; `vx = INT16((INT32(vx) * f) >> 8)`, `vy` the same; `e = r -
   2D40:382F(0, 0, vx, vy)`; `e = INT16((INT32(e) * ((INT32(base) << 8) / INT32(r))) >> 8)`.
5. `q = +09CE(0, 0, c1, c2)`; `vx += INT16((T[q].x * e) >> 8)`, `vy += INT16((T[q].y * e) >> 8)`;
   word `out[2i] = vx`, `out[2i + 1] = vy` through `DS`.
6. `acc += k`, `i += 1`.

It returns `i`. The test of `k` against 0 at `+04CB`, which would return 1, cannot pass after the
prologue.

**Reach.** A reach query (FND-RNG-050's six targets and 112 declarations, the start's own call as
the control) from overlay 179 `+0000` walks 17 routines and reaches only the generator, through
`random_between` at `+0384` in `+025D`. Queries from `+07BD`, `+08B5`, `+0959` and `+09CE`, whose
walks include `2D40:382F`, and from overlay 208 `+0933` and `+03BA`, whose walks include
`2C5F:0003`, reach no target. Queries from overlay 193 `+07F1` and `+20DA` walk 1835 routines and
reach the seed setter, the generator, `roll_sum` and `random_between`; their shortest chains run
through overlay 193 `+12A8` (from `+07F1` by `+0AAD`, from `+20DA` by `31E0:0121` and `+0DC7`) into
the hit routine and the cast routine.

**The effect rows.** `effect_rows.py <dsun> <resource>` prints the 112 rows and the `DATA` records
naming each. Mode is 0 in 37 rows (sub 0 in 4, 1 in 33), 1 in 61 and 2 in 14; no row has mode 3.
The low four bits of byte 6 are 0 in every row, so the factor is 256. The low four bits of byte 2
are 0 in 4 rows, 1 in 54, 2 in 13 and 3 in 41. Word `0x0C` is 2 in 50 rows, 60 in 36, 600 in 25
and 30000 in one, row 28 (`DATA` 67). 51 rows have a dice count of 0. 99 rows are named by at
least one `DATA` record; 216 records hold -1 and `DATA` 73 holds -38.

## Interpretation

`+0617` is the area step of a cast: it turns the record's effect row into up to 250 point pairs
and an amount, and gives both to a reader, `+20DA` for rows whose byte 2 is 1 and `+07F1` for the
rest. Mode 0 scans a box of width `w` and height `h` centred on `(-c1, -c2)` in columns of random
height, with the corners first passed through `+08B5` with 0x40 when `sub` is 0 (read here as a
quarter turn, which the angle's later reduction by 0x40 undoes); mode 1 samples a full turn of the table at
`4DA1:0060` and mode 2 a sector of half-width `r` around angle 0, each point pulled in from the
edge by a random part of its radius. Since the factor is always 256, each `+0181` point costs a
draw for `r1` when `d >> 8` is positive and one for `r2` when `y0 < y1`, and each `+025D` point a
draw for `d1` when `r` is positive and always one for `d2`. The amount draws the row's dice count
times, after all sample draws, and not at all for row 28.

## Alternatives

- What overlay 208 `+0933` and `+03BA`, `+08B5`, `+09CE`, `+0959`, `+07BD` and `2D40:382F` compute
  was not read, only that they reach no draw. The names `w`, `h`, `c1`, `c2` and `ang` follow their
  use in `+0000`.
- The readers `+07F1` and `+20DA` were not read past their arguments; the reach chains show
  routes to draws, not that a reader takes them for given inputs.
- The buffer is in `SS` and the samplers store through `DS`; this reading takes them to be the same
  segment, as in a program whose stack is in its data group.
- `DATA` 73's -38 would select a row before `51F1:0000`; whether a cast of `DATA` 73 reaches
  `+0617` was not read.
- Calls through a stored pointer to `+0617` were not searched beyond the trampoline.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE` and `<resource>` the installed `RESOURCE.GFF`: `direct_callers.py <dsun> 193+0617
179+0000 179+0181 179+025D 173+2A5D`; `overlay_listing.py <dsun> 173 5A920 5EA7C`, `179 642E0
66F95`, `193 7E2F0 80ECE` and `208 91890 93088`; `resident_listing.py <dsun> 362C:06B9..362C:0760
2C5F:0003..2C5F:0040 2D40:382F..2D40:3900`; `effect_rows.py <dsun> <resource>`. For each reach
start, copy FND-RNG-050's query with that one start, a call inside it as the control and that
call's callee as the instruction control, run `reach_config.py` and then `python -I -m
scientific_method_engine reach`. Outputs are kept under `GAME_DIR/analysis/rng-callers-20261020/`
and `GAME_DIR/analysis/rng-0617-20261020/`. Nothing was executed.
