# COMBAT

Next ID: Q-COMBAT-067

## Static

- Q-COMBAT-042. FMT-COMBAT-001, SCR-COMBAT-001, RULE-AI-001: What does object 4 hold, which
  loads or scripts put a combatant there, and is it ever `turn_combatant`? Does a creature's own
  combatant record keep its shared details record from being freed when the creature is released?
  Settles it: the callers of `2D40:000A` and `31E0:0EFF` that pass object 4, the writers of
  `57E0:426D`, and the order of the kind-2 and kind-3 releases of `2D40:248D`, with overlay 187
  `+1B67`, `2D40:0E80` and `2D40:0EB4`. Tried: FND-COMBAT-072 reads the allocators and finds
  that they never hand out records 0 to 4; RULE-MAGIC-012 already places a summoned creature in
  object 4; FND-PARTY-119 and FND-UI-047. Blocks: slice 4.
- Q-COMBAT-043. FMT-COMBAT-001, FMT-COMBAT-002: What do combatant byte `+0x17` (field 9) and
  details bytes `+0x36` (field 77) and `+0x41` (field 86) hold? Settles it: the scripts that
  compare field 77, and any reader of field 9 of a kind-2 object. Tried: FND-COMBAT-074 finds no
  executable reader of any of them and tallies the scripts' uses; FND-PARTY-151, FND-PARTY-152
  and FND-PARTY-159. Blocks: slice 4.
- Q-COMBAT-007. RULE-COMBAT-007, RULE-CONFIG-002: What do the 47 script calls of function 21 of
  instruction 0x22 do with difficulty? Settles it: follow the setting through all 19 named
  script resources of FND-COMBAT-075 to their consumers, including child preservation of
  MAS/67 local word 0 before claiming its total attempts. Tried: FND-COMBAT-065 finds the
  enemies' to-hit term; FND-COMBAT-075 reads every executable reader of DS:143A and the hit
  point scaling. FND-COMBAT-076 reads the count helper and MAS/67 loop; its triangular totals
  remain conditional on child preservation. The independent save-load question is Q-COMBAT-044.
  Blocks: slice 4.
- Q-COMBAT-044. RULE-COMBAT-007: Do the remaining direct load continuations reload creature
  resources or reset restored current or maximum hit points? Settles it: the reached noncallback
  children and failure continuations of FND-SAVE-022, FND-COMBAT-077, FND-COMBAT-079,
  FND-COMBAT-080 and FND-COMBAT-081, after binding caller admission (Q-COMBAT-046), saved-table
  producers (Q-COMBAT-047), callbacks (Q-COMBAT-048) and temporary-slot admission (Q-COMBAT-045).
  Tried: split from Q-COMBAT-007. FND-COMBAT-075 reads resource-load scaling;
  FND-COMBAT-077 excludes the flags-zero RDFF branch and reads coordinate adjustment;
  FND-COMBAT-079 excludes the region object-rebuild block; FND-COMBAT-080 reads cleanup
  ordering; FND-COMBAT-081 reads direct-caller result handling. Their independent remaining
  admission, producer and callback obligations now have separate items. Blocks: slice 4.
- Q-COMBAT-046. RULE-COMBAT-007, RULE-SAVE-002, SCR-UI-013, SCR-UI-014: Which admitted window events and state producers reach the
  numbered-load call and reader roots? Settles it: remaining producers and lifetimes of
  50AF:0004, 4C4C:0000, 4C4C:0002 and DS:627C beyond the explicit mode entries and
  local navigation of FND-COMBAT-085 and sentinel producer/own reset ordering
  of FND-COMBAT-131; actual DS:A103 count/target state at dispatch beyond
  FND-CONFIG-221's branch exclusions, FND-CONFIG-222's count transitions and
  FND-COMBAT-133's single admitted F2 entry and FND-COMBAT-134's Down
  call-to-callback delivery and FND-COMBAT-135's returned empty-list Down
  case, and FND-COMBAT-136's Enter-to-numbered-load call for row one/top zero;
  event delivery, snapshot/slot producers and
  preservation and live window/handler identity beyond FND-COMBAT-098's three
  restoration-site orderings; window-path incoming transfers not covered by
  FND-UI-051's direct caller census. Independent reader routes are Q-COMBAT-055.
  Tried: split from Q-COMBAT-044; FND-COMBAT-081 settles local confirm-table mapping and
  ignored load result, while FND-SAVE-022 gives reader order. FND-COMBAT-084 settles
  the wrapper's AL/AX tests, close-obligation byte and replacement of the reader result
  by the close result. FND-COMBAT-085 settles local navigation's asymmetric wrap and
  empty-result selection; FND-UI-051, FND-SAVE-018 and FND-SAVE-026 supply existing
  caller, exit-byte and shared-page producer evidence. The remaining admission and
  preservation obligations above are not settled by those local results. Blocks: slice 4.
  Tried: FND-COMBAT-098 binds the three restoration sources named by FND-UI-051:
  one late local snapshot and two shared-slot rereads, with their gates and later
  window work. Their own restore-call ordering is removed from Settles it. Actual
  caller/slot admission, preservation, handler/window identity at event delivery
  and remaining incoming reader routes are still open.
  Tried: FND-COMBAT-099 resolves the script sentinel/argument widths and the
  current-region equality gate. Its independent reader admission is split into
  Q-COMBAT-055; this item retains the window/event producer question.
  Tried: FND-COMBAT-131 connects the character-list FFFF producer and own
  cleanup behavior to Save/Load's absent own reset and marker-before-movement
  ordering. Those local sentinel/reset obligations are removed. Actual scan
  outcome, returning children, field lifetime and event admission remain;
  ten controls do not alone admit an initial row in 0..9.
  Tried: FND-CONFIG-221 corrects zero-result dispatch and binds the count-zero
  event-kind rewrite. A window result has no own global fallthrough; a
  preserved count-zero global call supplies kind three and cannot take the
  Save/Load action branch. Those routing alternatives are settled. Actual
  count, targets, event fields and preservation on remaining routes stay open.
  Tried: FND-CONFIG-222 resolves the three literal count writers, wrapped
  mutation relation and signed-walk versus dispatch predicates. Those local
  count-contract obligations are removed. Admit actual mutation/reset history
  or current count and graph directly; failed operations and a nonzero count
  do not independently establish live membership or an admitted target.
  Tried: FND-COMBAT-132 excludes all 46 fixed nominal SAVE payload spans
  from eight named UI admission regions, with fresh table checks and
  intersection/boundary controls. That payload-only writer family is removed
  under the stated pointer/length conditions; actual lengths, dynamic or
  aliased pointers, direct producers and child writes remain unadmitted.
  Tried: FND-COMBAT-133 supplies one admitted map F2 entry, its count-one
  null-target/global-hook state, and final Load count two/top zero/row one.
  Initial values for this route are removed from the remaining admission
  gap; action events, other routes, callback preservation and lifetimes stay open.
  Tried: FND-COMBAT-134 observes Down's kind-six/5000 event, global hook and
  return frame at the actual call and Load callback entry. That navigation
  delivery boundary for row one/top zero is removed; branch execution,
  numbered-load actions, child preservation and other histories remain open.
  Tried: FND-COMBAT-135 observes the matched Down callback return with ten
  empty markers, row one becoming zero, top zero and the named entry/return
  fields. That execution case is removed; other markers/rows/tops, later
  dispatch preservation and numbered-load action admission remain open.
  Tried: FND-COMBAT-136 admits Enter's kind-six/1C0D event and the actual
  +05ED call with argument one after window cleanup, for top zero and ten
  empty entry markers. That action route and its post-cleanup row/mode/list
  values are removed; other states/routes and later preservation remain open.
- Q-COMBAT-055. RULE-COMBAT-007: Which actual script states and other incoming routes
  admit the independent numbered reader +0A6C, including selector zero?
  Settles it: execution admission and parameter preservation at instruction 0x5E,
  after the operand domains in Q-COMBAT-056/Q-COMBAT-057/Q-COMBAT-058 and
  gap reading in Q-COMBAT-059; current DS:140C producers and lifetimes; preparation
  outcomes before +1132's read, and incoming transfers beyond FND-UI-051's direct
  caller census for the reader and its two wrappers. Reuse FND-COMBAT-035,
  FND-COMBAT-043 and FND-COMBAT-044 for local reader/wrapper behavior.
  Tried: split from Q-COMBAT-046. FND-COMBAT-099 establishes the full-width
  sentinel test, three low-word arguments and current-region equality gate;
  it does not establish actual operand values, preparation or transfer completeness.
  Tried: FND-COMBAT-100 excludes zero selectors at 207 decoded literal sites.
  The eleven variable-selector producers and nonliteral sentinel/gap obligations
  are split into Q-COMBAT-056 through Q-COMBAT-059; executable admission remains here.
  Blocks: slice 4.
