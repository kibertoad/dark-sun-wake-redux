---
id: FND-SOUND-065
title: Shipped DJ.DAT provides nine startup fallbacks and nonempty combat lists of four three and three records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DJ.DAT
    offset: 0x00..0xE7
tool: tools/evidence/report.mjs table and boundedTable in tools/evidence/review.mjs at c94131d7; FND-SOUND-011/012/061/064
environment: null
---

## Observation

The approved installed DJ.DAT is 231 bytes, XXH3-128
78012eb5cebe8d445b19b365cbfc8362, matching the build manifest.
The bounded table report reads 38 six-byte records from offset three,
ending exactly at byte 231. Fields are region byte +0, chance byte
+1, health-band byte +2, mode word +3 and song byte +5, following
FMT-SOUND-002 and the direct code readings. The reporter returns
unsigned raw fields; region 255 maps to signed -1 only through
FND-SOUND-071/064's signed-byte interpretation.

Grouped results are:

| Mode | Region distribution | Health-band distribution | Records |
|---|---|---|---|
| 1 | Nine at 255 and one at 1 | All band one | 10 |
| 2 | In each band, three at 255 and one each at 1, 56 and 66 | Six each in bands one, two and three | 18 |
| 3 | All at 255 | Four at band one, three at band two, three at band three | 10 |

The mode totals and mode-one fallback distribution agree with
FND-SOUND-011, supplying independent structural controls for this
reporter's record offset/stride and selected field positions. All ten
mode-three records have chance byte ten. No other mode or health-band
value occurs in the 38 records.

### Conditional selector consequences

For the unchanged shipped records, FND-SOUND-064's mode-one scan
can populate its any-region list with nine indexes. The direct-region
record can instead take the immediate song path when the supplied
region matches one. Actual current region, count, pointer and segment
bindings remain prerequisites to applying that scan.

For unchanged mode three, its three candidate-list counters finish
at four, three and three respectively. Those values are below the
conditional capacity of 38 bytes per list. With admitted DS/SS backing,
intact frames and retained stores, each list is populated before later
selection. The resulting normal last indexes are three, two and two.
Health eight's special initialized selector/last-index zero instead
reads index zero of the first list; that list has four stored entries,
so this particular case does not read an empty list under those
conditions. The code's lack of an unconditional list clear remains;
changed records, fields, frames or bindings require separate evidence.

When supplied region is not -1, no mode-three record matches that
region byte. Under the unchanged signed-region comparison, this takes
the random-list fallback rather than the found-region two-chance route.
All such selected records supply chance ten. The random-index and
chance helper contracts must still be applied to their admitted inputs;
raw table values alone do not prove returned indexes, successful driver
calls or native output. A supplied region -1 is a distinct local case
and is not excluded from actual caller history by this data survey.

Mode-two records remain present but do not become used merely because
their lists would be populated by the same fields. Q-SOUND-004 retains
actual mode-two selection and song dispatch meaning. This survey does
not convert an unused record or an opaque driver request into gameplay.

## Interpretation

Q-SOUND-011 can remove empty candidate lists for the unchanged shipped
payload under its explicit selector/frame/binding conditions. Actual
loaded-byte preservation, changed input domains, DS/SS/ES identities,
other computed/aliased/indirect accesses and selected random-helper
contracts remain open. The findings now distinguish a fixed payload's
list population from runtime lifetime and output. No complete reading,
whole queue closure or native successful playback is claimed.

## Alternatives

- A shipped mode-three band has no candidates: the raw field grouping
  finds four, three and three records.
- Health eight necessarily selects uninitialized list memory: with the
  unchanged payload and admitted list stores, first-list index zero exists.
- The payload proves every runtime selector result: pointer, state,
  frame, list preservation and callee outcomes remain separate conditions.

## How to reproduce

Use the existing fingerprint-checking bounded table reporter; no new
reader is required. Retain music-table-population-config.json and
music-table-population-report.json under
GAME_DIR/analysis/handler-census-20261106/. The config source is the
installed DJ.DAT with the hash above; table start 3, count 38, stride 6,
countEvidence FND-SOUND-011/061, and the five field offsets/widths above.
Run node tools/evidence/report.mjs table with that config. Group its rows
by mode, health_band and region, then separately inspect chance for mode
three. In PowerShell, ConvertFrom-Json and Group-Object on those property
names reproduce the counts. Keep the raw unsigned interpretation separate
from signed selector use. No original game, emulator session or emulated
call supplied evidence; the same manifest hash for CD:DJ.DAT does not
claim an independent media read in this batch.
