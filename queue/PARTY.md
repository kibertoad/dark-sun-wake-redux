# PARTY

Next ID: Q-PARTY-093

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
- Q-PARTY-078. RULE-PARTY-001: After overlay 201 `+0410`'s defeat window closes, which routine
  takes the characters out of the party and returns to the start window, and does it empty every
  slot of a larger party? Settles it: complete admitted placement/slot inputs and returning
  cleanup callees of `31E0:05BB`, `2D40:2410`, `2D40:2452` and `2D40:248D`, with
  the recursive index map's DS/SS identity tracked separately by Q-PARTY-081,
  and the caller of `+3CDA` that START GAME uses;
  or a controlled several-member defeat run. Tried:
  FND-PARTY-194 reads `+0410`; FND-PARTY-212 runs START GAME with the only member at 0 hit points
  and sees the defeat window wait for a click, then `2D40:2539` clear `character_id` and the start
  window return. Tried: FND-PARTY-213 identifies the repeated index-zero unload, count
  decrement/compaction and chunk-type-2 identifier clearing; the former `2488` is a return
  site. Remaining input, callee and map-alias obligations prevent a whole-party claim.
  Tried: FND-PARTY-214 settles the compactor's near-helper return and identifies
  the deletion-map builder's parent-derived object stores and AX-only field-result
  consumption. Remaining map admission, lookup/conversion effects and DS/SS
  identity are distinct from that now-read index-adjustment helper.
  Tried: FND-PARTY-215 identifies the explicit startup SS replacement and
  initialization dispatch's lack of DS/SS restoration. The header's initial
  SS is not the later stack producer; service effects and initializer/later
  segment preservation are separated into Q-PARTY-081.
  Blocks: nothing yet.
- Q-PARTY-045. RULE-PARTY-015: Does any enemy's `kill_experience` divided by the filled party
  slots reach 32,768, so that killing it lowers each member's experience (FND-PARTY-086,
  FND-PARTY-094)? Settles it: what moves objects 430 and 541 out of the region `Limbo`, where
  their only `ETAB` records are, read to show whether the party can fight them. Blocks: nothing
  yet. Tried: FND-PARTY-094 finds `RDFF` 430 (107,000) and 541 (33,000), of kind 7, with CHAR
  chunk headers; FND-PARTY-153 shows `RDFF` resources load through the CHAR chunk walk and finds
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
- Q-PARTY-003. FMT-PARTY-001, RULE-PARTY-004, RULE-MAGIC-002: What do the record's remaining `unk_`
  fields hold? Settles it: the other
  routines that print the character sheet from the record at `DS:1429` (FND-PARTY-055), and the
  generation code that sets the scores. Tried: a search of the first 79 bytes of records 40 and 42
  for the label positions of their gender, origin, alignment and class (FND-PARTY-061);
  FND-PARTY-063 and FND-PARTY-066 show the four codes of Cleric, Druid and Ranger are the spheres; FND-PARTY-049, FND-PARTY-051 and FND-PARTY-055 to
  FND-PARTY-057 place hit points, the object offset, the combatant identifier, the control flags,
  greatest hit points, origin, gender, alignment, class codes with their names, and levels, and
  FND-PARTY-058 and FND-PARTY-059 place experience at `0x45` and the current and greatest
  psionic points at `0x0C` and `0x51`. FND-PARTY-142 shows `0x5A` holds the base scores, and
  with FND-PARTY-071 that the scores are stored with the origin modifiers added. FND-PARTY-143 shows `0x1F` is the
  combatant kind, FND-PARTY-144 that `0x29` is the facing and FND-PARTY-145 that `0x66` is the base
  armor class; FND-PARTY-146 shows `0x1D` and `0x67` are the current and base movement rate, FND-PARTY-147 that `0x53` is the details record's key, and FND-PARTY-148 that `0x04`, `0x06` and
  `0x0E` hold the slot the character was written from and `0x12` to `0x16` the child words at that time; FND-PARTY-149 shows `0x81` and `0x83` are sound numbers, and FND-PARTY-150 that `0x68` is the magic resistance; FND-PARTY-151 places the footprint at `0x7C` and a spell list at `0x7D`. FND-PARTY-152 finds no reader through the combatant table of
  `0x1C` or `0x21`, and `0x2A` only as field 33 of overlay 204's field getter and setter, which FND-PARTY-156 shows are
  functions 48 and 49 of script opcode `0x22`, so what `0x2A` means waits on Q-SCRIPT-004; FND-PARTY-157 names `0x18` the special attack kind
  and `0x19` the immunity type, and FND-PARTY-158 names `0x85` the psionic power list; FND-PARTY-159 shows the saving throw never
  reads `0x7B`. Blocks:
  slice 2.