- Q-COMBAT-056. RULE-COMBAT-007: Can the three local-number selector sites in
  GPL/4, GPL/137 and GPL/163 supply zero to the independent numbered reader?
  Settles it: actual entry/compare-frame admission and local-bank identity,
  lifetime and intervening preservation for FND-COMBAT-104's declared-path
  assignments; other admitted entry sources and returning cross-script paths
  beyond its fixed-listing boundaries. Include entry after compare initialization,
  possible aliases and opcode/child effects before the selector parameter is read.
  GPL/119's separate shared-prefix preservation contract is Q-COMBAT-060;
  the independent named-context contract is Q-COMBAT-058.
  Tried: split from Q-COMBAT-055; FND-COMBAT-100 enumerates the eleven sites,
  but source presence and a local context producer do not establish actual values.
  FND-COMBAT-101 maps the 38 declared nonzero literal entry controls and
  the 22 nonzero direct assignments for the four local selectors. The four
  entry-zero walks return immediately and are not used as an admission negative.
  Assignment coverage, intervening preservation and other entry sources remain.
  FND-COMBAT-102 separates the named-context setter/argument reading into
  Q-COMBAT-058 without treating it as local-storage preservation.
  Tried: FND-COMBAT-104 records matched local-call histories for all 38 controls,
  reproduces the four positive target counts and the 22 named assignments, and
  reads GPL/4's nine-case/default assignment correlation. Its abstract bypass
  is not an admitted zero or assignment omission. The bounded direct-history
  assessment is removed from Settles it; actual admission, storage preservation
  and unmodeled returns remain above.
  Tried: FND-COMBAT-105 reads GPL/119's intervening 0x0522 relocation and
  conditional native-request prefix. Its selector 65 excludes its own encoded
  zero-reader branch but proves no preservation for the later local read.
  The visible backup/restore pair follows that read in a separate local routine;
  native-child effects and bank preservation move to Q-COMBAT-060, leaving
  the other three selector sites here.
  Blocks: slice 4.
- Q-COMBAT-060. RULE-COMBAT-007: Does GPL/119's shared prefix preserve the
  assigned local-number selector until instruction 0x0547 reads it?
  Settles it: admitted entry/frame and local-bank identity/lifetime for the four
  literal assignments and calls of 0x04E4 in FND-COMBAT-101/FND-COMBAT-104;
  preservation through the prefix's conditional native requests 9/49, nested
  instructions 0x1E/0x80, instruction 0x41 and earlier relocation 0x0522, and
  its common request 37. Include remaining callbacks, aliases, error paths
  and local clears of FND-SCRIPT-020/FND-SCRIPT-026. Request 9's independently
  answerable trigger admission and interpreter continuation are Q-COMBAT-062.
  Q-COMBAT-055 retains the overall reader/parameter admission contract.
  Tried: split from Q-COMBAT-056 by FND-COMBAT-105. The earlier relocation's
  selector 65 excludes its own encoded zero-reader branch; it does not prove
  preservation of the later local. The visible requests 29/30 are after
  0x0547 in a different local routine and supply no bracketing proof for
  this prefix. Native-child effects and actual bank preservation remain open.
  Tried: FND-COMBAT-107 connects request 9's group replacement to the loader's
  conditional trigger call and the script run-entry resets. Its actual trigger
  admission and later continuation move to Q-COMBAT-062, rather than treating a
  native return as proof that the old frame reaches 0x0547 with cleared locals.
  Blocks: slice 4.
- Q-COMBAT-062. RULE-COMBAT-007: Does GPL/119's request 9 admit a nested trigger
  run, and can the old prefix then resume at its 0x0547 selector?
  Settles it: actual -49 lookup matches and load/record state reaching the
  31E0:02F6 call, the later DS:0DAB and script-state gates, selected cell/box
  trigger and target, plus interpreter frame/stream/stop/local-bank continuation
  through the nested run and native return. Preserve rejected and no-match paths
  separately; distinguish local clearing from reaching the later selector.
  For the unchanged category-two route, FND-COMBAT-108 excludes MAS/65's
  eighteen encoded category-zero boxes without a coordinate proof. Establish
  another admitted record, a smaller-slot/category path or changed category/record
  state to admit reentry; retain actual list identity and preservation.
  Reuse Q-COMBAT-003 for the DS:0DAB state producers instead of duplicating them.
  Tried: split from Q-COMBAT-060. FND-COMBAT-107 connects the encoded request,
  mode-bit producer, loader's conditional trigger call and run-entry clear/reset.
  FND-SCRIPT-007, FND-SCRIPT-020, FND-SCRIPT-026 and FND-SCRIPT-028 give local
  contracts, not actual trigger admission or preservation of the outer run.
  Other prefix effects remain Q-COMBAT-060. Blocks: slice 4.
  Tried: FND-COMBAT-108 reproduces the 150/321 global cell/box controls and
  reads MAS/65's eighteen literal category-zero registrations against the loader
  and walker predicates. Those unchanged records cannot select a category-two
  event; active-list/slot/category admission and outer continuation remain above.
- Q-COMBAT-058. RULE-COMBAT-007: Can the fifteen kind-9 number-36 selector sites
  in FND-COMBAT-100/FND-COMBAT-105 supply zero while current DS:140C is nonzero?
  Settles it: admitted context values and entry/callback timing at those sites;
  preserve the first-operand conditions of Q-COMBAT-057 and Q-COMBAT-061
  for the eight additional sites;
  preservation or replacement of the initial named pointer; all later writers,
  including bulk restore destinations; and context/DS:140C/stack-argument
  preservation through the callers and children in FND-COMBAT-102. Include
  incoming transfers outside its direct-call census. Reuse FND-PARTY-241,
  FND-SCRIPT-028, FND-SAVE-015 and FND-COMBAT-035.
  Tried: split from Q-COMBAT-056. FND-COMBAT-102 reads the setter's store
  before its state gate, both known caller argument producers and the separate
  conditional save destinations. The displacement candidates have one checked
  setter and read-only comparison controls, with indirect/bulk and other
  segment-formation exclusions. This does not establish equality with DS:140C
  or admit a zero-context execution. Blocks: slice 4.
  FND-COMBAT-105 adds eight conditional-sentinel sites sharing this same
  named context source; its initial pointer binding is not a lifetime proof.
- Q-COMBAT-057. RULE-COMBAT-007: Can nonliteral first operands admit additional
  instruction 0x5E zero-selector routes?
  Settles it: admitted values, pointer bindings and lifetimes of named words 38
  and 40 at GPL/176 0x0451 and GPL/301 0x073D; can they read
  0x7FFE under the signed-word getter while the context selector is zero and
  DS:140C is nonzero? Keep joint timing and parameter preservation explicit;
  Q-COMBAT-058 owns the shared selector source. The independent
  GPL/37 gap is Q-COMBAT-059. Preserve independent incoming
  transfers in Q-COMBAT-055. Tried: split from Q-COMBAT-055; FND-COMBAT-100's
  literal positive control reproduces 218 sites but cannot exclude these cases.
  FND-COMBAT-103 separates the native malformed-expression continuation from
  decoded nonliteral value domains; it does not exclude either route.
  Tried: FND-COMBAT-105 excludes the other 98 sites' own encoded zero selectors
  without requiring their first values, reproducing all FND-COMBAT-100 controls.
  The eight retained first operands are named words 38, 40 and 44, with initial
  pointer bindings checked through actual relocations. Actual domains and
  pointer/field preservation remain above; no native absence is claimed.
  Tried: FND-COMBAT-106 separates the six number-44 sites' producer proof into
  Q-COMBAT-061. This item keeps the two other first-word sites; shared context
  and joint timing remain Q-COMBAT-058, not a presumed object-index bound.
  Blocks: slice 4.
- Q-COMBAT-061. RULE-COMBAT-007: Can named word 44 supply the sentinel at
  GPL/163 0x148D, GPL/185 0x0885/0x0893/0x0A49/0x0A57 or GPL/301 0x0748?
  Settles it: admitted setter input domains, other field writers, initial-pointer
  preservation and field lifetime at those six reads, including retained values
  on failed lookup and narrowing before lookup. Bind their 0x7FFE reads jointly
  to the zero-context/nonzero-current-region condition of Q-COMBAT-058; keep
  entry and parameter preservation in Q-COMBAT-055. Reuse FND-RNG-068 for
  the known native replacement producer rather than treating 0x41 as the only writer.
  Tried: split from Q-COMBAT-057. FND-COMBAT-106 reads the full local setter
  and distinguishes 16 negative literal inputs from 299 named reads and one path
  expression. The literal inputs cannot directly produce 0x7FFE under the stated
  lookup-preservation conditions; other inputs, retention and native writers
  remain. Full-width sign is not a bound after word narrowing. Blocks: slice 4.
