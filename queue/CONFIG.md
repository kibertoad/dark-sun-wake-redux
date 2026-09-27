# CONFIG

Next ID: Q-CONFIG-008

## Static

- Q-CONFIG-002. FMT-CONFIG-003, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004: What initializes the settings for a new game, does any path
  synchronize the saved speech gate and runtime voice button state, and can
  music volume be adjusted elsewhere? Settles it:
  bounded readings of the sound-library consumers, new-game initialization and voice
  synchronization paths. Tried: the save/load copies (FND-SAVE-004, FND-SAVE-005),
  renderer (FND-CONFIG-009, FND-UI-033), Preferences hover routine (FND-UI-034)
  and button dispatcher (FND-CONFIG-010) identify the difficulty, effect-volume,
  music/effects enable and animation-state fields and the ordinary button steps.
  FND-CONFIG-012 identifies byte `0x02` as the sound library's music-level
  request; FND-CONFIG-013 identifies byte `0x04` as the music bar's denominator.
  FND-CONFIG-014 shows that the launcher and Preferences button change the
  runtime voice gate while save/load carries the other gate, and playback
  checks both. FND-CONFIG-015 bounds the overlay 171 difficulty write to a
  record-field branch; its callers and inputs remain unread. Bounded mapped
  Ghidra queries found no recognized references to either that entry or its
  resident trampoline, which does not exclude indirect dispatch.
  FND-CONFIG-016 places the literal `PREF` tags in save/load and one
  untraced resident data site. These readings
  do not identify new-game values, animation
  polarity, any synchronization beyond those paths or another music-level control. Blocks:
  slice 3.

- Q-CONFIG-007. RULE-CONFIG-005: Does another path impose an upper limit on
  the message-delay word, does a new game replace the loaded-image value 50,
  and which text-message calls pass the overlay 172 wait gate? Settles it: a
  bounded reading of initialization, other writers and the wait-gate pointer's
  lifecycle. Tried: the
  Preferences hover and click branches (FND-UI-034, FND-CONFIG-010), the
  millisecond wait (FND-TIME-004), a raw search for direct references to
  `DS:26B7`, and a bounded reading of the overlay 172 wait gate
  (FND-CONFIG-011) identify the word's 100-ms scaling, six literal uses and
  the nonzero pointer condition. FND-CONFIG-017 identifies several resident
  text-message callers, with prior overlay findings showing more, but does
  not establish initialization, all indirect writers, the pointer's role or
  which calls reach the wait.

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

- Q-CONFIG-001. SCR-UI-007, RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003,
  RULE-CONFIG-004, RULE-CONFIG-005, FMT-CONFIG-003: What are the native
  Preferences defaults and control frames, the visible message-delay and effect-volume
  endpoints, what `F6` toggles, the difficulty and description placement, and the
  About page's layout and dismissal? Settles it: the preferences live session.
  Tried: the strings and saved resource (FMT-TEXT-004, FND-CONFIG-001),
  renderer (FND-CONFIG-009, FND-UI-033), hover routine (FND-UI-034) and click
  dispatcher (FND-CONFIG-010) establish labels and button transitions, but
  cannot establish new-game defaults or native drawn states. Blocks: slice 3.

## Source

None.

## Blocked

None.
