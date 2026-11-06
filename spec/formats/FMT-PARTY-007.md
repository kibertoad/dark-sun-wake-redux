---
id: FMT-PARTY-007
title: Class combination table (DATA 1001)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 576
text: false
definition: fmt_party_007.ksy
evidence: [FND-PARTY-058, FND-PARTY-068, FND-PARTY-264, FND-PARTY-265, FND-PARTY-267, FND-PARTY-268, FND-PARTY-269, FND-PARTY-270, FND-PARTY-271, FND-PARTY-272, FND-PARTY-273, FND-PARTY-274, FND-PARTY-275, FND-PARTY-276, FND-PARTY-277, FND-PARTY-278, FND-PARTY-279, FND-PARTY-281, FND-PARTY-282, FND-PARTY-283, FND-PARTY-284, FND-PARTY-285, FND-PARTY-286, FND-PARTY-287]
conflicting: []
split_with: []
related: [RULE-PARTY-009]
---

## Layout

The `DATA` resource 1001 of `RESOURCE.GFF`, with no header: one mask of classes for each origin,
first class and second class. Overlay 185 `+0000` returns the byte at 72 times the origin code
plus 9 times the first class less 1 plus the second class, the second class being 0 for none
[FND-PARTY-058], and the generation screen enables the class buttons from it [FND-PARTY-068].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 576 | `UINT8[8][8][9]` | `masks` | Indexed by `origin` (0 to 7, human first), then by the first class's generation code less 1 (0 to 7, Cleric first), then by the second class's generation code (1 to 8), or 0 when there is no second class. Each byte is a set of classes in the bits below: at second class 0, the classes that may be the second; otherwise, the classes that may be the third. | supported | FND-PARTY-058, FND-PARTY-068 |
| `0x240` | | | | Total size 576 | | |

## Enumerations and flags

### `masks`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x80` | `CLASS_BIT_CLERIC` | Cleric, generation code 1. | supported | FND-PARTY-068 |
| `0x40` | `CLASS_BIT_DRUID` | Druid, generation code 2. | supported | FND-PARTY-068 |
| `0x20` | `CLASS_BIT_FIGHTER` | Fighter, generation code 3. | supported | FND-PARTY-068 |
| `0x10` | `CLASS_BIT_GLADIATOR` | Gladiator, generation code 4. | supported | FND-PARTY-068 |
| `0x08` | `CLASS_BIT_PRESERVER` | Preserver, generation code 5. | supported | FND-PARTY-068 |
| `0x04` | `CLASS_BIT_PSIONICIST` | Psionicist, generation code 6. | supported | FND-PARTY-068 |
| `0x02` | `CLASS_BIT_RANGER` | Ranger, generation code 7. | supported | FND-PARTY-068 |
| `0x01` | `CLASS_BIT_THIEF` | Thief, generation code 8. | supported | FND-PARTY-068 |

## Differences between builds

None known.

## Coverage

The one `DATA` 1001 resource of the installed `RESOURCE.GFF`, at `0x186CB`; FND-PARTY-068 gives
every row that is not all 0. The two reads in overlay 183 `+0A3E` are the only ones found
[FND-PARTY-068].

## Open questions

