# CONFIG

Next ID: Q-CONFIG-015

## Static

- Q-CONFIG-002. FMT-CONFIG-003, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004, SCR-UI-007: What initializes the settings for a new game, does any path
  synchronize the saved speech gate and runtime voice button state, and can
  music volume be adjusted elsewhere? Settles it:
  establish new-game setting producers and later voice-gate synchronization.
  Other volume-control and sound-library output questions remain here.
  Q-CONFIG-014 now owns the independently answerable Preferences readback
  contract and its caller/state/target admission. This split preserves the
  evidence and unresolved mechanisms; it answers none of them.
  Tried: the save/load copies (FND-SAVE-004, FND-SAVE-005),
  renderer (FND-CONFIG-009, FND-UI-033), Preferences hover routine (FND-UI-034)
  and button dispatcher (FND-CONFIG-010) identify the difficulty, effect-volume,
  music/effects enable and animation-state fields and the ordinary button steps.
  FND-CONFIG-236 identifies byte `0x02` as the sound library's music-level
  request; FND-CONFIG-013 identifies byte `0x04` as the music bar's denominator.
  FND-CONFIG-014 shows that the launcher and Preferences button change the
  runtime voice gate while save/load carries the other gate, and playback
  checks both. FND-CONFIG-120 bounds the overlay 171 difficulty write to a
  record-field branch; its callers and inputs remain unread. Bounded mapped
  Ghidra queries found no recognized references to either that entry or its
  resident trampoline, which does not exclude indirect dispatch.
  FND-CONFIG-025 corrects the resident `PREF` hit to a Preferences label;
  the direct tag uses found so far are in save/load. FND-CONFIG-026 bounds
  the literal difficulty-word writers to load, Preferences and the guarded
  overlay branch, without excluding an indirect or block write. FND-CONFIG-027
  bounds literal speech-gate writes to load, launcher and Preferences paths;
  three table-like raw hits are overlay fixup payloads, and indirect or block
  writes remain possible. FND-CONFIG-028 follows the Start Game button branch
  into a shared setup helper without finding a direct settings write there;
  its later calls resolve to overlay 187 and 182 entries, but their effects
  and later start paths remain unread. FND-CONFIG-029 bounds literal
  music-level request writes to Load Game and the Preferences-entry getter,
  without excluding indirect control. FND-CONFIG-218 gives the animation
  byte's polarity from the `A` and `F6` handler's messages and shows that no
  key writes either voice byte. These readings
  do not identify new-game values, the writer of the animation lock,
  any synchronization beyond those paths or another music-level control.
  FND-CONFIG-235 replaces the direct-store review and withdraws its former
  global no-control conclusion. Settles it also requires the getter's admitted
  driver state/result after FND-CONFIG-236 resolves its local sentinel exits
  and setter signed clamps; retain concrete slot/table/sequence admission and
  music-level writes through the inspected controls'
  callees, computed destinations or block copies; name each searched mechanism
  and retain exclusions. This affects RULE-CONFIG-003's saved-byte readback and
  any claim excluding other controls, not unrelated sound-service behavior.
  Tried: FND-CONFIG-237 reads the selected AE/AF/B1 tables and connected local
  bodies across ten Copy drivers, distinguishing current and target level;
  ADV 11 registration/reset supplies conditional initial writes of 100. It
  removes the unspecified query-word and immediate/reset-writer conditions,
  not actual slot/payload/lifetime admission. Six absent-entry resources and
  the unparsed ADV 10 table remain explicit. Masked-query flag restoration
  uses an overlapping IRET stream, verified separately from Ghidra's listing.
  No whole item closure or complete reading follows.
  Tried: FND-SOUND-082 resolves the selected query's progress mechanism:
  it rescans missing cache membership rather than advancing a cursor. The
  connected installer/evictor has an unchanged retry when no entry is eligible;
  the zero-count query is post-tested, not an empty scan. This removes the
  unspecified local enumeration behavior; actual count, flag/retention, arena,
  alias and caller admission remain above. The conditional no-progress path
  was not run or established as reachable in shipped play.
  Tried: FND-SOUND-083 checks all twenty-six manifested installed GFF
  directories with plain/indexed positives and finds none of the four song
  types requested here. It composes the known missing-type failure with the
  ordinary pre-setup epilogue. Installed TIMB payload parsing for those arms
  is not the remaining obligation; media/runtime-record/configuration and
  bypass/other-caller admission stay above. No universal unreachability or
  whole item closure follows. Blocks:
  slice 3.

