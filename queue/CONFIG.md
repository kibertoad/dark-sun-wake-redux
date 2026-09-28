# CONFIG

Next ID: Q-CONFIG-009

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
  FND-CONFIG-025 corrects the resident `PREF` hit to a Preferences label;
  the direct tag uses found so far are in save/load. FND-CONFIG-026 bounds
  the literal difficulty-word writers to load, Preferences and the guarded
  overlay branch, without excluding an indirect or block write. FND-CONFIG-027
  bounds literal speech-gate writes to load, launcher and Preferences paths;
  three table-like raw hits are overlay fixup payloads, and indirect or block
  writes remain possible. FND-CONFIG-028 follows the Start Game button branch
  into a shared setup helper without finding a direct settings write there;
  its later calls resolve to overlay 187 and 182 entries, but their effects
  and later start paths remain unread. FND-CONFIG-029 bounds literal
  music-level request writes to Load Game and the Preferences-entry getter,
  without excluding indirect control. These readings
  do not identify new-game values, animation
  polarity, any synchronization beyond those paths or another music-level control. Blocks:
  slice 3.

- Q-CONFIG-007. RULE-CONFIG-005: Does another path impose an upper limit on
  the message-delay word, and does a new game replace the loaded-image value
  50? Settles it: a bounded reading of initialization and other direct,
  indirect and block writers. Tried: the
  Preferences hover and click branches (FND-UI-034, FND-CONFIG-010), the
  millisecond wait (FND-TIME-004), a raw search for direct references to
  `DS:26B7`, and a bounded reading of the overlay 172 wait gate
  (FND-CONFIG-011) identify the word's 100-ms scaling and six literal uses.
  FND-CONFIG-028 finds no direct message-delay write in the Start Game
  button branch or its shared setup helper. The new-game starting value and
  indirect or block writers remain unread.

- Q-CONFIG-008. RULE-CONFIG-005: Which text-message calls reach the overlay
  172 wait gate after acquiring `WIND/10501`? Settles it: bounded readings
  of resource acquisition, possible indirect or block writes to the
  window bounds, and relevant caller conditions; an original observation
  only for an outcome the code does not decide. Tried: FND-CONFIG-011 and
  FND-CONFIG-018 identify the pointer gate and `WIND/10501` setup;
  FND-CONFIG-030 through FND-CONFIG-033 bound callback, child and graphics
  registration. FND-CONFIG-034 and FND-CONFIG-038 trace the resident
  reader's search and failure branches. FND-CONFIG-039 identifies the
  startup resource archive; FND-CONFIG-037, FND-CONFIG-040 and
  FND-CONFIG-041 trace its active pointer, wraparound search mode and
  direct close sites. FND-CONFIG-059 maps close-all to a separate
  overlay 180 entry. FND-CONFIG-060 finds no literal far call or local
  near call to it; FND-CONFIG-061 identifies its exit-callback
  registration, while any other indirect callers remain unread.
  FND-CONFIG-064 bounds literal traversal-mode writers and readers. Indirect archive
  changes and I/O outcomes remain open. FND-CONFIG-065 shows that the
  shipped `WIND/10501` record passes the indexed range test and takes the
  `GFFI` lookup path. FND-CONFIG-066 bounds literal uses of the startup
  resource-archive handle to startup; FND-CONFIG-067 distinguishes that
  numeric handle from the internal active record pointer. FND-CONFIG-068
  bounds direct active-pointer writers and a guarded indirect growth path.
  FND-CONFIG-062 traces the
  startup archive-open failure to a termination request.
  FND-CONFIG-036 and FND-CONFIG-063 classify literal window-size
  references; the latter follows its two helper calls to mouse range
  services. Indirect or block writes remain open. FND-CONFIG-017
  identifies resident callers; FND-CONFIG-035 inventories 56 direct
  calls from 21 overlays. FND-CONFIG-042 through FND-CONFIG-058 give
  bounded local readings for all 56 distinct direct overlay call sites;
  FND-CONFIG-046 deepens the overlay 192 site already in FND-CONFIG-042.
  Shared sinks' full incoming paths, possible indirect callers, and live
  success through acquisition and registration are not established.

- Q-CONFIG-005. FMT-CONFIG-001: What does the game's sound library read from each field of
  `SOUND.CFG`, including its unexplained tail? Settles it: bounded readings
  of the remaining consumers of the whole-file buffer and the setup writer.
  Tried: comparison of the shipped file with `SOUND.INI` records names most
  fields but not the tail (FND-CONFIG-003). FND-CONFIG-019 shows the file
  entry loads bytes whole, without parsing fields; FND-CONFIG-020 identifies
  direct uses of `0x08`, `0x0A`, `0x14` and `0x32` in initialization.
  FND-CONFIG-021 adds the first four words as a group, another `0x08`
  comparison, and further `0x14` and `0x32` branches. The receiving
  routines' effects remain unread. FND-CONFIG-022 maps each tail
  output to an input-record offset or a literal 4, but the source fields'
  meanings remain open. FND-CONFIG-023 identifies bounded game-side reads
  of `0x34..0x3A`, including alternative `ADV ` resource numbers, but the
  remaining branch effects are still unresolved. FND-CONFIG-024 pairs each
  `ADV ` number with a separate ten-byte settings block and shows bit
  `0x01` of `0x14` gates the second pair; the device roles and input-record
  field meanings remain open.

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
