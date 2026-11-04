---
id: FND-ITEM-028
title: The bounded resident kind dispatcher replaces its kind-five child's low byte with local bookkeeping
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:044D..371D:04A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:04CA..371D:04DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0002C879..0x0002C883
    kind: file-data
tool: tools/evidence/report.mjs table and x86-incoming; tools/research/exec-census/resident_listing.py at 328637a2; locked evidence reader
environment: null
---

## Observation

FND-ITEM-027 leaves a possible low-byte return through resident 371D:04CA.
The source-derived dispatch table and its consuming instructions identify
the known local caller, without assuming the unmodeled indirect jump has
no successors.

Resident 371D:044D clears local byte BP-1. It sign-extends its byte
argument, subtracts three in a word, and tests the resulting index against
four unsigned. Values outside zero through four reach the ending. The
admitted index is doubled and selects a little-endian word from CS:04A9.
Exactly five words fit before the next function at +04B3.

The source word for kind five selects +0482. That block passes its own
far-pointer argument to +04CA at +0488. On return it reaches +04A1 for
argument cleanup, then +04A4 loads AL from BP-1 before returning. No store
of the child's AL or conditional test of it intervenes. The local remains
zero on this branch under the ordinary intact-frame assumption. Kind
three's different branch calls +04B3 and sets that local to one; the
dispatcher therefore has its own bookkeeping byte rather than a uniform
forwarding contract. Frame/alias preservation remains Q-ITEM-011.

The uncontrolled incoming report for +04CA has zero confirmed matches
and one raw candidate at +0488 in an undecoded range. Adding the explicitly
partial five-word jump declaration changes that site to one entry-path
match, with zero remaining matching raw or contested calls. The call is
also required as an added decoding control. This qualifies its instruction
identity under the table declaration; it does not prove native reachability
or that the table cannot be changed at runtime.

## Interpretation

The remaining known local consumer replaces the possible hit low byte with
its own result. Together with FND-ITEM-018/021/022/023/024/025/027 this
settles Q-ITEM-009's qualified direct-consumer reading. This is not a proof
that every computed incoming route has been found. Q-ITEM-010 retains those
routes and the partial-table qualification; Q-ITEM-011 retains the frame,
count and storage admission needed for these local readings. No rule is
promoted or declared completely read, and AH remains unnormalized.

## Alternatives

- Zero confirmed calls without an indirect-jump model did not mean no caller.
- A source table edge supports a conditional reading, not observed execution.
- Kind five returns the local bookkeeping byte, not its child's AL.
- A zero local is not preserved under arbitrary aliased child writes.

## How to reproduce

Use the installed DSUN.EXE resolved by tools/game-dir.mjs, size 634416,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with
371D:044D..371D:04A9 and 371D:04CA..371D:04DF. The first range ends
before the table, rather than decoding table data as instructions.

The table reporter configuration uses the same source/hash, start 0x2C879,
count five, stride two, limit five and one field named target at offset zero,
width two. Its countEvidence is the sign-extended byte-minus-three gate,
unsigned index bound four and doubled word index at 371D:0457..046B.
Run node tools/evidence/report.mjs table with that configuration.

Reuse FND-ITEM-027's full controlled incoming configuration with target
0x2C89A. For the declared query add indirectJumps containing site 0x2C837,
exhaustive false, and table start 0x2C879, count five, stride two,
fieldOffset zero. Consumer evidence is the argument transformation, bound
and CS word indexing above; table evidence is the bounded five-word report.
Add control 0x2C858 to the existing eight call controls. Keep every source,
region, root, source-table and scan/instruction/result limit unchanged.
Run node tools/evidence/report.mjs x86-incoming with each configuration.

Both queries retain 896 unresolved calls, are untruncated and have
partialSearch false. The declared query retains the nonexhaustive table
assumption and other computed-transfer gaps; no universal negative follows.
Reports/configurations stay in GAME_DIR/analysis/hit-incoming-20261104/:
resident-dispatch-table.json and .report.json; outer-371d-04ca.json and
.report.json; outer-371d-04ca-declared.json and .report.json.
The retained resident-forwarding-parent.txt includes the table's bytes
after the function; use only its code through +04A9 for instruction claims.
No original listing, bytes or table dump is committed.
