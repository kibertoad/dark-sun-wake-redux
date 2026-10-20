---
id: RULE-MAGIC-023
title: A cast samples up to 250 area points by its DATA record's effect row, as a box scan, a full turn or a sector, and rolls the row's dice plus a bonus per a, times the row's word 0x0C, as the amount
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-052, FND-RNG-026, FND-RNG-051, FND-PARTY-106, FND-PARTY-100, FND-RNG-012, FND-RNG-035, FND-PARTY-112]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-016]
---

## Summary

`cast_area` is overlay 193 `+0617`. For a `DATA` record whose byte `+0x17` names a row of the
effect table at `51F1:0000`, it samples point pairs into a 250-pair buffer by the row's mode: mode 0
scans a box in columns, mode 1 samples a full turn of directions and mode 2 a sector. It rolls the
row's dice, adds the row's bonus per `a` and multiplies by the row's word `0x0C`, unless that word
is 30000, which is the amount itself. Then it passes the points and the amount to a reader.

## When it runs

When the cast routine overlay 193 `+003C` applies a `DATA` record whose byte `+0x17` is not
`0xFF`, after its own steps up to `+0362` (FND-RNG-052). `a` and `g` are those of FND-RNG-051.

## Parameters

- `n_data: INT16`: the `DATA` number.
- `a: UINT8`, `g: UINT8`: the cast's bytes (FND-RNG-051).
- `p: INT16[6]`: `+0617`'s six other word arguments, two locals and four word arguments of
  `+003C`.

## Inputs

`data_record`'s byte `+0x17`; the row `R` of `effect_rows` it numbers, 18 bytes at `51F1:0000 + 18
* INT8(byte +0x17)` (FND-PARTY-106); the signed word pairs of `g_4DA1_0060`.

## Procedure

```text
define cast_area(n_data, a, g, p):
    let D = data_record(n_data)
    if D[0x17] == 0xFF:
        return
    let R = effect_rows[INT8(D[0x17])]
    let ang: INT16 = 0
    let w: INT16 = 0
    let h: INT16 = 0
    let c1: INT16 = 0
    let c2: INT16 = 0
    # sets ang, p[2] and p[3]; makes no draw
    let k = fn_57A6_0025(n_data, a, p[0], p[1], p[2], p[3], p[4], p[5], ang)
    # sets w, h, c1 and c2; makes no draw
    fn_57A6_0034(n_data, k, w, h, c1, c2)
    let mode = (R[5] >> 2) & 3
    let scale = ((R[5] >> 4) & 0x0F) << 6
    let factor = (INT16(R[6] << 12) >> 12) * 64 + 256
    let out: INT16[500]
    let n = sample_area(mode, scale, factor, ang, w, h, c1, c2, R[0x0A] & 3, out, 250)
    let amount: INT32 = 30000
    if R.word_0C != 30000:
        # roll_sum draws (R[0x0A] >> 2) & 0x3F times (RULE-RNG-001)
        let roll = roll_sum((R[0x0A] >> 2) & 0x3F, R[0x0B] & 0x0F)
        amount = INT32(INT16(roll + ((R[0x0B] >> 4) & 0x0F) * a)) * UINT16(R.word_0C)
    fn_362C_06B9()
    if (R[2] & 0x0F) == 1:
        fn_573B_00E8(p[2], p[3], out, n, D[0x17])
    else:
        fn_573B_0093(p[2], p[3], out, n, R.word_0E, amount, R.word_10, D[0x17], a, g)

define sample_area(mode, scale, factor, angle, w, h, c1, c2, sub, out, cap) -> INT16:
    let x0 = -(w / 2) - c1
    let y0 = -(h / 2) - c2
    let x1 = w / 2 - c1
    let y1 = h / 2 - c2
    # n keeps the stale frame word when no branch below is taken (Edge cases)
    let n: INT16 = 0
    if mode == 0 and sub == 0:
        # each call updates the two words it is given
        fn_56A7_0048(0, 0, x0, y0, 0x40)
        fn_56A7_0048(0, 0, x1, y1, 0x40)
        let t = x0
        x0 = x1
        x1 = t
        n = box_scan(x0, y0, x1, y1, scale, factor, out, cap)
        angle = (angle - 0x40) & 0xFF
    else if mode == 0 and sub == 1:
        n = box_scan(x0, y0, x1, y1, scale, factor, out, cap)
    else if mode == 1:
        n = turn_scan(0, 255, w, h, c1, c2, scale, factor, out, cap)
    else if mode == 2:
        let r = fn_56A7_004D(h / 2, w, h, 0)
        n = turn_scan(256 - r, r, h, w, c1, c2, scale, factor, out, cap)
    fn_56A7_0043(0, 0, out, angle, n)
    fn_56A7_0061(c1, c2, out, n)
    return n