- Q-PARTY-020. FMT-PARTY-006: What do the remaining fields of a 23-byte attached
  record mean, apart from `0x00`, quantity at `0x02`, chain link `0x04`,
  cached bitmap number at `0x0C`, and
  placed-entry bits at `0x12`, and inventory position at `0x13`? Its reference
  at `0x08` is Q-PARTY-083; complete position behavior is Q-PARTY-085.
  Settles it: direct readers/writers of the remaining offsets in the table at
  `DS:19C1`, with descriptor-to-consumer bindings and actual input admission. Tried:
  FND-PARTY-181 and FND-PARTY-182 give word `0x00` as the negated object number and byte `0x12`
  as the placed entry's low bits; FND-PARTY-196 lists the shipped values of every field and shows
  9999 ending each chain at `0x04`. Tried: FND-PARTY-216 identifies quantity at
  `0x02`; the character heads, nested reference and complete quantity reading
  are split into Q-PARTY-082, Q-PARTY-083 and Q-PARTY-084. Blocks: slice 2.
  Tried: FND-PARTY-217 identifies byte `0x13` as the signed position code
  consumed by the inventory projection; Q-PARTY-085 holds its remaining contract.
  Tried: FND-PARTY-224 identifies word +0C as the BMP-number cache read and
  lazily written by inventory drawing. That field-use question is removed
  from Settles it; its complete producer/resolver/lifetime reading is
  Q-PARTY-087, separate from the other still-unassigned fields.
