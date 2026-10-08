---
id: FND-EXE-229
title: Segment publisher compares a word difference while preserving a separate carry-path result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0785..4AE5:07A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:055A..4AE5:05A4
tool: Ghidra 12.1.3 PUBLIC, p-code segment-output reporter and executable-reader 2.5.0
environment: null
---

## Observation

FND-EXE-228's comparison callee at `4AE5:0785` has eleven instructions
and 28 bytes, ending exclusively at `4AE5:07A1`. It has no calls or
explicit memory stores, and returns near at `4AE5:07A0` without extra
argument cleanup.

It reads the current DS-relative word at `0x012C`. If nonzero, it loads
that value into ES, reads current ES-relative word `0x0010` and subtracts
current DS-relative word `0x0120` at 16-bit width. A result without unsigned
borrow returns directly with carry clear. A zero initial word or a borrow
takes the fallback: read DS-relative word `0x0126`, subtract DS-relative
word `0x0120` at 16-bit width, explicitly set carry, and return. Thus carry
marks the fallback path even when the fallback subtraction itself would
not borrow. Both arithmetic results retain 16-bit wrap behavior.

The instruction at `4AE5:078C` is rendered as though it reads ES, but its
decoded p-code has an ES register output. This positive semantic check
supports the segment-load reading without trusting the displayed operand
direction. It does not establish the loaded segment's storage identity.

The publisher calls this helper at `4AE5:0592`, saves its flags, then
compares the retained word against the returned word at 16-bit width using
an unsigned-above branch. The repeated-work arm restores those saved flags
before testing carry to choose whether to call `4AE5:0637`. The completion
arm restores flags and its saved ES, then reloads DS-relative word `0x0120`
for the segment store. The helper's difference result is therefore not the
published segment value, and its carry result is not the comparison's carry.

## Interpretation

This reads one of the publisher's callee/result dependencies, including the
separate word and flag results and the caller's restoration order. The helper
does not explicitly write the publisher's saved ES storage or source word.
Native state-segment admission, input-field writers, interrupt-enabled changes,
the other publisher callees and storage/size interpretation remain Q-EXE-001
and Q-EXE-010. No allocation contract, complete_reading or inventory replacement
is established.

## Alternatives

Treating carry as the arithmetic sign or borrow of every returned difference
is contradicted by the explicit set on the fallback path. Treating the returned
word as the published segment is contradicted by the caller's later reload.
The rendered segment-MOV direction cannot override its decoded ES output.

## How to reproduce

At revision `5a7f797`, use FND-EXE-226's original-source x86-bounds region,
hash and default limits, setting entry and sole entries value to `0x000407D5`
(`4AE5:0785`). Supply no seeds or summaries. In the resident Ghidra snapshot,
read-only with analysis disabled, run ReportInstructionWindow at `4AE5:0785`,
count fourteen, restricting this helper to the interval above; use
`4AE5:055A`, count 28, for the caller's save/compare/restore sequence.

Run tools/ghidra/ReportSegmentWrites.java at revision `5a7f797` with names
ES and limit 10000. Check its positive result at `4AE5:078C`. The scan reports
74571 decoded instructions, 3093 matching ES outputs, all emitted without
truncation. Its separate opaque-site output is capped; no negative search,
opaque-effect or native preservation claim follows from this positive check.
Source, configs, listings and reports remain in GAME_DIR and are not committed.
