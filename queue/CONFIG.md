# CONFIG

Next ID: Q-CONFIG-007

## Static

- Q-CONFIG-002. FMT-CONFIG-003, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003: Which
  Preferences setting does each remaining field of `PREF/100` hold, and how do the button
  handlers change the saved volumes, on-off settings and difficulty? Settles it: the routines
  called after the overlay 192 load (FND-SAVE-005), the Preferences button handlers and the
  callers that initialize the settings. Tried: the load routine (FND-SAVE-005) and the renderer's
  references to `DS:143A`, which identify offset `0x00` as the saved difficulty-label index
  (FND-CONFIG-009); and the renderer's use of `DS:1437` for filmstrip button `16303`, which
  identifies offset `0x07` as animation control state without its polarity (FND-UI-033). Neither
  establishes button transitions or new-game defaults. Blocks: slice 3.

- Q-CONFIG-005. FMT-CONFIG-001: What does the game's sound library read from each field of
  `SOUND.CFG`, including its unexplained tail? Settles it: a bounded reading of the sound
  library's reader at `47B9:0236` (FND-CONFIG-005) and the consumers of its output. Tried:
  comparison of the shipped file with `SOUND.INI` records, which names most fields but not the
  tail (FND-CONFIG-003).

- Q-CONFIG-006. FMT-CONFIG-002: How does the setup program parse `SOUND.INI`, including unknown
  or malformed keys, and what do `CardGroup` and the chunk numbers mean? Settles it: the parser
  at `SOUND_DS.EXE` `1AF6:0BF9` (FND-CONFIG-004) and its consumers. The file layout alone does
  not determine parser behavior (FND-CONFIG-007).

- Q-CONFIG-004. FMT-CONFIG-005: What is the structure and role of `PATCH.RTP`? Settles it: a
  bounded inspection of the file and references from the installed setup programs. Blocks:
  Survey format coverage.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-CONFIG-001. SCR-UI-007, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003, FMT-CONFIG-003:
  What are the native Preferences defaults, the selected and unselected frames of each control,
  the volume endpoints and step counts, whether the difficulty stops at its ends, what `F6` toggles,
  the difficulty and description placement, and the About page's layout and dismissal? Settles it:
  the preferences live session. Tried: the executable's Preferences strings (FMT-TEXT-004) and the
  saved `PREF` resource (FND-CONFIG-001), which give labels and one set of saved values but no
  defaults, ranges or frame states. Blocks: slice 3.

## Source

None.

## Blocked

None.
