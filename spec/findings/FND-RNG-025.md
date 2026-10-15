---
id: FND-RNG-025
title: Psionic action and resistance checks qualify five range-helper sites with conditional draws
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061540..0x0006159B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061D2E..0x00061E8F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061E94..0x00062024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062024..0x00062038
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000616D6..0x00061834
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085052..0x000851C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03..2D40:3A43
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py and resident_listing.py; tools/evidence/report.mjs table; fingerprint, FBOV fixup and synthetic branch-pair checks
environment: null
---

## Observation

The range-helper incoming query recovers five sites in overlay 176:
+08A0, +08B1, +0925, +0AB8 and +0ACD. Its exported trampolines
5691:0075 and 5691:0066 resolve to +07EE and +0954 respectively.
Reading from those entries and resolving their branches qualifies the
five sites as calls to resident 2D40:3A03. They are callers of
FND-RNG-012's inclusive-range helper, not direct modulo reductions.
Six other recovered range candidates in overlay 179 remain outside
this finding; indirect and noncanonical incoming forms remain open.

### Action check at overlay 176 +07EE

This far entry receives actor, power and a word modifier, whose low byte
is consumed later. FND-CONFIG-052 identifies its caller's psionic feedback.
The reviewed call at +0294 supplies actor, power and zero, copies returned
AL into DS:4224 and tests it as a signed byte. FF selects disruption
feedback; zero selects failed-check feedback for a party actor. Positive
results continue that caller's separate success path. No whole action,
UI outcome or input-admission contract is inferred from those messages.

The entry first asks +0000 whether the actor has the power. Its low-byte
zero result returns zero before any of the three range calls. The rank
helpers +0000 and +0033 have no RNG call: for actor indices below four
they read the PSST byte identified by FND-PARTY-104, and for indices of
four or more their meaningful low-byte result is one. Negative indices
and indexed storage admission are not proved valid here.

After eligibility, selector 573B:00A7 is called with 58. Its returned
word 9999 bypasses the two-roll contest and proceeds to the final check.
An equal-to-actor result returns FF without a range call. For another
index, the body computes an opposing signed word threshold from the
code-3 value query, the signed upper five bits of the power-32 table
byte, and the low-byte rank for power 32, less one. Callee effects and
that selector's admitted results remain dependencies.

On this other-index path, it reads the acting power's rank twice in
succession and keeps the second low byte as an unsigned word. A preceding
actor value query is not the retained comparison threshold: both later
rank calls replace its register result. It then calls the range helper
with lower 1 and upper 20 twice, storing their low bytes as a and b.
Both calls precede the first comparison. Each costs one generator draw,
even when a alone already determines the following FF return.

The contest returns FF if a is greater than the retained acting rank.
Otherwise it proceeds to the final check when b is greater than the
opposing word threshold, or when b is less than a; it returns FF in
the remaining case. These are signed comparisons. Thus a equal to the
rank passes the first test, while b equal to a and no greater than the
opposing threshold selects FF.

The final check computes a threshold using the actor's selected value
query, the low modifier byte, the signed upper-five-bit table adjustment
and the low rank byte, less one. Additions after the value query retain
only a byte. It obtains another independent range result r in 1..20.
A result of 20 returns zero. Otherwise it returns r when r is 1 or the
stored threshold interpreted as signed is at least r; it returns zero
for the other cases. The successful result is the die byte, not just one.
This preserves both the automatic 1 branch and the automatic 20 failure,
including when the computed threshold's byte has wrapped negative.

The three range calls' argument pair is formed by a single 32-bit push
whose little-endian stack words are lower 1, then upper 20. Four bytes
are removed after each far call. The local direct draw paths are:

| Reached path | Direct generator draws through these range calls |
|---|---|
| Eligibility returns zero, or selector names the actor | 0 |
| Selector returns 9999, then final check | 1 |
| Other-index contest returns FF | 2 |
| Other-index contest reaches final check | 3 |

These counts exclude unknown draws or mutations inside the selector or
value-query callees, and any earlier caller work. They are not total
draw counts for an entire psionic action.

### Resistance-side predicate at overlay 176 +0954

This separate far entry receives an object and a type word. It returns
zero before either range call when the record mapper's low byte is zero
or the type word is not 0400 hexadecimal. For admitted mapped records,
it derives an upper bound U from byte fields and rank/cost arithmetic,
then clamps it to at least one.