- Q-CONFIG-014. RULE-CONFIG-003: What value and state does the Preferences
  entry obtain from music-level readback on its admitted caller/driver paths?
  Settles it: complete the entry/getter/driver-result/byte-store contract with
  actual configuration, frame, driver/sequence and storage admission, all
  relevant writer histories and indirect targets. Reuse the existing local
  branches and source restrictions below; do not grow this into unrelated
  audible output or new-game defaults. Unknown effects on the readback state
  remain obligations until resolved, including those shared with Q-CONFIG-002.
  For RULE-CONFIG-003 retain actual configuration/slot/sequence admission,
  driver-return and write effects, and computed/copied music-level writers;
  reuse FND-CONFIG-236's local sentinel exits and signed setter clamps, and
  FND-CONFIG-237's Copy-driver query fields, immediate writes and ADV 11 reset
  stores. Establish actual registration/payload/storage admission and later
  current/target writers, changed or missing function tables and reached sound
  update effects; those local field identities are no longer unread.
  Search mechanisms and exclusions must be explicit.
  FND-SOUND-083 excludes the four ordinary song-request arms from obtaining
  sequence payloads in the faithful, validated installed archive set. Establish
    actual archive-record/configuration admission and other media, bypass or
    indirect setup sources before extending TIMB parsing. FND-SOUND-084
    classifies both remaining known direct setup sites: neither constructs a
    separate payload; both explicitly publish DS:3442 rather than getter handle
    DS:3464. Their stable-configuration gates exclude setup locally. Retain
    changed configuration/binding, entry after those gates and any child or
    computed/copied propagation into readback state; do not reread their local
    argument/result classification. FND-SOUND-085 classifies the listed literal
    handle writers and source-initialized FFFF words: do not repeat that scan
    with unchanged inputs. Establish which initial/prior 3440 value survives
    acquisition failure and the start request's child effects before its
    unchecked post-call copy into 3464. FND-SOUND-086 settles ADV 11's
    sentinel bypass and admitted non-playing start: with stable table/binding,
    valid disjoint sequence/frame storage it resets level to 100, sets state
    one and preserves the game's handle words. Retain actual entry-state and
    storage admission and active already-playing cleanup. FND-SOUND-087
    settles quiet restart for empty note slots and inactive signed flags;
    retain actual note/flag histories, 2C1D count/occupied-slot consistency,
    mapping values and all selected registered blocks. Reuse the local
    2CF3/2635/2483/25FA/3F01 contracts; active 1B1D/1B88 descendants and
    their return/write/alias effects remain unread, along with
    other/missing driver targets and later setters; do not reread the local
    sentinel/fresh-start/reset bundle. Retain nonliteral/block/alias writes,
    unknown entries and actual segment/storage histories outside the census.
    For paths that do
  reach ADV 11 setup, reuse FND-SOUND-082 and retain actual TIMB count/backing/
  stop-key, cache flags/retention and arena state across the patch handoff.
  A finite list or sequence handle alone does not prove setup publication.
  Inherited evidence: FND-CONFIG-235/236/237 and FND-SOUND-082/083; the
  research attempts remain under parent Q-CONFIG-002. The current finding
  supplies the installed-input restriction, not live state or a complete
  reading. Q-CONFIG-002 keeps initialization, voice synchronization and other
  controls/output questions. Native screen frames remain Q-CONFIG-001.
    Tried: FND-SOUND-084 connects the two alternate direct callers with their
    shared payload/control-block producers and result consumer. The known
    direct-site classification is answered; indirect entry and state history
    prevent a complete readback result. This does not close the queue item.
    Tried: FND-SOUND-085 removes unread literal handle producers and initialized
    source words. The source-word reload after start and uncleared source on
    acquisition failure distinguish initial sentinel and retained-handle cases;
    actual intervening writers/children remain unproved. No queue closure.
    Tried: FND-SOUND-086 connects selected ADV 11 AA start with its reset and
    stop boundary. The sentinel and admitted fresh-start cases no longer need
    a child-effects reading; restart cleanup and actual state histories remain.
    Tried: FND-SOUND-087 removes quiet-restart effects and active local
    flag/count/dispatch classification. Remapping selects other registered
    blocks, so active preservation needs their backing and count/slot
    admission as well as descendant effects. No whole reading or queue closure.
  Blocks: slice 3.

- Q-CONFIG-007. RULE-CONFIG-005, SCR-UI-007: Does another path impose an upper limit on
  the message-delay word, and does a new game replace the loaded-image value
  50? Settles it: a bounded reading of initialization and other direct,
  indirect and block writers. Tried: the
  Preferences hover and click branches (FND-UI-034, FND-CONFIG-010), the
  millisecond wait (FND-TIME-004), a raw search for direct references to
  `DS:26B7`, and a bounded reading of the overlay 172 wait gate
  (FND-CONFIG-011) identify the word's 100-ms scaling and six literal uses.
  FND-CONFIG-028 finds no direct message-delay write in the Start Game
  button branch or its shared setup helper. The new-game starting value and
  indirect or block writers remain unread.
  Blocks: completing the named entry readings.

