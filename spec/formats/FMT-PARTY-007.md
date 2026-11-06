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
evidence: [FND-PARTY-058, FND-PARTY-068, FND-PARTY-264, FND-PARTY-265, FND-PARTY-267, FND-PARTY-268, FND-PARTY-269, FND-PARTY-270, FND-PARTY-271, FND-PARTY-272, FND-PARTY-273, FND-PARTY-274, FND-PARTY-275, FND-PARTY-276, FND-PARTY-277, FND-PARTY-278, FND-PARTY-279, FND-PARTY-281, FND-PARTY-282, FND-PARTY-283, FND-PARTY-284, FND-PARTY-285, FND-PARTY-286, FND-PARTY-287, FND-PARTY-288, FND-PARTY-289, FND-PARTY-290, FND-PARTY-291, FND-PARTY-292, FND-PARTY-293, FND-PARTY-294, FND-PARTY-295, FND-PARTY-296, FND-PARTY-297, FND-PARTY-298, FND-PARTY-299, FND-PARTY-300, FND-PARTY-301, FND-PARTY-302, FND-PARTY-303, FND-PARTY-304, FND-PARTY-305, FND-PARTY-306, FND-PARTY-307, FND-PARTY-308, FND-PARTY-309, FND-PARTY-310, FND-PARTY-311, FND-PARTY-312, FND-PARTY-313, FND-PARTY-314, FND-PARTY-315, FND-PARTY-316, FND-PARTY-317, FND-PARTY-318, FND-PARTY-319, FND-PARTY-320, FND-PARTY-321, FND-PARTY-322, FND-PARTY-323, FND-PARTY-324, FND-PARTY-325, FND-PARTY-326, FND-PARTY-327, FND-COMBAT-087, FND-COMBAT-088, FND-COMBAT-090, FND-COMBAT-091, FND-COMBAT-092, FND-COMBAT-093, FND-PARTY-328, FND-PARTY-329, FND-PARTY-330, FND-PARTY-331, FND-PARTY-337, FND-PARTY-338, FND-PARTY-339, FND-PARTY-340, FND-PARTY-341]
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
  FND-PARTY-288 bounds two reflected-cast producer components; the
  alternative input and named DI/queue/kind/lifetime conditions remain.
  FND-PARTY-289 removes leaf/cache-hit DI questions and identifies the
  effect helper's saved word and getter-miss child contracts still needed.
  FND-PARTY-290 resolves lookup and allocation restoration mechanisms;
  named saved-memory, guard and transfer-interface conditions remain.
  FND-PARTY-291 resolves the transfer wrappers' local DI mechanisms;
  reached AH=3F/40/42 interrupt contracts and saved-word lifetime remain.
  FND-PARTY-292 supplies the exact built-in runtime source's ordinary-file
  DI contract; runtime routing, release correspondence and memory/handler
  admission remain. Public source does not establish original-platform behavior.
  FND-PARTY-293 excludes the verified known setter-call branches as
  vector-21 replacement requests. Other mechanisms remain; its newly
  identified 123-byte handler is now declared and inventoried after the
  isolated exporter repair in docs/EVIDENCE-TOOLS.md. This changes the
  mapping denominator, not accepted readings or the callback obligations.
  FND-PARTY-294 reuses fixed startup and known dynamic-service arguments;
  the loader live selector and uncensused routing mechanisms remain open.
  FND-PARTY-295 binds the explicit-displacement indexed writer to DS:19C5;
  FND-PARTY-296 excludes its known offset-four allocation representation
  by address residues; alternate pointer histories and broader writes remain.
  FND-PARTY-297 excludes the mask-10 reflection flag branch for an intact
  DATA 1001 payload; getter binding/failure and mask-20 preservation remain.
  FND-PARTY-298 identifies the failed-miss flag as vector-four offset bit
  fifteen; actual vector admission and later reflection checks remain open.
  FND-PARTY-299 resolves the later helper result for intact and default-vector
  null copies; actual outcome transitions and retained numbers remain open.
  FND-PARTY-300 bounds explicit cache-key operands and the local-copy
  lifetime; actual key/number retention, hit/retry content and frames remain.
  Q-PARTY-134 keeps original producer domains; its independent getter-pair,
  helper-preservation and queue-handoff obligations are Q-PARTY-135,
  Q-PARTY-136 and Q-PARTY-137 respectively. This split answers none.
  FND-PARTY-301 maps all three encoded range-wrapper producer bindings;
  their intrinsic numeric exclusions retain Q-PARTY-136's storage contract.
  FND-PARTY-302 assesses the remaining seven encoded candidates: known
  case-four paths bypass the client and two cursor paths share DS:9BE2.
  Actual stored-number history and alternate entry/frame admission remain.
  FND-PARTY-303 binds both cursor SI inputs to exact source-word or
  non-list-mode freshness conditions; index/state and retention remain open.
  FND-PARTY-304 settles both encoded callers' own mode-zero count clears;
  actual intervening and later state histories remain Q-PARTY-134.
  FND-PARTY-305 bounds intended list/shipped last-cast registration indices;
  changed bindings, dispatch preservation and source histories remain.
  FND-PARTY-306 settles the known callback's own active/empty refresh split
  and build-before-count ordering; callback admission and later state remain.
  FND-PARTY-307 settles member publication before redraw/empty-record testing
  and the cast handler's later zero-mode clear; interval/later admission remains.
  FND-PARTY-308 resolves the state brackets' own counter/refresh behavior;
  actual reached graphics callback targets and interval admission remain.
  FND-PARTY-309 excludes those graphics callbacks for unchanged shipped
  APFM fields under known mode writers; changed fields/histories remain.
  FND-PARTY-310 classifies the finite ES field-writer window; remaining
  graphics-field admission names specific backing, setter and excluded-write forms.
  Existing FND-COMBAT-088/090/091 settle named conditional constructor,
  paint and rebased text-write effects on the callback region. They are reused
  as dependency contracts, not accepted whole-entry readings. Their storage
  and input conditions remain Q-PARTY-134; FND-COMBAT-087/092/093 retain
  distinct lookup/hit records and publication before constructor success.
  FND-PARTY-311 excludes tested incoming forms of the lookup setter and
  classifies four selector callers, retaining a concrete menu-cleared pointer
  with separate-tag, null-state memory and interval-admission conditions.
  FND-PARTY-312 excludes both encoded menu-selection input routes for
  unchanged shipped graph/payload bindings; changed MENU-producing records
  and excluded transfers remain Q-PARTY-134, not an assumed native event.
  FND-PARTY-313 classifies the cursor-number overlap and known local-pointer
  uses; actual publication history and equality across later reloads remain.
  FND-PARTY-314 removes an ordinary returning guard-DI question under its
  forced-zero/frame conditions; changed guard rejoining remains Q-PARTY-136.
  FND-PARTY-315 classifies the five established stack-limit-window writers;
  actual setter inputs, backing aliases and excluded writes remain Q-PARTY-136.
  FND-PARTY-316 supplies the timer's saved-DI/captured-stack restoration
  contract and excludes thirteen trampoline-data SS decodings on their known route.
  FND-PARTY-317 classifies the literal capture-word window and fixed callback
  writes; actual timer/frame backing, selected sound-record aliases, changed
  driver/table bindings, BIOS-chain effects and excluded writes remain.
  FND-PARTY-318 reuses the shipped sound-record pointer and known ADV write
  domains; excluded pointer histories and actual frame/driver backing remain.
  FND-PARTY-319 identifies IVT capture, nesting reset and IRET flag-restoration
  frames; actual old-vector/reset admission and excluded writers remain.
  FND-PARTY-320 classifies both known reset callers and the sound-ready
  gate/clear; actual re-entry, mode/ready histories and bypasses remain.
  FND-PARTY-321 distinguishes the ordinary shutdown gate from the selected
  error route's conditional absence of a finite ordinary return. Reentrant
  exit readers and changed-frame/nonlocal rejoining remain unresolved.
  FND-PARTY-322 binds the known shutdown root to its exact-one exit-callback
  route; changed admission and reentrant exit readers remain Q-PARTY-136.
  FND-PARTY-323 excludes the named unchanged alias-pattern pair because
  its supposed offset word is itself relocated; changed/computed routes remain.
  FND-PARTY-324 classifies the literal mode writers and their conditional
  zero/one domain; actual state history and excluded write forms remain.
  FND-PARTY-325 classifies three raw mode-write candidates as operand-interior
  decodings on their known streams; excluded alternate entries remain.
  FND-PARTY-326 excludes same-overlay unprefixed near-transfer patterns
  to the two overlay candidates with six source controls; far, indirect,
  computed, modified and other excluded entry forms remain unresolved.
  FND-PARTY-327 excludes modeled declared-segment/raw-offset pairs for the
  resident candidate; runtime-built, distant-pair and transformed forms remain.
  Newly identified inventory rows 4734:010F, 49DE:000F and 0x00067902
  remain unassigned in the current metadata plan; no ownership is inferred.
  FND-PARTY-328 bounds the admitted queue index transitions and corrects
  the old unconditional kind-zero claim; actual kinds/backing remain Q-PARTY-137.
  FND-PARTY-329 bounds the known shipped VECT request after the queue/kind
  spans; changed source/pointer/frame and other writers remain Q-PARTY-137.
  FND-PARTY-330 classifies fixed spell/save request ranges; SPST slot/length,
  changed SAVE records/pointers and variable backing remain Q-PARTY-137.
  FND-PARTY-331 binds both direct SPST callers: retained byte-loop inputs
  differ from the selected-load full-word reload; their actual state remains Q-PARTY-137.
  FND-PARTY-337 binds the reflected direct-return root to its first drain;
  normal hit-tail/resource helpers are skipped there. Both selected kinds
  forward the saved number; actual backing/transfers remain Q-PARTY-137.
  FND-PARTY-338 supplies the admitted header-return first-frame rejoin and
  default notification leaf; actual graph/backing/loader effects remain Q-PARTY-137.
  FND-PARTY-339 classifies both cache-hook copy backends and descriptor/link
  writes; actual EMS window, native move and backing remain Q-PARTY-137.
  FND-PARTY-340 observes the strict play-ready baseline with EMS selected,
  an empty queue and disjoint buffer/SS windows; later preservation remains Q-PARTY-137.
  Q-PARTY-137 now keeps the first reflected handoff; Q-PARTY-138 preserves
  other roots, client-side enqueue/nested drain and later iterations. This
  evidence-boundary split answers neither item and drops no writer obligation.
  FND-PARTY-341 records the post-client index transition and cast tail;
  actual enqueue/nested-drain admission remains Q-PARTY-138.
  Their queue conditions are authoritative; this map does not repeat it or
  declare any of those obligations complete.
  The separate metadata-only ownership plan
  `docs/research-closure-cast-window.json` assigns the already identified
  cast-window and APFM rows to PARTY/UI, including shared selection callbacks.
  It also assigns the identified timer restoration/reset, sound callback and
  initialization rows to PARTY/TIME/SOUND from FND-PARTY-316 through 320.
  The current map assigns 54 unique rows, 25 shared; 6959 remain unassigned
  across the eight-inventory denominator. Sixteen open metadata obligations
  include eight newly explicit TIME/SOUND checklist components of Q-PARTY-136,
  not eight new research questions or answered dependencies. Accepted readings
  remain zero; the raw reporter lacks only the owner-excluded DOSBox inventory. This map's per-area counts
  are non-additive and do not replace the original four-root map above.
  Its open obligations reference Q-PARTY-134/135/136/137/138; it does not replace
  this entry's four-root map, the separate SAVE/COMBAT map or their denominators.
  Assignment and cited coverage do not establish complete readings.
  Ownership is tied to `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, LF-normalized
  SHA-256 `820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2`.
  Known owned roots are `0x0006F640` (overlay 185 accessor, shared with
  RULE-PARTY-013's experience-table use) and `0x0006BA1E` (overlay 183
  selection caller, shared with RULE-PARTY-009 and generation level setting).
  Known reached shared services are `38FF:04AB` (resource acquisition)
  and `444C:0092` (release). Their relevant source/extent and byte effects
  belong to this obligation; their unrelated clients and general service
  success are not absorbed into the table contract. These four assignments
  describe ownership, not accepted readings. Further reached functions
  have no ownership assignment in this entry's map until identified by the
  census; the other 3,227 installed-DSUN rows remain unassigned here.
  The current pinned report measures 7,013 rows across eight inventories;
  this entry's four-root map leaves 7,009 unassigned across that denominator,
  independently of the separate SAVE/COMBAT ownership assessment.
  The byte-identical disc CHARTRAN/SOUND_DS metadata associations reuse the
  existing snapshots as docs/EVIDENCE-TOOLS.md describes; no independent
  disc reading follows. The raw reporter still lacks `DOSBOX/DOSBox.exe`,
  excluded from game research by the recorded owner scope decision. No
  missing-inventory file is inferred to read the table.
  There are no unmeasured inventories or coverage problems in that report,
  and no accepted complete-reading functions. A missing-inventory file is
  not asserted to read this table merely because it is unmeasured.
  First objective: remove the computed/indirect DATA-request caller gap by
  reusing the controlled direct-reader and shared-accessor reports, then
  classifying only unresolved target/argument sources. A bounded result
  must identify every remaining excluded mechanism; another literal-1001
  search cannot close this objective. The three closure-objective
  batches measure research, tooling, validation and handover separately.