For an object below four it scans power indices 0..33. A bounded ten-word
switch selects powers 24, 27, 28, 30 and 33 for the activation-bit/rank
and cost checks. Its ten targets are +09C4 for indices 24, 27, 28, 30
and 33, and +0A49 for 25, 26, 29, 31 and 32; the range guard routes
all other indices to +0A49. This proves the skipped interval is reached
code, not an arbitrary decode from its interior. A passing stronger
candidate replaces U and the retained cost. After scanning, the retained
cost is subtracted from the combatant's points word before either roll.
Failure of a later test does not roll back that local debit.

For other objects, a separate details-byte comparison can raise U. Under
the reviewed byte loads and unchanged mapped sources, U after clamping
is 1..255: the initial unsigned-byte-minus-seven term is at most 248;
the party rank-plus-half-cost candidate is at most 254; and the other
branch's unsigned details byte is at most 255. No wrapped negative span
from FND-RNG-012 arises in these local endpoint pairs.

The predicate sign-extends the current DS:4224 byte, then calls the
range helper with lower 1 and upper U at +0AB8. If that signed byte
is at least the returned word, it returns zero without the second call.
Otherwise it reads and sign-extends DS:4224 again, calls the same range
at +0ACD and returns one only when this second result also exceeds
that second threshold reading. The helper preserves the caller's DI
holding U. The two threshold reads are distinct, not one cached input.

When U is one, either or both calls can still execute, but each returns
one without a generator draw. For U above one, the first call draws once
and the second draws only if its branch is reached. This distinguishes
helper-call count from generator-draw count. No unconditional two-draw
claim follows from the two call sites.

FND-PARTY-112's resistance helper, overlay 197 +1C52, supplies a reached
caller: for type 0400 it passes the object and type to this predicate
at +1DB2. A true AL clears that caller's local result before return.
This does not mean every effect of that type reaches the call or that
all surrounding resistance/cost semantics are established.

## Interpretation

Five range candidates have qualified instruction boundaries, callers,
endpoint preparation and local consumption order. The older lack of
a qualified range consumer in RULE-RNG-001 is resolved for these roots.
Q-RNG-001 retains other candidates, full rule mapping, callee effects,
shipped inputs, index/storage admission and indirect RNG/state access.

As a synthetic branch cross-check, enumerate a and b over 1..20. For
acting rank 20 and opposing threshold 20, 210 of 400 pairs select FF
and 190 reach the final check. For rank 1 and threshold 20 all 400
select FF, but both contest rolls have already happened. For rank 20
and threshold zero all 400 reach the final check. These are predicate
checks, not equally weighted native probabilities or shipped inputs.

## Alternatives

- Combining the two contest rolls into a short-circuit comparison would
  change RNG state on the first-roll rejection branch.
- Returning a Boolean one for every successful final check loses its
  returned die value, which the caller stores for later checks.
- U equal to one is an actual local clamp case, not proof of a draw.
  Calls to an inclusive-range helper with equal endpoints consume none.
- Mapped record identity, selector/value-query behavior, table writers,
  valid actor/power indices and native reachability remain open. No
  complete psionic-action rule or implementation is authorized by this
  finding. The delivered resident harness cannot execute these overlays.

## How to reproduce

Use the project evidence Python and approved installed DSUN.EXE.
Run direct_callers.py for 2D40:3A03, 176+07EE, 176+0954 and
197+1C52, with the previously established resident helper as the target.
Resolve 5691:0075 and 5691:0066 with trampoline_target.py. Read the
listed overlay ranges from their entries with overlay_listing.py;
check the range body and preserved registers with resident_listing.py.
Verify default word sign extensions rather than relying on the printed
mnemonic for opcode 98, the 32-bit push's two stack words, signed byte
branches, both contest calls before comparisons, and the point debit.

Use report.mjs table on the fingerprinted DSUN.EXE with start 62024
hexadecimal, count/limit 10, stride 2 and one width-2 target at offset
zero. Count evidence is the unsigned selector bound of nine at +09B5
before the two-byte jump-table index. Resolve each target relative to
overlay 176's code base and verify it is +09C4 or +0A49. Configurations
and original-derived output stay in GAME_DIR/analysis/rng-consumers/.
For the synthetic pair counts, enumerate a and b from 1 through 20
and count a > rank or (b <= opposing_threshold and b >= a).
No original code is executed and no original-derived fixture is committed.
