# SOUND

Next ID: Q-SOUND-011

## Static

- Q-SOUND-002. FMT-SOUND-001, RULE-SOUND-001, RULE-SOUND-002, RULE-VIDEO-001: How does the sound library play a
  voice file or a `BVOC` resource: at what rate and volume, through which driver, and what do the
  library routines the two rules call check, play and stop? Settles it: `4611:0177`,
  `4654:04FE` with its arguments 6,000 and 6,001, `4611:0051`, `4611:03A5`, `49E9:00FD`,
  `49E9:0142`, `38FF:05B5` and `56BD:0034`, and the volume routine `45E1:0097` that the startup
  passes `unk_03` of `PREF` to. Tried: searches of both executables for a VOC signature, a `.VOC`
  extension, a helper launch and a BIOS wait (FND-SOUND-003, FND-SOUND-005), the helper's port
  output (FND-SOUND-004), and the effect and speech routines (FND-SOUND-007, FND-SOUND-008), which
  show which files play but not how. Blocks: slice 7.
- Q-SOUND-003. RULE-SOUND-001, RULE-SOUND-002, RULE-VIDEO-001: Which effects do the 19 other callers of the
  sound-effect routine play, and when; which lines do overlays 187 and 204 speak; and what are
  `g_57E0_0D9C`, `g_57E0_6554`, `g_57E0_14E8`, `g_57E0_4263` and `g_57E0_4275` for? Settles it:
  the callers FND-SOUND-007 and FND-SOUND-008 list, and the writers of those globals. Blocks:
  slice 7.
- Q-SOUND-004. RULE-SOUND-003, FMT-SOUND-002, RULE-VIDEO-002: What chooses the music outside combat after
  startup, the 18 mode-2 songs of `DJ.DAT` that the selector never takes, and how does a disc
  track or an FM song start and end? Settles it: the writers of `music_mode` through a pointer or
  a register, the routine at `2660:0250` and the requests `2660:035E` sends, the FM branch of
  `4A32:0011`, `4ABF:01E5`, `4A32:0185`, and the stores to `current_region` at `277B:03C1` and in
  overlay 187. Tried: a search for stores to `music_mode`, which finds only `2834:000C` and the two
  speech calls of `2834:0001` (FND-SOUND-012). Blocks: slice 7.
- Q-SOUND-005. RULE-SOUND-003: Does the game ever play a `PLYL` playlist or the `CSEQ` sequence,
  and through which code? Settles it: a caller of `2660:0004`, a writer of the far pointers at
  `DS:3496` and `DS:349A` that `2660:01AF` sets, and the resource lookups of the FM or MIDI branch
  of `4A32:0011`. Tried: searches of `DSUN.EXE` for the tags and for far calls to the playlist
  routines, which find none (FND-SOUND-014, FND-SOUND-015). Blocks: none.

- Q-SOUND-007. FMT-SOUND-004: Which remaining setters/callers and state
  changes affect mode/current settings through actual driver use?
  Settles it: remaining mode writers and incoming settings-copy/pointer-setter
  paths beyond the recorded combined/separate/fallback routes, and current-record
  lifetime/aliases through later driver children/digital use, and remaining
  filename/resource loader paths.
  Tried: FND-SOUND-019 validates all nineteen common directory envelopes;
  their structure is removed from Settles it. FND-EXE-491/492 identify the
  two filename consumers and setup batch; FND-EXE-585/590 bind the shipped
  game selectors to ADV eleven/eight and their disc-file matches. Those
  known consumers/default matches do not settle the remaining producers.
  Tried: FND-SOUND-020 binds all four name tags to inline parsed fields
  and the complete local real/protected-mode output-copy alternatives.
  It also binds DS:E01B through the initial pointer setter to the conditional
  first name helper. These local producers are removed from Settles it;
  actual record/mode selection, other setters and later preservation remain.
  Tried: FND-SOUND-021 settles local header actions, combined/separate
  group-to-writer and fallback-index routes, label/record stride separation
  and exact FFFF caller gates. These local producers are removed from
  Settles it. Independently answerable dialog row/index/storage admission
  moves to Q-SOUND-010; preserve later state/callee and other-caller gaps here.
  Blocks: complete loader interpretation of FMT-SOUND-004.
- Q-SOUND-010. FMT-SOUND-004, FMT-CONFIG-002: What actual row/index,
  storage and continuation contract admits setup's list-choice result?
  Settles it: cardinality and index correspondence through 19CF:000D/009A,
  label/buffer/source spans, the row comparison's far-zero storage binding,
  initial index and navigation/completion return paths, and preservation
  across reached children. Tried: split from Q-SOUND-007 after FND-SOUND-021
  identifies the wrapper's unconditional first-label read, entry-only negative
  index clamp, layout FFFF path and normal current-index return. A source
  record count or the callers' exact FFFF checks do not settle those bounds.
  Tried: FND-SOUND-022 settles local cardinality/index arithmetic for the
  explicit small printable-label profile, including the real LF separators,
  rounded row storage and conditional retained-vector prefix. Those local
  computations are removed from Settles it for that profile. Actual profile
  producers/retention, vector state, formatter/allocator and child/frame
  admission remain; nonpositive, larger and other-label profiles stay open.
  Blocks: complete setup-selection interpretation of the named formats.
- Q-SOUND-009. RULE-SOUND-004: Does any computed or bulk write change the voice tables at
  `DS:290A..291E`? Settles it: a census of block copies, string stores and computed stores whose
  destination can fall in `DS:290A..291E`, with a positive control for each kind, or a reached
  writer. Tried: split from Q-RNG-014. FND-RNG-043's immediate search finds no instruction naming
  `2909`, `290A`, `2911` or `2912` other than the two table reads. Blocks: slices 2-7.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-SOUND-001. RULE-SOUND-001, RULE-SOUND-002, RULE-SOUND-003: Does the game audibly change when
  Music or Sound Effects is toggled at a stable screen, and is continuous audio present at the
  start screen and after START GAME? Settles it: the audio-presence live session. Tried: the
  static reading of the three rules, which shows that music starts at startup from the mode-1
  songs and that effects stop while `g_57E0_1435` is 0, but not which Preferences control sets
  which byte. Blocks: slice 7.

## Source

None.

## Blocked

None.
