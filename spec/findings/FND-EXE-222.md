---
id: FND-EXE-222
title: Source decoding reaches conditional return tails from all four descriptor-base dispatch targets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874E7..0x000874F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874F5..0x0008753C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008753C..0x00087552
tool: executable-reader 2.5.0, engine 13.6.0, Capstone 5.0.7 and pypcode 4.0.0
environment: null
---

## Observation

Independent source decoding accepts all four descriptor-base candidate roots
from FND-EXE-221, including the three absent from the saved corrected listing.
The roots are descriptor-relative offsets `0x01D7`, `0x01E5`, `0x01F1`
and `0x01F4`, shipped offsets `0x000874E7`, `0x000874F5`,
`0x00087501` and `0x00087504`. Each initial bounded traversal reaches
the computed jump at `0x00087515` and far return at `0x0008753B`.
Without a declared indirect continuation each reports incomplete discovery.

The separately bounded eleven-slot table in FND-EXE-173/221 occupies
`0x0008753C..0x00087552`. Under the descriptor code-base binding, its
eleven words name four distinct targets: descriptor-relative offsets
`0x020A`, `0x020F`, `0x0214` and `0x0217`. The source reader checks
the supplied computed-jump site and derives these targets from the table
words, rather than accepting target addresses from the query.

Adding that explicit table continuation yields these results:

| Root offset | Reached instructions | Reached instruction bytes | Complete half-open shipped-file intervals |
|---|---:|---:|---|
| `0x01D7` | 29 | 73 | `0x000874E7..0x000874F3`, `0x000874FC..0x00087501`, `0x00087504..0x0008753C` |
| `0x01E5` | 27 | 68 | `0x000874F5..0x00087501`, `0x00087504..0x0008753C` |
| `0x01F1` | 24 | 59 | `0x00087501..0x0008753C` |
| `0x01F4` | 23 | 56 | `0x00087504..0x0008753C` |

Every extended report has one far-return exit at `0x0008753B`, exclusive
end `0x0008753C`, with zero additional argument cleanup. No calls,
hardware boundaries or remaining traversal gaps are listed. Each explicitly
retains one assumed continuation: the jump consumes the declared table,
exhaustively under the supplied descriptor-relative mapping. The reports
set their CFG-complete field to true under that assumption.

## Interpretation

FND-EXE-221's missing saved-listing starts are not a failure of bounded
source decoding at those same candidates. The original bytes support
conditional tail traversals beyond the saved analyzer body. This narrows
the inventory-repair task but does not authorize publishing those candidates
as new functions or replacing the current whole-function body blindly.

These roots are table targets inside a candidate procedure, not admitted
procedure entries. Native CS, the outer procedure's callers and frame setup,
input admission, exhaustive runtime dispatch and ownership remain unresolved
under Q-EXE-010. The far-return suffix relies on its incoming frame; this
static CFG report neither supplies that frame nor executes the return.
CFG-complete is not the Standard's complete_reading declaration. No status
promotion or inventory replacement follows.

## Alternatives

The saved listing's missing starts cannot alone show that these shipped
bytes are undecodable. Conversely, successful decoding and one conditional
return do not prove actual entry or the runtime descriptor-base binding.
The competing alias interpretation and remaining anomalous fragments still
need their own evidence; these results do not settle them.

## How to reproduce

At revision `445022b`, run `node tools/evidence/report.mjs x86-bounds`
against the installed DSUN.EXE with sourceKind mz and XXH3
`e296af55ba2ecde7e77f555c90f33d0b`. Use one region with start
`0x00087310`, exclusive end `0x000878CC`, segment `0x9211`, ip zero,
and name descriptor-198-candidate. Its evidence is FND-EXE-221's
descriptor-relative candidate with native CS and incoming admission
unresolved. This segment is the analysis binding, not a claimed native CS.
Use formatControls overlays 49 and fixups 8262.

Run separately with each of the four shipped root offsets above as entry
and as the region's sole entries value. First omit indirectJumps. Then add
one declaration: site `0x00087515`, exhaustive true conditional on this
binding, evidence naming the independently bounded eleven-slot arm. Its
table has start `0x0008753C`, count eleven, stride two, width two and
fieldOffset zero. Table evidence names the nonzero selector mask, word
input minus ten, unsigned bound ten and doubling from FND-EXE-173/221.
Supply no target-address list, register seeds or callee summaries.

Retain default limits: 512 steps, 64 paths, depth eight; continuation
limits 64 paths, 20,000 total steps, 512 per path, visit limit four and
4,096 string iterations. Preserve assumedContinuations and complete along
with intervals and exits; none of the reports is a native observation.
Configurations and report output stay in GAME_DIR and are not committed.
