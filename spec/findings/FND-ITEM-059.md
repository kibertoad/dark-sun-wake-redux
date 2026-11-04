---
id: FND-ITEM-059
title: Equipment reset copies a live item word before dispatch and preserves the captured selector registers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007B99A..0x0007BA12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077C1A..0x00077CDB
tool: tools/research/exec-census/overlay_listing.py at 566c0455; composition with FND-PARTY-248/254 and FND-ITEM-056/057/058
environment: null
---

## Observation

Q-PARTY-109 retains the reset and flag effects of the after-equipment hook.
FND-PARTY-248 already reads reset 190 +365A's two branches. This checks its
complete local register/write scope and composes its rereads with +326A's
publication order, rather than treating reset as an opaque returning child.

### Exact local reset contract

+365A takes an attached-record index I and selects its 23-byte record R
through DS:19C1. It compares R word +0A unsigned with 9999. A value below
9999 selects the fifteen-byte type through DS:19CD and tests byte +0B mask
02 hex. With that bit set it copies R word +10 hex to R word +0E. With the
bit clear, or type word at least 9999, it copies R word +10 to R word +02,
which FMT-PARTY-006 identifies as quantity. It does not subtract one or test
the copied word for zero.

Under stable record/base identity and valid nonwrapping spans, the only own
persistent write is one two-byte destination: R+[0E,10) hex or R+[02,04).
Source word +10, signed bonus byte +16, type word +0A, position byte +13
and all other record bytes remain unchanged by this body. It contains no
callee, callback, resource access, interrupt or port operation. Its stack
frame stores are separate under the stated disjoint-frame condition.

The helper uses AX,BX,CX,DX and ES as scratch, without modifying SI or DI.
Thus selector/owner values correctly held in SI/DI on entry survive this
reset under ordinary returning execution and intact frames. This does not
prove those values survived every earlier status/DATA or rebuild operation;
FND-ITEM-058 independently supplies the status query's bounded preservation.

The unsigned type-sentinel test is not a 102-row capacity check. Valid native
type/record indices, pointer identity and nonaliasing remain prerequisites
when the type lookup is taken. A source word may change before reset is
entered; the contract copies what reset actually rereads, not a remembered
selector parameter supplied by its caller.

### Captured selector versus live copied value

FND-PARTY-254's +326A captures W from item word +10, then retains D=W-1 in
SI. It later calls +365A with the item index, not D. Let V be the item word
+10 at reset's deciding load. After the reset the selected destination
contains V, while an intact SI still contains the captured D. V equals D+1
only when the source word and deciding state have been preserved across the
intervening calls. This distinction applies after both the ordinary DATA
path and the special ability-rebuild path.

For the ordinary dispatch path, order is:

1. Read DATA property and pass the existing property/bonus gates.
2. Reset the item, copying V to its branch-selected destination.
3. Assign DS:6174 one.
4. Request 193 +2290 with owner in both object arguments, captured D,
   word six, two zero doublewords, the far SS pointer to a local byte,
   word one and word zero.
5. On normal return, assign DS:6174 zero and return.

The helper does not inspect a dispatch-success result before the final flag
clear, and it does not restore the destination written by reset. An actual
callee can itself change item or flag state; this is the outer local order,
not a claim that V survives every dispatched effect. There is no local
all-or-nothing or rollback condition around the reset/dispatch pair.

Incoming DS:6174 is not saved. A nonzero incoming value is therefore not
restored by the normal post-dispatch zero assignment. Early W-zero or
status-nonzero returns do not reach these publication/clear instructions.
The special W branch rebuilds abilities and then resets the item, without
reaching this helper's own flag-publication or dispatch site. Its rebuild
child's effects remain separate.

### Limits of the existing source reports

FND-ITEM-057's 93 property-one ordinary payloads are conditional candidates
for the path above. The available complete item census supplies W and bonus
marginals; its detailed rows cover the type-50 Look subset, not all 1931
items with their type/W pair. Those reports cannot assign all candidate
resets to quantity versus word +0E. A valid per-item join with the type bit
is required for such destination counts; no frequency is inferred from
independent marginals or from the Look-only rows.

## Interpretation

This removes reset as an unknown selector-register or broad persistent-write
child in Q-PARTY-109. Its bounded direct effect is the live-word copy, before
the captured-selector dispatch and flag publication. The remaining questions
are actual record/type/near-frame and pointer admission, the value at each
reread, DATA/status/rebuild preservation, dispatched effects and DS:6174's
independent writers/consumers. Q-ITEM-003 retains these placement and ownership
conditions. No native quantity change, failed effect, rollback defect,
confidence, parity or complete-reading promotion is claimed.

## Alternatives

Copying D instead of V would substitute the decremented captured selector
for the word reset rereads. Restoring incoming DS:6174 or undoing reset on a
dispatch failure would add behavior absent from this caller. Treating reset
as modifying W/bonus would add direct stores absent from its closed local
body. A disjoint write footprint does not admit aliased or invalid native
record/frame storage.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 190
range 7B99A..7BA12 hex; placement-equipment-reset.txt under
GAME_DIR/analysis/item-removal-20261104/ retains the complete body. Verify
both source reloads, the mask/sentinel branches, the two word stores and
absence of SI/DI writes or child calls. Reuse FND-PARTY-248's field binding.

Use FND-PARTY-254's complete +326A body, also retained in
placement-equipment-hooks.txt, to trace the reset, flag stores and dispatch
arguments. For the source-report limit inspect item-input-domains.json's
census.itemInputs and census.rows schema; the latter is the recorded type-50
subset, not an all-item join. No original code, record fixture, resource
payload or image is committed or executed.