- Which other code reads or writes this table, and with which index and
  bit interpretations (Q-PARTY-124)? FND-PARTY-058 records the shared
  accessor and its direct callers; FND-PARTY-068 identifies the two known
  DATA-1001 sites but explicitly excludes computed resource numbers.
  That census does not establish a whole-entry complete reading.
  FND-PARTY-264 now accounts for relocated/fixed-up nearby offset/segment
  pointer forms to the accessor and three shared resource-reader roots,
  including segment aliases and the three unrelated operand candidates.
  Runtime-computed or remotely assembled targets, separate request routes
  and computed resource numbers remain; the bounded Ghidra reference query
  misses a control and supplies no negative proof for those routes.
  FND-PARTY-265 excludes positive 1001 at the equipment hook's first
  guarded DATA-getter request, not at every nonliteral caller. The repeated
  call still needs selector/frame preservation. FND-PARTY-272 excludes
  two further name-helper requests by local bounds, leaving 83 other
  direct sites. Three of those admit formal word 1001 locally and need
  their actual incoming argument bindings. Indirect clients and separate
  DATA request routes remain.
  FND-PARTY-273 bounds all canonical effect-row DATA words for the
  object-dice route; valid row selection, actual row contents and selector
  preservation remain. It removes no whole getter site from those 83.
  FND-PARTY-274 extends the source bound to every wrapped byte-derived
  position, including negative indices, and excludes MZ relocation effects
  on the selected words. Runtime writers to the concrete resident aliases
  and other word-index producers remain; shipped zeros are not immutable.
  FND-PARTY-275 excludes twenty literal error-word stores as producers
  of 1001 and finds no fixed scalar operand at the -38 alias. It retains
  43 other inventory writer matches, 34 raw candidates and the excluded
  address/copy mechanisms; the 83-site count does not change.
  FND-PARTY-276 excludes sixteen more constant overlap stores and
  classifies sixteen raw candidates as operand-size-prefix interiors.
  Twenty-seven inventory value matches, eighteen unclassified raw
  candidates and possible entry into the classified prefixless forms
  remain, alongside the previously excluded address mechanisms.
  FND-PARTY-277 classifies the other eighteen enclosing instructions
  and finds no tested direct incoming call to any of the 34 interiors.
  Nine resident nearby-word pairs, jumps and excluded target forms remain;
  the missed Ghidra store control supplies no negative ownership proof.
  FND-PARTY-278 accounts for all nine pairs as unrelated operands or
  descriptor fields. Its 33-byte inventory-body repair changes this map's
  current hash but no function count or assignment. The retained writer
  census is unchanged; computed/copied/remote targets and jumps remain.
  FND-PARTY-279 answers the selection-count writer's ordinary value
  domain: its aliased byte excludes E9 under the named capacity/frame
  conditions. Those conditions remain alongside 26 other unclassified
  writer values; this is separate from the 36 constant-store exclusions.
  FND-PARTY-281 names six archive sites' pointer-byte and modular-count
  conditions, leaving twenty other unclassified values. Their reachable
  pointer/counter state and storage preservation remain unresolved.
  FND-PARTY-282 excludes the canonical timer segment-store result and
  names five drawing step/residual conditions, leaving fourteen other
  unclassified values. Caller values alone do not prove preservation.
  FND-PARTY-283 names three graphics stores' FONT pointer-byte and
  allocation-segment conditions, leaving eleven other unclassified
  values. Acquisition and valid-heap preservation remain unresolved.
  FND-PARTY-284 binds four event-window publications and their shared
  offset-high/segment-low byte condition, leaving seven unclassified
  values. Their distinct pointer producers and preservation remain open.
  FND-PARTY-285 classifies the final seven expressions: all 63 fixed
  inventory matches are now 36 literal exclusions, one canonical
  segment-store exclusion and 26 conditional expressions. Their named
  input/state admission and excluded writer/transfer mechanisms remain.
  FND-PARTY-267 excludes 105 direct literal non-DATA request setups from
  number tracing. FND-PARTY-268 classifies its thirteen variable sites
  into five local non-DATA choices, four known-caller non-DATA bindings
  and four initially unresolved type domains. FND-PARTY-269 bounds the
  chunk-loader setups and shipped window-child tags, and finds no nearby
  relocated pointer binding for the two unbound readers. Remaining type
  conditions are the tag builder's prefix/frame state, live child-tag
  provider/writers and excluded incoming target forms under Q-PARTY-124;
  the known DATA/1002 route stays separate.
  FND-PARTY-270 removes fixed-displacement scalar prefix writers and
  literal in-window pointer producers in the tested decodes, preserving
  computed/copied addresses, aliases, block operations and alternate
  streams. Main-entry DS/SS equality is already observed; preservation
  to these callers remains unresolved, not another startup experiment.
  FND-PARTY-271 binds both known direct window-registration routes to
  WIND acquisition and excludes their own selector writes under distinct
  valid storage. The live pointer, byte +F2, word +F3 and selected
  child type/number need preservation through reached helpers and aliases;
  the known source provider/table need no repeated census.
  The partial-reader ICON candidate omitted by the inventory remains an
  executable-ownership gap, not an asserted DATA consumer. The argument
  reporter's PUSH-CS limit at wrapper calls supplies no type classification.
  This entry describes the table representation and its index/bit
  meanings. It does not assert that every live resource load succeeds or
  that every later consumer retains valid storage. Any source, base,
  extent or byte-write effect that changes this interpretation remains
  within Q-PARTY-124; the full generation-screen behavior is separate.
  Closure objective selected after the ten-entry existing-evidence review:
  use Q-PARTY-124 for the remaining caller census, interpretation-affecting
  state producers, indirect transfers and source/base/extent evidence.
  FND-PARTY-286's unsuccessful immediate-caller attempt splits the three
  word-client caller domains to Q-PARTY-134, retaining eighty other
  direct sites and repeated-hook preservation under Q-PARTY-124.
  FND-PARTY-287 bounds the script producer's signed remainder at -999..999;
  its saved-word and later-call preservation remain Q-PARTY-134.
  Their queue conditions are authoritative; this map does not repeat it or
  declare any of those obligations complete.
  Ownership is tied to `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, LF-normalized
  SHA-256 `baac52b88e23a72661cfc2caaf4c19c8c08b1cdce2c23d04e3d2ad402f553477`.
  Known owned roots are `0x0006F640` (overlay 185 accessor, shared with
  RULE-PARTY-013's experience-table use) and `0x0006BA1E` (overlay 183
  selection caller, shared with RULE-PARTY-009 and generation level setting).
  Known reached shared services are `38FF:04AB` (resource acquisition)
  and `444C:0092` (release). Their relevant source/extent and byte effects
  belong to this obligation; their unrelated clients and general service
  success are not absorbed into the table contract. These four assignments
  describe ownership, not accepted readings. Further reached functions
  have no ownership assignment in this entry's map until identified by the
  census; the other 3,226 installed-DSUN rows remain unassigned here.
  The fresh pinned report measures 6,376 rows across six inventories;
  this entry's four-root map leaves 6,372 unassigned across that denominator,
  independently of the separate SAVE/COMBAT ownership assessment.
  Missing inventories remain `CD:CHARTRAN.EXE`, `CD:SOUND_DS.EXE` and
  `DOSBOX/DOSBox.exe`; no installed result establishes their behavior.
  There are no unmeasured inventories or coverage problems in that report,
  and no accepted complete-reading functions. A missing-inventory file is
  not asserted to read this table merely because it is unmeasured.
  First objective: remove the computed/indirect DATA-request caller gap by
  reusing the controlled direct-reader and shared-accessor reports, then
  classifying only unresolved target/argument sources. A bounded result
  must identify every remaining excluded mechanism; another literal-1001
  search cannot close this objective. The three closure-objective
  batches measure research, tooling, validation and handover separately.
