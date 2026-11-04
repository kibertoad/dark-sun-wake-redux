---
id: FND-SCRIPT-036
title: The list-wide idle runner reloads next after script and unlink calls and stops after clearing its current link
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0EF1..2D40:0F89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0BA4..2D40:0C31
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2001..2D40:207E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:207E..2D40:2196
tool: tools/research/exec-census/resident_listing.py and direct_callers.py at e48a4e65; Python 3.12.10, Capstone module 5.0.7; composition with FND-SCRIPT-016/027, FND-COMBAT-062, FND-PARTY-098 and FND-CONFIG-144
environment: null
---

## Observation

Q-SCRIPT-017 retains the invocation and field contract of the nineteen-byte
walk-script records. FND-COMBAT-062 already supplies the per-slot idle
runner and map sweep; FND-PARTY-098 supplies the second-entry distance and
return-value gates. This reading adds the complete list-wide idle runner's
mutation order, separating that local body from evidence of its invocation.
The source is installed DSUN.EXE, length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, at analysis load segment 1000.
No original function was executed.

### List-wide idle runner

Entry 2D40:0EF1 loads the current index SI from DS:5AF5. Only exact -1
ends the traversal. For each index it forms the low word of nineteen times
that index and addresses the record at 4F49:08A3 plus that product.
It has no independent index-capacity or cycle check.

The record's word +04 must be zero for either action below; otherwise the
body advances. If word +0C is nonzero, it writes SI to trigger actor
4C13:0367 and calls 172C:000C with that script number, offset word +08
and selector one. It does not check the returned result. If word +0C is
zero, word +06 must also be zero before it instead calls unlink 0BA4
with SI. Neither path locally checks slot >=4, unlike per-slot entry 2001.
All offsets in this finding are hexadecimal unless stated otherwise.

After either child returns, or after skipping an ineligible record, the
body reconstructs the current record from SI and rereads byte +12. It
sign-extends that byte to the next index and compares it with -1. It does
not cache the successor before a script or unlink call, restart from the
head or test a script-stop byte at this continuation. A script may change
the successor it later reads; its target and storage effects remain open.

### Unlink continuation

Under valid disjoint record, neighbour, head and stack storage, unlink
0BA4 first reconnects the previous/next neighbours when their signed-byte
links are not -1, and moves the head when it equals the current index.
It then requests a zero fill of all nineteen current-record bytes through
1000:3FA2 and finally writes FF to byte +12 followed by byte +11.
FND-CONFIG-144 supplies the fill helper's local contract.

Assume that call returns, retains the runner's SI and restores its frame,
and the current-record identity and final link writes remain intact.
The runner then reads FF at current +12, sign-extends it to -1 and returns.
Thus removing one eligible idle record ends this list-wide pass even when
neighbours remain linked after it. This is a conditional call-order result,
not an observed missing update or a claim that this runner is invoked by
the ordinary map sweep. That sweep has its separate per-slot route.

### Existing per-slot gates and incoming evidence

The complete 2001 body agrees with FND-COMBAT-062: it requires mode word
+04 zero and signed slot >=4. A nonzero script word +0C runs first pair
+0C/+08 with trigger actor set to that slot. Otherwise condition word +06
zero admits unlink. The sweep calls it for individual slots, so it does
not inherit 0EF1's successor-after-unlink traversal.

The complete 207E body agrees with FND-PARTY-098: nonzero condition +06
admits the position/helper calculations. The later reread selects condition
one or two, using radius byte +10 with bit seven masked off and signed
distance comparisons. On a passing gate it sets trigger actor and runs
second pair +0E/+0A with selector one. Helper returns and record/slot
identity must survive the intervening calls; no universal visibility label
or admission guarantee is added here.

The encoded direct-call query finds 2001 at resident +1D0A and 207E at
resident +1D11 and overlay 173 +0DEA, matching FND-COMBAT-062 and
FND-PARTY-098. The known unlink calls at +0F68 and +2078 provide positive
controls. The same query returns no candidate for 0EF1. It searches only
the documented relocated far and same-region near encodings; computed,
split, other aliases/representations and independent entry transfers remain.
The empty candidate set does not prove the list-wide runner is unused.

## Interpretation

Q-SCRIPT-017 no longer needs another reading of these three local runner
gates or an assumption that the list-wide traversal preserves its successor.
Existing findings already describe target-cell setup, mode and second-entry
radius/condition bindings. Remaining work is their admitted producer/lifetime
and caller ordering, the list-wide runner's independent invocation, other
readers/writers and aliases, and the geometry helper's broader contract.
No complete-reading, confidence or parity promotion follows.

## Alternatives

A cached-successor traversal would continue to the old next record after
unlinking; this body instead reloads the cleared current link. A per-slot
sweep can still process later slots independently. A script mutation of
links can change traversal, and malformed indices/cycles have no local
normalization. None of these local facts proves an admitted gameplay case.

## How to reproduce

With the source identity above, use resident_listing.py separately on
the four half-open ranges in Locations. Keep all reports in GAME_DIR.
Follow 0EF1's post-child +0F6C..+0F86 continuation and unlink's final
+0C22..+0C31 link stores independently of the earlier neighbour/head changes.
Reuse the cited fill and other runner findings rather than assuming child
preservation. Query direct_callers.py for 2D40:0EF1, 2D40:2001,
2D40:207E and the positive-control 2D40:0BA4; verify the known sites from
their containing established functions, not from byte patterns alone.

Reports retained under GAME_DIR/analysis/walk-script-20261104/ are
runner-bodies.txt (0EF1 and two adjacent unrelated functions),
entry-runners.txt, unlink-body.txt, runner-callers.txt and
entry-runner-callers.txt. Only the named runner ranges are used here;
the adjacent functions' meanings are outside this finding.
No original code, listings or source-derived fixtures are committed.
