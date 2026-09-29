# CONFIG

Next ID: Q-CONFIG-011

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

- Q-CONFIG-008. RULE-CONFIG-005: Which caller paths enter overlay 172's
  shared message routine? Settles it: complete incoming-path readings for
  the shared sinks and possible resident or computed-pointer calls,
  including the conditions and text pointers that reach the routine.
  Tried: FND-CONFIG-017 identifies resident calls; FND-CONFIG-035 inventories
  56 direct calls from 21 overlays. FND-CONFIG-042 through FND-CONFIG-058
  give bounded local readings for all 56 sites, with FND-CONFIG-046
  deepening the Save Game call. FND-CONFIG-070 rules out an address-taking
  fixup to this entry from another overlay; FND-UI-037 and FND-SAVE-010
  trace the Save Game UI and keyboard paths. FND-CONFIG-071 traces the
  guarded save-capacity call from startup. FND-CONFIG-072 confirms the four
  direct resident message calls exhaust MZ relocations to that entry.
  FND-CONFIG-073 adds five internal calls from overlay 172's `0034` and
  `0043` routines, including a direct resident path into `0034`.
  FND-CONFIG-074 traces the `0043` callback through registration on seven
  frame identifiers and the frame dispatch path; FND-CONFIG-075 finds
  only six in the shipped window graph. The complete event sequence into
  that callback, other shared sinks' incoming paths, and computed or
  unrelocated pointer calls remain open. FND-CONFIG-076 closes the direct
  incoming routes to overlay 187's four save and cinematic message sites.
  FND-CONFIG-077 traces overlay 204's two rest message sites to an overlay
  182 handler and script request one; their upstream live inputs remain open.
  FND-CONFIG-078 traces overlay 171's list messages to a choice branch and
  the `WIND/18501` callback. FND-CONFIG-079 identifies the guarded resident
  event-dispatch route into that callback. FND-CONFIG-080 identifies two
  event-record discriminators for the callback's message branch.
  FND-CONFIG-081 traces a conditional pointer-hit producer for the first;
  physical input mapping and live state remain open. FND-CONFIG-082 shows
  that a keyboard packet can carry the second discriminator. FND-CONFIG-083
  traces the list callback's global registration and the event-six fallback
  route into it. FND-CONFIG-084 traces one temporary replacement and
  restoration path. FND-CONFIG-085 identifies six overlay 182 setter calls
  that install a resident key callback or zero; their timing relative to the
  list remains open. FND-CONFIG-086 classifies the remaining twelve direct
  setter sites, including one saved-pointer restoration. Computed or indirect
  calls, registration order and live state remain open. FND-CONFIG-087
  shows overlay 209's no-other-classes message skips its callback
  registration and bounds event routes that can restore its prior pointer.
  FND-CONFIG-088 traces an overlay 213 window callback's guarded event-two
  route into its shared message sink; its live event and record inputs remain
  open. FND-CONFIG-089 finds a matching shipped button and conditional
  pointer-hit route in one of its windows, but physical input and live
  window state remain open. FND-CONFIG-090 traces the callback's unsigned
  event-bit threshold to the queued mouse packet; the bit meanings and
  remaining live gates remain open. FND-CONFIG-092 separates overlay 175's
  setup from a registered frame handler whose value-32 branch enters the
  helper with two conditional message sites. FND-CONFIG-093 supplies all
  six matching shipped frames and their enabled value-32 mask.
  FND-CONFIG-094 traces mouse-packet bit 4 through the resident APFM
  dispatcher into that handler. FND-CONFIG-095 traces hit-selection
  refresh and earlier helper gates. FND-CONFIG-096 shows the intervening
  window return is ignored and its child branches skip the shipped APFM
  graph. FND-CONFIG-097 and FND-CONFIG-098 bound the selection and
  old-window helpers by their two-tag tables. FND-CONFIG-099 bounds the
  position/region callees' fixed write targets. Physical input mapping,
  other prior control types and later state changes remain open.

- Q-CONFIG-010. RULE-CONFIG-005: Can later state changes prevent
  `WIND/10501` acquisition or registration during an ordinary message?
  Settles it: bounded readings of indirect or block writes to display
  bounds, archive options and traversal mode, plus indirect archive-close
  and cleanup callers. Tried: FND-CONFIG-018 and FND-CONFIG-030 through
  FND-CONFIG-034 bound window setup and registration. FND-CONFIG-036
  through FND-CONFIG-041, FND-CONFIG-059 through FND-CONFIG-069 trace
  literal bounds and archive references, startup, lookup, direct closes
  and cleanup registration. No ordinary-state failure is established;
  indirect changes and callers remain unread.

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

- Q-CONFIG-009. RULE-CONFIG-005: Does the Save Game completion message pass
  the `WIND/10501` gate and wait in the owner's installed GOG build, and
  does changing Message Delay change its visible duration? Settles it:
  the bounded two-setting Save Game comparison in
  `docs/live-sessions/preferences.md`. Tried: FND-CONFIG-046 locates the
  success branch's direct message call; FND-CONFIG-018, FND-CONFIG-030
  through FND-CONFIG-034 and FND-CONFIG-065 identify the acquisition
  gates and shipped record. The code does not decide the live I/O outcome
  or observed duration. Blocks: slice 3 message timing validation.

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
