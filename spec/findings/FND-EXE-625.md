---
id: FND-EXE-625
title: The 13 routines of overlay 179 that no entry placed lay points along a line, the outline of a rectangle or an arc of an ellipse, sort points by distance, rotate a point or a list of points, load the VECT angle table once, turn a vector into a byte angle and back, and scale a vector to a length
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000647DA..0x00064F5F
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 13 rows of overlay 179 (header segment `56A7`, 27
trampolines, code from file offset `0x642E0`, 11,445 bytes) that no spec location covered, 1,925
bytes in all, one contiguous range. Each row below is one row of the inventory; "offset" is the
overlay code offset, the file offset is `0x642E0` plus it, and `DS` is `57E0`. Every routine has
a trampoline in `56A7`. Callers are the hits of `direct_callers.py`, checked against the
instructions around them, named by the row that holds them. `T` is the table of 256 signed word
pairs at `4DA1:0060` indexed by a byte angle (FND-RNG-052); the octant tests of `+09CE` put angle
0 toward negative `y`, `0x40` toward positive `x`, `0x80` toward positive `y` and `0xC0` toward
negative `x`. `dist(x0, y0, x1, y1)` is `2D40:382F`, the distance the area sampler uses
(FND-RNG-052). Points are pairs of words in a near buffer, 4 bytes each. All arithmetic on `T` is
`(T * v) >> 8` in 32 bits.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+04FA` | `0x647DA..0x6484B` | `56A7:002F` | Takes an angle `a`, a length `L`, a word `c`, a start `(x0, y0)`, a step `s`, a point buffer and a cap. For `k` from 0 while `k * s < L` and the count is below the cap, takes the point `(0, -k * s)`, rotates it by `a` about `(0, c)` through `+08B5`, adds `(x0, y0)` and stores it. Returns the count. Called from `+056B` four times and overlay 208 `+0E4F` and `+0E7D`. |
| `+056B` | `0x6484B..0x64946` | `56A7:0034` | Takes an angle, a width `w`, a height `h`, `c1`, `c2`, a step, a point buffer and a cap. With `x0 = -(w / 2) - c1`, `y0 = -(h / 2) - c2`, `x1 = w / 2 - c1` and `y1 = h / 2 - c2` (halves truncated toward zero), calls `+04FA` four times, each with `c` 0 and the room the earlier calls left in the buffer and the cap: from `(x0, y0)` at angle `0x40` for `w`, from `(x1, y0)` at `0x80` for `h`, from `(x1, y1)` at `0xC0` for `w` and from `(x0, y1)` at 0 for `h`. Then rotates all the points by the angle about `(0, 0)` through `+0959` and returns their count. Called from overlay 208 `+0DC4`. |
| `+0666` | `0x64946..0x64971` | `56A7:0025` | Passes its eight words to `+0691` after the angles 0 and `0xFF`, and returns what it returns. Called from overlay 208 `+0EB1`. |
| `+0691` | `0x64971..0x64A9D` | `56A7:002A` | Takes angles `a0` and `a1`, a turn `t`, a width `w`, a height `h`, an offset `(dx, dy)`, a spacing `k`, a point buffer and a cap. With `m = (w + h + 1) / 2` and `p = (m * 0x324) >> 8`, sets the count `n = (k / 2 + p) / k` and scales it by `((a1 - a0) & 0xFF) + 1` over 256. For `i` from 0 while `n` and the cap last, takes `T[b]` with `b = (((i * (0x10000 / n0)) >> 8) + a0) & 0xFF`, `n0` being the count before scaling, puts it on the ellipse of half-axes `w / 2` and `h / 2` through `+0BEC`, rotates it by `t` about `(0, 0)` through `+08B5`, adds `(dx, dy)` and stores it. Returns the number stored. Called from `+0689` (in `+0666`) and overlay 208 `+0E25`. |
| `+07BD` | `0x64A9D..0x64B95` | `56A7:0061` | Takes `c1`, `c2`, a point buffer and a count `n`. For each point from the second on, finds by a binary search on `dist(c1, c2, point)` the place among the points before it where it belongs, moves the points from there up by one entry through `289B:01DC` and stores it there, so the buffer ends sorted by distance from `(c1, c2)`, nearest first. Does nothing for `n` below 2. Called from `+0174` in the area sampler `+0000` (FND-RNG-052). |
| `+08B5` | `0x64B95..0x64C39` | `56A7:0048` | Takes a centre `(x, y)`, near pointers to a point's two words and a byte angle `a`. Subtracts the centre from the point, takes `q = +09CE(0, 0, point)` and `r = dist(0, 0, point)`, stores `T[(q + a) & 0xFF]` scaled by `r` back in the point and adds the centre: the point turned by `a` about the centre. Called from `+0082` and `+0097` in `+0000`, `+0522` (in `+04FA`), `+077A` (in `+0691`), `+0987` (in `+0959`), overlay 173 `+36E3`, `+36FF` and `+377B`, and overlay 208 `+0142` and `+015B`. |
| `+0959` | `0x64C39..0x64C78` | `56A7:0043` | Takes a centre `(x, y)`, a point buffer, a byte angle and a count, and turns each point, from the last to the first, about the centre through `+08B5`. Called from `+0162` in `+0000` and `+065A` (in `+056B`). |
| `+0998` | `0x64C78..0x64CAE` | `56A7:0039` | When the byte `DS:0914` is 0, sets it to 1 and requests resource `VECT` 1 through `38FF:04AB` with a far pointer whose value is `4DA1:0060`, the table `T` (FND-PARTY-107), returning the request's result; once the byte is set it returns with `AX` not set. Called from `+0C7A` (in `+0C76`). |
| `+09CE` | `0x64CAE..0x64E10` | `56A7:004D` | Takes `(x0, y0)` and `(x1, y1)` and returns 0 when `dist` of them is 0. Otherwise scales `(x1 - x0, y1 - y0)` by `0x10000 / dist`, giving `(u, v)` of length about 256, picks one of eight octants by the signs of `u` and `v` and which is larger, and from that octant's first angle (0, `0x20`, `0x40`, ... `0xE0`) steps up through `T` until the octant's component of `T` passes `u` or `v`; returns that angle, the direction from the first point to the second. Called from `+0128` in `+0000`, `+0458` in `+025D`, `+08D6` (in `+08B5`), `+0C2A` (in `+0BEC`), overlay 173 `+360E` and `+36BB`, overlay 193 `+257C`, overlay 206 `+0449` and overlay 208 `+020A`, `+0221`, `+09E6`, `+0A0D`, `+0A8A` and `+0DE6`. |
| `+0B30` | `0x64E10..0x64E73` | `56A7:0052` | Takes a byte angle `a`, a length `r` and far pointers to two words, and stores `T[a]` scaled by `r` through them: the vector of length `r` at angle `a`. The angle is not masked. Called from overlay 208 `+0A23`. |
| `+0B93` | `0x64E73..0x64ECC` | `56A7:0057` | Takes near pointers to `x` and `y` and a length `L`, and scales `(x, y)` by `(L << 8) / dist(0, 0, x, y)` so that it has length `L`; a zero vector divides by zero. No direct call was found. |
| `+0BEC` | `0x64ECC..0x64F56` | `56A7:005C` | Takes near pointers to `x` and `y`, a width `w` and a height `h`. Takes the angle `q = +09CE(0, 0, x, y)` and stores `T[q].x` scaled by `w / 2` in `x` and `T[q].y` scaled by `h / 2` in `y`: the point of the ellipse of those half-axes in the direction of `(x, y)`. Called from `+0764` (in `+0691`). |
| `+0C76` | `0x64F56..0x64F5F` | `56A7:003E` | Calls `+0998` and returns. Called from overlay 193 `+20CE` (in `+20C6`, FND-EXE-606). |

## Interpretation

These are the geometry helpers behind the area sampler `+0000` of FND-RNG-052 and the target
cursor of overlay 208. `T` holds a direction vector of length 256 for each byte angle, loaded once
from `VECT` 1 by `+0998`; `+09CE` turns a vector into its angle and `+0B30` an angle and a length
into a vector. `+08B5` and `+0959` turn points about a centre, `+0B93` scales a vector to a length
and `+0BEC` projects a direction onto an ellipse. `+04FA` lays points along a line, `+056B` along
the four sides of a turned rectangle and `+0691` and `+0666` along an arc or the whole of a turned
ellipse, with the number of points set by the spacing and the perimeter `p`, about `3.14 * m`
(`0x324 / 256`). `+07BD` sorts the sampler's points by distance from `(c1, c2)`. The names stay
`fn_179_<offset>` until a reading shows what the outlines are drawn for.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0B93` in particular may be reached only through its trampoline
or a computed far call. `2D40:382F`, `289B:01DC` and `38FF:04AB` are not read here; the reading of
them as a distance, a block move of overlapping entries and a resource request that fills the
caller's buffer rests on how their arguments and results are used and on FND-RNG-052 and
FND-PARTY-107. The values in `VECT` 1 were not read, so the compass directions given for the
angles rest on the octant tests of `+09CE` and assume `T` holds unit vectors in angle order. What
overlay 208 does with the outlines of `+056B`, `+0666` and `+0691` is not read here.

## How to reproduce

Run `overlay_listing.py` for overlay 179 against the installed `DSUN.EXE` over `0x647DA..0x64F5F`
and `direct_callers.py` for each offset (`179+<offset>`), and compare the range with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Read overlay 179 `+0000..+0180` for the arguments the
sampler passes.
