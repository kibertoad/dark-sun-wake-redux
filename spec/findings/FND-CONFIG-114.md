---
id: FND-CONFIG-114
title: Bounded reference inventories do not identify the resident value-64 handler producer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1261
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; MZ relocation and FBOV fixup mapping
environment: null
---

## Observation

Entry 28C9:1261 at file offset `0x0001F0F1` contains the
state-one event-five/value-64 route in FND-CONFIG-128. Targeted
reference inventories produce these bounded negative results:

| Representation searched | Boundary | Result |
|---|---|---|
| MZ relocated offset/segment pair naming raw 18C9:1261 | All declared MZ relocation words, interpreted with the preceding word as offset | No match |
| Aliased MZ relocated pair resolving to file offset `0x0001F0F1` | Same relocation list, converting every pair to a file target | No match |
| Declared FBOV fixup naming descriptor 22 with preceding offset 1261 | All declared overlay fixup lists | No match |
| 16-bit relative near-call candidate landing at `0x0001F0F1` | Complete resident segment `0x0001DE90..0x000217F0` | No candidate |
| Exact two-byte word pattern for offset 1261 | Resident load image from file header's image start through the aligned MZ end | No raw match |

The raw pattern result rules out that exact literal word within that
range, not another segment alias, transformed offset or overlay data.
The aliased relocation query resolves targets using each stored segment
and preceding offset; it does not require a direct-call opcode and can
therefore select other relocated pointer forms. Its zero result still
says nothing about unrelocated or computed pointers. The known resident
call to 57A6:006B in FND-CONFIG-127 remains a separate positive
example of the exact relocated-call representation.

## Interpretation

The inventories do not supply the handler's registration or incoming
producer. FND-CONFIG-128's local argument and branch reading remains
valid, but it cannot yet be connected to an original player action by
these inventories. No absence-of-behavior conclusion follows.

## Alternatives

A computed registration or transformed pointer can enter the handler;
another reading is that this entry has no reachable producer in the
supported build. Neither is established (Q-CONFIG-008). Reading
registration and indirect dispatch state, or a new bounded reference
tool that covers those representations, would distinguish them. A
near jump, other call encoding, different entry into the routine or
unrelocated aliased pointer lies outside the negative inventory.

## How to reproduce

Reuse the approved target and mappings in FND-CONFIG-128. Inspect
all MZ relocation words for the exact pair and separately resolve each
pair to a file target with the MZ header's image start. Select FBOV
fixups with decoded descriptor 22 and preceding offset 1261 using
FMT-EXE-005; do not compare the shifted stored index directly with
22. Search 16-bit relative-call candidates only across the full
resident segment range above. Search the explicit two-byte offset
pattern only through the aligned MZ end, reporting that raw search
separately from verified instruction and pointer inventories. Keep
the lack of a recognized incoming producer in Q-CONFIG-008.
