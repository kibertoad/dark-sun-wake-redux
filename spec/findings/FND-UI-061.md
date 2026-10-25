---
id: FND-UI-061
title: The inventory handler casts a pending spell on a clicked character box, gives the held item on a right click, otherwise switches character, drops, splits, pages and sells through four buttons, closes an open container view on its frame events, and passes the other buttons and keys to overlay 190
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074DDF..0x00075560
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077AE9..0x00077C1A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078DD5..0x000790C9
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0004B3EF..0x0004BF62
tool: Capstone 5.0.9 over overlay code; tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 189's code starts at file `0x749B0`
and overlay 190's at `0x78340`. `cur` is the word `4E71:0B44`. The event word at record offset
`0x12` is written `bits`; FND-PARTY-078 and FND-PARTY-205 read it as the mouse driver's event
mask, so "left" below means `bits` below 8 and "right" 8 or more. `5713:0043` enters `+042F`.

The handler `+0559` dispatches on the event kind through the table at `+0BA4`: 1 to `+0B4E`, 2
to `+06E9`, 5 to `+062A`, 6 to `+0584`, and 3, 4 and others to the exit `+0B44`, which calls
`2C5F:0182` and returns 0.

**Kind 1** calls `+042F` and returns -1. `+042F` stores `cur` in `DS:4264` and 2 in
`DS:4265 + 2 * cur`, closes the item view at `55CB:0000` through overlay 213 `+02D5` when there is
one, releases the buffers at `DS:619D`, `DS:617D`, `DS:6189 + 4k`, `4E71:002F` and `DS:6558`,
closes the window at `5072:003C` through overlay 182 `+0128` with 1, clears that pointer,
`DS:1431` and `DS:43F7`, and calls overlay 190 `+319E`.

**Kind 2** first closes the item view through overlay 213 `+02D5` when `55CB:0000` is set, then
looks the button number up in nine words at `+0B58`:

| Button | In `WIND/13500` | Branch |
|---|---|---|
| 11300 + n, n 0 to 3 | the character boxes | `+0725` |
| 11304 | the View inventory icon at (187, 181) | `+0B44`, nothing |
| 13300 | the 42 x 12 button at (186, 130) | `+0A21` |
| 13302 | the 42 x 12 button at (186, 142) | `+0968` |
| 13303 | the 42 x 12 button at (235, 159) | `+0A91` |
| 13304 | the 42 x 12 button at (277, 159) | `+0AAE` |
| others | | `+0AC2` |

- `+0725`, character box n:
  1. With byte `DS:43F7` set and left, it clears `DS:43F7`, calls overlay 182 `+059C` with 19101
     and 1, and calls overlay 193 `+2290` with `cur`, n, word `DS:43F5` and 6.
  2. With n equal to `cur` and left, it does nothing.
  3. With an item held (overlay 191 `+0000` nonzero), n not `cur`, word `+0x0E` of n's 66-byte
     record nonzero and right, it calls overlay 191 `+06B3` with n. On `0xFFFF` it shows the
     string at `DS:1AC5` through overlay 172 `+05DB`; otherwise it moves the item with overlay
     191 `+074B`. Neither is followed by a redraw.
  4. Otherwise it stores 2 in `DS:4265 + 2 * cur` and closes an open container view as kind 5
     does. It then calls overlay 190 `+05A9` with the event when a store is open (`DS:1A30` below
     9999), or overlay 190 `+0D89` with the event and `5713:0043` when none is. Unless the window
     has closed, it calls `+12FF` with the new `cur` and redraws.
- `+0A21` does nothing unless an item is held. It calls overlay 191 `+05DB` with the words at
  `DS:67C5 + 37k` and `DS:67C7 + 37k` and 0, where k is `cur` when byte `DS:13FA` is set and the
  leader `4C13:0369` otherwise, then overlay 182 `+059C` with 19101 and 1.
- `+0968` needs, in order: `cur`'s word `+0x0E` nonzero, an item held, the held item's type
  below 9999, bit 1 of byte `+0x0B` of that type's 15-byte record, and a quantity above 1. It
  calls `+3139`, which returns 0xFF when overlay 191 `+06B3` finds no free backpack position.
  Otherwise `+3139` creates a stack of half the quantity, rounded down, through overlay 188
  `+1184`, takes that half off the held item, and returns the new position. The handler clears
  the high byte of the result and compares it with -1, a test that never holds, then calls
  `+1688` with `cur` and `+1AD7` with the position plus 4.
