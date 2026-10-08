---
id: FND-EXE-218
title: Empty effects in the decoded host listing all classify as NOP without admitting initial segment bases
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401368..0x00401369
tool: Ghidra 12.1.3 PUBLIC, controlled local p-code classifier and engine 13.6.0 instruction window
environment: null
---

## Observation

FND-EXE-217 leaves instructions with empty p-code unclassified. Repeating
its DS/SS output query with decoded-mnemonic grouping finds one empty-effect
class, NOP, comprising all 472 empty-effect instructions. Its first site is
`0x00401368`; an independent one-instruction window verifies a one-byte
decoded NOP ending exclusively at `0x00401369`. This representative verifies
the emitted site's class and span, not every byte of the listing.

The scan still contains 369,192 decoded instructions, no matching DS/SS
outputs and 24 opaque sites. Match, opaque-site and empty-class outputs all
finish without truncation. Every empty effect is counted in the grouped
class totals; these totals do not classify undecoded or overlapping streams.
The opaque instructions retain FND-EXE-217's separate classification and
effect limits.

Independent synthetic controls show that the analyzer gives both NOP and
WAIT empty p-code, while preserving their distinct decoded classes. A
one-class output limit reports truncation and retains the complete class
count. The full-limit control reports both classes and their representative
addresses. Thus an empty effect list alone does not identify a NOP or prove
that the instruction has no relevant native behavior.

## Interpretation

The empty-effect classification gap in this saved decoded listing is now
bounded: its empty effects all carry the NOP label. This does not establish
the loaded process's initial DS/SS bases or their preservation across code
outside the listing. Unsearched streams, generated instructions, external
libraries and exceptional paths remain outside this result. No native
execution, descriptor inspection or complete_reading declaration occurs.

FND-EXE-198's stack/pointer storage identity therefore remains conditional.
An initial host descriptor state and preservation evidence for the admitted
call route are required to settle Q-EXE-011; a generic external ABI sample
does not supply them. The current runtime policy supplies neither native
register/descriptor inspection nor a host-PE emulated-call capability.
Other reader dependencies remain Q-EXE-012 and Q-EXE-013.

## Alternatives

Calling every empty effect a NOP without examining its decoded class is
ruled out by the synthetic WAIT control. Conversely, treating the listed
NOP class as an explicit unresolved segment assignment loses its decoded
classification. Treating this classification as proof of equal descriptor
bases or complete native preservation is unsupported.

## How to reproduce

Verify FND-EXE-011's source size 3,802,624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Reuse its saved PE project read-only,
analysis disabled. Run tools/ghidra/ReportSegmentWrites.java from commit
fc0cf29 with arguments DS+SS and 256. The domain is all decoded instructions
in the saved listing, independent of function boundaries. Check completion,
each truncation field and the sum of empty-class counts against the total
empty effects. Run ReportInstructionWindow at `0x00401368`, count one,
as the separately bounded representative check. Run the committed synthetic
Test-SegmentWrites.ps1 only in its admitted disposable fixture project,
never on the licensed source. Keep reports local and execute no interpreter
or game. Source mappings and decoded labels do not admit native descriptors.
