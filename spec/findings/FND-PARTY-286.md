---
id: FND-PARTY-286
title: Immediate callers of three word-selector clients supply no new 1001 exclusion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091E42..0x00091F10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000920F6..0x00092115
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084E94..0x00085044
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py at a3a10444; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-PARTY-272, FND-MAGIC-018/019 and FND-PARTY-201
environment: null
---

## Observation

Q-PARTY-124's three word-input clients remain locally capable of
requesting 1001 as FND-PARTY-272 establishes. Installed DSUN.EXE
is 634416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.
This attempt checks immediate callers for a deciding bound.

The direct-call reporter returns six encoded candidates: overlay 179
+0E45 to 193 +1E81; 193 +04B0/+1431 to 193 +1F88; and
208 +0666/+087C plus 197 +1B83 to 208 +07F2. FND-MAGIC-018/019
and FND-PARTY-201 already describe the first three numeric arguments
as the retained DATA number. The following source reading checks the
other three, rather than treating another caller edge as an answer.

Overlay 208 +05B2 loads its second word argument into DI, requests
that DATA number directly, and later compares DI with signed zero,
235 and 269. These branches select a level helper for 0..234 or
235..268; outside those ranges the code supplies literal level six.
They do not reject the number. After a later helper, record byte +0C
chooses the route to +07F2 at +0666 versus a different helper.
The +0666 route pushes DI as the number. Thus input 1001 is not
excluded by those range comparisons, under retained DI/frame state
and admitted continuation. The preceding direct getter also uses DI.

Overlay 208 +0866 loads its second word argument into SI, calls
overlay 177 +07AF, then pushes returned AX as the other argument
and SI as +07F2's number. A level result does not replace the number.
This prefix has no own deciding number comparison. Preservation by
the reached helper and actual incoming number remain obligations.

Overlay 197 +1A94 loads its third word argument into SI. Its switch
on the first argument and effect-state branches can reach +1B78,
where only SI equal to FFFF skips +07F2. The continuing +1B83
call pushes literal one and SI. The local number test does not
exclude 1001; actual caller, state, register and frame admission remain.

## Interpretation

This attempt removes no remaining caller-domain obligation. It rules
out interpreting +05B2's level-choice comparisons as a number-rejection
gate, but establishes neither an actual game request of DATA 1001
nor universal exclusion at any of the three getter sites.

The independently answerable three-client caller-domain question moves
to Q-PARTY-134, split from Q-PARTY-124. All 83 unresolved direct sites
remain accounted for: these three plus eighty others. The repeated
equipment-hook preservation and excluded indirect/bypass mechanisms
remain. This is a split, not an answer or closure. FMT-PARTY-007 stays
supported; no complete reading is declared.

## Alternatives

- Level selection outside the usual spell/power range is not rejection
  of the DATA number. The literal-six continuation disproves that reading.
- A formal retained input 1001 is not evidence a native caller supplies
  it. Conversely, a familiar spell parameter is not a proved bound.
- The encoded reporter omits indirect calls, jumps and bypass entries.
  Six candidates do not establish exhaustive incoming execution paths.

## How to reproduce

Run direct_callers.py on the fingerprinted executable for 193+1E81,
193+1F88 and 208+07F2. Retain its encoding exclusions; reuse
FND-MAGIC-018/019 and FND-PARTY-201 for the earlier caller bindings.
Run overlay_listing.py for descriptor 208 at 91E42..91F10 and
920F6..92115, and descriptor 197 at 84E94..85044. File bounds are
hexadecimal and half-open. Check the default-six branch, pushed DI/SI
and the FFFF comparison. Reports word-selector-direct-callers,
word-selector208-first-caller, word-selector208-second-caller and
word-selector197-caller remain under
GAME_DIR/analysis/class-table-closure-20261106/. No original game ran.