- Q-CONFIG-008. RULE-CONFIG-005, RULE-SCRIPT-002: Which caller paths enter overlay 172's
  shared message routine? Settles it: complete incoming-path readings for
  the shared sinks and possible resident or computed-pointer calls,
  including the conditions and text pointers that reach the routine.
  Tried: FND-CONFIG-017 identifies resident calls; FND-CONFIG-035 inventories
  56 direct calls from 21 overlays. FND-CONFIG-042 through FND-CONFIG-058
  give bounded local readings for all 56 sites, with FND-CONFIG-046
  deepening the Save Game call. FND-CONFIG-070 rules out an address-taking
  fixup to this entry from another overlay; FND-UI-037 and FND-SAVE-010
  trace the Save Game UI and keyboard paths. FND-CONFIG-123 traces the
  guarded save-capacity call from startup. FND-CONFIG-072 confirms the four
  direct resident message calls exhaust MZ relocations to that entry.
  FND-CONFIG-073 adds five internal calls from overlay 172's `0034` and
  `0043` routines, including a direct resident path into `0034`.
  FND-CONFIG-074 traces the `0043` callback through registration on seven
  frame identifiers and the frame dispatch path; FND-CONFIG-075 finds
  only six in the shipped window graph. The complete event sequence into
  that callback, other shared sinks' incoming paths, and computed or
  unrelocated pointer calls remain open. FND-CONFIG-076 closes the direct
  incoming routes to overlay 187's four save and cinematic message sites.
  FND-CONFIG-077 traces overlay 204's two rest message sites to an overlay
  182 handler and script request one; their upstream live inputs remain open.
  FND-CONFIG-124 traces overlay 171's list messages to a choice branch and
  the `WIND/18501` callback. FND-CONFIG-221 corrects the guarded resident
  dispatch: a window result has no own global fallthrough, and zero window
  count rewrites the global event kind to three. Actual count/target state
  and event admission remain. FND-CONFIG-080 identifies two
  event-record discriminators for the callback's message branch.
  FND-CONFIG-215 traces a conditional pointer-hit producer for the first;
  physical input mapping and live state remain open. FND-CONFIG-216 shows
  that a keyboard packet can carry the second discriminator. FND-CONFIG-083
  traces the list callback's global registration and the event-six fallback
  route into it. FND-CONFIG-084 traces one temporary replacement and
  restoration path. FND-CONFIG-085 identifies six overlay 182 setter calls
  that install a resident key callback or zero; their timing relative to the
  list remains open. FND-CONFIG-086 classifies the remaining twelve direct
  setter sites, including one saved-pointer restoration. Computed or indirect
  calls, registration order and live state remain open. FND-CONFIG-087
  shows overlay 209's no-other-classes message skips its callback
  registration and bounds event routes that can restore its prior pointer.
  FND-CONFIG-088 traces an overlay 213 window callback's guarded event-two
  route into its shared message sink; its live event and record inputs remain
  open. FND-CONFIG-089 finds a matching shipped button and conditional
  pointer-hit route in one of its windows, but physical input and live
  window state remain open. FND-CONFIG-217 traces the callback's unsigned
  event-bit threshold to the queued mouse packet; the bit meanings and
  remaining live gates remain open. FND-CONFIG-092 separates overlay 175's
  setup from a registered frame handler whose value-32 branch enters the
  helper with two conditional message sites. FND-CONFIG-093 supplies all
  six matching shipped frames and their enabled value-32 mask.
  FND-CONFIG-094 traces mouse-packet bit 4 through the resident APFM
  dispatcher into that handler. FND-CONFIG-095 traces hit-selection
  refresh and earlier helper gates. FND-CONFIG-096 shows the intervening
  window return is ignored and its child branches skip the shipped APFM
  graph. FND-CONFIG-097 and FND-CONFIG-098 bound the selection and
  old-window helpers by their two-tag tables. FND-CONFIG-099 bounds the
  position/region callees' fixed write targets. Physical input mapping,
  other prior control types and later state changes remain open.
  FND-CONFIG-100 traces overlay 176's two message sites through overlay
  193's range- and byte-gated selector. FND-CONFIG-101 inventories its
  eleven declared overlay callers. FND-CONFIG-102 traces overlay 172's
  zero-gate frame-code source; FND-CONFIG-104 supplies a resident
  mouse-bit-two/value-64 producer. FND-CONFIG-103 excludes overlay 173's
  direct route by its literal nonzero gate. FND-CONFIG-125 reads overlay
  174's three zero-gate routes: two helper-result codes and one
  sign-extended table byte plus 235. Producing helpers, table contents
  and callers remain open. FND-CONFIG-106 excludes overlay 189 and
  213's three nonzero-gate invocations; FND-CONFIG-126 reads overlay
  211's two zero-gate code sources. FND-CONFIG-108 reads overlay 208's
  stored gate and code, completing local gate classification of the
  eleven declared sites. Stored-field and table producers, code-producing
  helpers, remaining guards, upstream reachability and live state remain
  unread. FND-CONFIG-109 traces overlay 208's input stores to setup
  arguments; FND-CONFIG-111 reads its six declared setup calls.
  FND-CONFIG-110 traces two local selector-caller routes, including
  conditional repetition. The 006B incoming routes, helper effects,
  data producers, later writes and live state remain open.
  FND-CONFIG-127 traces entry 006B to a resident mode-five dispatch
  forwarding two input words. FND-CONFIG-128 traces an event-five/
  value-64 state-one branch that forwards its input record to the
  dispatcher. Entry 28C9:1261 incoming routes and event producers,
  helper effects and mode changes remain unread. FND-CONFIG-114
  records qualified negative incoming-reference inventories for that
  entry without finding its producer. FND-CONFIG-129 reads the
  state-two/three paths: early pointer exits bypass mode dispatch, and
  reaching it otherwise requires a later stored state of one. Helper
  state writes, registration and indirect dispatch remain open.
  FND-CONFIG-130 reads the local record-taking wrapper and its
  conditional index remapping; transitive callee effects remain unread.
  FND-CONFIG-117 identifies the state getter/setter and overlay 204's
  temporary-five assignment with saved-word restoration. Its intervening
  overlay 179 call, incoming routes, other writers and handler timing
  remain open. FND-CONFIG-131 reads overlay 179's wrapper and
  pending-record drain, including state-one/five return-region gates.
  FND-CONFIG-119 identifies seven local wrapper calls in overlay 204
  entry 0020, guarded by two local thresholds. Earlier caller inputs,
  local producers, transitive helper effects and state timing remain open.
  FND-CONFIG-132 traces first-pass thresholds and captured indices,
  their read-only value helper and second-pass grouped calls. Iterator
  producers, setup effects and intervening helper effects remain open.
  FND-CONFIG-133 reads the iterator's signed-negative record selection
  and separate stored-index branch. Input producers, allowed ranges and
  intervening changes remain unread; reachable termination is not established.
  FND-CONFIG-144 reads a local stored-index assignment and table clear.
  FND-CONFIG-135 reads a registered flag writer and another clear path.
  Their upstream dispatch, timing, callee effects and later or other writes
  remain unread; no complete range or termination invariant is established.
  FND-CONFIG-136 traces the registered writer to pre-handler script-opcode
  dispatch. Opcode 0x22 clears the flag before parameter reading, whose
  nested dispatch and later effects remain unread; other rest routes, pointer
  replacement and reachable script requests remain open.
  FND-CONFIG-137 reads normalized nested-instruction dispatch and
  ordinary parameter save/restore spans, which do not roll back the flag.
  Reachable nested instructions, byte-reader and other expression effects,
  nesting-word provenance and intervening state changes remain unread.
  FND-CONFIG-138 reads the byte-reader and advancement bodies: ordinary
  cursor updates do not write the flag, and a failed end check calls its
  known clear path. Reader inputs, remaining expression effects, reachable
  nested instructions and later rest-entry changes remain unread.
  FND-CONFIG-139 reads the B1 expression root, seed/count/selector parser
  and chained lookup wrapper. Seed and lookup callee effects, stored-field
  producers and reachable expression inputs remain unread.
  FND-CONFIG-140 reads the lookup's typed reads, stored error/pointer
  outputs and conditional table-slot writer. FND-CONFIG-141 reads the
  seed wrapper and shared-lookup scan. Metadata and slot-index producers,
  input validity and reachable expressions remain unread.
  FND-CONFIG-142 records the initial slot word and guarded setup
  assignment, rejects four overlapping writer decodes, and finds no verified
  metadata writer in its qualified literal query. Other write forms and
  timing remain unread; do not repeat that query without new coverage.
  FND-CONFIG-143 reads setup callee 2D40:2196's local slot checks,
  output writes and conditional table writes. The zero-slot path returns
  without traversal calls; that reading did not resolve setup-time slots,
  traversal helpers 1AA0:0566 and 1AA0:051C, retry termination or input
  ranges. FND-CONFIG-144 replaces the setup finding after
  resolving its pushed output-pointer segment fixup.
  FND-CONFIG-145 reads both traversal helpers, their shared getter path,
  save/restore copy and count-consumption branches. Retry termination is
  conditional on valid, non-aliased state. The near-pointer segment
  relationship, buffer validity, selector/starting-position producers and
  actual setup-time slots were not resolved by that helper reading.
  FND-CONFIG-148 adds a qualified selector-base query and rejects three
  overlapping decodes; no producer was verified. Do not repeat that query
  without new coverage of other producer forms. FND-CONFIG-147 reads the
  intervening setup calls for their actual ranges and resolves slot 523's
  zero state at the local traversal call. Setup invocation, bypass state,
  later table changes and rest-time iterator inputs remain open.
  FND-CONFIG-149 inventories the declared direct setup call and reads
  its local branch join and following return checks. The initial gate byte
  and a qualified literal-writer query do not establish the gate at that
  call. FND-CONFIG-150 reads the following initializer's direct selector
  and starting-position stores and metadata-buffer argument. Resource
  callees 38FF:05B5 and 38FF:04AB, earlier gate producers, cleanup,
  other later changes and reachability remain open.
  FND-CONFIG-151 reads the length and full-transfer wrappers, existing-
  buffer branch and count checks. FND-CONFIG-152 traces a positive
  metadata count at most 98 to one operating-system request. This is a
  block-producer path, not a successful native transfer observation.
  Selected record stability, accepted FNFO bytes, archive preparation,
  positioning, operating-system results, cleanup and later inputs remain open.
  FND-CONFIG-153 identifies fingerprint-matching OBJEX.GFF FNFO lengths
  at the initializer's exact limits; the RESOURCE.GFF and GPLDATA.GFF
  catalogs have no FNFO. FND-CONFIG-154 gives bounded source-backed
  selector and offset cases without asserting actual loads or valid records;
  the width-prefix direction flag and later width changes remain open.
  FND-CONFIG-155 reads the zero-mode pre-setup helper's code-segment
  resets and internal flag return; it does not supply an archive registration
  or gate producer. FND-CONFIG-156 reads post-setup 00C0 callee
  565C:0020's local memory initialization and return contract: an early
  FFFF failure becomes a retained FF and passes the caller's zero check.
  Its external effects, failure-byte producers and buffer validity remain open.
  FND-CONFIG-157 locates the earlier OBJEX.GFF registration attempt before
  both mode branches. Its outcome and the intervening callees' archive effects,
  including 172C:000C with word 99, remain open. FND-CONFIG-158 reads
  the three pre-join graphics calls and their restored DS, retaining BIOS
  and hardware outcomes. FND-CONFIG-159 reads the nonzero-mode helper,
  its repeated gates, stored function pointer and local video-reset target.
  Their own-code paths supply no direct archive or setup-gate producer.
  External callee effects, pointer consumers and replacements, script 99's
  entry and resource effects, gate producers, successful loads and later
  inputs still need bounded readings. FND-CONFIG-160 reads the script-entry
  status gate, reset and loader call order. FND-SCRIPT-019 corrects cache
  age paths and pre-transfer state writes; FND-SCRIPT-020 bounds the guarded
  buffer reset. FND-SCRIPT-021 bounds replacement selection and writes.
  Actual cache state, resource loading, error entry and reachable MAS/99
  opcodes remain open. FND-SCRIPT-022 bounds allocation search and its
  restart/comparison paths. FND-SCRIPT-023 reads error-entry ordering,
  and FND-CONFIG-161 bounds its shared helper and final poll. Complete
  external effects, pointer/gate inputs, return outcomes and resource
  provenance remain open alongside Q-SCRIPT-003. FND-CONFIG-162 reads
  the first local callee's registration and two polls; FND-CONFIG-163
  bounds the setter guard and mode-one callback-loop bypass. Interrupt
  outcomes, cleanup/helper effects, stack/gate producers and actual
  registration/dispatch timing remain open. FND-CONFIG-164 traces the
  common poll's returned BX predicate and aliased scratch outputs, while
  keeping actual interrupt/input sequences open. FND-CONFIG-165 reads
  the pointer wrapper's marker/order and returned zero; FND-CONFIG-166
  bounds state-clear/status gates and the stable-state first-zero case.
  Runtime 1000:149B, active status and service effects, pointer/state
  writers, DS preservation and actual intervening input remain open.
  FND-CONFIG-167 reads runtime dispatch, a bounded rejection predicate,
  result conversions and shared-slot DS restoration. FND-CONFIG-168
  reads the zero-selector pointer consumer's conditional exit after list/count
  changes and the caller's ignored result. FND-CONFIG-179 reads the
  following helper's separate mode gates, callback and resource requests
  without success-result branches. FND-CONFIG-170 resolves local
  retained-result restoration through the near-state collector.
  FND-CONFIG-171 reads following resident callback gates, fresh targets,
  gated helper calls and fixed word writes. FND-CONFIG-172
  bounds child cleanup, recursive error propagation without a leaf origin,
  and ignored EBOX results. FND-CONFIG-173 reads the intervening list
  helper's signed gates, pointer walks and checked/ignored results.
  FND-CONFIG-174 reads the guarded EBOX dependency's common zero return;
  FND-CONFIG-175 bounds bracketed guard-bypass SI/DS and copy contracts.
  FND-CONFIG-176 reads setup and append result contracts and a count-16
  pre-write rejection. FND-CONFIG-177 reads wrapper staging, sentinel
  bypasses and conditional pair expansion. FND-CONFIG-178 bounds 0180's
  checked private append and output routes and the 0DEC self-copy, retaining
  full geometry and native input provenance.
  FND-CONFIG-180 resolves bitmap result gates and the callback/mask
  argument widths, superseding FND-CONFIG-169 through FND-CONFIG-179.
  FND-CONFIG-181 reads the filename conversion and archive requests,
  retaining near-DS/far-SS provenance and external path/error effects.
  FND-CONFIG-182 reads the path append and runtime scan/copy helpers;
  signed lengths, prefix-dependent zero placement, aliases, capacities
  and reachable filename/prefix inputs remain conditional.
  FND-CONFIG-183 reads the initializer's checked allocation/handle gates
  and raw descriptor/trampoline target; FND-CONFIG-184 reads failure
  cleanup's ordered record/pointer/handle writes and VGA dependency.
  FND-CONFIG-185 reads the named zero-option resident resource call.
  Full producer, allocation/runtime, service and termination outcomes
  remain open; a returning result alone does not settle the parent gate.
  FND-CONFIG-186 reads the temporary byte's earlier clear, local helper
  gates and mode/number changes. FND-CONFIG-187 bounds replacement's
  size/result asymmetry, unchecked transfer and later cache/byte writes.
  Complete child/service, register, segment, state and content producers
  remain open; none of these paths proves native message entry or timing.
  FND-CONFIG-188 reads pointer reset/replacement and word guards over
  low-byte writes. FND-CONFIG-189 reads active callback, handle and
  state-commit/restoration routes, including unchecked +5E targets and
  stored FFFF paths. Neighboring bytes, full producers/targets, primitive,
  runtime and VGA outcomes still prevent an end-to-end entry claim.
  FND-CONFIG-190 reads frame count/index and dimension contracts;
  FND-CONFIG-191 reads partial request metadata and wrapper/coordinate
  gates before the graphics primitive. Complete accepted image/slot
  producers, bounded reference chains and primitive effects remain open.
  FND-CONFIG-192 reads the complete local graphics primitive's shared
  preparation, reference walks, direction/overlap gates, port accesses
  and phased copies. Accepted root/mask/segment inputs, capacities,
  aliases, full callers and native VGA outcomes remain open.
  FND-CONFIG-193 reads one fixed-root/free-slot pool initializer,
  a declared overlay setup call and the local remaining-space arithmetic.
  An entry-based walk shows no local bypass before that call;
  preceding external-callee outcomes, later slot/pool writers and
  accepted hardware state remain open.
  FND-CONFIG-194 reads release flag gates, E4E changes and
  restarted adjacent-block copies. Accepted slot state, alias
  and native VGA effects remain open.
  FND-CONFIG-195 inventories declared resident release calls and connects
  one record-bit-gated direct route outside the signed wrapper. Record
  producers, other callers and native slot/hardware outcomes remain open.
  FND-CONFIG-196 reads a four-word display-state writer with signed
  old-handle release gates and untested replacement writes. Its bounded
  incoming inventories found no route; new pointer/registration evidence
  is needed before repeating them. Arguments and later refresh remain open.
  FND-CONFIG-197 reads refresh wrappers that temporarily replace A033,
  restore one saved word and normalize returning completion to zero.
  Input/segment/frame provenance and transitive effects remain open.
  FND-CONFIG-198 reads a fixed 256-request/release caller, retaining
  DS/SS frame conditions and initial-slot provenance. Its count measures
  qualifying calls, not success; incoming routes and hardware remain open.
  FND-CONFIG-198's subsequent MZ/FBOV far-call and bounded
  same-segment near-call inventories found no incoming route. Do not
  repeat these queries without new pointer/registration coverage.
  FND-CONFIG-199 reads a rectangle transfer caller whose first-request
  failure reaches a cleanup read of an unassigned second frame word.
  Incoming state, frame provenance and native outcomes remain open.
  FND-CONFIG-200 connects a text-output continuation to the rectangle
  helper, with untested result and no equal-coordinate branch. Text
  inputs, formatting effects and cleanup frame provenance remain open.
  FND-CONFIG-201 reads the fixed text wrapper's 28-byte argument
  map and unchanged return. Incoming callers, DS-relative format
  provenance and accepted inputs remain open.
  FND-CONFIG-202 connects one of 21 encoded overlay text-wrapper
  calls to formatted SS-frame storage and strict screen-edge gates.
  Format capacity, width/state producers and later counter progress remain open.
  FND-CONFIG-203 reads literal formatter writes and the extra zero
  terminator without a capacity gate. Actual format/conversion output,
  admitted arguments and six-byte caller fit remain open.
  FND-CONFIG-204 identifies two distinct initial formats and the
  decimal formatter's DS-based argument read. Current DS/format state,
  parser flags, decimal output and caller fit remain open.
  FND-CONFIG-205 bounds plain decimal word output: nonnegative inputs
  need at most six bytes including termination. Current format, admitted
  argument identity/preservation and DS/SS aliases remain open.
  FND-CONFIG-206 reads the modular text-width sum and unchecked
  character-word lookup. Pointer/table producers, admitted text,
  storage aliases and native presentation remain open.
  FND-CONFIG-207 connects one lookup record to a FONT selector-100
  request and nonzero returned pointer. Acquisition validity, incoming
  state, later record/pointer writers and lookup capacity remain open.
  FND-CONFIG-208 connects the pointer acquisition wrapper to the
  shared archive reader and distinguishes signature/reader output paths.
  Archive producers, reader storage, aliases and FONT lifetime remain open.
  FND-CONFIG-232 settles ASCII byte lookup capacity for the unchanged
  shipped FONT at offset four and classifies every high-byte address.
  Retain actual text/pointer/table identity and lifetime, rather than an
  unclassified byte-index range or a universal zero-extended lookup model.
  FND-CONFIG-233 identifies FONT cleanup39D1:0326, its conditional
  release without an own pointer clear, and the final saved-record restore.
  Retain named preceding-helper/alias and actual release effects plus
  later-reader or reinitialization/exit admission; do not assume cleanup
  nulls A0ED, A043 or the graphics-record root. Its literal field census
  retains computed/bulk/physical aliases and excluded incoming streams.
  FND-SAVE-027 binds the known post-cleanup interrupt-table restore to
  source4DE7:0002 and its DF-sensitive1024-byte forward interval.
  Retain actual source/DF/vector and subsequent callback admission;
  it is not a file-output call or a passive continuation. Q-SAVE-007
  separately owns the unsupported transitive no-save claim.
  Q-CONFIG-013 now owns the shared allocation/header contract formerly
  accumulated here, retaining FND-CONFIG-209/211/212/223 and all their
  heap/link/bound, caller-range, direction-flag and runtime obligations.
  Message/FONT selection, caller-specific aliases, later resource lifetime
  and other runtime metadata/bound/slot writers remain in this item.
  Tried: FND-COMBAT-169 admits source-equal FNFO buffers, widths/selectors, scratch 520,
  selected 523 and finite disjoint pools at one paused saved-map boundary. FND-COMBAT-171
  adds exact equality of all 1530 live shared item-type bytes with canonical ALL/4 at that
  boundary. Provider identity, use-time admission and preservation through later calls remain.
  Blocks: completing the named entry readings.

