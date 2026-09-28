---
id: FND-CONFIG-086
title: The remaining global-callback setter calls register local handlers or restore a saved pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:01D8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B9:04FF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:02CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 574B:04D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0BEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5778:00BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 577D:036D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:00BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57B0:12C3..57B0:178E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:03F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C9:06D3
tool: Python 3.14.7 FBOV descriptor and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

FND-CONFIG-084 counts 22 direct calls to overlay 190's `571F:0034`
global-callback setter. FND-CONFIG-083 accounts for one from overlay 171;
FND-CONFIG-084 reads three from overlay 172; FND-CONFIG-085 reads six from
overlay 182. The other twelve direct sites pass these pointers:

| Source overlay | Call file offset | Far pointer passed |
|---|---|---|
| 175 | `0x0005FDB8` | `5689:0052`, an entry in its own overlay |
| 181 | `0x000683AF` | `56B9:0034`, an entry in its own overlay |
| 192 | `0x0007D5CE` | `5736:002F`, an entry in its own overlay |
| 194 | `0x00081601` | `574B:002A`, an entry in its own overlay |
| 199 | `0x000884EE` | resident `28C9:0CFF` (FND-AI-004) |
| 201 | `0x0008987E` | `5778:002A`, an entry in its own overlay |
| 202 | `0x0008A80D` | `577D:002F`, an entry in its own overlay |
| 203 | `0x0008B2FA` | `5782:0043`, an entry in its own overlay |
| 209 | `0x00094423` | `57B0:0048`, an entry in its own overlay |
| 209 | `0x000948EE` | The far pointer stored at `0300:000F` |
| 211 | `0x00096200` | `57C1:005C`, an entry in its own overlay |
| 212 | `0x000988D3` | `57C9:0043`, an entry in its own overlay |

Each immediate segment word above has a declared FBOV fixup to its named
descriptor. At `0x0009440A`, the overlay 209 path reads the previous pointer
from setter companion `571F:0039` and stores it at `0300:000F` before
installing its own handler. The separate routine at `0x000948DB` passes that
stored pointer back to the setter. The same saved-pointer field is also used
by the overlay 172 site described in FND-CONFIG-084.

## Interpretation

All 22 declared direct setter calls have now been located and their immediate
arguments classified. The eleven non-restoring sites here can replace an
earlier global callback when their paths run; the overlay 209 saved-pointer
site can restore a prior value. This inventory does not establish which
paths can run while the stored-character list is active.

## Alternatives

An unrelocated, computed or indirect call to the setter is not excluded by
this direct-fixup inventory. The overlay 209 save and restore sites are in
separate routines; their complete incoming and exit paths have not been
read, so a balanced lifetime is not established. The local handlers' event
behavior and their relation to the shared message routine remain open.

## How to reproduce

Select FBOV fixups targeting descriptor 190 offset `0x0034`, as in
FND-CONFIG-084, and inspect bounded windows around the twelve file offsets
in the table. For each immediate segment word, verify the corresponding
source overlay's fixup resolves to the listed descriptor. Read overlay 209
at `0x00094405..0x0009442B` and `0x000948DB..0x000948F6` for the getter,
saved pointer and later setter call.
