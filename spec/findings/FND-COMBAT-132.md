---
id: FND-COMBAT-132
title: Nominal fixed SAVE payloads miss eight UI admission regions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E57E..0x0004E672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00044580..0x00044674
tool: tools/evidence/report.mjs table and operand at 8c52bf0f; FND-COMBAT-116 relocation map and half-open interval comparison
environment: null
---

## Observation

Q-COMBAT-046 retains UI-state producers and preservation through
restoration. FND-COMBAT-118 already compares nominal payloads with
source-pointer metadata; this checks a different named writer target:
the shared UI fields that admit or route a later load. It reuses the
46 fixed relocated destinations of FND-COMBAT-116 and retains all
fifteen dynamic destinations as separately unmeasured. Address and
length endpoints below are hexadecimal.

### Input identity and fixed-span reuse

Fresh table reads verify DSUN.EXE length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The pointer table has 61 rows at
4E57E with stride four, offset word at +0 and relative segment word
at +2. The length table has 61 rows at 44580 with stride four and
a double-word length at +0. The same 46 rows are nonnull.

For every nonnull row, the fresh offset/relative segment and length
match the retained resolved span: file target is
5200 + 16*relative_segment + offset. FND-COMBAT-116 supplies the
declared relocation check for each segment operand at 4E580 +4*i,
mapped with load segment 1000. That unchanged-source relocation
evidence is reused; raw segment words alone do not establish it.

### The eight checked UI regions

These are the specific resident fields read or written in the existing
UI/dispatch findings, not an inventory of all UI state:

| State | Loaded region | File region | Existing field reading |
| --- | --- | --- | --- |
| Selected row and shared top | 4C4C:0000..0004 | 416C0..416C4 | FND-SAVE-026, FND-COMBAT-131 |
| Save/Load window pointer and mode byte | 50AF:0000..0005 | 45CF0..45CF5 | FND-COMBAT-085/129 |
| Shared saved handler | 4C4D:000F..0013 | 416DF..416E3 | FND-COMBAT-098 |
| List pointer and exit byte | 57E0:6278..627D | 53278..5327D | FND-COMBAT-081/129 |
| Stored global-handler mirror | 57E0:61A2..61A6 | 531A2..531A6 | FND-COMBAT-098 |
| Global A0F1 and A0F5 callbacks | 57E0:A0F1..A0F9 | 570F1..570F9 | FND-CONFIG-221 |
| Current event window target | 57E0:A0FD..A101 | 570FD..57101 | FND-CONFIG-221 |
| Window count and head | 57E0:A103..A109 | 57103..57109 | FND-CONFIG-222, FND-COMBAT-093 |

Grouping joins only adjacent named fields. The gap A101..A103 is
not silently assigned to the current-target or count/head region.
All file coordinates use the same modeled MZ mapping as the payloads.

For each fixed row, compare [target, target + shipped_length) with
each of the eight regions. Half-open intersection requires
`payload_start < region_end` and `region_start < payload_end`.
All 368 comparisons have zero intersections.

Two controls use SAVE/24 alone: its own 43B5D..442DD interval
intersects itself, while the adjacent one-byte target 442DD..442DE
does not intersect that payload. The latter is not a comparison
against every other SAVE row. These controls distinguish actual
intersection from an always-empty test and from inclusive endpoints.

### Dynamic spans and the complete-resource reader remain separate

FND-COMBAT-117 identifies the fifteen runtime bindings. Under its
valid producer/heap, preserved-field, admitted-lifetime and fitting
nonwrapping-transfer conditions, those payloads lie above resident
data and miss these regions too. The corrected heap-base condition is
FND-RNG-093. Those conditions are not proved by this fixed-span check.

FND-COMBAT-035 and FND-CONFIG-151 show that the restore reader requests
the selected complete resource length rather than enforcing this shipped
length table as destination capacity. Different supplying resources,
larger lengths, stale or replaced pointers and wrap remain outside
the result. The explicit placed-buffer clear of FND-COMBAT-119 is
also outside these payload comparisons.

Likewise, allocation, archive handling, callbacks and postprocessing
can have their own writes. A payload missing the count or handler field
does not prevent a child from changing it. This is not an all-writer
census or an immutability assertion for any of the eight regions.

## Interpretation

The known fixed SAVE payloads, at their shipped lengths and pointers,
do not themselves restore or replace these eight UI admission regions.
This removes that specific nominal payload family from Q-COMBAT-046's
unread preservation work. Actual bindings/lengths, direct and aliased
state producers, helper effects and event delivery remain open.
Q-COMBAT-064/065 retain resource-length and destination admission;
Q-COMBAT-063 retains the actual selected path and its storage lifetime.
No queue item closes or evidence status is promoted.

## Alternatives

- Fixed-span nonoverlap is not a statement about every runtime transfer.
- Allocation requests alone do not admit the fifteen dynamic spans.
- The shipped length table is not a bound enforced by the restore reader.
- A child write is distinct from a resource payload write.
- Nonoverlap cannot establish an unchanged global hook or valid window.

## How to reproduce

Use tools/game-dir.mjs and the hash-locked table/operand procedure in
FND-COMBAT-116. Reuse restore-shipped-pointers.json and
restore-shipped-lengths.json, the declared-relocation results and
restore-shipped-resolved-slots.json under
GAME_DIR/analysis/hit-incoming-20261104/. Read both tables afresh,
require 61 rows each and match all 46 retained nonnull offsets and
lengths against the fresh rows. Preserve the fifteen null/dynamic rows.

For each resolved [a,b) and each of the eight [c,d) regions above,
evaluate a < d and c < b. The endpoints, source mapping, table query
and the two SAVE/24 controls fully specify the comparison. Compare
FND-COMBAT-118 for the separate source-metadata check; do not treat
its seven regions as these eight UI regions.

Fresh reports and comparison metadata stay only in that local store:
ui-payload-pointers-report.json, ui-payload-lengths-report.json and
ui-payload-comparison.json. No payload, original byte table or memory
image is committed, and no original program is run.
