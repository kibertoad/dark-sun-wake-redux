---
id: FND-EXE-647
title: The finite overlay callback-pair census has 466 exact instruction-start matches in the retained declared-root model
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083387..0x00083392
tool: scientific-method-engine 19.10.0, Capstone distribution 5.0.9, pypcode 4.0.1, pinned Python 3.12.10; reach_config.py at 303c5558 and q033_header_pointer_segment_starts.json, q033_header_pointer_offset_starts.json, q033_header_pointer_consumer_starts.json from the commit adding this finding
environment: null
---

## Observation

FND-EXE-644/645 classify the local callback arguments of the finite
153 overlay fixup/push candidates from FND-EXE-567. Their argument
reader's backward decode does not independently establish every source
instruction boundary. This reading queries those exact boundaries in
the declared-root model of q007_dsun_apfm_census.json, retaining
FND-EXE-588's declaration evidence and exclusions and the later retained
report rather than assuming its old published counts still apply.

The current exporter at 303c5558 reproduces the retained configuration
in GAME_DIR/analysis/handler-census-20261106/ exactly. A fresh pinned
engine run reproduces that retained report exactly too. Its source
identity is the installed DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. It has 205 code regions, 7562 declared
far transfers, 157 jump-table declarations, 69 starts and 106 queried
targets. There are no leaves, stops or noReturn declarations. Inventory
entries partition routines; they are not additional execution roots.
The query also carries the retained indirect-call declarations.

The fresh baseline decodes 167165 instructions in 2398 ranges and
reaches 2196 routines. It retains 50 unresolved transfers, 146 interrupt
sites, five gaps and four contested decodes. It is not truncated and
does not exhaust its 588250 instruction limit. Its negativeUsable flag
is false. The 10000 report bound also remains unchanged.

The three q033 queries copy the same roots, transfer declarations and
other model inputs. Their only changes to the exported configuration
are the target sites and instruction controls. An attempted combined
467-target query was rejected by the engine's 256-target cap; no result
was taken from that rejected query. The three accepted cohorts are:

| Cohort | Queried sites | Exact reached instruction starts |
| --- | --- | --- |
| Segment push | Each candidate's source segment-fixup offset minus one | 153 of 153 |
| Offset push | Each candidate's source segment-fixup offset plus two | 153 of 153 |
| Consumers | 144 eventual callback-consumer calls and sixteen preceding structure-copy calls | 160 of 160 |

The consumer query also asks for overlay 196 +041E as an inside-byte
control. It is not a reached instruction start: the report explicitly
places it inside the reached call at +041D, as independently read in
FND-EXE-646. This is a decoder-boundary control, not proof that an
unread computed transfer could never enter an operand.

All three original instruction controls pass. Each cohort adds three
more controls: overlay 196 +0417, +041A and +041D, the two callback
pushes and their registration call. All six controls pass in every
cohort. The timer's former off-by-one label is therefore distinguishable
from the admitted instruction start. Range containment alone is not
used as a positive match.

Every cohort retains exactly the baseline walk counts, decoded ranges,
unresolved transfers, gaps and contested decodes; none is truncated or
at its instruction limit. Their target results attach each matched site
to a routine and a route from the declared roots. Such routes inherit
assumed returns, declared callback targets, continued interrupts and the
other report assumptions. They do not observe caller values or a live
frame. The unresolved/gap/contested rows remain and negativeUsable stays
false in every cohort.

## Interpretation

For this finite 153-pair set, instruction-start uncertainty from an
arbitrary backward decoder is replaced by positive exact-site matches
in the retained declared-root CFG model. Combined with FND-EXE-644/645,
the local callback-pair and argument classification need not be repeated
merely because its sites were initially candidates. This is a bounded
component of Q-EXE-033, not its general header reader/writer answer.

## Alternatives

- The roots, declared indirect-call destinations and table completeness
  are model inputs resting on their cited evidence. A positive path in
  this model does not admit every actual predecessor, segment, frame,
  callback history, native effect or asynchronous interaction.
- Fifty unresolved transfers, five gaps and four contested decodes can
  add behavior beyond the paths read. No negative whole-program result
  is supported, and these exclusions are not erased by all 466 positives.
- Actual destination/header aliasing, adjusted-base or block-copy writers,
  copied/runtime-computed pointers and reached header-relevant target
  effects remain Q-EXE-033 obligations. The reports do not establish
  FMT-EXE-003 or add a complete-reading declaration.
- The argument and copy-helper preservation conditions in FND-EXE-645
  remain; a matching call instruction does not establish its incoming
  stack bytes or the success of a child or native service.

## How to reproduce

Use the pinned evidence Python and engine 19.10.0. From the commit
adding this finding run `reach_config.py <dsun> <inventory> <config>
<query>` for each committed q033_header_pointer_*_starts.json query in
tools/research/exec-census/, using inventory
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Run
`python -I -m scientific_method_engine reach <config>` and retain each
report only in GAME_DIR. The input exporter checks the source identity.

The queries' targets are metadata-only exact sites. Rebuild their source
candidate set with segment_table_readers.py --pointer-sites as in
FND-EXE-645. For each candidate use source fixup minus one for the
segment push and plus two for the following offset push. First-call
records give the immediate call. For the sixteen 1000:0699 records also
query the call five bytes later. Consumer sites are unique; branch-join
consumers are already among the queried calls. Add 196+041E only as
the inside-byte control. Check the exact target.reached result, not
whether a site falls inside decodedRanges.

Compare exported configurations with q007_dsun_apfm_census.json's
configuration: only targets and instructionControls may differ. Check
all six instruction controls, every target row, unchanged decodedRanges,
unresolved/gaps/contested lists, counts, limits and false negativeUsable.
Any missing positive or changed model input requires reassessment.

Retained outputs in GAME_DIR/analysis/ are
header-pointer-reach-config-20261106.json and
header-pointer-reach-report-20261106.json for the baseline, and
header-pointer-boundary-<segment|offset|consumer>-config-20261106.json
and matching -report-20261106.json for the accepted cohorts.
