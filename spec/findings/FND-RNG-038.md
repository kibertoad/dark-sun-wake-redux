---
id: FND-RNG-038
title: Overlay 195 +0000 returns the byte at 50C2:0000 and makes no draw
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081770..0x0008177E
tool: tools/research/exec-census/overlay_listing.py, direct_callers.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 195's code starts at file `0x81770`.
Trampoline `574E:0020` enters overlay 195 `+0000`. Decoded from its entry, the routine sets up a
frame, loads ES with the segment word `0x03C0` (descriptor 120, segment `50C2`), loads AL from the
byte at `ES:0000`, restores BP and returns far (`+0000..+000E`). It takes no argument, calls
nothing and changes no memory. The direct-call query finds four far callers: overlay 173 `+0F35`,
overlay 197 `+1E23` and overlay 198 `+04CD` and `+04DA`. The instruction after each call tests AL:
overlay 173 compares it with 9, overlay 197 tests its bit 0x01, and overlay 198 compares it with
4 at `+04D2` and with 0 at `+04DF`.

## Interpretation

Overlay 195 `+0000` reads the byte at `50C2:0000`; the two calls in overlay 198 `+041D`
(FND-RNG-035) test that byte, for 4 and for 0. AH keeps 3 from the segment load, so a caller that tested AX
would read 0x03 in its high byte; none of the four does.

## Alternatives

- What the byte at `50C2:0000` holds and which code writes it were not read. FND-RNG-036 has
  overlay 195 `+00D1` test it for 0.
- What overlay 173 and overlay 197 do after their tests was not read.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `trampoline_target.py <dsun>
574E:0020`; `overlay_listing.py <dsun> 195 81770 8177E`; `direct_callers.py <dsun> 195+0000`.
Nothing was executed.
