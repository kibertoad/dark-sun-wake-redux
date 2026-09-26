# CONFIG

Next ID: Q-CONFIG-005

## Static

- Q-CONFIG-002. FMT-CONFIG-001, FMT-CONFIG-002, FMT-CONFIG-003, RULE-CONFIG-001, RULE-CONFIG-002,
  RULE-CONFIG-003: Which setting does each byte of `PREF/100` hold, where do the game's volume,
  on-off and difficulty settings live, what does the sound library read from each field of
  `SOUND.CFG`, and how does the setup program parse `SOUND.INI`? Settles it: the routines the load
  routine of overlay 192 calls with the settings (FND-SAVE-005), the sound library's reader at
  `47B9:0236` (FND-CONFIG-005), the code that handles the Preferences buttons, and the parser at
  `SOUND_DS.EXE` `1AF6:0BF9` (FND-CONFIG-004). Tried: the values of the shipped `SOUND.CFG` against
  the `SOUND.INI` records, which name most fields but not the tail (FND-CONFIG-003). Blocks:
  slice 3.

- Q-CONFIG-003. FMT-CONFIG-004: What structure does `game.ins` use to map installed Ogg files to
  disc tracks, and which part of the installed DOSBox setup reads it? Settles it: a bounded reading
  of the file and its installed consumer. Blocks: Survey format coverage.
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
