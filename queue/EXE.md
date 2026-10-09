# EXE

Next ID: Q-EXE-014

## Static

- Q-EXE-001. FMT-EXE-001, FMT-EXE-002, FMT-EXE-003, FMT-EXE-004,
  FMT-EXE-005: How does the overlay manager load an overlay? Which code
  reads the pack header and the segment table, what do the descriptor words
  `unk_02` and `unk_06` and the `flags` values 0, 1 and 4 mean, what does the
  `INT 3Fh` trap do with a trampoline's `target`, what fills the overlay
  header's zero fields at run time, and how are the fixups applied? Settles
  it: the `INT 3Fh` handler that the startup code installs, read through its
  file reads, the header fields it writes and the fixup loop. Tried: the only
  resident routine that calls both literal DOS seek and read wrappers
  (FND-EXE-007), which reads signature-and-length records and takes no pack
  input. FND-EXE-175 adds an original-source bounded resident candidate
  with explicit call targets and unresolved interrupt/root-frame stops.
  Next: establish its incoming transfers and state/segment producers, then
  follow the listed callees into handler installation and loading.
  FND-EXE-176 resolves the initial state/vector pointer; account for every
  writer and the vector procedure's replacement with the old pointer.
  FND-EXE-252 corrects the initializer-only gate; resolve both callbacks
  and state gate producers before claiming cleanup completion.
  FND-EXE-178 adds default-stub and replacement-writer leads; follow their
  segment preservation and target bodies. FND-EXE-179 lists the intervening
  callee returns, interrupts and unresolved far targets; follow their producers
  and saved-stack integrity before claiming preservation. FND-EXE-180 and
  SRC-DOSBOX-GOG-0742 connect the paired producer to an external-provider
  contract; live target identity, writers and source/binary correspondence
  remain unresolved. FND-EXE-182 adds a bounded shipped-provider pattern;
  read its input/output field identities, pointer producer and registration
  consumer before accepting correspondence. FND-EXE-183 follows the packed
  pointer producer and counter-helper success return; establish counter
  initialization/unit/lifetime, failure effects and callback setup. FND-EXE-184
  records setup state ordering and result consumption; read its builder,
  diagnostic and metadata callees, state producers and dispatch. FND-EXE-185
  resolves the selected builder arm; establish its backing-memory producers,
  destination extent and callback-table dispatch. FND-EXE-186 bounds the
  table consumer; establish its selector/table writers, callee effects,
  callback inputs and root callers. FND-EXE-187 traces the backing-base producer
  and controlled CRT imports; read request producers/range, wrapper records,
  retry target, initialization, loaded CRT effects and lifetime. FND-EXE-188
  reads local record-helper paths; establish shared state/selector producers,
  initializer/import contracts, frame preservation and record aliases. FND-EXE-189
  resolves local selector publication; establish shared/gate/flag producers,
  imported preservation, admitted extent and wait completion. FND-EXE-190
  reads shared-record publication; establish query extent, low-word helper
  decoding, adopted-record admission, source globals and lifetime. FND-EXE-191
  resolves local recovery; establish initialized prefix, admitted identifiers,
  unchanged names, record extent/lifetime and loaded failure effects. FND-EXE-192
  resolves producer query extent; establish runtime suffix preservation,
  existing atom provenance, admitted retrieval extent and record lifetime.
  FND-EXE-193 establishes callback-slot indirection and a field +4 setter;
  follow setter callers/arguments, other field writers and copied targets,
  retaining record admission and lifetime requirements. FND-EXE-194 follows
  dispatch continuations and the failure import; its controlled direct-call
  searches exclude indirect setter uses. Establish those uses and loaded
  callback/failure contracts before closure. FND-EXE-195 excludes the shipped
  contiguous absolute setter dword with stored-target controls; calculated,
  relocated, encoded and runtime-created targets remain unresolved. Blocks: none.

