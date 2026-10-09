---
id: FND-EXE-380
title: Sound utility shipped selector-state initializer values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D498..0x0001D49A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D30C..0x0001D30D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D320..0x0001D321
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D334..0x0001D335
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D348..0x0001D349
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D35C..0x0001D35D
tool: Python 3.14.7 and xxhash 4.0.1
environment: null
---

## Observation

The shipped SOUND_DS.EXE has length 204593 and XXH3-128
236c2dc23c071eca421eb5b427caee57, matching FND-EXE-350's identity.
Its little-endian word at shipped offset 0x0001D498 is twenty.
With MZ header size 0x1400 and modeled load segment 0x1000,
this is the source storage for 1E36:DD38. FND-CONFIG-213 supplies the
utility's data-segment convention; FND-EXE-370 identifies the selector's
DS-relative read of that offset. This source correspondence does not
establish the actual segment or value at the selector's invocation.

For the same mapping, the first five positions at record offset plus four,
starting from 1E36:DBA8 with FND-EXE-370's twenty-byte stride, contain:

| Record index | Shipped offset | Initial byte |
| --- | --- | --- |
| 0 | 0x0001D30C | 0 |
| 1 | 0x0001D320 | 1 |
| 2 | 0x0001D334 | 2 |
| 3 | 0x0001D348 | 3 |
| 4 | 0x0001D35C | 4 |

Only these five byte positions and the separate limit word were inspected
for this finding. It does not inventory all records or their other fields.

## Interpretation

The shipped initializer supplies a concrete candidate for the selector's
limit, rather than an arbitrary unknown source word. Its first five tested
record bytes are nonnegative when interpreted as signed bytes. Those are
source values only: startup, aliases, later writes and actual DS admission
must still be followed before using them as the selector's incoming state.
No operating-system handle identity, admitted table extent or native result
is inferred from the sequence zero through four.

Q-EXE-007 remains open. FMT-EXE-006's status is unchanged, and this
bounded data reading supplies no complete_reading or execution exclusion.

## Alternatives

An all-negative shipped initializer for these first five selector bytes is
ruled out by their values. A negative value produced later remains possible.
Treating the limit word twenty as a proven runtime capacity would conflate
shipped storage with admitted state and would not justify the selector's
further read after its comparison stops (FND-EXE-370).

## How to reproduce

Use the unchanged installed utility with the length and XXH3-128 above.
Read the two bytes at shipped interval 0x0001D498..0x0001D49A and
decode an unsigned little-endian word. Read one byte at each of
0x0001D30C, 0x0001D320, 0x0001D334, 0x0001D348 and 0x0001D35C.
Python's standard-library Path.read_bytes and struct.unpack_from with
format '<H' suffice; the committed XXH3 reporter verifies source identity.
For each source correspondence compute segment times sixteen plus offset,
minus modeled load base 0x10000, plus header size 0x1400.
All location ends are exclusive. No original process, DOSBox or emulated
call runs, and source bytes and local reports remain outside Git.
