---
id: FND-EXE-635
title: Explicit tail decodes recover the three overlay 204 bodies in an isolated regenerated snapshot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DDE9..0x0008DE17
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE45..0x0008DF0E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DF0E..0x0008DFC4
tool: Ghidra 12.1.3; Temurin JDK 21.0.12.1; New-FbovMappedImage.ps1, fbov_overlay_plan.py, CorrectNearTransfers.java, ApplyDeclaredEntries.java, ApplyDeclaredFlows.java, FillInstructionBodies.java, ClipBodiesToEntryRegion.java and ExportResearchBaseline.java at 9c9e6b5b
environment: null
---

## Observation

FND-EXE-633 records three truncated overlay-204 bodies in the committed
DSUN inventory. This reading tests their regeneration mechanism, without
replacing the committed inventory or claiming the other rows are complete.

### Recovered static prerequisite and input controls

The historical Ghidra projects were absent from GAME_DIR/analysis and
the repository's ignored local artifacts. The documented profile Ghidra
and JDK 25 installations were also absent. Ghidra 12.1.3 was restored
from its official archive after checking the published SHA-256; its
application properties permit the installed JDK 21. Exact installation
and launcher details are in docs/GHIDRA.md.

The shipped DSUN.EXE remains size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b. The existing revision-2 mapped-image
procedure produces XXH3 f48148049b7bd26475845bfcbeb2d11e, MD5
c1cf710e754178e4fe5fb5aa0a56fafa. The overlay-plan comparison reports
49 overlays, zero mismatches, 8262 fixup writes and 854 trampoline targets.
No game or emulator process was run.

A fresh mapped-image import followed the existing overlay reconciliation
steps in docs/EVIDENCE-TOOLS.md. Its three target rows still have sizes
10, 195 and 7. A further frozen export of the saved snapshot, read-only
with analysis disabled, has 878 overlay rows. Two exports agree for each
of the inventory, provenance and region files.

### Separate repair controls

All three overlay-204 entries map to segment 96BC in this image.
A normal-fall-through experiment at file offsets 8DDEF, 8DF05 and
8DF14 does not extend their bodies. That experimental configuration
is retained locally and was discarded from the repository declarations.

A second control applies only the following `decode` ranges through
ApplyDeclaredFlows.java to a copy of the saved pre-repair snapshot.
Its jumps, returns and fallThroughs arrays are empty. Ranges are
half-open shipped-file offsets; owners name existing function entries.

| Owner | Decode range | Before size | After size | Added bytes |
|---|---|---|---|---|
| 0x0008DDE9 | 0x0008DDF3..0x0008DE17 | 10 | 46 | 36 |
| 0x0008DE45 | 0x0008DF08..0x0008DF0E | 195 | 201 | 6 |
| 0x0008DF0E | 0x0008DF15..0x0008DFC4 | 7 | 182 | 175 |

Compared with the frozen pre-repair export, these are exactly the
three changed bodies, with no added or removed entry. Their ends now
match the returns read in FND-EXE-633, adding 217 body bytes in total.
The isolated flow pass reports three decoded ranges, three affected
functions, two body-recomputation passes, no conflicts, no removed
false starts, no jumps or references added or dropped and no fall-through
override changes. The later clipping pass removes no bytes, and the
entry check reports zero entries without instructions. Both repeated
exports agree for all three metadata files.

Reapplying the full existing flow model as well as these ranges is a
different control: it also expands the body at mapped 6F0E:1373 from
2131 to 4998 bytes relative to the frozen snapshot. That extra change
is not attributed to the isolated three-owner repair or accepted into
the committed denominator here. The import's in-process exports also
precede an automatic post-analysis pass: their 876 rows differ from
the saved snapshot's frozen 878 rows. Repeated exports inside that
import alone therefore do not identify the final saved snapshot.

Against the committed overlay rows, that full-model export has seven
changed sizes, new starts 0x00065EB2 and 0x00065EF1, and lacks start
0x0006B749. Besides the three tail controls, sizes differ at 0x00065653
(5048 to 4998), 0x0006B581 (456 to 544), 0x0008C647 (388 to 178)
and 0x0008C834 (374 to 358). These metadata differences bound the
remaining review; sizes alone do not establish correct body ownership.

## Interpretation

The three documented tails can be recovered with the existing declared
decode mechanism and source-evidenced owner/range inputs. Merely
restoring tools, repeating export or setting normal fall-throughs does
not repair them. The isolated control separates these tail repairs
from reapplication of unrelated flow declarations and from import
post-analysis changes.

Q-EXE-032 remains open. A durable declaration path, its preservation
by regeneration, the full overlay-row boundary check and accepted
resident/overlay reconciliation are still required. The committed
inventory and its closure-plan fingerprint are unchanged.

## Alternatives

- The extra full-model body expansion and other differences against the
  older committed inventory need review before staging a replacement.
  Equal function counts do not prove equal starts or body ownership.
- The isolated pass repairs the three positive controls; it is not a
  negative census for all other premature endings. No complete-reading
  declaration or gameplay evidence status follows from this repair.
- walk_declared_flows.py currently regenerates the decode array from a
  walk and does not preserve separately evidenced manual decode rows.
  A durable repair must address that explicitly rather than silently
  relying on the local experimental configuration.

## How to reproduce

Use the supported source and mapped-image identities above. Follow the
overlay-view procedure in docs/EVIDENCE-TOOLS.md with Ghidra 12.1.3,
then save the project and export it twice with -noanalysis -readOnly.
Keep that frozen export as the pre-repair control. Use a separate copy
for each experiment. For the isolated control, give ApplyDeclaredFlows.java
the existing configuration layout with empty jumps, returns and
fallThroughs, and only the three decode/owner pairs above. Run the
existing fill, clip, entry check and repeated metadata export steps.
Compare exact starts and body ranges with the frozen control; check
all script-level markers as well as exit status.

Local regeneration inputs, projects, configurations and per-invocation
metadata are under GAME_DIR/analysis/inventory-reconcile-20261029/.
The runs overlay-import-01 and overlay-import-02 retain the failed
unquoted launcher and successful quoted retry. overlay-frozen-01,
overlay-repair-01, overlay-decode-01 and overlay-isolated-01 retain
the frozen, fall-through, full-model and isolated controls respectively.
No original bytes, project database or decoded listing is committed.
