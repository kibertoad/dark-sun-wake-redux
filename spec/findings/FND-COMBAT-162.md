---
id: FND-COMBAT-162
title: Canonical OBJEX definitions and temporary-slot overrides keep the flags-zero image selector below the lazy-animation gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0EFF..31E0:128C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2670..31E0:271E
tool: tools/research/exec-census/resident_listing.py and tools/evidence/xxh3.mjs at df4c62bb; Python 3.12.10; Capstone 5.0.7; existing FND-ACTOR-001 data census
environment: null
---

## Observation

FND-COMBAT-161 bounds a temporary-slot lifetime when the image argument
is unsigned at most 8000 hexadecimal. This reading binds that argument
to the canonical source and the loader's own overrides. It reuses
FND-ACTOR-001's complete OJFF census rather than repeating it.

The licensed local OBJEX.GFF still has length 6816516 and XXH3-128
e08d3cac153c547f6804ca27b155915d, matching FND-ACTOR-001. That finding
records all 4479 OJFF resources as sixteen bytes, with word +0C from
1 through 3953 and word +0E zero in every resource. This fingerprint
check does not independently repeat its field census or prove an actual
runtime request selected this archive.

### Local word flow and temporary-slot overrides

The complete loader's OJFF destination buffer begins at SS:BP-24 hex.
Word +0C is BP-18 and word +0E is BP-16. The later +1155 push passes
BP-16 to 31E0:2670 as its image argument. The two words are distinct:
the selector uses the local +0E word after preprocessing.

Requested slot FFFF becomes 319 before these tests. Seven object numbers
can override local +0C, as already described in FND-ACTOR-003: the values
are 11001, 11002, 11005, 11007, 11006 and 11008, with two object numbers
sharing 11005. The two party-mode overrides require signed slot below
four and are skipped for temporary slot 319.

At +0FD9 the loader compares the object number with 1339. Other numbers
copy local +0C to +0E; 1339 skips that copy. For 1339, the override 11008
is therefore in +0C while the canonical source's zero +0E remains the
selector. The other temporary-slot overrides are at most 11007. A
non-overridden canonical record contributes its +0C value, at most 3953.

Under the intact flags/local/nonaliasing and successful-allocation
conditions of FND-COMBAT-161, no intervening selected call rewrites that
local selector. Its possible values are therefore zero, a canonical image
word or one of the temporary-slot overrides at most 11007 decimal. All
are below 8000 hexadecimal.

With flags zero, 31E0:2670 writes a negative signed presentation image
word: zero becomes 8000, and a positive selector becomes its word-width
negation. The parent requires a nonnegative signed image word before
calling 31E0:271E. It consequently skips that lazy-animation branch for
this canonical-source temporary-slot case.

## Interpretation

This removes the high-selector alternative for a successfully supplied,
intact canonical OBJEX record in FND-COMBAT-161's admitted continuation.
It also resolves the loader's local use of OJFF +0E as the preprocessed
image argument, without assigning every possible consumer or flag mode
the same semantics.

It does not bind FND-COMBAT-150's OJFF provider handle to OBJEX.GFF,
establish all live resource lengths/providers, or admit changed resources,
partial transfers, local aliases, RDFF-enabled loading or eviction. The
party override 13009 is outside this temporary-slot path. Other incoming
slots/flags and the reason for preserving zero in the named exception
remain open. No native run, universal save-admission claim or confidence
promotion follows.

## Alternatives

Using the original +0E value alone would incorrectly predict zero for
most records; using +0C alone would incorrectly predict selector 11008
for object 1339. Matching the canonical file fingerprint does not prove
that a live archive lookup selected it or retained its bytes unchanged.

## How to reproduce

Resolve licensed inputs through tools/game-dir.mjs. Verify OBJEX.GFF's
length and run the committed tools/evidence/xxh3.mjs on that file; compare
with FND-ACTOR-001 and reuse its recorded complete field census.
With the project-local evidence Python run resident_listing.py on complete
half-open ranges 31E0:0EFF..31E0:128C and 31E0:2670..31E0:271E.
The relevant complete listing is retained in
GAME_DIR/analysis/hit-incoming-20261104/temporary-normal-callset.txt.
Trace BP-24, +0C/+0E, all override/copy gates and the +1155 argument;
then apply the flags-zero store and parent's signed comparison.
Approved executable readers verify the configured DSUN.EXE fingerprint
and mappings. Original-derived output stays only in GAME_DIR.
