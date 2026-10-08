---
id: FND-EXE-164
title: First transfer resets its candidate before register-input selection and retains only full-seven admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600C36..0x00600C57
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AD0..0x00600B44
tool: Ghidra 12.1.3 PUBLIC composed caller and register-input selector reading under Temurin 25.0.4.1
environment: null
---

## Observation

FND-EXE-163 grounds the first transfer's full-six callback admission.
Its continuation reads current local minus 20, writes zero to input P
at offset twelve, writes that current candidate to P offset sixteen,
and replaces local minus 20 from the distinct local minus 16.
It then supplies P in EAX and the address of local minus 20 in EDX
to `0x00600AD0`. The register-input mapping is grounded by the
callee's entry copies in FND-EXE-053, rather than inferred stack slots.

The first selector candidate is therefore the word loaded through the
reset local, not necessarily the candidate just published to P offset
sixteen. Both locals began equal in FND-EXE-163, but its earlier
callback received the address of minus 20 and could change it.
No local instruction copies the changed candidate into minus 16
before this reset. Callee aliases that also change minus 16 remain
conditional; the two local addresses are distinct storage.

FND-EXE-053 grounds the selector's iteration. A zero candidate returns
full two before its saved-match abort test. For a nonzero candidate,
the match is against the already published P-offset-sixteen word.
Its full offset-24 target is loaded before the match overwrites AL.
When called, that target receives two prepared copies of a candidate
word whose low byte has become the equality result, in addition to
the local address, P and the other prepared words in FND-EXE-053.
The retained pre-callback match is zero or four. It is not recomputed
from later callback changes to the local or P field.

Full callback seven returns seven immediately and leaves any mutation
of the candidate local available to the first-transfer caller. Eight
instead tests the saved match: nonzero reaches FND-EXE-041's abort
import boundary; zero freshly reads the mutable local, follows its
first-word link, stores that link through the local and repeats.
The zero-target path uses the same saved-match/advance decision.
Other callback values return full two. A zero candidate also returns
two, even when P offset sixteen is zero and its match was nonzero.
Callback and imported-library outcomes remain conditional.

Back in the first-transfer caller, full two is not changed to three:
its non-seven edge jumps directly to frame restoration while retaining
EAX. Full seven alone reaches the fresh-mode publication and saved-state
transfer in FND-EXE-163. That route later reloads the candidate local
again. It need not use either the pre-selector reset value or the
candidate published to P offset sixteen. The selector does not locally
prove the frame/target/stack fields at candidate offsets 32, 36 and 40.
This linkage cannot establish their initialization or the indirect
callback's argument consumption.

## Interpretation

The first transfer composes two distinct selection stages. The first
stage's candidate publication is followed by a reset before the second
stage, whose seven admission can expose another callback mutation.
An ordinary selector return here is locally two or seven; two returns
through the first-transfer caller, while seven admits its state transfer.
This is not proof that either ordinary return is reached for native
inputs, nor a complete callback, alias, cycle or saved-state contract.
Q-EXE-009 remains open for those dependencies and earlier callee effects.

## Alternatives

- If the first callback changes minus 20 from A to B while minus 16
  remains A and returns six, P offset sixteen receives B, but the
  register-input selector begins at A after the reset.
- If that selector's callback changes the addressed local to C and
  returns seven, the first-transfer state route can later read C.
  It does not necessarily transfer through A or B.
- A callback result `0x00000107` is not full seven: this selector
  returns two and the first-transfer caller returns that two.
- A zero initial candidate returns two before the abort test even if
  P offset sixteen is zero. A nonzero matching candidate with no
  target instead reaches the saved-match abort boundary.

## How to reproduce

Use FND-EXE-011's executable identity and FND-EXE-099's six physical
mapping controls. FND-EXE-163 records the caller and FND-EXE-053 the
selector. Recheck the saved Ghidra program with -noanalysis using
ReportInstructionWindow.java at `0x00600C36` limit twelve and
`0x00600AD0` limit 65, excluding instructions at or beyond the
respective location ends. Track both distinct local addresses, ordered
P publications and local reset, EAX/EDX input copies, the saved match
and low-byte overwrite, zero-candidate priority, full seven/eight tests,
mutable-local advancement and the caller's direct non-seven return.
Check the Alternatives as conditional dataflow/width controls, not
admitted native states. No complete caller or runtime writer search,
native run or emulated execution is part of this finding.
