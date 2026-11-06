# PARTY

Next ID: Q-PARTY-134

## Static

- Q-PARTY-076. BUG-PARTY-001: Can anything other than overlay 184 `+0DA7` run while the
  generation screen is up and point `DS:1429` at a stored details record before the screen draws
  its sheet? Settles it: the 34 unresolved calls of FND-PARTY-189's reach, the main loop's calls
  through `DS:A0F5` and `DS:A0F9` on routes into generation other than `NEW` and `EDIT` from View
  Character, and stores to `DS:1429` other than direct-address ones. Tried: FND-PARTY-187 runs
  View Character in play and finds codes 1 to 8; FND-PARTY-188 lists the sheet's three callers
  and the direct-address writers of `DS:1429`; FND-PARTY-189 shows generation's own handler
  reaches those writers only in `+1029`, after its sheet draw and as it closes the screen;
  FND-PARTY-190 and FND-PARTY-191 open generation by `NEW` (from Create Characters and from
  play) and by `EDIT` and find no other handler and Fighter's own threshold on the sheet.
  Blocks: nothing yet.
- Q-PARTY-077. BUG-PARTY-001: On the DUAL path, does the word at `4E71:0B44` name the slot
  overlay 209 `+123E` was called with when it calls overlay 184 `+1DCE` at `+14FF`, so that the
  sheet at `+151E` prints that character's own classes? Settles it: the routes from overlay 172
  `+05DB` through overlays 173, 188 and 210 to the overlay 209 routines that store `4E71:0B44`,
  including 371D:050F zero-count unload target admission and free-list/slot
  lifetime after FND-PARTY-262/263 withdraw the earlier exclusion,
  given `DS:0DAB` 0 (FND-PARTY-191 reads 0 on the Create Characters route; whether the in-play
  route can offer `DUAL` with another value), and what those routines store (FND-PARTY-080).
  Tried: FND-PARTY-080 reads the calls before `+1DCE` on this path and FND-PARTY-188 finds the
  sheet call at `+151E` right after it; FND-PARTY-191 runs `DUAL` for a character made just
  before and finds the word naming that character's slot and the Fighter threshold on the sheet.
  Blocks: nothing yet.
- Q-PARTY-073. RULE-PARTY-001: Can View Character, opened from Create Characters before START
  GAME, pass an event to overlay 211 `+081B` that runs the damage routine overlay 173 `+22C4` on a
  party member, giving it `combat_mark` 3, 7 or 8, or start a fight through overlay 193 `+0533`?
  Settles it: the conditions along overlay 211 `+0901`, overlay 193 `+2290`, `+0000` and `+003C`
  to its target loop, how that loop's list is filled on Create Characters (whether it can name a
  combatant above 4), and overlay 179 `+0D2F` to its near calls of `+2A05` at `+1078` and `+1373`
  at `+1134`, and those routines to their calls of `+22C4`; or a run that casts each of
  a stored character's damaging spells at a party member from the USE window on Create
  Characters. Tried: FND-PARTY-183, FND-PARTY-184,
  FND-PARTY-192 and FND-PARTY-193 show ADD and NEW give mark 1; FND-PARTY-197 takes a census of
  the stores to combatant byte `+0x14`, finds `+22C4` the only one through `DS:19C9` writing a
  mark above 2, and a reach from the title screen's windows to it through this route;
  FND-PARTY-198 opens the USE window on Create Characters with the key `c`; FND-PARTY-199 and
  FND-PARTY-200 show `set_leader`'s route to the attack routine needs a fight, and that the only
  store starting one that a walk reaches outside that route is overlay 193 `+0533`, on this one;
  FND-PARTY-201 shows `+0533` needs a target numbered above 4, while every target reaches
  `179 +0D2F`; FND-PARTY-202 shows `+0D2F`'s own damage call is for kind-1 slots only, so a party
  member is damaged only through `+1373` or `+2A05`. Blocks: nothing yet.
- Q-PARTY-080. RULE-PARTY-001: Does any other write give a filled slot's combatant record a
  `combat_mark` of 3 or more, or hit points below 1, before START GAME? Settles it: the pointer
  sources of overlay 172 `+01AE`, 179 `+2515`, `+25AD` and `+2668`, 200 `+04DD`, resident
  `31E0:2350` and the resident word stores at `+0x14` FND-PARTY-197 lists; writes to `+0x14` not
  of its search's form (DS or SS segment, 16-bit displacement, string stores, block copies); the
  stores to word `+0x00`; and the value overlay 187 `+018F` writes to a type-1 chunk's byte
  `0x14` (FND-PARTY-054); and the routines reached only through registered handlers or the 66
  unresolved transfers of FND-PARTY-197's walk. Tried: FND-PARTY-197 classifies the 42 candidates
  of its search.
  Blocks: nothing yet.
- Q-PARTY-078. RULE-PARTY-001: After the defeat window closes, does the cleanup loop at
  `277B:0330` empty every slot of a party of several, deleting each member's chunks? Settles it:
  for each placed party object, whether the unload `31E0:05BB` with index 0 reaches the chunk
  deletion `2D40:2410` (the gate on `DS:265B`, the slot it passes, and the slot's kind byte and
  9999 test), and what the deletion dispatch `2D40:248D` does for each chunk type a character's
  graph holds, so that every member's `character_id` is cleared before the count reaches zero.
  The deletion map's construction is Q-PARTY-094, its DS/SS identity Q-PARTY-081, and the route
  into the loop Q-PARTY-095. Tried: FND-PARTY-194 reads the defeat window, FND-PARTY-212 runs it
  with one member and sees that member's `character_id` cleared, and FND-PARTY-213 reads the loop
  (unload of entry zero while the count is above zero, decrement and compaction before the chunk
  deletion) and the type-2 and type-3 deletion branches; the gates for a member other than the
  first and the other chunk types remain. Blocks: nothing yet.
- Q-PARTY-094. RULE-PARTY-001: What does the deletion map built at `2D40:27B7` hold when defeat
  cleanup deletes a member's chunks, and what do its field lookups change? Settles it: the
  current-object, parent-index and count inputs the builder reads, the bound of its 160-byte map
  against the chunk count, and the field accessor and conversion callees it calls, with their
  failure results, which it does not test. Tried: split from Q-PARTY-078. FND-PARTY-214 reads the
  builder's stores of parent-derived objects for type-2 steps, its use of AX alone from the field
  result, and the compactor's near helper; the inputs and callees remain. Blocks: nothing yet.
- Q-PARTY-095. RULE-PARTY-001: How does START GAME's call of overlay 173 `+3CDA` with no member
  standing lead to the cleanup loop at `277B:0330`? Settles it: the caller of `+3CDA` on the
  START GAME route, what it does after `+3CDA` returns with `DS:13FB` at 1 and no leader, and the
  path from there to `277B:0330` and back to the start window. Tried: split from Q-PARTY-078.
  FND-PARTY-194 reads `+0410` returning to `+3CDA` and leaves its callers unread; FND-PARTY-212
  sees the start window return after the defeat window; FND-PARTY-213 finds the loop runs while
  `DS:13FB` is nonzero. Blocks: nothing yet.
- Q-PARTY-045. RULE-PARTY-015: Does any enemy's `kill_experience` divided by the filled party
  slots reach 32,768, so that killing it lowers each member's experience (FND-PARTY-086,
  FND-PARTY-094)? Settles it: what moves objects 430 and 541 out of the region `Limbo`, where
  their only `ETAB` records are, read to show whether the party can fight them. Blocks: nothing
  yet. Tried: FND-PARTY-094 finds `RDFF` 430 (107,000) and 541 (33,000), of kind 7, with CHAR
  chunk headers; FND-PARTY-260 shows `RDFF` resources load through the CHAR chunk walk and finds
  both objects only in `RGN0FF.GFF`. Scripts that move objects are the SCRIPT area's, outside
  this goal's claim.
- Q-PARTY-062. BUG-PARTY-006: Does anything other than a block copy, or a copy FND-PARTY-115's
  searches do not reach, put a value other than 0 in bytes `+0x0E` or `+0x0F` of a party member's
  FMT-COMBAT-001 record? Settles it: the reads of a saved game into the table at `DS:19C9` and the
  writes that produce them; the routines that reload the table pointer from `DS:142D`, `DS:6157`,
  `DS:6177` or `DS:619B` or from a local and pass it on, followed past branches and more than one
  call deep; and the indirect calls that take it. Tried: FND-PARTY-115 reads the block copies, the
  straight-line pushes and stores of the table segment and one level of calls below them. Blocks:
  nothing yet.
- Q-PARTY-003. FMT-PARTY-001, RULE-PARTY-004, RULE-MAGIC-002: What do
  the remaining unknown character fields and combatant identifier mean?
  Settles it: meanings of CHAR bytes 0x1C, 0x21, 0x2A, 0x7B and 0x86, and the
  combatant_id/CACT relationship, after the field placements below.
  For byte 0x2A, decode the shipped script calls of opcode 0x22 functions
  48/49 with field 33 using FND-SCRIPT-024's layouts and
  FND-PARTY-156's actual target/field dispatch. For the other bytes,
  distinguish retained opaque values from an interpretation established
  by a reached consumer; FND-PARTY-152's fixed-displacement exclusions
  and FND-PARTY-159's shipped-save-selector bound are not universal
  absence proofs. Complete access/serialization coverage is Q-PARTY-126,
  separate from assigning these remaining meanings.
  Tried: the FMT-PARTY-001 assessment reuses FND-PARTY-052/054/148/152/
  156/159/235/259 and existing field findings below. Their recorded positions
  are not still questions about which sheet label belongs at which byte.
  The remaining semantic work is named above; writer/access exclusions
  that affect the record's universal-prefix claim move to Q-PARTY-126.
  Tried: a search of the first 79 bytes of records 40 and 42
  for the label positions of their gender, origin, alignment and class (FND-PARTY-061);
  FND-PARTY-063 and FND-PARTY-066 show the four codes of Cleric, Druid and Ranger are the spheres; FND-PARTY-049, FND-PARTY-259 and FND-PARTY-055 to
  FND-PARTY-057 place hit points, the object offset, the combatant identifier, the control flags,
  greatest hit points, origin, gender, alignment, class codes with their names, and levels, and
  FND-PARTY-058 and FND-PARTY-059 place experience at `0x45` and the current and greatest
  psionic points at `0x0C` and `0x51`. FND-PARTY-256 shows `0x5A` holds the base scores, and
  with FND-PARTY-071 that the scores are stored with the origin modifiers added. FND-PARTY-143 shows `0x1F` is the
  combatant kind, FND-PARTY-144 that `0x29` is the facing and FND-PARTY-145 that `0x66` is the base
  armor class; FND-PARTY-146 shows `0x1D` and `0x67` are the current and base movement rate, FND-PARTY-147 that `0x53` is the details record's key, and FND-PARTY-148 that `0x04`, `0x06` and
  `0x0E` hold the slot the character was written from and `0x12` to `0x16` the child words at that time; FND-PARTY-149 shows `0x81` and `0x83` are sound numbers, and FND-PARTY-150 that `0x68` is the magic resistance; FND-PARTY-151 places the footprint at `0x7C` and a spell list at `0x7D`. FND-PARTY-152 finds no reader through the combatant table of
  `0x1C` or `0x21`, and `0x2A` only as field 33 of overlay 204's field getter and setter, which FND-PARTY-156 shows are
  functions 48 and 49 of script opcode `0x22`, so what `0x2A` means waits on the decoded calls, whose layouts FND-SCRIPT-024 gives; FND-PARTY-157 names `0x18` the special attack kind
  and `0x19` the immunity type, and FND-PARTY-158 names `0x85` the psionic power list; FND-PARTY-159 shows the saving throw never
  reads `0x7B`. Blocks:
  slice 2.
