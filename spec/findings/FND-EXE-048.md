---
id: FND-EXE-048
title: Record setup resolves TLS imports and tail-returns the later error query on a zero result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0060097C..0x00600988
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602570..0x00602576
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006023B0..0x006023B6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602580..0x00602586
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602440..0x00602446
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602590..0x00602596
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035914E..0x00359162
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359222..0x0035922E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359266..0x0035926B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359286..0x00359291
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359296..0x003592A1
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

The five direct thunks used by FND-EXE-045 and FND-EXE-046 jump through
specific PE import slots. Physical descriptor and lookup-thunk reading maps
those slots to these KERNEL32.dll names, including their terminating bytes:

| Thunk | Import slot | Imported name |
|---|---|---|
| `0x00602570` | `0x02431858` | InterlockedIncrement |
| `0x006023B0` | `0x02431894` | Sleep |
| `0x00602580` | `0x024318A0` | TlsGetValue |
| `0x00602440` | `0x02431888` | SetLastError |
| `0x00602590` | `0x024318A4` | TlsSetValue |

Thus FND-EXE-046 passes its saved base-offset-56 address to
InterlockedIncrement and tests the full returned word for zero. Its wait
loop calls Sleep with zero, then rereads the saved completion-flag address.
This identifies the imports; it does not make the loop bounded or establish
concurrent writers, scheduling, or whether an imported call returns normally.

In FND-EXE-045, the nonzero-mode path saves GetLastError's return, passes
its saved offset-44 word to TlsGetValue, then passes the first saved return
to SetLastError. After these calls return normally it stores TlsGetValue's
saved return in the supplied record's first word. It then rereads the shared
pointer and its offset-44 word and calls TlsSetValue with that fresh word and
the supplied record address. The earlier record write precedes this call
and its full-width result test; there is no intervening local rollback.
The two index reads are distinct and need not be equal without a lifetime
or interleaving argument.

The zero-result continuation left unread in FND-EXE-045 begins at
`0x0060097C`. It restores the stack to the saved-register region, restores
three saved registers and the frame pointer, then jumps to `0x00602490`.
FND-EXE-047 resolves that thunk to GetLastError. No new return address is
pushed by this tail jump: on normal external return the later error query
returns to the record-setup caller using the restored caller frame. This
path therefore does not merely return the zero from TlsSetValue or restore
the first saved error value again. The nonzero-result path remains the
ordinary frame-restoration return described by FND-EXE-045.

## Interpretation

The imported call identities and the previously unread zero-result
continuation are now bounded directly. They do not establish a complete
TLS, error-preservation, exception-record or concurrent initialization
contract. Q-EXE-009 retains record cleanup, caller consumption of the
returned word, index ownership and lifetime, shared-storage provenance,
concurrent writers and exceptional effects. No game behavior is implemented.

## Alternatives

Mapping imports by nearby symbol names or call order is unnecessary and
unsafe; the exact slots identify them. A plain return of the tested zero
on the zero-result path is ruled out by the frame restoration and tail jump.
Assuming one cached index is used for both TLS calls is ruled out by the
shared-pointer and index reread. Import names alone do not prove that record
publication succeeds or that waiting eventually finishes.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read one instruction at each
listed thunk and six at `0x0060097C`, restricting claims to the listed ranges.
Map each exact slot through the physical PE descriptors and terminated lookup
thunks rather than analyzer symbol ordering; bound names and include their
terminators. Use the independently checked malloc/free mappings in FND-EXE-024
as controls. Follow the saved returns, record store, fresh index, full-width
result test and restored caller frame using FND-EXE-045 and FND-EXE-046.
Use FND-EXE-047 for the final error-query import. Keep rich reports local and
execute no interpreter or game.