- `+0A91` adds 1 to the store page `DS:2932` and calls overlay 207 `+0555` with `DS:6199` and
  `DS:1A30`, then `+0739`.
- `+0AAE` calls overlay 207 `+0D74` when an item is held.
- `+0AC2` passes the event and `5713:0043` to overlay 190 `+0802`. Its table at `+0D4D` sends
  10300, 11305 and 11306 to `+0B57`, which marks the clicked button, closes this window through
  the passed routine and opens the character, spell or effects window. 11308 goes to `+0A95`,
  which closes this window and calls overlay 181 `+0480`. 10308 goes to `+0AEE`, which closes it.
  The leader buttons go to `+08EB`, the computer-control buttons to `+09DA` and the name box to
  `+0828`. After 10308, the handler sets pointer mode 3 through `28C9:2535` when an item is held,
  and calls overlay 208 `+1612` when `DS:43F7` was set at entry and `DS:43F5` is below 328.

**Kind 5** acts on frame 13200 always. It acts on frames 11248 to 11253 only while byte
`DS:6188` is set, and on 11254 to 11259 only while `DS:6187` is set. The event's word must be
`0x20` or `0x80`, and one of the two bytes must be set. It then calls overlay 175 `+16DE`, overlay
175 `+162C` with `DS:32CA` when `DS:6188` is set or `DS:32CB` otherwise, `28C9:2A81` with 1,
overlay 186 `+00B8` with 15, `+12FF` with `cur`, and redraws.

**Kind 6**: `I` and `i` do nothing, and any key does nothing more than close an open item view.
Without a store every other key goes to overlay 190 `+139B` with `5713:0043`. With a store, only
the ten keys at `+0B7C` go there: Esc, 1 to 4, `S` and `s`, Alt-X, Ctrl-C and F3. After Esc,
when the window is still open, the handler runs the steps that follow 10308.

## Interpretation

On a character box, a pending spell or power (FND-PARTY-242) is cast on the clicked character.
A right click while holding an item gives it to that character, or reports a full backpack
(FND-PARTY-220, FND-PARTY-221), and stays on the current inventory. Every other click shows
that character's inventory; that overlay 190 `+0D89` and `+05A9` change `cur` is inferred from
the redraw that follows. The button at (186, 130) drops the held item at a party member's place
(FND-EXE-622). The one at (186, 142) splits a stack (FND-PARTY-245). The one at (235, 159) shows
the next page of a store's items (FND-EXE-607), and the one at (277, 159) offers to buy the held
item (FND-PARTY-245). The order matches the manual's labels for Drop and Split. The navigation
buttons work as on the character screen (FND-PARTY-170), and the View inventory icon does
nothing here. A frame event on `APFM/13200` or on the slots of an open pouch or chest view
(FND-EXE-598) closes that view and redraws the inventory.

The keys 1 to 4 and Space reach the character boxes through overlay 190 `+139B`
(FND-COMBAT-025).

With a full backpack, Split still calls `+1AD7` with 259, past the words 4 to 47 of the
position map that FND-PARTY-217 finds filled. What that draws was not read. Overlay 175 registers the slot
handlers with 11248 to 11253 for `DS:6187` and 11254 to 11259 for `DS:6188` (FND-EXE-598), the
reverse of the pairing this handler tests, so a click on an open container's slot is accepted
only when its group's flag is the other one.

## Alternatives

The left and right reading of `bits` is FND-PARTY-078's and FND-PARTY-205's; FND-PARTY-220 did
not adopt it for this handler. That `DS:67C5` and `DS:67C7` hold the member's map position, what
overlay 181 `+0845`, overlay 188 `+1184`, overlay 208 `+1612`, the outline routines `+1991` and
`+1A90`, and the sale after overlay 207 `+0D74` do, and what the event words `0x20` and `0x80`
stand for were not read.

## How to reproduce

`overlay_listing.py <dsun> 189 74DDF 75560`, `... 189 77AE9 77C1A` and `... 190 78DD5 790C9`;
`trampoline_target.py <dsun> 5713:0043`; the children of `WIND/13500` at `RESOURCE.GFF` offset
`0x4B3EF`, read by FMT-UI-002's layout.
