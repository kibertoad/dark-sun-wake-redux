# MAGIC

Next ID: Q-MAGIC-006

## Static

- Q-MAGIC-001. FND-MAGIC-001, SCR-UI-009: What do the darker icons and the lower panel of the Use
  screen show, which group of spells or powers does the screen open on, and how does it pick the
  captions it shows? Settles it: the code that draws the Use screen's icons and caption boxes.
  Blocks: slice 5.
- Q-MAGIC-002. RULE-MAGIC-001: How does the game count the spells a caster can cast: which
  progression a druid uses, whether the Wisdom bonus reaches spell levels the cleric's level does
  not, how a multi-class or dual-class caster's spells add up, and what the ranger casting level
  changes? Settles it: the code that decides whether a caster has a spell of a level left, read
  with the tables it indexes. Blocks: slice 5.
- Q-MAGIC-003. RULE-MAGIC-002: Which cleric spells does the game offer a cleric, a druid and a
  ranger, and where does it keep a spell's sphere and level and a character's sphere? Settles it:
  the spell definitions the game loads and the code that builds a caster's spell list for the Use
  screen. Blocks: slice 5.
- Q-MAGIC-004. RULE-MAGIC-003: How does the game make a psionic power check, how does it round
  half an odd initial cost, what does it do when a character has too few PSPs, how many PSPs does
  a character have at each level, and which powers can a character who is not a psionicist use?
  Settles it: the code that activates a psionic power and the code that sets a character's PSPs.
  Blocks: slice 5.
- Q-MAGIC-005. RULE-MAGIC-004: What does camping do besides restoring PSPs and spells: does it
  take game time, restore hit points, stop for enemies, and in which order does it cast cure
  spells, and does the game keep memorized spells? Settles it: the code behind the Look pointer on
  a fire ring. Blocks: slice 5.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
