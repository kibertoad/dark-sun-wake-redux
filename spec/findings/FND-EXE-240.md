---
id: FND-EXE-240
title: Shared gate helpers use wrapped size and stop linked selection at the first rejected candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0C22..4AE5:0C2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0CD5..4AE5:0D04
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D04..4AE5:0D11
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-239's three distinct callees decode from the shipped source without
calls or explicit interrupt instructions. Each returns near without extra
argument cleanup. Their bounded bodies are respectively five instructions
and twelve bytes, 22 instructions and 47 bytes, and six instructions and
thirteen bytes, with exclusive ends as given in Locations.

The helper at `4AE5:0C22` reads current ES-relative word `0x0008` into SI,
increments it at sixteen-bit width, clears its low bit and clears DI.
For an unsigned input w, SI becomes `(w + 1 modulo 65536) & 65534` and
DI becomes zero. Input 65535 therefore produces zero, not 65536. It has no
explicit memory stores or segment-register writes. The caller subtracts
this SI/DI pair from its low/high word difference and later uses it in
low/high accumulation; the helper does not supply an unbounded size.

The selector at `4AE5:0CD5` saves DS and clears ES to zero. It first reads
current DS-relative word `0x000E` into AX, without testing the initial
structure as a candidate. A zero link exits. A nonzero link is loaded into
DS. It subtracts incoming BX/CX from that candidate's low/high words at
`0x0014` and `0x0016`, using sixteen-bit SUB then SBB. An unsigned borrow
exits immediately. Otherwise it subtracts incoming SI/DI from the difference;
no borrow also exits immediately. Thus qualification requires an unsigned
32-bit difference at least zero and strictly below the SI/DI bound.

A qualifying candidate receives BP in DS-relative word `0x0018`. The helper
copies its DS segment into ES, reads its next word at `0x000E` and repeats.
It does not continue past a rejected candidate. At exit it copies ES to AX,
restores saved DS and returns. AX and ES explicitly report the last qualifying
segment encountered before a zero link or rejection, or zero if none qualified.
Qualification writes can occur for multiple links; their number and effective
destinations have no admitted bound here. Saved DS preservation is conditional
on those writes not aliasing its stack slot and on interrupt-time effects.

The helper at `4AE5:0D04` reads incoming DS into AX, loads ES from AX, reads
ES-relative word `0x000E` into AX and repeats the ES load while that link
is nonzero. Its explicit normal exit has AX zero and ES identifying the
last accessed segment. It neither saves nor explicitly changes DS, and has
no explicit memory stores. A zero incoming segment is not rejected before
the first read. Finite links and absence of cycles are not established.

## Interpretation

This supplies all three direct helpers' explicit arithmetic, selection and
link-following effects, narrowing FND-EXE-239's callee dependencies without
admitting their actual inputs. The selector's writes and returned segment
must be followed in caller execution order; an empty or rejected first link
can return zero. Bounded CFG completion is not traversal termination, an
output-count bound, stack preservation or a complete reading.

Q-EXE-001 and Q-EXE-010 retain native input/state/segment admission, all link
and field writers, effective aliases, chain and output bounds, stack-slot
integrity, interrupt-enabled changes and caller use of each returned pair
and segment. No complete_reading or replacement inventory is established.

## Alternatives

An unbounded even size ignores the helper's sixteen-bit increment wrap.
Searching every link for any qualifying candidate ignores the selector's
immediate exits on rejection. Returning the first qualifying segment ignores
the subsequent qualifying links' replacements of ES. A finite candidate
body cannot prove that a linked traversal terminates or bounds its writes.

## How to reproduce

At revision `a04b3ae`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings. Separately set entry and sole entries value
to `0x00040C72`, `0x00040D25` and `0x00040D54`. Supply no seeds or summaries.
Check the body counts above and absence of calls in each candidate.

Independently hash-check the source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Decode only file intervals
`0x00040C72..0x00040C7E`, `0x00040D25..0x00040D54` and
`0x00040D54..0x00040D61` with Capstone 5.0.7 in x86 sixteen-bit mode,
initial IPs `0x0C22`, `0x0CD5` and `0x0D04`. Inspect the arithmetic widths,
segment-MOV directions, rejection branches, state stores and returns above.
Read the callers in FND-EXE-239 separately. Sources, configurations and
listings remain in GAME_DIR.