- Q-PARTY-020. FMT-PARTY-006: What does the word at `0x10` of a 23-byte attached record, kind-1
  field 8, hold?
  Settles it: FND-PARTY-257 (overlay 173) and the answer of Q-PARTY-113 put together with the local readings
  already recorded into one description of field 8 in FMT-PARTY-006, by item type where its
  meaning depends on the type, with any type-dependent meaning still open in its Open questions.
  Split on 2026-10-21 into this item and the overlay 173 and overlay 213 readers, after eleven
  attempts that each removed sites from Settles it.
  Tried: FND-PARTY-196 gives the shipped values; FND-PARTY-216 and FND-PARTY-248 bind the field
  descriptors; FND-PARTY-245 reads the initializer's 0; FND-PARTY-248 lists the 38 accesses at
  `0x10` and reads overlay 190 `+365A`; FND-PARTY-249 reads the overlay 197 stores; FND-PARTY-250
  and FND-PARTY-251 every overlay 182 site; FND-PARTY-252 overlay 178 `+0CAC`; FND-PARTY-253
  `25AF:02D9` and `28C9:26EE`; FND-PARTY-254 overlay 189; FND-PARTY-255 overlay 204; and
  FND-PARTY-256 the remaining overlay 197 read. Their downstream meanings are Q-PARTY-106 to
  Q-PARTY-111. Blocks: slice 2.
- Q-PARTY-114. FND-PARTY-257: Can `4C4F:0007` hold a value that overlay 173 `+2A9A` stored on a
  path that did not reach the swing `+2560`'s cast step, so that a later damaging swing casts an
  effect its weapon does not carry? Settles it: for each caller of `+2A9A` (overlay 173 `+0444`,
  `+1CC1`, `+2560` and resident `28C9:254C`), whether an ammunition entry is found and nothing
  after it reaches `+29CB` before another swing reads `4C4F:0007`, with a search for its accesses
  that does not depend on the segment load being near. Tried: FND-PARTY-257 finds one writer of a
  nonzero value and one clearing store, by a near-context operand search. Blocks: Q-PARTY-020.
- Q-PARTY-113. FMT-PARTY-006: What do overlay 213's reads of word `0x10` at `+010A`, `+019C`,
  `+0287`, `+04EA`, `+0529`, `+0754`, `+0766` and `+154C` test or pass on? Settles it: a reading of
  each site's routine from the read through every branch the value decides and every callee or
  store it reaches, with the routine's callers where they decide which record is read. Tried:
  FND-PARTY-248's census lists the eight reads and reads only overlay 213's name formatting at
  `+0F8F..+1005`, which tests byte `0x16`. Blocks: Q-PARTY-020.
- Q-PARTY-111. FMT-PARTY-006, FMT-PARTY-001: Which admitted inputs and
  child effects establish the item ability/movement rebuild's complete
  contract in overlay 197 `+1E7B`? Settles it: resolver/index admission,
  effect-list/table construction and all effect cases, graph/record and
  DS/SS storage identity, item payload/value domains and arithmetic-edge
  reachability, flag sources and preservation across children. Graph and
  field-16 naming remain Q-PARTY-099 and Q-PARTY-082. Tried:
  FND-PARTY-256 corrects the earlier 24 bound and settles local item
  cases and serialization order; complete admission remains open.
  Blocks: complete interpretation of FMT-PARTY-006 and FMT-PARTY-001.
- Q-PARTY-110. FMT-PARTY-006: Which admitted script-created objects and
  child effects give gameplay meaning to overlay 204 `+1322`'s packed
  word-0x10 fields? Settles it: caller argument domains, record and
  DS/SS local-table identity, target-buffer population/capacity, guard
  child meanings, the per-target requests and replacement-slot cleanup.
  Tried: FND-PARTY-255 settles the local fields, guard and request order,
  while retaining the explicit inventory gap. Blocks: complete
  interpretation of FMT-PARTY-006.
- Q-PARTY-109. FMT-PARTY-006: Which admitted item actions reach overlay
  189 `+326A` and `+332B`, and what effects do their child requests
  produce? Settles it: caller owner/index/value domains and record
  preservation, actual effect-four membership, reset rereads and special
  rebuild/other dispatch-child effects, including high source-word admission.
  DATA coherence is Q-PARTY-117, placed-row publication/lifetime is
  Q-PARTY-116 and flag lifetime is Q-PARTY-118; do not repeat those as
  unnamed dependencies here. Tried: FND-PARTY-254 settles the complete local gates,
  call arguments/order and unreachable post-FFFF call without assigning
  a native effect outcome.
  Tried: FND-ITEM-056 removes canonical high signed-selector and repeated-
  DATA alternatives: all 1931 source words are 0..327 and bonuses nonnegative;
  1671/240/20 payloads occupy the zero/ordinary/special branches. Actual
  selected values after child calls, record/flag lifetime and child effects
  remain. Blocks: complete interpretation of FMT-PARTY-006.
  Tried: FND-ITEM-057 removes canonical DATA absence/property ambiguity:
  all ordinary IDs have 73-byte source records; 93 of 240 ordinary item
  payloads select property one. The complete getter distinguishes cache
  hits, accepted misses and FFFF failures without local cache invalidation.
  Actual key/content coherence, reader result, owner/record/flag retention
  and reset/dispatch effects remain.
  Tried: FND-ITEM-058 removes the opaque status-child/register alternative:
  the closed local graph preserves SI/DI and has no persistent data stores.
  Its admitted result is counted effect-four membership; boundary-word
  storage is required, but its value is irrelevant at index N. Actual
  owner/rows/count/frames, membership lifetime and other child effects remain.
  Tried: FND-ITEM-059 removes reset's opaque register/write alternative:
  the closed body preserves SI/DI and copies a live word only to quantity or
  word +0E. Reset precedes publication/dispatch, with no local rollback or
  saved flag restoration. Actual reread/type/frame/record admission, child
  effects and independent flag writers/consumers remain; source reset-
  destination frequencies require a per-item join absent from the marginals.
  Tried: FND-ITEM-060 removes that source-pairing obligation: the 93 ordinary
  property-one candidates select 92 quantity destinations and one word +0E;
  all twenty special candidates select quantity. Of these 113 destinations,
  81 initially equal W and 32 differ. Actual invocation, child preservation,
  reset rereads and record/type/frame/flag admission remain.
  Tried: FND-ITEM-061 removes the dispatcher's child-family and level choice:
  the explicit byte one forces 193 +0000 at level six for all retained
  canonical candidates, including fifteen psionic-range payloads. Its local
  store-free prefix carries the flag to that entry; downstream child effects,
  use-time flag preservation and native input/frame admission remain.
  Tried: FND-ITEM-062 removes the selected main-cast target and return
  ambiguities: it begins with one owner target, copies DATA locally and
  requests two sounds before the hit. One canonical candidate selects the
  separate post-hit area gate. The return is a continuation counter, and
  the wrapper drains queued effects before the equipment flag clear.
  Actual child/queue effects, cache/snapshot/list/frame/output preservation
  and use-time flag values remain.
  Tried: FND-PARTY-262 corrects the area child's zero-argument exclusion:
  zero removals still request unloading its target slot. Actual target-list,
  object/entry/table lifetime, redraw/deferred work and child effects remain.
  Tried: FND-ITEM-063 removes the canonical area-list family and count
  alternatives: D=42 selects an effect-object scan capped at one before
  filtering. Selection does not test the removal row's flag, so positive
  removal count is not proved. Actual range/coordinate setup, predicate,
  row/slot/frame preservation and child effects remain; the range helper's
  FF-derived row-minus-one read needs separate memory admission.
  Tried: FND-ITEM-064 removes the canonical D=42 final predicate and
  compaction alternatives: it takes the complete table's default accept
  path with no children or persistent stores and preserves the capped
  zero-or-one list. Prefix metadata reads, actual range/scanner/row flags,
  removal children, frame/list storage and flag admission remain.
  Tried: FND-ITEM-065 resolves one flag producer and its publication order:
  31E0:03CA prepares bit 20, increments the persistent count before its
  insertion child and supplies a separate local count copy. Actual counted
  insertion index, spare row/order/capacity, other producers and retained
  flagged-row identity remain; a prepared flag is not a publication proof.
  Tried: FND-ITEM-066 removes insufficient nominal capacity on the admitted
  ordinary allocation: plus-one and paragraph rounding cover the extra
  insertion row before the marker. Extra-row contents/order, actual
  header/lifetime, counted publication and row preservation remain; rounded
  capacity is not initialized data or a native heap admission.
  Tried: FND-ITEM-067 supplies one strict paused endpoint: 873 ordered
  counted rows and an all-zero extra row, which breaks extended ordering;
  header/marker and exact roundtrip checks pass. Constructor/use-time state,
  actual inserted key/K, later writers, selection and removal remain; no
  constructor was executed and endpoint equality is not continuous lifetime.
  Tried: FND-ITEM-068 removes the sample caller's local key/result-binding
  alternatives: byte four and low flag inputs are retained zeros, coordinates
  are wrapping point translations behind named cell/geometry gates, and
  exact-not-FFFF acceptance adds no counted-index check before immediate
  consumers. Actual slot/sample/key/K, child preservation and lifetime remain.
