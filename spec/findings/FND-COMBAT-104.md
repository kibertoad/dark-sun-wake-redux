---
id: FND-COMBAT-104
title: Matched local-call histories narrow selector assignments while GPL 4's default prevents a simple bypass inference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: tools/research/exec-census/script_local_paths.py at 728b8e24, SHA256 1df8f4564bdf93715e43f0c0f1115b071590e68ec8d0487443e6ed8d702c66c2; retained script_decode.py index of FND-COMBAT-100
environment: null
---

## Observation

FND-COMBAT-101 lists 38 declared literal entry controls and 22 direct
assignments for the four local-number selector sites. This reading reuses
that unchanged index and those controls, pairing local calls with their
returns rather than following a continuation as if every call returned.

### The bounded histories reproduce the positive controls

The input index SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
The committed tool's synthetic controls include assignment bypasses,
callees that stop instead of returning, matched menu frames, unknown versus
literal-zero assignments, other effects, rejected spans and caps.

Each run uses a shared budget of 20,000 graph states across its declared
roots and a bound of 32 additional modeled local frames. No selected run
reaches either cap or reports a rejected span. Repeated reports are
byte-identical, including their tool, listing and config identities.

| Resource and selected local | Declared controls | Controls with a first-target history | Distinct direct writer sites in those histories | Histories with no known direct assignment |
|---|---:|---:|---:|---:|
| GPL/4 local 11 | 14 | 13 | 10 | 13, one per reaching control |
| GPL/119 local 0 | 13 | 4 | 4 | None in the modeled reaching paths |
| GPL/137 local 5 | 6 | 6 | 6 | None in the modeled reaching paths |
| GPL/163 local 0 | 5 | 1 | 2 | None in the modeled reaching paths |

The first-target control counts reproduce FND-COMBAT-101's independently
recorded hits. The 22 distinct writer sites are exactly its direct-assignment
list, all with nonzero literals. GPL/4 contributes 143 aggregate histories
across its reaching controls; the other resources contribute four, six and
two. Every target history is marked with possible intervening other effects.
That conservative mark does not establish that a value actually changes.

These are paths before the first requested selector instruction, not its
parameter execution or later iterations. Cross-script calls, returns to
unmodeled script contexts and menu selection remain explicit boundaries.
An entry without a target history is not an admitted-runtime absence result.

### GPL/4 has a default assignment after its nine cases

The ordinary local routine beginning at 0x03CD compares global number 12.
Its nine case instructions test literals 0 through 8. Cases 0 through 4
assign 66 to local number 11; cases 5 through 8 assign 65. After the cases,
instruction 0x29 at 0x050B targets the compare end at 0x052C. The fall-through
default assigns 65 at 0x0527 before that end; the local return is at 0x052D.

RULE-SCRIPT-003 describes the relevant matched-state relation: compare
initialization clears the matched flag, a matching case sets it, and 0x29
jumps only when it is set. With ordinary entry, valid compare depth and
preserved compare/variable storage, a matched case has already assigned
local 11; if no case matches, the default assignment runs. This local block
therefore does not omit the assignment merely because the compared number
lies outside 0 through 8.

The broad graph takes conditional branches independently. In particular,
skipping every case body and then taking 0x29's matched branch loses that
correlation. Its no-known-assignment history is not, by itself, evidence
that the original reaches the selector with an unchanged or zero local.
Entering after initialization, invalid depth, aliases, opcode callbacks and
other state changes remain outside the conditional block reading.

## Interpretation

The declared controls now have bounded direct-assignment histories with
matched local returns and explicit unresolved boundaries. Together with
the compare-default reading, this rules out inferring a real assignment
omission solely from the abstract GPL/4 bypass or its unmatched case value.

It does not prove nonzero runtime selectors. Actual entry and compare-frame
admission, local-bank identity and lifetime, intervening effects, returning
cross-script paths and parameter preservation remain Q-COMBAT-056 and
Q-COMBAT-055. No initial unknown value is treated as zero, and no callback,
native transition, gameplay rule or complete reading is established.

## Alternatives

- Every listed direct literal being nonzero does not prove preservation
  until the selector read. All reaching histories retain possible effects,
  and other entry sources remain outside the declared-root set.
- A no-assignment graph history does not prove zero. The graph does not
  evaluate conditions or correlate the compare matched flag; the ordinary
  compare/default block above has an assignment on both matched and
  unmatched paths under its stated preservation conditions.
- Missing histories cannot exclude execution through a cross-script return,
  changed frame or callback. Those boundaries remain in the reports and queue.

## How to reproduce

Reuse FND-COMBAT-100's source identities and FND-COMBAT-101's retained
decoded index and 38 decimal entry controls. Use script_local_paths.py with
one config per resource: target/local pairs are GPL/4 0x02A3/11,
GPL/119 0x0547/0, GPL/137 0x1069/5 and GPL/163 0x027A/0. Set the explicit
raw listing SHA-256 above and retain the default state/frame bounds.
Compare report identities, root hits, assignment sites, effect flags and
every unresolved reason. Never replace a boundary by an assumed return.

Read GPL/4's indexed instructions 0x03CD..0x052E with RULE-SCRIPT-003's
compare, case, matched-branch and end semantics. Check the default assignment
as well as all nine case bodies. Do not infer branch feasibility from the
graph's independent conditional edges.

Configs and reports are retained under
GAME_DIR/analysis/local-selector-paths-20261103/research-01/ and agree exactly
with the corrected matched-menu control reports in that store. The research
directory also retains GPL-4-compare.txt. Earlier reports outside those two
subdirectories precede corrected menu pairing and are diagnostic only.
All original-derived indexes, configs and reports remain local.
