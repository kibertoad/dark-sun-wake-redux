# MAGIC

Next ID: Q-MAGIC-018

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
  ranger, where does it keep a spell's sphere and level and a character's sphere, and how does it
  treat a spell of several spheres? Settles it:
  the spell definitions the game loads and the code that builds a caster's spell list for the Use
  screen. Blocks: slice 5.
- Q-MAGIC-004. RULE-MAGIC-003: How many PSPs does a character have at each level, which powers
  can a character who is not a psionicist use, does the game keep the manual's costs, how does it
  price the powers the manual gives as Varies, and can a power with a zero maintenance cost be
  maintained? Settles it: the code that sets a character's PSPs, the values of `psionic_powers`
  against the manual's, and the code that keeps a power running from round to round.
  Tried: FND-RNG-046 reads the activation: RULE-MAGIC-014 takes the power check, the half cost
  of a failed check rounded down and the refusal for too few points, which contradict
  RULE-MAGIC-003's check. Blocks: slice 5.
- Q-MAGIC-005. RULE-MAGIC-004: What does camping do besides restoring PSPs and spells: does it
  take game time, restore hit points, stop for enemies, and in which order does it cast cure
  spells, and does the game keep memorized spells? Settles it: the code behind the Look pointer on
  a fire ring. Blocks: slice 5.
- Q-MAGIC-006. RULE-MAGIC-005: What do the two callers of the `DATA` dice formula do with its
  total and its maximum, and which level byte does each pass? Settles it: overlay 193 `+003C`'s
  use of the total it stores at `bp-0x14` and of the maximum at `bp-0x16` on the way to the hit
  routine, the routine around overlay 193 `+1431` and its use of both, and the writers of each
  caller's level byte. Tried: FND-RNG-035 reads the formula and names the two calls. Blocks:
  slices 2-7.
- Q-MAGIC-007. RULE-MAGIC-006: What do details byte `+0x12` 9 and byte `+0x1E` hold, and what
  does `2D40:06E1` do with the caster when `DATA` 306's test fails? Settles it: the writers of
  the two details bytes for party members and for creatures, and a reading of `2D40:06E1`.
  Tried: FND-RNG-035 reads the test and its one caller. Blocks: slices 2-7.
- Q-MAGIC-008. RULE-MAGIC-007, RULE-MAGIC-008, RULE-MAGIC-009, RULE-MAGIC-010: What do the hit
  routine's gate tests in overlay 197 stand for: combatant record byte `+0x0E` 13, details byte
  `+0x12` 8 and 9, the effect block bits they test, bit 0x100 of `+1760`'s mask, the hit routine's
  byte at `bp+0x14`, overlay 208 `+0885`'s value, `2D40:35EF`, codes 0x2E and 0x4F of overlay 193
  `+1AA4`, and `DATA` 307; and is overlay 197 `+314E` reached other than from `+0E8F`? Settles it:
  readings of the writers of those record bytes and block bits, of overlay 208 `+0885`, `2D40:35EF`
  and overlay 193 `+1AA4`, of the hit routine's callers' byte argument, and a computed-call search
  for `+314E`'s trampoline `575A:00C0`. Tried: FND-RNG-040 reads the gates and FND-PARTY-112 the
  tests before them. Blocks: slices 4-5.
- Q-MAGIC-009. RULE-MAGIC-009: What does overlay 179 `+1304` do with the 2d6 total and its other
  seven arguments, and what does `DATA` 308 do when the cast queue runs it? Settles it: a reading of
  `+1304` from entry to return and of overlay 193 `+003C` for a queued record of `DATA` 308.
  Tried: FND-RNG-040 reads the call and FND-PARTY-107 the queue. Blocks: slices 4-5.
- Q-MAGIC-010. RULE-MAGIC-011: What do overlay 172 `+0A73`, `+0D44` and `+1140` and overlay 211
  `+159B` and `+1C46` do with `class_11_check`'s result, which slots do they pass, and what do word
  `+0x0E` of a details record, `g_4C4F_00B2` and the block bits it tests stand for? Settles it:
  readings of the five callers around their calls and of the writers of those fields. Tried:
  FND-RNG-040 reads the check and lists its callers. Blocks: slices 4-5.
