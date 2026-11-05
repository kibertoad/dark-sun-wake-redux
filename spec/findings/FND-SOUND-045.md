---
id: FND-SOUND-045
title: Setup error-state literal census identifies a create-mode selector consumer and retains normalized-word boundary candidates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04CE..1000:0507
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:317F..1000:333D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0C90..1C08:0C9C
tool: tools/research/exec-census/sound_ds_reach_config.py, tools/evidence/report.mjs x86-operand-candidates and tools/research/exec-census/resident_listing.py at 92d80760; locked scientific-method-engine and executable-reader; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

Q-CONFIG-011 retains error consumers and writers after FND-SOUND-044.
This reuses FND-EXE-572's setup region/entry builder, not DSUN-only
operand scripts. Its 25 regions total 58208 scanned byte starts; all
three queries are untruncated and report partialSearch=false for their
declared regions. This is search coverage, not a complete reading.

The operand command rejects the builder's indirectCalls property because
that property is supported only by reach/inventory-check. The local
operand configurations therefore omit it. Existing regions, their entries,
relocations and bounded indirect jumps remain; unsupported indirect-call
continuations remain a limit rather than being silently resolved. Each
report retains 59 walk gaps. No additional raw entry was invented.

Independent positive controls are the stores/read in 1000:04CE's known
body: file 18FB for literal 007F, 18E1/18F5 for DD66 and 18E5 for DD68.
Moving the first control to file 18FA causes explicit rejection. Each
successful report was repeated and is byte-identical to its repeat.

| Literal query (hex) | Verified memory operands | Verified other operands | Rejected overlaps | Unresolved boundaries |
| --- | --- | --- | --- | --- |
| 007F | 1 | 6 | 7 | 2 |
| DD66 | 4 | 0 | 0 | 0 |
| DD68 | 1 | 0 | 0 | 0 |

The sole verified 007F memory operand is the normalized-error word store
at 1000:04FB. The six verified other operands are immediate constants,
not memory uses of that word. DD66 has the two selector stores at
04E1/04F5 and two reads at 31D0/31D7. DD68 has the indexed map read at
04E5. These are literal representations: equal displacements alone do
not prove data-segment identity or exclude pointer-based aliases.

### Selector consumer

317F first queries file attributes and saves its result. Its flag-0100
branch, skipped by FND-SOUND-033's actual rb flags 8001, can reach the
DD66 reads. When the saved attribute result is FFFF, 31D0 compares DD66
against two. An unequal value is passed back to 04CE and returns through
the failure continuation. Equality instead constructs an attribute word
of zero or one from permission bit 0080, then continues into creation
handling. Thus the selector word, rather than the normalized word at
007F, controls this local distinction. Preservation of the selector
through prior children remains a condition; no native create was run.

### Retained boundary candidates

For 007F the unresolved candidate starts are file E115 and E116,
corresponding to 1C08:0C95 and 0C96. The former decodes a memory read,
the latter an immediate move. Bounded context from 0C90 shows an apparent
jump at 0C93 followed by the immediate move at 0C96. The operand walk
does not verify either candidate as an entry-path memory use. That
context alone does not establish a new boundary or justify discarding
the possible read. Both starts remain explicit obligations.

## Interpretation

This removes repeated open-ended exact-literal searching for these three
representations and identifies the verified selector consumer. The rb
route does not use that create-mode comparison under its retained flags.
Remaining obligations are the two normalized-word boundary candidates,
computed/pointer/implicit accesses, broader aliases and storage lifetime,
unsupported indirect transfers and actual child/interrupt preservation.
No global absence-of-consumers/writers claim follows from the counts.

## Alternatives

A zero verified-read count is not proof that normalized errors are unused:
the report has unresolved candidates and excluded representations. A
full byte-start scan of declared regions is not full executable semantics.
The prior builder's regions and established roots retain FND-EXE-572's
external/runtime limits. No complete reading or parity status is promoted.

## How to reproduce

Use the approved installed SOUND_DS.EXE identity of FND-SOUND-044. Generate
a local base with sound_ds_reach_config.py. For each query copy that base,
omit indirectCalls, set query.offset to 007F, DD66 or DD68, controls to the
file sites above, scanLimit=1000000 and limit=10000. Retain the builder's
instructionLimit and all other entry/region/transfer definitions. Run
report.mjs x86-operand-candidates with each local configuration. Replacing
the 007F control by 18FA must fail. Run resident_listing.py for
1000:317F..1000:333D to read the verified consumer.

Reports remain under GAME_DIR/analysis/disc-adv-survey-20261104/.
setup-error-operands-007f-report.json and its -repeat pair have SHA-256
d3257654c5283ca8943b105215ea51b544ca0238694099b7ccc798acdc4f4316;
dd66's pair has
7df1b744e31081a540f1ffdd3b6cc102b8c127b1b60b02370042f4fb6d25851a;
dd68's pair has
0f7df90d310718bf400f19ed4f1887b60355f10a84cc5764461e2fe413560c2c.
setup-error-consumer.txt and its -repeat pair have SHA-256
ae61db051a9c1e7db78a6a4af812d7eef482bd182521d6cc23625f20852ba2f5.
Local base/query/negative-control configurations are retained beside them.
Never commit these original-derived reports or listings.
