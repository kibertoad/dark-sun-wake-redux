# EXE

Next ID: Q-EXE-027

## Static

- Q-EXE-007. FMT-EXE-006: Do the shipped game or sound-setup executables
  launch any batch helpers? Settles it: for both game editions'
  executables, overlays included, a census of the program-execution
  mechanism as the standard's Findings section describes it: every
  interrupt 21 site with its AX value and every interrupt 2E site, the
  callers of any routine that builds an interrupt on the stack with the
  value each passes, sites reached from established entries that the
  finding names with the kinds of entry it did not have, a control for each
  kind of site, and each transfer out of the censused code. Only undecided
  sites and outside transfers are traced further, each as its own item.
  Tried (before the split into this item, Q-EXE-025 and Q-EXE-026): the
  helper contents and distribution wrapper (FND-EXE-008, FND-EXE-010)
  locate no caller; FND-EXE-350 finds one sound.bat suffix in each game
  edition and an autoexec.bat suffix in the sound utility; FND-EXE-353 to
  FND-EXE-380, FND-EXE-391 to FND-EXE-433 and FND-EXE-471 to FND-EXE-489
  trace the utility's suffix consumers one callee further each time;
  FND-EXE-490 to FND-EXE-498 and FND-EXE-501 census the utility and its
  drivers (none selects AH=4B), now retaken as Q-EXE-025 and Q-EXE-026;
  FND-EXE-499 to FND-EXE-559 trace the game editions' consumers the same
  way with no launch claim. More than two of these attempts ended in the
  same place, a consumer one step further on. Split from Q-EXE-004.
  Blocks: complete game/setup caller coverage.
- Q-EXE-025. FMT-EXE-006: Does any instruction start that the sound
  utility's code reaches from an established entry select DOS program
  execution? Settles it: FND-EXE-490's census of SOUND_DS.EXE taken again
  from established entries (the MZ entry point, the far-call targets the
  relocated code names, the startup and exit records, the handlers the code
  installs or passes as pointers, and jump tables read to their checked
  bounds), naming those entries and the kinds it did not have, with
  FND-EXE-370's AH=43 and AH=3D sites as the control for interrupt
  instructions and FND-EXE-353's selector 0021 call to 1000:20DF as the
  control for the stack-built interrupt. Tried: FND-EXE-490 took each
  interrupt 21 site's AH writer from the most common decode of the 48
  start offsets before it, which the standard now counts as candidate
  selection, and names no entries and no controls. Blocks: Q-EXE-007.
- Q-EXE-026. FMT-EXE-006: Which kinds of entry did FND-EXE-496's decode of
  the 19 disc drivers not have, and what control shows its interrupt sites
  are found? Settles it: a finding that names the kinds of entry the
  drivers' decode did not start from (vectors the drivers install, by
  interrupt 21 AH=25 or by a store to the vector table, transfers built on
  the stack, code written at run time) and whether any driver has one, and
  a control for its interrupt instructions located by other evidence.
  Tried: FND-EXE-496 decodes from each function table, the far-pointer
  handlers and SBAWE32.ADV's bounded tables, and lists its sites; it names
  no control and no missing kinds of entry. Blocks: Q-EXE-007.


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
