---
id: FND-EXE-232
title: Carry-path helper resets subtracts and finally replaces the published state word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0637..4AE5:0672
tool: Ghidra 12.1.3 PUBLIC, p-code segment-output reporter and executable-reader 2.5.0
environment: null
---

## Observation

The carry-path callee of FND-EXE-228 at `4AE5:0637` contains 24 instructions
and 59 bytes, ending exclusively at `4AE5:0672`. It returns near at
`4AE5:0671` without extra argument cleanup. Its two calls are to
`4AE5:07A1` and `4AE5:06E4`; the bounded CFG assumes both return.
FND-EXE-230 reads the first helper's explicit arithmetic effects. The
second helper's effects are not established here.

It first reads current DS-relative word `0x012C` into AX and clears CX.
Each traversal iteration increments CX at sixteen-bit width, pushes AX,
loads ES from AX, then reads current ES-relative word `0x001C` into AX.
A nonzero word repeats from the increment. A zero word ends this traversal:
the procedure stores zero into current DS-relative word `0x012C`, reads
DS-relative word `0x0126`, and stores it into DS-relative word `0x0120`.

The next loop pops one retained word into ES, saves CX, reads current
DS-relative word `0x012C` into AX, writes AX to current ES-relative word
`0x001C`, then writes ES into current DS-relative word `0x012C`. It calls
the arithmetic helper, subtracts its returned AX from DS-relative word
`0x0120` at sixteen-bit width, calls `4AE5:06E4`, restores CX and uses
sixteen-bit LOOP to repeat from the pop. On loop exit, it reads current
DS-relative word `0x0124` into AX and replaces DS-relative word `0x0120`
with that value before returning. The final replacement therefore follows
all the loop subtractions; those intermediate values are not its final
explicitly stored result.

Decoded p-code has ES outputs at the traversal load `4AE5:063E` and the
later pop `4AE5:0651`. The former is rendered with the segment-MOV operands
reversed; its semantic output supports the load direction. This is not
evidence that any loaded segment identifies an admitted header.

The first traversal has no explicit bound other than encountering a zero
word, and retains a pushed word for every visited link. Its count wraps at
sixteen bits. The second loop preserves that counter around each pair of
calls, but preservation of the saved stack word and the number of retained
words has not been established against the unread callee or interrupts.
Finite links, absence of cycles and safe stack extent are not proved.

## Interpretation

This identifies further direct writes to FND-EXE-228's published state word,
including a reset, intermediate subtraction and final replacement, together
with their surrounding link and stack operations. It also identifies writes
to the state word read by FND-EXE-229's difference helper. It does not imply
that the publisher's state monotonically increases or that its loops terminate.

Q-EXE-001 and Q-EXE-010 retain native DS/ES and link/header admission, all
writers of the three source words, effective-address aliases, incoming stack
and traversal bounds, saved-word preservation, interrupt-enabled changes,
the effects of `4AE5:06E4` and the other publisher callees. No complete_reading,
allocation contract or replacement inventory is established.

## Alternatives

Treating the state word as changed only by the increment in FND-EXE-230 is
contradicted by the reset, subtractions and final replacement here. Treating
the last subtraction as the helper's final stored value ignores the later
reload and store. A CFG with all targets decoded does not prove finite link
traversal or bound the number of retained stack words.

## How to reproduce

At revision `8d39f59`, use FND-EXE-226's original-source region, hash and
default x86-bounds limits. Set entry and sole entries value to `0x00040687`
(`4AE5:0637`), with no seeds or summaries. Check all 59 covered bytes, the
24 instructions and both returning-call assumptions. Its CFG completion
does not mean termination or a Standard complete reading.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:061F`, count 36, restricting this helper
to the interval above. Run tools/ghidra/ReportSegmentWrites.java at revision
`8d39f59` with names ES and limit 10000, checking the two positive outputs
above. FND-EXE-229 records this scan's totals and opaque-output limitation;
no negative or preservation claim follows. Sources and reports stay in GAME_DIR.
