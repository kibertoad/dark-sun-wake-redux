---
id: FND-CONFIG-079
title: The resident event dispatcher calls a loaded window's callback at offset 0xF5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:071F..39D1:0854
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly of the resident MZ image
environment: null
---

## Observation

The resident routine at `39D1:071F` copies a 24-byte event record and
passes it to `39D1:0752`. The latter first handles an event whose first
word is 3 through the global callback pointer, and returns `0xFFFF` for
one whose first word is 1. For other event words, it tests the registered
window count at `DS:A103`. When that count is nonzero, it copies the far
window pointer at `DS:A0FD`, tests that pointer and the far pointer at
window offset `0xF5`, then calls the latter with a copy of the event record.
It returns the callback's nonzero result after local cleanup; a zero result
falls through. With no current window or no `0xF5` callback, it clears
`DS:A0FD` and may call the global callback pointer at `DS:A0F1`.
FND-CONFIG-083 shows that the list-opening routine installs the same
callback function there as in its window.

The loop beginning at `39D1:0854` obtains an event through resident
`4464:02FE`, passes it through a local event preparation routine, and
calls `39D1:071F` for events that pass the loop's initial filter.
FND-CONFIG-078 establishes that the stored-character list window receives
overlay 171's `5664:002F` as its `0xF5` callback when window setup succeeds.

## Interpretation

The stored-character list callback has indirect incoming routes through
the resident event dispatcher. It can be invoked for a prepared event with
a first word other than 1 or 3 while the window is current, or through the
global fallback while that registration remains active (FND-CONFIG-083).
The callback's message branch still has its own conditions.

## Alternatives

The event's player-visible source, the complete set of event words passed
by the loop, and whether the list window is current when a particular event
arrives remain unread. This route does not prove that the callback's message
or the shared message wait occurs during ordinary play.

## How to reproduce

Map resident segment `39D1` to file base `0x0002EF10`. Disassemble
`0x0002F62F..0x0002F763` and `0x0002F764..0x0002F822`. Follow the
copy of the 24-byte record, the tests of its first word, `DS:A103`,
`DS:A0FD` and window offset `0xF5`, and the indirect far call at file
offset `0x0002F712`. Compare the callback installed in FND-CONFIG-078.
