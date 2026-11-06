---
id: FND-PARTY-267
title: The direct resource-reader tag census excludes 105 literal non-DATA requests and names the remaining variable-tag sites
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0438..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:1283..3D72:1297
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F640..0x0006F6F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072254..0x000722C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068DEC..0x00068E8D
tool: Ghidra 12.1.3 with scientific-method-engine 19.10.0 ReportFirstArgumentCallSummary.java, ReportConstantFirstArgumentCalls.java and ReportReferences.java; tools/research/exec-census/direct_callers.py, immediate_search.py, candidate_starts.py and resident_listing.py at 7f16c71b; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1
environment: null
---

## Observation

This replaces FND-PARTY-266, which mislabeled the variable-tag site at
file 0x68E5F as overlay 181 +060F. The authoritative FBOV map places it
in overlay 182 +060F (mapped 7365:060F); the file offset, candidate sets,
counts and other observations below are retained unchanged. The correction
uses ReportFbovOverlayMap.ps1 and the entry-based overlay-182 reading at
44dd935c. Overlay 181 instead starts at file 0x67EB0, and its reader
correctly rejects the supplied range. No new caller or confidence follows.

This answers the literal-tag part of Q-PARTY-124's alternate DATA-request
census. A request for DATA 1001 could expose another interpretation of the
class masks; a request with another type does not select that resource by
number alone. This is request-argument evidence, not proof of archive,
buffer, lifetime or I/O success.

The source is installed DSUN.EXE, 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Read-only Ghidra queries of the
reconciled mapped view classify the first pushed argument at direct
resource-reader calls. The four-byte type is first in these interfaces;
FND-ITEM-057 and FND-CONFIG-208 already read their forwarding contracts.

| Root | Ghidra direct sites | Literal type arguments | Variable arguments |
|---|---:|---:|---:|
| Whole-resource `38FF:04AB` | 110 | 98 | 12 |
| Pointer-returning `38FF:0438` | 10 | 9 | 1 |
| Lower reader `38FF:07B1` | 3 | 0 | 3 |

The whole-reader reference set agrees with all 110 raw direct-call sites,
not only their count. Exactly two literal arguments are DATA:
`2C5F:0037`, inside the generic getter, and overlay 185 `+0029`, inside
the class/experience accessor. The other 96 literals are different types.
All nine pointer-wrapper literals are also different types. Thus these
105 direct sites do not request DATA through their recorded literal-type
call setup; their resource numbers need not be traced to exclude DATA 1001
at those sites. This does not exclude their unrelated child effects.

The twelve variable whole-reader sites are finite:

| Site | Recorded argument form |
|---|---|
| `172C:0601` | local doubleword |
| `2D40:00A9` | caller type argument |
| `31E0:36DB` | local doubleword |
| `47E5:049E`, `47E5:0527`, `47E5:05B7` | caller type argument |
| overlay 182 `+060F`, file `0x68E5F` | local doubleword |
| overlay 184 `+1593`, file `0x6E623` | caller type argument |
| overlay 187 `+08CE`, file `0x706FE` | caller type argument |
| overlay 187 `+0B18`, file `0x70948` | local doubleword |
| overlay 187 `+20FD`, file `0x71F2D` | local doubleword |
| overlay 187 `+244D`, file `0x7227D` | local doubleword |

FND-EXE-639 already identifies the last site's local type as DATA and its
number as 1002, in the complete `+2424` body. That known binding is not a
newly unknown class-table request. The other eleven sites retain their
source-type classification obligation here; a variable push alone does
not mean the type is unconstrained.

The variable pointer-wrapper site is `3A8E:03B4`, which takes a type from
the record field at `+4`. FND-PARTY-343 and FND-COMBAT-089 already record
this loader. The lower reader has two wrapper calls and one generic
direct call at `47E5:04E6`. At the wrapper calls, the argument reporter
sees PUSH CS immediately before a near call. That is the far-return frame,
not the resource type. Reuse the read wrapper contracts instead of
classifying PUSH CS as a tag or a fourth independent DATA route.

