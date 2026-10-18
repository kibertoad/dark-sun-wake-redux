# COMBAT

Next ID: Q-COMBAT-025

## Static

- Q-COMBAT-002. FMT-COMBAT-001, FMT-COMBAT-002, SCR-COMBAT-001, RULE-AI-001: What do the unknown fields of
  the two combatant records hold, how many records does each table have, and how does
  `2D40:3E64` map `turn_combatant` to a record of each? Why does the panel show question marks
  only above 4? Settles it: a reading of `2D40:3E64` and of the code that fills the tables at
  the far pointers `57E0:19C9` and `57E0:19C5`. Tried: the panel routine and the party buttons
  (FND-COMBAT-022, FND-COMBAT-023). Blocks: slice 4.
- Q-COMBAT-003. RULE-COMBAT-008, SCR-UI-022: Which values do `combat_state` and the word at `57E0:0DAB` take,
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
- Q-COMBAT-005. RULE-COMBAT-002, RULE-COMBAT-003, RULE-COMBAT-006: Where does the game decide an
  attack: its kind, its roll against THAC0 and Armor Class, whether equality, 1 and 20 are
  special, the modifiers, the damage, and what a combatant's hit points do at 0 and -10? Settles
  it: a reading from the code that handles a click on an enemy in combat, or from the writers of
  `hit_points` in FMT-COMBAT-001. Tried: the constant -10 (FND-COMBAT-002), the literal `ATTACK`
  (FND-COMBAT-003), the condition names (FND-COMBAT-028), and the coordinate branch of the input
  loop (FND-COMBAT-012 to FND-COMBAT-017). Blocks: slice 4.
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
- Q-COMBAT-009. RULE-COMBAT-009: Which effect IDs can the effect rows hold when the status
  selector collects category zero, and can a stored ID exceed 112, the last index of
  `effect_names`? Settles it: for every admitted insertion caller, the effect byte it passes and
  where that byte comes from (shipped `DATA` effect bytes, the handler default for code 127), and
  whether any route stores an ID above 112. Tried: FND-COMBAT-029 reads the collector's category
  filter and row order, FND-COMBAT-031 the common insertion's field writes, and FND-COMBAT-046
  the shipped `DATA` effect-byte domain, its one code 127 and that code's handler-default route;
  which IDs the admitted callers store remains. Blocks: established status-effect selection.
- Q-COMBAT-017. RULE-COMBAT-009: Can the effect-row count leave 0 to 192? Settles it: the bound
  each count writer keeps: the checked insertion wrapper's exact-192 test against the exported
  insertion entry that skips it, the removal decrements, the owner transfer's decrement and
  increment pair, and the numbered restore that writes the count whole. Tried: split from
  Q-COMBAT-009. FND-COMBAT-031 (the wrapper's test and the unchecked entry), FND-COMBAT-032 (the
  removal decrements), FND-COMBAT-033 (the transfer pair) and FND-COMBAT-045 (resident backing of
  the boundary row for counts 0 to 192) read each writer locally; no bound over all of them
  follows. Blocks: established status-effect selection.
- Q-COMBAT-018. RULE-COMBAT-009: What does loading a saved game put in the effect rows and their
  count? Settles it: which archive and numbered resources the restore reads for `SAVE/24` and
  `SAVE/26` on each of its two callers, the destination extent against the shipped lengths, and
  any check or postprocessing of the restored count, IDs and order. Tried: split from
  Q-COMBAT-009. FND-COMBAT-034 (the save sources and shipped lengths), FND-COMBAT-035 (the
  complete-resource reads, rows before count), FND-COMBAT-043 and FND-COMBAT-044 (both restore
  callers' continuations, neither of which checks the rows) answer the local routes; the archive
  selection and the restored state's validity remain. Blocks: established status-effect
  selection.
- Q-COMBAT-019. RULE-COMBAT-009: Which effect rows and count does a new game begin with?
  Settles it: whether a script on the new-game route places object 147 (through a placement
  whose object number comes from a variable, such as `GPL/27` at 491 with global 2, or through a
  script `MAS/50` registers with instruction 0x1B), whether the party's own objects include it,
  and whether a window handler, window record or frame callback registered between START GAME
  and the gate routine's return (Q-UI-020), or a script instruction, reaches an insertion or the
  index removal +18E3. Tried: split from Q-COMBAT-009. FND-COMBAT-031 notes the
  count's zero word in the load image; no initializer has been read. Tried: FND-COMBAT-053 finds
  no write of the count or rows from program start to the party load, no constant store to the
  count, the numbered restore unreached on the new-game route, and every route to the effect
  writers passing the gate routine's calls from +1300 on; what holds at program start is removed
  from Settles it. Tried: FND-COMBAT-054 finds the removal and transfer entries inert at count 0
  and every route from those calls to an insertion passing overlay 193 +12A8, which runs only for
  a slot made from object 147 (placed only by `RGN03B.GFF#ETAB/59`) or one +12A8 itself marked;
  which of the gate routine's calls change the rows is removed from Settles it, and what an
  earlier game leaves moves to Q-COMBAT-024. Tried: FND-UI-050 finds that a new game loads
  region 50, whose `ETAB/50` does not place object 147, and that no literal placement on
  `MAS/50`'s route names it; the region is removed from Settles it. Tried: FND-EXE-592 resolves
  every transfer the walks from the gate routine's later calls left; with them the effect walk
  reaches the insertion through the script dispatch, the window hook and the frame callback, by
  no routine common to every route, and FND-UI-051 finds the region word changed on that span only
  by script opcode 0x5E with a first operand of 32766 or a saved-game load. The unresolved
  transfers and the region stores are removed from Settles it. Blocks: established status-effect
  selection.
- Q-COMBAT-024. RULE-COMBAT-009: Which effect rows does a game begun after an earlier one in the
  same run start with? Settles it: whether the defeat cleanup's unload reaches the owner removal
  at 31E0:0AC9 with `DS:265B` nonzero, which owners' rows the earlier game's other exits (the
  Game Menu's new game, a load) remove, and what removes the rows of owners that are not party
  slots. Tried: split from Q-COMBAT-019. FND-COMBAT-053 finds that no writer sets the count to a
  constant, so a later game keeps the earlier game's rows apart from those the removal routes
  take out, and reads the owner removal +1DB0 and its call from the object unload;
  FND-COMBAT-054 finds the removal entries inert only while the count is 0. Blocks: established
  status-effect selection.
- Q-COMBAT-020. RULE-COMBAT-009: Which routines insert, remove and transfer effect rows, and with
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

## Emulated call

None.

## Agent run

None.

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
- Q-COMBAT-007. RULE-COMBAT-007, RULE-CONFIG-002: Does the difficulty scale a hostile creature's hit points when
  it appears, by how much on each setting and with what rounding, and does it change anything
  else? Settles it: from equivalent saves, the same hostile's hit points looked at on each
  setting, with the setting changed before and after the creature appears. Tried: none; no code
  reads `difficulty` (RULE-CONFIG-002). Blocks: slice 4.

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

None.
