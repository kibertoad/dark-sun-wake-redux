---
id: FND-ITEM-038
title: Recorded canonical generic setter families and decoded script paths do not select item-name field eleven
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:012A..1AA0:0321
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:295B..172C:299C
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF5E..0x0001FF60
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00020327..0x00020328
tool: script_field_paths.py and direct_callers.py at 7dbec36c; Python 3.12.10; composition with FND-PARTY-234/235/238/248/259, FND-ACTOR-015 and FND-COMBAT-100
environment: null
---

## Observation

Q-PARTY-115 retains producers of an invalid name for type-50 items after
FND-ITEM-037 excludes unchanged canonical first-copy inputs. FND-PARTY-248
maps kind-one field eleven to name word +14 hex with metadata type three.
This reading applies the existing accessor-family evidence to that field
and measures the retained decoded script field-path domain explicitly.

### Conditions and known setter families

Use the canonical field descriptors/types, installed traversal lists and
the recorded direct callers with intact arguments, list/count/stack
storage and near-pointer identity. Intervening callees preserve those
inputs. These conditions exclude neither other caller representations
nor stores through aliased destinations.

The positive encoded direct-call query returns the same fourteen setter
sites recorded in FND-PARTY-238. Its known immediate setter fields are
77,2,10,1,1,77, none eleven. The two type-gated calls require metadata
seven or eight, excluding field eleven's canonical type three. The
descendant graph and admitted transfer-list families select 2,4,5,15,16
or 17; negative transfer fields skip the access. FND-PARTY-218/238 keep
root object indices distinct from descendant field selectors.

FND-PARTY-235's admitted chain writer produces load-consumed header fields
4,16,17,15 or 5, not eleven. The type-one and type-four header words do
not reach the chunk loader's generic setter. FND-ACTOR-015's complete
canonical RDFF chunk-domain table independently gives those same
load-consumed fields. Runtime-written or changed headers need their own
admission; this is not validation of arbitrary CHAR/SAVE inputs.

FND-PARTY-234 resolves the other recorded wrapper/interpreter setter
fields to script field-path words. A zero-length path can instead make
the setter read a preceding argument word, so absence of eleven among
nonempty selectors alone would be insufficient.

### Controlled decoded script-path domain

The retained indexed listing from FND-COMBAT-100 has SHA-256
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
The new bounded census validates canonical indexed instruction records,
path forms and selectors, and keeps paired decoder-error diagnostics.
It counts nested paths in each printed instruction without treating a
nested instruction as another top-level record.

| Measurement | Count |
|---|---:|
| Field paths | 2235 |
| Empty field lists | 0 |
| Omitted field lists | 0 |
| Field-eleven occurrences in any list position | 0 |
| Positive field-15 occurrences | 231 |
| Positive field-77 occurrences | 60 |
| Unique paired decoder gaps | 1 |

The gap is GPL/37, stopped at 0D39 hex and resumed at 0D58 hex. Its
bytes are outside this decoded-domain exclusion. This retains the
FND-COMBAT-100 limitation instead of converting a source listing into
a complete runtime execution census.

All printed paths are nonempty. Under the wrapper/count preservation
conditions, neither a field-eleven list word nor the zero-count fallback
therefore supplies eleven on these unchanged decoded paths. A positive
synthetic target-eleven path, empty and omitted lists, unrelated positive
selectors and malformed-record controls pass. Two original-derived
census runs are byte-identical. A wrong listing hash or missed positive
occurrence count refuses with empty stdout.

## Interpretation

This removes direct field-eleven selection from the recorded fixed/type/
traversal/transfer families, canonical chunk headers and unchanged decoded
script field paths under their stated conditions. It narrows Q-PARTY-115
away from another reading of those same accessor bodies or selectors.

It does not prove that every script or native operation preserves name
memory. Remaining mechanisms include the decoder gap, other entry/target
representations, runtime stream/list/count/metadata changes, direct or
computed field stores, nonfirst data and whole-record copies/restores,
and aliases of other selected fields. A store need not select field
eleven to overlap a name word. Actual invalid-name/type-50 admission and
the other Look gates remain open. No original ran, and no confidence,
parity or complete-reading promotion follows.

## Alternatives

A field-eleven path in the controlled domain would refute the selector
exclusion; the synthetic positive case shows the query reports it. A
zero count would require following the preceding-word fallback; no such
printed path occurs. Neither result excludes the undecoded gap or a
changed runtime path. In particular, absence of a generic field selector
must not be restated as absence of raw pointer or whole-record writes.

## How to reproduce

Reuse the verified listing at
GAME_DIR/analysis/reader-admission-20261029/scripts.txt from FND-COMBAT-100.
Its regeneration procedure uses script_decode.py with approved GPLDATA.GFF
length 2191945, XXH3-128 2bd1d2b989eef2dcb0545feb95975fca and OBJEX.GFF
length 6816516, XXH3-128 e08d3cac153c547f6804ca27b155915d. A regenerated
listing must meet the named identity and controls before reusing this result.

Run script_field_paths.py LISTING CONTROLS at 7dbec36c. Controls name the
listing SHA above, paths 2235, emptyLists 0, omittedLists 0, gaps 1,
targetField 11 and positiveFields {15:231,77:60}. Retained inputs/results
are under GAME_DIR/analysis/item-removal-20261104/: name-field-controls.json,
name-field-domain.json, name-field-repeat.json and the hash/positive-control
refusals. name-setter-callers.txt is the positive direct_callers.py
DSUN.EXE 1AA0:012A query. Reuse the cited caller-family findings rather
than treating its candidate paragraph aliases as established entries.
No original code, listings or source-derived records are committed.
