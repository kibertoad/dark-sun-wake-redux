# SOUND

Next ID: Q-SOUND-013

## Static

- Q-SOUND-011. FMT-SOUND-002: Which original code reads or writes DJ.DAT
  and the loaded count, retry word and six-byte record array, including
  aliases and indirect accesses outside the known loader/selector?
  Settles it: actual BP/frame-to-DS backing and interval exclusions for
  FND-SOUND-064's four computed list writers and DS/SS read paths, changed
  count/list-byte admission beyond FND-SOUND-065's nonempty shipped lists,
  plus other computed/aliased
  filename/table addresses, overlapping writes, indirect accesses and
  alternate-entry mechanisms; identify
  actual field/extent bindings beyond the controlled literal component,
  with each computed or indirect undecided access retained; relate any
  additional access to the format's width, encoding or field interpretation.
  Tried: the existing-evidence assessment of FND-SOUND-011/012/013 locates
  2834:043B acquisition, 2834:000C selection, 2834:0665 song forwarding and
  4A32:0011 dispatch, but those findings do not establish an exhaustive
  loaded-table reader/writer census. Generic I/O success and unrelated
  driver output are not added as layout obligations. FND-SOUND-061 classifies
  the 32 listed field-window operands and local loader branches, including
  signed-count extent, partial-header mutation and non-clearing cleanup.
  FND-SOUND-062 classifies all eight field candidates as two loader
  instruction interiors and six combat-table data locations. Its exact
  filename query retains four interior candidates; wider-query history and
  excluded aliases/indirects remain explicit. FND-SOUND-063 classifies all
  four exact-name candidates as instruction interiors in unrelated argument,
  direction and state-field arithmetic. No raw field/exact-name candidate
  remains unclassified. FND-SOUND-064 identifies local record/list address
  equations, four computed byte stores, conditional 38-byte list capacities
  and direct procedure callers. Actual frame/DS/ES and list population,
  plus other excluded mechanisms, remain. FND-SOUND-065 establishes
  shipped list populations: nine mode-one fallbacks and mode-three lists
  of four/three/three, including a populated health-eight first-list read
  under the recorded frame/binding conditions. Actual preservation and
  changed input domains remain. FND-SOUND-066 bounds the known RNG
  children: local DS/ES preservation, fixed DS:384A..384E plus stack
  writes, admitted index ranges and no-draw chance ten. Retain actual
  state/table/list/frame aliasing and external interference rather than
  unspecified effects of those helpers. Blocks: complete
  FMT-SOUND-002 reading; slice 7.

- Q-SOUND-002. FMT-SOUND-001, RULE-SOUND-001, RULE-SOUND-002, RULE-VIDEO-001: How does the sound library play a
  voice file or a `BVOC` resource: at what rate and volume, through which driver, and what do the
  library routines the two rules call check, play and stop? Settles it: actual name/file/configuration/handle histories and preparation-state retention after FND-SOUND-054's `4611:0177` contract,
  actual card/mode/record/frame admission for `4654:04FE` and its `4654:056A` child after FND-SOUND-049's local gate/remap/forwarding contract, applying FND-SOUND-050's recorded type gates and default-selector meaning of 6,000/6,001 while retaining actual state/register/frame correspondence, explicit-value domains and downstream driver/output semantics, actual refill count/buffer/state admission and downstream submission effects after FND-SOUND-056's descriptor staging/frame contracts and FND-SOUND-055's active `4611:0051` contract and FND-CONFIG-166's stop/early-status contracts, actual stop/release admission and child effects, actual driver/handle admission, excluded state writers, adjacent buffer/mode histories and interrupt/child effects after FND-SOUND-053's literal state-transition census and FND-SOUND-052's local `49E9:00FD`/`49E9:0142` query/stop contracts, `38FF:05B5` and `56BD:0034`, and actual preference/record/configuration admission, child effects and driver semantics for `45E1:0097`, whose byte clamp and record-relative adjustment FND-SOUND-051 records. Tried: searches of both executables for a VOC signature, a `.VOC`
  extension, a helper launch and a BIOS wait (FND-SOUND-003, FND-SOUND-005), the helper's port
  output (FND-SOUND-004), and the effect and speech routines (FND-SOUND-007, FND-SOUND-008), which
  show which files play but not how. FND-SOUND-049 removes the unknown local checks, unsigned index boundary and argument forwarding of 4654:04FE; actual state/storage correspondence and downstream playback/units remain. FND-SOUND-050 settles 6000/6001 as record-default selectors across the three parameter children and classifies 056A's local type gates, publications and conditional zero/three exclusion. FND-SOUND-051 removes the unread preference-helper contract: it clamps the low byte, can request stream activity and stores its difference from record +0C for the default path. FND-SOUND-052 settles the local state-two tests, repeated query and unverified stop return, including the conditional DIGPAK software-state/interrupt path. FND-SOUND-053 classifies eight literal software-state stores, their local conditions and the start-child overwrite of two with three; excluded writers, actual admission and child/interrupt effects remain. FND-SOUND-054 settles file-start gates, bounded preliminary scan, preparation publications and persistent-open ordering; reuse FND-CONFIG-166 for stop and early-status contracts. FND-SOUND-055 settles active refill ordering, requested-count debit, toggle, AX-only short-read classification and final status tests. FND-SOUND-056 settles conditional DIGPAK descriptor staging and submission-wrapper ordering; retain actual count/capacity, storage/frame histories, actual submission/interrupt admission and effects after FND-SOUND-057's arithmetic/chunk-request and local frame contracts, actual capability result/cache epoch and excluded writers after FND-SOUND-058, explicit-value application, driver meanings and output, preserving FND-SOUND-059's post-submission DS/SI conditions and FND-SOUND-060's eleven controlled direct sites, including five AL-poll loops and the 201 offset-loop repeat route. Blocks: slice 7.