- Q-PARTY-116. FMT-PARTY-006: Does an admitted effect-row constructor
  publish a counted flagged row that survives until the equipment area
  removal uses its slot? Settles it: bind the admitted constructor's slot,
  coordinates, extra row, count N and table/frame identity to the returned
  K, then show K<N and the flagged slot row remains in the counted prefix
  at 371D:050F, or give an admitted counterexample. FND-ITEM-065 supplies
  the conditional K<N requirement; FND-ITEM-093 removes the complete
  count-zero/retained-zero-key search domain, not its native admission.
  Account for entry-index/slot/neighbor identity, intervening row flag,
  count, key and table-base writes through direct stores, indexed stores,
  whole-row copies/restores and reachable child calls. Bind the selected
  scanner's coordinate/range/row-minus-one access and positive flagged
  match to valid cell pointers and spans beyond FND-ITEM-063/064/073/076.
  Entry-state production is Q-PARTY-121, immediate-handler preservation is
  Q-PARTY-120, unload/descriptor/refresh preservation is Q-PARTY-122,
  deferred consumption is Q-PARTY-123 and bitmap/backing coherence is
  Q-PARTY-119. Their answers are dependencies, not assumed guarantees.
  Q-PARTY-077 retains the distinct DUAL contract, Q-PARTY-117 DATA
  coherence and Q-PARTY-118 equipment-flag lifetime. Do not infer native
  publication from a local nonnegative return or a paused endpoint.
  Tried: reviewed existing findings and split input production,
  immediate handling, cleanup and deferred dispatch into Q-PARTY-121,
  Q-PARTY-120, Q-PARTY-122 and Q-PARTY-123 respectively. FND-ITEM-065
  establishes only conditional counted publication; FND-ITEM-093 settles
  local count-zero/zero-key cases and FND-ITEM-073 removal/compaction,
  while FND-ITEM-063/064 settle selection/filter gates. Actual constructor
  admission and use-time identity remain. FND-ITEM-067 is a paused endpoint,
  not a constructor call; FND-PARTY-262/263's withdrawn exclusion is not
  restored. Earlier detailed attempts remain in Q-PARTY-116 at revision
  3d9b7ea4 and in the cited findings. New parts each retain their relevant
  evidence and exclusions below; no dependency was answered by the split
  and no complete reading is declared.
  Blocks: complete interpretation of FMT-PARTY-006.
- Q-PARTY-120. FMT-PARTY-006: Does the immediate effect handler preserve
  the slot, placed-row/count and source state needed by the subsequent
  sampled effect constructor on the admitted creation route?
  Settles it: bind actual effect-row/hidden-position/coordinate inputs to
  overlay 193 +0AAD, including the plus-one byte decode and DATA/73's
  signed-index exception; establish actual rectangle/class/owner inputs,
  accepted target population and the caller's 64-record frame/span and
  DS/SS identity. Identify writes to that list/frame and distance scratch
  4C49, and the owner/effect table 50B0:000F/count DS:2102, through indexed
  stores, copies and reachable calls. Bind +12A8 mode-zero routing to
  actual +1119/+1249/+13EC children and their slot/row/count effects,
  including failed/rejected paths and lookup state retained across casts.
  FND-ITEM-099/100/101 settle local decode, list/insertion and distance
  behavior; FND-COMBAT-034/037 settle local lookup/append and
  FND-MAGIC-019/FND-RNG-094 routing and defined-index round-trip limits.
  Do not repeat those bodies or treat local AL one as preservation.
  DATA acquisition coherence remains Q-PARTY-117; ordinary entry-state
  production is Q-PARTY-121 and final publication/lifetime Q-PARTY-116.
  Tried: split from Q-PARTY-116 using FND-ITEM-098/099/100/101,
  FND-COMBAT-034/037 and FND-MAGIC-019/FND-RNG-094. Their existing local
  results do not admit native counts/aliases or settle the casting children.
  Earlier attempts remain in Q-PARTY-116 at revision 3d9b7ea4; no answer is discarded.
  Blocks: Q-PARTY-116 constructor-entry preservation.
- Q-PARTY-121. FMT-PARTY-006: Which placement count, extra-row/key and
  slot-index state can actually reach the sampled effect constructor?
  Settles it: bind active archive identity and exact ETAB transfer to
  31E0:0025 recount, including later indexed key/flag/count writers,
  whole-table restores and other creation routes; bind ordinary 31E0:0121
  lower-load mode-one inputs and reached children to retained index,
  count and valid table/presentation/stack spans. FND-ITEM-103 supplies
  normal-return DF and the conditional fixed-copy/index handoff; actual
  OJFF provider/exact-transfer identity, retained selector/frame, first-allocation result
  and source/index/count identity across resource/refresh calls remain,
  with retry/eviction on other outcomes. FND-COMBAT-149/161/162 and FND-ITEM-082/084 exclude RDFF,
  descriptor acquisition and the later lazy-animation branch only for the
  intact mode-one, canonical low-selector, first-success domain described
  in FMT-PARTY-006. Refresh/cache and linked-slot preservation remain
  Q-PARTY-122/119; do not reread the excluded branches for that domain.
  Admit the coordinate helper's stable
  disjoint 1..1500-row domain, slot-kind and separate far-argument backing
  after lower loading; FND-ITEM-102 settles its local search/step/DF and
  finite termination conditions, not the actual post-load count. Admit
  the retained initializer index across resource loading and refresh
  after FND-ITEM-103's first-success copy proof, and bind the slot/source/
  position/lifetime inputs of FND-ITEM-098's producer to the sampled loop.
  Establish actual constructor count/key/extra-row state after its
  immediate/cell/queue calls, using Q-PARTY-120/123 for their effects.
  FND-ITEM-094/095 settle local recount and exact shipped-source count
  composition, FND-ITEM-096/097 ordinary local/count-publication branches,
  and FND-ITEM-098 producer ordering. Source/count/extra-row and storage
  admission outside FND-ITEM-103's normal-return DF proof remain open.
  Q-PARTY-116 retains
  the constructor's K<N and subsequent lifetime question.
  Tried: FND-ITEM-103 resolves first-success pop/copy/index handoff
  and DF at normal loader return. The paired word and presentation index
  are distinct; both fixed odd-length copies include their tails. Final
  fill supplies clear DF after earlier returning children, and the ordinary
  coordinate/anchor path preserves it to insertion. Actual heads, valid
  replacement/source/frame backing, e/count identity and refresh effects
  remain; neither a first-success assumption nor an older snapshot admits
  this native call. Retry/eviction outcomes stay in scope.
  Tried: compose FND-COMBAT-149/161/162 with FND-ITEM-082/084 and
  FND-ITEM-098's mode-one caller. Unchanged mode one skips RDFF and
  descriptor acquisition but reaches bitmap refresh. Retained canonical
  low selectors still produce a signed-negative image word after refresh,
  excluding the parent's later lazy-animation branch; first valid
  allocation at least five excludes retry/eviction and below-four chain
  insertion in that continuation. Refresh failure does not cancel the
  image store. Provider/transfer, actual allocation/copies/aliases and
  refresh/cache preservation remain above; no native state is admitted.
  Tried: FND-ITEM-102 composes FND-EXE-622/FND-ITEM-096/098 and
  resolves the full coordinate child's local footprint and bounded
  termination. Separate far SS arguments need no DS=SS equality; no child
  call or DF write exists. Stable 1..1500-row backing/aliases and actual
  slot-kind admission remain; zero count has no early guard. Selected
  unchanged mode one skips ordinary placement's later mask-04 services,
  so those paths are not another dependency on that route. Lower loading
  and the producer's later immediate/cell/queue calls remain.
  Tried: split from Q-PARTY-116 using FND-ITEM-093/094/095/096/097/098.
  Source-positive counts and conditional ordinary publication do not
  establish constructor-entry state after unchecked children. Preserve
  the earlier attempts in Q-PARTY-116 at revision 3d9b7ea4 and all remaining producer paths.
  Blocks: Q-PARTY-116 native constructor input admission.
- Q-PARTY-122. FMT-PARTY-006: What slot, row and neighbor state survives
  the unload/common-cleanup/descriptor-refresh paths reached by removal?
  Settles it: bind 371D:050F entry-index/flag/count gates and its later
  linked-row traversal to admitted metadata/frame/span state and a
  termination argument; resolve graphics, movement and script children.
  For 31E0:044C and descriptor acquisition/decrement, identify actual
  object/header/descriptor/resource-length/count inputs and direct,
  indexed, copy/restore and indirect-acquisition producers. Bind eviction
  handle/count/link/object state and image-refresh link termination,
  owner/slot/frame/buffer identity; resolve event/heap/chunk/free-list
  and graphics effects, including changes retained on failure.
  FND-ITEM-078/079/080 settle local unload/cleanup ordering;
  FND-ITEM-081/082/083 settle local acquisition/caller/decrement/eviction
  contracts; FND-ITEM-084/085 settle local refresh/pointer normalization.
  Q-PARTY-119 owns cache/backing content and buffer lifetime. Prove the
  remaining slot/row/count/neighbor preservation at the removal return,
  rather than equating null/fill/AL returns with successful cleanup.
  Tried: split from Q-PARTY-116 using FND-ITEM-078 through FND-ITEM-085.
  Local bodies and known direct caller bindings are already recorded;
  native identities, unresolved indirect sites and named child effects
  remain. Earlier attempts remain in Q-PARTY-116 at revision 3d9b7ea4, with no answer deleted.
  Blocks: Q-PARTY-116 use-time row/slot/neighbor preservation.
