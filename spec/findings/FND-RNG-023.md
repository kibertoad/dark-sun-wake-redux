---
id: FND-RNG-023
title: Seed clock sampling reads date before time without a coherence retry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:046E..1000:0494
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
tool: tools/research/exec-census/resident_listing.py; fingerprint-checked resident MZ relocation and bounded instruction reading
environment: null
---

## Observation

The clock wrapper makes exactly one direct call to the date-service wrapper,
then exactly one to the time-service wrapper, then calls the converter with
the two stored buffers. Its complete local control flow contains no loop,
second date read, comparison of successive samples, interrupt exclusion or
service-error branch. The only conditional branch after conversion selects
whether to copy the saved return pair to the optional destination.

Each service wrapper loads only AH with its selecting value, 2A for date or
2C for time, before INT 21. It does not initialize the other service registers.
After the interrupt it stores CX and then DX through the supplied near
pointer using DS, restores SI and BP, and returns. It neither stores AL nor
checks returned flags. The complete bodies have no local fallback, range
clamp or retry. These statements describe the game's request sites, not the
implementation or success conditions of the interrupt service.

The date destination is the four-byte local span at BP-4; time uses BP-8.
The wrapper derives both offsets from its SS frame, while the service stores
use DS. The spans are adjacent and disjoint if DS and SS address the same
storage. FND-RNG-014 observes that identity for one startup invocation.
Nothing in these local bodies sets DS equal to SS; identity on other callers
remains a separate admission requirement.

FND-RNG-015 and FND-RNG-016 establish the converter's direct byte usage.
Under the ordinary service interpretation, the time result's hundredths
byte is stored with DX but not directly consumed by the converter. A
weekday result in AL is not copied into its date buffer. This does not
prove those service outputs are independent of the retained values.

## Interpretation

Reproducing this local wrapper requires preserving two sequential samples,
date first. A host API that supplies one atomic date/time value introduces a
coherence guarantee absent from the game's local code. If the supplied date
belongs to the day before the supplied time, the converter receives that
mixed pair without a local repair. Whether the admitted runtime can actually
produce that pair remains an external-service question, not an observed
original defect.

As a synthetic conditional example, a date sample of April 11 followed by
a time sample of 00:00:01 after an April 12 rollover supplies April 11
00:00:01 to conversion. Holding the reviewed tables, offset and daylight
branch constant, that is 86400 seconds earlier than the coherent April 12
pair. This demonstrates the consequence of the sampled inputs only; no
midnight run or interrupt behavior is claimed.

This settles the local request order, register preparation, stored-result
widths and absence of local coherence/error handling within Q-RNG-004's
service contract. The actual service domain and cross-request behavior are
separated into Q-RNG-006. Environment/string admission remains Q-RNG-004;
storage lifetime and alias coverage remain Q-RNG-005.

## Alternatives

- An atomic timestamp or a retry-until-date-stable algorithm does not match
  this wrapper's complete local control flow.
- The absence of a local error check does not prove that all service outputs
  are admitted or valid. Interrupt implementations, hooks and register
  preservation outside these bodies have not been read here.
- A conditional mixed-date example is not evidence that a midnight mismatch
  occurred in the original or is reachable in the pinned runtime.
- No native observation, emulated service model, complete seed-source rule
  or parity promotion follows from this reading.

## How to reproduce

Use the project evidence Python with resident_listing.py on the licensed
installed DSUN.EXE. Read the two listed half-open ranges, whose calls and
frames are already identified by FND-RNG-015. Follow every local branch to
its far return. Check the AH-only request preparation, CX/DX store order,
near destinations, caller cleanup and date-before-time call order. Count
the service calls and check that no branch returns to either request.
Compare the converter's direct fields with FND-RNG-015 and FND-RNG-016.
Keep any original-derived listing only in GAME_DIR; no original is executed.
