---
id: FND-EXE-242
title: Segment-to-header candidate checks state bounds marker and published segment before far return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07C5..4AE5:07FB
tool: executable-reader 2.5.0, Capstone 5.0.7 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The source candidate at `4AE5:07C5` contains 22 instructions and 54 bytes,
ending exclusively at `4AE5:07FB`. It has no calls or explicit interrupt
instructions and returns far at `4AE5:07FA` without extra argument cleanup.
The saved resident listing reports the start as undisassembled; that missing
window does not establish absent source bytes.

It saves DS and ES and loads DS from CS-relative word five, the candidate
state-segment source read in FND-EXE-176. It compares incoming AX against
DS-relative words `0x0124` and `0x0126`, at unsigned sixteen-bit width.
AX below the first or at least the second takes failure. Otherwise it
decrements AX at sixteen-bit width, loads ES from AX and loads ES from that
segment's word `0x000E`. It then increments AX back to the original value.
The preceding-segment calculation can wrap for input zero; there is no
separate zero guard before dereferencing the word. Whether actual state
bounds exclude that case remains unread.

It loads BX from DS-relative word `0x0110` and compares it against current
ES-relative word zero. A mismatch takes failure. Otherwise it compares
original AX against current ES-relative word `0x0010`, copies the current
ES segment into AX, and takes the successful restore path only on equality.
That equality comparison leaves carry clear; the segment read does not
change flags. Failure instead explicitly clears AX and sets carry. Both
paths restore saved ES and DS and return far.

Thus its explicit successful result is AX holding the located header segment
and carry clear, while failure has AX zero and carry set. ES is restored,
rather than left pointing at the returned header. BX contains the marker
when the marker test is reached. The candidate does not explicitly write
state/header fields; its stack saves and any aliases or interrupt-time changes
remain separate storage obligations. No null-header check precedes the marker
and published-segment reads.

## Interpretation

This gives a concrete source-derived segment-to-header admission procedure:
unsigned bounds, preceding-segment link, marker and published-segment equality.
It advances the native-header dependency in Q-EXE-010 without establishing
the candidate's native callers, actual state bounds or linked header contents.
The different return register and restored ES must be followed in callers.

Q-EXE-001 and Q-EXE-010 retain incoming CS/AX and state-segment admission,
every bound/link/marker/segment writer, effective aliases, saved-stack
integrity, interrupt-enabled changes and all callers' use of AX/carry.
No complete_reading or replacement inventory is established.

## Alternatives

An inclusive upper bound is contradicted by the unsigned at-least rejection.
Returning the header only through ES ignores the restoration and AX copy.
Treating AX alone as the success indicator ignores the separate carry result.
Assuming a null header is rejected before access adds a guard not present.

## How to reproduce

At revision `4f2c6f6`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, with entry and sole entries value
`0x00040815` (`4AE5:07C5`), no seeds or summaries. Check the 54 covered bytes,
22 instructions, sole far return and absence of calls.

Independently hash-check the source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Decode only file interval
`0x00040815..0x0004084B`, initial IP `0x07C5`, using Capstone 5.0.7 in
x86 sixteen-bit mode. Inspect segment-load directions and all comparisons
and restore paths. The resident Ghidra snapshot, read-only with analysis
disabled, reports an undisassembled start for ReportInstructionWindow at
`4AE5:07C5`, count eighteen. Preserve that distinction from source decoding.
Sources, configurations and listings remain in GAME_DIR.