- Q-COMBAT-059. RULE-COMBAT-007: Can GPL/37's malformed-expression span admit
  another instruction 0x5E zero-selector route?
  Settles it: admitted source/buffer mutations or fetch/dispatch-context changes
  around 0x0D37..0x0D58 that can supply another 0x5E route; the script-error
  children and their frame, segment, stream, saved-value, operand-address and
  stop-byte effects; the opcode callback and dispatch-argument/table preservation;
  and the two-parameter opcode-0x77 handler's actual indirect-write target and
  continuation into the interpreter stop check. FND-COMBAT-109 excludes an
  encoded 0x5E at every byte alignment in the unchanged 33-byte span.
  Keep the incoming-transfer obligation of Q-COMBAT-055 separate and the
  earlier script/operand context explicit. Reuse FND-SCRIPT-023 and Q-SCRIPT-003
  for the error-child contract, not a terminating-error assumption.
  Reuse Q-SCRIPT-018 for opcode-hook target admission rather than assuming
  the optional callback has no target.
  Tried: split from Q-COMBAT-057. FND-COMBAT-103 verifies the shipped byte's
  native default-table target, its returning local error continuation, the
  parameter loop without an inter-operand stop check and the handler's later
  indirect byte write. No patch, admitted execution or pointer/stop preservation
  follows from those local bodies. FND-SCRIPT-024's recovery remains an analysis
  boundary, not original-game recovery.
  Tried: FND-COMBAT-109 repeats the fingerprinted 33-byte read and finds no
  0x5E, with independent GPL/119 0x0547 and 0x77/0xB3 controls. Native fetch
  saves and reloads a byte around its cursor/error child, and dispatch rereads
  its stack byte after an optional callback. Encoded-byte uncertainty is removed;
  mutation, context/argument/target preservation and actual admission remain.
  Blocks: slice 4.
- Q-COMBAT-047. RULE-COMBAT-007: Which saved-resource and allocation contracts admit restoring
  SAVE/5 and SAVE/6 into the combatant and details tables? Settles it: admitted DS:193C
  bypass states, remaining nonliteral pointer writers and destination/alias lifetimes beyond
  the declared startup, failure and exit-table routes of FND-COMBAT-086; actual
  archive/resource-length provenance for these two entries; and later pointer replacement.
  Tried: split from Q-COMBAT-044; FND-COMBAT-072 gives 42 combatant and 17 details records;
  FND-SAVE-015 and FND-SAVE-021 give table lengths and pointer binding.
  FND-COMBAT-082 settles pointer forwarding, the missing capacity argument, nominal
  transfer and loop failure behavior; FND-COMBAT-083 accounts for explicit half-pointer
  stores and unchanged release aliases. FND-COMBAT-086 binds startup failure to the
  termination wrapper and the known release route to exit registration, distinguishing
  returning runtime wrappers. The remaining obligations above stay open. Blocks: slice 4.
  Tried: FND-COMBAT-112 excludes all 26 unchanged fingerprinted top-level
  shipped GFF files as SAVE providers. The remaining resource provenance is
  runtime-written/copied or other admitted archives, not a search for a
  shipped SAVE table in those files. Handle/list, directory/length lifetime,
  transfer and destination admission remain.
- Q-COMBAT-048. RULE-COMBAT-007: Which other callbacks reached during numbered loading can
  change restored creature health or select a different continuation? Settles it: admitted
  targets and effects of input/window callbacks reached by cleanup, region setup and caller
  continuations of FND-COMBAT-079, FND-COMBAT-080 and FND-COMBAT-081, with target rereads
  and preservation after each callback; APFM +5A/+5E record admission is Q-COMBAT-049.
  Tried: split from Q-COMBAT-044; FND-CONFIG-187 and FND-CONFIG-189 read local service
  branches. FND-COMBAT-087 separates the APFM record-admission dependency from the
  remaining callbacks, rather than treating a selector reading as their effects. Blocks: slice 4.
- Q-COMBAT-049. RULE-COMBAT-007: Can numbered loading reach a nonzero APFM +5A or +5E
  callback, and if so which target? Settles it: combine the independently evidenced
  acquisition (Q-COMBAT-050), record identity and remaining writers (Q-COMBAT-051),
  and mode admission (Q-COMBAT-052) for the numbered-load paths; identify the concrete
  target and its effects if nonzero fields are admitted, or exclude both dispatches
  on every admitted path. A conditional zero-field reading alone does not settle it.
  Tried: split from Q-COMBAT-048. FND-UI-005, FND-PARTY-044 and FND-EXE-588
  supply shipped zeros and the bounded field census; FND-COMBAT-087 through
  FND-COMBAT-091 remove bounded selector/setup/paint/text questions. The remaining
  independent acquisition, storage/writer and mode obligations are split below;
  no live callback target or whole-load exclusion has been established.
  Blocks: slice 4.
- Q-COMBAT-050. RULE-COMBAT-007: Which archive and transfer paths can supply an APFM
  child during numbered loading? Settles it: active archive, traversal-mode and open-list
  producers reaching the FND-COMBAT-089 acquisition call; selected type/number/length,
  allocation and seek/read admission, including rejection and alternative-archive paths.
  Distinguish successful transfer from merely finding a shipped directory entry. Preserve
  unresolved external I/O outcomes explicitly rather than assuming success.
  Tried: split from Q-COMBAT-049. FND-COMBAT-089 binds the null local destination
  and sole APFM provider among 26 inspected archives; FND-CONFIG-038,
  FND-CONFIG-039, FND-CONFIG-040, FND-CONFIG-151 and FND-CONFIG-208 supply
  lookup, startup and wrapper gates, without proving their state at the load call.
  Blocks: slice 4.
- Q-COMBAT-051. RULE-COMBAT-007: Which records and writes supply the APFM callback
  fields read through A125 and A15B on numbered-load paths? Settles it: graph and
  child-table producers beyond the registration path of FND-COMBAT-093, preservation
  through their calls, payload identity, storage lifetime and remaining construction/copy/write paths for both
  saved and reread pointers; account for block copies, alternative bases and split
  pointer stores beyond the bounded census. Bind aliases and text-source termination
  where the exclusions of the existing helper readings require them. Acquisition
  admission is Q-COMBAT-050; selected-mode admission is Q-COMBAT-052; parent-window
  source and initial buffer admission is independently tracked by Q-COMBAT-053.
  Tried: split from Q-COMBAT-049, with parent-window input admission now split to
  Q-COMBAT-053. FND-COMBAT-087 separates saved and reread pointers;
  FND-COMBAT-088, FND-COMBAT-090 and FND-COMBAT-091 bound conditional setup,
  paint and text writes. FND-COMBAT-092 and FND-UI-068 map local selection;
  FND-COMBAT-093 and FND-COMBAT-094 map this registration producer and connected
  status/output propagation. Those bounded parts are removed from Settles it.
  FND-PARTY-044 and FND-EXE-588 retain the other-writer census limits; actual
  payload identity, aliases, lifetime and preservation remain unanswered.
  Blocks: slice 4.
- Q-COMBAT-053. RULE-COMBAT-007: Which source bytes and allocation/transfer states
  supply the window buffer passed into registration during numbered loading?
  Settles it: allocation extent and initialized contents, and the resulting
  header/child-table bytes on the admitted complete,
  early-rejection and post-allocation failure paths. Trace the input producers and
  preservation needed for the actual window call; retain I/O and runtime limits
  explicitly. Local status/output propagation is already FND-COMBAT-094; later
  graph/table mutations and callback-byte writers remain Q-COMBAT-051. Live
  archive/directory and selected resource/length admission is split to Q-COMBAT-054.
  Tried: split from Q-COMBAT-051, with archive-state admission now separated
  into Q-COMBAT-054. FND-COMBAT-040 and FND-COMBAT-093 identify the request
  and registration order; FND-COMBAT-094 binds the returned allocation slot, and
  FND-COMBAT-095 supplies the fingerprinted provider/source bounds. Those
  bounded parts are removed from Settles it. FND-CONFIG-151, FND-CONFIG-209
  and FND-CONFIG-210 retain allocation, marker, clearing and I/O limits; nonnull
  does not establish complete source bytes, valid capacity or native failure.
  Blocks: slice 4.
- Q-COMBAT-054. RULE-COMBAT-007: Which archive/directory state and selected resource
  can reach the window reader on the numbered-load routes? Settles it: actual caller
  ordering and input admission before each relevant request, plus the state, link,
  handle and catalog producers for the known selection/open/close/growth and
  traversal-mode mechanisms. Account for indirect and copied/aliased writes beyond
  the retained literal censuses. Use Q-COMBAT-046 for caller/event admission;
  Q-COMBAT-053 retains allocation and transferred-buffer contents.
  Tried: split from Q-COMBAT-053. FND-COMBAT-095 bounds on-disk providers;
  FND-COMBAT-096 maps local selection/preparation/close transitions.
  FND-COMBAT-097 binds the Save/Load row reader's own temporary handle and
  result ordering, including its absence of a close-AX test. Those local paths are
  removed from Settles it; their admission and preservation at a particular later
  window request remain. Reuse FND-CONFIG-037, FND-CONFIG-041,
  FND-CONFIG-068, FND-CONFIG-069 and FND-PARTY-042 for the census and
  startup limits; a row marker does not establish archive cleanup success.
  Tried: FND-COMBAT-126 resolves growth's local snapshot/release/allocation,
  file-directory reload and conditional global/neighbor publication order.
  These local producer effects are removed from unread work; actual graph membership,
  valid allocations/links, input admission and external outcomes remain.
  Tried: FND-COMBAT-127 resolves the opener's own header/allocation gates,
  record construction, directory reload, accounting-before-I/O and cleanup
  ordering. These local producer obligations are removed; actual caller
  inputs, graph membership, bounds, aliases and external outcomes remain.
  Blocks: slice 4.