- Q-SOUND-003. RULE-SOUND-001, RULE-SOUND-002, RULE-VIDEO-001: Which effects do the 19 other callers of the
  sound-effect routine play, and when; which lines do overlays 187 and 204 speak; and what are
  `g_57E0_0D9C`, `g_57E0_6554`, `g_57E0_14E8`, `g_57E0_4263` and `g_57E0_4275` for? Settles it:
  the callers FND-SOUND-007 and FND-SOUND-008 list, and the writers of those globals. Blocks:
  slice 7.
- Q-SOUND-004. RULE-SOUND-003, FMT-SOUND-002, RULE-VIDEO-002: What chooses the music outside combat after
  startup, the 18 mode-2 songs of `DJ.DAT` that the selector never takes, and how does a disc
  track or an FM song start and end? Settles it: the writers of `music_mode` through a pointer or
  a register, actual track/response/status/table/frame admission and external
  handler effects after FND-SOUND-067's local `2660:0250`/`2660:035E` contracts, actual FM tag/configuration, failure-flag and 3458/344A pointer
  source/block extents and lifetime after FND-SOUND-082's separate-argument
  setup binding and FND-SOUND-069's acquisition/cleanup/start contracts.
  Reuse FND-SOUND-096's repeated fixed startup callback/metadata/allocation/
  final-write admission through the first music gate. It supplies no later
  track-start, fresh per-track response, heap extent, preservation or audible
  result; those remain rather than repeating initial callback admission.
  FND-SOUND-083 settles absence of the four requested song tags in the validated
  installed archive set; retain other media/runtime records and bypass/other
  setup entries rather than parsing nonexistent installed payloads. Also retain
  actual status/query/stop/release and target/frame admission
  after FND-SOUND-068's local `4ABF:01E5`/`4A32:0185` contracts, and actual region-argument/state histories for the five literal
  region sites in FND-SOUND-071, including overlay 182 +15FC/+16B5,
  plus the computed SAVE/18 producer identified by FND-SOUND-074. The
  four known transition-call argument tuples and result consumers are
  settled by FND-SOUND-074; remaining region domains are actual source-table/
  resource lengths and bytes, incoming frame contents, segment/frame
  retention, state-4 admission and excluded callers/writers. FND-SOUND-075
  settles the local command-line branch: ordered key gating and a low-word
  numeric store without a region bound; actual argv/segment admission and
  subsequent startup preservation remain. FND-SOUND-076 settles known
  health aggregation/override and tick forwarding; retain actual record
  input histories, script ordering, backing and excluded health writers
  rather than an assumed 1..10 field domain. FND-SOUND-077 identifies
  37 shipped literal script writers: 27 fixed values and ten local-variable
  inputs. FND-SOUND-078 bounds eight adjacent random-assignment paths and
  identifies the two carried-local producer sets. Remaining script input
  scope is admission/backing and entries outside FND-SOUND-079's tested
  top-level literal transfers for those eight paths; GPL/15 0131 retains
  its 01B3 helper/returned-local binding and GPL/18 0769 its producer
  calls and local-five selection. FND-SOUND-080 settles the fixed-snapshot
  interface and excludes explicit GPL/21 nested backup/direct script calls;
  retain snapshot 4E71:0216..0218, current 4C13:0337 backing, other
  native/callback writers and the restore continuation. FND-SOUND-081
  classifies all four nearby saved-word alias-pair candidates and binds
  allocation/current-pointer publication plus the returning +0422 cleanup
  clear; remaining lifetime scope includes actual release admission between
  save and restore, other aliases and excluded computed/bulk writers,
  GPL/27 01C8 dynamic selection, Q-SCRIPT-024's GPL/37 decode-gap
  admission/error continuation, actual
  scheduling and excluded script/native producers. Tried: a search for stores to `music_mode`, which finds only `2834:000C` and the two
  speech calls of `2834:0001` (FND-SOUND-071). FND-SOUND-067 records
  local disc-start, response-table writes and request-status ordering; track
  publication ignores start-request failure. Actual handler/output, table
  lifetime and the other named behavior remain. FND-SOUND-068 records
  busy/stop branches: FFFF status can satisfy the busy bit test, while
  stop has conditional disc-target and sequence cleanup/publication paths.
  FND-SOUND-069 settles local FM tag selection, resource-interface reuse,
  prior cleanup and start forwarding. Actual load failure does not locally
  set the tested flag; retain excluded flag producers, source/sequence pointer
  extents/lifetimes and query/handle/release/output evidence. FND-SOUND-082
  resolves the local 3458/344A relationship as separate arguments to driver
  selector 0097 and records literal allocation/release components. Actual
  driver/block/source binding, patch-loop termination and excluded pointer
  writers remain. FND-SOUND-071 corrects the literal region census to five
  stores and identifies the two overlay-182 transition branches; actual
  caller/state domains and excluded writers remain. FND-SOUND-072 removes
  three raw instruction interiors from the unclassified mode/region set.
  The remaining raw candidate and its ownership move to Q-SOUND-012
  with FND-SOUND-073; this split closes no part by declaration.
  FND-SOUND-074 binds all four known direct transition calls, distinguishes
  fresh restored-region reloads from incoming words and separates the
  reader-failure option from the preparation byte. Reuse its connected
  wrappers and child interfaces; native input/preservation histories remain.
  Blocks: slice 7.
