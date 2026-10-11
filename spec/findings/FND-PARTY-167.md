---
id: FND-PARTY-167
title: The byte at +0x255 of an active effect counts down an effect given a negative duration, and the spells LEVEL DRAIN and ENERGY DRAIN give their effect 59 a count of 0, or 15 while DS:6174 is set
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FAB0..0x0007FB25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FD94..0x0007FE08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081C9E..0x00081CCF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080171..0x00080278
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D37D..0x0005D3BA
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_stores.py, direct_callers.py, overlay_listing.py, immediate_search.py; a scratch script that prints DATA resources read through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Active effects are 10-byte records at `4E71:024D + 10 * e`; overlay 197 `+026F..+0286`
subtracts 2 from the save modifier for each count up to the byte at `+0x255` of the target's
effect of type 0x3B (FND-PARTY-166).

**The stores.** `field_stores.py <dsun> <inventory> --es 255` lists five writes of `+0x255`:

1. Overlay 193 `+17D1`, in `+166F`, the routine that adds an effect: 0 when the duration dword at
   `bp+0xE` is not negative (`+17C0..+17D1`).
2. Overlay 193 `+1830`, in `+166F`: the low byte of the duration, negated, when the duration is
   negative (`+181A..+1830`).
3. Overlay 193 `+1B03`, in `+1AA4`: `+1AA4` finds the slot's effect of a type, passes over it when
   the byte is 0, subtracts the result of overlay 195 `+147C` from the byte and, when the byte is
   then 0, goes on to remove the effect (`+1AA4..+1B18`).
4. Overlay 195 `+0558`: adds 1 when the byte is below 4 (`+0546..+0558`).
5. One write at overlay 195 `+0BA9` that decodes only from a start inside another instruction.

**The duration.** The hit routine passes `193+166F` the duration in `bp-0xC`: -9999 (`0xD8F1`)
while the byte at `DS:6174` is not 0, and otherwise the result of overlay 193 `+1E81` for the
item and a level (FND-PARTY-166). `+1E81` returns -9999 when the word at `+7` of the `DATA` record
is `0xD8F1`. Otherwise it returns, capped at 0x7FFF, the product of that word and the sum of the
word at `+5` times the level plus the signed high nibble of `+0x1E`, divided by `+0x1D & 7` or 1,
with the result of overlay 173 `+2A5D` for the low and high nibbles of `+4` and 0
(`+1E81..+1F87`). `+2A5D` adds, for each count up to its first argument, 1 plus a draw scaled to
its second argument, and returns the sum plus its third (`+2A5D..+2A99`).

**The records.** `DATA` 104, `LEVEL DRAIN`, and 225, `ENERGY DRAIN`, the two whose byte at
`+0x19` is 59 (FND-PARTY-108), both hold 0x10 at `+4`, 0 in the word at `+5` and -1 in the word at
`+7`.

## Interpretation

The byte counts the uses left of an effect given a negative duration, which `+1AA4` spends.
For `LEVEL DRAIN` and `ENERGY DRAIN`, `+2A5D` rolls no dice, so `+1E81` returns 0 and the drain
effect's count is 0; while `DS:6174` is set the duration is -9999, whose low byte 0xF1 negated
gives 15. Overlay 195 `+0558` does not raise a count of 4 or more. So the spells leave the count
at 15 or less, and the save modifier's term at overlay 197 `+026F` takes off at most 30. With the
other terms that can apply against `CHAOS` (FND-PARTY-166), the sum then stays at -145 or more for a
roll of 2, short of the -256 plus the save it needs to wrap back below the save.

## Alternatives

- The effect 59 that the item of `DATA` 313 and a script's function 23 add (FND-PARTY-108) gets
  a duration not read here; a negative one below -60 would give a larger count.
- What `DS:6174` selects, and whether `+1AA4`'s subtraction can pass below 0, were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_stores.py <dsun> <inventory> --es 255`,
`direct_callers.py <dsun> 193+1E81 173+2A5D` and `overlay_listing.py <dsun>` with 193 `7FAA0
7FB30`, 193 `7FD94 7FE40`, 195 `81C70 81CD8`, 193 `80171 802A0` and 173 `5D37D 5D3BA`. Print the
first 0x20 bytes and the name at `+0x20` of `DATA` 104 and 225 in `RESOURCE.GFF`.
