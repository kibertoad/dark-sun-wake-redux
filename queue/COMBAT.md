# COMBAT

Next ID: Q-COMBAT-013

## Static

- Q-COMBAT-002. FMT-COMBAT-001, FMT-COMBAT-002, SCR-COMBAT-001, RULE-AI-001: What do the unknown fields of
  the two combatant records hold, how many records does each table have, and how does
  `2D40:3E64` map `turn_combatant` to a record of each? Why does the panel show question marks
  only above 4? Settles it: a reading of `2D40:3E64` and of the code that fills the tables at
  the far pointers `57E0:19C9` and `57E0:19C5`. Tried: the panel routine and the party buttons
  (FND-COMBAT-022, FND-COMBAT-023). Blocks: slice 4.
- Q-COMBAT-003. RULE-COMBAT-008: Which values do `combat_state` and the word at `57E0:0DAB` take,
  and which code sets each? Settles it: the seven direct stores to `combat_state` of
  FND-COMBAT-023, read in their routines, and the writers of `57E0:0DAB`. Tried: an immediate
  store search (FND-COMBAT-023) and Ghidra's references to `57E0:0DAB` (FND-COMBAT-011). Blocks:
  slice 4.
- Q-COMBAT-004. RULE-COMBAT-004: What does the routine behind the stub `5682:004D` of overlay
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
- Q-COMBAT-006. RULE-COMBAT-004, RULE-COMBAT-005: What do `fn_5671_0093` with 1 and 2 and
  `fn_5671_0098` with 0 do, and what do `fn_182_19F8`, `fn_3D72_0D83`, `fn_28C9_000A`,
  `fn_2C5F_018C` and the menu `fn_566A_0025` do? Settles it: a reading of overlay 173 from its
  code offsets `0x2FE9` and `0x306B`, and of the other routines. Tried: none. Blocks: slice 4.
- Q-COMBAT-009. RULE-COMBAT-009: What admitted effect-row and bitmap state
  supplies the status selector's category-zero list? Settles it: producers,
  count/capacity, object sorting and boundary/sentinel identity of the
  ten-byte row table and admitted ID domain. Complete the
  reading without inferring valid state from the local row loop alone.
  FND-COMBAT-031 answers the common insertion's local assignments,
  conditional owner-group placement and count/store ordering; its inputs,
  initialization, other mutation sites and continued identity remain.
  A sentinel owner value is not required by the local count stop, but
  backing storage for its preceding owner read remains to be admitted.
  Tried: FND-COMBAT-029 settles the local preparation, category filtering,
  duplicate-preserving row order and status caller's sort clamp; it retains
  explicit producer, boundary-read, signed-ID and alias prerequisites.
  Tried: FND-COMBAT-031 qualifies the two exported insertion-related roots,
  their distinct gates, owner-local key search and upward overlapping move.
  Those local producer/order obligations are answered; the initial zero
  word and wrapper's exact-192 test do not prove a global count/ID bound.
  Other count writers/callers, callback/removal effects, table initialization
  and native buffer/segment identity remain. Follow-up to closed
  Q-COMBAT-008. Blocks: established status-effect selection.
  Tried: FND-COMBAT-032 answers the local key/index/code removal branches,
  two decrement sites and retained-row order. Their downward moves read
  the entire boundary row, so the backing obligation includes ten bytes
  at the old count, not just an owner word. Local removal compaction and
  API distinctions are settled; handler/request effects, admitted callers,
  other writers and initialization remain.
  Tried: FND-COMBAT-033 answers the local owner-relocation count pair,
  saved fields and conditional ordering/progress. All encoded count
  mutation candidates now have local readings or remain overlapping
  decode candidates; this does not close computed/aliased/bulk writers
  or restoration. The row loop uses a lower-bound threshold, not owner
  equality; admitted transfer callers remain required. Independently
  answerable bitmap producers and selector storage admission are split
  to Q-COMBAT-011 and Q-COMBAT-012.
- Q-COMBAT-010. FMT-COMBAT-003: Which complete resource/consumer contract
  admits the effect record's ICON number? Settles it: reached field-consumer
  coverage, admitted active archive/request route and runtime outcomes for
  the shipped nonzero IDs; resource-pointer/error divergence and validity
  before later display, and selected frames;
  distinguish the local no-icon zero branch from failed resource requests
  and later display. Tried: FND-COMBAT-029 identifies an exported consumer's
  zero exclusion and zero-extended ICON request, settling the field's local
  purpose. Complete lookup and other consumers remain. Follow-up to closed
  Q-COMBAT-008. Tried: FND-COMBAT-030 matches all 107 nonzero references
  to 97 installed ICON resources, including their frame counts/dimensions
  and shared IDs. That shipped reference-existence/metadata obligation is
  answered. Its connected wrapper reading retains the distinct reader
  error and caller pointer test; active archive, failure continuation,
  buffer validity and display/frame selection remain.
  Blocks: established effect-icon format contract.
- Q-COMBAT-011. RULE-COMBAT-009: What initializes and subsequently writes
  the category bitmap and prepared membership bitmap used by the status
  selector? Settles it: qualified writer/initializer coverage, admitted
  ID/bit domains and lifetimes for both bitmaps, including restoration
  or bulk writes; distinguish static classification from per-owner state.
  Tried: split from Q-COMBAT-009 after FND-COMBAT-029 identifies the
  shipped classifier and local membership preparation. Local bit tests
  and initial bytes alone do not admit all later bitmap contents.
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

## Source

None.

## Blocked

None.
