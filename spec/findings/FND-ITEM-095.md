---
id: FND-ITEM-095
title: Exact shipped ETAB loads produce positive counts and a cleared trailing sentinel rather than the fresh zero-count case
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x0000DC89..0x0000F7A1
tool: tools/research/exec-census/etab_objects.py and tools/evidence/xxh3.mjs at 2895c032; Python 3.12.10; composition with FND-REGION-004, FND-ACTOR-001/012 and FND-ITEM-066/093/094
environment: null
---

## Observation

Q-PARTY-116 retains actual ETAB content and constructor count/key admission.
FND-REGION-004 already records all twenty installed region ETAB tables:
13559 eight-byte records, resource lengths 1144 through 7672 bytes, and
positive object words in every record. This reuses that complete field
census and binds it to FND-ITEM-094's loader, with a fresh zero/control scan.
Offsets below are hexadecimal; all other counts are decimal.

### Source controls and existing field census

The twenty installed RGN*.GFF files' lengths and XXH3-128 values match
their entries in BLD-GOG-EN-1.1.files.yaml. etab_objects.py scans the 26
top-level GFF archives and reports twenty ETAB resources with 13559 records.
Requested object word zero produces no row; the positive object-305 control
appears in RGN0FF.GFF ETAB/255 at index 150. Two repeated reports match.
This verifies the absence test on the reader's full ETAB domain while
retaining a positive row through the same decoding path.

The control is not an opening-region placement: region 255 is outside
overlay 200 +00E2's unsigned 1..99 argument gate. FND-ACTOR-012 separately
records opening-region RGN032.GFF ETAB/50 as 867 rows. This finding does
not identify the active runtime archive or admit a request for every source.

FND-REGION-004's field census also records mask 20 in 8128 source flags.
Those are flag seeds across the full installed domain, not a count of
runtime effects or of additional rows flagged by equal-object propagation.
FND-ACTOR-001 binds source byte +4 to its corresponding object definition;
it is not universally zero. No source field is converted into gameplay code.

### Conditional exact-transfer count

Assume an admitted region argument selects one of these source resources,
the 12000-byte clear succeeds, the transfer writes exactly that resource's
bytes at the same buffer origin, and table/frame/segment identity survives.
Then its number of nonzero-slot source rows is exactly length divided by
eight, between 143 and 959 across the full source census. The first
following row is wholly in the cleared tail and has a zero slot word.

The recount negates each positive source object word, sets mask 08 and
stops at that cleared tail row before the 1500 cap. It therefore produces
the source row count, strictly positive. Its read at the stopping row is
within the 12000-byte clear and does not need the allocation padding at
row 1500. The opening resource's admitted count is 867.

Initial mask-20 seeds survive the recount's own mask-08 stores. Its second
phase propagates mask 20 across equal normalized object words as
FND-ITEM-094 describes. There is no source zero-slot terminator inside any
of these exact table bodies and no need to infer one from a count cap.
This is a conditional source-to-loader composition, not an executed load.

### Constructor boundary consequence

Immediately after this admitted exact-load recount, without another count
writer, the state cannot satisfy FND-ITEM-093's entry count zero. That
finding's conditional boundary remains applicable to other admitted states;
it is not a demonstrated first effect after an exact shipped ETAB load.

The trailing zero row does not by itself settle a later constructor's
search result: row order, extra-row identity after mutations, coordinate/key
inputs, wrapper/child preservation and intervening creators/removers still
matter. Failed, partial, redirected or changed-source transfers and restore
paths remain separate cases. No native effect lifetime follows from this
positive-count result.

## Interpretation

This removes the source-internal-zero and empty-table alternatives for an
exact, identity-preserving load of the shipped ETAB domain from Q-PARTY-116.
It supplies a conditional positive-count and trailing-sentinel result with
known source flags. Active archive selection, transfer completeness/identity,
actual eligible region/caller inputs, later count/key/slot writers and row
lifetime remain open. Source fingerprints do not prove those runtime facts.

## Alternatives

Treating any initial clear as a later empty table ignores the positive source
rows. Treating a full-source region-255 control as a loader-admitted region
ignores its gate. Treating 8128 source flags as observed runtime effects
ignores slot normalization, propagation and later writers. No native run,
complete-reading declaration or parity promotion follows.

## How to reproduce

Resolve licensed input through tools/game-dir.mjs. Compare every installed
RGN*.GFF length and tools/evidence/xxh3.mjs result with the committed build
manifest; all twenty match. Reuse FND-REGION-004's complete field census.
Run project-local evidence Python -I etab_objects.py GAME_DIR 0 305 twice.
GAME_DIR/analysis/item-removal-20261104/ holds effect-etab-zero-control.txt
and its -repeat pair, both SHA-256
9ea710bb76c26197c102055028ed858656a19a1aba3232c4eaaac192566d3951.
Compose the count with FND-ITEM-094's clear/transfer gates and retain all
identity assumptions. Keep reports local. No original or isolated function ran.