define box_scan(x0, y0, x1, y1, scale, factor, out, cap) -> INT16:
    let step = 25600 / INT32(INT16(y1 - y0))
    let d = (INT32(scale) * step) >> 8
    let acc = INT32(x0) << 8
    let i = 0
    while i < cap and (acc >> 8) < x1:
        let r1 = random_between(0, INT16(d >> 8))
        let r2 = random_between(y0, y1)
        let r3 = random_between(256, factor)
        out[2 * i] = INT16(INT16(acc >> 8) + r1)
        out[2 * i + 1] = INT16((INT32(r2) << 8) / INT32(r3))
        acc = acc + d
        i = i + 1
    return i

define turn_scan(a0, a1, w, h, c1, c2, k, factor, out, cap) -> INT16:
    let m = (w + h) / 2
    let sq = INT32(INT16(3 * m)) * INT32(INT16(3 * m))
    k = INT16((INT32(k) * (71680000 / sq)) >> 8)
    if k == 0:
        k = 1
    let fx = (INT32(w) << 8) / 512
    let fy = (INT32(h) << 8) / 512
    let span = ((a1 - a0) & 0xFF) + 1
    let base = fn_2D40_382F(0, 0, c1, c2)
    let acc = INT32(a0) << 8
    let last = a0
    let i = 0
    while span > 0 and i < cap:
        let b = (acc >> 8) & 0xFF
        if b != last:
            span = span - ((b - last) & 0xFF)
            last = b
        let vx = INT16((g_4DA1_0060[2 * b] * fx) >> 8)
        let vy = INT16((g_4DA1_0060[2 * b + 1] * fy) >> 8)
        let r = fn_2D40_382F(0, 0, vx, vy)
        let d1 = random_between(0, r)
        let d2 = random_between(32, 48)
        let u = INT16(r - INT16(INT16(d1 << 5) / d2))
        let d3 = random_between(256, factor)
        u = INT16((INT32(u) << 8) / INT32(d3))
        let f = (INT32(u) << 8) / INT32(r)
        vx = INT16((INT32(vx) * f) >> 8)
        vy = INT16((INT32(vy) * f) >> 8)
        let e = INT16(r - fn_2D40_382F(0, 0, vx, vy))
        e = INT16((INT32(e) * ((INT32(base) << 8) / INT32(r))) >> 8)
        let q = fn_56A7_004D(0, 0, c1, c2)
        vx = INT16(vx + INT16((g_4DA1_0060[2 * q] * e) >> 8))
        vy = INT16(vy + INT16((g_4DA1_0060[2 * q + 1] * e) >> 8))
        out[2 * i] = vx
        out[2 * i + 1] = vy
        acc = acc + k
        i = i + 1
    return i
```

Division truncates toward zero and `>>` is arithmetic, as in RULE-RNG-001. `random_between` and
`roll_sum` are RULE-RNG-001's; overlay 173 `+2A5D`, which `+0617` calls for the amount, is
`roll_sum` plus its third argument, here 0 (FND-PARTY-100, FND-RNG-052). `R.word_0C`, `R.word_0E`
and `R.word_10` are the row's words at `+0x0C`, `+0x0E` and `+0x10`. The draws come in the order shown: every point's draws in `sample_area`, then the
amount's.

## Outputs

Up to 250 point pairs, their count and the amount, given to overlay 193 `+20DA` or `+07F1`, and
`g_57E0_29EB` set to 8 by `fn_362C_06B9`.

## Edge cases

Every shipped row has a factor of 256, so `r3` and `d3` never draw and divide by 256. `d2` draws
for every `turn_scan` point; `r1`, `r2` and `d1` draw only when their upper bound is above the
lower. A row with a dice count of 0 (51 of 112) makes no amount draw, and row 28 (`DATA` 67), whose
word `0x0C` is 30000, none either. `box_scan` divides by `y1 - y0` before its loop and `turn_scan`
by `r` in every point, so a height of zero or a point of radius zero is a division by zero. Mode 0
with a `sub` other than 0 or 1, and mode 3, sample nothing and pass on an unset count; no shipped
row has them. `DATA` 73's byte `+0x17` of -38 selects memory before the table.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_57A6_0025` and `fn_57A6_0034` return as `ang`, `w`, `h`, `c1` and `c2`, what
  `fn_56A7_0048`, `fn_56A7_004D`, `fn_56A7_0043`, `fn_56A7_0061` and `fn_2D40_382F` compute, what
  the readers `fn_573B_00E8` and `fn_573B_0093` do with the points and the amount, including
  whether they draw, what reads `g_57E0_29EB`, which `fn_362C_06B9` sets, what the pairs of
  `g_4DA1_0060` hold, and what the bytes of `effect_rows` this rule does not use stand for
  (FND-RNG-052, Q-MAGIC-017).