- Q-CONFIG-013. RULE-CONFIG-005, RULE-SCRIPT-002, FMT-SCRIPT-004,
  RULE-SCRIPT-008: What allocation, header and returned-span contract do
  the shared heap entry and wrappers supply to the recorded FONT and
  script-setup requests, including zero and partial/failure outcomes?
  Settles it: connect 1000:15A5 and reached heap helpers to the selected
  header/paragraph count and returned pointer consumed by 444C:00FA and
  the FONT wrapper. Establish root/free-list/header/link/break/bound producers,
  their field widths and aliases; request ranges of the named callers;
  direction-flag and segment-wrap conditions; indirect targets, DOS/service
  outcomes and failure continuations that change these returned spans or
  marker writes. Classify invalid and null results rather than requiring
  every allocation to succeed. Do not infer capacity from request size alone.
  FND-CONFIG-209/211/212/223 and FND-EXE-272/293/294 supply the recorded
  wrapper, arithmetic, ordinary exact/split/new-memory header and return paths.
  FND-SCRIPT-056 removes arithmetic rejection/marker wrap for its twelve
  named setup requests under the retained header/offset bindings; actual
  heap-state admission and backing remain here. Resolve each remaining
  mechanism against the span/marker claim, not every unrelated heap caller.
  FND-CONFIG-225 assigns the sixteen inventoried explicit control stores and
  classifies ordinary nested saved-DS publications, including the copy branch's
  reload before release. Retain computed/bulk/physical aliases, reentry and
  changed incoming/callee segment effects; do not repeat the controlled
  direct-store census or assume a nested write necessarily changes its value.
  FND-CONFIG-226 binds the eight listed startup/later bound-current stores,
  cache store and unchanged-shipped-quantity startup range, separating15F3
  records and ES:BX objects from direct heap globals. Retain actual P/DS,
  native/intervening updates, computed/bulk/object aliases and backing state,
  not unspecified startup quantities or an invariant that failures leave bounds unchanged.
  FND-CONFIG-227 settles allocated/free metadata footprints and exact-size/
  tail-split transitions. Retain actual free-list membership and coalesced
  predecessor/successor admission, especially the following header's word-two
  state overwritten unconditionally by splitting; payload-only disjointness
  and fixed decompiler DS aliases do not settle those inputs.
  FND-CONFIG-228 reads the original consistency checker and three direct
  diagnostic consumers: selected adjacency/backlink/range and word-sum checks
  do not prove full membership or the closing backlink, and the break query
  can change state. FND-CONFIG-229 resolves the named2C5F:0D30 query's
  bypass: its diagnostic consumers receive summed unchecked free extents
  plus a bound-derived addend after allocation/release, not a validator
  result. Retain other actual gameplay caller/bypass and backing evidence,
  including near-cursor DS/SS binding; do not substitute checker status2
  or the query total for the unresolved heap invariants.
  FND-CONFIG-230 composes nonterminal predecessor/successor coalescing,
  insertion/unlink writes and ordinary DS/SS restoration. Retain actual
  membership, positive counts, no-adjacent-free successors, physical
  separation and saved-slot stability, plus terminal/native effects;
  do not reopen the composed merge bodies as unspecified callee behavior.
  FND-CONFIG-231 settles the four terminal predecessor choices, requested
  pairs and metadata publication before17F2, including persistence on local
  bounds rejection. Retain actual bound/header/link/alias and native-result
  admission, not an assumed transactional rollback or unspecified terminal body.
  FND-CONFIG-232 settles the unchanged shipped FONT/100 length8299's
  request8300, paragraph count519 and no-wrap fill/marker/transfer arithmetic
  under the retained offset-four/header binding. Retain actual selected
  record/length, DF, heap/backing and lifetime admission, not an arbitrary
  unknown request range for that named unchanged resource.
  Tried: split from Q-CONFIG-008's shared-service dependency; no answer or
  complete reading is implied. Message/FONT acquisition and later use remain
  Q-CONFIG-008; script phase/publication/later use remain Q-SCRIPT-019;
  restore destination/live-pointer identity is Q-SCRIPT-025.
  Blocks: completing the named entry readings and slices 2-4.