- Q-SOUND-012. RULE-SOUND-003, FMT-SOUND-002: Does an established path
  enter the 1BF3:4889..4C09 ownership gap in a way that makes the raw
  488B candidate a current-region access, or binds the provisional 490D
  reader and its lookup-data interpretation? Settles it: establish incoming
  ownership through the remaining computed/paragraph-aliased, constructed
  target or alternative-entry mechanisms, then determine the reached
  segment/index domains and whether the candidate reads game region state.
  This could add an access or exclude the candidate from the region census;
  it does not require unrelated playback output. Tried: split from
  Q-SOUND-004 after FND-SOUND-072/073 classify three interiors and exclude
  the tested exact-word bindings and resident 1BF3 rel8/rel16 forms.
  Reuse their controls/reports; do not repeat those searches without changed
  inputs. Preserve omitted ranges, other entries and modified code.
  Blocks: slice 7.
- Q-SOUND-005. RULE-SOUND-003: Does the game ever play a `PLYL` playlist or the `CSEQ` sequence,
  and through which code? Settles it: a caller of `2660:0004`, a writer of the far pointers at
  `DS:3496` and `DS:349A` that `2660:01AF` sets, and the resource lookups of the FM or MIDI branch
  of `4A32:0011`. Tried: searches of `DSUN.EXE` for the tags and for far calls to the playlist
  routines, which find none (FND-SOUND-014, FND-SOUND-015). Blocks: none.