- Q-MAGIC-011. RULE-MAGIC-012, RULE-MAGIC-013: Which spells are the eighteen summoning `DATA`
  records and `DATA` 71, which creatures are the numbers 400 to 569, what do `g_4F49_0C3F` and
  `g_4C13_0369` hold, what do overlay 188 `+0AA8`, `31E0:0121`, `2D40:085B`, `2D40:06E1` and overlay
  197 `+32A1` do with the summoning's arguments, and which slots does overlay 208 `+0000` list for
  `DATA` 71? Settles it: the names of the `DATA` records and creature resources in `RESOURCE.GFF`,
  readings of those routines, and of the writers of the two globals. Tried: FND-RNG-040 reads the
  summoning and the pool. Blocks: slices 4-5.
- Q-MAGIC-012. RULE-MAGIC-014, RULE-MAGIC-015: Which values does `psionic_powers` hold for each
  power, where do they come from, and which score does each `score_code` name? Settles it: the
  writers of `51F1:07E0..51F1:0846` or the resource they load it from, and the readers of
  `fn_27E5_04A0`'s numbers. Tried: FND-RNG-046 reads the check and defence that use the table.
  Blocks: slice 5.
- Q-MAGIC-013. RULE-MAGIC-014: Which callers of the `DATA` dispatcher overlay 193 `+2290` pass a
  number from 235 to 268, with which word arguments and byte pointer, does any reach `use_power`'s
  path that applies an earlier check's result, and what does `fn_573B_007F` do with a psionic
  number? Settles it: the eleven far callers of `+2290` read for the psionic numbers they pass,
  and overlay 193 `+0000` read for numbers 235 to 268. Tried: FND-RNG-046 lists the callers.
  Blocks: slice 5.
- Q-MAGIC-014. RULE-MAGIC-015: Which `DATA` records reach overlay 197 `+1C52` with the type word
  0x400, and what do the defence's bytes, `+0x1D` of the combatant record and `+0x1E` of the
  details record that `fn_2D40_3E64` maps the target to, hold? Settles it: the hit routine's
  computation of the type word it passes at `+1289`, the records whose type gives 0x400, and the
  writers of the two bytes. Tried: FND-RNG-046 reads the call for 0x400 and FND-PARTY-112
  the rest of `+1C52`. Blocks: slice 5.
- Q-MAGIC-015. RULE-MAGIC-014: What blocks or contests a psionic use: which effects are the codes
  58 and 0x43 of `effect_entries` and what puts them on a slot, what `g_4C4F_00B2` stands for, and
  which level `fn_27E5_0154` gives powers 20 and 21? Settles it: the writers of the code byte
  `4E71:0253 + 10 * e` that store 58 or 0x43 and the `DATA` records behind them, the writers of
  `g_4C4F_00B2`, and a reading of `27E5:0154` past FND-PARTY-081's. Tried: FND-RNG-046 reads the scan and the test that
  read them. Blocks: slice 5.
- Q-MAGIC-016. RULE-MAGIC-016, RULE-MAGIC-017, RULE-MAGIC-019, RULE-MAGIC-020: What values reach the
  hit routine's bytes `a` and `g` where its callers pass on their own arguments or locals, and
  what do the actor's byte `+0x15` and `a` stand for? Settles it: the writers of overlay 193
  `+2290`'s byte argument at `bp+0xC`, overlay 197 `+111A`'s at `bp+0x14`, overlay 198 `+0000`'s
  at `bp+0xE`, overlay 204 `+18CD`'s local at `bp-6` and `+0CBA`'s at `bp-6` and `bp-8`, the
  results of overlay 176 `+076C`, overlay 177 `+07AF` and overlay 193 `+1A2D`, the details record
  overlay 173 `+2560` reads byte `+0x1E` of, and the writers of the object slot byte at
  `DS:67D0 + 37 * k`. Tried: split from Q-RNG-021. FND-RNG-051 reads the direct callers and the
  cast queue, and finds the constants 1, 6, 11 and 0 at the calls that pass them. Blocks: slices
  2-7.
- Q-MAGIC-017. RULE-MAGIC-023, RULE-RNG-001: What do a cast's sampled area points and amount
  become, and what shapes the area? Settles it: overlay 193 `+20DA` and `+07F1` read from their
  entries with the arguments overlay 193 `+0617` passes, including whether and when they reach the
  draws their walks find through overlay 193 `+12A8`, and the returns of overlay 208 `+0933` and
  `+03BA` and the helpers overlay 179 `+08B5`, `+09CE`, `+0959`, `+07BD` and `2D40:382F`.
  Tried: split from Q-RNG-009. FND-RNG-052 reads `+0617`, overlay 179's wrapper and both
  samplers, the amount's dice and the 112 effect rows, and finds the helpers draw nothing.
  Blocks: slices 5-7.

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
