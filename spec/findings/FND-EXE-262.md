---
id: FND-EXE-262
title: Resident loader initial and current link slots start at zero rather than a relocated segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B002..0x0004B004
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B00C..0x0004B00E
tool: executable-reader 2.5.0
environment: null
---

## Observation

At FND-EXE-176's initial state source base, shipped offset `0x0004AEE0`,
the unsigned words at relative offsets `0x0122` and `0x012C` both contain
zero. They correspond to shipped intervals `0x0004B002..0x0004B004`
and `0x0004B00C..0x0004B00E`. The initial-link word at `0x0122` has
no declared relocation or fixup in the source operand reader's MZ result;
that query reports an unresolved target, not a relocated segment.

FND-EXE-246 loads current DS-relative word `0x0122` into CX and
publishes it to word `0x012C` before processing the linked headers.
The shipped values therefore do not independently supply a nonzero
native header segment to that consumer. Their live values require
producer and effective-segment admission. This reading does not prove
that either slot remains zero, that the consumer runs with these initial
values, or that zero is a valid link for the later header accesses.

## Interpretation

This supplies the missing initial-file facts for the two link slots and
rules out deriving a nonzero initial link by applying an assumed MZ
relocation to the first word. It does not close the live producer question.
The initial state segment is a source correspondence, not proof of every
consumer's effective DS or of preserved initial storage.

Q-EXE-001 and Q-EXE-010 retain all live writers, including indexed and
bulk writes, other code regions, storage aliases and loader lifecycle
ordering. No native absence-of-writer claim, complete_reading, code range
or replacement inventory is established by these data locations.

## Alternatives

Treating the initial-link slot as a shipped relocated nonzero segment is
contradicted by its zero value and unresolved relocation result. Equating
that default with the live consumer input ignores intervening writers and
segment identity. FND-EXE-246's publication supplies a current-link writer,
not the initial-link producer it reads.

## How to reproduce

At revision `3a9947a`, run the committed table reader against the installed
DSUN.EXE identity in FND-EXE-236, sourceKind mz. Use start `0x0004AEE0`,
count one, limit one and stride `0x012E`, with countEvidence identifying
one initial state record and only explicitly accessed fields. Read two
unsigned width-two fields: initialLinkWord at relative `0x122` and
currentLinkWord at `0x12C`. Check both raw values are zero.

Independently query operand at shipped site `0x0004B002`, targetOffset
zero, sourceKind mz and the same source hash. Check raw zero, relocated
false and the unresolved-target result. Compare the consumer in
FND-EXE-246 without assuming its live DS or incoming word. These queries
make no negative writer-search claim. Keep source, configurations and
reports in GAME_DIR.