- Q-CONFIG-010. RULE-CONFIG-005: Can later state changes prevent
  `WIND/10501` acquisition or registration during an ordinary message?
  Settles it: bounded readings of indirect or block writes to display
  bounds, archive options and traversal mode, plus indirect archive-close
  and cleanup callers. Tried: FND-CONFIG-018 and FND-CONFIG-030 through
  FND-CONFIG-034 bound window setup and registration. FND-CONFIG-036
  through FND-CONFIG-041, FND-CONFIG-059 through FND-CONFIG-069 trace
  literal bounds and archive references, startup, lookup, direct closes
  and cleanup registration. No ordinary-state failure is established;
  indirect changes and callers remain unread.
  Blocks: completing the named entry readings.

- Q-CONFIG-005. FMT-CONFIG-001: What does the game's sound library read from each field of
  `SOUND.CFG`, including its unexplained tail? Settles it: bounded readings
  of the remaining consumers of the whole-file buffer and the setup writer.
  Tried: comparison of the shipped file with `SOUND.INI` records names most
  fields but not the tail (FND-CONFIG-003). FND-CONFIG-019 shows the file
  entry loads bytes whole, without parsing fields; FND-CONFIG-020 identifies
  direct uses of `0x08`, `0x0A`, `0x14` and `0x32` in initialization.
  FND-CONFIG-021 adds the first four words as a group, another `0x08`
  comparison, and further `0x14` and `0x32` branches. The receiving
  routines' effects remain unread. FND-CONFIG-022 maps each tail
  output to an input-record offset or a literal 4, but the source fields'
  meanings remain open. FND-CONFIG-023 identifies bounded game-side reads
  of `0x34..0x3A`, including alternative `ADV ` resource numbers, but the
  remaining branch effects are still unresolved. FND-CONFIG-024 pairs each
  `ADV ` number with a separate ten-byte settings block and shows bit
  `0x01` of `0x14` gates the second pair; the device roles and input-record
  field meanings remain open.
  Blocks: completing the named entry readings.