- Q-COMBAT-052. RULE-COMBAT-007: Which state producers admit APFM modes that dispatch
  +5A or +5E during numbered loading? Settles it: producers and ordering of A13D,
  A155, A141 and the services' blocking/selection state on each load path, including
  rejected or bypassed services and preservation between target check and target read.
  Keep current A125, retained A15B and the target rereads distinct. Record origins and
  callback-byte writers are Q-COMBAT-051; this question establishes mode admission.
  Tried: split from Q-COMBAT-049. FND-COMBAT-087 supplies conditional selector
  transitions; FND-CONFIG-189 supplies distinct pre/post replacement service checks
  and ordering, without native input or complete caller/producer admission.
  Blocks: slice 4.
- Q-COMBAT-045. RULE-COMBAT-007: Can a numbered saved game made by the original hold a
  populated presentation slot 319, and what does loading that slot do to restored creatures?
  Settles it: indexed and bulk producers of slot 319's presentation entry-index word, save
  call admission relative to temporary-slot lifetime, and, if admitted, its free-list and
  reclamation children on the load path. Tried: split from Q-COMBAT-044; FND-COMBAT-078
  accounts for three fixed-address copy/clear sites but excludes indexed and restored writes.
  FND-PARTY-119 covers the free-list setup and FND-COMBAT-054 the reclamation effect route.
  Tried: FND-COMBAT-165 removes kind resurrection/reparenting/dispatch in admitted finite rooted deletion;
  actual graph/pool admission, earlier enumeration, later release and save timing remain.
  Tried: FND-COMBAT-164 removes root kind ordering/childless cleanup; other state, presentation emptiness and timing remain.
  Tried: FND-COMBAT-163 removes RDFF status propagation/-2-only parent failure; actual replacement needs state/handler/provider admission.
  Tried: FND-COMBAT-162 removes canonical-source high selectors; actual provider/length, mutations/aliases and other lifetime classes remain.
  Tried: FND-COMBAT-161 removes synchronous save/input re-entry in admitted flags-zero success/low-image states; other lifetime cases remain.
  Tried: FND-COMBAT-160 removes per-record wrappers' extents/results and immediate slot sources; actual length/provider, slot/kind, aliases and timing remain.
  Tried: FND-COMBAT-159 removes sorted adjustment's FFFF case; direct writes require caller/free-list admission and other producers/aliases/timing remain.
  Tried: FND-COMBAT-149 removes retries/eviction own gates including 0B4A owner removal; actual admission, free-list/victim and child/alias effects remain.
  Tried: FND-COMBAT-150's empty/no-eviction baseline proves no populated-slot admission; do not repeat it for reclamation.
  Blocks: slice 4.
- Q-COMBAT-003. RULE-COMBAT-008, SCR-UI-022, RULE-INPUT-003: Which values do `combat_state` and the word at `57E0:0DAB` take,
  and which code sets each? Settles it: the seven direct stores to `combat_state` of
  FND-COMBAT-023, read in their routines, and the writers of `57E0:0DAB`. Tried: an immediate
  store search (FND-COMBAT-023) and Ghidra's references to `57E0:0DAB` (FND-COMBAT-011). Blocks:
  slice 4.
- Q-COMBAT-004. RULE-COMBAT-004, SCR-UI-022: What does the routine behind the stub `5682:004D` of overlay
  174 do with the `N` and `P` keys? What callback does the key routine pass to `Q`, and what is
  `g_57E0_143C`? Settles it: a reading of overlay 174 from its code offset `0x0924`, and the
  callers of the key routines at overlay 190 offset `0x139B` and `28C9:0CFF`. Tried: a search for
  the six scancodes together (FND-COMBAT-001, FND-INPUT-008), the overlay 190 key routine
  (FND-COMBAT-025) and the resident key routine (FND-AI-004). Blocks: slice 4.
- Q-COMBAT-038. RULE-COMBAT-002, RULE-COMBAT-003, RULE-COMBAT-006: Which effects set the block bits
  and effect codes that overlay 197 `+259E`, `+27A7`, `+2499`, `+0E8F`, `+111A` and `+1C52` test in
  a weapon attack, and what do the attack routine's location 0x11, type bytes `+1`, `+2`, `+6` and
  `+0x0B` and the type words of `DS:2588` stand for? Settles it: the writers of the block bits
  and the shipped item types' and creatures' values. Tried: FND-COMBAT-065 to FND-COMBAT-069 give
  every term by bit and value. Blocks: slice 4.
- Q-COMBAT-040. RULE-COMBAT-003: How does the player order an attack on a chosen combatant, and
  does that path test the target's `combat_mark`? Does `31E0:05BB` free the slot of an unloaded
  combatant? Settles it: the writers of `4F49:08A3` and the rest of `31E0:05BB`. Tried:
  FND-COMBAT-069 reads the doubled damage; FND-COMBAT-071 reads the computer's target choice, the
  turn order and every writer of `combat_mark`. Blocks: slice 4.
- Q-COMBAT-006. RULE-COMBAT-004, RULE-COMBAT-005: What admitted state
  and later consumers give the guard, wait and end-turn stores their
  gameplay meaning? Settles it: producers, readers and lifetime of the
  byte at 4E71:0B1A plus mapped index and words at 4C4F:00DC plus four
  times index and 4C4F:01D8 plus twice index; complete reached command
  callers, mapping/type/index and segment/storage admission, and the
  connection to later turn selection. Tried: FND-COMBAT-047 answers
  both complete local helper bodies, signed wait clamp/decrement,
  guard-before-resolver store, zero-mask versus OR branches and known
  menu/keyboard argument routes. These local branches are removed from
  the settling condition; native gameplay semantics and full state/caller
  admission remain. FND-COMBAT-048 separately answers the local anchor
  predicate. Independent adjustment, menu-result and postprocessing
  obligations move to Q-COMBAT-014/015/016. Blocks: slice 4.
- Q-COMBAT-014. RULE-COMBAT-005: What does the zero-predicate adjustment
  through 2C5F:018C leave for the menu under admitted runtime state?
  Settles it: complete bound/slot/segment and independent-storage
  admission, writer/lifetime coverage through adjustment, child effects
  and preservation through the menu's rereads, and final presentation.
  Tried: split from Q-COMBAT-006 after FND-COMBAT-048 settles the
  inclusive wrapped-unsigned anchor predicate and its caller's
  conditional adjustment and coordinate-reload route; that predicate
  does not establish successful view adjustment.
  Tried: FND-COMBAT-049 answers the wrapper's low-byte mode and anchor
  arguments, all local aligned/signed/wrapped view calculations, bound
  test order, later child-call sequence and byte-versus-word flag
  widths. FND-COMBAT-050 answers request-33 bound packing and request-38
  coordinates/results, reset constants and the local restore stores.
  Those local calculation and identified-producer obligations are
  removed from Settles it; complete admitted inputs/writers, lifetime,
  child preservation and presentation remain. Blocks: slice 4.
- Q-COMBAT-015. RULE-COMBAT-005: Which results does the menu at
  566A:0025 return for selection and cancellation? Settles it: reached
  construction/control admission, choice initialization and storage identity,
  native event/callback delivery, admitted child-result domains and
  choice/register preservation through cleanup, connecting actual selection
  and cancellation to the already-read caller branches.
  Tried: split from Q-COMBAT-006; FND-COMBAT-026 and FND-COMBAT-047
  establish the caller's exact one/two/three branches, but not the
  producer's cancellation and selection results.
  Tried: FND-COMBAT-051 answers the local type-two seven-key selector,
  type-six first-byte/011B branches, distinct handler-versus-choice results,
  first-nonzero child-result precedence and cleanup-before-return order.
  These local mapping/return obligations are removed from Settles it;
  initialization, admitted children/storage and native event delivery remain.
  Blocks: slice 4.
- Q-COMBAT-016. RULE-COMBAT-005: What state does the common
  postprocessing after the end-of-move menu leave for its next consumer?
  Settles it: admitted mode/record/slot/pointer inputs and their producers,
  complete children and callback targets of overlay 182 +19F8 and
  3D72:0D83, and storage/register preservation and final state through
  those children on every choice path.
  Tried: split from Q-COMBAT-006; FND-COMBAT-026 and FND-COMBAT-047
  establish the common caller sequence after chosen or unhandled
  results, but not the two callees' effects.
  Tried: FND-COMBAT-052 answers +19F8's local four-record marking,
  post-child mode rereads, view/input flag and conditional slot stores,
  resource/result branches and lack of caller result consumption. It
  replaces FND-COMBAT-008's incomplete singular-caller claim. The existing
  FND-CONFIG-171/175 readings answer the resident root's conditional
  callback/result sequence. Those local ordering/result obligations are
  removed from Settles it; actual inputs, children, targets and preservation
  remain. Blocks: slice 4.