- Q-PARTY-123. FMT-PARTY-006: Does a kind-three deferred request preserve
  its intended slot and queue storage through insertion and consumption?
  Settles it: bind actual clock/queue selection, pointer/count/capacity,
  order and boundary-readable storage at 371D:01F9 and consumption;
  identify direct/indexed queue writers, copied/restored records and
  aliases that can change those bindings. Trace the payload's initialized
  slot versus its uninitialized bytes, snapshot/dequeue ordering and
  handler selection to the use-time slot, including rejected/full paths
  and nonnegative-lifetime scheduling. Bind cell/read-span and redraw
  page/table/rectangle/source inputs and graphics effects, with
  Q-PARTY-122 for unload and Q-PARTY-119 for image-buffer dependencies.
  FND-ITEM-073/074/075 settle local copy/shift/snapshot/dequeue/dispatch
  contracts and FND-ITEM-076/077 cell/redraw local ordering; these do not
  prove native queue admission or scheduling success.
  Tried: split from Q-PARTY-116 using FND-ITEM-073 through FND-ITEM-077
  and FND-ITEM-098's deferred producer. Preserve all initial-bound,
  odd-copy, boundary-row and ignored-result conditions. Earlier attempts
  remain in Q-PARTY-116 at revision 3d9b7ea4; no successful native scheduling is inferred.
  Blocks: Q-PARTY-116 deferred slot/queue lifetime and Q-PARTY-121
  constructor-entry queue-child preservation.
- Q-PARTY-119. FMT-PARTY-006: Does the bitmap cache supply coherent image
  content at the frame/dimension reads across lookup, reload and compaction?
  Settles it: admitted image/tag/size and archive identity at 31E0:3568;
  count/flags/offset/buffer producers and lifetime for the 52A2:0CA8 cache,
  including other writers/restores and failed acquisition state; pool record
  order, inclusive-bound/boundary storage, aliases and pointer/content
  correspondence through allocation, reclamation and consolidation;
  backing-handle initialization and lifetime at DS:45A2, selector/chain
  producers and remaining 0C54/0C04/0591 children, complete backend-table
  writers and active registration/selection beyond FND-EXE-584's literal
  writer exclusions; native service/driver content outcomes and per-call
  preservation for the four known families in FND-ITEM-092; and remaining
  56EF:00D9 and 5773:0039/0025 reset-child effects. Bind a successful or
  failed provider/reload result to the actual retained buffer and frame
  span consumed by image refresh, including content moved and pointers
  adjusted together. FND-ITEM-084/085 settle local refresh/pointer effects,
  FND-ITEM-086 through FND-ITEM-091 settle local provider/pool/transfer/
  validator contracts, and FND-ITEM-092 composes known callback contracts;
  none alone supplies native content or a complete writer census.
  Q-PARTY-116 retains constructor publication and use-time owner/slot identity;
  Q-PARTY-087 retains the distinct bitmap-number cache's reader/writer scope.
  Tried: split from Q-PARTY-116 after its local provider, pool and backend
  readings expose this independently answerable content/lifetime obligation.
  No unresolved native admission or failed attempt is discarded.
  Tried: FND-ITEM-092 composes existing known producer/callback contracts
  for all four backend families. Their local zero/error mappings differ
  and do not guarantee complete content. Active registration/selection,
  unexcluded writers and native service/driver outcomes remain above;
  do not reread only the already recorded callback bodies.
  Tried: FND-ITEM-091 removes offset-validator logic and composes existing
  FND-EXE-409/414 selector publication contracts. Positive counts reject
  offset carry; zero count passes only at offset zero. Native buffers,
  chain/selector producers and backend/child effects remain above.
  Tried: FND-ITEM-090 removes backing read/write's local count/range/error,
  linked-block/chunk dispatch and offset-only buffer advance. Native
  selector/chain/validator producers, installed backend targets/effects
  and buffer lifetime remain, not another unchanged transfer-loop reading.
  Tried: FND-ITEM-089 removes local release/residency/pool-match gates,
  metadata insertion/deletion and consolidation/pointer-adjustment ordering.
  Bound resets and residency clears are not proven content preservation;
  native pool/cache producers, spans/order/aliases and backing-store/reset
  children remain above, not another unchanged reading of these bodies.
  Tried: FND-ITEM-088 removes reclamation's local age/candidate/release
  accounting and consolidation-call ordering, and composes the already
  recorded pointer update. Its byte total is not verified free space;
  native pool/cache state and release/consolidation children remain above.
  Tried: FND-ITEM-087 removes reload's local pre-read/index/publication
  gates and the pool allocator's search/selection/two-pass/reset ordering.
  Exhaustion requests global cleanup before null return. Native pool/cache
  inputs and remaining named children stay above, not unchanged rereading
  of these local bodies.
  Tried: FND-ITEM-086 removes the bitmap provider's own scan/tag/residency,
  size/failure and count-publication gates. Hit reload and backing-store
  results are unchecked; native cache/archive/buffer state and the named
  children remain above, not another unchanged reading of the provider.
  Blocks: complete interpretation of FMT-PARTY-006 and Q-PARTY-116 child preservation.
- Q-PARTY-117. FMT-PARTY-006: Do the equipment and cast DATA reads obtain
  coherent key/content and retain the snapshots actually used? Settles it:
  actual getter key/buffer inputs, cache-hit versus accepted-miss results,
  the lower reader's returned pointer and complete transferred content,
  shared buffer/key writers and failures, repeated read identity across
  hook/reset/cast children and the separate SS snapshots' lifetime.
  Caller item/owner/index admission remains Q-PARTY-109, placed-table
  publication Q-PARTY-116 and equipment flag lifetime Q-PARTY-118.
  Tried: split from Q-PARTY-109 after FND-ITEM-057 settles the full local
  getter/cache wrapper and canonical source properties, and FND-ITEM-062
  identifies the main cast's separate local copy and later reread. Source
  record existence is not successful transfer or use-time coherence.
  Blocks: complete interpretation of FMT-PARTY-006.
- Q-PARTY-118. FMT-PARTY-006: Is the equipment flag's published value
  preserved to each immediate or queued effect consumer, and what decides
  its later value? Settles it: a complete controlled census of direct and
  indirect/aliased flag writers and consumers, with gaps and positive
  controls; their admitted ordering across 193 +0000/+003C, nonsentinel
  sound requests not excluded by the guest gates, hit,
  queued effects and callback/reentrant children; actual use-time values
  and the outer caller's final clear. DATA coherence is Q-PARTY-117 and
  placed-row lifetime Q-PARTY-116; other owner/item admission is Q-PARTY-109.
  Tried: split from Q-PARTY-109 after FND-ITEM-059 settles the outer
  publication/clear and FND-ITEM-061/062 settle selected local prefixes and
  queue ordering. FND-MAGIC-018 and FND-PARTY-167 identify duration
  consumers; their use-time value is not established by the outer store.
  Tried: FND-ITEM-069 supplies a controlled direct byte/word/doubleword
  operand census with the independently known read and two store controls.
  All additional raw/anchored numeric matches are classified; no other
  overlap is found within those forms. Computed/aliased, bulk/wider and
  startup producers, actual DS, callbacks and use-time preservation remain.
  Tried: FND-ITEM-070 identifies startup's explicit forward zero fill as
  a bulk flag producer before initialization callbacks. It also separates
  nominal save spans from actual recorded-resource transfer lengths;
  initialization targets, actual pointer/length/segment state, other bulk
  and computed writers and later preservation remain.
  Tried: FND-ITEM-071 measures zero at guarded main entry after startup
  initialization on the fixed -W0 -L path. The initial endpoint is settled
  for that path; continuous callback preservation, other paths, restored
  state and immediate/queued consumer values remain unestablished.
  Tried: FND-ITEM-072 excludes sound children for the 85 correctly bound
  sentinel requests among the canonical 186 requests. Every one of the
  93 candidates still has a nonsentinel request, so actual guest gate state
  or reached sound-child preservation remains required; host muting does
  not exclude guest sound logic.
  Blocks: complete interpretation of FMT-PARTY-006.
- Q-PARTY-108. FMT-PARTY-006: Which player-visible decisions consume
  classification bytes returned by `28C9:28E3` and `178 +0C6B`, beyond
  the recorded Look gate? Settles it: other reached consumers and their
  tested widths/bits, original input producers and capacities, and the
  name and door replacement branches on those routes. Tried:
  FND-PARTY-252 settles the local classifier and direct caller, including
  a distinct kind-two 0x08 producer, but assigns no downstream UI meaning.
  Tried: FND-PARTY-253 identifies the mode-two pointer's reached consumer
  as any-nonzero AL, with zero falling back to a separate word-mask gate;
  that consumer's local width/test is removed from Settles it. Other
  consumers and admitted inputs remain. FND-ITEM-036 removes the complete pointer
  body's own consumer width and canonical type-domain predicate; the independent
  surviving Look admission moves to Q-PARTY-115.
  Blocks: complete interpretation of FMT-PARTY-006.
- Q-PARTY-115. FMT-PARTY-006: Can admitted type-50 item state make word +0x10
  independently change the Look pointer? Settles it: remaining creations/replacements,
  type changes and name-invalidating writes outside FND-ITEM-038's selector exclusion
  and FND-ITEM-039's initializer/free-list/scalar kind-one exclusions,
  admitting name >=9999 outside preserved
  canonical RDFF copies of every initial type covered by FND-ITEM-041;
  retained value/quantity/word state,
  passing target and
  preliminary gates, zero fallback mask 0x1D and the later door-bit check; or complete
  producer/domain exclusion. Tried: split from Q-PARTY-108 after FND-ITEM-036 settles
  the local predicate and single canonical candidate; FND-COMBAT-171 supplies a paused
  source-equal table, not a Look call. Tried: FND-ITEM-037 removes unnamed canonical
  type-50 first items: all 631 have a valid name, including two other-size resources.
  Tried: FND-ITEM-038 removes field-eleven selection from recorded fixed/type/list/transfer
  setter families, canonical chunk headers and 2235 unchanged decoded script paths.
  The GPL/37 gap, changed/other paths, direct/computed stores, whole-record copies,
  nonfirst records, aliases and live admission remain. Tried: FND-ITEM-039 removes
  the known initializer, allocator/release leaves and independent scalar setter
  kind-one paths as direct name/type invalidators under valid separated storage.
  Other direct/computed writers and whole-record producers remain, as do the
  previously named gap, alias and use-time obligations. Tried: FND-ITEM-040 removes
  nonfirst canonical RDFF type-50 payloads: all 633 selected items are named,
  including the two later chunks. Other input domains, type changes from other
  initial types and runtime replacement/preservation remain. Tried: FND-ITEM-041
  removes unnamed canonical RDFF inputs of other initial types: all 1931 item
  payloads across 87 types are named, so a name-preserving type-only change cannot
  supply the invalid-name prerequisite. Other input domains and runtime name
  writes/copies/aliases and changed streams remain. Tried: FND-ITEM-042 removes
  the unchanged GPL/37 0D37 route's local target uncertainty: preserved returning-error
  context leaves an accumulator-byte store and stops before another opcode.
  The gap now retains error-child effects (Q-SCRIPT-003, Q-CONFIG-008), context
  preservation/aliases, other incoming offsets and changed dispatch/stream state,
  rather than another guessed decoding of that route. Blocks: slices 4-5.
