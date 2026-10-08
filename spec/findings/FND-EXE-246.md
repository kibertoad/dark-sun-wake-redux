---
id: FND-EXE-246
title: Post-bound helper publishes linked-header segments before downstream content and callback work
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:031B..4AE5:037A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:037B..4AE5:03DB
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-175's post-bound callee at `4AE5:031B` covers 191 instruction
bytes in the two intervals above, with 68 instructions and one near return
at `4AE5:03DA` without extra argument cleanup. Its one-byte gap at
`4AE5:037A` is not reached by the declared traversal.

The helper loads CX from current DS-relative word `0x0122`, writes CX to
word `0x012C`, retains CX in BX, loads SI from word `0x0124` and DI from
word `0x0126`, and saves DS. It loads DS from CX and reads that segment's
word `0x0012` into CX. A zero word ends the first traversal. Otherwise it
loads ES from CX and subtracts current DS-relative words four/six from
ES-relative words four/six using sixteen-bit SUB followed by SBB. It divides
the resulting unsigned DX:AX pair by sixteen, retaining the quotient in AX,
then adds SI to AX at sixteen-bit width.

It rejects a result strictly above DI using an unsigned comparison. Equality
is accepted. There is no explicit subtraction-borrow or quotient-fit guard
before division, and no addition-overflow guard before the upper comparison.
An unsigned sixteen-bit division faults when the quotient cannot fit AX;
admitted inputs must exclude that case before this can be treated as an
ordinary returning path. The source CFG does not establish exception behavior.

On an accepted result it stores SI in current DS-relative word `0x0010`,
sets SI to the new AX, copies ES into CX, stores CX in current DS-relative
word `0x001C`, copies the current DS segment into BX and repeats from the
DS load. At a zero link or rejected result it loads DS from retained BX,
clears that segment's word `0x001C`, restores saved DS and stores SI into
DS-relative word `0x0120`. BX identifies the retained preceding segment
from the last accepted step, or the initial segment when none was accepted;
it is not simply the current lookahead segment at every stop.

It reloads DS-relative word `0x0124`, subtracts it from SI at sixteen-bit
width and branches to its carry-clear return when the difference is zero.
Otherwise it proceeds to downstream work. The bounded CFG lists:

| Site | Target |
| --- | --- |
| `4AE5:0395` | `4AE5:03E8` |
| `4AE5:03A7` | `4AE5:0421` |
| `4AE5:03B2` | `4AE5:0693` |
| `4AE5:03CB` | far pointer at current DS-relative `0x0086`, unresolved |

The call to `4AE5:0693` is an interior entry of FND-EXE-227's bounded
writer, after its initial guards and optional call. Reading only the outer
entry cannot supply this call's admission contract. The downstream sequence
also writes a header segment to a word in the preceding segment and calls
the far pointer after loading DS from CS-relative word five. Its callee,
input, saved-stack and callback effects remain unread here. All calls in
the CFG are assumed to return; this does not make the complete helper read.

## Interpretation

This identifies further producers of the live header segment, state word
`0x012C` and current-segment word `0x0120`, and the separate initial source
word `0x0122`. It narrows segment-provenance dependencies in Q-EXE-010 while
retaining link admission, arithmetic guards and downstream effects. Publication
precedes those downstream calls; successful execution is not proved by the
ordered stores or the source traversal.

Q-EXE-001 and Q-EXE-010 retain native state/header admission, every link and
field writer, incoming bounds, division and output bounds, effective aliases,
saved-stack integrity, downstream callee/interior-entry contracts, far-pointer
writers and interrupt-enabled changes. No complete_reading or replacement
inventory is established.

## Alternatives

An exclusive upper rejection is contradicted by the strictly-above branch.
Unbounded addition or automatically safe division ignores their instruction
widths and absent guards. Clearing the current lookahead header at every stop
ignores the retained BX reload. Applying all outer-writer guards to its
interior-entry caller skips the caller's actual target.

## How to reproduce

At revision `18f6f18`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, with entry and sole entries value
`0x0004036B`, no seeds or summaries. Check both intervals, the one-byte hole,
191 covered bytes, 68 instructions and all returning-call assumptions.

Independently hash-check the source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Decode file intervals
`0x0004036B..0x000403CA` and `0x000403CB..0x0004042B`, initial IPs
`0x031B` and `0x037B`, using Capstone 5.0.7 in x86 sixteen-bit mode.
Inspect the first traversal's register versions, widths, stores and stop
paths, then the listed downstream calls. Read FND-EXE-227 separately for
the interior-entry distinction. Keep division exceptions, actual storage
and call effects unresolved. Sources and listings remain in GAME_DIR.
