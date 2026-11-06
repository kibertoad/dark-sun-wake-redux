---
id: FND-CONFIG-225
title: Inventoried heap control stores have explicit owners and ordinary resize nesting republishes the same saved data segment under preserved bindings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1367..1000:177C
tool: window_operands.py and resident_listing.py at 09421e90; installed function inventory; Ghidra 12.1.3 read-only INST-single-20261103b; FND-CONFIG-167/211/223 and FND-EXE-272/273/281/283/295/296/297 interfaces
environment: null
---

## Observation

This audits concrete root/saved-state producers for Q-CONFIG-013 rather
than treating all shared-slot writes as an unspecified preservation gap.
Source DSUN.EXE is634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The installed inventory's LF-normalized
SHA-256 is820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.

### Explicit-displacement mechanism

The controlled window1358..1368 query emits58 operands in inventoried
routines, including16 word stores through CS in the modeled1000 segment.
Their field ownership is:

| CS field | Listed stores | Sites |
| --- | ---: | --- |
| 135B | 2 | 13A5, 150C |
| 135D | 5 | 137B, 138E, 13AC, 1511, 1550 |
| 135F | 4 | 13B3, 1454, 145C, 148D |
| 1361, saved DS | 3 | 14A0, 15B4, 1710 |
| 1363, request high word | 1 | 1715 |
| 1365, request low word | 1 | 171A |

FND-EXE-283 supplies the release-root controls, FND-EXE-273/281 the
free-list controls, FND-CONFIG-211 the acquisition/growth controls,
and FND-CONFIG-167/FND-EXE-272/295 the three independent saved-DS
writer controls. The query recovers every named control. The earlier
FND-EXE-298 request-word MOV scan agrees with the two listed stores,
but its raw-scan limits are not silently removed by that agreement.

The second pass emits58 raw candidates, all inside inventoried instructions,
with zero outside candidates for this window. These include decodes after
segment prefixes; no additional writer is thereby established. Alternate
interior entries, computed addresses/segments, indexed stores whose encoded
displacements lie elsewhere, bulk destinations and physical aliases remain
outside the controlled explicit-displacement result. String-store counts
for routines with hits are not a census of all implicit writes in the file.

FND-EXE-295's four shipped zero words at135B..1361 are initial file data,
not an invariant that roots remain zero or saved DS remains unchanged.
The listed root producers and request-word publications have branch-specific
values already read in the findings above; the census does not invent an
additional root initializer or atomic publication of adjacent words.

### Ordinary resize calls need not change the saved DS value

Let D denote the incoming DS of1702. It publishes D to1361 and the
request words to1363/1365 before selecting its branch. The connected
instruction reading and FND-EXE-295/296/297 give these local relationships:

| Selected branch | Nested shared-slot writer and incoming DS |
| --- | --- |
| No existing segment | Calls15A5 before an own DS change; it republishes D |
| Existing segment, zero request | Calls149B before an own DS change; it republishes D |
| Existing count smaller than request | 1622 calls15A5 before changing DS for copying; it republishes D |
| Existing count larger, nonterminal block | 169E changes ES, not DS, before149B; it republishes D |
| Existing count larger, terminal block | 169E calls17F2, which has no own1361 store |
| Equal count or local arithmetic rejection | No nested allocator/release call on that branch |

On the copying branch,1622 later changes DS to the original block segment,
but explicitly reloads DS from1361 at168D before calling149B. If the
shared slot retains D, that release again receives and republishes D,
rather than the copy-source segment.149B and15A5 each restore DS from
1361 on their ordinary returning suffix;1702 does so again at1773.

These relationships assume the recorded argument/segment bindings, intact
frames, stable relevant locals and no intervening alias, reentry or external
effect replacing1361 or DS. They classify the bodies' own nested writes;
they do not establish those assumptions for every invocation or prove heap
validity, successful release or returned capacity.

The repaired resident Ghidra view contains1702 and169E and agrees with
their branch calls. Its inferred saved-DS constant57E0 is not used as an
all-callers invariant, nor its inferred arguments as the resize ABI. The
instructions establish incoming DS publication, ES-only changes in169E,
the168D reload and the three actual shared-slot writer sites.

## Interpretation

Q-CONFIG-013 can reuse the sixteen named explicit control stores and
the ordinary nested-write relationships. A nested call alone is not proof
that the saved data segment becomes different. Equally, shared storage is
not a private stack save: computed/bulk aliases, changed segment admission,
reentry and actual callee effects still need evidence.

The unresolved heap obligations are now those mechanisms and concrete
header/free-link/break/bound producers, live spans and service outcomes,
not rediscovery of these direct root stores. No writer-completeness claim,
whole-entry complete reading or queue closure follows.

## Alternatives

Treating the saved word as private ignores all three writers. Treating
every nested write as a changed value ignores the DS relationships above.
Assuming DS must be57E0 from a decompiler constant skips actual incoming
segment provenance. Prefix-interior candidates are not extra established
writers or a reason to discard the known instruction starts.

## How to reproduce

Run window_operands.py on the supported executable and
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv with1358..1368. Retain
heap-root-and-saved-ds-window.txt under
GAME_DIR/analysis/handler-census-20261106/. Require58 listed operands,
the sixteen controlled stores and58 interior/zero outside candidates.
Keep all report exclusions when reusing the result.

Read resident_listing.py1000:169E..1000:177C and reuse the full entry
readings cited above, especially1622's DS reload before149B. The local
listing is heap-resize-shared-ds-context.txt. In current read-only
INST-single-20261103b, program DSUN.EXE, run ReportDecompileMatches.java
at1000:1702 with1361,1363,1365,15a5,149b and at1000:169E with149b,17f2.
Require both function markers; retain heap-root-resize-decompile.log.
All original-derived reports remain local. No original or emulated run
supplied this result.