- Q-CONFIG-006. FMT-CONFIG-002: What remains of setup's `SOUND.INI`
  group/numeric/count parsing and malformed token/value behavior?
  Settles it: 0B26's source/destination length and classification admission
  outside FND-SOUND-034's installed spelling set and FND-SOUND-036's
  bounded ASCII/token/span conditions, including high-byte classification
  and signed-index wrap admission;
  native line/tag/value/record spans, aliases and intervening writers
  beyond FND-SOUND-038's complete installed source-band/count composition
  and FND-SOUND-041's complete populated-record/header endpoint;
  and outer 0001's error/close child continuation beyond the local returns
  already recorded. Native stream admission is Q-CONFIG-011.
  Independent stream/error/storage obligations move to Q-CONFIG-011;
  full field-consumer effects move to Q-CONFIG-012.
  The local outer/nested record dispatch is FND-SOUND-037, not remaining
  arm mapping. The file layout alone does not determine native admission.
  Tried: the outer error/close reading reuses FND-SOUND-024/042 and the
  direct 333D wrapper, but its 0F25 formatter/351D output callback and
  preservation through error output remain unproved. FND-SOUND-043
  settles the independently answerable lower write under Q-CONFIG-011;
  this does not remove the outer diagnostic continuation obligation.
  Tried: FND-SOUND-041 removes populated record-byte and modeled extent
  uncertainty at the installed-source normal return: both 3302-byte
  groups match, with all three size headers checked in two fresh runs.
  Intermediate spans/aliases/writers, later lifetime and other paths remain.
  Tried: FND-SOUND-040 binds the normal allocator size word at returned
  segment offset zero and the offset-four payload capacity. Native size
  values and complete record bytes remain; the header field's location
  and local normal-path writers do not need another reading.
  Tried: FND-SOUND-039 observes two fresh native successful parser returns
  with counts 13, 13, -1 and nonzero group pointers. The fixed entry/frame,
  return and field-read capabilities are verified; DS and SS are distinct
  at return. Record-byte/extent coherence, intermediate aliases/writers
  and other input/error/child paths remain, not launch/normal-return uncertainty.
  Tried: FND-SOUND-038 accounts for all 26 records and 940 interior keys,
  including the two ignored MidiDmaJumper children, and removes exact
  installed nested consumption and positive group tag-counter composition
  from Settles it. Native storage/preservation, other input profiles and
  outer error/close children remain, rather than another source-band census.
  Tried: independently reviewed FND-SOUND-037 supersedes the old nested
  jumper rejection claim: nonpositive lookup consumes a line without OR,
  unlike Flags; positive non-jumper selectors fail. The strict source
  profile first excludes MainGroup record ordinal eleven's fourth jumper
  MidiDmaJumper, absent from the native table. That is not a native failure;
  full source accounting needs an explicit ignored-lookup profile extension.
  Missing-token, short sign/non-assignment, bare-prefix-zero and local
  repeated-count/failure continuation are FND-SOUND-035, not remaining
  cases; retain other malformed/overflow inputs and complete nested/state
  admission rather than retrying those bounded paths.
  FND-SOUND-036 additionally removes the bounded numeric overflow and
  bracketed-value branch/copy-failure paths; its declared span conditions
  are not native storage admission.
  Tried: FND-SOUND-036 settles unsigned 32-bit magnitude overflow,
  positive-status low-word assignment and the complete local 0AC1
  first-close/next-open/value-copy branches under its explicit bounds.
  Native classification/spans, nested source context and aliases, and
  outer error/close children remain; do not repeat those local branches.
  Tried: FND-SOUND-035 removes those representative numeric failure
  channels and repeated-count publish/allocation/counter ordering from
  Settles it. The ignored scanner count can propagate an unassigned local
  despite helper success; global counts and earlier stores are not rolled
  back on the read local failure paths. Other cases and native context remain.
  Tried: FND-SOUND-034 removes installed line/tag/numeric spelling widths
  and their local small conversion paths from Settles it. It records
  thirty-two alternate-address spellings whose suffix is discarded before
  successful hexadecimal assignment of two or three. Actual context and
  retention, general malformed inputs and failure continuation remain.
  Tried: FND-SOUND-020 settles the four local driver-name tag/value/copy
  paths and their mode-controlled output copies, plus the chunk-field
  destinations. Those local bindings are removed from the remaining parser
  work; numeric/group/count arms, complete file/error handling and field
  consumers remain. Actual loader-record/mode/pointer admission is Q-SOUND-007.
  Tried: FND-SOUND-021 settles the eight local header actions, including
  mode-tag no-ops and flag-controlled combined/separate routing. Those
  local header actions are removed from Settles it; reader/storage/callee,
  group/numeric/count and complete file/error conditions remain. Dialog
  row/index admission is the independent Q-SOUND-010.
  Tried: FND-SOUND-024 settles the group reader's local tag/count,
  low-word allocation, separate record counter and pointer-return branches.
  Remove those local branches from Settles it; numeric conversion, child
  contracts, malformed/repeated-tag admission and complete file/error
  handling remain. The conditional -1 path is not a native observation.
  Tried: FND-SOUND-025 removes conversion of the delivered installed 13
  and -1 count lines from Settles it, under retained tables and valid spans:
  the decimal scanner assigns 000D and FFFF. Its local line/comment/token
  path is recorded too. Remaining numeric work is other tokens, bounds and
  failure cases; full byte-delivery/file and child contracts remain open.
  Tried: FND-SOUND-026 records ordinary request rounding and exact clearing
  for the installed count products, removing those local child operations
  from Settles it. Native heap/break/service and malformed-input behavior
  remain separate from the conditional normal continuation.
  Tried: FND-SOUND-037 removes all fifty local outer record targets, five
  alternate loops and nested flag/jumper actions from Settles it. It records
  rejection, no-op, early EndRecord and no-rollback paths. Source capacity,
  helper/file contracts and wider field consumers remain independent.
  Tried: FND-SOUND-029 removes installed-source string lengths and setting
  count/primary/alternate occurrences from Settles it. Native context/order
  and field/value admission remain; the measured missing DigitalAddress and
  duplicate Dma are source evidence, not proof of native failure or repair.
  Blocks: completing the named entry readings.

