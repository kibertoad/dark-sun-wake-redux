---
id: FND-PARTY-265
title: The equipment hook's guarded DATA request cannot pass positive 1001, while the generic getter has 87 nonliteral direct-call arguments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077C53..0x00077C99
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
tool: Composition of FND-PARTY-254 and FND-ITEM-056/057/062; tools/research/exec-census/direct_callers.py at 3a6640a3; Ghidra 12.1.3 ReportFirstArgumentCallSummary.java from scientific-method-engine 19.10.0; Python 3.12.10 and xxhash 4.0.1
environment: null
---

## Observation

Q-PARTY-124's computed-number question includes the shared DATA getter
`2C5F:0003`, whose selector is not restricted to item numbers by its own
body. FND-ITEM-057 already reads that body: except for exact FFFF, a cache
miss requests DATA with the sign-extended selector word. Equality with the
cached key returns a buffer without requesting a resource. The getter has
no positive-319 upper-bound test of its own.

A read-only first-argument summary in the reconciled Ghidra overlay view
finds 87 direct getter calls. All 87 push a register or memory value, not
an immediate selector. The independent raw direct-call reporter finds the
same 87 sites. This classifies the argument form, not each source domain.
It excludes neither computed 1001 nor indirect getter calls.

As a control, the same summary on `56E6:0020` finds twelve direct accessor
calls, ten with immediate 1000 and two with immediate 1001, agreeing with
FND-PARTY-058/068. The known two hook calls appear as nonliteral SI pushes
at overlay 189 `+32BC` and `+32E4`.

FND-PARTY-254's complete hook reading and its retained entry-based report
already settle the first site's value gate. After the status call returns
zero, `+32A3` compares SI signed with 319. Values at least 319 take the
rebuild/reset path and return. The lower branch reaches `+32BB`'s push of
that same SI and `+32BC`'s getter call with no intervening child or selector
write. Thus the actual word passed at this first site is in signed
`-32768..318`. Positive 1001 cannot reach this call through that gate.
This remains true if an earlier status child changed SI: the comparison
tests the current SI after the child. It assumes ordinary entry/return and
valid instruction/stack state, not independent entry after the comparison.

The repeated getter site uses SI after the first call and a negative-bonus
test. If the returning call/frame retains SI, its selector is the same and
also excludes positive 1001. That preservation condition is not discharged
by the earlier comparison alone. The first getter's saved-SI frame and
resource writes must not be treated as admitted nonaliasing native storage
merely from the local return.

FND-ITEM-056's separate canonical-source census bounds all 1,931 complete
item payloads' source word to 0..327. With that source and deciding values
retained, the hook's actual DATA branch has selectors 0..318; the 320..327
source-word cases use rebuild instead. FND-ITEM-057/062 already compose
the ordinary source DATA and the equipment cast's forwarded selector.
This source-preserving result is reusable, not a proof that all other
getter clients or runtime item writes have the same domain.

## Interpretation

The first equipment-hook request's positive-1001 possibility is removed
from Q-PARTY-124. The repeated site's preservation and the other 85 direct
getter sites still require their actual domains. All 87 sites are known
nonliteral forms; another search for literal 1001 cannot settle them.
This does not establish a complete access census or FMT-PARTY-007.

## Alternatives

- A nonliteral push is not evidence of an unbounded value; the first hook
  site demonstrates a deciding local bound. Conversely, a canonical source
  bound is not a universal runtime bound for other callers or mutations.
- A request-number exclusion does not prove the getter returns correctly
  keyed bytes. Cache hits, failed transfers, aliases or separately written
  shared buffers remain FND-ITEM-057's qualified content obligations.
- The direct-call count excludes computed targets, near/bypass entries and
  separately assembled getter pointers. No negative Ghidra reference result
  is used; the raw count and literal controls are positive checks.

## How to reproduce

Use DSUN.EXE length 634416, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. At 3a6640a3 run the existing
`tools/research/exec-census/direct_callers.py <dsun> 2C5F:0003` using the
project-local evidence Python. In the read-only INST-overlay-20261029g
project, program INST-MAPPED.EXE, run
`ReportFirstArgumentCallSummary.java 2C5F:0003` and separately
`ReportFirstArgumentCallSummary.java 56E6:0020`, with no analysis or writes.
Compare all 87 sites and both literal-control counts; retain unresolved
argument forms rather than inferring their values. Use the mapping reporter
for overlay addresses; this finding cites only shipped-file locations.
The compared site sets agree exactly. For each mapped call `S:O`, the
shipped offset is `0x5200 + 16 * (S - 0x1000) + O`; sort those offsets
and the raw reporter's offsets and require an empty set difference, not
merely equal counts.

Reuse FND-PARTY-254's complete descriptor-189 reading, retained as
GAME_DIR/analysis/item-removal-20261104/placement-equipment-hooks.txt,
and FND-ITEM-057's getter report. Check the signed branch and absence of
another write/call before the first push. FND-ITEM-056's retained
item-input-domains.json still hashes to
`ffc738c98dd640dfad4f11b04f784e71728b1cfd99efedc8ac4262bf67301bcb`;
keep its source identity and scope rather than regenerating that census.
The new caller and argument reports remain under
GAME_DIR/analysis/class-table-closure-20261106/. No original game ran.