- Q-EXE-007. FMT-EXE-006: Do the shipped game or sound-setup executables
  launch any batch helpers? Settles it: direct executable launch references
  traced through selectors and arguments, including computed command names.
  Tried: complete helper contents (FND-EXE-008) and distribution-wrapper
  reading (FND-EXE-010); neither locates an executable caller. Split from
  Q-EXE-004. Blocks: complete game/setup caller coverage.
  Tried: FND-EXE-350's new complete physical ASCII suffix census identifies
  one sound.bat lead in each game edition and one autoexec.bat suffix in
  the sound utility. Trace their actual consumers and the utility's leading
  zero-byte writer before distinguishing display/file access from launch.
  Extensionless, encoded, split and runtime-constructed names remain outside
  this literal search; the question remains Static for consumer reading.
  Tried: FND-EXE-360 follows the utility suffix through a stack constructor,
  its append helper and the same interface used with sound.ini. Read the
  interface's two near callees, returned-byte producer's external wrapper,
  caller/data-segment admission and later paths before a launch exclusion.
  The game editions' sound.bat consumers remain separate targets.
  Tried: FND-EXE-353 follows the selected interrupt wrapper's segment record,
  register transfers and output word-six store into the producer's stack byte.
  Continue native result/input admission, error helper 1000:04CE and the
  file-interface near callees; this does not establish pathname usability or launch.
  Tried: FND-EXE-354 follows 1000:2BC7's signed record-byte test,
  mutable low-word bound and advanced-pointer final test. Continue the
  record/count writers and DS admission, 1000:2AF6 and later pathname
  consumers; a selected pointer is not a validated or reserved extent.
  Tried: FND-EXE-370 reads the selector, mode parser and initializer, and
  follows the retained-rt route to explicit attribute/open service selectors.
  The first wrapper's CX word comes from saved incoming DI, not a pushed
  zero. Admit record/state writers, interrupt-preserved options and later
  handle/buffer helpers before an execution exclusion or complete reading.
  Tried: FND-EXE-355 follows 1000:2AF6 and the selected rt parser path,
  ordered result stores and distinct failure continuations. Continue
  1000:317F, 1000:0519, 1000:37F1 and 1000:2873, segment/record
  admission and later consumers before claiming an operation or exclusion.
  Tried: FND-EXE-356 follows 1000:0519's post-interrupt DX-bit return
  and caller flag update without a status check. Continue native result and
  record-byte admission, 1000:37F1 and 1000:2873, aliases and lifetime.
  Tried: FND-EXE-380 records the shipped limit-word initializer and the
  first five selector-byte initializers. These do not admit incoming state;
  trace startup, actual DS, aliases and later writers before applying them.
  Tried: FND-EXE-357 separates 1000:37F1's pre-mutation rejection from
  its later allocation failure after record/global stores. Continue 1000:2CC8,
  1000:1ACC, 1000:1BD6, caller cleanup 1000:2873 and state/preservation admission.
  Tried: FND-EXE-358 distinguishes 1000:2873's preliminary failure from
  later record clears, and records 1000:27B6/05F5's local paths. Follow its
  remaining callees and SI/stack preservation before assigning release or rollback.
  Tried: FND-EXE-359 resolves 05AC/052A's retained-word argument binding,
  local cleanup and digit-production bounds. Admit 1487's returned destination
  capacity and preservation; other cleanup helpers and source state remain open.
  Tried: FND-EXE-361 follows 1487/3AE4/30EB's scan, copy and low-word
  destination advance, including unchecked scan exhaustion. Admit substituted
  storage, termination, extents and frame aliases; other cleanup helpers remain open.
  Tried: FND-EXE-362 reads 0EF5's DS restoration/carry paths and 04CE's
  signed-word mapping, including the minimum-word negation case. Admit native
  preservation, table/state writers and actual DS; other cleanup helpers remain open.
  Tried: FND-EXE-363 reads 292B's wrapped count and pre-call stores,
  mismatch exception and separate caller returns. Follow 3B03/29F8, their
  result/preservation contracts, flag writers and record/buffer admission.
  Tried: FND-EXE-364 reads 3B03's count guard and direct route, 05CC's
  unchecked preliminary result and 3C54's carry-dependent publication. Follow
  the 3B6D byte-processing path and native/state preservation and admission.
  Tried: FND-EXE-365 reads newline expansion, post-store batching and
  separate input/output result arithmetic. Admit nonwrapping frame/source state,
  native SI/DI/DS preservation, extents and aliases before assigning outcomes.
  Tried: FND-EXE-366 reads 29F8's captured limit, selected-call count
  and low-word pointer traversal without result tests. Admit segment/table extent,
  flag writers, preservation and wrapped-pointer/re-entry behavior.
  Tried: FND-EXE-367 reads 1ACC's segment-only dispatch and 1998/1A6C's
  matching path and held arguments. Follow 19FB/1E34, shared CS state writers,
  link/segment admission, extents, aliases and re-entry before release claims.
  Tried: FND-EXE-368 reads 19FB/1A95's alternate field combinations,
  retained-segment replacement and explicit SS-field access order. Admit field
  producers, aliases, shared CS state and re-entry; matching terminal 1E34 remains open.
  Tried: FND-EXE-369 reads 1E34/06C2/1DBE/25FD's encoded pair gate,
  rounded quantity, saved-BX return and sentinel-selected state changes. Admit
  bounds/base writers, native preservation/results, extents, aliases and shared state.
  Tried: FND-EXE-371 reads 1BD6/1BE0's quantity conversion and candidate
  search plus 1BB3's split stores and distinct fitted returns. Follow 1AF5/1B59,
  unit/extent admission, topology, shared CS-state writers, aliases and re-entry.

- Q-EXE-008. FMT-EXE-006: Which disc helpers does the disc installer launch?
  Settles it: direct installer launch references and their selection inputs.
  Tried: helper contents and GOG wrapper (FND-EXE-008, FND-EXE-010), which
  do not cover the disc installer. Split from Q-EXE-004. Blocks: complete
  disc-installer caller coverage.