- Q-CONFIG-011. FMT-CONFIG-002: What native stream/storage lifetime and
  remaining file/error contracts admit setup's actual SOUND.INI reads?
  Settles it: preservation of startup stream/handle/count/buffer state
  through intervening children and later refills, aliases and other writers;
  exhausted-slot storage beyond the nominal table; cleanup with nonzero
  size/count/auxiliary state and its 292B/1ACC/05F5/0EF5 child preservation,
  beyond FND-SOUND-042's retained initial-slot failure path; negative-flush
  reachability, source/stack bounds and write/seek child preservation
  beyond FND-SOUND-043's local lower-write contract;
  error-state computed/pointer/implicit accesses and aliases beyond
  FND-SOUND-045's exact-literal census; excluded entry mechanisms into
  1C08:0C95/0C96 beyond FND-SOUND-046's controlled direct-transfer
  exclusion, unsupported indirect continuations and
  selector preservation through the create-mode consumer; actual DOS/device
  and heap/break outcomes
  beyond the local path and FND-SOUND-039's normal native parser endpoint.
  Keep unsupported native
  outcomes separate from code-decided branches and non-rb callers.
  Tried: FND-SOUND-046 removes supported direct-transfer entry into
  both normalized-word candidates with a repeated fixed census and
  independent controls. The overlapping stub context is recorded;
  excluded dynamic/prefixed entry mechanisms and broad aliases remain,
  rather than another unchanged direct caller or operand search.
  Tried: FND-SOUND-045 records a controlled repeatable exact-literal
  census and the create-mode selector consumer. Do not repeat those
  searches unchanged; two boundary candidates and excluded representations,
  child preservation and runtime storage remain explicit in Settles it.
  Tried: FND-SOUND-044 removes the initial 89-row error-map values,
  reachable extent, fallback/direct categories and argument cleanup.
  Consumers, table/word preservation and actual interrupt outcomes remain,
  rather than another read of the same mapping.
  Tried: FND-SOUND-043 removes lower-write raw/translated dispatch and
  short-result arithmetic. Remaining negative-flush reachability, buffer
  and stack spans, aliases, state/child preservation and actual DOS results
  are distinct from that local contract.
  Tried: FND-SOUND-042 removes local exhausted-loop ordering and zero-size
  initial-slot construction-failure cleanup. Close-result discard, active
  field reset and retained pointers are recorded; extra-slot storage,
  nonzero-state children and actual DOS outcomes remain.
  Tried: split from Q-CONFIG-006 after FND-SOUND-030/031/032 remove
  local byte/refill/read/translation/error/flush dispatch. FND-SOUND-033
  removes the actual rb mode, startup free-handle/identity writes and
  conditional normal 512-byte stream construction from Settles it.
  Under its retained successful regular-file conditions, text translation
  and read-side flush scans are bypassed. Native preservation and outcomes,
  other paths/callers and failure cleanup remain.
  Tried: FND-SOUND-039 verifies the fixed utility's native parser entry,
  far caller and successful return twice, with counts and nonzero group
  pointers. This removes a solely hypothetical normal endpoint. Full
  stream/record bytes, extents, intermediate state and other/error paths
  remain; this is not a general input/UI/driver capability.
  Blocks: complete native parser admission for the named format.
