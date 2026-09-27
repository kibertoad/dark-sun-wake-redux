# UI

Next ID: Q-UI-006

## Static

- Q-UI-001. FMT-UI-001, FMT-UI-003, FMT-UI-004, FMT-UI-005, RULE-UI-001: What do the unnamed
  fields and the other mask values of the four control formats do: the window flags beyond 4 and
  `0x100`, the frames' masks such as 486 and 494, a button's tail, the bytes a window copies from
  a control, whether the game draws a window's `image`, and what `WIND/19501` and `WIND/19502`
  are for? Settles it: a reading of the loaders under `39D1` and `3D72` and of the routines that
  read these offsets. Blocks: nothing yet.
- Q-UI-002. SCR-UI-001 to SCR-UI-012: Which code opens each screen and handles each of its
  controls, and which icon frame shows a pointed-at, pressed or unavailable control? No routine
  names the controls' numbers as constants (FND-UI-012). Settles it: a reading of the handlers
  that a window's `after_children` and `before_children` pointers and a frame's handler at
  `0x62` point to, starting from `3D72:0515` and `3D72:0EB8`, and of overlay code.
  Tried: overlay 203's Preferences hover and click routines identify its button
  actions (FND-UI-034, FND-CONFIG-010), but do not establish native frame
  selection. Overlay 194's opener and event-2 callback dispatch the four
  start-window button IDs to distinct branches (FND-UI-035), but their callee
  effects and the remaining screens are unread. Blocks: nothing yet.
- Q-UI-003. SCR-UI-002, SCR-UI-005, SCR-UI-006, SCR-UI-007, SCR-UI-011: Where does the game
  place each window, given that the resources hold no screen position? Settles it: a reading of
  the code that writes a window's runtime `unk_96` and `unk_98`, or captures of each screen.
  Blocks: nothing yet.
- Q-UI-005. SCR-UI-013 to SCR-UI-023: Which resources, native widgets and handlers produce the
  manual's load, save, context, training, quick-cast, item, store, exit, map and About views? Settles
  it: focused readings of the resource graph and the code that opens, draws and handles each view,
  checked against owner captures for presentation the code cannot decide. Blocks: Survey screen
  coverage is recorded, but slices 2, 3 and 5 need these details. Tried:
  FND-UI-036 identifies the shared ten-row `WIND/18500` graph and the LOAD/SAVE
  mode image changes. FND-UI-037 identifies its event-2 row, action and EXIT
  branches. FND-SAVE-007 identifies the row description source. Native
  chrome, placement and further handlers remain open.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-UI-004. SCR-UI-001, SCR-UI-002, SCR-UI-004, SCR-UI-005, SCR-UI-008 to SCR-UI-012: Captures
  of the screens no capture shows yet (party creation, character generation with both lists, a
  Look panel for a friendly target, a notice being dismissed), of each
  button pointed at and pressed, and of the inventory screen's colours. Settles it: a live session
  with DOSBox screenshots at each step. Blocks: nothing yet.

## Source

None.

## Blocked

None.
