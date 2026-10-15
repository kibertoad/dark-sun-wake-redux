---
id: FND-COMBAT-030
title: Every nonzero effect icon reference matches the installed resource archive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00041A70..0x0004281F
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AC6F..0x0008AE70
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00000000..0x005759FD
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0005BB83..0x0005BD25
tool: tools/evidence/report.mjs table; approved DarkSunWakeRedux.Inspect gff and image-catalog commands; resident_listing.py; source fingerprint validation and metadata-only joins
environment: null
---

## Observation

The supported DSUN.EXE effect table has 113 records of 31 bytes.
Reading only the word at offset 29 of each record gives six zero
references and 107 nonzero references. The latter name 97 distinct
ICON numbers. The zero-reference effect IDs are 12, 24, 88, 93, 97
and 111. FND-COMBAT-029 establishes that zero excludes a record from
the reviewed icon-request path rather than requesting ICON zero.

The installed RESOURCE.GFF was independently checked against its build
manifest: 5724669 bytes and XXH3-128
92fe031616f36062f99484b0f728210e. Its directory contains 400 ICON
resources. Every one of the 97 distinct nonzero references is present;
none is an unresolved number in this installed archive.

The approved image catalog reads those resources' frame metadata through
the existing bounded image reader. The 97 matched resources contain
100 frames in total, all 16 by 16. Ninety-six resources have one frame.
ICON 16102 alone has four frames, all 16 by 16; effect record zero
references it. That resource occupies file offsets 5BB83..5BD25.
The catalog run succeeded for all image entries it visited, but this
finding claims only metadata and reference agreement for the selected
effect icons, not native pixels or an independently confirmed decoder.

Different effect records can share the same ICON number:

| ICON number | Effect record IDs |
|---|---|
| 11112 | 66, 83 |
| 11114 | 30, 65 |
| 11122 | 82, 105 |
| 19116 | 15, 31, 99 |
| 21029 | 42, 45, 51 |
| 21036 | 52, 91 |
| 21080 | 7, 102 |
| 21179 | 1, 71 |

These are all eight repeated nonzero-number groups in the table.
The remaining nonzero references each occur in a single effect record.
Sharing an icon does not merge the corresponding effects or their
names, nor prove that those effect records all occur in one live state.

### Request path and remaining runtime conditions

The exported effect-icon consumer reviewed by FND-COMBAT-029 excludes
the zero words, zero-extends a nonzero word and requests the ICON tag
through 38FF:04AB. Its local output far pointer starts at zero. After
the request it tests the destination pointer rather than the returned
AX status, skipping later processing when that pointer remains zero.

The complete local wrapper forwards tag, number, start zero, full-length
sentinel FFFFFFFF and the supplied pointer-output address to 38FF:07B1.
FND-CONFIG-151 reads this wrapper and the reader's complete-transfer
branches; FND-CONFIG-037..040 describe the stateful archive list and its
startup registration. These are shared reader contracts, not proof that
the selected live archive is the installed RESOURCE.GFF in every icon
call or that allocation/seek/read succeeds.

In particular, the reader can assign an allocated pointer before a later
I/O error (FND-CONFIG-151). A nonzero pointer tested by the effect caller
is therefore not interchangeable with a zero reader error return. The
corpus match does not settle the reached failure continuation or the
validity of the buffer supplied to later display calls.

## Interpretation

This answers Q-COMBAT-010's shipped reference-existence and frame-metadata
part for the supported installed archive. It removes a missing-resource
explanation for these 107 references in that file; it does not establish
native acquisition or display success. The local zero-word exclusion,
failed/null acquisition and later pointer use remain distinct conditions.

The four-frame resource must retain its frame list. This metadata alone
does not say which frame the effect caller selects or that it animates.
No format status or parity promotion is made. Runtime route, other
consumers, reader-error/pointer divergence, buffer validity and display
selection remain Q-COMBAT-010.

## Alternatives

- A nonzero word is not a unique effect identifier: eight ICON numbers
  are shared, including two numbers referenced by three effects each.
- Zero icon words are deliberate local exclusions, not absent resources
  discovered by this corpus join.
- Present and readable on disk does not imply a successful original
  runtime request. Current archive state and external I/O remain inputs.
- Image dimensions and frame count do not establish colors, transparency,
  chosen frame, animation or screen placement. No original was executed
  and no asset payload, pixel fixture or report was committed.

## How to reproduce

Use the approved installed files and the project's pinned tools. Read
the executable with report.mjs table using its build XXH3, start 41A70
hexadecimal, count/limit 113, stride 31, and one width-two field at
offset 29. Count evidence is FMT-COMBAT-003 and FND-COMBAT-027.
Verify RESOURCE.GFF with its size/hash above before cataloging it.

Build the existing tools with tools/Test.ps1, then use
dotnet run --project tools/DarkSunWakeRedux.Inspect -- gff followed by
the owned RESOURCE.GFF path, and its image-catalog command on that same
path. Retain the JSON only in GAME_DIR/analysis/combat-effects/.
Join nonzero table words against catalog entries whose tag is ICON;
check every distinct ID, sum frame counts and compare every selected
frame's width and height. Group the original table indices by number
to reproduce the zero set and shared-number table above. Use the gff
directory's offset and size to locate ICON 16102.

Read 38FF:04AB..04D5 with resident_listing.py. Compare the ICON caller's
pointer initialization/test with FND-COMBAT-029, and retain the reader's
error-versus-pointer ordering in FND-CONFIG-151. No prepared game state,
native run or emulated call is required for this corpus result.
