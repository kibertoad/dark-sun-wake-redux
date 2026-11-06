---
id: FND-SOUND-076
title: Music health input is recomputed with wrapping unsigned division before selection and script request 52 can overwrite its byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:2369
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:389A..28C9:391D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C19A..0x0008C1A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C481..0x0008C4F1
tool: resident_listing.py, overlay_listing.py and bounded table reporter at 33da9807; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

This connected producer/consumer reading uses the installed DSUN.EXE,
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. It refines
FND-SOUND-071's health arithmetic and alternate direct writer, without
claiming that those literal sites exclude computed or aliased writes.

28C9:389A initializes two SS-relative 32-bit sums and examines exactly
four slots. It loads near bases from DS:19C9 and DS:19C5, advancing them
by 49 and 66 respectively at word width. Each slot tests the word at
combatant offset six through ES loaded from DS:19CB. A zero word skips
both additions. Otherwise it adds the unsigned details word at offset
eight to the maximum sum, through ES loaded from DS:19C7, and the signed
combatant word at offset zero to the current sum. This body has no child
calls and does not change DS or SS; actual pointer, frame, segment and
storage correspondence are still preconditions.

With maximum sum zero it sets DX zero. Otherwise it multiplies the
current sum by nine at 32-bit width, clears EDX and performs unsigned
32-bit division by the maximum sum. Thus a negative current sum is
represented by the wrapped product's unsigned bits at division; it is not
a signed quotient or a clamped zero-health case. It copies quotient AX
to DX and publishes byte low8(10 - DL) at DS:14E7. AX on return is the
low quotient word (zero on the zero-maximum path), not the published byte.
There is no local clamp to 1..10 or test that current health is at most
maximum health. For four properly bound word inputs the maximum sum is
at most 262140, but actual pointer aliases or changed backing are not
excluded by that arithmetic bound.

These synthetic one-active-slot arithmetic consequences are not native
observations:

| Current / maximum word values | Published byte |
|---|---|
| 1 / 1 | 1 |
| 0 / 1 | 10 |
| 0 / 0 | 10 |
| 2 / 1 | 248 |
| -1 / 1 | 19 |

The tick at 28C9:2322 calls 2D40:3DBD, then this producer at 2333.
It tests DS:13F7; nonzero loads the newly stored byte at 233D,
sign-extends it into AX and passes that word plus DS:140C to selector
2834:000C at 2346. There is no intervening child call between producer
return and selector entry. The producer's own returned AX is ignored.
A zero sound flag skips selection but does not skip recomputation.
This prefix precedes the DS:0D9C main-view gate at 2361, so that later
gate does not suppress this health update. Actual segment retention,
interrupt effects and other entries remain separate questions.

The alternate direct byte store belongs to overlay 204's established
script-function dispatcher +0169, not the preceding +0000 function.
The dispatcher decrements its first word argument, accepts unsigned
indices 0..52 and jumps through 53 word targets at local +06C7.
The bounded table identifies request 52 uniquely with +03DA.
That case copies the low byte of the third word argument (BP+0A) to
DS:14E7 and joins +06C1. It has no clamp or child call on this case.
DI was initialized zero at entry and is copied to AX at the common
return, so this case returns word zero on the unchanged frame path.
FND-COMBAT-050 and FND-VIDEO-010 bind the dispatcher and script route.
No claim about which shipped scripts admit request 52 is made here.

## Interpretation

The known local health producers and the ordinary tick handoff are
resolved: ordinary bounded health can yield the familiar 1..10 measure,
but the field itself has byte-wide arithmetic and a direct script option.
A script override is overwritten by the next reached tick recomputation
under stable ordinary storage; its existence alone does not establish
that it supplies a value to a selector call. Actual health-record histories,
script ordering and excluded writes remain Q-SOUND-004 obligations.

## Alternatives

Treating every stored value as a guaranteed 1..10 domain discards the
unclamped arithmetic and override. Treating the aggregation's AX as the
selector input discards the fresh byte load. Treating the later view gate
as a guard on this prefix contradicts the instruction order. No native
out-of-domain health value or exhaustive writer exclusion is inferred.

## How to reproduce

Use tools/research/exec-census/ at 33da9807 with installed DSUN.EXE.
Retain reports under GAME_DIR/analysis/handler-census-20261106/:
resident_listing.py ranges 28C9:389A..28C9:391D and
28C9:2322..28C9:2369 give music-health-producer.txt and
music-health-tick-consumer.txt. overlay_listing.py descriptor 204 ranges
8BF29..8BF4E, 8C19A..8C1A3 and 8C481..8C487 give
music-health-script-dispatch.txt, music-health-script-case.txt and
music-health-script-return.txt. The earlier broad sequential listing
music-health-alternate-producer.txt includes table bytes and is diagnostic;
use the separate established-root and table queries for this reading.

Run tools/evidence/report.mjs table with music-health-dispatch-config.json:
source fingerprint above, start 0x8C487, count 53, stride two, unsigned
width-two target at offset zero. The decrement/unsigned-52 guard supplies
count evidence. music-health-dispatch-report.json identifies request 52
as +03DA; request one -> +02B1 is FND-COMBAT-050's independent control.
No original-game run or emulated call supplied the evidence.
