---
id: FND-ITEM-010
title: The hit routine's cases for DATA 103, 235, 275, 283 and 284 run overlay 191's item tests on the target and set bit 0x02 of the hit's flags byte when a test returns 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000669DB..0x00066A36
tool: tools/research/exec-census/overlay_listing.py at commit c40653e3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 179's code starts at file `0x642E0`.
Overlay 179 `+1373`, the effect switch of the hit routine, keeps the target `t` in `DI` and a
near pointer to the hit's flags byte at `bp+0x12` (FND-RNG-034). Its 81-entry case table sends
`DATA` 235 to `+26FB`, 103 to `+271A`, 284 to `+2734`, and 275 and 283 to `+2741`. Decoding
`+26FB..+2756` gives:

| Case | Calls, in order | When a call returns 1 |
|---|---|---|
| 235 (`+26FB`) | overlay 179 `+12B1` with `t`, the second slot `c`, 311 and the byte at `bp+0x14`; then overlay 191 `+0D60` with `t` | sets the bit |
| 103 (`+271A`) | overlay 191 `+0CD0` with `t`; when it returns 0, overlay 191 `+0E3A` with `t` | sets the bit |
| 284 (`+2734`) | overlay 191 `+0CD0` with `t` | sets the bit |
| 275, 283 (`+2741`) | overlay 191 `+0E3A` with `t` | sets the bit |

Each test's byte return is tested with `or al, al`. A return other than 0 goes to `+274C`, which
sets bit 0x02 of the byte at the near pointer `bp+0x12`; a return of 0 goes to `+2752`, past that
store. Both continue at `+2752`, the code after the switch, which tests the byte at `bp+0x10`.
So for `DATA` 103 the second test runs only when the first returned 0, and `DATA` 235 sets the
bit on every call, since overlay 191 `+0D60` returns 1 on every path (FND-RNG-035).

## Interpretation

The five `DATA` numbers apply overlay 191's item tests to the target of the hit: 103 tries the
weapon test and then the armour test, 284 the weapon test alone, 275 and 283 the armour test alone,
and 235 queues `DATA` 311 for the target and runs the lowest-ranked item test. The flags byte gets
bit 0x02 when an item test reports that it acted; what the hit routine does with that bit was not
read.

## Alternatives

- Calling `+0CD0` the weapon test and `+0E3A` the armour test rests on the messages they show
  (FND-ITEM-009) alone.
- `+12B1` is read as adding a record to the cast queue from FND-RNG-037's account of its
  arguments; this call was not followed further.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179
642E0 66F95` and read `+26FB..+2756`; the case keys and targets are FND-RNG-034's 81 words at file
`0x66BA1` and 81 at `0x66C43`. Output is kept under `GAME_DIR/analysis/rng-callers-20261020/`.
Nothing was executed.
