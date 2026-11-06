---
id: FND-SOUND-062
title: Music-table field candidates decode loader interiors or combat-table data and the exact filename query retains four interiors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:04E2..2834:0508
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00041A70..0x0004281F
tool: window_operands.py and resident_listing.py at e641366b; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-061, FND-COMBAT-027 and FMT-COMBAT-003
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-061 retains eight raw field-window candidates. This
reading classifies their source-byte ownership, without promoting
alternate decodings to established instructions or proving runtime
entry exclusions beyond the known streams.

### Two instruction interiors

Paragraph alias 2882:000F is 2834:04EF, one byte into the
operand-size-prefixed dword comparison at 04EE. Alias 2883:0007
is 2834:04F7, one byte into the prefixed dword PUSH at 04F6.
Decoding after the prefix produces a word-form comparison/push;
the established loader path starts at the prefixes. These do not
supply independent known readers. Changed or alternate entries
remain mechanisms outside that local boundary conclusion.

### Six candidates belong to a recorded data table

FND-COMBAT-027 and FMT-COMBAT-003 identify 113 records of
31 decimal bytes at 4C87:0000..0DAF. Each has 29 name bytes
and a two-byte icon identifier at +1D. The six other candidates
fall within that table as follows; record indexes are zero-based
and written decimal, while offsets are hex.

| Raw candidate | Offset from 4C87:0000 | Record | Offset within record |
|---|---|---|---|
| 4C94:0006 | 00D6 | 6 | 1C |
| 4C9C:0002 | 0152 | 10 | 1C |
| 4CA9:000B | 022B | 17 | 1C |
| 4CAB:000A | 024A | 18 | 1C |
| 4D5B:000F | 0D4F | 109 | 1C |
| 4D5D:000F | 0D6F | 110 | 1D |

The first five decodings begin at the name field's final byte;
the last begins in the icon word. Their instruction operands are
formed from this recorded data representation, including adjacent
record bytes, rather than an established executable stream. The
inventory region 4C87:0000 spans 3504 bytes and records zero
instruction bytes, independently consistent with that classification.
The data-table interpretation has its own direct reader evidence;
an absent function row alone would not prove that bytes are data.

No licensed names or original byte listings are needed to reproduce
this ownership classification. Normalize each candidate to the base
segment, then divide its offset by 31 decimal. It is not a claim
that modified code or an arbitrary alternate transfer can never
enter data; those mechanisms remain outside this census.

### Exact filename-offset query

The earlier 08BF..08C8 window also matched surrounding state and
unrelated displacement values. Querying 08C2..08C3 instead retains
one listed operand, the known loader filename-offset PUSH at
2834:044D, which is the positive control. Four raw candidates
remain, all labeled interior by the inventory decode:
3961:0009, overlay 173 +2558, overlay 190 +2486 and overlay
201 +0A1C. They are not established filename accesses from that
classification alone; containing instructions and alternate-entry
conditions remain to be assessed.

The exact query does not replace the wider query's historical count
of 787, nor does it exclude pointers computed from other constants,
overlapping accesses or another representation of the filename.
These are distinct scopes. Only the exact-offset component now has
four explicit retained interior candidates rather than an unspecified
wide-window set.

## Interpretation

Q-SOUND-011 can remove the eight field-window candidates from its
unclassified-byte component: two are known instruction interiors,
six recorded table data. The remaining exact-name interiors and
excluded computed/aliased/indirect mechanisms remain open. The
known loader/selector contracts do not become complete readings
from candidate classification. No whole queue closure or native
extra reader/writer exclusion is claimed.

## Alternatives

- Missing inventory coverage makes the six candidates unread code:
  existing format and direct-reader evidence identifies their table.
- Prefix-interior decoding adds a known word access: the established
  loader stream includes the prefix and uses the dword operation.
- The exact query resolves all filename uses: four interior candidates
  and computed/alternative representations remain outside that conclusion.

## How to reproduce

Reuse FND-SOUND-061's controlled field-window report. Read the loader
range with resident_listing.py, retaining
music-table-field-interior-controls.txt under
GAME_DIR/analysis/handler-census-20261106/. Normalize the six addresses
and compare the offsets with FND-COMBAT-027/FMT-COMBAT-003; check
coverage/BLD-GOG-EN-1.1/DSUN.EXE.regions.tsv's 4C87:0000 row.
Run window_operands.py with the approved executable, the DSUN inventory
and 08C2..08C3, retaining music-table-exact-name-window.txt. Inventory
LF SHA-256 remains
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
No original game, emulator session or emulated call supplied evidence.
