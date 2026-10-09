# EXE

Next ID: Q-EXE-024

## Static

- Q-EXE-001. FMT-EXE-001, FMT-EXE-002, FMT-EXE-003, FMT-EXE-004: What do the
  remaining overlay-manager fields hold? Which other code reads the pack's
  `segment_table_offset` or `segment_count`; what `unk_02` and `unk_06` hold and
  `flags` 0, 1 and 4 mean for descriptors that are not overlays; what the
  manager keeps in header words `0x0E`, `0x14`, `0x16`, `0x1C` and `0x1E`; and
  does the disc's manager read the pack, the table, the headers and the
  trampolines as the installed one does? Settles it: the manager code after
  `4AE5:0900` in the installed `DSUN.EXE` that uses header offsets `0x0E`,
  `0x14` and `0x16`, the placement code's use of `0x1C`, every reader of the
  segment table at `55E8:0000` (the fixup pass reads it by index), and the
  disc manager's startup, walk, handler and trampoline writer. Tried:
  FND-EXE-560 to FND-EXE-562 read the installed manager's startup, descriptor
  walk, `INT 3Fh` handler and trampoline forms, which settle which fields the
  startup reads, the bit-1 overlay test, header `+0x02`, `+0x10` to `+0x1B`
  and the trampoline forms; FND-EXE-520 and FND-EXE-227/228/248 read the load,
  fixup and placement paths, and FND-EXE-175 to FND-EXE-180 the handler's
  setup. FND-EXE-181 to FND-EXE-195 followed a lead into the host executable,
  which this goal's scope excludes. Blocks: none.
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
  Tried: FND-EXE-372 reads 1AF5/1B59's request quantities, distinct alignment
  checks and saved-segment publication. Follow 1E73, preservation and state
  changes before admitting units, extents, rollback, aliases or re-entry.
  Tried: FND-EXE-373 follows 1E73's signed gate, arithmetic callees,
  candidate bounds and saved-prior-pair return. Admit inputs, stored pairs,
  native preservation, frame aliases and state writers before extent claims.
  Tried: FND-EXE-374 identifies neighboring entry shared-state writes and
  its source-field copy count, chunk bounds and unchecked terminal result.
  Follow its callers, 1CD9, headers, extents, aliases and shared-state lifetime.
  Tried: FND-EXE-375 reads 1CD9's selected field stores, held terminal
  arguments and retained-old-segment return despite ignored results. Admit
  callers, count/link and shared-state writers, extents, aliases and native paths.
  Tried: FND-EXE-376 reads 2CC8/2C46/05CC's first-result guard,
  signed adjusted quantity, scan bounds, ordered stores and native pair test.
  Admit producers, extents, frames, aliases, actual DS and native preservation.
  Tried: FND-EXE-377 follows the actual pathname continuation's matcher,
  table mapper, unchecked position calls and first-byte suppression. Follow
  2D48/2F81, continuation 158E:011A and pattern/table/state admission.
  Tried: FND-EXE-378 reads 2D48's three request roles, segment binding
  and signed count adjustment. Follow 2F81 and later pathname
  continuation, record/flag producers, native preservation, extents and aliases.
  Tried: FND-EXE-379 reads 2F81's cached-byte/failure distinction,
  refill result gate and uncached delimiter loop. Follow 2EF1/2EB3/3720/27FC,
  later pathname continuation and producer/preservation/extent admission.
  Tried: FND-EXE-471 reads 2EF1's pointer/count publication and signed
  result plus 2EB3's fixed-count selected pass. Follow 3720/27FC and later
  pathname continuation, table/record producers, preservation, extents and aliases.
  Tried: FND-EXE-472 reads 3720/06FF's count gates, byte transform,
  unchecked trailing-byte slot and 1A position-result discard. Follow 27FC,
  later pathname consumers, native results/preservation, extents and producers.
  Tried: FND-EXE-473 reads 27FC's immediate flag result, held-pair request,
  final returned-pair comparison and carry-error paths. Follow later pathname
  consumers, native contracts/preservation, frame state and index/flag producers.
  Tried: FND-EXE-474 follows pathname continuation 158E:011A's
  selected quantity, overwritten reads, inclusive copy and reverse separator
  search. Follow 158E:01D3, index/frame preservation, extents and producers.
  Tried: FND-EXE-475 follows the second sw32.ini stack pathname,
  its mode and sequential section/field matches. Follow 158E:0284's
  conversion/consumers, actual DS, frame/string extents, aliases and preservation.
  Tried: FND-EXE-476 follows three-byte field conversion, fixed word
  mapping and one-byte Irq conversion store. Follow 2713, 158E:0347's
  consumers, actual DS/frame admission, extents, aliases and preservation.
  Tried: FND-EXE-477 reads 2713's classification, decimal-prefix
  accumulation, width transition, modular sign and selected shipped table bits.
  Follow later stored-word consumers and actual source/table/frame admission.
  Tried: FND-EXE-478 follows Dma and MIDI field collection, repeated
  fixed mapping and discarded cleanup. Follow 158E:0496's consumers,
  retained DI/native preservation, actual DS/frame state, extents and producers.
  Tried: FND-EXE-479 follows repeated table selection and parsed-word
  publication, signed search limits and ambiguous zero lookup. Follow
  158E:06B0, base/limit/row and field consumers, extents, aliases and lifetime.
  Tried: FND-EXE-480 follows final buffer publication, fixed-limit copy
  exhaustion and unchecked optional-call result. Follow formatter 0F25,
  callback, 190F:0000, source/global producers, extents and state admission.
  Tried: FND-EXE-481 follows local formatter flush, callback descriptor
  mutation and nested cleanup. Follow conversion dispatch, input producers,
  total output bounds, aliases and the optional consumer.
  Tried: FND-EXE-482 follows normalized string dispatch, argument width,
  scan exhaustion and signed default padding. Follow actual DS/source and
  callback admission, other conversions and the optional consumer.
  Tried: FND-EXE-483 follows optional consumer text collectors, signed
  length traversal and unchecked row publication. Follow 190F:019F,
  producer/frame extents, initialization calls, aliases and preservation.
  Tried: FND-EXE-484 follows coordinate arguments, row consumers,
  signed size comparison and spacing division. Follow 190F:02DF,
  downstream effects, count/object producers, extents and remaining returns.
  Tried: FND-EXE-485 follows spacing recurrence, row interfaces and
  mixed argument provenance. Follow 190F:04CA, interfaces 08E5 and
  1A7C:01C6, record consumers, extents, aliases and preservation.
  Tried: FND-EXE-486 follows polling, sequential selector transforms,
  record matches and remaining caller returns. Follow 08E5 and 1A7C
  interfaces, selector/count and input producers, extents and preservation.
  Tried: FND-EXE-487 follows nine-byte row registration, byte argument
  consumption and polling mode selection. Follow 00EA, 022A, 20DF,
  readiness/count/flag producers, table extents, aliases and actual DS.
  Tried: FND-EXE-488 follows byte and coordinate modes, result widths,
  readiness loops and numeric sentinel collision cases. Follow 00EA,
  20DF, input/table producers, count admission, extents and preservation.
  Tried: FND-EXE-489 binds readiness to returned BX bit tests and
  coordinate reads to returned CX/DX. Follow native input/segment and
  error-helper admission, table/count producers, aliases and 08E5.
  Tried: FND-EXE-499 follows both game editions' containing diagnostic
  through overlay 180 and a resident word-forwarding interface. Follow
  actual DS, producer and deeper output-like callees before a launch claim.
  Tried: FND-EXE-500 follows diagnostic length, selected byte dispatch
  and buffered/native-write character paths. Follow actual DS and record
  admission, 345B's other branches, flush/error helpers and caller coverage.
  Tried: FND-EXE-502 reads 06BA's signed error mapping and 07B0's
  pre-request flag clear, request arguments and failure pair. Follow actual
  DS, table/state writers, flush paths and broader game caller coverage.
  Tried: FND-EXE-503 reads 2F13/2F94's record updates, mismatch
  handling and aggregate result disposal. Follow 3DCC, actual DS and
  record/table admission, other byte-dispatch branches and game callers.
  Tried: FND-EXE-504 reads 3DCC's quantity guards, expansion and
  mixed-count short-write returns. Follow actual DS/SS and frame/storage
  admission, record/table writers, remaining dispatch branches and game callers.
  Tried: FND-EXE-507 reads the remaining 345B branches and 3220/32FA,
  including overflow-sensitive tests and fallback-byte provenance. Follow
  actual DS/SS, record/table writers, admitted extents and broader game callers.
  Tried: FND-EXE-508 records both shipped diagnostic records and initial
  handle-table words. Follow startup/intervening segment and state writers,
  lifetime and broader callers before admitting those bytes at the diagnostic.
  Tried: FND-EXE-509 traces startup's separate saved-segment, DS/SS
  and zero-fill producers. Follow native preservation, later startup/callers
  and record/table writers before admitting diagnostic state.
  Tried: FND-EXE-510 reads 0220's priority dispatch and seven shipped
  targets, including relocated far segments. Follow target bodies, SI/DI
  preservation, table writers and subsequent record/segment state admission.
  Tried: FND-EXE-511 reads target 0886's indexed initialization and
  conditional diagnostic-flag clear. Follow 0705's return, 364E's record
  writes and preservation, the earlier target and remaining startup/callers.
  Tried: FND-EXE-512 reads 0705/364E's return masking, signed mode
  guard and stores before allocation failure. Follow native preservation,
  302B/20A3/2172, earlier startup target and remaining state writers/callers.
  Tried: FND-EXE-513 reads 2172's DS-based stack-derived input,
  size arithmetic and retained head sentinel. Follow 21D2/2212/223B/2133,
  segment/link/block admission and earlier startup/caller effects.
  Tried: FND-EXE-514 reads 2133/223B and adjacent head writer 214F,
  resolving local BX preservation and ordered metadata/link writes. Follow
  21D2/2212, block/head producers, 214F callers and storage/segment admission.
  Tried: FND-EXE-515 reads 21D2/2212's requests and ordered header
  publication. Follow 0F68, storage/segment admission, shared-state writers
  and remaining initialization/startup callers.
  Tried: FND-EXE-516 reads 0F68's shared-offset update and stack-margin
  guards. Follow DS:009C initialization/writers, actual DS/SS and storage
  extent, caller stack depth and remaining initialization/startup effects.
  Tried: FND-EXE-517 records the shipped offset seed and 0F46/0F99
  setter path. Follow setter callers/inputs, other shared-word writers,
  actual segments, stack/storage extent and remaining startup effects.
  Tried: FND-EXE-518 reads 20A3's selected 20C0 setter path,
  including stores before rejection. Follow 20FA, candidate caller 22A2,
  other writers and block/segment/frame admission before cleanup closure.
  Tried: FND-EXE-519 reads 20FA's metadata merging, 214F call
  and 2133 fall-through. Follow remaining setter callers and writers,
  block/link producers, segments, extents and earlier startup effects.
  Tried: FND-EXE-526 admits setter lead 22A2 through 2289/22CB,
  including discarded failure and metadata writes. Follow 2254, outer
  callers/input producers, other writers and storage/segment admission.
  Tried: FND-EXE-527 reads 2254's zero-result return, metadata-derived
  word copy and cleanup ordering. Follow outer callers/input producers,
  storage/segments, other writers and earlier startup/launch coverage.
  Tried: FND-EXE-528 reads 302B and 2FCE, including signed adjustment,
  scan bounds and state-before-positioning stores. Follow actual record/buffer
  producers, native contracts, other callers/writers and earlier startup coverage.
  Tried: FND-EXE-529 reads the first-priority startup target and five segment
  relocations. Follow 029B/029A, 15A5, 0010/000F and 02AD, their cleanup,
  state/segment admission and earlier launch coverage.
  Tried: FND-EXE-530 reads 029B/029A and 07AD/07AC, including
  ES changes, metadata stores, fixed traversal and the 011A producer.
  Follow table/field and segment admission, other startup callees and consumers.
  Tried: FND-EXE-531 reads 15A5's request transform, segment-link sentinel,
  pair result and shared DS restoration. Follow 14C4/1528/1582/143B,
  their state/storage contracts and remaining startup dependencies.
  Tried: FND-EXE-546 reads 143B/1582, segment-selected link stores and
  returned AX/DX through 15A5. Follow 14C4/1528, record/link and shared-word
  producers, units/extents, aliases, other callers and startup dependencies.
  Tried: FND-EXE-547 reads 14C4/1528, differing padding-result handling
  and published header sources. Follow 1831 and its callees, shared-word/record
  producers, units/segments/extents and remaining startup dependencies.
  Tried: FND-EXE-548 reads 1831 and its arithmetic/comparison helpers,
  including signed bounds, normalized pairs and capture order. Follow 177C,
  shared-pair producers, actual storage and remaining startup dependencies.
  Tried: FND-EXE-549 reads 177C/298E and adjacent 17F2, including
  upper-bound changes on failure, native BX result and four-byte cleanup.
  Follow initial pair/cache producers, native contracts and remaining startup work.
  Tried: FND-EXE-550 reads shipped pair/cache zeros and startup's conditional
  upper-word replacement before dispatch. Follow native preservation, complete
  writer coverage, segment/storage admission and the 02AD failure target.
  Tried: FND-EXE-551 reads 02AD's selected write/cleanup chain and native
  termination request, including the no-return dependency. Follow native contracts,
  other 0388 callers/stored targets, actual frames/segments and startup dependencies.
  Tried: FND-EXE-490 counts every interrupt 21 instruction in the sound
  utility's load image and the stack-thunk wrapper's nine callers; none
  selects AH=4B, and the image has no interrupt 2E. The utility's direct
  references cannot launch a helper, so its pathname consumers no longer
  decide this question. FND-EXE-491 traces its indirect far targets:
  code outside the image is loaded driver code or a null timer slot, which
  FND-EXE-494 shows needs a DIGPAK image the build lacks. FND-EXE-492
  limits installable drivers to 19 disc .ADV files with no AH=4B request.
  FND-EXE-496 finds their decoded code transfers out only to host callbacks,
  which FND-EXE-497 shows the utility never registers, and to resident
  programs outside the build. FND-EXE-498 and FND-EXE-501 find that
  SBAWE32.ADV, in the utility's use, sends control only to code entries.
  The game editions' sound.bat consumers remain unread here.

- Q-EXE-008. FMT-EXE-006: Which disc helpers does the disc installer launch?
  Settles it: direct installer launch references and their selection inputs.
  Tried: helper contents and GOG wrapper (FND-EXE-008, FND-EXE-010), which
  do not cover the disc installer. Split from Q-EXE-004. Blocks: complete
  disc-installer caller coverage.

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