- Q-PARTY-107. FMT-PARTY-006: Which admitted door actions preserve the
  source/partner record identity and realize overlay 182's flag and
  presentation requests? Settles it: the `+0C23` and `+0DE6` caller
  arguments, DS:0DAB and DS:655C producers, slot/type/record identity across
  the resource/SCMD/action children, partner kind/index admission, the
  0x8000 bypass's local initialization, bit-2/bit-7 door meaning and the
  admitted strip dimensions/coordinates and GMAP identity, and the
  effects of `31E0:271E`, initialization's resource requests and `+0BAC`.
  Tried: FND-PARTY-250 settles the
  classifier, masks, exact store values and local ordering only.
  Tried: FND-PARTY-251 answers `+0FC5`'s complete local strip geometry and
  bounded GMAP set/clear writers, and initialization's local flag consumer;
  those local effects are removed from Settles it. Native inputs, resource
  and presentation effects remain.
  Blocks: complete interpretation of FMT-PARTY-006.
- Q-PARTY-106. FMT-PARTY-006: Which admitted actions and later consumers give
  gameplay meaning to the low/high bytes written at overlay 197 `+3792` and
  `+38B3`? Settles it: their callers' object and second-argument provenance,
  threshold-table initialization and stable inputs, the subsequent readers
  of both bytes and their effects, including zero-low decrement wrap and
  conditional draw admission. Graph identity is Q-PARTY-099 and head naming
  Q-PARTY-082. Tried: FND-PARTY-249 settles the local gates, packing and
  helper/draw sequence, but assigns no gameplay effect. Blocks: complete
  interpretation of FMT-PARTY-006.