- Q-CONFIG-012. FMT-CONFIG-002: What full consumer effects follow the
  CardGroup, driver chunk-number and SamplePlaybackMode fields?
  Settles it: remaining field consumers and their incoming selector/input
  producers, aliases and later state/driver effects beyond FND-SOUND-047's
  field handoff, digital-copy alternatives, CardGroup formatter argument
  and literal census; resolve its three 002A/six 00DA boundary candidates,
  computed/pointer/implicit accesses, native summary storage and later
  screen drawing beyond FND-SOUND-048's bounded local decimal conversion.
  Reuse FND-CONFIG-023/024 for the already recorded game ADV selectors.
  Keep field parsing distinct from device
  behavior and from the current-record admission of Q-SOUND-007/010.
  Tried: FND-SOUND-048 removes the plain signed-word formatter dependency:
  the CardGroup sentinel yields local text -1 with NUL, with a seven-byte
  worst-case bound. Native storage, drawing and device effects remain;
  do not repeat only this conversion reading.
  Tried: FND-SOUND-047 removes handoff widths, digital-copy alternatives
  and the CardGroup-four summary-argument distinction. Eight controlled
  literal queries and format controls repeat exactly. Remaining consumer,
  boundary, incoming-state, alias and driver obligations are in Settles it;
  no device effect follows from an offset or numeric match alone.
  Tried: split from Q-CONFIG-006 using FND-SOUND-020's chunk-field
  destinations/output copies and FND-SOUND-037's complete local scalar
  dispatch, including SamplePlaybackMode's zero/one gate. Those stores
  alone do not establish all consumer effects.
  Blocks: completing the named entry reading.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-CONFIG-009. RULE-CONFIG-005: Does the Save Game completion message pass
  the `WIND/10501` gate and wait in the owner's installed GOG build, and
  does changing Message Delay change its visible duration? Settles it:
  the bounded two-setting Save Game comparison in
  `docs/live-sessions/preferences.md`. Tried: FND-CONFIG-046 locates the
  success branch's direct message call; FND-CONFIG-018, FND-CONFIG-030
  through FND-CONFIG-034 and FND-CONFIG-065 identify the acquisition
  gates and shipped record. The code does not decide the live I/O outcome
  or observed duration. Blocks: slice 3 message timing validation.

- Q-CONFIG-001. RULE-CONFIG-001, SCR-UI-007, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004, RULE-CONFIG-005, FMT-CONFIG-003: What are the native
  Preferences defaults and control frames, the visible message-delay and effect-volume
  endpoints, the difficulty and description placement, and the
  About page's layout and dismissal? Settles it: the preferences live session.
  Tried: the strings and saved resource (FMT-TEXT-004, FND-CONFIG-001),
  renderer (FND-CONFIG-009, FND-UI-033), hover routine (FND-UI-034) and click
  dispatcher (FND-CONFIG-010) establish labels and button transitions, but
  cannot establish new-game defaults or native drawn states. FND-CONFIG-218
  reads the `F4`, `F5`, `A` and `F6` handlers and takes what `F6` toggles out.
  Blocks: slice 3.

## Source

None.

## Blocked

- Q-CONFIG-004. FMT-CONFIG-005: What is the structure and role of `PATCH.RTP`? Settles it: a
  reading of the program `PATCH.EXE` keeps compressed after its load image, unpacked by a
  committed tool. Tried: FND-CONFIG-220 finds that no uncompressed shipped program or batch file
  names the file and that `PATCH.EXE`'s program is compressed; FND-EXE-570 gives the size
  pattern. Waiting on: an unpacker for `PATCH.EXE`'s compressed program, which needs a reading of
  its loader's decompression routine. Blocks: Survey format coverage.