- Q-PARTY-087. FMT-PARTY-006, SCR-UI-008: Which remaining code reads or
  writes the attached record's cached BMP number at +0C directly, outside
  the generic field accessor? Settles it: a census of instruction starts
  that form a kind-1 record address (DS:19C1, its copy DS:6153 or the
  accessor's DS:614F row, times 23) and access +0C, including computed
  offsets and partial-record copies, beyond the reviewed inventory drawing,
  current-item helper, opener refresh and 191 +01F7 store, with what each
  stores or does with the value read. Field-six accessor requests through
  script operands are Q-PARTY-092; whole-record creation and copies are
  Q-PARTY-091; resource/buffer validity is Q-PARTY-088; cache-specific binding/selection
  preservation is Q-PARTY-089; placement/backend rendering remains
  Q-PARTY-085.
  Tried: split after six attempts. FND-PARTY-224 (drawing reuse, zero
  resolver and store), FND-PARTY-227 (current-item helper), FND-PARTY-228
  (opener 9999 refresh) and FND-PARTY-229 (191 +01F7 slot-image store)
  each removed one direct site; FND-PARTY-225 answered the resolver's local
  arithmetic. No census of the remaining direct accesses was taken.
  Blocks: complete reading of FMT-PARTY-006 and item-picture admission.
- Q-PARTY-091. FMT-PARTY-006, SCR-UI-008: Which code creates or copies a whole 23-byte
  attached record, and what does each leave in +0C? Settles it: every
  kind-1 record allocation (2D40:2D78 through 2D40:0E80), record
  duplication on quantity splits, and block copies covering +0C, with the
  zero and 9999 values each can leave for the drawing resolver and opener
  refresh.
  Tried: split from Q-PARTY-087. FND-PARTY-051 records overlay 187 +04F8
  copying 23 bytes and then storing zero at +0C; FND-PARTY-196 gives the
  shipped +0C values; FND-PARTY-228 reads the opener's 9999 refresh
  without naming a producer of 9999.
  Tried: FND-PARTY-235 reads the CHAR chain writer: it saves each 23-byte
  record whole, +0C included, and the load's kind-1 copy (FND-PARTY-051)
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
  backend behavior remain Q-PARTY-085.
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
  graph roots/capacity/modes remain Q-PARTY-085; producer/consumer coverage is
  Q-PARTY-087 and resource/buffer outcomes are Q-PARTY-088.
  Tried: split from Q-PARTY-087 after FND-PARTY-224/227/228 record post-child
  selection reloads, FND-PARTY-229 records the local capture/store sequence,
  and FND-PARTY-230 binds shipped field-six metadata. Reloads and initial data
  do not prove preservation or runtime table stability. Blocks: complete
  cache identity and admitted item-picture selection.
- Q-PARTY-082. FMT-PARTY-006: Which possessions or equipment lists do the
  kind-2 combatant words `0x08`, `0x0A` and `0x0C` head? Settles it:
  code-backed roles of fields 16 and 17 relative to field four, and other
  character screen/action bindings beyond the reviewed recipient-transfer
  field-four request, including what selects each list and the player-visible
  meaning, if any, of those distinctions. Generic enumeration order alone
  does not assign inventory or equipment labels. Head mutations are
  Q-PARTY-086; graph/root/storage admission is Q-PARTY-085. Tried:
  FND-PARTY-051 and FND-PARTY-052 map those words to handles attached by CHAR
  chunks, and FND-PARTY-216 reads quantity placement but not the screen's
  distinction between the three lists. Blocks: slice 2.
  Tried: FND-PARTY-217 identifies the inventory opener and handler and shows
  redraw consuming a kind-1 graph projection indexed by position byte `0x13`.
  The roles of the three incoming heads remain inside the graph enumeration,
  not proved by the earlier literal-offset candidate query.
  Tried: the selector-initializer and stack reading is already recorded in
  FND-PARTY-054, with FND-CONFIG-150's producer qualifications and
  FND-PARTY-218's corrected field provenance. Reconnecting that reading to
  FND-PARTY-217 supplies no new head-role distinction and ends in the same
  place. Split head mutations into Q-PARTY-086 and retain root/storage
  admission in Q-PARTY-085 rather than repeating the enumeration query.
  Tried: FND-PARTY-221 binds the reviewed character-recipient transfer to
  target graph entry zero and field four, the shipped kind-2 word at +0C.
  That route's field-selection question is answered; Settles it retains
  the other two heads' roles, other screen/actions and complete meaning,
  not rediscovery of this local target argument.
- Q-PARTY-086. FMT-PARTY-006: Which character-screen actions change a kind-2
  object's field-16, field-17 or field-4 head, and how do they preserve or
  replace the attached chains? Settles it: remaining action-handler bindings,
  admitted current-object/head identity beyond the reviewed recipient request,
  complete list detach/append/replacement effects,
  failure continuation and the later screen consumer. Keep this separate
  from assigning gameplay labels to the heads (Q-PARTY-082) and graph/root
  storage admission (Q-PARTY-085).
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
- Q-PARTY-083. FMT-PARTY-006: What do a 23-byte record's attached handle at
  `0x08` and the records linked through it represent, and how are they used?
  Settles it: direct field-4 or offset-8 readers/writers for kind 1, including
  traversal of their targets and the screen/actions consuming them. Tried:
  FND-PARTY-052 gives the three type-2 chunks attaching to earlier kind-1
  chunks; FND-PARTY-196 gives their shipped values; FND-PARTY-216 confirms
  field 4 maps to offset 8 but establishes no purpose of that reference.
  Blocks: slice 2.
- Q-PARTY-084. FMT-PARTY-006: Does the quantity layout reading cover every
  admitted count producer and consumer, with field/table bindings preserved?
  Settles it: all kind-1 field-1 and direct offset-2 readers/writers, descriptor
  and record-table population/stability, and placement's request admission,
  signed arithmetic, aliases, callee returns and status consumption. Tried:
  FND-PARTY-216 locates the shipped field-1 descriptor and placement's full
  versus partial split branches; it leaves those complete-reading obligations
  unproved. Blocks: complete reading of FMT-PARTY-006, not its remaining fields.
- Q-PARTY-085. FMT-PARTY-006, SCR-UI-008: Which physical inventory slots do the
  position codes name, and is the position projection complete for all admitted
  records? Settles it: remaining position-byte writers beyond the reviewed
  field-ten request, remaining source coordinate/icon consumers beyond the
  26 main origins and the reviewed dimension-dependent draw requests,
  actual loaded-coordinate mutations and backend rendering effects,
  graph and current-object producers, map capacity and direction-flag provenance,
  parent traversal termination, mode flags and 189 `+146B`'s recursive effects.
  Tried: FND-PARTY-217 reads byte `0x13`'s signed projection and the main draw
  loop; it leaves those position labels and complete contracts unproved.
  Blocks: complete inventory positioning, not other attached-record fields.
  Tried: FND-PARTY-220 answers the local first-free recipient scan for codes
  14..25 and its backpack-full diagnostic route, corresponding to logical
  draw indices 18..29. FND-PARTY-221 answers the reviewed transfer's field-ten
  request at +13 and the runtime-width dependency. Settles it retains exact
  physical rectangles, other codes/writers and full admitted graph/storage/
  child contracts, not those local scan/request calculations.
  Tried: FND-PARTY-222 binds backpack codes 14..25 to APFM 11230..11241
  and their twelve shipped origins, with column-first order. Those source
  placement/identifier obligations are removed from Settles it; other codes,
  actual loaded rectangles and all image, child and state admission remain.
  Tried: FND-PARTY-223 supplies the other fourteen main source origins and
  identifier mapping, removing that remaining main-layout table from Settles
  it. FND-PARTY-224 identifies the local cached BMP request; its full cache
  lifecycle is Q-PARTY-087. Other draw paths, actual loaded coordinates,
  image/backend effects, state admission and all other writers remain.
  Tried: FND-PARTY-226 maps the shared tail's signed dimension gate,
  centered versus temporary-view request, FFFF exit and conditional release
  request. Those local placement/cleanup branches are removed from Settles
  it; loaded coordinates, admitted buffers, child preservation/backend effects
  and all other state/record/writer obligations remain.
- Q-PARTY-005. FMT-PARTY-004, FMT-PARTY-005: When do `30B0:00C1` and overlay 202 `+0C82` clear
  bit 0 of `PSST` bytes, what does the byte at `51F1:07E2 + 3 * power` hold, and does anything
  write a slot's 15-byte `SPST` entry at `4D62:039C` without naming that address? Settles it:
  `2D40:197A` and its caller `2AAD:0000`, the callers of the routine holding overlay 202 `+0C82`,
  the readers of the power table at `51F1:07E1`, and a search for block copies and reads into
  segment `4D62` (segment word `0x0320`) at computed offsets. Tried:
  FND-PARTY-104 reads the level-gain windows, which show `SPST` as the known spells and bits 1 to
  7 of `PSST` as power ranks, and keep bit 0; FND-PARTY-203 shows the load writes only the
  resource's own length and that no shipped character with a 9-byte `SPST` can be loaded;
  FND-PARTY-209 lists every instruction naming `0x039C` and finds no clear of a whole entry;
  FND-PARTY-210 shows bit 0 of a `PSST` byte marks a power in effect, and FND-PARTY-211 that
  every power in effect ends at the end of a fight and of a rest.
  Blocks: slice 3.
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

## Live session

- Q-PARTY-059. BUG-PARTY-006: Does the game end with `Math Err` when a party druid of priest level
  13 to 15 casts the spell `DATA` 225 at a single-class companion above level 1 while a Shift key
  is held? Settles it: an owner capture, from a save with such a druid that has level 7 spells to
  cast, of the cast window at level 7, the target cursor on the companion with Shift held, and the
  screen after the companion fails its saving throw, repeated until one fails (FND-PARTY-109,
  FND-PARTY-110, FND-PARTY-111). Required parts: Send it input, Capture frames and sound (frames)
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

- Q-PARTY-092. FMT-PARTY-006, SCR-UI-008: Does any shipped script name kind-1 field six,
  the +0C bitmap-number cache, in a field-path operand? Settles it: the `GPL ` resources
  decoded instruction by instruction, with the field bytes and counts each instance passes to
  the interpreter routines 172C:284D (expression byte B1), 172C:2914 (assignment), 172C:13E0
  and 172C:084E, and the field and kind operands of overlay 204's setter at +18B5, checked
  against the handle kinds they reach. Tried: FND-PARTY-234 traces those routines' fields to
  script code bytes (a count of zero takes the setter's field from the high word of the
  assigned value); FND-PARTY-156 and FND-PARTY-233 trace overlay 204's field to script
  operands. A byte search cannot tell field bytes from other operands without the instruction
  layouts. Waiting on: Q-SCRIPT-004, the layout of every script instruction, in the SCRIPT
  area. Blocks: complete reading of FMT-PARTY-006.
- Q-PARTY-050. RULE-PARTY-013: Does any trigger record's second entry point that can run during a
  fight hold script opcode `0x24`, which makes the slot in `4C13:0369` the current record through
  overlay 190 `+36D2` (FND-PARTY-097, FND-PARTY-098)? Settles it: the `GPL ` resources and offsets
  the trigger records name, decoded instruction by instruction and searched for opcode `0x24`.
  Blocks: slice 2. Tried: FND-PARTY-098 reads the conditions under which `2D40:207E` runs the
  entry point; a byte search for `0x24` would also match parameter bytes, so it cannot settle
  the question without the instruction layouts. Waiting on: Q-SCRIPT-004, the layout of every
  script instruction, in the SCRIPT area.
- Q-PARTY-051. FMT-PARTY-001, BUG-PARTY-006: Does any script call function 23 of opcode `0x22` with the effect
  code 59, which runs the level drain on a party slot, and on which slots does overlay 204 `+1A68`
  apply it (FND-PARTY-108), or call function 46 with 9999 as its second argument and a fourth
  argument whose remainder by 1000 is 104 or 225, which casts that spell (FND-PARTY-122)? Settles
  it: the `GPL ` resources decoded instruction by instruction and searched for opcode `0x22` with
  function 23 and code 59 or function 46 with those arguments, and a reading of `+1A68` and of
  where function 46's cast lands. Blocks: nothing yet. Tried: FND-PARTY-108 and FND-PARTY-122
  trace the code and the spell to the script's arguments; a byte search cannot separate
  parameters without the instruction layouts. Waiting on: Q-SCRIPT-004, the
  layout of every script instruction, in the SCRIPT area.
