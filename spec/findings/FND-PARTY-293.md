---
id: FND-PARTY-293
title: Known setter-call branches request vectors other than 21 and expose an unowned interrupt-23 body
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0361..1000:0387
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:04A3..1000:04B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1DBA..1000:1E12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2BDB..1000:2C56
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2C77..1000:2DA2
tool: tools/research/exec-census/direct_callers.py, resident_listing.py and far_pointer_words.py at 5ede4813; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-EXE-287/288/574 and FND-PARTY-292
environment: null
---

## Observation

Q-PARTY-134 retains the actual vector route needed to apply
SRC-RUSTDOS-181's built-in service contract. Installed DSUN.EXE is
634416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.
Offsets and vector numbers below are hexadecimal.

### Nine encoded calls to two known interfaces

The direct-call reporter finds two relocated far candidates for
44DE:046D and seven same-segment push-CS/near-call candidates for
1000:04A3. Existing FND-EXE-287/288 independently bind the first
interface's setup and cleanup calls, both with vector nine. These
are positive controls for relocated-call decoding and stack arguments.

1000:04A3 sets AH=25, loads AL from the low byte of its first
word argument and takes the handler pointer from the following far
argument. Its seven encoded call sites have these incoming values:

| Call | Vector argument on the verified own branches |
|---|---|
| 1000:037F | 23 |
| 1000:1DDE and 1E0A | 24 at each site |
| 1000:2C3C | 23, restoring the saved pointer from DS:A4DA/A4DC |
| 1000:2D31 | 00 |
| 1000:2D70 | 05 |
| 1000:2D90 | 06 on fallthrough, 04 from the jump at 2D42, or 23 through the join at 2D10/2D13 |

At the shared 2D90 call, one literal load immediately preceding the
common push does not describe every incoming branch. The vector-23
route can pass either the saved pointer or literal 1000:2BDB,
depending on its input-pointer test. The vector-04 route passes
1000:2B6A. FND-EXE-574's encoded call-site count remains seven;
its listed roots do not constitute an exhaustive branch-specific
handler set for this shared site.

None of these verified ordinary call arguments is 21. This excludes
these particular instruction paths as requests to replace vector 21;
it does not prove the active vector or absence of another mechanism.
The reporter omits indirect calls, jumps and bypass entries. Direct
vector-table writes, computed interrupt instructions, other AH=25
sites or the dynamic-AH wrapper retained by FND-EXE-574, loaded
drivers and outside services are not covered by this two-interface
caller result. Existing raw/decoded controls and exclusions survive.

### A source-verified body outside the current inventory

The vector-23 literal binding at 2D15..2D1D identifies entry
1000:2BDB. The source decodes continuously through IRET at 2C55,
giving a half-open body 2BDB..2C56 of 123 bytes. It saves the
general/segment registers, loads DS=57E0 and uses a four-byte local
copy of a callback pointer. One path requests the exit wrapper,
another restores the saved vector-23 pointer and calls through that
local pointer. Its common ending restores the saved registers and
uses IRET. Callback targets, continuation and frame admission remain
unresolved; the body is not an ordinary far-return helper.

The current DSUN.EXE inventory at 5ede4813 has a preceding body
ending at 2BDB and the next start at 2C56, but no owner for this
span. The 123 bytes therefore require an inventory/export repair.
They are not silently counted as an existing inventoried function.
The current 3230-row denominator remains the old inventory's count,
not a completeness claim for the executable. The literal binding and
IRET boundary supply concrete evidence for the repair; analyzer byte
classification and updated provenance still need reconciliation.

## Interpretation

The known setter-interface call branches' vector arguments are answered,
including two alternatives at a shared tail that a nearest-load reading
would miss. Q-PARTY-134 retains actual vector admission and the
mechanisms explicitly outside this census; no global no-hook claim follows.

The new handler is a mapping gap with a bounded source location, not
an accepted complete reading. Repairing its inventory metadata is the
next tooling task. Q-PARTY-124's eighty other direct sites and the
three Q-PARTY-134 clients remain 83 total, with the existing lifetime,
guard, file-handle and source/release conditions. FMT-PARTY-007 stays
supported; no queue item closes or complete reading is declared.

## Alternatives

- One encoded call site can accept several vector/handler pairs.
  A nearest literal is not an exhaustive incoming argument domain.
- No vector-21 request in these two caller sets is not absence of
  vector-21 replacement in every instruction stream or driver.
- A missing analyzer function does not make a referenced IRET body
  data, nor does source decoding alone reconcile its exporter sidecars.

## How to reproduce

Run direct_callers.py for 44DE:046D and 1000:04A3. Reuse
FND-EXE-287/288 for the two vector-nine controls. Read the resident
ranges above from their stated entries, following all branches into
2D90. Run far_pointer_words.py for 1000:2BDB and check its
positive literal at file 7F1A, not the unrelated raw word at 585C.
Compare the inventory's neighboring ranges before adding an owner.
Reports vector-setter-direct-callers, vector-setter-runtime-callers
and vector-setter-unowned-context remain under
GAME_DIR/analysis/class-table-closure-20261106/.
No original game ran.