- Q-SOUND-007. FMT-SOUND-004: Which remaining setters/callers and state
  changes affect mode/current settings through actual driver use?
  Settles it: remaining mode writers and incoming settings-copy/pointer-setter
  paths beyond the recorded combined/separate/fallback routes, and current-record
  lifetime/aliases through later driver children/digital use, and remaining
  filename/resource loader paths.
  Tried: FND-SOUND-019 validates all nineteen common directory envelopes;
  their structure is removed from Settles it. FND-EXE-491/492 identify the
  two filename consumers and setup batch; FND-EXE-646/590 bind the shipped
  game selectors to ADV eleven/eight and their disc-file matches. Those
  known consumers/default matches do not settle the remaining producers.
  Tried: FND-SOUND-020 binds all four name tags to inline parsed fields
  and the complete local real/protected-mode output-copy alternatives.
  It also binds DS:E01B through the initial pointer setter to the conditional
  first name helper. These local producers are removed from Settles it;
  actual record/mode selection, other setters and later preservation remain.
  Tried: FND-SOUND-021 settles local header actions, combined/separate
  group-to-writer and fallback-index routes, label/record stride separation
  and exact FFFF caller gates. These local producers are removed from
  Settles it. Independently answerable dialog row/index/storage admission
  moves to Q-SOUND-010; preserve later state/callee and other-caller gaps here.
  Blocks: complete loader interpretation of FMT-SOUND-004.
