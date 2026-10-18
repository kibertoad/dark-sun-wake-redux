# UI

Next ID: Q-UI-011

## Static

- Q-UI-001. FMT-UI-001, FMT-UI-002, FMT-UI-004, FMT-UI-005, SCR-UI-002, SCR-UI-005, SCR-UI-006, SCR-UI-008, SCR-UI-011, SCR-PARTY-003: What do the unnamed
  fields of the window, child, frame and edit box records do: the window flags `0x800` and
  `0x2000`, the bytes a window copies from a control, the frames' unnamed fields, and does the
  game draw a window's `image`? Settles it: for the two flags and the picture word, a copied,
  rebased or computed consumer traced from the loaded window, since the literal-operand
  censuses find none; for the other fields, a reading of the loaders under `39D1` and `3D72`
  and of the routines that read these offsets. Blocks: nothing yet.
  Tried: FND-UI-042 classifies all three raw literal +C2 memory-displacement
  candidates as interior decodes, with four reached width/height reads as
  positive controls. That literal-reader avenue is removed from Settles it;
  no unused-field or native background claim follows, and copied/computed
  routes plus the other control fields remain open.
  Tried: FND-UI-044 takes a census of every literal 19501 and 19502 and of
  the window-number argument at all 31 calls of the window opener: the
  game-entry routine opens `WIND/19501`, and two overlay 201 routines open
  `WIND/19502`. What the two windows are for is removed from Settles it;
  the screen of the second 19502 opener moves to Q-UI-006.
  Tried: FND-UI-045 takes a census of every operand with displacement `0x9E`
  or `0x9F` (and `0x9C`/`0x9D`, and rebasing by `0x9E`/`0x9F`), with the
  dispatcher's two tests as controls. The low five bits are a state the
  window routines set, `0x40` and `0x80` are set from the children and admit
  the key searches, and `0x4000` gates a release on removal: those bits are
  removed from Settles it. No literal operand reads or writes `0x800` or
  `0x2000`; their copied or computed consumers remain. The event-mask bits
  move to Q-UI-007 and the button caption layout to Q-UI-008.
- Q-UI-007. FMT-UI-003, FMT-UI-004, FMT-UI-005, RULE-UI-001, SCR-UI-005, SCR-PARTY-003: What does each bit of a
  control's `event_mask` stand for (the frames' shipped values include 486 and 494), which
  callers pass which requested bits to the frame dispatcher, when does the game change a mask,
  and what does a frame keep in `unk_5A` while it runs? Settles it: a reading of the frame and
  child dispatchers' callers and of every writer of the `event_mask` words of loaded `BUTN`,
  `APFM` and `EBOX` records, beyond the setter FND-UI-007 reads. Blocks: nothing yet.
  Tried: FND-COMBAT-039 and FND-COMBAT-040 map the effect window's callback
  branches by event number and its frames' masks `0x1E6` and `0xE6`, and
  FND-UI-007 reads the mask setter `3F96:02F8`; none assigns an input meaning
  to a mask bit.
- Q-UI-008. FMT-UI-003: Which writers and layout flags decide a button's caption position words
  `+60` and `+62`, and what do `unk_0D` and the remaining `unk_0E` flags do? Settles it: a
  reading of the button constructor `3EBE:0008`'s nonzero-tail branch that writes `+60` and
  `+62` with its flag and mask selectors, of the other writers of the button's words `+0C` and
  `+0E`, and of the font table state its metric helper reads. Blocks: nothing yet.
  Tried: FND-UI-043 maps the button tail to declared-count temporary copies,
  appended zero, metric-word requests and a positioned string directive using
  loaded words +60/+62. The tail's basic caption role is removed from Settles
  it; complete layout flags, writers, admitted local/source storage, DS/SS,
  font tables and native backend outcomes remain. FND-UI-045 adds that
  registration sets bit `0x2000` of the loaded word `+0C` when the window's
  word `+A4` has bit 2 and bit 1 of the button's word `+0E` is clear.
