# COMBAT

Next ID: Q-COMBAT-009

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
- Q-COMBAT-008. RULE-COMBAT-009, FMT-COMBAT-003: Which effects does `fn_573B_002A` list for a
  combatant, in what order, what does `573B:0025` prepare, and what is the word after each effect
  name? Settles it: a reading of overlay 193 from the targets of the stubs `573B:0025` and
  `573B:002A`. Tried: none. Blocks: nothing yet.

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