- Q-EXE-010. FMT-EXE-005: Which analyzer-owned overlay body fragments
  represent native code under the original code-segment and jump-table bindings?
  Settles it: establish the native CS producers and input-to-index bounds,
  validate every admitted table target and reconcile each suspect function body
  with source code/fixup/padding regions before replacing inventories.
  Tried: FND-EXE-173 classifies all fifteen anomalous body spans separately
  from their valid overlay entries and compares one bounded table under the
  descriptor and analyzer-alias segment bases. FND-EXE-174 records the fresh
  corrected-derivative partitions; the repair alone does not reconcile bodies.
  FND-EXE-175 supplies a bounded resident candidate and explicit trace stops;
  read its native callers and segment/input producers before using its callees.
  FND-EXE-176 adds the relocated state segment and initial handler pointer,
  with live pointer/vector writers and native CS admission still unresolved.
  Tried: FND-EXE-221 independently bounds descriptor 198's adjacent
  eighteen-slot table and checks all four distinct descriptor-base targets;
  three are undisassembled in the corrected snapshot. Native CS and candidate
  instruction admission remain necessary before body reconciliation.
  Tried: FND-EXE-222 decodes all four descriptor-base candidates from the
  shipped source and closes their conditional tails with the separately
  bounded eleven-slot table. This does not admit native CS, whole-function
  ownership, incoming frames or runtime dispatch.
  Tried: FND-EXE-223 extends the source traversal to the outer candidate;
  three additional computed jumps require independent consumer/bound readings.
  Tried: FND-EXE-224 independently bounds all four tables and supplies a
  conditional outer-body traversal. Native entry/CS, input/frame admission
  and analyzer ownership still require reconciliation before replacement.
  Tried: FND-EXE-225 resolves the candidate's stored trampoline target and
  compares complete body sets: missing candidate chunks and extra ownership
  beyond the descriptor both require review, not just scalar size correction.
  Tried: FND-EXE-226 supplies the initial resident handler's direct callee
  and unresolved far callback; its complete CFG still assumes all calls return.
  Tried: FND-EXE-227 identifies the far-jump rewrite and its separate live
  segment source, retaining optional-callee and post-call count/segment effects.
  Tried: FND-EXE-228 locates the segment-field store from a post-call state
  reload; state-word writers, saved-header storage and seven callee effects
  remain unread. Its returning-call CFG does not prove allocation or termination.
  Tried: FND-EXE-229 reads the comparison helper's word/carry results and
  caller restoration, with semantic ES-write control. Other state writers,
  publisher callees and native segment/storage admission remain unresolved.
  Tried: FND-EXE-230 identifies one source-word writer and its wrapped
  header-word increment; actual input bounds, other writers and the remainder
  of the writing procedure remain unread.
  Tried: FND-EXE-231 reads that writer's later DS switch and both paths'
  ordered stores; actual segment/header admission, destination aliasing and
  other source-word writers remain unresolved.
  Tried: FND-EXE-232 reads the carry-path helper's resets, subtractions and
  final replacement; traversal/stack bounds, source-word writers and its
  remaining callee effects still need admission.
  Tried: FND-EXE-233 reads the shared callee's publication/copy/store order
  and separate wrapped copy and rewrite counts. Its tail callee, physical
  aliases, buffer/stack bounds and native header admission remain unresolved.
  Tried: FND-EXE-234 reads the tail helper's incoming-BP SS word traversal,
  matching stores and caller result consumption; native link/stack admission,
  saved-slot aliases and traversal/output bounds remain unresolved.
  Tried: FND-EXE-235 reads cleanup rewrite/callback/clear order and returned
  CX consumption. The DS-relative near callback's writers and native target,
  output bounds and segment/saved-slot admission remain unresolved.
  Tried: FND-EXE-236 reads the near-callback default and two replacement
  stores; native writer DS, replacement-target effects, other scalar hits'
  storage identities and invocation order remain unresolved.
  Tried: FND-EXE-237 reads the replacement callback's ordered state-bit
  dispatch and both direct targets; native DS/caller admission, state-byte
  writers and the two callee effects remain unresolved.
  Tried: FND-EXE-238 reads the priority callee's carry gates, unresolved far
  callback, saved registers, full-width product and far error transfer.
  Direct callee effects, pointer writers, stack aliases and native state remain open.
  Tried: FND-EXE-239 reads the shared gate's sole explicit carry-clear
  normal return and six call sites; three distinct callees, native state,
  saved-stack integrity and intervening writes remain unresolved.
  Tried: FND-EXE-240 reads all three shared-gate helpers' explicit size,
  selection and link effects. Native inputs, link/field writers, aliases,
  traversal/output bounds and caller result consumption remain unresolved.
  Tried: FND-EXE-241 follows gate helper outputs through both paths' stores
  and stack-depth balance, including final AX/carry results. Native inputs,
  field/link writers, aliases, bounds and saved-slot integrity remain open.
  Tried: FND-EXE-242 reads a source-derived segment-to-header candidate's
  bounds, preceding-segment link, marker and published-segment checks.
  Native callers, input/state admission and field writers remain unresolved.
  Tried: FND-EXE-243 reads the adjacent state consumer's separate SS word
  traversals and store/cleanup order; its incoming bound does not cover the
  second traversal. Native frames, link writers and independent bounds remain open.
  Tried: FND-EXE-244 identifies loader-entry writes to the lower/upper bounds
  and current segment from SS-relative inputs, followed by a post-call wrapped
  difference check. Native inputs, preservation and the intervening callee remain open.
  Tried: FND-EXE-245 reads the intervening descriptor scan and size callee,
  identifying the threshold producer and direct bound-store exclusions.
  Native inputs, descriptor/header aliases and saved-slot preservation remain open.
  Tried: FND-EXE-246 reads the post-bound helper's first header-segment
  publications and concrete downstream calls, including an interior writer
  entry. Arithmetic/input admission, downstream effects and aliases remain open.
  Tried: FND-EXE-247 reads the interior writer entry's separate caller guard,
  skipped outer obligations and returned-register consumption. Native count,
  header/SS aliases, output bounds and other incoming transfers remain open.
  Tried: FND-EXE-248 reads the content-transfer helper's seek/read requests,
  unchecked seek carry, short-read exit and actual caller carry consumption.
  DOS effects, handle/offset writers, destination extent and aliases remain open.
  Tried: FND-EXE-249 reads the post-transfer word rewrites, shifted-count edge
  and nested pattern/search contract. Native table/count admission, independent
  extents, saved-stack aliases and other incoming transfers remain open.
  Tried: FND-EXE-250 follows the header-dispatched transfer wrapper's carry
  consumption and the handler's ordered publications and loop re-entry values.
  Live dispatch, counter/link writers, aliases and termination remain open.
  Tried: FND-EXE-251 extends the vector initializer through handle open/close
  publications and the actual caller's far-return frame and carry consumption.
  DOS outcomes, name/mode producers, state identity and other writers remain open.
  Tried: FND-EXE-252 supersedes the incorrect full-cleanup gate reading; both
  branches reach the two callback continuations. Post-callee DS identity, far
  return frames, live targets and preservation remain open.
  Tried: FND-EXE-253 reads both replacement cleanup bodies and their distinct
  state-clear paths, interrupt and post-call reloads. Live slot/segment admission,
  external contracts, flag/pointer writers and low-memory aliases remain open.
  Tried: FND-EXE-254 follows the first callback writer's helper-result gate,
  early flag and argument stores and fresh post-call product publications.
  Native frame/segment preservation, cleanup-flag writers and bounds remain open.
  Tried: FND-EXE-255 traces the setup helper's pre-failure repeat gate, cleanup
  flag/handle producers and duplicated full-word status. Native lifecycle,
  interrupt contracts, argument writers and aliases remain open.
  Tried: FND-EXE-256 reads the second callback writer's exact pair comparisons,
  pre-call publications and conditional two-pass mechanism. Native local/frame
  preservation, argument/state writers and helper effects remain open.
  Tried: FND-EXE-257 reads the second setup helper's external/direct paths,
  pre-failure gate and distinct full-word and partial-byte stores. Native pointer
  and flag writers, external contracts and partial-byte initialization remain open.
  Tried: FND-EXE-258 follows the full second initializer's retained pointer,
  fallback publications, partial-byte consumers and endpoint-difference result.
  Environment/callee contracts, local output admission and lifecycle remain open.
  Tried: FND-EXE-259 follows the local-buffer helper's separate request paths,
  rounded/truncated counts, byte classification and carry/result contract.
  External initialized extent, request writers and effective aliases remain open.
  Tried: FND-EXE-260 records selected initial request words and nonzero neighbors
  of partial-byte stores. Live preservation, consumer layout, overlapping writers
  and initialized output extent remain open.
  Tried: FND-EXE-261 connects the direct transfer wrapper's rounded count,
  source/destination formation and carry consumer, including maximal-size wrap.
  Live dispatch, header writers, external zero-count/output contracts remain open.
  Tried: FND-EXE-262 records zero initial/current link words and the initial
  word's unresolved relocation query. Live producers, indexed/bulk writes,
  other code regions and effective DS/lifecycle admission remain open.
  Tried: FND-EXE-263 checks wider positive scalar candidates and an independent
  source consumer missing from the saved listing. CS overrides and incomplete
  analyzer coverage prevent admitting them as exhaustive state-link writers.
  Tried: FND-EXE-264 follows indexed name-buffer writes into the initial state
  segment: bounded tail output does not bound the preceding prefix. Native
  prefix limits, frame/segment preservation and overlapping writes remain open.
  Tried: FND-EXE-265 distinguishes the direct tail caller from the earlier
  external-string copy, whose rewind does not undo writes. Native external
  input limits and interrupt/frame/segment preservation remain open.
  Tried: FND-EXE-266 measures the initial default input's eleven-byte extent
  including zero and checks relocation overlap. Live preservation and the
  separate prefix/external-string output bounds remain open.
  Tried: FND-EXE-267 follows stack-buffer read gates, caller carry consumption,
  seek continuation and the offset calculation's overwritten carry. External
  initialized output, frame preservation and loop/input admission remain open.
  Tried: FND-EXE-268 identifies another initial loader-segment pointer whose
  offset overlaps a known comparison input, not the selected root. Pointer
  consumers, alternative transfer representations and native entry remain open.
  Tried: on 2026-10-09, INST-resident read-only queries for memory scalars
  0x02A6/0x02A8 and exact references to 55E8:02A6, 55E8:02A8 and
  4AE5:0010 returned no listing matches. An independent resident raw-operand
  candidate scan also supplied no positive lead at those two displacements.
  The scan used FND-EXE-236's installed identity, shipped range
  0x00005200..0x0004AEE0, and sixteen-bit decoding from up to six bytes
  before each matching little-endian displacement; it did not admit boundaries.
  These are uncontrolled negative leads, not absence evidence: scalar queries
  cover decoded instructions only, references require analyzer-defined objects,
  and indexed/aliased/runtime-produced accesses remain excluded. Do not repeat
  these literal queries without new mapping or reference coverage; next follow
  indirect target producers and segment-qualified aliases from admitted code.
  Tried: FND-EXE-269 follows the declared MZ startup's source-segment slot
  publication across its DS switch. Incoming storage, interrupt preservation,
  other slot writers and native loader entry remain open.
  Tried: FND-EXE-270 reads startup's first callee and the adjacent restoration
  candidate, distinguishing stored vector pairs and DS changes/restoration.
  External effects, saved-stack preservation and restoration/native loader
  admission remain open.
  Tried: FND-EXE-271 links the bounded startup initializer selector and initial
  pointer to a wrapper that constructs the root's far frame and tests its word
  result. External allocation, startup preservation, other callers and live
  target/input writers remain open.
  Tried: FND-EXE-272 follows the root wrapper's allocation request through
  paragraph conversion, link traversal and shared DS restoration. Four helper
  effects, list/slot writers and returned initialized extent remain open.
  Tried: FND-EXE-273 reads the exact-size unlink helper and ordinary DS
  restoration; incoming DX, live link writers, physical aliases and the other
  allocation helper contracts remain open.
  Tried: FND-EXE-274 reads the larger-count split helper and its DS/DX returns;
  incoming DS/DX identity, header writers, arithmetic bounds and aliases remain
  unresolved. The other helper contracts remain unread.
  Tried: FND-EXE-275 reads the fallback and its separate alignment call;
  the 1000:1831 contract, DS preservation, failure effects and returned extents
  remain unread. Follow that callee before assuming contiguous allocation.
  Tried: FND-EXE-276 reads the fallback request callee and saved-pair return;
  arithmetic/comparison helpers, final state updater and all state-word writers
  remain unread. Preserve the signed guard and failure-state obligations.
  Tried: FND-EXE-277 reads the arithmetic and comparison helpers, including
  normalization, wrapping and local DS preservation. State-word admission,
  writer coverage and the final updater still prevent closure.
  Tried: FND-EXE-278 reads the final updater's argument cleanup and bound
  publication on failure. Its far callee, post-call preservation, state-word
  writers and admitted arithmetic ranges still prevent closure.
  Tried: FND-EXE-279 follows the far wrapper and error-helper cleanup, tracing
  the saved post-interrupt BX into the updater result. Interrupt effects,
  post-interrupt DS/SI preservation and state/table writers remain unresolved.
  Tried: FND-EXE-280 reads the initial allocation helper, its ignored intermediate
  results and shared-state publications. Remaining state/link writers, shared
  DS-slot lifetime, aliases and interrupt effects still prevent closure.
  Tried: FND-EXE-281 reads a list-link writer candidate, including temporary SS
  access and flag restoration before its final stores. Establish incoming
  transfers and DS producers, other writers, aliases and interrupt admission.
  Tried: FND-EXE-282 establishes one list-writer call and incoming-DX DS
  producer, plus the merge fall-through into unlinking. Trace incoming DX,
  other callers, header/count writers and admitted segment ranges next.
  Tried: FND-EXE-283 reads selected cleanup's branch publications and retained
  pair through unlinking. Existing FND-CONFIG-167 covers wrapper selection;
  segment/state producers, other callers and saved-DS lifetime remain open.
  Tried: FND-EXE-284's controlled relocated-call census adds a gated cleanup
  caller. Read its preceding far callees and pair/state writers; retain near,
  computed and unrelocated caller exclusions and the literal-query retry.
  Tried: FND-EXE-286 resolves and reads those two callee bodies, including
  overwritten outgoing arguments and saved-AX restoration. Nested calls,
  DS provenance, state writers and interrupt effects still prevent closure.
  Tried: FND-EXE-287 reads the nested interrupt wrappers and resolves the
  temporary data segment. Trace pointer/state writers and admitted lifecycle;
  native interrupt effects and first-wrapper DS preservation remain unresolved.
  Tried: FND-EXE-288 follows the cleanup's saved-pointer and byte-gate setup,
  including outgoing argument writers and unchecked interrupt continuations.
  Other writers, returned-pointer admission and lifecycle callers remain open.
  Tried: FND-EXE-289's controlled setup incoming query returns one relocated
  candidate, but isolated decoding and an ungrounded earlier start do not
  establish its entry path. Recover an independently grounded caller next.
  Tried: FND-EXE-290 follows the entry already named by FND-INPUT-005
  through shared publications and both setup calls. Recover the incoming
  candidate's path and argument writers, allocation DS/CX preservation,
  the second setup callee and state lifetime before closure.
  Tried: FND-EXE-291 follows the second setup callee and its interrupt
  wrapper, with final outgoing word writers and ordered failure publications.
  Continue incoming caller provenance, allocation preservation and state
  writers; interrupt effects and post-interrupt DS remain unresolved.
  Tried: FND-EXE-292 follows the existing resident entry to the setup
  candidate, with pointer/size widths and discarded gate-setter returns.
  Continue its storage producer, upstream entry admission and state writers;
  nonzero storage and local gate publications do not settle native readiness.
  Tried: FND-EXE-293 reads the supplied-storage producer's request increment,
  header-derived write and unchanged returned pair. Continue allocator header
  production and returned segment/offset bounds, state writers and lifetime;
  the unchecked nonzero path does not establish a valid writable extent.
  Tried: FND-EXE-294 traces DX into DS at each list comparison and the
  exact/split helper inputs, with the selected request's conditional address
  calculation. Continue actual segment admission, header/count writers,
  aliases and extent lifetime; local segment equality is not valid storage.
  Tried: FND-EXE-295 records shipped zero words and follows an adjacent
  wrapper to another shared saved-DS writer and nested allocator path.
  Continue its incoming callers, helpers 1000:169E and 1000:1622,
  other writers and aliases; shipped data does not establish live state.
  Tried: FND-EXE-296 follows helper 1000:169E's two header-publication
  paths and unchecked callee continuations. Continue 1000:1622, actual
  segment/header admission, other writers and aliases; offset four does
  not prove that a request or cleanup succeeded.
  Tried: FND-EXE-297 follows 1000:1622's DX-only allocation test,
  header-count copy bound and discarded release result. Continue actual
  header/extent admission, shared request and DS writers, overlap, segment
  wrap and lifetime; the local byte bound does not prove valid storage.
  Tried: FND-EXE-298 bounds direct request-word scans and the wrapper's
  incoming domains, retaining an additional allocator near-call candidate.
  Ground that candidate's path, CS and arguments next; other write/call
  forms, aliases and admitted header state remain unresolved.
  Tried: FND-EXE-299 connects a relocated incoming candidate to the
  product-request body and follows its chunk-fill argument writers.
  Continue incoming entry/path and argument admission, 1000:0583,
  direction-flag provenance and valid extents before closure.
  Tried: FND-EXE-351 reads 1000:0583's signed delta branches and shared
  tail, ordered local-pointer stores and SI preservation. Continue outer
  caller admission, direction provenance, actual extents, aliases and
  lifetime; encoded normalization is not validated destination storage.
  Tried: FND-EXE-352 connects FND-CONFIG-209's existing wrapper reading
  and FND-CONFIG-183's grounded overlay request, with separate MZ/FBOV
  incoming controls. Continue remaining caller admission and direction
  provenance; the concrete request pattern does not establish valid extent.
  Next: the segment source state-word and live header/callee admission,
  then descriptor 198's native entry
  and CS admission, then the remaining suspect target producers.
  Blocks: reconciled executable denominator and complete-reading declarations
  that depend on those function boundaries.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-EXE-005. FMT-EXE-006: Which code page interprets the disc sound helper's
  display bytes? Settles it: a distribution declaration or code-page/font
  selection record tied to the disc helper's actual launch environment and
  distinguishing compatible decoders. Tried: FND-EXE-008's complete byte
  profile does not select a unique code page. FND-EXE-340's new complete-file
  literal-token searches of the declared GOG configuration pair find no
  codepage= or country= token, and the written keyboard layout is auto.
  That installed launch configuration does not establish the disc helper's
  executing environment. Reopen static work only with a declaration or
  selection record that distinguishes the decoder; host-internal complete
  readings remain outside owner-approved scope. Split from Q-EXE-002.
  Blocks: original display encoding identification.

