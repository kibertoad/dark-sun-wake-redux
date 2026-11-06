---
id: FND-CONFIG-226
title: Heap break and bound producers separate startup quantities and later updates from same-offset object and transfer-record stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:014C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:177C..1000:17F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000507EC..0x000507EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005084E..0x00050850
tool: window_operands.py, resident_listing.py and tools/evidence/report.mjs table at 60d45adb; Ghidra 12.1.3 read-only INST-single-20261103b; FND-EXE-509/550/548/549 producer and consumer reuse
environment: null
---

## Observation

This audits Q-CONFIG-013's break/bound producers. Source DSUN.EXE is
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. The installed
inventory's LF-normalized SHA-256 is
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.

### Named global stores and same-offset accesses

The controlled009E..00AA window emits123 inventoried operands and698
raw candidates outside their instruction starts. Among the listed accesses,
eight unindexed DS stores in segment1000 belong to the startup/bound code:

| Site | Field | Producer |
| --- | --- | --- |
| 0021 | 00A8 | Startup's retained incoming word |
| 00A3 | 00A0 | Selected startup segment sum |
| 00A7 | 00A4 | Same selected sum |
| 0107 | 00A8 | Conditional native allocation result plus one |
| 179B | 00A4 | Admitted updater's incoming segment |
| 179E | 00A2 | Admitted updater's incoming offset |
| 17E2 | 00A8 | Failed updater's adjusted returned bound |
| 17E5 | 00A6 | Failed updater's explicit zero |

FND-EXE-509/550 and FND-EXE-549 supply independent controls for all
eight. A separate3908..390A window emits two listed operands, both
in177C: a cache comparison and the17D8 cache store, with one raw
indexed candidate inside an inventoried instruction. FND-EXE-549's
cache update is the positive write control.

Same displacements elsewhere do not establish more heap-global stores.
FND-EXE-394/397 read nine listed stores in15F3's probe/transfer bodies
whose own DS is assigned from CS. Under those bindings they update
15F3 records, not the modeled57E0 data fields. Eleven listed ES:BX
stores in3A8E's window routines and two in409B's EBOX cleanup use
supplied objects (FND-COMBAT-093, FND-CONFIG-172). They are not direct
absolute heap-field accesses. Object/segment aliases remain unresolved;
this classification does not assert physical disjointness.

The raw candidates are retained, not promoted to instructions or discarded
as a complete set of impossible writers. Computed/indexed destinations,
bulk writes, native effects and segment aliases remain outside this
explicit-displacement census. Shipped zero fields and the startup zero
fill are distinct: FND-EXE-550 places these fields below that fill's start.

### Checked shipped quantities select a bounded startup branch

The bounded table reporter verifies the source fingerprint and reads
two unsigned words: DS:37EC at file507EC is zero, and DS:384E at
file5084E is4096. These are shipped inputs, not guaranteed later values.

Startup clamps the latter quantity to at least512, then adds A4EC
and the current extra quantity, checking carry after each word addition.
Under unchanged shipped quantities this gives B4EC, which is shifted
right four and incremented to B4F paragraphs. Let P denote the retained
incoming top-segment word minus current DS, at word width. The code
requires unsigned P at least B4F or transfers to its startup failure path.

The later zero-extra test selects DI=min(P,1000 hexadecimal), rather
than retaining B4F unconditionally. Hence on this selected continuation
B4F <= DI <=1000. Startup writes word(current DS + DI) into both00A0
and00A4. If the lower/current offset words009E/00A2 retain their shipped
zeros, those two pairs are equal at that publication boundary. No actual
unchanged-field or native segment-preservation invariant is inferred.

For modeled DS57E0, the segment sum lies from632F through67E0 and
does not wrap. Different actual segments require their own binding and
wrap assessment. FND-EXE-509 establishes that DS is assigned from a
post-interrupt register, so the modeled segment alone does not admit
the native equality. FND-EXE-550's later conditional DOS request can
replace00A8 before startup dispatch; the upper bound is not frozen by
this earlier lower/current publication.

### Later bound failure changes future admission

FND-EXE-548 reads current00A2/00A4 and compares each candidate with
lower009E/00A0 and upper00A6/00A8. FND-EXE-549 connects the updater's
successful cache/native branch to the current-pair stores, but its failed
branch instead writes upper segment00A8 and clears upper offset00A6,
without committing the requested current pair. That branch changes the
bound used by a later request; a failure is not a no-state-change result.

The current Ghidra view contains startup and177C and agrees with the
quantity tests and ordered stores. Instructions establish widths, carry,
unsigned comparisons, native-result use and the DS-relative destinations;
inferred pointer types are not used as runtime segment evidence.

## Interpretation

Q-CONFIG-013 now has specific startup and later bound/current/cache
producers, checked shipped quantities and a selected startup arithmetic
range. Those sources need not be rediscovered for each allocated buffer.
Remaining admission includes actual P/DS and other runtime fields,
intervening/native changes, selected callback paths, computed/bulk/object
aliases and the header/free-link state and backing outcomes. No valid
live heap, allocation success, complete writer census, complete reading
or queue closure follows.

## Alternatives

Counting every displacement match as a heap-global access would conflate
CS-bound transfer records and ES:BX objects with game data. Treating the
shipped zero extra quantity as permanent would exclude later writers
without evidence. Keeping the lower/current pairs equal forever would
ignore their later distinct updates and changed upper-bound failure path.

## How to reproduce

Run window_operands.py on the supported executable/inventory with
009E..00AA and3908..390A. Retain heap-break-bound-writer-window.txt
under GAME_DIR/analysis/handler-census-20261106/ with all raw candidates.
Read resident_listing.py1000:0000..00B0 and1000:00D2..0110; retain
heap-startup-bound-publications.txt and reuse FND-EXE-509/550/549.

The local heap-startup-quantity-config.json selects source DSUN.EXE
with the fingerprint above, start507EC, count/limit one, stride100,
and unsigned two-byte fields at offsets zero and98. Its count evidence
is FND-EXE-509's two startup operands. Run tools/evidence/report.mjs
table on it and retain heap-startup-quantity-report.json. Require the
zero/4096 controls before calculating B4F and the selected DI range.

In read-only INST-single-20261103b, program DSUN.EXE, run
ReportDecompileMatches.java at1000:0000 with384e,37ec,00a0,00a4 and
at1000:177C with3908,00a8. Require both function markers; retain
heap-startup-bound-decompile.log in the same local store. Original-derived
reports remain out of Git. No original game or emulated call was run.
