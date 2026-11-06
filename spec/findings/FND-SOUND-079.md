---
id: FND-SOUND-079
title: Top-level literal transfers do not bypass eight health assignments and GPL18 carries its value through producer and saved-local calls
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
tool: retained script_decode.py all-script and sweep reports from 211ea53c; RULE-SCRIPT-003/005 and FND-SCRIPT-026 interface review at 12d0a417
environment: null
---

## Observation

This connected entry-path review reuses FND-SOUND-077/078's installed
GPLDATA.GFF and OBJEX.GFF identities and retained reports. Its whole-file
location is the resource census scope, not a native code range. No new
binary decoder or original-game execution was used.

### Tested literal incoming forms

The eight assignment/request intervals are those in FND-SOUND-078.
A top-level decoded transfer query compares current-script literal targets
of opcodes 13, 27, 29, 3E, 3F, 63 and 64, and literal destination script/offset
pairs of opcode 14, with those intervals. Targets and script numbers use
their low 16 bits (RULE-SCRIPT-003). There are 21476 matching literal
transfer instructions across the decoded report: 3471 local calls,
797 cross-script calls, 3896 compare-case branches, 102 compare-end
branches, 8370 if branches, 3452 else branches, 694 loop-exit branches
and 694 loop-back branches. All operands selected by this query are literal.

None targets a decoded offset strictly after one of the eight assignment
starts and at or before its request. The only target in their closed
intervals is GPL/133 at 020A, whose loop-exit branch targets 0255, the
second assignment's start rather than its request at 0261. This removes
these tested incoming forms from the assignment-bypass component.

The query retains a positive decoded example of each present transfer
form: GPL/1 at 0009 (if), 0039 (else), 00E2 (local call), 00F8
(cross-script call), 09C7 (case) and 0A7D (compare end), plus GPL/2
at 0424 (loop exit) and 071C (loop back). Their target operands follow
the independently read opcode layouts in FND-SCRIPT-024/RULE-SCRIPT-003.
FND-UI-050's independently known calls from MAS/50 at 0181 and 0362
to GPL/142 at 03A9, and at 036A to 0438, supply cross-script controls.
Its GPL/142 local calls at 0434 and 053C target 0540 as recorded.

Opcode 12 has no top-level decoded occurrence and supplies no positive
control here, so no separately validated exclusion for that form is claimed.
The query also excludes nested transfers, menu targets, registered entries,
restored frames, runtime-built targets, the GPL/37 undecoded interval and
native or changed-script entry mechanisms. It is not an exhaustive incoming
census or proof that a local value cannot change through an alias.

### The carried GPL18 value has a producer-before-consumer route

In GPL/18's decoded entry at 0001, the compare block beginning 0045
chooses a local producer call from the selected object's field zero:
case -3080 calls 0790 at 0051; case -3081 calls 0C5B at 005A;
the remaining compare path calls 0F4C at 0060. Later, the compare on
local number five at 00BC calls consumer subroutine 0643 from 00C4
when its value is one. That consumer contains the health request at 0769.
Thus the textually later writers identified in FND-SOUND-078 can run
before the consumer through local calls; textual order is not execution order.

The 0790 and 0C5B producer bodies contain the eleven local-number-nine
assignments already listed by FND-SOUND-078, together with assignments to
local number five. Their observed number-nine publications are 1..7 on
those paths. The case tests, other producer paths and retained local state
still determine admission to the later number-five-one consumer call.

Inside the consumer, a condition at 0723 gates the sequence containing
the request. On its admitted path, request 29 saves locals before a call
to GPL/21 at offset 0001, and request 30 follows that call. A second
request-29/GPL21-at-0315/request-30 sequence immediately precedes 0769.
FND-SCRIPT-026 records local clearing when another script becomes current
and the separate local save/restore mechanisms. The request's value
therefore depends on those save-buffer, current-script and continuation
bindings, not merely on the earlier assignment. This is a concrete
preservation dependency; no unconditional successful restoration is inferred.

GPL/15 likewise has a local producer route: its 00FA assignment can reach
the 0131 request through tests and calls to local helper 01B3. That helper
stores literal four into local number one at 0217 and returns at 021C.
The returning helper path can therefore replace the earlier 1..3 draw.
This narrows the carried-value history without excluding alternative entries,
restored values or aliases.

## Interpretation

The eight adjacent paths no longer need another search for the tested
top-level literal bypass forms. The remaining entry mechanisms are named
above. For the two carried locals, Q-SOUND-004 now has concrete local-call
and saved-local dependencies to establish, instead of an unspecified history
of writes. Neither this census nor these conditional routes establish every
health input, a whole-entry complete reading or a queue closure.

## Alternatives

A direct decoded literal transfer to one of the eight requests without its
assignment is absent within the tested forms and scope, with the controls
and exclusions above. A universal no-bypass conclusion would ignore the
excluded entry mechanisms. Assuming GPL/18's local nine survives its
cross-script calls automatically ignores local clearing and save/restore.
Assuming GPL/15's first random assignment is always the last producer ignores
the called helper's literal-four store.

## How to reproduce

Reuse music-health-all-scripts.txt and music-health-script-sweep.txt under
GAME_DIR/analysis/handler-census-20261106/, generated by script_decode.py
at 211ea53c with --all and --sweep. Verify source fingerprints as in
FND-SOUND-077. Select top-level resource-prefixed instruction lines for
opcodes 12, 13, 14, 27, 29, 3E, 3F, 63 and 64. For 14 use its second
operand as destination script and first as target; for 27 use its second
operand as target; other selected forms use the first operand in the current
resource. Keep only literal decimal operands and reduce to low words.
Compare with the eight FND-SOUND-078 intervals, retaining target-equals-start
separately from assignment-bypassing targets. Check the positive controls
and retain all excluded mechanisms above.

Read GPL/18 at 0001..0101, 0643..0790, 0790..0A1A and
0C5B..0F4C, and GPL/15 at 00FA..013A and 01B3..021D in the retained
report. Reuse FND-SCRIPT-026's separately recorded native local-state
interfaces. No runtime call ordering or successful restoration was observed.
