---
id: FND-EXE-266
title: Shipped default filename input terminates within the bounded tail copy
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
    offset: 0x0004B7A8..0x0004B7B3
tool: Python 3 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-265's conditional default pointer has offset `0x0728` in the
relocated segment whose shipped base is `0x0004B080`. Its initial shipped
input therefore starts at `0x0004B7A8`. The first ten bytes are nonzero,
and the following byte is zero. No two-byte MZ relocation site intersects
this eleven-byte interval. This records the terminator and extent without
retaining the original text.

Under this initial input and the admitted forward-copy contract of
FND-EXE-264, the tail copies ten nonzero bytes and that zero, returning
from its loop before exhausting its twelve-byte source counter. It does
not take the separate appended-zero path. In FND-EXE-265's direct helper,
with admitted ES:DI at the state buffer base, these eleven writes occupy
offsets `0x008C..0x0097`, exclusive end. They do not reach offset `0x0122`.

## Interpretation

This closes the initial-data length subcase of Q-EXE-010's filename input
obligations. It does not establish live preservation of the pointed-to
bytes, the default-pointer branch's native admission, every other pointer,
or an input bound for either preceding bulk-copy path. For those paths,
the same eleven tail writes begin at their resulting DI rather than the
buffer base, and earlier writes still need their own bound. Q-EXE-001
and Q-EXE-010 retain those obligations. No complete_reading or inventory
replacement follows.

## Alternatives

The maximum thirteen-byte tail output is a correct general bound but is
not the output count for this initial default input. Applying this
eleven-byte direct-path extent to all paths would ignore both their
destination formation and writes executed before the tail. Absence of an
intersecting MZ relocation does not exclude runtime writes to the input.

## How to reproduce

At revision `d14c92c`, read the installed source identity in FND-EXE-236
with Python and verify its XXH3-128 before examining data. Inspect only
the thirteen-byte window starting at shipped offset `0x0004B7A8` and
report the index of its first zero, not its text: ten. Check all preceding
bytes are nonzero. Derive the eleven-byte input interval including zero.

Read the MZ relocation count from header offset six, relocation-table
offset from header offset `0x18`, and header paragraph count from offset
eight. For every relocation entry, read its little-endian offset and
segment words; its shipped two-byte site starts at header-size plus
sixteen times segment plus offset. Count sites intersecting the half-open
interval `0x0004B7A8..0x0004B7B3`: zero. This checks loader relocation
overlap only. Follow FND-EXE-264's copy-before-zero-test ordering and
FND-EXE-265's direct DI initialization to derive the destination interval.
Original input and local measurement output stay in GAME_DIR.
