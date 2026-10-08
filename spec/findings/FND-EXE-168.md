---
id: FND-EXE-168
title: Explicit shared-base publications select allocated or decoded storage without a local stack-disjointness test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600784..0x006007AD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006007AD..0x006007D7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006007D7..0x006007EE
tool: Ghidra 12.1.3 PUBLIC reference, instruction-text and bounded instruction-window reporters
environment: null
---

## Observation

Queries for the three shared cells `0x0242F640`, `0x0242F630` and
`0x0242F650` find explicit stores at `0x006007BC`, `0x006007C2` and
`0x006007CA`, respectively. A separate enumeration of all decoded
instructions, matching each literal cell address without selecting by
function boundaries, recovers the same three stores. Operand direction
and width, rather than the reference-manager type alone, identify these
as four-byte stores. The other displayed literal-address instructions
are loads; no additional explicit store appears in these queried domains.
Both searches complete without reaching their output caps.

The positive controls are the second and third publication cells, whose
independently read stores are already recorded by FND-EXE-043. These checks
cover decoded operands carrying these exact addresses. They do not exclude
indexed or computed destinations, stores through pointers, undecoded code,
runtime-generated code, relocated runtime references, or external writers.
No unique writer over all possible reference kinds is claimed.

At the publication join, the selected base is held in a saved register.
The body forms base plus four, stores the full base to `0x0242F640`,
stores the formed value to `0x0242F630`, forms base plus eight and stores
that value to `0x0242F650`, in that order. Both additions have 32-bit
effective-address width. No intervening call, selected-base null test,
allocation-range test or comparison with a caller's stack appears in
this local publication sequence. It then restores its frame and returns.
These are successive stores, not an atomic publication of three cells.

FND-EXE-043's verified allocation-and-registration route retains its
saved allocation only after the masked nonzero identifier is passed to
FND-EXE-042's reader and the reader's full returned word compares equal
to that saved allocation. The comparison path restores the masked
identifier into a separate register, uses it as the admission flag and
joins publication on the nonzero equality route. A returned record is
not copied over the saved allocation before that comparison.

The fallback route first calls the verified release thunk with the saved
allocation, calls the atom lookup with its original lookup buffer, masks
the low sixteen bits and calls the reader without another local zero
guard. After normal return it removes its pending outgoing bytes, copies
the full reader result into the selected-base register and jumps to the
same publication join. The existing-record route also copies that reader's
full return into the selected-base register and falls through to the join.
The local join itself cannot tell which storage origin supplied that word.

FND-EXE-042 reads the leading four-byte value through its decoded address
and accepts equality to 60 on its ordinary return. That is a record-content
test, not an allocation-origin or non-stack test. Neither that test nor
publication of base-plus-four and base-plus-eight establishes disjointness
from FND-EXE-167's incoming callback slot. Existing-record input, fallback
lookup, decoded-name admission, storage lifetime and segment identity
therefore remain distinct obligations. The fresh allocation route needs
its allocator/storage contract too; this search does not supply it.

## Interpretation

The explicit decoded publication family is narrowed and its selected-base
sources remain separate. A claim that every published base is the current
initializer's fresh allocation is contradicted by the existing-record
and fallback paths. A claim that the leading value 60 proves heap ownership
or caller-stack separation is not supported by the local predicate.
Q-EXE-009 retains computed and indirect writers, external input contracts,
decoded-record origins, allocator lifetime, aliases and segment admission.
No complete-reading declaration or actual overlapping-store outcome follows.

## Alternatives

Publishing the reader's return before the allocation verification comparison,
retaining the allocation after the fallback release, or individually testing
each derived pointer for validity at the publication join is ruled out by
the bounded instructions. Treating all explicit references as writers
without checking operand direction is also ruled out. Unsearched indirect
writers remain possible; an empty additional-store result in the decoded
literal domain cannot exclude them.

## How to reproduce

Use FND-EXE-011's verified shipped PE identity and preferred base. Open the
saved snapshot read-only with automatic analysis disabled. Query
ReportReferences for `0x0242F640`, `0x0242F630` and `0x0242F650`, cap 200
per target. Independently run ReportInstructionText with tokens `0242f640`,
`0242f630` and `0242f650`, combined cap 256. Check the known publication
controls from FND-EXE-043 and classify each displayed instruction by its
actual destination operand; keep the excluded reference kinds explicit.

Read fifty-five instructions from `0x00600734`, retaining only the three
cited spans. Follow the saved allocation, reader return, masked identifier,
comparison and admission flag, release before fallback lookup, full-word
selected-base writes, pending outgoing cleanup and successive global stores.
Use FND-EXE-042 and FND-EXE-043 for the other initializer and reader branches,
and FND-EXE-167 for the distinct callback-frame preservation question.
Keep rich reports local and execute no original program.
