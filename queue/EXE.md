# EXE

Next ID: Q-EXE-028

## Static

- Q-EXE-007. FMT-EXE-006: Do the shipped game or sound-setup executables
  launch any batch helpers? Settles it: for the installed DSUN.EXE (the
  disc copy is not studied, docs/SOURCE-EDITIONS.md), each of the 49
  computed transfers FND-EXE-588's walk leaves unresolved, and each return
  that jumps to an address pushed before it, other than the 50
  FND-EXE-575 and FND-EXE-578 to FND-EXE-588 already classify (the
  23 calls to the XMS entry, `4842:0498`, `4842:040A`, `4842:13FB`,
  `4842:14EA`, the far jumps `2707:0188`, `02FA`, `03D5`, `04D0` and
  `058C`, `1BF3:5D0E` (FND-PARTY-043), `4AB9:002B`, `4AB9:0052`, `1000:1F10`, `1000:1FBC`, `1000:2A24`, `4842:051A`,
  `44B6:00B8`, `55CE:018F`, `1000:1D8D`, `1000:252C`, `1000:029D`,
  `1000:298A`, `3D72:012A`, `09C7`, `0BD7`, `0DA1`, `0DD0`), classified by where its target
  comes from: an address inside the file, which is declared to the walk
  and the census rerun over what it then reaches, or code
  outside the file (handlers taken from the interrupt vector table, the
  XMS driver's entry, code the program loads from a file), with a control
  for each kind of origin. Each transfer that can leave the file is named
  with the code it reaches; only those are traced further, each as its own
  item.
  Tried: FND-EXE-578 censuses every interrupt 21, 2F and 2E site of the
  installed DSUN.EXE, its two stack-built interrupt routines and its one
  argument-selected request: none selects AH=4B and there is no interrupt
  2E; the walk from established entries reaches 80 of the 100 interrupt 21
  sites and leaves 83 transfers unresolved (FND-EXE-574, the same census
  from 17 starts, missed four installed handlers). FND-EXE-575 finds that the
  program never reads the interrupt 21 vector and that its calls through
  saved vectors go to the handlers of 5, 8, 9 and 19 installed before it
  started (two of the 77: `1000:2A24` and `4842:051A`); FND-EXE-576 gives
  each kind of site an earlier finding as its control. FND-EXE-579 traces
  the runtime library's Ctrl-Break, exit and critical-error calls to
  handlers inside the file; declared to the walk, they reach two more
  interrupt 21 sites (AH 2D and the argument-selected one), leaving 18
  candidates and 77 unresolved transfers. FND-EXE-580 traces the startup
  and exit record walkers, the formatting engine's output routine and the
  four signal dispatch calls to code inside the file; declared, they reach
  no further interrupt site and leave 69 unresolved. FND-EXE-581 finds
  twelve of them calling the XMS driver entry, with function numbers 00
  and 08 to 0D only. FND-EXE-582 traces the overlay manager's hooks to
  manager routines and empty returns, leaving 65 unresolved. FND-EXE-583
  traces eight guarded calls through DGROUP pointers to routines in
  2660, 49D2 and overlay 180, or to pointers nothing sets, leaving 59.
  FND-EXE-584 traces the nine calls through the record table at DGROUP
  3F42 to routines in 1425 and 15F3; they reach one more interrupt 21
  site (AH 3F, 17 candidates left) and nine more XMS calls. FND-EXE-585
  traces the timer callbacks and the sound driver dispatch, a far return
  the walk read as a return, to routines in the file or to driver code
  loaded from ADV resources (Q-EXE-027); declared, they reach six
  interrupt 66 sites and two calls into a DIGPAK image, leaving 61.
  FND-EXE-586 traces the script opcode dispatch, the text formatter, a
  block copy, an overlay 204 callback and five video far jump tables to
  routines in the file; declared, they reach no further interrupt site
  and leave 58, 14 of them unclassified (in segments 39D1, 3A8E and
  3D72). FND-EXE-587 traces the window hooks and window record callbacks
  in 39D1 and 3A8E, and three overlay 190 routines' callback arguments,
  to overlay routines and 28C9:0CFF that callers register as constants;
  declared, they reach no further interrupt site and leave 50, of which
  only the six in segment 3D72 are unclassified. FND-EXE-588 traces
  those: the application frame callback at 3D72:0590 goes to 20
  registered routines in the file, and the other five calls go through
  fields and a hook nothing sets, so they never run; declared, the walk
  reaches no further interrupt site and leaves 49, every one classified,
  and 4842:040A is still its only return directly after a push.
  Tried (before the split into this item, Q-EXE-025 and Q-EXE-026): the
  helper contents and distribution wrapper (FND-EXE-008, FND-EXE-010)
  locate no caller; FND-EXE-350 finds one sound.bat suffix in each game
  edition and an autoexec.bat suffix in the sound utility; FND-EXE-353 to
  FND-EXE-380, FND-EXE-391 to FND-EXE-433 and FND-EXE-471 to FND-EXE-489
  trace the utility's suffix consumers one callee further each time;
  FND-EXE-490 to FND-EXE-498 and FND-EXE-501 census the utility and its
  drivers (none selects AH=4B), retaken as Q-EXE-025, which FND-EXE-572
  closed (no reached site selects AH=4B), and Q-EXE-026, which
  FND-EXE-573 closed (no kind of entry missing from the drivers' decode);
  FND-EXE-499 to FND-EXE-559 trace the game editions' consumers the same
  way with no launch claim. More than two of these attempts ended in the
  same place, a consumer one step further on. Split from Q-EXE-004.
  Blocks: complete game/setup caller coverage.
- Q-EXE-027. FMT-EXE-006: Does the sound driver code the installed
  DSUN.EXE loads from its ADV resources, or the interrupt 66 handler,
  request program execution? FND-EXE-585 finds the timer callback at
  `4842:0498`, the driver dispatch at `4842:040A`, the calls into a
  DIGPAK image at `4842:13FB` and `4842:14EA`, and six `int 66`
  sites entering that code; the image is the ADV resource numbered by
  SOUND.CFG's word at +0x38 or +0x36, 11 and 8 in the installed file
  (FND-CONFIG-023). Settles it: the installed GFF resources of tag ADV
  with numbers 11 and 8 located and identified by their bytes, with which
  of the `Copy` and `DIGPAK` signatures each carries; for each, either
  its bytes matched to a disc driver FND-EXE-492, FND-EXE-496 and
  FND-EXE-573 already census, or the same census taken of it from its
  function table entries (function 0067 included) or its +0x100 and
  +0x103 entries; and the interrupt 66 handler named, from the code that
  installs it, or listed as installed outside the program.
  Blocks: Q-EXE-007's answer for the transfers that leave DSUN.EXE.


## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

- Q-EXE-024. FMT-EXE-002: What do `flags` values 0, 1 and 4 mean? Settles
  it: the linker's own description of the FBOV segment table it writes.
  Tried: FND-EXE-567 finds that the program reads no `flags` bit but bit 1.
  FND-EXE-571 settles `end_offset` and `start_offset` (once `unk_02` and
  `unk_06`) as each segment's end and start offsets, and finds code in
  the flags-1 segments and data in the flags-0 ones, which is
  circumstantial; the flags-4 descriptors hold no bytes.
  A 2026-10-10 web search for Borland's description of the overlay segment
  table found only a third-party layout naming the four words, with no
  meaning for the flags values. SRC-BCPP-40-DOSREF describes the overlay
  manager but not the executable's segment table.
  Split from Q-EXE-001, which FND-EXE-569 closed. Blocks: none.

## Blocked


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