- Q-UI-002. SCR-UI-001 to SCR-UI-012, FMT-UI-003, FMT-UI-005, SCR-UI-002, SCR-UI-003, SCR-UI-004, SCR-UI-005, SCR-UI-006, SCR-UI-007, SCR-UI-008, SCR-UI-009, SCR-UI-010, SCR-UI-011, SCR-PARTY-003: Which code opens each screen and handles each of its
  controls, and which icon frame shows a pointed-at, pressed or unavailable control? No routine
  names the controls' numbers as constants (FND-UI-012). Settles it: a reading of the handlers
  that a window's `after_children` and `before_children` pointers and a frame's handler at
  `0x62` point to, starting from `3D72:0515` and `3D72:0EB8`, and of overlay code.
  Tried: overlay 203's Preferences hover and click routines identify its button
  actions (FND-UI-034, FND-CONFIG-010), but do not establish native frame
  selection. Overlay 194's opener and event-2 callback dispatch the four
  start-window button IDs to distinct branches (FND-UI-035), but their callee
  effects and the remaining screens are unread. Blocks: nothing yet.
  Tried: FND-PARTY-217 links the inventory's `WIND/13500` request to overlay
  189's handler and redraw projection, without establishing all event branches
  or native control states; position geometry is separated into Q-PARTY-085.
  Tried: FND-UI-041 supersedes FND-UI-009's incorrect direct-caller
  absence claims, preserving callback field/order evidence and qualifying
  the effect-window setup's before/after targets and icon-builder gate.
  These target/handoff obligations are answered; complete screen routes,
  callback effects, loaded graph admission and native states remain.
  Tried: FND-PARTY-232 maps the class-selection window's WIND/17502
  request, callback, source button IDs, closure flags and local event-loop/
  close-pointer branches. Those local mappings are answered; remaining
  setup/cleanup children, admitted loaded state and native frame/input
  behavior are not established by the source geometry.
- Q-UI-003. SCR-UI-002, SCR-UI-005, SCR-UI-006, SCR-UI-007, SCR-UI-011, FMT-UI-001, SCR-UI-001, SCR-UI-003, SCR-PARTY-003, SCR-UI-022: Where does the game
  place each window, given that the resources hold no screen position? Settles it: a reading of
  the code that writes a window's runtime `unk_96` and `unk_98`, or captures of each screen.
  Blocks: nothing yet.
- Q-UI-005. SCR-UI-013 to SCR-UI-023, SCR-UI-014, SCR-UI-015, SCR-UI-016, SCR-UI-017, SCR-UI-018, SCR-UI-019, SCR-UI-020, SCR-UI-021, SCR-UI-022: Which resources, native widgets and handlers produce the
  manual's load, save, context, training, quick-cast, item, store, exit, map and About views? Settles
  it: focused readings of the resource graph and the code that opens, draws and handles each view,
  checked against owner captures for presentation the code cannot decide. Blocks: Survey screen
  coverage is recorded, but slices 2, 3 and 5 need these details. Tried:
  FND-UI-036 identifies the shared ten-row `WIND/18500` graph and the LOAD/SAVE
  mode image changes. FND-UI-037 identifies its event-2 row, action and EXIT
  branches. FND-SAVE-007 identifies the row description source. Native
  chrome, placement and further handlers remain open.
  Tried: FND-UI-046 identifies the overhead map's window, pictures, marks,
  handler and openers; the map is removed from Settles it, apart from its
  mark drawing (Q-UI-009) and placement (Q-UI-003).
- Q-UI-009. SCR-UI-022: What does an overhead-map mark look like? Settles it: a reading of
  `1BF3:2AF4`, which the map calls with 1, a point and the mark value 1, 11, 16 or 100, and of
  overlay 182 `+059C`, which the map calls with 19101 and 1 after the marks, with the palette
  entries the mark values select. Blocks: nothing yet.
- Q-UI-010. SCR-UI-022: Which code sets the byte at `57E0:4459` that picks the overhead map's
  picture within a region, and to which values? Settles it: a census of stores to `57E0:4459`
  (FND-PARTY-021 clears it, FND-SAVE-004 saves it) with the value each stores. Blocks: nothing
  yet.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-UI-004. SCR-UI-001, SCR-UI-002, SCR-UI-004, SCR-UI-005, SCR-UI-008 to SCR-UI-012, SCR-UI-003, SCR-UI-009, SCR-UI-010, SCR-UI-011: What native states and pixels are shown? Captures
  of the screens no capture shows yet (party creation, character generation with both lists, a
  Look panel for a friendly target, a notice being dismissed), of each
  button pointed at and pressed, and of the inventory screen's colours. Settles it: a live session
  with DOSBox screenshots at each step. Blocks: nothing yet.

## Source

None.

## Blocked

None.
