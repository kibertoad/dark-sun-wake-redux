---
id: FND-PARTY-118
title: Removing a placed entry through 31E0:05BB releases its object through 31E0:044C with no test that keeps the party objects 0 to 3 off the free list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:0612
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:09AF..31E0:09DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0ABC..31E0:0AD1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:05BB
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, direct_callers.py)
environment: null
---

## Observation

`31E0:05BB` takes a placed-entry index `di` and returns at once when it is negative or not below
the placed-object count `DS:264E`. Otherwise it takes the object `si` from the word at `+6` of
the entry's 8 bytes at the far pointer `DS:67B7` (`31E0:05BB..31E0:0612`). For an object whose
kind byte at `4F49:0C33 + 3 * si` is 2, it calls `25AF:071F` with the object when the object is
below 4, and continues the same way for every object (`31E0:09AF..31E0:09DC`). Both the kind 1 and
the kind 2 paths reach `31E0:0ABC`, which calls `31E0:044C` with the object and three of its own
arguments (`31E0:0ABC..31E0:0ACF`).

`31E0:044C` returns at once while `DS:265B` is 0. Otherwise, after work on the object's 0x25-byte
record at `DS:67BB + 0x25 * object` that depends on its arguments, it stores -1 at the record's
`+0x10`, calls `2D40:2410` and then `2D40:03C1` with the object, and clears the record
(`31E0:0549..31E0:056C`). It tests the object against no bound below 4. `direct_callers.py <dsun>
31E0:05BB` finds 29 far callers, in overlays 171, 173, 179, 187, 188, 191, 193, 197, 201, 204 and
206, and one near caller, `31E0:0DC3`.

## Interpretation

When any of those callers removes a placed entry that holds a party member, `2D40:03C1` puts the
party object back on the `DS:56ED` list (FND-PARTY-117), from which the next combatant the
`RDFF` loader places takes its object. So an `OBJEX` actor can occupy a party object if a placed
entry of a party member is removed while the game runs and an `RDFF` combatant is placed before
the party object is filled again.

## Alternatives

- Whether any of the 30 callers passes the placed index of a party member was not read; a caller
  that stops at the first four entries, or a removal that only runs at a region change followed by
  the party's own placement, would keep the party objects from an `RDFF` actor.
- What `25AF:071F` does for a party object, and what `2D40:2410` does, were not read.
- The list is popped last-in first-out, so the actor that takes the object is the first combatant
  placed after the release; which loads follow a removal was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun> 31E0:05BB..31E0:0AFC
31E0:044C..31E0:05BB`, and `direct_callers.py <dsun> 31E0:05BB`.
