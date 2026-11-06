---
id: FND-SCRIPT-044
title: Shipped GPLI entries 167 and 168 bind GPL37 entry 0001 and its rejected-atom routine at 0A41
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001F8F7F..0x001FAE57
tool: tools/evidence/report.mjs table at c55e6711; FND-SCRIPT-002/017 index layout and FND-COMBAT-103/109 native-gap evidence reuse; pinned EVIDENCE_PYTHON
environment: null
---

## Observation

This entry-binding assessment uses installed GPLDATA.GFF, 2191945 bytes,
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca. FND-SCRIPT-002 identifies
GPLI/1 as 7896 bytes beginning at file 1F8F7F, containing 1316 six-byte
records of an entry number, script offset and GPL resource number.
FND-SCRIPT-017 reads the conversion between entry numbers and script pairs.

A bounded table query over those 1316 records finds exactly two whose
script number is 37:

| File-order row, zero-based | Entry number | Script-relative offset |
|---|---|---|
| 216 | 168 | 0A41 |
| 368 | 167 | 0001 |

The first three records independently match FND-SCRIPT-002's controls:
entry zero/offset zero/script zero, entry one/offset one/script one,
and entry three/offset 712 decimal/script one. This checks the field
order, stride and input origin. The explicit reporter limit is 1316,
matching the resource's documented size, within its supported maximum.
The records are not sorted in file order by entry number; their row
positions must not be used as the entry IDs.

FND-SCRIPT-043 reads the local routine starting 0A41, its conditional
stop, global-flag skip, returning menu and postamble at 0D29..0D37.
Thus entry 168 is a concrete shipped index binding for that routine.
An entry-table record does not establish that a particular trigger,
object, saved record or caller supplies that entry, or that its current
loaded mapping remains unchanged. Those actual input and lifetime cases
remain Q-SCRIPT-024. Entry 167's distinct 0001 binding must not be
substituted for entry 168's 0A41 offset.

### Reuse the existing malformed-continuation ownership

FND-COMBAT-103 already records the native B3 error-table target,
returning expression/parameter/opcode-77 continuation and the outer
interpreter's stop check. FND-COMBAT-109 retains a controlled read of
the unchanged 33-byte gap and excludes an encoded opcode 5E at every
byte alignment in that span. FND-SCRIPT-042 adds the scratch-initialization
and distinct parenthesis-error corrections to the rule; it does not
replace those earlier native findings or repair the script bytes.

Q-COMBAT-059 already owns the gap-specific error effects, saved operand
address, indirect byte write, stream/stop preservation and possible
additional zero-selector route. Q-SCRIPT-003 owns the shared error
service; Q-SCRIPT-018 owns opcode-hook target admission. Q-SCRIPT-024
is narrowed to actual entry and pre-rejection state/control-flow admission,
reusing those shared questions for later effects. This ownership
reconciliation answers no transferred condition and adds no queue closure.

## Interpretation

The entry uncertainty now has an explicit index binding: determine who
supplies entry 168 or independent entry routes, and which actual state
reaches the rejected atom along FND-SCRIPT-043's local paths. The native
error continuation need not be read again under a second question.
Neither the shipped mapping nor the earlier no-5E byte census establishes
native execution, a whole-entry complete reading or end-to-end behavior.

## Alternatives

Treating 216 as the entry number confuses file order with the first
record field. Treating the record as a native invocation omits the
entry-number producer and current mapping. Treating the decoder gap as
an unexamined native opcode layout omits FND-COMBAT-103/109; treating
those bounded findings as a complete error-path proof omits their retained
admission, callee and preservation questions.

## How to reproduce

Run tools/evidence/report.mjs table at c55e6711 with
GAME_DIR/analysis/handler-census-20261106/gpl37-entry-index-config.json.
Use the source path/fingerprint above, start 1F8F7F, count and explicit
limit 1316, stride six, and unsigned two-byte fields entry at zero,
offset at two and script at four. Count evidence is FND-SCRIPT-002's
7896-byte GPLI/1 resource and layout. Retain gpl37-entry-index-report.json
in the same local store. Select rows whose script field is 37 and verify
the three independent controls. The first request with the default
256-row reporter limit was rejected; the accepted query explicitly names
the documented 1316-row bound without changing the reporter.

Reuse FND-SCRIPT-043's retained GPL/37 text and FND-COMBAT-103/109's
native-gap procedures and controls. No original game, emulated call or
source modification supplied the result.
