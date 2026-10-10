---
id: RULE-PARTY-006
title: START GAME supplies characters 40 to 43 as the party
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-013, FND-PARTY-021, FND-PARTY-020, FND-PARTY-023, FND-PARTY-034, FND-PARTY-038, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [FMT-PARTY-001, SCR-UI-001]
---

## Summary

START GAME begins with a party the game supplies: the four characters stored in `CHARSAVE.GFF`
under the numbers 40, 41, 42 and 43, in that order.

## When it runs

When a game begins from START GAME on the start window (SCR-UI-001) [SRC-MANUAL-1994,
FND-PARTY-020]. The Start Game branch calls the overlay 182 routine that runs the loader only
while the placed-object count (`DS:264E`) is 0 [FND-PARTY-021].

## Parameters

`slot`, a party slot, 0 to 3.

## Inputs

None beyond the parameters.

## Procedure

```text
define supplied_character(slot):
    return 40 + slot
```

For each slot from 0 to 3, the game loads the FMT-PARTY-001 character record numbered
`supplied_character(slot)` into that slot, with the character's `PSIN`, `PSST` and `SPST`
records of the same number [FND-PARTY-013].

Before each slot's load, the game opens `CHARSAVE.GFF` in the directory the program was started
from, unless it is already open. When that open fails, the game prints that the file was not
found, with that directory, and ends with status 1, so no slot is loaded [FND-PARTY-034,
FND-PARTY-038]. When a
character record is missing, the slot's load fails and the slot is placed without its `PSIN`,
`PSST` and `SPST` records [FND-PARTY-013, FND-PARTY-023, FND-PARTY-034].

## Outputs

The number of the character record for the slot. No state changes.

## Edge cases

The numbers are computed from 40 and the slot, so the executable holds no list of them
(FND-PARTY-007). The archive holds a second set, 50 to 53, which this path does not use
(FND-PARTY-005). No other installed archive has `CHAR` records, so the lookup cannot find the
four elsewhere [FND-PARTY-034].

## What the sources say

SRC-MANUAL-1994, page 2, says START GAME begins play with a party that has already been made,
and does not name its members.

## Differences between builds

None known.

## Open questions

- Whether the gate routine reaches its gate with the placed-object count at 0, so that the
  loader runs. FND-PARTY-021 shows START GAME reaching the loader only through that gate, after
  overlay 187 entries and three early returns. For the reading that it does: the count and the
  word at `DS:0DAB` start at 0 (FND-PARTY-022, FND-PARTY-029), and no direct call made between
  program start and the gate reaches a routine that changes the count or makes that word nonzero
  (FND-PARTY-031). The pointers most indirect calls on those paths read hold routines from which
  no route reaches one (FND-PARTY-039, FND-PARTY-040). The run of `MAS` 99 on those paths executes
  only opcodes whose handlers reach none (FND-PARTY-040, FND-PARTY-041). `GPLDATA.GFF` is open
  when `MAS` 99 loads and nothing before closes it, so the load, whose error routine leads to
  such routines (FND-PARTY-040), fails only on a file call's result (FND-PARTY-042). Still open:
  9 indirect calls whose targets are unread (FND-PARTY-039, FND-PARTY-040, Q-PARTY-011);
  and whether either of the gate routine's two
  video-memory reservations fails (FND-PARTY-030, FND-PARTY-032, FND-PARTY-033). While the
  pointer is an `ICON` neither can fail on space, since the caret and pointer saves then hold at
  most 1,006 paragraphs, the room left, and no direct route before the gate makes it another
  image (FND-PARTY-035, FND-PARTY-036, FND-PARTY-037); routes through the unresolved
  indirect calls stay open (Q-PARTY-011). The owner's captures of a game started with START GAME show the four characters
  in this order, but they do not tell 41 from 53 or 43 from 33 (FND-PARTY-020); the
  shipped-party live session would confirm it (Q-PARTY-001).
- Where `CHARSAVE.GFF` is opened from rests on DOS reporting version 3 or later and placing the
  program's full path after the environment, which the C run time copies as `argv[0]`; with an
  earlier version `argv[0]` is empty and the name is opened in the current directory
  (FND-PARTY-034, FND-PARTY-038). (No item: what the DOS of GOG's DOSBox reports and writes
  there shows only in a run, and no run is possible.)
- Whether the load of `MAS` 99 before the gate succeeds rests on the file positioning, read and
  record-write calls on `GPLDATA.GFF` returning what the installed file gives (FND-PARTY-042). A
  run reaching START GAME's party on an intact installation would show it (No item: the outcome
  rests on the operating system, and no run is possible).
- Whether the program ends after a failed `CHARSAVE.GFF` open depends on the operating system
  carrying out the run time's terminate request (FND-CONFIG-062, FND-PARTY-034); a run without
  the file would show it (No item: agents cannot run the game, and no owner session asks for a
  broken installation).