- Q-COMBAT-009. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Which effect IDs can the effect rows hold when the status
  selector collects category zero, and can a stored ID exceed 112, the last index of
  `effect_names`? Settles it: for every admitted insertion caller, the effect byte it passes and
  where that byte comes from (shipped `DATA` effect bytes, the handler default for code 127), and
  whether any route stores an ID above 112. Tried: FND-COMBAT-029 reads the collector's category
  filter and row order, FND-COMBAT-031 the common insertion's field writes, and FND-COMBAT-046
  the shipped `DATA` effect-byte domain, its one code 127 and that code's handler-default route;
  which IDs the admitted callers store remains. Blocks: established status-effect selection.
- Q-COMBAT-017. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Can the effect-row count leave 0 to 192? Settles it: the bound
  each count writer keeps: the checked insertion wrapper's exact-192 test against the exported
  insertion entry that skips it, the removal decrements, the owner transfer's decrement and
  increment pair, and the numbered restore that writes the count whole. Tried: split from
  Q-COMBAT-009. FND-COMBAT-031 (the wrapper's test and the unchecked entry), FND-COMBAT-032 (the
  removal decrements), FND-COMBAT-033 (the transfer pair) and FND-COMBAT-045 (resident backing of
  the boundary row for counts 0 to 192) read each writer locally; no bound over all of them
  follows. Blocks: established status-effect selection.
- Q-COMBAT-018. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009:
  What effect rows and count does an admitted numbered load leave behind?
  Settles it: combine the independently evidenced copy inputs/outcomes
  (Q-COMBAT-063), selected resources and lengths (Q-COMBAT-064), destination
  storage (Q-COMBAT-065), and child/postprocessing effects (Q-COMBAT-066).
  Account for successful, skipped and failed requests and the prior state
  each leaves, without assuming atomic replacement. Use Q-COMBAT-009/017/020
  for the saved values' effect-code/count and mutation-producer domains.
  Tried: split from Q-COMBAT-009. FND-COMBAT-034/035 read source slots,
  shipped lengths and whole-resource transfer order; FND-COMBAT-043/044
  read both local callers. The second caller's nonzero selector requests
  only indices 0..16, not effect-state slots 23 and 25.
  Tried: FND-COMBAT-110/111 remove local open/selection and full reader
  continuation uncertainty; FND-COMBAT-112 excludes unchanged shipped
  providers; FND-COMBAT-113/114 remove local copy-loop/result and pathname
  transformation uncertainty. None admits live storage or external outcomes.
  The remaining obligations are separated below; this assessment closes no
  question and promotes no status. Blocks: established status-effect selection.
- Q-COMBAT-063. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009, RULE-SAVE-002, SCR-UI-013:
  Which actual source and working paths and copy outcomes reach the numbered load?
  Settles it: beyond FND-COMBAT-137's admitted empty-source/terminated-working
  pair, bind other selected save-record paths and directory prefixes to
  stable terminated inputs; admit the character tables, buffer extents and
  aliases required by normalization; bind DOS drive/directory state, distinct
  source/destination identity, source length/position and read/write/close outcomes.
  Distinguish copy rejection from partial or complete destination changes,
  beyond FND-COMBAT-138's empty-source rejection and FND-COMBAT-141's
  admitted rooted native-source copy/truncation/repopulation case.
  FND-COMBAT-139 also admits that case's wrapper/settings/outer return;
  other resource provenance and later action effects remain separate.
  FND-COMBAT-140's native-file candidate supplies FND-COMBAT-141's actual
  F2 entry and nonempty path/copy binding; other path/state cases remain.
  Exit: path and file-state cases sufficient to identify which working contents
  can reach archive opening, with any unobserved external outcomes retained.
  Tried: split from Q-COMBAT-018. FND-COMBAT-113 gives the default 128-byte
  copy, destination-open ordering, count tests and ignored final close results;
  FND-COMBAT-114 gives drive/directory and component normalization. Reuse
  FND-CONFIG-181/182 for the earlier string operations. Their local bodies
  need not be reread; actual inputs, runtime tables, storage and I/O remain.
  Tried: FND-COMBAT-115 resolves FND-EXE-621's numeric-parser assumption
  and the find wrappers' local buffer/return-register routing. Decimal-prefix
  parsing and the saved versus queried transfer-pointer distinction are
  removed from unread local helper work. Actual directory bytes, DS/SS and
  ES/BX state, selected slot, table capacity and preservation remain.
  Tried: FND-COMBAT-129 resolves the action-to-load row/top/base reread
  timing, exact word-arithmetic source pointer and own list-release order.
  Those producer/lifetime operations are removed from unread work. Actual
  selected values, buffer/terminator bounds and child/alias preservation
  remain; the explicit list release follows loading, not window closure.
  Tried: FND-SAVE-026 replaces the shared-page display/writer overclaim.
  The known clamp can store a non-multiple-of-ten top, and Save/Load's
  ten-record request is not enlarged by it. This removes an assumed valid
  later-page contract; actual index/storage and child preservation remain,
  with shared event/state producers in Q-COMBAT-046 and paint in Q-UI-005.
  Tried: FND-COMBAT-131 bounds the explicit no-selection producer and
  inherited-FFFF pointer arithmetic. Those local input cases are removed
  from unread work, but actual selection/top and retained storage still
  require admission through the intervening UI/preparation children.
  Tried: FND-COMBAT-137 binds row one/top zero after preparation to an
  empty terminated source and a terminated working destination at the copy
  entry, with the declared null buffer/zero capacity and retained list pointer.
  Those initial inputs for this route are removed; normalization, DOS/file
  identity and I/O outcomes, other inputs and later lifetimes remain open.
  Tried: FND-COMBAT-138 observes empty-source normalization returning FFFF,
  copy returning zero before direct file opens, and equal bounded private
  working-file state at entry/return. That rejection case is removed;
  nonempty inputs, source-open failure, copying and later continuation stay open.
  Tried: FND-COMBAT-139 observes the zero wrapper result, PREF/100 and GREQ/2
  requests returning zero, and matched numbered-load return AX one after the
  admitted empty-source rejection. That continuation case is removed;
  other paths/outcomes, supplying resources and later action cleanup remain open.
  Tried: FND-COMBAT-141 observes rooted native source/destination normalization,
  distinct private files, direct open returns, length 49739, working-file
  truncation then source-matching output and copy return one. That successful
  rooted case is removed; other paths, failure outcomes and later archive
  provenance remain open, without claiming unrecorded individual I/O returns.
  Blocks: Q-COMBAT-018.
