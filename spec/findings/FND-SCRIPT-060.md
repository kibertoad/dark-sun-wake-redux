---
id: FND-SCRIPT-060
title: Eight counted shipped trigger-registration families have no zero entry-offset operand
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: retained script_selector_census.py operand reports from FND-SCRIPT-052; source/index/tool fingerprint revalidation at 5f283271; FND-SCRIPT-038 producer interfaces and FND-SAVE-023/FND-COMBAT-151 conversion interfaces
environment: null
---

## Observation

FND-SAVE-023 leaves open whether scripts register trigger entry offset
zero with a nonzero script number. Its reverse-conversion observation
matters here: under the retained source-table and buffer conditions,
GPLI entry zero maps to the pair zero/zero, and the trigger pass gates
on the script word rather than testing whether the entry is already an
offset (FND-COMBAT-151). This assesses the eight counted top-level
trigger-registration producer families, not all native trigger states.

The reused index and operand reports are FND-SCRIPT-052's. Revalidation
matches GPLDATA.GFF XXH3-128 2bd1d2b989eef2dcb0545feb95975fca and
OBJEX.GFF e08d3cac153c547f6804ca27b155915d. The index SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
The current reporter and parser SHA-256 values match their retained
report provenance: cf79b67e2031a4ed8c336c93619b796ffd634d67e82aca87d56fa51c4e629bd1
and 1df8f4564bdf93715e43f0c0f1115b071590e68ec8d0487443e6ed8d702c66c2.
No changed input or rebuilt parser is substituted for that result.

FND-SCRIPT-038 establishes which operand becomes the trigger's entry
word. FND-SCRIPT-052's queries enforce these family counts and arities.
Every selected expression is a simple literal. Its low word, rather
than the source text's sign or spelling, supplies the comparison:

| Opcode | Sites | Zero-based offset operand | Minimum low word | Maximum low word |
| --- | ---: | ---: | ---: | ---: |
| 65 | 261 | 0 | 1 | 8474 |
| 66 | 185 | 0 | 1 | 8433 |
| 68 | 150 | 2 | 1 | 4469 |
| 6A | 321 | 4 | 1 | 7851 |
| 6C | 11 | 0 | 1 | 7572 |
| 6D | 305 | 0 | 1 | 8293 |
| 6E | 291 | 0 | 1 | 8700 |
| 70 | 163 | 2 | 1 | 7189 |

Together these are 1687 sites, with zero nonliteral selected operands
and zero offset low words equal to zero. The independently recorded
opcode counts and operand layouts are controls, not counts derived
from a search for zero. FND-SCRIPT-045's MAS/1 +0135 registration
supplies the known nonzero control, entry 2625 for GPL/37.

The same retained operand query also covers opcode 6F, which changes
object entry points rather than these trigger lists. It contains 206
zero low words among 699 literal operands. The index explicitly shows
zero/zero object-entry pairs at GPL/2 +226D, GPL/3 +079F and GPL/5 +05C1.
These are positive zero controls for literal decoding and classification;
they are not trigger-registration sites. A zero is not being silently
dropped or treated as a nonliteral expression by this query.

## Interpretation

The eight counted shipped top-level registration families do not supply
the zero-entry trigger case questioned in FND-SAVE-023. This removes
those producer sites as its source under the recorded parameter/copy
bindings. It does not remove the converter's zero case from its contract.

Nested instructions, the GPL/37 omitted decode span, other opcodes,
native construction, reset contents, restored or partial records and
changed script/parameter/buffer bindings remain outside this exclusion.
Historical zero counts for opcodes 69 and 6B are FND-SCRIPT-038's, not
new controlled operand queries here. No runtime registration or failure
admission is proved. Q-SCRIPT-025 retains the restore buffer, partial-I/O
and conversion conditions; Q-SCRIPT-019 retains setup and later-use states.
There is no queue closure or complete-reading declaration.

## Alternatives

The zero-valued 6F object setters cannot serve as evidence that a trigger
list receives such a pair. Conversely, no zero in these eight source
families cannot establish that the live trigger pool never contains zero
entry words with nonzero script words. In particular, failed or skipped
restore transfers need not recreate a decoded registration's literal pair.

## How to reproduce

Reuse FND-SCRIPT-052's gpl37-entry-producer-offset-config.json and
gpl37-entry-producer-offset-report.json under
GAME_DIR/analysis/handler-census-20261106/, after verifying source hashes,
the music-health-all-scripts.txt index hash and reporter/parser hashes
above. If any input changes, regenerate with script_selector_census.py
and retain its count, arity, decoder-error and inline-echo controls.

For the eight opcode domains in the table, inspect every row's lowWord
and literal fields, require nonliteral count zero and verify the bounds
and site totals above. Inspect the separately queried 6F domain for its
zero-valued controls; do not merge object setters with trigger producers.
Retain the 104919 indexed starts, one GPL/37 decode error at 0D39 and
one inline echo, with resumption at 0D58. No source decode gap, native
state or transfer outcome is resolved by this aggregation.