## Blocked

- Q-EXE-006. FMT-EXE-006: How does the supported interpreter handle the
  undefined jump labels and colon-suffixed target in the disc sound helper?
  Settles it: static reading of the actual interpreter's label matching and
  error paths, with an owner observation only for environment-dependent
  behavior. Tried: complete label and jump reading (FND-EXE-009); the file
  cannot decide how the shell interprets those tokens. New reading:
  SRC-DOSBOX-GOG-0742 predicts literal trailing-colon matching and deletion
  of the active batch on a failed label search. Next static step: establish
  the relevant source-to-shipped-binary correspondence. FND-EXE-011 and
  FND-EXE-012 now locate compiled token normalization, search and conditional
  normal cleanup. FND-EXE-013 now reads the command-record consumer and its
  two-word call target. Next: trace batch-line production and the parser
  helper outputs into this input path, then external file helpers and
  exceptional cleanup. These partial direct readings do not establish the
  complete shell outcome.
  Blocks: complete shell outcome description.
  Waiting on: an owner scope change admitting DOSBox complete-reading work.
  The 2026-10-09 decision in docs/DECISIONS.md excludes this host-code
  question from game-restoration priorities; its evidence remains historical.

- Q-EXE-009. FMT-EXE-006: How does the shipped interpreter resolve the bare
  ravager/sound commands and continue after them in the declared GOG wrapper?
  Settles it: complete interpreter command-search, batch chaining and EXIT
  reading under declared working directory, mounts and configuration order;
  mutable overlay substitutions remain conditional. Tried: complete wrapper
  text (FND-EXE-010), bundled source lead (SRC-DOSBOX-GOG-0742), compiled
  dispatch/CALL and conditional batch replacement (FND-EXE-013, FND-EXE-014),
  local/PATH candidate order and the count-80 scan (FND-EXE-015, FND-EXE-016),
  initial selector production/writers and drive-command gates (FND-EXE-017,
  FND-EXE-018, FND-EXE-019), filename byte/component transformations and
  retained failure writes (FND-EXE-020, FND-EXE-021), and two pointer-installation
  paths with an object-plus-four prefix transfer (FND-EXE-022), their bounded
  indexed-writer controls (FND-EXE-211), and record
  append/growth arithmetic and publication boundaries (FND-EXE-023), and
  allocation/release import and local retry boundaries (FND-EXE-024), and
  failure-object prefix/bitmap fallback boundaries (FND-EXE-025), and a bounded
  append/initializer caller sequence (FND-EXE-026), and an earlier list producer
  with insertion before status dispatch (FND-EXE-027), and the bounded output
  table/default paths and shared reread (FND-EXE-028), and a shared helper
  sentinel-linked search (FND-EXE-029) and stored-length comparison
  operations (FND-EXE-030), a collection creation/link path (FND-EXE-031),
  signed field-publication branches (FND-EXE-032), and preceding-word addition
  helpers with distinct return contracts (FND-EXE-033), and negative-path
  payload-copy/length publication (FND-EXE-034), and storage capacity rounding
  and prefix initialization (FND-EXE-035), and the capacity-limit temporary
  construction/decrement/publication path (FND-EXE-036), and object
  first-word/payload-field construction order (FND-EXE-037), and temporary
  input/end production and range copying (FND-EXE-038), and null-input
  construction and failure publication (FND-EXE-039), and raw-prefix
  release forwarding (FND-EXE-040), and mutable final-target/import
  boundaries (FND-EXE-041), and encoded shared-record reading
  (FND-EXE-170, including the physical CRT failure import and conditional local continuations; SRC-MS-CRT-ASSERT is an external contract only), and shared-record initialization/publication
  (FND-EXE-043), and copied-tail/local-name termination
  (FND-EXE-169), and lazy initialization/record publication
  (FND-EXE-167), and mode admission/flag waiting
  (FND-EXE-200), and conditional negative-mode duplicate-word clearing
  (FND-EXE-201), and gate-neighbor unsigned-index publication limits
  (FND-EXE-202), and its direct allocation/import preservation boundary
  (FND-EXE-203), and neighboring cursor fixed-store/zero-request distinctions
  (FND-EXE-204), and scalar publication/indirect-write and reused-slot limits
  (FND-EXE-205), and controlled shared numeric operand-domain agreement
  (FND-EXE-206), and bounded-width physical overlap-start encodings
  (FND-EXE-207), and resource/helper-derived mode publication
  (FND-EXE-047), and exact record/wait imports and zero-result tail return
  (FND-EXE-048), and pre-helper saved-link cleanup through a fresh mode
  (FND-EXE-049), and construction-caller setup/cleanup return handling
  (FND-EXE-050), and recovered stored-handler prefixes and forwarding paths
  (FND-EXE-051), and selected-record publication and saved-state transfer
  (FND-EXE-052), and register-input selection with mutable-local traversal
  (FND-EXE-053), and second-selector callback order and distinct result gates
  (FND-EXE-054), and nested callback record writers and early status returns
  (FND-EXE-165), and selected-record access and wrapped field adjustments
  (FND-EXE-056), and signature-selected state and seven-return preparation
  (FND-EXE-057), and ordinary signed admission, iteration and six-return stores
  (FND-EXE-196), and matching byte consumption and terminating word outputs
  (FND-EXE-059), and metadata markers, cursor returns and relative targets
  (FND-EXE-060), and modifier mask classes, marker bypass and zero callees
  (FND-EXE-061), and guarded typed reads, zero bypass and cursor return
  (FND-EXE-062), and nibble-nine byte termination and sign-fill output
  (FND-EXE-063), and marker-stride matching, low-byte virtual-result publication
  and decoded-zero index scans (FND-EXE-214), and matching classification, opposite helper-result tests
  and full-word fallback flags (FND-EXE-215), and pair-decoder direct write/frame
  separation from the counter and saved cursor (FND-EXE-216), and the second terminal wrapper, normal-return fallback
  and initial shared-target tail dispatch (FND-EXE-066), and classification-one counter/link effects, saved payload
  and untested caller finalization (FND-EXE-067), and guarded context acquisition, converted TLS returns
  and post-publication zero stores (FND-EXE-068), and initialization index stores, converted guard writes
  and ignored callback values (FND-EXE-069), and stored-handler state branches, signed cleanup counters
  and the separate context getter (FND-EXE-070), and head-associated indirect target guards, outgoing slots
  and passed-through returns (FND-EXE-071), and published head-target mode admission, optional callback
  and adjusted-payload tail dispatch (FND-EXE-072), and payload-range bitmap clearing, separate guard reads
  and prefix-adjusted free (FND-EXE-073), and exact pool-associated wait/signal imports
  with full-word predicates and ignored caller returns (FND-EXE-074), and the pool counter writer,
  unchecked semaphore creation result and once completion (FND-EXE-075), and independent physical
  guard-address candidates absent from decoded references (FND-EXE-076), and their controlled
  read classification with conditional frame/state prefixes (FND-EXE-077), virtual-only guard
  storage and declared relocation-site limits (FND-EXE-078), and actual startup callees
  with an empty row-update route (FND-EXE-171), pre-dispatch guard publication and reverse callbacks
  (FND-EXE-080), and the selected prefix's paired increments and distinct context initializer
  (FND-EXE-081), mutable cleanup-cursor reloads and next-slot publication
  (FND-EXE-082), and the first cleanup target with its separately admitted pointer-constructor path
  (FND-EXE-083), shared old-value decrement gates and ordered resource-call continuations
  (FND-EXE-084), and reverse-slot releases with three-word initialization
  (FND-EXE-085), distinct status/handler gates (FND-EXE-086), signature-selected
  head effects and fresh-mode saved-state transfer (FND-EXE-087), and overlap
  construction with distinct stored handlers (FND-EXE-088), selected-callback
  release admission and first-word cleanup (FND-EXE-089), and stored-target
  dispatch with distinct low-byte fallback gates (FND-EXE-090), and retained
  pre-reader modifier/displacement with a physical zero-return fallback target
  (FND-EXE-091), and composed metadata first-field admission, stores and
  continued cursor stages (FND-EXE-092), PATH source/key/output-helper gates
  (FND-EXE-093), and word-boundary reads with bounded record copying
  (FND-EXE-094), final output replacement/alias gates (FND-EXE-095), and
  preparation spans, releases and publication (FND-EXE-096), and composed
  prefix-base admission with a recovered preparation handler (FND-EXE-097),
  and consecutive/list mapping reset producers with mutable fallback first-word
  selection (FND-EXE-098), and physically selected fallback methods with
  fresh-table word composition and a byte transfer route (FND-EXE-099),
  and biased mapping publication, source reentry and retained-result reset/
  restoration (FND-EXE-100), and category/flag mapping-state admission with
  copied-word publication (FND-EXE-101), and helper depth/callback publication
  with distinct conditional and normal restoration (FND-EXE-102), and grounded
  wait-target/callback linkage with signed dispatch and full-width retry
  gates (FND-EXE-103), and admission sum exits, callback-list order and
  budget publication (FND-EXE-104), and free-node callback insertion with
  repeated-match removers (FND-EXE-105), and pool link initialization with
  retained-successor callback traversal (FND-EXE-106), and fixed-target
  insertion/removal wrapper inputs (FND-EXE-107), and fixed callback slot
  selection and shifted-word forwarding (FND-EXE-108), and downstream
  dispatch/value-two clearing (FND-EXE-109), and value-seven flag priority
  and gated state transfers (FND-EXE-110), and value-one capture/progress
  and reinsertion (FND-EXE-111), and zero-branch progress/read ordering
  and fresh post-call count decisions (FND-EXE-112), and first-callee
  byte-write/removal and scheduling prefix (FND-EXE-113), and equality
  priority/state-call continuation (FND-EXE-114), and local-F-zero
  second-record writes/returns (FND-EXE-115), and nonzero-F counter/write
  ordering and fresh current-byte reads (FND-EXE-116), and exact-equality
  byte merge with fresh write arithmetic (FND-EXE-117), and shared counters
  with latch-setting tail insertion (FND-EXE-118), and gate-absent
  priority/state suffix admission (FND-EXE-119), and post-record nonzero-byte
  priority/local-F publication (FND-EXE-120), and bounded shared slot-byte/
  mask publishers (FND-EXE-121), and optional first-mode slot admission/clearing
  (FND-EXE-122), and second-mode bounds/group gate/load ordering (FND-EXE-123).
  Selected-callee prefix gates, reader calls and recursive fallback are recorded
  in FND-EXE-124; full-width mapping and boundary-byte assembly in FND-EXE-125.
  Physical full-width targets and shared four-call return widths are recorded
  in FND-EXE-126; larger zero-mode publication/reentry and full return in FND-EXE-127.
  Nonzero-mode lookup and missing-entry reloads are recorded in FND-EXE-128.
  Mode/entry-bit selector gates and precedence are recorded in FND-EXE-129.
  Retained-entry publication, full-width reentry and last-entry cleanup are recorded
  in FND-EXE-130; selected-prefix admission/physical targets/default in FND-EXE-131.
  Prefix width masks, retain-mask branch and selector clearing are recorded in FND-EXE-132.
  All physical byte-indexed mask contributions are recorded in FND-EXE-133.
  Distinct word-field and byte-count/full-width mask branches are recorded in FND-EXE-134.
  FND-EXE-135 records byte/full-width exact-input gates and retained/fresh lookup ordering;
  FND-EXE-136 records complementary count gates and fresh-word versus full-width comparisons;
  FND-EXE-137 records signed word/full-width shifts and their fresh-count mask gate;
  FND-EXE-138 records the signed byte shift with retained count and lookup inputs;
  FND-EXE-139 records full-width logical shift and fresh-count one/sign mask gates;
  FND-EXE-140 records zero-extended word shifts and fresh-word count/retained-sign gates;
  FND-EXE-141 records zero-extended byte shifts with retained count/lookup and saved-byte sign;
  FND-EXE-142 records complementary full-width shifts and retained sign/fresh tail gates;
  FND-EXE-143 records the complementary word-count boundary and continued sign gates;
  FND-EXE-144 records the complementary byte-count boundary and saved/retained inputs;
  FND-EXE-145 records the full-width result-nibble equality and exact-result gates with retained mask one;
  FND-EXE-146 records word/full-width retained-result widths, opposite nibble gates and shared-tail input producers;
  FND-EXE-147 records retained-byte result comparisons and lookup inputs with opposite nibble gates;
  FND-EXE-148 records guarded full-width admission, fresh byte masks and retained sign inputs;
  FND-EXE-149 records shared word comparison inputs, later result-word reads and saved-byte lookup admission;
  FND-EXE-150 records guarded word admission, full guard width and retained-word/saved-byte inputs;
  FND-EXE-151 records saved original bytes before working increment and register reuse in guarded byte admission;
  FND-EXE-152 records shared byte comparison and saved D/V inputs without guard admission;
  FND-EXE-153 records shared full comparison with fresh byte inputs and a later full result read;
  FND-EXE-154 records retained-word zero-nibble and exact-minimum tests with a fresh-byte lookup;
  FND-EXE-155 records guarded byte equality, saved original inputs and a flipped high-mask XOR;
  FND-EXE-156 records guarded word equality with retained words, fresh byte inputs and later word D;
  FND-EXE-157 records guarded full equality with fresh d/n/v bytes and later full D;
  FND-EXE-158 records strict byte comparison, saved inputs and bounded partial-register mask work;
  FND-EXE-159 records strict word comparison with retained words, fresh bytes and later word D;
  FND-EXE-160 records strict full comparison with retained inputs, fresh n/d/v bytes and later full D;
  FND-EXE-161 records a gated save/restore writer route with AL-only admission and a byte-derived return;
  FND-EXE-162 records its stored callback with signed return gates and a conditional last-pair comparison;
  FND-EXE-163 records the diagnostic first transfer with mutable candidate traversal and distinct saved-state admission;
  FND-EXE-164 composes its candidate reset with register-input selection and full-seven state-transfer admission;
  Runtime writers/other prefix effects, table/slot producers and remaining selected callee
  effects remain open. These bounded
  readings do not establish the complete shell outcome. Next: trace record
  construction and caller invariants, allocation callback/exceptional contracts, prefix
  length/storage/alias contracts, concrete
  vtable targets and remaining pointer/selector writers, then PATH admission,
  flag/EXIT continuations and declared mount/overlay inputs. Unread virtual
  and CRT effects stay conditional; truthy return is not proof of an update.
  FND-EXE-166 narrows the field reader's decoded direct-call and physical
  address-word searches and its post-setup argument reload. FND-EXE-208 adds
  the physical E8/E9 and decoded interior-flow comparison; its excluded
  transfer representations remain open. Next check: setup preservation of
  that incoming slot, selected-local lifetime,
  offset-28 field writers and excluded computed/indirect uses. FND-EXE-197
  bounds one shipped prefix's conditional five-byte read; runtime selection,
  field/source preservation and aliases remain required. FND-EXE-198 narrows
  the selected-local's forwarding-frame origin and ordinary nested lifetime.
  FND-EXE-209 supplies two concrete callback/metadata producers; admit each
  selected origin; FND-EXE-210 bounds both distinct conditional prefix extents.
  Source/field preservation and downstream stream/target consumption remain open.
  FND-EXE-212 adds branch-specific count writers and finite conditional pair
  consumption; fixed-arm admission, preservation and later matching still need evidence.
  FND-EXE-213 follows its count-one nonmatching-signature negative lookup to
  conditional saved six; actual selection, other routes and cleanup remain open.
  FND-EXE-167 narrows direct setup writes under the flat-address model;
  shared-base provenance, DS/SS identity, indirect aliases and preservation
  through the other setup routes and callees remain required.
  SRC-WIN32-X86-ABI supplies the external flat-mode/register-preservation
  contract; its sample selectors do not establish native DS/SS bases, and
  generic calling conventions cannot replace local argument/effect readings.
  FND-EXE-199 narrows zero-mode incoming-slot overlaps using the actual mode
  guard and duplicate outgoing words. Nonzero-word admission/preservation,
  remaining partial overlaps and selected-local aliases remain required.
  FND-EXE-168 narrows explicit decoded publication sites and separates fresh
  allocation from decoded existing/fallback origins. Next: decoded-record
  input admission and allocator/storage lifetime; retain excluded indirect
  writers and segment identities. SRC-WIN32-ATOMS adds the published local
  atom contracts: case-insensitive matching preserves the first name's case,
  and retrieval returns a copied length. Check unchanged identifier/name
  admission and input extent against FND-EXE-169; those external contracts
  do not prove native buffer, pointer or lifetime state. Do not count
  the bounded body as a complete reading before those inputs are admitted.
  FND-EXE-172 completes the independent physical rel32 comparison for
  FND-EXE-171's fixed-bound helper. Remaining caller/interior admission
  needs other transfer representations and computed/runtime target producers;
  matching physical and decoded domains alone does not settle those.
  Complete-reading prerequisites for the selected-record reader are tracked
  separately in Q-EXE-011, Q-EXE-012 and Q-EXE-013. Their closure does not
  settle the rest of this wrapper question or the downstream metadata stream.
  Blocks: resolved wrapper-helper and continuation description.
  Waiting on: an owner scope change admitting DOSBox complete-reading work.
  The 2026-10-09 decision in docs/DECISIONS.md excludes this host-code
  question from game-restoration priorities; its evidence remains historical.

