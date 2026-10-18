# UI

Next ID: Q-UI-020

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
- Q-UI-002. SCR-UI-002, SCR-UI-003, SCR-UI-004, SCR-UI-005, SCR-UI-009, SCR-UI-010, SCR-UI-011, SCR-UI-012: Which code opens View Character, the stored-character list,
  character generation with its discipline and sphere lists, the cast and current-effects
  screens, the Look panel and the conversation windows, and handles each of their controls?
  Settles it: for each of these screens, the routine that requests its window and the routine
  its window's `after_children` or `before_children` pointer or a frame's handler at `0x62`
  names, with the branch each control number takes. Tried: FND-UI-012 finds no decoded
  instruction of the resident image with these controls' numbers as operands, and FND-UI-041
  reads the callback setters and the generic before and after handoff that such handlers are
  installed through; no handler of these screens has been read. Blocks: nothing yet.
- Q-UI-013. FMT-UI-003, SCR-UI-001, SCR-UI-004, SCR-UI-006, SCR-UI-007, SCR-UI-008, SCR-UI-012: Which frame of a button's `icon`
  does the game draw when the button is pointed at, pressed or unavailable, and what makes
  `BUTN/16302`, whose four frames are each one colour, visible? Settles it: a reading of the
  window system's button draw routine and the state it chooses a frame by, from the frame
  dispatcher's draw requests. Tried: split from Q-UI-002. FND-UI-004 gives the record's icon
  and frame counts, FND-UI-024 finds three start-window icons with a 1 x 1 third frame, and
  FND-UI-034 reads the Preferences hover and click routines, which choose no frame; the draw
  routine has not been read. Blocks: nothing yet.
- Q-UI-014. FMT-UI-005: Which font does an edit box draw its text in, and where in the box?
  Settles it: a reading of the routine that draws an `EBOX` control's text, with the font it
  selects and the offsets it adds to the box's position. Tried: split from Q-UI-002.
  FND-UI-005 gives the record's fields; no reading of its draw routine has been made. Blocks:
  nothing yet.
- Q-UI-015. SCR-UI-001: Where does each start-window button lead? Settles it: the overlay 194
  setup helper's effect for arguments 0 and 1, and the routine each of the four branches
  reaches after it: overlay 187 and 182 for Start Game, overlay 212 for Create Characters,
  overlay 192 for Load Saved Game, and the common path that clears `DS:1462` for Exit to DOS,
  followed to the next window opened or to the program's exit. What the Create Characters and
  Load Saved Game paths change in the party is Q-PARTY-017 and Q-PARTY-018. Tried: split from
  Q-UI-002. FND-UI-035 reads the opener and the event-2 callback dispatching buttons 19300 to
  19303 to four branches; their callees remain. Blocks: nothing yet.
- Q-UI-016. SCR-UI-007: Which input events reach the Preferences dispatcher in overlay 203, and
  what does the screen redraw after a button changes a setting? Settles it: the event value the
  dispatcher compares against its 13 button numbers and the routine that delivers it, and the
  draw calls each branch makes after its state change. Tried: split from Q-UI-002. FND-UI-034
  reads the hover routine's labels and FND-CONFIG-010 the dispatcher's state change per button;
  the event gating and the redraws remain. Blocks: nothing yet.
- Q-UI-017. SCR-UI-008: What do the inventory window's controls do? Settles it: the event
  branches of overlay 189's handler `+0559` for each control of `WIND/13500`, among them the two
  42 x 12 buttons at the right and the buttons at (235, 159) and (277, 159). Tried: split from
  Q-UI-002. FND-UI-030 lists the window's controls, and FND-PARTY-217 links the inventory
  opener's request for `WIND/13500` to `+0559` and reads the redraw's position projection; the
  handler's other branches remain. Blocks: nothing yet.
- Q-UI-018. SCR-UI-006: What does each Game Menu button do? Settles it: the targets of overlay
  181's fourteen-entry button table other than the overhead map's `+0289`, followed to the
  window or state change each makes, and whether the menu prints a description line for the
  pointed-at button. Tried: split from Q-UI-002. FND-UI-046 reads the handler's table of
  fourteen button numbers and targets and the overhead-map branch; FND-UI-028 lists the
  window's buttons. Blocks: nothing yet.
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
  mark colours (FND-UI-047, FND-IMAGE-012) and placement (Q-UI-003).
- Q-UI-012. SCR-UI-022: Does the DAC change while the overhead map is shown, and do the cycling
  ranges keep rotating then? Settles it: the 41 transfers the walks from the map's routines leave
  unresolved, resolved or bounded, above all the window-system callbacks through `es:[bx+0F5]`,
  `+0F9` and `+0FD` and the pointers at `DS:A0F1` and `DS:A0F5`, and whether the map's wait for
  input reaches the main loop's call of the cycler 3796:047C. What first puts the region palette
  in the DAC on a new game is Q-UI-019. Tried: FND-IMAGE-012 finds the play palette loaded by
  overlay 204 +1D2E and the cycling ranges, and a walk from the map's routines whose only route
  to a DAC write passes the view draw `2C5F:0139` into overlay 201 +05D5. Tried: FND-UI-048 shows
  +05D5 ends every path with the fade to `PAL/current_region`, inside the map routine's first
  call and before it builds its window, and that it writes the DAC through nothing else; whether
  that route runs no longer decides the map's palette and is removed from Settles it. Blocks:
  nothing yet.
- Q-UI-019. SCR-UI-022: What first puts the region palette in the DAC when a new game begins?
  Settles it: the contents of the 768-byte block 444C:0008 returns for 256 and 3, which the gate
  routine overlay 182 +1167 writes to the DAC at +13E3, and whether overlay 201 +05D5 runs on the
  new-game route. Tried: split from Q-UI-012. FND-IMAGE-012 reads the fade overlay 204 +1D2E and
  its callers, and FND-UI-048 finds that every read route to the fade from the start window's
  callback, the gate routine's later calls and the main loop's overlay 192 +0015 passes +05D5,
  and that overlay 192 +0C62's call of the fade is not reached from them. FND-IMAGE-012 matches
  FND-IMAGE-010's capture of the opening region with `PAL/50`. Blocks: nothing yet.

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
