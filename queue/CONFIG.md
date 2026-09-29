# CONFIG

Next ID: Q-CONFIG-011

## Static

- Q-CONFIG-002. FMT-CONFIG-003, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004: What initializes the settings for a new game, does any path
  synchronize the saved speech gate and runtime voice button state, and can
  music volume be adjusted elsewhere? Settles it:
  bounded readings of the sound-library consumers, new-game initialization and voice
  synchronization paths. Tried: the save/load copies (FND-SAVE-004, FND-SAVE-005),
  renderer (FND-CONFIG-009, FND-UI-033), Preferences hover routine (FND-UI-034)
  and button dispatcher (FND-CONFIG-010) identify the difficulty, effect-volume,
  music/effects enable and animation-state fields and the ordinary button steps.
  FND-CONFIG-012 identifies byte `0x02` as the sound library's music-level
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
  without excluding indirect control. These readings
  do not identify new-game values, animation
  polarity, any synchronization beyond those paths or another music-level control. Blocks:
  slice 3.

- Q-CONFIG-007. RULE-CONFIG-005: Does another path impose an upper limit on
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

- Q-CONFIG-008. RULE-CONFIG-005: Which caller paths enter overlay 172's
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
  the `WIND/18501` callback. FND-CONFIG-079 identifies the guarded resident
  event-dispatch route into that callback. FND-CONFIG-080 identifies two
  event-record discriminators for the callback's message branch.
  FND-CONFIG-081 traces a conditional pointer-hit producer for the first;
  physical input mapping and live state remain open. FND-CONFIG-082 shows
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
  window state remain open. FND-CONFIG-090 traces the callback's unsigned
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
  Runtime metadata,
  bound/slot writers and actual interrupt outcomes remain open.

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

- Q-CONFIG-006. FMT-CONFIG-002: How does the setup program parse `SOUND.INI`, including unknown
  or malformed keys, and what do `CardGroup` and the chunk numbers mean? Settles it: the parser
  at `SOUND_DS.EXE` `1AF6:0BF9` (FND-CONFIG-004) and its consumers. The file layout alone does
  not determine parser behavior (FND-CONFIG-007).

- Q-CONFIG-004. FMT-CONFIG-005: What is the structure and role of `PATCH.RTP`? Settles it: a
  bounded inspection of the file and references from the installed setup programs. Blocks:
  Survey format coverage.

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

- Q-CONFIG-001. SCR-UI-007, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004, RULE-CONFIG-005, FMT-CONFIG-003: What are the native
  Preferences defaults and control frames, the visible message-delay and effect-volume
  endpoints, what `F6` toggles, the difficulty and description placement, and the
  About page's layout and dismissal? Settles it: the preferences live session.
  Tried: the strings and saved resource (FMT-TEXT-004, FND-CONFIG-001),
  renderer (FND-CONFIG-009, FND-UI-033), hover routine (FND-UI-034) and click
  dispatcher (FND-CONFIG-010) establish labels and button transitions, but
  cannot establish new-game defaults or native drawn states. Blocks: slice 3.

## Source

None.

## Blocked

None.
