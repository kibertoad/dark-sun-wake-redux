---
id: FND-EXE-519
title: Game allocator alternate cleanup merges metadata and falls into link removal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:20FA..1000:214F
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:20FA..1000:214F
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-518's alternate near helper 20FA first decrements current
DS word BX at word width, without checking zero or ownership. It
compares BX with shared first word DS:390C installed or DS:3880
on disc. Equality calls FND-EXE-514's near 214F. Inequality reads
SI from DS:BX+2, loads AX from DS:SI and tests AL bit
one. Set also calls 214F. That helper changes links/head and SI/DI,
locally preserves BX/AX and returns; 20FA continues afterward.

Clear bit one instead adds current word DS:BX into AX and stores
AX at DS:SI. It then reloads current word DS:BX into DI,
adds BX and stores SI at DS:DI+2, before setting BX to SI.
The reload occurs after the DS:SI store, so aliases can change it.
All arithmetic and offsets are word-width, with no carry or extent checks.

Every path then loads current word DS:BX into DI, adds BX,
loads AX from DS:DI and tests AL bit one. Set returns near
immediately without incoming cleanup. Clear adds AX into current word
DS:BX, forms SI as DI plus AX, stores BX at DS:SI+2,
then sets BX to DI. It falls directly into 2133, rather than
calling it or taking a separate return.

FND-EXE-514's 2133 loads DI from current DS:BX+6. Equality
with BX clears shared head DS:3910 installed or DS:3884 on
disc and returns near. Inequality stores DI into that shared head,
reloads SI from current DS:BX+4, stores SI at DS:DI+4
and DI at DS:SI+6, then returns near. On the fall-through path
this return uses 20FA's caller return address; there is no extra
stack cleanup or second return. The final selected BX is the adjacent
offset computed before falling through, not necessarily 20FA's incoming BX.

The body contains no interrupt, locally changes AX/BX/SI/DI and
does not locally change DS or SS. Stores precede later reads and
there is no local validation or rollback. Both editions share control
flow with their distinct shared offsets. FND-EXE-518's outer wrapper
restores its saved SI/DI and returns without testing AX; AX's last
writer differs between the early return and the metadata/link paths.

## Interpretation

This resolves the alternate immediate cleanup helper and an actual caller
of the shared-head insertion writer. Its arithmetic resembles adjacent-block
coalescing only under admitted block sizes, units, headers, links and disjoint
storage. Q-EXE-007 retains those producers, actual DS/SS, aliases and
lifetime, remaining setter callers and writers, 302B and earlier startup/
launch coverage. No complete cleanup or storage contract is claimed.

## Alternatives

Treating 2133 as a separate nested call ignores the direct fall-through
and its single return. Treating every read as a retained original value
ignores reloads after stores. Treating the final BX as the original block
ignores its path-dependent replacement. Treating unchecked metadata arithmetic
as admitted coalescing ignores sizes, units, bounds and aliases.

## How to reproduce

At revision e2cb3b0 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
72FA..734F in sixteen-bit mode. Follow decrement, first-word comparison,
low-byte tests, 214F's BX preservation, ordered metadata stores and later
reloads. Track BX through both replacements and the 2133 fall-through,
including both returns there, against FND-EXE-514. Keep stack-return
ownership and admitted storage separate. Licensed bytes stay outside Git;
no game process, DOSBox or emulated call runs.