- Q-COMBAT-064. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009:
  Which runtime archive resources and recorded lengths supply effect rows and count?
  Settles it: bind the actual path/reuse flag and saved handle to a live record
  through the locally read selection/open/close/growth transitions. Admit the
  pre-read state beyond FND-COMBAT-142's admitted mode/reuse/handle tuple,
  DS:145A versus working-handle resolver identity,
  current graph/mode and preservation beyond FND-COMBAT-143's one observed
  selector-zero SAVE/24 and SAVE/26 selection pair, rather than rereading
  those helpers. Identify the
  supplying record/file-handle lifetime and directory identity beyond
  FND-COMBAT-145's one working-open publication and selected-row binding,
  and source/length
  contracts on other complete, missing-resource and fallback paths.
  Reuse Q-COMBAT-054 for shared archive
  graph/state producers and Q-COMBAT-063 for copy admission, rather than
  creating another census of those mechanisms. Exit: a source/length contract
  for each request path; live selection alone is insufficient under fallback.
  Tried: FND-COMBAT-145 matches one fixed working-path open, I/O result and
  record publication to both later selected provider/handle tuples and plain
  rows, with matching copied-file rows and payload spans. Those endpoint
  publication/row bindings are removed from unread work. Continuous handle/
  record lifetime, full-directory/graph identity, later preservation and
  other source/fallback/missing paths remain open.
  Tried: FND-COMBAT-144 reaches both target core returns with success/counts
  and byte-equal destination/private-file spans under unchanged copied-file
  identity. That route's SAVE/26 completion and bounded payload equality
  are removed from unread work. I/O-handle/file and directory identity,
  intervening lifetime, later preservation and other paths remain open.
  Tried: FND-COMBAT-143 observes both native-route selected providers equal
  to active/head with working handle 14, lengths 1920 and two, and SAVE/24's
  matched successful core result/count. That pair's selection tuples are
  removed from unread work. File/payload and directory identity, intervening
  lifetime, SAVE/26 completion and missing/fallback paths remain open.
  Tried: FND-COMBAT-142 admits selector-zero reader entry after native copying,
  with current mode four, working handle matching active/head handle fields,
  zero region handle and the observed reuse/counter state. That entry tuple
  is removed; live record/file identity, intervening lifetime, request-time
  preservation and SAVE/24/26 provider/length remain open.
  Tried: split from Q-COMBAT-018. FND-COMBAT-110 distinguishes new-open
  selection from reuse; FND-SAVE-021 records the preceding writer's explicit
  selections; FND-COMBAT-096 bounds selection/close results. FND-COMBAT-112
  excludes all 26 unchanged shipped top-level files as SAVE providers. Reuse
  FND-CONFIG-037/038/040/068 and FND-CONFIG-151 for lookup, fallback and
  complete-resource requests. Runtime directory and state admission remain.
  Tried: FND-COMBAT-120 resolves the local fallback cursor, fresh active
  starting point on each read, failure paths without further fallback and
  traversal's current global anchor. Those local selection/retry questions
  are removed from unread work. Actual graph/mode/directory, per-resource
  provider/length and callee/global preservation remain.
  Tried: FND-COMBAT-121 resolves the writer's own missing-type plain
  constructor, existing-indexed rejection, growth relookup and metadata-before-
  payload ordering. Those local directory-producer questions are removed.
  Actual copied/written input, directory identity/count bounds, helper and
  external outcomes and later preservation remain; no universal plain-SAVE
  or complete-payload invariant follows from this branch alone.
  Tried: FND-COMBAT-122 resolves plain-resource removal and its actual
  result handling in the numbered writer. Selection failure still reaches
  the current pair, removal does not branch the caller, and the write's
  signature check can clear its error. Those local result combinations are
  removed from unread work; actual active-record/directory state and
  reservation/copy/flush/I/O effects remain.
  Tried: FND-COMBAT-123 resolves candidate-position selection, freed-range
  early gates and single-neighbour merging, and later selected-row consumption.
  The known helpers' own bookkeeping is removed from unread work. Actual
  record/list producers and bounds, pointer/copy/guard effects, directory
  identity and flushing/I/O remain; candidate selection itself reserves nothing.
  Tried: FND-COMBAT-124/125 resolve the final AH-68 wrapper and dirty
  clearing order, plus directional directory copying and its cursor steps.
  Those own helper footprints are removed from unread work. Actual record/
  list/directory inputs, lengths and capacities, guards, alias/preservation
  and external outcomes remain; a read route can flush dirty metadata.
  Reuse FND-COMBAT-126 under Q-COMBAT-054 for growth's local reload and
  publication order; shared graph/allocator admission remains there.
  FND-COMBAT-127 supplies the corresponding local open construction and
  failure contract under that shared item, including pre-publication effects.
  Tried: FND-COMBAT-128 resolves the concrete post-open region-close site,
  state-zero/two/other cleanup composition and ignored close result before
  the numbered reader. Those local call-order/gate obligations are removed.
  Actual DS:145A/working-handle identity, graph membership and intervening
  preservation remain; the reuse flag is not a membership test.
  Tried: FND-COMBAT-130 resolves literal handle-output producers, counter
  reset/issuance and the conditional modular separation relation. Those
  local generation obligations are removed. Admit actual values or a
  preserved non-repeating issuance interval without reset, plus current
  record +28 and graph identity; no uniqueness follows from field names.
  Blocks: Q-COMBAT-018.
- Q-COMBAT-065. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009:
  Which storage and pointer lifetimes admit the effect restore destinations?
  Settles it: admit source-table slots 23 and 25, their segment mappings and
  backing extents through transfer and return; compare each selected length
  from Q-COMBAT-064 with the actual writable span. Account for overlapping
  earlier resource destinations, bulk/aliased pointer writes, the placed-buffer
  equality/clearing branch, stack-local pointer/result aliases and failure
  paths that retain earlier or partial writes. Exit: destination and retained
  state cases for both requests, distinct from a nominal shipped length.
  Tried: FND-COMBAT-148 observes pairwise physical separation of the actual
  map, two bitmap planes and both effect spans at the GMAP transfer boundary.
  That one five-span comparison is removed from unread work. Continuous
  backing/lifetime, metadata/stack/other aliases and alternative cases remain;
  the second-plane field's retained value after cleanup proves no liveness.
  Tried: FND-COMBAT-146 observes both admitted target spans equal to their
  transfer-time references and source slots 23/25 equal to the destinations
  at the matched selector-zero reader return. That one endpoint-retention
  case is removed from unread work; continuous backing/lifetime, aliases,
  intermediate writes and alternative destination/failure cases remain.
  Tried: split from Q-COMBAT-018. FND-COMBAT-034 gives source slots and
  shipped lengths, FND-COMBAT-035 gives pointer forwarding and transfer order,
  and FND-COMBAT-045 bounds resident row backing. The binding helper does
  not itself replace these slots. None proves every runtime pointer, alias
  or transferred length. Reuse Q-ITEM-011/013 for shared segment/frame
  prerequisites without transferring their independent obligations here.
  Tried: FND-COMBAT-116 resolves all 46 nonnull shipped pointer relocations,
  their nominal nonoverlap with saved effect rows/count, and count-word
  resident backing. SAVE/21 contains the helpers' nominal boundary row.
  Those fixed-table comparisons are removed from unread work; runtime
  bindings for fifteen null slots, other writers, actual lengths and
  storage/segment/transfer aliases remain.
  Tried: FND-RNG-093 corrects the shared startup heap-base rounding before
  that contract is reused for dynamic-buffer separation. Its corrected
  bound remains above resident data, but current bounds, free-list/header
  validity and pointer lifetime remain shared allocator admission, already
  Q-CONFIG-008 and Q-SCRIPT-003. Do not infer those from allocation calls
  alone or repeat the nominal table comparison in FND-COMBAT-116.
  Tried: FND-COMBAT-117 pairs every one of the fifteen dynamic bindings
  with its known allocation family and matching nominal request. The local
  binder/known-producer map is removed from unread work. Live field identity,
  bypass/release lifetime, other writers, actual lengths and aliases remain;
  shared heap admission stays with Q-CONFIG-008 and Q-SCRIPT-003.
  Tried: FND-COMBAT-118 excludes nominal fixed payload writes into the
  source table and all fifteen supplier-pointer fields, with conditional
  separation for admitted dynamic blocks. That payload-versus-metadata
  comparison is removed from unread work; actual values/lengths/lifetimes,
  aliases and stores by other helpers remain unadmitted.
  Tried: FND-COMBAT-119 resolves the placed clear's complete local
  footprint, DS/SI/DI preservation and the caller's pointer reloads.
  That explicit child-call uncertainty is removed. Actual buffer identity,
  extent and metadata/stack aliases, along with the other helpers' effects,
  remain; the null skip does not guard the preceding clear.
  Blocks: Q-COMBAT-018.
- Q-COMBAT-066. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009:
  Which reached child effects preserve or change restored effect rows and count?
  Settles it: follow the actual preparation and continuation routes of both
  callers and the numbered reader, including diagnostics, map reconstruction,
  release, object initialization, script conversion and item enumeration.
  Identify effects on effect rows/count and required aliases or saved locals,
  including paths after a failed or skipped transfer. Reuse Q-COMBAT-048 for
  shared window/input callbacks and Q-COMBAT-020 for admitted mutation APIs.
  Exit: child effects sufficient to determine the rows/count at each returning
  continuation; unresolved callback targets cannot be assumed to preserve them.
  Tried: FND-COMBAT-158 removes the named cell's reached writer: occupancy setter via its
  placement wrapper. Initiating slot/caller, other routes and continuous storage remain.
  Tried: FND-COMBAT-157 locates the later caller's sole map difference at cell (107,36),
  00 to 60. Cell/mask identification is removed; reached writer/caller attribution remains.
  Tried: FND-COMBAT-156 removes normal enclosing post-setup/working-close/epilogue observations,
  retaining item/script/effect endpoints. One map byte and archive state change after reader return;
  individual writers are unattributed. Other caller/selector, failed/skipped transfers,
  populated children and continuous storage admission remain.
  Tried: FND-COMBAT-155 removes one native 400-record clear's three changed words, disjoint
  sampled item/output/effect/local spans and zero-result/no-traversal-helper enumeration
  retained at reader return. Populated traversal, metadata/slot aliases, continuous lifetime,
  failure states and later enclosing caller work remain.
  Tried: FND-COMBAT-152 removes one native conversion's size/data provider, 7896/1316 scan inputs,
  DS/SS and bounded chain/trigger bindings, with prediction equality before release and at
  reader return. File/continuous lifetime, other aliases, alternate lengths,
  failure/diagnostic states and later caller work remain.
  Tried: FND-COMBAT-151 removes the converter's own narrowed signed count, DS/SS match flags,
  exact pair/trigger writes, unmatched behavior and failure cleanup order. Actual provider/spans,
  live chain/trigger/stack values/aliases and failure/diagnostic/later effects remained; FND-COMBAT-152 admits the normal case.
  Tried: FND-COMBAT-150 removes one ordered 125-slot native loop, sixteen-byte OJFF local spans,
  matched core/loader epilogues and scoped initializer/image-selector/chain child counts.
  OJFF source/content/live aliases, individual writes, populated temporary/allocation/failure
  states and later caller continuations remain open.
  Tried: FND-COMBAT-149 completes the flags-zero object-loader own body,
  its no-call image-selector branch, chain-insertion/reset leaves and
  temporary eviction helper's own gates. Those local obligations are
  removed. Actual OJFF/stack and restored-table admission, allocation
  state, lazy-animation/unload children and other transitive effects remain;
  reuse Q-COMBAT-045 for populated temporary-slot admission.
  Tried: FND-COMBAT-148 admits one region-open publication, GMAP/50 plain
  provider/row and complete transfer, actual five-span separation and a
  full-map prediction match at reader return. Those native-route source,
  extent and endpoint obligations are removed. Continuous lifetime and
  intermediate mutations, other map states/failures, individual child
  attribution and later caller work remain open.
  Tried: FND-COMBAT-147 reads the complete map-application child, region-open
  wrapper and four leaf setters: full-pointer gate, GMAP-result branches,
  bitmap traversal, byte-store footprints and ignored close/open results.
  Those own-body obligations are removed. Actual reached path/state, GMAP
  provider/length, storage lifetime/aliases, runtime/diagnostic effects and
  attribution of the observed archive-counter change remain open.
  Tried: FND-COMBAT-146 admits the complete reader's matched return and
  retained target-byte/binding endpoints for one native route, with AL one.
  That returning-boundary case is removed; reached-child attribution,
  intermediate mutations, failed/skipped transfers and later caller
  continuations remain. The archive counter changed despite matching
  active/head endpoint pointers, so it is not an unchanged-history proof.
  Tried: split from Q-COMBAT-018. FND-COMBAT-111 accounts for the full reader's
  own body, which has no explicit effect-count, ID or ordering validator.
  FND-COMBAT-043/044 account for the callers' own continuations. Reuse
  FND-COMBAT-077, FND-SAVE-022/023 and FND-PARTY-236 for their bounded
  reconstruction and item-write contracts; their existence is not complete
  child/storage admission. Blocks: Q-COMBAT-018.