### Candidate and coverage controls

The raw reporter finds eleven pointer-wrapper far-call candidates, while
Ghidra references list ten. Their sole difference is file `0x33BB2`,
`3D72:1292`, outside an inventory body. FND-PARTY-035 already describes
the surrounding pointer-image routine and its missing row. Context at
`3D72:1283..1297` pushes zero, the zero-extended image selector and literal
ICON before that call. It is not a newly demonstrated DATA request.
The routine's incoming entry/reachability is not established by this
candidate scan, and the missing ownership remains visible. Neither a
raw call pattern nor Ghidra's missing reference closes that coverage gap.

A separate low-word immediate/displacement search for `4144` finds three
actual complete DATA constants at instruction starts: the getter's push,
the accessor's push, and overlay 187's local assignment in `+2424`.
The other aligned hits are accesses with displacement `4144`; nested
word decodes inside the doubleword instructions are not additional sites.
This is an independent positive check for the known constants, not proof
that tags cannot be computed or copied elsewhere.

## Interpretation

The alternate-request census no longer needs to investigate resource
numbers at the 105 direct literal non-DATA sites. Its variable-type work
is the named eleven whole-reader sites, the record-driven pointer wrapper
and the generic lower-reader route, plus separately assembled or indirect
routes excluded by the direct census. The known DATA/1002 binding is reused.

The generic getter's number domains remain FND-PARTY-265's separate
obligation. Source/base/extent or byte mutations that could change the
documented table interpretation also remain Q-PARTY-124. No whole-entry
complete reading, native buffer outcome or parity result follows.

## Alternatives

- A non-DATA literal call can have unrelated effects through other children;
  only its own request's type is excluded. A computed type, indirect entry
  or transfer bypassing the inspected argument setup remains outside this
  result. The direct-site set is not a whole-program control-flow proof.
- The partial-reader count discrepancy and PUSH-CS classification limit
  are retained rather than normalized away. The 105-site result uses only
  positively classified literal setups, not an empty reference search.
- A title about payload representation does not dismiss unknown DATA
  accesses. Conversely, general success of unrelated ICON, graphics or
  archive operations is not added as a class-mask layout claim.

## How to reproduce

Reuse the fingerprinted source and the read-only INST-overlay-20261029g
project, program INST-MAPPED.EXE. Run ReportFirstArgumentCallSummary.java
for `38FF:04AB`, `38FF:0438` and `38FF:07B1`; run
ReportConstantFirstArgumentCalls.java for the first two roots and
`0x41544144`. Compare ReportReferences.java on the two wrapper roots with
`direct_callers.py <dsun> 38FF:04AB 38FF:0438 38FF:07B1`. For mapped
call `S:O`, shipped offset is `0x5200 + 16 * (S - 0x1000) + O`.
Require equal full-reader sets and retain the one partial-reader difference.

Run `immediate_search.py <dsun> 4144`, then
`candidate_starts.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <candidates>`.
Check full operand widths and preserve the distinction between an
instruction boundary and proven executable entry. Reuse FND-PARTY-035's
pointer-image observation, FND-EXE-639's DATA/1002 body and the complete
wrapper findings. No new original-game run or helper implementation is needed.

The retained reports under GAME_DIR/analysis/class-table-closure-20261106/
use data-request-routes, data-route-reference-controls, data-tag-word-candidates,
data-tag-candidate-ownership and partial-reader-omitted-anchored stems.
They were made before the separate template adoption and reused unchanged;
the scripts and package versions did not change during adoption.
For the descriptor correction, run `ReportFbovOverlayMap.ps1 -SourcePath
<dsun>` and compare descriptors 181 and 182, then
`overlay_listing.py <dsun> 182 68DEC 68E8D`. The mapped start 7365:0000
corresponds to shipped file 68850 hex and descriptor 182.