- Q-PARTY-087. FMT-PARTY-006, SCR-UI-008: Which remaining code reads or
  writes the attached record's cached BMP number at +0C directly, outside
  the generic field accessor? Settles it: a census of instruction starts
  that form a kind-1 record address (DS:19C1, its copy DS:6153 or the
  accessor's DS:614F row, times 23) and access +0C, including computed
  offsets and partial-record copies, beyond the reviewed inventory drawing,
  current-item helper, opener refresh, 191 +01F7 store and placed-slot
  classifier's `28C9:29BF` store, with what each
  stores or does with the value read. No generic accessor request selects
  field six (FND-PARTY-239); whole-record creation and copies are
  Q-PARTY-091; resource/buffer validity is Q-PARTY-088; cache-specific binding/selection
  preservation is Q-PARTY-089; placement/backend rendering is
  Q-PARTY-100.
  Tried: split after six attempts. FND-PARTY-224 (drawing reuse, zero
  resolver and store), FND-PARTY-227 (current-item helper), FND-PARTY-228
  (opener 9999 refresh) and FND-PARTY-229 (191 +01F7 slot-image store)
  each removed one direct site; FND-PARTY-225 answered the resolver's local
  arithmetic. No census of the remaining direct accesses was taken.
  Blocks: complete reading of FMT-PARTY-006 and item-picture admission.
  Tried: FND-PARTY-252 adds `28C9:29BF`'s direct cache producer: with
  type byte +4 bit 5 clear, a zero cache receives the placed slot's
  image word. That site is answered locally; the remaining census and
  admitted identity/resource conditions stay open.
- Q-PARTY-091. FMT-PARTY-006, SCR-UI-008: Which code creates or copies a whole 23-byte
  attached record, and what does each leave in +0C? Settles it: every
  kind-1 record allocation (2D40:2D78 through 2D40:0E80), record
  duplication on quantity splits, and block copies covering +0C, with the
  zero and 9999 values each can leave for the drawing resolver and opener
  refresh.
  Tried: split from Q-PARTY-087. FND-PARTY-259 records overlay 187 +04F8
  copying 23 bytes and then storing zero at +0C; FND-PARTY-196 gives the
  shipped +0C values; FND-PARTY-228 reads the opener's 9999 refresh
  without naming a producer of 9999.
  Tried: FND-PARTY-235 reads the CHAR chain writer: it saves each 23-byte
  record whole, +0C included, and the load's kind-1 copy (FND-PARTY-259)
  replaces that word with zero, so the CHAR chunk copy and CHAR save
  serialization are removed from Settles it.
  Tried: FND-PARTY-236 reads the numbered save and restore: SAVE block
  selector * 61 + 7 holds the whole 400-record kind-1 table, and the
  restore then zeroes +0C in every record, so the SAVE blocks and their
  restore are removed from Settles it.
  Blocks: complete reading of FMT-PARTY-006 and item-picture admission.
- Q-PARTY-088. FMT-PARTY-006, FMT-ACTOR-001, SCR-UI-008: Which resource
  and buffer conditions admit the bitmap resolver and subsequent BMP request?
  Settles it: descriptor 190 +2F2F's admitted object-number/resource domain,
  actual selected resource and transfer/size/status outcomes beyond the
  reviewed full-count and reuse/allocation branches, successful offered-buffer
  image-word initialization, and the inventory acquisition/release paths'
  actual buffer extent, lifetime and pointer outcomes. Cache producers/consumers are
  Q-PARTY-087; record/selection/binding preservation is Q-PARTY-089; pixel and
  backend behavior are Q-PARTY-100.
  Tried: split from Q-PARTY-087 after FND-PARTY-225 maps the resolver's local
  arithmetic, status gate and original-buffer read, and FND-PARTY-226 maps the
  caller's placement/release requests. Neither establishes these resource and
  buffer conditions. Blocks: admitted item-picture acquisition.
  Tried: FND-PARTY-231 binds the resolver and BMP caller to the reader's
  full selected length, existing-buffer versus allocation branches, pointer
  assignment before later errors and bounded sixteen-byte transfer request.
  Those local choices are removed from Settles it; actual archive/record
  selection, admitted extents/aliases, OS results and lifetime remain.
- Q-PARTY-089. FMT-PARTY-006, SCR-UI-008: Which runtime bindings and
  preservation conditions keep a cache access associated with the selected
  item across its child calls? Settles it: cache-specific record-index/table
  identity, graph/map/current-character selection before and after the
  reviewed resolver/transfer/rebuild calls, local DS/SS correspondence, and
  current kind-one field-six offset/type/width bindings. Generic inventory
  graph roots/capacity/modes are Q-PARTY-099; producer/consumer coverage is
  Q-PARTY-087 and resource/buffer outcomes are Q-PARTY-088.
  Tried: split from Q-PARTY-087 after FND-PARTY-224/227/228 record post-child
  selection reloads, FND-PARTY-229 records the local capture/store sequence,
  and FND-PARTY-230 binds shipped field-six metadata. Reloads and initial data
  do not prove preservation or runtime table stability. Blocks: complete
  cache identity and admitted item-picture selection.
- Q-PARTY-082. FMT-PARTY-006: Which possessions or equipment lists do the kind-2 combatant words
  `0x08` and `0x0A`, fields 16 and 17, head? Settles it: code that reads field 16 or field 17 by
  number, or offset `0x08` or `0x0A` through the combatant table, apart from the generic graph
  walk, with what selects each list and what the player sees of it. Generic enumeration order
  does not assign inventory or equipment labels. Field 4 is Q-PARTY-097, head mutations
  Q-PARTY-086, graph and root storage Q-PARTY-099. Tried: FND-PARTY-259 and FND-PARTY-052 map
  the three words to handles attached by CHAR chunks, FND-PARTY-054 and FND-PARTY-218 give the
  walk order 15, 16, 17, 4 and its field provenance, and FND-PARTY-217 shows the inventory redraw
  reads the merged graph by position byte rather than any head; no reader tells fields 16 and 17
  apart. FND-COMBAT-074 tells fields 16 and 17 apart by use: the attack routine takes weapons
  only from field 17, and the ability rebuild and two spell routines count field 16; what the
  player sees of each list is still open. Blocks: slice 2.
- Q-PARTY-097. FMT-PARTY-006: Which screens and actions, other than the recipient transfer, read
  or select the kind-2 field-4 head at `0x0C`, and which list does the player see it as?
  Settles it: the callers that request field 4 of a kind-2 object through the field accessor or
  read offset `0x0C` through the combatant table, beyond the recipient-transfer route, with the
  screen or action each serves. Tried: split from Q-PARTY-082. FND-PARTY-221 binds the
  character-recipient transfer to target graph entry zero and field four, so a received item
  joins that list; FND-PARTY-196 counts 10 of the 42 shipped type-2 chunks attaching through
  field 4.
  Blocks: slice 2.
- Q-PARTY-086. FMT-PARTY-006: Which character-screen actions change a kind-2
  object's field-16, field-17 or field-4 head, and how do they preserve or
  replace the attached chains? Settles it: remaining action-handler bindings,
  admitted current-object/head identity beyond the reviewed recipient request,
  complete list detach/append/replacement effects,
  failure continuation and the later screen consumer. Keep this separate
  from assigning gameplay labels to the heads (Q-PARTY-082, Q-PARTY-097) and
  graph/root storage admission (Q-PARTY-099).
  Tried: split from Q-PARTY-082 after FND-PARTY-054's existing traversal
  reading and FND-PARTY-217's projection do not establish user-action head
  updates; FND-PARTY-052 supplies the serialized field-to-head bindings.
  Tried: FND-PARTY-221 answers the recipient route's target entry/field
  arguments, local optional field-four setter and final field-ten request,
  ignored setter statuses and conditional runtime width. Those local request
  bindings are removed from Settles it; other actions, admitted current-object
  preservation, all transfer/splice children, failures and later consumption
  remain.
  Blocks: slice 2.
- Q-PARTY-083. FMT-PARTY-006: Which action opens a container item, shows the items its word
  `0x08` heads, or moves an item into or out of it? Settles it: the callers of the graph-entry
  setter paths (`2D40:288C` and its neighbours) and of the field accessors whose field number is
  a run-time value (among them overlay 188 `+0764` and `+079D`) that can name field 4 of a kind-1
  record, with the screen or action each serves, and the inventory redraw's 42 to 47 branch,
  which `CHAR/30`'s chest contents take. Tried: FND-PARTY-052 and FND-PARTY-196 give the three shipped chest records;
  FND-PARTY-247 shows the word heads a Chest's contents, finds no direct access to it and no
  literal field-4 accessor call, and reads the redraw keeping contents out of map positions 1 to
  29. Blocks: slice 2.
- Q-PARTY-084. FMT-PARTY-006: Does the quantity layout reading cover every
  admitted count producer and consumer, with field/table bindings preserved?
  Settles it: all kind-1 field-1 and direct offset-2 readers/writers, descriptor
  and record-table population/stability, and placement's request admission,
  signed arithmetic, aliases, callee returns and status consumption. Tried:
  FND-PARTY-216 locates the shipped field-1 descriptor and placement's full
  versus partial split branches; it leaves those complete-reading obligations
  unproved. Blocks: complete reading of FMT-PARTY-006, not its remaining fields.
- Q-PARTY-085. FMT-PARTY-006, SCR-UI-008: Where on the inventory screen does an item with each
  position code appear, beyond the source origins of codes 0 to 25? Settles it: what the
  inventory does with positions 42 to 47 (codes 38 to 43) and 1 to 3 (codes -3 to -1, `0xFF`
  among them), which the main draw loop skips, and whether anything moves the `APFM/11216` to `APFM/11241`
  rectangles between loading `WIND/13500` and drawing. Writers of the byte are Q-PARTY-098, the
  graph the projection reads Q-PARTY-099, and the draw paths Q-PARTY-100. Tried: FND-PARTY-217
  reads the signed projection, the main draw loop over positions 4 to 29 and the other branches'
  positions 42 to 47; FND-PARTY-222 and FND-PARTY-223 bind codes 0 to 25 to their `APFM`
  identifiers and source origins. Blocks: complete inventory positioning, not other
  attached-record fields.
- Q-PARTY-098. FMT-PARTY-006, SCR-UI-008: Which code writes an attached record's position byte
  at `0x13`, and with which codes? Settles it: every request for kind-1 field 10 through the field
  setter and every direct store to offset `0x13` in the table at `DS:19C1`, beyond the
  recipient transfer's field-ten request, with the code each writes. Tried: split from
  Q-PARTY-085. FND-PARTY-220 reads the recipient-space check that picks the first free code from
  14 to 25, and FND-PARTY-221 the recipient transfer's field-ten request and its dependence on
  the runtime width of type 0. Blocks: complete inventory positioning.
- Q-PARTY-099. FMT-PARTY-006, SCR-UI-008: Which object graph does the inventory projection read,
  and does it place every attached record of the character? Settles it: the producers of the
  graph and of the current object the projection is given, the capacity of its map, where the
  direction flag the fills depend on is cleared, whether the parent traversal ends, what the
  mode bytes `DS:6187` and `DS:6188` select, and what overlay 189 `+146B` does when it recurses.
  Tried: split from Q-PARTY-085. FND-PARTY-217 reads the projection's fills and branches and
  leaves those inputs and `+146B`'s effects unread. Blocks: complete inventory positioning.
- Q-PARTY-100. FMT-PARTY-006, SCR-UI-008: What do the inventory's item draw requests put on the screen? Settles
  it: the inventory's draw paths other than the main loop, and the window-system and backend
  routines the shared tail's centred and temporary-view requests and its release reach, with
  the buffers they write and the clipping they apply. Image acquisition is Q-PARTY-088. Tried:
  split from Q-PARTY-085. FND-PARTY-224 reads the main loop's cached `BMP` request and
  FND-PARTY-226 the shared tail's signed dimension test, its centred or temporary-view request,
  the `0xFFFF` exit and the release; the routines those requests reach remain. Blocks: complete
  inventory positioning.
- Q-PARTY-101. SCR-PARTY-003: What do the class window's setup, event dispatch, close and
  restoration calls do beyond the local mapping of its buttons? Settles it: the children the
  `WIND/17502` setup calls before its event loop, the routine that delivers events to its
  callback, and the close and restoration calls after the loop, with the window state each
  leaves. Tried: split from Q-UI-002. FND-PARTY-232 reads the window request, the callback, the
  eight source button IDs with their eligibility flags, and the local event loop and close
  pointer branches; the children they call remain. Blocks: nothing yet.
- Q-PARTY-005. FMT-PARTY-004: Which paths and inputs reach overlay
  202 +0C82's PSST active-bit clear?
  Settles it: the entry and callers or stored callback transfers reaching
  the routine that holds +0C82, with the current member and power index
  used for that write and any interpretation-affecting state changes.
  Reuse FND-PARTY-210's local byte clear; do not repeat the rank or
  maintenance-table readings. FND-MAGIC-007/008 settle the maintenance
  byte and positive/zero/99 gates, and FND-MAGIC-014 binds the walker's
  signed clock condition and selected clear calls. Their stated alias,
  table-writer and clock-source exclusions remain where relevant; they
  do not prove all possible native timing or a new invariant.
  SPST computed-copy coverage is now Q-PARTY-128.
  Tried: assess FMT-PARTY-004 using FND-PARTY-006/011/012/104/210/211
  and FND-MAGIC-007/008/014. The later findings remove the table-byte
  meaning and local walker-condition obligations. The overlay-202
  caller/input context remains; the older 2AAD spelling is not another
  independent sweep implementation. Split the distinct SPST question
  into Q-PARTY-128 without answering it or changing evidence confidence.
  Blocks: complete interpretation of FMT-PARTY-004.
- Q-PARTY-017. RULE-PARTY-006: When the player opens Create Characters (start window button
  `0x4B65`, overlay 212 `+061C`, window `0x2CEC`) before START GAME, which actions on that
  screen other than `ADD` on an empty box change the placed-object count, `DS:0DAB`, the
  pointer image or a window callback before the start loop reaches START GAME, and does any
  routine reached only through the unresolved transfers of FND-PARTY-046's two runs (48 from
  the ADD window, 66 from the screen's handler) store 0 to the count or make `DS:0DAB`
  nonzero? Settles it: the code of `NEW` (overlay 184 `+07D8`, and whether it reaches
  `+1029`'s placement call at `+12E5`), of overlay 190 `+139B` and `+0802` read for the events
  that reach the `DS:13FB` setter overlay 173 `+3CDA` and the pointer-image sites, each
  unresolved transfer of the two runs resolved or shown unreachable, and `56E9:0025`, overlay
  171 `+0938` and `56BD:00A2` read for stores to the party record between `ADD`'s load and its
  placement (FND-PARTY-049). Tried: FND-PARTY-045 run 4
  found the routes; FND-PARTY-046 reads `ADD`, which increments the count and leaves no store
  that clears it on the way back to the start window. Blocks: slice 2.
- Q-PARTY-018. RULE-PARTY-006: After a confirmed load from the start window's Load Saved Game
  window (overlay 192 `+0785`, confirm branch `+0B21`), can START GAME still be chosen, and with
  what placed-object count and `DS:0DAB`? Settles it: the load routine overlay 192 `+05ED`,
  overlay 182 `+19F8`, `28C9:2522`, the gate routine called with 0, and whether window 19500 or
  its handler survives those calls (FND-PARTY-045). Blocks: slice 2.
- Q-PARTY-105. BUG-PARTY-006: Can a party member's cast or item power give the spell dispatcher
  or the hit routine `DATA` 313, whose strike on an attack roll of 20 sends effect code 59
  (FND-PARTY-096)? Settles it: whether overlay 211 `+10F4` can reach its cast at `+15FA` while the
  member's mode byte `DS:43E8` is 0 and `DS:9D79` is above 0 (when `SI` keeps its caller's value),
  read from where `+1A4F` runs after each store of mode 0 (overlay 190 `+284A`, `+28C5`) and from
  the controls and window that send `+10F4` its events through trampoline `57C1:0061`. Tried:
  split from Q-PARTY-103, which FND-PARTY-242 closed by finding that no enemy or script route
  passes 313; FND-PARTY-242 traces the party-side numbers to those stores. Tried: FND-PARTY-243
  reads every store of `DS:6273`, the table at `DS:445E`, the list at `DS:9C20` and `DS:43F5`:
  modes 1 to 3 give list numbers below 269, overlay 172 repeats table entries and overlay 213
  passes item powers, so only the mode 0 case is left. Blocks: nothing yet.

- Q-PARTY-124. FMT-PARTY-007, RULE-PARTY-009: Which complete set of game-code accesses
  reads or writes DATA 1001, and which caller/index domains do they admit? Settles it: remaining accesses that could change the claimed 576-byte extent, row indices or class-bit meanings, through the mechanisms below.
  Resolve runtime-computed or remotely assembled/copied targets, entries bypassing FND-PARTY-264's four roots, and separate buffer readers/writers; retain its near-transfer and producer-distance exclusions.
  For 2D40:00A9, resolve prefix changes through computed/copied addresses, aliases, block operations or excluded streams, and segment preservation from FND-PARTY-219's observed main equality to the two tag-builder callers. FND-PARTY-270 removes tested scalar writers/literal in-window pointer producers. For 3A8E:03B4, resolve indirect/bypass inputs, runtime provider/transfer identity and reached-helper or computed/copied/alias writes affecting the window pointer, byte +F2, word +F3 or selected child type +4/number +8; reuse FND-PARTY-271's two direct WIND routes, own-store exclusions and FND-COMBAT-095's sole-provider/table bounds, including incomplete-transfer qualifications.
  For 47E5:04B8/0500, resolve incoming computed, unrelocated, remotely assembled/copied and near/bypass target forms excluded by FND-PARTY-269; direct and tested nearby relocated bindings are no longer unknown argument sources. Reuse FND-PARTY-268's five local choices/four caller bindings and FND-PARTY-267's literal exclusions/DATA-1002 binding.
  For 2C5F:0003, bind the other 83 direct caller number domains and repeated-hook preservation. FND-PARTY-265/272 exclude three sites; 273/274 bound shipped words/relocations. For resident aliases, resolve eleven other writer values, FND-PARTY-283's FONT pointer-segment/offset, prior A0ED byte and allocation-segment provenance/preservation, FND-PARTY-282's step-source, unsigned-selection, modular-sum, DS/SS and preservation conditions, FND-PARTY-281's four archive-pointer low-byte/current-count conditions and increment E8FF..E9FE/decrement E901..EA00 predecessor admission, and FND-PARTY-279's count-capacity/frame/saved-SI/output-pointer and DS:9C1E handoff conditions; its ordinary count domain is answered. Keep jumps/computed/unrelocated/copied/remote targets or runtime overlay bindings to the 34 interiors, larger/block/indexed/copied/segment-alias writes, index retention and other word-index domains. FND-PARTY-277/278 remove tested calls/nearby pairs; 275/276 exclude 36 constants. Other word inputs retain producers; 208+0866 forwards its second word as selector.
  Reuse FND-PARTY-264's relocated/fixed-up nearby pointer census and FND-PARTY-058/068's direct-site/index/mask reading. Bind resulting argument/result widths, origin/class producers and rejection paths.
  Use the literal-1001 sites, DATA-1000 callers and stored/split-pointer controls; preserve unresolved transfers, decoder/range exclusions and interpretation-affecting source/base/extent/byte writes.
  General I/O success and unrelated lifetime are not claims of this payload format. Full generation-screen behavior stays RULE-PARTY-009; the unowned ICON candidate in FND-PARTY-267 is not asserted to interpret DATA.
  Tried: FND-PARTY-267 removes the 105 literal non-DATA request sites from number tracing and names the variable-tag remainder. Its partial-reader ownership gap and PUSH-CS limitation remain explicit.
  Tried: FND-PARTY-268 classifies thirteen variable sites into five local non-DATA choices, four known-caller bindings and four type domains. FND-PARTY-269 removes unclassified canonical chunk/window tags and tested nearby relocated reader bindings. FND-PARTY-270 removes the tested direct prefix-writer/pointer forms; computed/alias/block/alternate-stream writes, later segment preservation, live-child writers and excluded targets remain. FND-PARTY-267 corrects the prior overlay label without changing the census.
  Tried: FND-PARTY-264 removes the tested relocated/fixed-up pointer mechanism; its alias candidates are unrelated operands and the missed Ghidra control supplies no negative proof. FND-PARTY-265 excludes the first equipment request. FND-PARTY-272 excludes two name-helper number domains and identifies three word-input sites whose local FFFF test admits 1001. The other 83 direct sites, repeated hook preservation and indirect clients remain.
  Tried: FND-PARTY-058/068 leave computed-1001/direct-only coverage; no undeclared qualifying reading. FND-PARTY-271 removes separate direct window tables/own stores; 273/274 exclude shipped words/relocations; 275/276 exclude 36 constants; 277/278 account for tested interior calls/pairs. FND-PARTY-279 bounds the count-value result while retaining its named storage conditions. FND-PARTY-281 answers six archive expressions, retaining pointer/count admission. FND-PARTY-282 excludes the canonical segment store and names five step/residual conditions; FND-PARTY-283 binds three graphics expressions to FONT pointer bytes and allocator segment; eleven other values remain. The 83-site count is unchanged. Blocks: complete reading of FMT-PARTY-007.

- Q-PARTY-126. FMT-PARTY-001, FMT-PARTY-006: Do all CHAR readers and writers preserve the documented chunk prefix and field interpretations? Settles it: the remaining indirect entries, separate readers/writers,
  pointer copies and computed field accesses affecting the documented CHAR representation, after FND-PARTY-235's fourteen known direct
  resource-write sites and computed-tag bindings. Bind the serializer's root kind, mutable graph/field lists and restored metadata to its
  type-1/type-3 prefix and sentinel; account for state writers or copies that can change that field selection, including the DS/SS identity
  and later field-list changes left open by FND-PARTY-235. The literal tag gap in FND-PARTY-054 alone is no longer the remaining direct-site
  question. Retain FND-PARTY-259's reader/header/reference and corrected status facts. Account for copied pointers, computed offsets and alias
  accesses excluded by FND-PARTY-152 wherever they could change a field interpretation or its no-use bound. Known CHAR load/write sites remain
  controls; preserve unresolved transfers and decoder exclusions. Reuse the known serializer/chunk and copy contracts; this does not ask
  for general file-I/O success or unrelated gameplay effects. Remaining unknown field meanings stay Q-PARTY-003.
  Tried: the FMT-PARTY-006 assessment identifies this as the same chunk-reader/writer obligation, not a second census. FND-PARTY-259
  retains the local dispatch/copy contracts and corrected error-status qualification; its unread allocation/reclamation children do not
  change the documented byte destinations but do not prove acquisition success. FND-PARTY-052 leaves later FNFO/type-width writers unsearched;
  FND-PARTY-235 leaves field-list mutation and DS/SS correspondence open. Track those bindings here. Placement publication, effect handling and
  bitmap content/lifetime remain their existing independent questions Q-PARTY-116/119/120/121/122/123, not prerequisites for deciding the
  stored header's byte widths or the known writer's selector source. Tried: review FND-PARTY-052/054/148/152/156/159/235/259 for the whole
  FMT-PARTY-001 claim. The later direct-site/tag census and serializer selector reading remove those parts of the older writer limitation;
  metadata/graph-state, indirect and pointer/access exclusions remain. No newly observed contradiction or qualifying whole-entry declaration
  was identified. Blocks: complete reading of FMT-PARTY-001 and FMT-PARTY-006.

- Q-PARTY-127. FMT-PARTY-003: Do all PSIN access paths agree with the documented one-byte representation and discipline-bit interpretation? Settles it: complete the game and unpacked CHARTRAN read/write and
  caller census for PSIN and its slot-byte sources, covering indirect entries, computed resource tags and pointer/index aliases that can
  affect the represented byte. Reuse FND-EXE-006's literal occurrences, FND-PARTY-011/012's one-byte transfer sites, FND-PARTY-235's fourteen
  known direct resource-write sites and FND-RNG-091's direct reader accounting; do not repeat those unchanged searches. Bind any remaining
  interpretation-affecting source/width/index or bit writes to their callers, including FND-PARTY-077's choice projection,
  FND-PARTY-210's reset and FND-SAVE-015's four-byte SAVE table entry 22. Keep opaque copies distinct from a new bit interpretation. Preserve
  unresolved transfers and census controls/exclusions for both executables. The entry does not claim that higher bits are forbidden, that every
  resource operation succeeds, or that a closed UI button never delivers an event; those are not additional blockers for this format claim.
  Tried: the existing-evidence review of FND-PARTY-002/011/012/077, FND-PARTY-210/235, FND-RNG-091 and FND-SAVE-015 confirms the physical
  byte and known bit mapping. The literal/direct-site findings do not establish a complete game/utility access set. No qualifying whole-entry
  complete reading was identified but left undeclared. Blocks: complete reading of FMT-PARTY-003.

- Q-PARTY-128. FMT-PARTY-005: Which unaccounted SPST accesses or
  transfers can affect its bit interpretation and short-load tail? Settles it: remaining indirect/computed SPST read routes beyond
  FND-PARTY-203's known reader/callers, and pointer-derived copies or computed-offset accesses reaching 4D62's four 15-byte entries, with
  their source/span/slot identity and ordering relative to a short read. Reuse FND-PARTY-203's exact resource-length copy and shipped ADD/START
  admission, FND-PARTY-209's literal-base bit accesses and clear ranges, and FND-PARTY-011/012/104's two write sizes and spell-bit indexing.
  FND-SAVE-015/022 already bind SAVE table entry 20 to the whole 60-byte bank and its reader: an admitted 60-byte restore replaces all four
  entries, not necessarily with zeros. Remove that mechanism from the unknown-transfer list. FND-RNG-091 supplies the known direct file-read
  accounting, retaining its stored-pointer and context-scan exclusions. Identify only remaining accesses that can change the claimed byte/bit
  interpretation or retained tail; preserve controls and unresolved transfers rather than asking for all generic I/O or consumer behavior.
  Tried: the FMT-PARTY-005 assessment reuses FND-PARTY-006/011/012/104/ 203/209 with FND-SAVE-015/022 and FND-RNG-091. The full-bank SAVE
  restore is a known writer; the local nine-byte read retains whatever tail was present at its entry. Other-reader and nonliteral-access
  exclusions remain. No actual transferred-character reuse sequence or whole-entry complete reading follows from that conditional composition.
  Tried: split from Q-PARTY-005 with its FND-PARTY-104/203/209 evidence. Known literal accesses do not clear a whole entry; pointer-derived
  transfers and actual reused-slot state remain outside that result. No earlier attempt, source exclusion or unresolved path is discarded.
  Blocks: complete interpretation of FMT-PARTY-005.

- Q-PARTY-129. RULE-PARTY-009, RULE-PARTY-002: Do the generation screen's admitted
  inputs and state writers preserve the documented class-button choices? Settles it: complete caller/event-target and working-record producer
  coverage for overlay 183 +0A3E and its mask/compaction/button helpers, binding origin 1..8, class codes 0..8 and the compacted zero-to-three
  class list at each selection. Reuse FND-PARTY-065/068/073's local mask, add/remove and code-conversion contracts. Account for the edit route's
  selected record and 4E71:0B44 writers left open by FND-PARTY-073. For the origin mask at 4E68:0000..0010, resolve writes via computed
  segments, overlapping segment aliases, stored pointers/copies and segment retention beyond FND-PARTY-069's 26-instruction windows,
  with known direct reads and the separate array stores as controls. Bind the button enable/selected-state contract and reachable callback
  targets needed for add/remove admission; this asks for no general graphics or successful file-I/O contract. DATA-1001 access/index and
  representation coverage is shared Q-PARTY-124, not another census. Tried: FND-PARTY-068 settles the local offer masks and the finite
  reachable-order check; FND-PARTY-070 settles DONE's local predicate, not every incoming edit/event path. FND-PARTY-069 explicitly excludes
  computed/alias and longer-lived segment accesses. FND-PARTY-073's fixed-displacement stores and direct callers leave the selected-record
  writer question open. No new contradiction or qualifying undeclared whole-entry reading was identified by this assessment.
  Blocks: complete reading of RULE-PARTY-009.

- Q-PARTY-130. RULE-PARTY-002: Can every admitted generation DONE route
  store only the documented portrait, score, alignment and choice state, apart from the already documented Ctrl-alignment exception?
  Settles it: bind the event/callback routes to overlay 184 +0F93/+1029, including flag-zero/flag-one callers, working-record initialization,
  editing and selection preservation through storage. Account for writes and copied/restored aliases of discipline/sphere words DS:42C0/42C2,
  the working portrait/origin/gender/alignment and six score bytes between their setters, DONE enablement and final copying. Resolve the closed
  button event-dispatch exclusion in FND-PARTY-077 wherever it can admit a choice outside the predicate. Reuse FND-PARTY-066/070/071/073/077's
  local setters, bounds, predicate and code conversion, and FND-PARTY-207's Ctrl bypass/recursive alignment reading. Class-mask and
  selected-record admission is shared Q-PARTY-129, not another census. This predicate review needs no complete RNG draw ordering or generic
  rendering proof: identify only effects that can change stored validity. Tried: existing-evidence assessment finds the local DONE checks and
  portrait mapping recorded, but FND-PARTY-070 explicitly leaves other finalizer admission open and FND-PARTY-077 does not complete dispatch.
  Those findings do not alone prove the whole screen's reachable states. No complete-reading declaration or new contradictory behavior follows.
  Blocks: complete reading of RULE-PARTY-002.

- Q-PARTY-131. RULE-PARTY-007: Does every admitted level-gain route use
  the recorded level-15 gate without an origin limit or prime-requisite bonus? Settles it: complete caller/indirect-entry coverage for overlay 210 +08BC/+0740
  and classification of level writes beyond FND-PARTY-160/081's fixed-displacement search: index-only accesses, copied details pointers and whole-record copies/restores.
  Bind loaded/edited/current class and level inputs to the gain gate, and identify reached children that can change those inputs or the level after the gate.
  Distinguish an opaque saved level from a competing level-gain rule; CHAR copying is already recorded in FND-PARTY-259, not proof that arbitrary stored levels
  were gained. Reuse the known below-15 comparison and absence of score/origin limit tests in the read bodies. A nonmatching README byte pattern excludes
  only that encoding; retain computed/word-table possibilities until actual relevant consumers are accounted for. Generation eligibility is shared
  Q-PARTY-129; other class-change behavior remains RULE-PARTY-004. Tried: FND-PARTY-065/081/160 establish the local starting/gain/class-shift
  bodies and bounded store census, but explicitly exclude copies/index-only writers and leave reached child effects and indirect caller coverage incomplete.
  No qualifying whole-entry declaration or new contradiction follows. Blocks: complete reading of RULE-PARTY-007.

- Q-PARTY-132. RULE-PARTY-008: Do all admitted leader-key and leader-button
  routes preserve the documented selection gates and party-state effects? Settles it: complete handler registration/restoration and callback-target
  coverage beyond FND-PARTY-127's immediate-pointer/direct-call searches; bind combatant/object-slot mapping, turn/leader/status and party-shown
  producers to the key handler and overlay 182 +1D3E. Account for computed, pointer-based and copy/restore writes left outside FND-PARTY-171's direct
  leader-store census, and reached positioning/redraw children affecting party state. Reuse FND-PARTY-176/179's local gates and stores and
  FND-PARTY-178's out-of-combat two-member observation. Do not assume unexamined registration restore callers cannot restore the handler in
  display mode two. Include the View Character and inventory button routes; the observed equal slot/combatant mapping is not a universal invariant.
  Tried: existing findings leave indirect registration/restoration, aliased state writers and some reached child effects open. FND-PARTY-130's
  unresolved transfers and missing controls cannot prove redraw preservation. Blocks: complete reading of RULE-PARTY-008.
- Q-PARTY-133. RULE-PARTY-008: Does every documented leader-key display
  path show the redrawn view immediately without changing party state? Settles it: complete the relevant 4328:0B4C page/pointer branch and input
  producer reading beyond FND-PARTY-180/195, retaining high-byte flag, page identity, DS:3330 and DS:A2B2 alternatives. Resolve the pointer
  service branch DS:A155 zero and other reached party-state effects; confirm actual visible page/pixels where VGA/retrace behavior determines
  the output, using evidence appropriate to that claim. FND-PARTY-195's map/out-of-combat trace establishes its observed branch and memory effects,
  not every gate or a screenshot comparison. Static work must identify the remaining hardware-dependent assertion before any new run is selected.
  Tried: FND-PARTY-179/180 record display request ordering and 195 records one reached page-flip branch; its other branch and pixel exclusions remain.
  Blocks: complete reading of RULE-PARTY-008's display claim.

## Emulated call

None.

## Agent run

- Q-PARTY-081. RULE-PARTY-001: On the defeat cleanup path, does the DS-relative
  deletion map at `2D40:27B7` and `2D40:248D` address the SS-local allocation of
  `2D40:2452`, and where is that segment relationship established? Settles it:
  a state-driven run reading DS/SS at this deletion call, including the caller
  frame and map offset, with complete preservation coverage between startup's
  segment producer, initialization-record targets and the observed cleanup
  call. The game-main equality observation is now FND-PARTY-219; it does not
  prove the later call. Required parts: Start it and bring it to a given state,
  Send it input, Read memory and set breakpoints, Guarded writes to supported
  combatant hit_points, and Record RNG draws, through the session package
  (docs/RUNTIME.md's earlier input/write/draw attempts and verified fixed
  startup stop). Tried: FND-PARTY-213 and FND-PARTY-214 identify the near map;
  FND-PARTY-215 reads the startup SS replacement; FND-PARTY-219 observes DS/SS
  equality at main without draws. Tried: FND-UI-039 reaches the start-window
  opener call without input on a verified private drive, reads argument 1 and
  matching DS/SS, and guards the known RNG entry. Tried: FND-RNG-010 observes
  the startup seed call and saves a checked opener state; FND-UI-040 restores
  that state natively and observes the opener returning with unchanged RNG
  state, then saves a checked opened state. Pixels, input readiness,
  subsequent state-driven gameplay/deletion stops and complete preservation
  coverage remain needed. The startup_segments.py executable-only drive cannot
  be continued to gameplay. Blocks: Q-PARTY-078's complete cleanup reading, not
  its remaining independent static input/callee work.
- Q-PARTY-096. RULE-PARTY-001: With a party of three or four, all at 0 hit points before START
  GAME, which slots are empty after the defeat window closes? Settles it: a state-driven run from
  the start window with several stored characters added, guarded writes setting each member's
  `hit_points` to 0, START GAME, a click on the defeat window, and reads of every slot's kind byte
  and `character_id` once the start window returns. Required parts: Start it and bring it to a
  given state, Send it input, Read memory and set breakpoints, and Guarded writes to supported
  combatant `hit_points`, through the session package. Tried: split from Q-PARTY-078.
  FND-PARTY-212 makes the same run with one member; FND-PARTY-213's static reading of the loop
  does not settle the gates for later members. Blocks: nothing yet.

## Live session

- Q-PARTY-104. RULE-PARTY-013: Can `GPL/144`'s trigger for object 161, registered at `0x0023`
  with entry offset 281, run its dialog branch to the opcode `0x24` at `0x0F14` during a fight
  (FND-PARTY-261)? Settles it: an owner capture, from a new game in region 50, of a fight in which
  the object placed at x 168, y 239 of region 50 (object 161) takes part, with a party member
  within 5 of it, showing whether its conversation menu opens during the fight, and if it does,
  whether choosing the row whose target is 3510 brings up a character screen (FND-PARTY-244).
  Required parts: Send it input and Capture frames and sound (frames), the owner's. Tried: split
  from Q-PARTY-050, which FND-PARTY-261 closed. Tried: FND-PARTY-244 finds the fight runs a
  trigger only for a slot whose combatant has `combat_mark` 1 and byte `+0x15` other than 1, and
  object 161 placed only in region 50; whether object 161 fights there and what the kind-9
  variable 32, `varD[374]`, `varD[331]`, `varE[7]` and the row's condition then hold depend on
  the course of play. Blocks: slice 2.
- Q-PARTY-059. BUG-PARTY-006: Does the game end with `Math Err` when a party druid of priest level
  13 to 15 casts the spell `DATA` 225 at a single-class companion above level 1 while a Shift key
  is held? Settles it: an owner capture, from a save with such a druid that has level 7 spells to
  cast, of the cast window at level 7, the target cursor on the companion with Shift held, and the
  screen after the companion fails its saving throw, repeated until one fails (FND-PARTY-109,
  FND-PARTY-280, FND-PARTY-111). Required parts: Send it input, Capture frames and sound (frames)
  and Load a patched save, for a save holding such a druid, all of them the owner's. Tried: the static readings FND-PARTY-082, FND-PARTY-105 and FND-PARTY-109 to
  FND-PARTY-111. Blocks: nothing yet.
- Q-PARTY-037. RULE-PARTY-013, BUG-PARTY-003: Does a party gain levels after a fight as
  RULE-PARTY-013 gives, and does a level-14 psionicist stay at level 14 with 1,400,000
  experience? Settles it: an owner capture of each member's experience, levels and hit points on
  the character sheet before and after a fight that gives experience, with the messages shown,
  for a party that includes a level-14 psionicist (FND-PARTY-081). Tried: the static reading
  FND-PARTY-081. Blocks: slice 2.
## Source

None.

## Blocked

None.
