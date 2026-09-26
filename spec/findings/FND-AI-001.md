---
id: FND-AI-001
title: None of the static paths followed from the first hostile, its data or the input code reaches a routine that picks an enemy's target, move or action
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

These paths were followed in the load image and in the overlay-mapped copy of `DSUN.EXE`:

| Path | Where it ends | Finding |
|---|---|---|
| The first hostile's `OJFF` resource, object 9,258 | No direct consumer in the executable | FND-ACTOR-009 |
| The `MONR` tag | Bytes in overlay 204 with no reference and no decoded instruction | FND-ACTOR-006 |
| The `ETAB` tag | No literal of the tag in the load image | FND-REGION-007 |
| The `RDFF` requests of the object slots | Indexed record requests that give no field a role | FND-ACTOR-003, FND-ACTOR-005 |
| The mouse coordinates and the key packets | Wrappers and a packet queue with no recovered reader, and a validation gate in overlay 195 | FND-INPUT-004, FND-INPUT-005, FND-COMBAT-012 to FND-COMBAT-017 |
| The random number generator | A shared routine and the panel's initialisation, with no rule-level caller | FND-RNG-001 to FND-RNG-008 |

None of them leads to a routine that takes a hostile's record and chooses a target, a move, an
action or the end of its turn.

## Interpretation

The routine that decides what a computer-controlled combatant does was not found by these paths.

## Alternatives

The search covers only the paths listed; overlay code reached through the stubs, state built at
run time and tags built from parts remain possible routes. The legacy record also counted the
`COMPUTER CONTROL` messages as unlinked data; they are pushed by overlay 190 (FND-AI-002), and the
flag they report is read in several places (FND-AI-003), none of which was followed to a decision.

## How to reproduce

Repeat the queries of each finding listed, in the overlay-mapped copy described in
`docs/GHIDRA.md`, "FBOV mapped image".
