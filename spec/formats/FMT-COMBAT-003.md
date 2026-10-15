---
id: FMT-COMBAT-003
title: Effect name record in DSUN.EXE
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE"]
byte_order: little
size: 31
text: false
definition: fmt_combat_003.ksy
evidence: [FND-COMBAT-022, FND-COMBAT-027, FND-COMBAT-029, FND-COMBAT-030]
conflicting: []
split_with: []
related: [RULE-COMBAT-009]
---

## Layout

One of the 113 records of the resident data of `DSUN.EXE` at `4C87:0000` (file offset `0x41A70`)
in BLD-GOG-EN-1.1, indexed by effect number from 0 [FND-COMBAT-027]. FND-COMBAT-027 lists every
record's name and word.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 29 | `char[29]` | `name` | The effect's name, ASCII, ending at the first NUL and padded with NULs. Empty in records 12 and 88. The status panel's third line shows it. | supported | FND-COMBAT-022, FND-COMBAT-027 |
| `0x1D` | 2 | `UINT16LE` | `icon_resource_id` | ICON resource number, zero-extended for the resource request. Zero excludes the effect from the reviewed icon list. Values are 0, or from `0x2B68` to `0x5313`. | supported | FND-COMBAT-027, FND-COMBAT-029 |
| `0x1F` | | | | Total size 31 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 113 records of the installed `DSUN.EXE` of BLD-GOG-EN-1.1 were read with this layout; every
name is printable ASCII followed only by NULs [FND-COMBAT-027].

The 107 nonzero icon references name 97 distinct ICON resources, all present
in the supported installed RESOURCE.GFF. Their 100 frames are all 16 by 16;
ICON 16102 has four frames and the other 96 matched resources have one.
Six records have zero references, and eight nonzero numbers are shared by
multiple effects [FND-COMBAT-030]. This verifies the file references and
metadata, without establishing native acquisition or display success.

## Open questions

- Complete ICON request/archive mapping and other field consumers remain
  Q-COMBAT-010; the local request proves the field's role without proving
  that every nonzero ID produces a rendered icon. FND-COMBAT-030 settles
  reference presence and frame metadata in the installed archive; active
  archive state, reader-error/pointer divergence and display selection remain.
  FND-COMBAT-038 supplies the consumer's frame-zero request argument,
  distinct zero-icon/null-pointer continuations and draw/release ordering.
  Valid reader outcomes, backend presentation and later cache interaction
  remain Q-COMBAT-010.
  FND-COMBAT-039 supplies the later cache label/action branches and
  their count/ID/key distinction; native graph/event admission and
  synchronized cache state remain part of that item.