- Q-EXE-012. FMT-EXE-006: Which shipped-code transfers can enter the
  selected-record reader or an interior instruction in its direct body?
  Settles it: locate every admitted caller and interior entry for the
  half-open body in FND-EXE-166, resolving target producers for relevant
  indirect/computed transfers and checking unsearched encoded transfer kinds
  with independently located positive controls. Retain each search's domain,
  cap, source identity and exclusions; analyzer boundaries are not controls.
  Existing evidence: FND-EXE-166 covers decoded direct calls and exact raw
  address words; FND-EXE-208 compares physical rel32 and decoded interior
  targets. Next: classify the excluded transfer representations and establish
  which can actually supply this body as a target; follow their producers.
  Tried: FND-EXE-219 adds controlled short/conditional forms and a raw
  short-jump candidate in the preceding undecoded gap. Five-form physical
  and controlled decoded incoming searches find no admitted direct route
  into that gap. Next: its excluded transfer/prefix and alternate-stream
  admission, without declaring the raw candidate padding or a caller.
  Tried: FND-EXE-220 extends the controlled physical absolute-word search
  to every gap and reader byte address. Computed, split, relative and
  runtime-written target representations remain outside that search.
  Blocks: reader caller-completeness admission in Q-EXE-009.
  Waiting on: an owner scope change admitting DOSBox complete-reading work.
  The 2026-10-09 decision in docs/DECISIONS.md excludes this host-code
  question from game-restoration priorities; its evidence remains historical.

