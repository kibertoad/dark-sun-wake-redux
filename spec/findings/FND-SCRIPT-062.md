---
id: FND-SCRIPT-062
title: Reverse conversion after failed restoration can reinterpret a retained ordinary trigger offset as a GPLI entry number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070B4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001F8F7F..0x001FAE57
tool: tools/evidence/report.mjs table at 38fb861a; retained counted producer index from FND-SCRIPT-052; connected interface composition using FND-COMBAT-035/111/151 and FND-SAVE-023
environment: null
---

## Observation

This assesses one failure-continuation consequence for Q-SCRIPT-025,
using the already-read reader, registered pair producers and reverse
converter together. It does not claim a native failed load was observed.

FND-COMBAT-035 establishes that a nonzero resource-reader AX stops the
numbered loop without rolling back earlier buffers. Failure before its
index 37 therefore omits the reader's own SAVE/38 request. It does not
prove that earlier transfers or other aliases leave trigger bytes intact.
FND-COMBAT-111 follows the failed selector-zero continuation: it skips
the object recount/reconstruction but rejoins the converter call, with
the trigger-pass low argument byte one. Returning child calls and the
saved local byte's preservation are conditions of reaching that call.

FND-COMBAT-151 reads the reverse converter's trigger pass. For each of
200 records, a nonzero script word at byte two admits a lookup of the
entry word at byte zero against GPLI's entry-number field. It does not
first validate the paired script word against that row, identify an
offset-versus-entry representation tag or test the earlier transfer flag.
A matching row replaces both words; no match leaves them unchanged.
Its successful GPLI acquisition, positive narrowed scan bound, current
pointer/record/table bindings and absence of interfering aliases remain
the recorded conditions of those local outcomes.

### Concrete source pairs and shipped mapping

The fresh bounded table report verifies GPLDATA.GFF size 2191945 and
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca. Its GPLI/1 span has 1316
six-byte rows. All 1316 entry numbers are distinct, ranging from zero
through 1315. The controls are entry 1 mapping to offset 1/script 1,
entry 167 to offset 1/script 37, and entry 168 to offset 2625/script 37.
The latter two reproduce FND-SCRIPT-044's independent mappings.

FND-SCRIPT-052's retained counted index supplies two ordinary opcode-6E
registrations: GPL/48 +0363 requests offset 1/script 37, and MAS/1 +0135
requests offset 2625/script 37. FND-SCRIPT-038's builder copies those
parameters as the trigger's entry and script words. Actual execution,
allocation, insertion and survival to a failed load are not inferred
from these decoded source instructions.

Under the converter conditions above, their distinct input representations
give these local outcomes:

| Input entry/script words | Meaning on entry | Reverse conversion result |
| --- | --- | --- |
| 167 / 37 | Saved entry number for offset 1 in script 37 | 1 / 37 |
| 1 / 37 | Retained ordinary offset 1 in script 37 | 1 / 1 |
| 168 / 37 | Saved entry number for offset 2625 in script 37 | 2625 / 37 |
| 2625 / 37 | Retained ordinary offset 2625 in script 37 | unchanged |

In the second row the nonzero script word passes the gate, but its value
37 does not prevent the entry-number-one match from replacing it with 1.
In the fourth row there is no shipped GPLI entry numbered 2625. These
are conditional outcomes of the documented procedure and checked table,
not an execution of original code or a trace of an actual failed restore.

## Interpretation

A failed transfer flag alone does not make later reverse conversion
harmless to retained ordinary trigger offsets. Whether a retained pair
changes depends on its entry word's collision with a GPLI entry number,
not on whether it is a valid ordinary offset for its existing script.
Conversely, the converter does not necessarily change every retained pair.

This removes the local representation-confusion outcome from unread
failure work under Q-SCRIPT-025. Remaining evidence is whether an admitted
failed or skipped transfer leaves such a record in the buffer actually
read, with the required table, pointer and child continuations. Earlier
transfer aliases, partial bytes, initialization, cleanup and later use
remain explicit. No native failure or player-visible defect is established,
and no queue closure or complete-reading declaration follows.

## Alternatives

Using the earlier transfer-success byte as an implicit converter guard
contradicts the shared failed-reader continuation. Using the record's
nonzero script word as a representation discriminator contradicts the
entry-only match. Treating a valid registered pair as evidence it survives
unchanged to that continuation would assume the remaining admission proof.

## How to reproduce

Regenerate the bounded table with tools/evidence/report.mjs table and
GAME_DIR/analysis/handler-census-20261106/gpl37-entry-index-config.json,
whose source fingerprint, start 1F8F7F, stride six, count/limit 1316 and
unsigned word fields are FND-SCRIPT-044's. The accepted output is
trigger-failed-restore-index-report.json in that local store. Verify the
entry domain and three controls above; do not confuse physical row order
with its entry-number field.

Reuse FND-SCRIPT-052's fingerprinted index for the two exact registration
sites. Follow FND-COMBAT-035's failed-loop exit and FND-COMBAT-111's shared
ending, then FND-COMBAT-151's gate, first-match writes and unmatched path.
Apply that procedure separately to the four input pairs above. No native
run, emulated call, new parser or original-derived output in Git is needed.
