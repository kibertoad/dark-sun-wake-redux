---
id: FND-EXE-260
title: Initial transfer-request words retain nonzero adjacent bytes that partial stores do not initialize
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
    offset: 0x0004AF23..0x0004AF29
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF3D..0x0004AF3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF41..0x0004AF43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF45..0x0004AF47
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AF49..0x0004AF4B
tool: executable-reader 2.5.0
environment: null
---

## Observation

At FND-EXE-176's initial state source base, shipped offset `0x0004AEE0`,
selected unsigned words have these raw values. Relative offsets refer to
that initial record, not a proved live DS binding.

| Relative word offset | Initial value |
| --- | --- |
| `0x0043` | `0x0000` |
| `0x0045` | `0x0000` |
| `0x0047` | `0x0000` |
| `0x005D` | `0xFFFF` |
| `0x0061` | `0x9300` |
| `0x0065` | `0xFFFF` |
| `0x0069` | `0x9300` |

The pointer pair consumed in FND-EXE-180 and the transfer-route word
consumed in FND-EXE-259 are initially zero in this source record. These
values do not exclude later nonzero publications or establish which
request route runs. The words at offsets `0x0061` and `0x0069` each
contain a low zero byte and an adjacent `0x93` byte in little-endian order.
Those adjacent bytes are not initially zero.

FND-EXE-259's direct request path explicitly writes only DL to relative
byte `0x0061` and CL to relative byte `0x0069`, beside full-word writes
at `0x005F` and `0x0067`. Those individual byte stores do not initialize
the next byte. With this initial record still supplying the destination
and no other writer, the adjacent bytes would retain `0x93`, rather than
becoming zero through the byte store. Neither that premise nor a whole-word
interpretation of those positions is established for the native request.
The two other selected words begin at `0xFFFF`; their semantic role and
preservation require the external reader's contract and all live writers.

## Interpretation

This supplies missing initial-file facts for the request structure without
turning defaults into preservation or output evidence. A partial store's
untouched neighbor has a specific initial value, but proving the same
value at an external request still needs effective segment identity,
intervening effects and every relevant writer. No native layout is inferred
from familiar-looking constants or adjacency.

Q-EXE-001 and Q-EXE-010 retain the request's external consumer contract,
all field/overlapping writers, live pointer and handle producers, state and
saved-stack aliases and actual initialized output extent. No complete_reading,
code range or replacement inventory is established by these data locations.

## Alternatives

Treating the adjacent request bytes as initially zero is contradicted by
the two raw words `0x9300`. Treating the shipped zero pointer and handle
as permanent ignores the explicit later writers in FND-EXE-180,
FND-EXE-255 and FND-EXE-257. Naming a native descriptor layout solely
from these constants remains unsupported.

## How to reproduce

At revision `e362365`, run the committed table reader against the installed
DSUN.EXE identity in FND-EXE-236, sourceKind mz. Use start `0x0004AEE0`,
count one, limit one and stride `0x006C`, with countEvidence specifying
one initial state record and selected request defaults only. Read unsigned
width-two fields at relative offsets `0x43`, `0x45`, `0x47`, `0x5D`,
`0x61`, `0x65` and `0x69`, named word43, word45, word47, word5D,
word61, word65 and word69 respectively. Check the seven values above;
there is no native-state observation or negative writer search in this query.

Compare FND-EXE-259's partial store locations independently, retaining
all intervening-writer and effective-binding obligations. The source and
query report remain in GAME_DIR; no request bytes or proprietary export
are committed.
