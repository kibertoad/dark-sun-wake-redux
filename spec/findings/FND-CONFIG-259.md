---
id: FND-CONFIG-259
title: Setup marks the first matching record in each card group and retains a finite census of other EC writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:00B0..17FB:012A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:01B0..17FB:023A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:0346..17FB:03D3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1861:06DB..1861:0861
tool: Ghidra 12.1.3 ReportFunctionSummary.java; tools/evidence/report.mjs x86-operand-candidates; resident_listing.py; checkout-local evidence Python 3.12.10
environment: null
---

## Observation

FND-CONFIG-257's constructors test selected-record word+EC. This finding
reads one conditional set-one producer and its group lookups, and preserves
the other literal sites instead of treating the marker as immutable.
SOUND_DS.EXE is204593 bytes, XXH3-128236c2dc23c071eca421eb5b427caee57.
All addresses, offsets, masks and card numbers below are hexadecimal;
report counts and record sizes are decimal. Data segment is1E36.

### Group marking and first-match selection

The complete17FB:00B0..012A body takes one card-number word. It performs
three independent membership tests in this order, and for each nonzero
result obtains that group's index and writes word1 to record+EC:

| Group | Membership helper | Index helper | Group far pointer | Signed count |
|---|---|---|---|---|
| Main | 17FB:0346 | 17FB:01B0 | DS:188C | DS:EC74 |
| Digital | 17FB:03A4 | 17FB:020C | DS:1894 | DS:EC76 |
| Music-only | 17FB:0375 | 17FB:01DE | DS:1890 | DS:EC78 |

These group bindings and254-byte record stride agree with FND-SOUND-021.
Every membership/index helper has no child: it starts at index0, compares
signed index<count and compares record word+28 with the supplied number.
Membership returns1 at the first match, otherwise0. Index helpers return
the first matching index, otherwise0. Nonpositive counts admit no member.
Record offsets use low-word index*254 and addition, retaining the far
base segment. Valid nonwrapping group spans and preserved segments remain
conditions; this arithmetic is not an original bounds check.

Thus the local producer marks at most the first matching record per group,
not every duplicate ID. Absent groups are skipped. After all three tests,
it always copies the input card number to DS:ECCE, even if none matched.
Its own stores do not populate EE..FC or clear EC. Membership and index
are separate calls and reads; retained group/count/record state is needed
to infer that they select the same member. No global preservation follows.

### Recognized caller sites

The Ghidra report gives six ordinary calls from1861:06DB. The bounded
instruction reading through0861 verifies these supplied card numbers:

| Call site | Supplied card number | Immediate local gate |
|---|---|---|
| 1861:074F | 81 | 1000:307B's AX-or-DX result nonzero, then17FB:02CD nonzero |
| 1861:077C | 6A | 17DA:000B(0) returns AX1, then17FB:02CD nonzero |
| 1861:07A9 | 7C | 1000:307B's AX-or-DX result nonzero, then17FB:02CD nonzero |
| 1861:07D8 | 7D | 1000:307B's AX-or-DX result nonzero, then17FB:02CD nonzero |
| 1861:0803 | 82 | 158E:0006(0) returns AX1, then17FB:02CD nonzero |
| 1861:085B | local word | The current SI flag is nonzero at0809 |

The02CD calls receive the same literal card number as their following
mark call. The last arm calls164C:03BE with a local word destination.
For returned local value6 it supplies DS:ECCE; for4/5 it supplies73,
for2/3 supplies67, for1 supplies66. Other values leave the card-number
local at its initialized zero. These are caller-side result tests, not
proof that any device/environment condition occurs natively. The initial
SI-setting branch and intervening resets remain explicit in the retained
prefix; no caller return-status claim or all-entry census is made here.

### Controlled EC representation census

Reuse FND-SOUND-045/047's source,25 declared regions, entries and transfer
definitions, including omission of unsupported indirectCalls. Query literal
EC with scanLimit1000000 and limit10000. Independent positive controls are
the constructor comparison at file5AC3 and the mark store at9484. Both are
verified memory operands. A negative control at5AC4, inside that constructor
instruction, is rejected as not a verified memory use.

Both reports scan58208 starts, are untruncated and partialSearch=false,
and retain59 walk gaps. There are30 verified memory uses, no verified other
operands,37 rejected overlaps and no unresolved literal boundaries. Of the
verified uses,25 are writes and5 are reads. The three writes in00B0 are
the producer above; two reads are the constructors and three are in the
separate marker-testing routine17FB:012A. That routine's complete effects
are not established here.

The remaining22 writes are in declared modules154E,158E,15FD,164C,1783
and17DA. Their record identity, selecting states and extended-field effects
remain obligations, not discarded equal-offset hits. The census excludes
computed displacements, implicit/block access, segment aliases and runtime
reachability; it is not a global absence claim about EC or its other writers.

## Interpretation

There is a concrete group-based route that makes EC nonzero before a
constructor might choose its extended fields. It supplies no values for
those fields and does not prove a current selected record took that route.
Q-CONFIG-005 can remove this local group-marking/lookup mapping from its
unknown producer obligations. It retains the22 other literal writes,
marker-test callers, EE..FC and initialization producers, group/record
lifetime and aliases, excluded mechanisms and actual device outcomes.
No original was run, complete reading declared or entry status promoted.

## Alternatives

Membership is not a count or selected index, and an index helper's fallback
zero alone is not proof of membership. The marking routine distinguishes
them by testing membership first. Under changed group state their results
can diverge. Equal EC displacements elsewhere are not automatically accesses
to these parsed records. The controlled census preserves that distinction
and its excluded mechanisms instead of proving universal producer coverage.

## How to reproduce

Local output is GAME_DIR/analysis/config-block-construction-20261108/.
Run packaged ReportFunctionSummary.java against the verified SETUP-blocks
project for17FB:00B0/0346/0375/03A4/01DE/020C and1861:06DB. Reuse the
MainGroup index reading in FND-SOUND-021. record-marker-ghidra.log and
record-marker-caller-ghidra.log retain those results. Regenerate
record-marker-instructions.txt with resident_listing.py for the first three
location ranges, and record-marker-caller.txt for the fourth.

Copy FND-SOUND-047's setup-field-00da.json base, change query.offset toEC
and controls to5AC3/9484, and run x86-operand-candidates twice. The local
ec-query.json retains the full configuration. ec-report.json and ec-repeat.json
have matching SHA-25690e391cf6820747a72937eefe8624b2dcb7ef6e335c4dff5140ec80f8a793151.
The ec-negative-query.json and ec-negative.log retain the failed5AC4 control.
Keep reports, code listings and original-derived configurations local.