- Q-COMBAT-024. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Which effect rows does a game begun after an earlier one in the
  same run start with? Settles it: whether the defeat cleanup's unload reaches the owner removal
  at 31E0:0AC9 with `DS:265B` nonzero, which owners' rows the earlier game's other exits (the
  Game Menu's new game, a load) remove, and what removes the rows of owners that are not party
  slots. Tried: split from Q-COMBAT-019. FND-COMBAT-053 finds that no writer sets the count to a
  constant, so a later game keeps the earlier game's rows apart from those the removal routes
  take out, and reads the owner removal +1DB0 and its call from the object unload;
  FND-COMBAT-054 finds the removal entries inert only while the count is 0. Blocks: established
  status-effect selection.
- Q-COMBAT-020. RULE-COMBAT-009, RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Which routines insert, remove and transfer effect rows, and with
  which owners, keys and codes? Settles it: the admitted callers of the insertion wrapper and the
  exported insertion entry, of the key, index and code removal entries and of the owner transfer,
  with the arguments each passes and the handler or request effects those operations trigger.
  Tried: split from Q-COMBAT-009. FND-COMBAT-031, FND-COMBAT-032 and FND-COMBAT-033 read the
  three operations locally, and FND-COMBAT-039 one removal caller, the effect window's cache
  action, which passes the first matching row's key; the other callers remain. Blocks:
  established status-effect selection.
- Q-COMBAT-010. FMT-COMBAT-003: Does each effect's nonzero ICON number reach the `ICON` resource
  the effect window draws? Settles it: the archive that is open when the effect window requests
  the icon, the request's result on success and on a reader error, and which frame the draw path
  takes, for the shipped nonzero numbers, telling the zero-icon branch apart from a failed
  request. Tried: FND-COMBAT-029 (the consumer's zero test and zero-extended request),
  FND-COMBAT-030 (all 107 nonzero references match 97 installed `ICON` resources) and
  FND-COMBAT-038 (frame 0, null-request slots kept, draw and release order) answer the local
  route; the open archive and the error outcomes remain. Blocks: established effect-icon format
  contract.
- Q-COMBAT-021. FMT-COMBAT-003: What initializes the image cache the effect window draws its slot
  background and icons through? Settles it: the startup order of the cache fill and the
  handle-capacity request, every writer of the cache offsets and handles, and the mapping of the
  third offset group. Tried: split from Q-COMBAT-010. FND-COMBAT-041 (cache index 8 from
  `BMP/13007` for the slot background) and FND-COMBAT-042 (three offset groups and a separate
  handle-capacity request) read the local fills; their order at startup, the other writers and
  whether the transfers succeed remain. Blocks: established effect-icon format contract.
- Q-COMBAT-022. FMT-COMBAT-003: Which loaded window, children and callbacks does the effect window
  run with, and what does each frame event do? Settles it: a reading of the effect window's
  create, reuse and close paths through to the loaded `WIND/11500` graph, of the callbacks the
  window system calls for its 35 frames, and of the event numbers that reach the frame handler.
  Tried: split from Q-COMBAT-010. FND-COMBAT-039 (the frame handler's label, action and removal
  branches), FND-COMBAT-040 (`WIND/11500`'s 86 children with all 35 frames) and FND-UI-041 (the
  callback fields, the before and after handoff and the icon gate) answer the local and shipped
  parts; the loaded state and which events arrive remain. Blocks: established effect-icon format
  contract.
- Q-COMBAT-011. RULE-COMBAT-009: What initializes and subsequently writes
  the category bitmap used by the status
  selector? Settles it: qualified writer/initializer coverage, admitted
  ID/bit domains and lifetime, including restoration or bulk writes;
  distinguish classification from the per-owner membership state now
  tracked in Q-COMBAT-013.
  Tried: split from Q-COMBAT-009 after FND-COMBAT-029 identifies the
  shipped classifier and local membership preparation. Local bit tests
  and initial bytes alone do not admit all later bitmap contents.
  Tried: FND-COMBAT-034 qualifies two exported neighboring-table writers
  and the SAVE/25 source at offset 000F, outside the fifteen-byte
  classifier prefix for valid local counts. This answers their relative
  field/source positions; the other relocation/fixup candidates, count
  admission, bulk/aliased writers and membership lifetime remain.
  FND-COMBAT-035 qualifies SAVE/25's local restore destination and
  request semantics; other resource destinations, aliases and later
  initialization/postprocessing remain outside that reading.
  Tried: FND-COMBAT-037 accounts for all nineteen returned overlay fixup
  sites and both resident data relocation sites under the explicit
  segment-reference query. Neighboring-record destinations are separated
  conditionally; the descriptor site is metadata. That bounded census
  classification is answered. Count/index admission, computed/aliased
  and bulk writes, restoration and callee effects remain. Prepared
  membership producers/content lifetime split to Q-COMBAT-013.
  Blocks: established status-effect selection.

- Q-COMBAT-013. RULE-COMBAT-009: What admitted producers and intervening
  writes determine the prepared per-owner membership bitmap contents at
  collection? Settles it: complete preparation/consumer caller paths,
  admitted ID-to-bit writes and content lifetime through collection,
  including two-category append interactions and any other bitmap
  writers; physical buffer/segment admission remains Q-COMBAT-012.
  Tried: split from Q-COMBAT-011 after FND-COMBAT-029 records the local
  fifteen-byte clear and membership ORs, and FND-COMBAT-036 records
  actual selector buffer arguments and its one-/two-category paths.
  Those local operations do not prove unchanged contents on every caller.
  Tried: FND-COMBAT-038 qualifies the second direct collector caller's
  category-zero/one appends, shared bitmap, late count clamps and copy
  to the effect cache. Together with FND-COMBAT-036 this answers local
  arguments/order at all four returned direct collector sites, within
  that query's transfer model. Admitted aggregate writes, content
  identity, other transfer kinds and intervening effects remain.
  Blocks: established status-effect selection.
- Q-COMBAT-012. RULE-COMBAT-009: Do the status selector's actual callers
  admit the bitmap, row table and output as independent valid storage?
  Settles it: reached caller/frame and buffer allocation/lifetime
  coverage, DS/SS identities and non-overlap for the preparer, collector
  and status sorter, with their distinct 15-byte clear, 113-byte append
  and 35-element sort limits. Tried: split from Q-COMBAT-009 after
  FND-COMBAT-029 supplies the local accesses and FND-COMBAT-031/032/033
  supply producer copies with separate backing obligations. Those local
  ranges do not prove caller buffer or segment identity.
  Tried: FND-COMBAT-036 answers the selector frame's exact 114-byte
  pre-bitmap span, helper argument/frame provenance, mixed DS/SS uses
  and clamp timing. The flag-one status wrapper has a conditional
  single-collection separation argument; the exported flag-zero path
  appends both categories before clamping. Local frame offsets and
  limits are settled; full reached callers/ordinal domains, runtime
  segment/stack identity, callee preservation and admitted aggregate
  writes in that second path remain.
  Tried: FND-COMBAT-038 answers the other direct collector caller's
  114-byte pre-bitmap span and near buffer arguments, plus its separate
  resource-pointer, coordinate and cache lifetimes. The four returned
  direct collector sites now have local frame readings; runtime segment/
  stack admission, aggregate bounds, complete callers and preservation
  remain outside that bounded coverage.
  Blocks: established status-effect selection.
- Q-COMBAT-025. RULE-COMBAT-010: Which spells, powers or items are `DATA` 18, 53, 121, 169 and
  277, which combatants are numbered 48 or more, and does overlay 173 `+2C00` give the target's
  Armor Class for the target, the caster and 0? Settles it: the names or uses of the five `DATA`
  records, a reading of how combatant numbers are given out, and a decode of `+2C00` from its
  entry to its returns. Tried: FND-RNG-032 reads the roll and the gate that runs it, and
  FND-PARTY-091 reads `+2C00` only as the attack routine's fourth argument. Blocks: slice 4.
- Q-COMBAT-026. RULE-COMBAT-012: What do the results 0, 1, 2 and 7 of `confusion_roll` make the
  turn routine do, what do bit 0x08 of byte 4, bit 0x80 of byte 0 and bit 0x20 of byte 2 of
  `effect_block` stand for, which code sets bit 0x100 of a word in `g_4C4F_01D8`, and what do
  `g_4E71_0AC6` and record byte `+0x15` hold? Settles it: a reading of overlay 173 `+1238` past
  its store to `DS:420C`, of the writers of the three bits and of bit 0x100, and of the readers
  of byte `+0x15` and `g_4E71_0AC6`. Tried: FND-RNG-089 reads overlay 195 `+000E` and its caller
  up to the store. Blocks: slice 4.
- Q-COMBAT-027. RULE-COMBAT-013: What does record byte `+0x19` hold, and what does each of the
  three steps of overlay 195 `+00D1` that call `byte_19_check` do with 0 and 1? Settles it: a
  reading of the writers of byte `+0x19` and of the three steps past each call. Tried: FND-RNG-089
  reads `+152F` and names the three steps. Blocks: slice 4.
- Q-COMBAT-028. RULE-COMBAT-014: Which groups do the masks 0x71, 0x184 and 0x608 name, what do
  script request 25's values 7 and 11 and the byte at `DS:054E` do, and does a combatant that has
  acted get -1 as its order word? Settles it: the writers of combatant byte `+0x15` and of
  `DS:054E`, and a reading of overlay 173 `+1098` from `+1155` on with its caller. Tried: split
  from Q-RNG-015. FND-RNG-042 reads the two terms, the writers of the side byte, the shipped
  scripts' request 25 values and `+1098`'s comparison. Blocks: slice 4.
- Q-COMBAT-029. RULE-COMBAT-015: What do `1B0B:00A4`, `2D40:35EF` and `3143:00A8` compute, what
  does an effect 33 entry's word `+0x24F` hold, and what do overlay 173's four callers of
  `+1ACD` use the pick for? Settles it: readings of the three routines, of the writers of the
  effect entry word, and of `+1354`, `+1684`, `+1822` and `+1A55` with the local limit of
  `+1684`. Tried: split from Q-RNG-015. FND-RNG-041 reads `+1ACD`, its list builders and its
  callers' arguments. Blocks: slice 4.
- Q-COMBAT-030. RULE-COMBAT-016: What is object 2200, what do item byte `+0x12`, held-object kind
  1 and byte `+5` of the `57E0:67B7` entry hold, and what do `31E0:0121`'s other arguments,
  `2D40:288C` and `31E0:05BB` do? Settles it: readings of the three routines and of the readers
  of byte `+5`. Tried: split from Q-RNG-015. FND-RNG-041 reads overlay 173 `+3136`. Blocks:
  slice 4.
- Q-COMBAT-031. RULE-COMBAT-011: What do the item type's byte `+4`, its low four bits, and the item
  record's byte `+0x16` and word `+0x10` stand for, and what does overlay 173 `+3E6B` do to the
  broken item? Settles it: readings of `+3E6B` and of the writers of those record fields. Tried:
  split from Q-RNG-015. FND-RNG-041 reads the break test. Blocks: slice 4.
- Q-COMBAT-032. FMT-COMBAT-001, RULE-COMBAT-003: Which rule entry takes overlay 173 `+22C4`'s
  damage step, and what do the `combat_mark` values 3, 7 and 8 it sets and its calls of `+306B`
  and `+30C7` stand for? Settles it: a rule entry for `+22C4` from FND-RNG-043's reading, with
  readings of `+306B`, `+30C7` and the readers of `combat_mark` 3, 7 and 8. Tried: FND-RNG-043
  reads `+22C4`'s branches while tracing the removal list. Blocks: slice 4.
- Q-COMBAT-033. RULE-COMBAT-017: What do the effects and callees of overlay 195 `+00D1`'s
  per-round steps stand for? Settles it: the effect codes 0x1E, 0x64, 0x10, 0x32, 0x45, 0x18,
  0x65, 1, 0x13, 0x6E and 0x6F named, with the block bits that mark them; readings of overlay 193
  `+0DC7`, `+166F`, `+194F`, `+1A2D` and `+1AA4`, overlay 173 `+2C00` and `+24C7`, overlay 197
  `+30ED` with 0x122 and 0x12D, `3143:00A8`, and the words overlay 179 `+0C7F` takes after the
  code; the fields at `4E71:024F`, `4E71:0251`, `4E71:0255` and `4E71:0256` of an effect entry;
  and the readers of `DS:444B`. Tried: FND-RNG-049 reads every step of `+00D1` and
  RULE-COMBAT-017 takes it. Blocks: slice 4.

## Emulated call

None.

## Agent run

- Q-COMBAT-041. RULE-COMBAT-002: Does a non-ranger with two melee weapons and Dexterity below 6
  hit more often, and does overlay 197 `+27A7`'s comparison at `+287D` ever replace the Armor
  Class when block byte 13 bit 1 is clear? Settles it: a breakpoint run at overlay 173 `+20E2`
  and overlay 197 `+287D` from a fight save, reading the bonus word and `bp-4` and `bp-2`. Tried:
  FND-COMBAT-066 and FND-COMBAT-068 read both statically. Blocks: slice 4.
- Q-COMBAT-037. BUG-COMBAT-001: Does a load from the repeated-pass state store the leader over
  9999 in `DS:426D` at overlay 182 `+1772`, and is a leader key for another slot taken after the
  state ends? Settles it: a run from a checkpoint whose drive holds a saved game, that makes the
  bug's click, gets F2 through in the gap after a pass's `flush_input`, loads the game watching
  `DS:426D`, and after EXIT in another run presses a leader key for a slot other than the leader.
  Tried: FND-COMBAT-064 ends the state with F1 or F2 and EXIT, and with a save; its load stored 1
  over 1. Blocks: BUG-COMBAT-001's frequency.

## Live session

- Q-COMBAT-001. RULE-COMBAT-001, RULE-COMBAT-006, SCR-COMBAT-001, RULE-AI-002: How does the first ordinary
  combat begin, what does one accepted attack and one turn command do, how does the turn pass from
  one combatant to the next, and how does combat end? Where are the members who appear placed,
  and what does the frame labelled as an enemy moving show? Settles it: the opening-combat live
  session. Tried: static searches from the hostile object record, `MONR`, `ETAB`, `RDFF`,
  coordinate input, the computer-control labels and the random number and status-panel routes
  (FND-COMBAT-001 to FND-COMBAT-017), the owner's captures and reports (FND-COMBAT-018 to
  FND-COMBAT-021), and the key routine and end-of-move menu (FND-COMBAT-025, FND-COMBAT-026).
  Blocks: slice 4.
- Q-COMBAT-023. FMT-COMBAT-003: What does the effect window show for an effect whose record has a
  nonzero ICON number, and for one whose number is zero? Settles it: a live session with DOSBox
  screenshots of the effect window for a character with at least one effect of each kind, and of
  the same window after one of them ends. Tried: split from Q-COMBAT-010. FND-COMBAT-038 and
  FND-COMBAT-040 give the draw route and the slot geometry; what the window shows depends on the
  effects a character has in play, which no static reading supplies. Blocks: established
  effect-icon format contract.

## Source

None.

## Blocked

- Q-COMBAT-019. RULE-COMBAT-009: Which effect rows and count does a new game begin with?
  Settles it: Q-UI-020's answer. If no script before the gate's call of `MAS/50` changes global
  22, global 82 or global flag 188, FND-UI-052 finds no row written on the way to `0x05DC`, so the
  game begins with no rows and a count of 0.
  Tried: split from Q-COMBAT-009. FND-COMBAT-053 finds nothing writing the count or rows before
  the party load and no constant store to the count; FND-COMBAT-054 finds the other removal and
  transfer entries inert at count 0 and every read route to an insertion passing overlay 193
  +12A8, which needs a slot made from object 147; FND-UI-050 and FND-UI-051 find region 50 loaded,
  whose `ETAB` does not place object 147; FND-COMBAT-055 finds no placement on `MAS/50`'s script
  route naming object 147 and the party's objects 300 to 313; FND-EXE-592 resolves every transfer
  and finds the insertion reached through the script dispatch, the window hook and a frame
  callback; FND-UI-052 finds none of those live and no row written from the gate routine's +12FF
  to `0x05DC`, given the values of those three variables after `MAS/99`. What an earlier game
  leaves is Q-COMBAT-024. Waiting on: Q-UI-020. Blocks: established
  status-effect selection.