- Q-SOUND-010. FMT-SOUND-004, FMT-CONFIG-002: What actual row/index,
  storage and continuation contract admits setup's list-choice result?
  Settles it: intermediate and later group/label lifetime beyond FND-SOUND-100's
  post-label endpoint; heap/break/state history and remaining vector/chooser
  allocations;
  far-zero storage/history from101's endpoint to use; intervening190F guard
  complete input delivery and later continuation/object/frame retention
  after106's observed notice return with pending event,105's fixed native
  pre-poll profile and102's caller binding;
  simultaneous allocation lifetime/alias and deeper-stack admission after
  103's prologue/earlier-block intersection and105's wrapped-frame endpoint;
  chooser BIOS SI/DI/ES, port progress,
  actual mode/extent retention after099/101, scrolling scratch/coordinate and
  backing/frame admission after097/098, and
  native reachability; and nonpositive, larger or other-label profiles
  outside FND-SOUND-022's conditional local proof. Local cardinality/index,
  installed source profile, count conversion, allocation rounding/clearing
  and plain formatter are FND-SOUND-022 through FND-SOUND-027.
  Tried: FND-SOUND-041 verifies all populated record bytes and the three
  predicted size headers in two fresh runs. Endpoint record coherence is
  removed from Settles it; later lifetime/label/vector/chooser work remains.
  Tried: FND-SOUND-040 binds the normal group-header size word and
  capacities for counts 13/-1. The native word values, record contents
  and later label/vector/chooser admission remain distinct.
  Tried: FND-SOUND-039 observes two fresh successful parser returns with
  counts 13, 13, -1 and nonzero offset-four group pointers. The native
  negative-count group result is no longer hypothetical at that endpoint;
  record bytes/extents and later label/vector/chooser admission remain.
  Tried: split from Q-SOUND-007 after FND-SOUND-021
  identifies the wrapper's unconditional first-label read, entry-only negative
  index clamp, layout FFFF path and normal current-index return. A source
  record count or the callers' exact FFFF checks do not settle those bounds.
  Tried: FND-SOUND-022 settles local cardinality/index arithmetic for the
  explicit small printable-label profile, including the real LF separators,
  rounded row storage and conditional retained-vector prefix. Those local
  computations are removed from Settles it for that profile. Actual profile
  producers/retention, vector state, formatter/allocator and child/frame
  admission remain; nonpositive, larger and other-label profiles stay open.
  Tried: FND-SOUND-023 removes installed-source count/length/printability
  uncertainty: MainGroup and DigitalGroup satisfy the source profile;
  MusicOnlyGroup is empty with declared count -1 and remains outside it.
  Native producers/retention and the empty-group route remain in Settles it.
  Tried: FND-SOUND-024 removes the local special -1 allocation-bypass
  alternative: both record and label callers pass low-word products, while
  the signed music-only label loop skips negative counts. Numeric conversion,
  allocator outcomes, state retention and actual chooser reachability remain.
  Tried: FND-SOUND-025 removes the zero/absent-sentinel conversion alternative
  for the delivered installed -1 count line. The decimal path assigns FFFF;
  allocator, retention, vector and dialog/child admission remain unsettled.
  Tried: FND-SOUND-026 removes local allocator signedness/rounding and
  exact group-clearing uncertainty for those requests. The word request
  is zero-extended; normal pointers have offset four and clearing covers
  the requested span. Heap links/break state, DOS resize outcome and later
  chooser/child admission remain; arithmetic does not prove native success.
  Tried: FND-SOUND-027 removes the plain percent-s formatter dependency for
  the bounded short-label profile: its nonnull valid string is copied
  unchanged with one NUL and no padding. Actual record production/spans,
  heap/state retention, vector and chooser children remain in Settles it.
  Tried: FND-SOUND-037 completes the local record-arm mapping, exposing
  source capacity obligations for five alternate arrays and five NUL copies.
  Actual source bounds and native spans remain; this is not label-retention
  or successful chooser evidence.
  Tried: FND-SOUND-029 removes installed-source string/count-span and
  primary-key occurrence uncertainty across all 26 records. It preserves
  the first MainGroup record's missing DigitalAddress and duplicate Dma;
  actual context/order, heap spans, alias/retention and chooser state remain.
  Tried: FND-SOUND-097 binds completion to the registered ordinal, AL-only poll rejection, transformed-byte final test and cleanup-before-return; drawing/backing/mode/interrupt preservation, table/frame aliases and native delivery remain. Reuse EXE-487/488/489; their helper contracts are not new gaps.
  Tried: FND-SOUND-098 settles attribute setters, rectangle gates/count/direction and local copy/delay saves. BIOS SI/DI/ES, port termination, scrolling scratch/coordinates, heap/backing spans and native lifetime remain; caller saves alone do not admit the BIOS branch.
  Tried: FND-SOUND-099 assigns all five controlled geometry/gate writes to1783 and binds startup/mode requests. Queried AL/AH,0040:0084, prefix/native BL, DEC7 retention and excluded access remain; requested03 alone does not establish80 columns or copy synchronization.
  Tried: FND-SOUND-100 verifies all26 written label prefixes, all populated records and six modeled headers at two fresh post-label stops. Positive label allocation/prefix coherence at that endpoint is settled; intermediate and later heap/alias/vector/chooser lifetime remains.
  Tried: FND-SOUND-101 observes matching mode03/80x25, direct-enabled/sync-zero and declared vector at two post-label endpoints. The chooser-use attempt stops at190F:0000 before entry; caller/action, supported input and later frame/vector retention are required before retry.
  Tried: FND-SOUND-102 binds the guard's incoming return to164C:050D in two fresh attempts, matching262's BLASTER notice call. Caller identification is removed from Settles it for this profile; native objects, dismissal/input, frame and continuation remain unverified.
  Tried: FND-SOUND-103 rules out a nonwrapping handler frame under102's observed entry state. Its upper interval intersects100's earlier negative record-block model; simultaneous lifetime/alias and child/deeper-stack admission remain. This does not observe continuation or a native overwrite.
  Tried: FND-SOUND-104 verifies both fixed object pointers, selector zero, six style words,99-byte body and one eight-byte choice in two fresh guard observations. Entry object/termination uncertainty is removed for this profile; retention, wrapped consumption, native input and continuation remain.
  Tried: FND-SOUND-105 reaches the first pre-poll stop twice with matching wrapped BP/SP, three body rows, one registered choice and retained entry objects/prefixes. Construction reachability and those endpoint counts are removed from Settles it; input/return, later retention and live allocation ownership remain.
  Tried: FND-SOUND-106 observes000D after one Enter and normal caller return0000 in two corrected fresh attempts; the original host diagnostic remains partial. Fixed guest input/return reachability is removed from Settles it; event completion is pending at return, with later delivery/continuation and retention still open.
  Blocks: complete setup-selection interpretation of the named formats.
- Q-SOUND-009. RULE-SOUND-004: Does any computed or bulk write change the voice tables at
  `DS:290A..291E`? Settles it: a census of block copies, string stores and computed stores whose
  destination can fall in `DS:290A..291E`, with a positive control for each kind, or a reached
  writer. Tried: split from Q-RNG-014. FND-RNG-043's immediate search finds no instruction naming
  `2909`, `290A`, `2911` or `2912` other than the two table reads. Blocks: slices 2-7.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-SOUND-001. RULE-SOUND-001, RULE-SOUND-002, RULE-SOUND-003: Does the game audibly change when
  Music or Sound Effects is toggled at a stable screen, and is continuous audio present at the
  start screen and after START GAME? Settles it: the audio-presence live session. Tried: the
  static reading of the three rules, which shows that music starts at startup from the mode-1
  songs and that effects stop while `g_57E0_1435` is 0, but not which Preferences control sets
  which byte. Blocks: slice 7.

## Source

None.

## Blocked

None.