- Q-EXE-013. FMT-EXE-006: What last writes the selected-record reader's
  actual incoming pointer chain and offset-28 word before its call?
  Settles it: trace the incoming sixth slot, its pointed-to selected local,
  selected record and full offset-28 field through every admitted writer and
  intervening setup/callee path. Admit each record origin's lifetime and
  aliases and read any partial overwrite byte by byte. Use Q-EXE-011 for
  storage identity and Q-EXE-012 for additional incoming routes.
  Existing evidence: FND-EXE-165/166 supply the reload and reader contract;
  FND-EXE-198 supplies one selected-local lifetime; FND-EXE-167/168/200/201
  retain setup, shared-base origin and conditional overlap routes;
  FND-EXE-209 supplies distinct direct metadata producers.
  Next: follow shared-base and selected-record producer admission before
  treating a header predicate, finite file prefix or ordinary ABI contract
  as pointer preservation. Metadata byte-stream consumers stay Q-EXE-009.
  Blocks: reader input/writer completeness in Q-EXE-009.
  Waiting on: an owner scope change admitting DOSBox complete-reading work.
  The 2026-10-09 decision in docs/DECISIONS.md excludes this host-code
  question from game-restoration priorities; its evidence remains historical.

- Q-EXE-011. FMT-EXE-006: Do the selected-record reader's pointer accesses
  address the same storage as the forwarding frame's selected local?
  Settles it: establish the actual initial DS/SS descriptor bases and their
  preservation on the forwarding/selector/setup/reader route, including
  intervening segment setters, external and exceptional effects.
  Tried: FND-EXE-198's frame geometry, FND-EXE-167's distinct pointer/stack
  accesses, FND-EXE-217's controlled decoded output search and opaque-site
  classification, and FND-EXE-218's exhaustive empty-effect class accounting
  within that listing. These do not admit initial native bases. The external
  SRC-WIN32-X86-ABI sample does not establish the loaded process state.
  Waiting on: an owner scope change admitting DOSBox complete-reading work,
  then admissible initial host-descriptor and external-preservation
  evidence. docs/RUNTIME.md admits no native memory/register inspection;
  the current harness covers resident MZ calls, not the host PE. Screenshot
  sessions cannot measure descriptor bases. A new source/tool/reading that
  admits those inputs can reopen this item without relaxing storage identity.
  Unsearched instruction streams and exceptional effects remain explicit;
  Q-EXE-012/013 continue independently, without assuming equal bases.
  Blocks: reader storage-identity admission in Q-EXE-009.
