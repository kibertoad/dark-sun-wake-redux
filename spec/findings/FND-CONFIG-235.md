---
id: FND-CONFIG-235
title: Preferences direct music-level stores do not exclude indirect or callee changes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008B2A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B32E..0x0008B735
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B735..0x0008B769
    kind: file-data
tool: Existing-finding reconciliation plus overlay_listing.py and resident_listing.py at 636538f8; Ghidra 12.1.3 ReportDecompileWindow; no original execution
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE` (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`); overlay 203's code
starts at file `0x8B240`. The music level is the byte `DS:26B4` (FND-CONFIG-236).

**The entry.** Overlay 203 `+0000`, which opens the Preferences window with the handler at
`5782:0043`, does, before it opens the window: when the byte `DS:13F7` is not 0, it calls
`49E9:00FD` and keeps the result at `DS:9B79`, and when `DS:1436` is also not 0 it calls
`4A6E:011F` and stores its return in `DS:26B4` (`+0010..+0041`). This is the only direct store to the literal
`DS:26B4` identified by that overlay reading; it identifies no direct store
to literal `DS:26B6`. This does not classify indirect or copied destinations.

**The buttons.** The handler `+00EE` passes event 6 to overlay 190 `+139B` (`+0105..+0126`)
and, for event 2, compares the event's word with the 13 words at `+04F5` and jumps through the 13
words that follow (`+0132..+014E`):

| Button | Branch | Button | Branch |
|---|---|---|---|
| 10308 | `+0152` | 16305 | `+0176` |
| 11308 | `+0164` | 16306 | `+029E` |
| 16300 | `+01CC` | 16307 | `+0280` |
| 16301 | `+035F` | 16308 | `+03D0` |
| 16302 | `+048A` | 16309 | `+03D5` |
| 16303 | `+0491` | 16310 | `+01B9` |
| 16304 | `+018E` | | |

FND-CONFIG-010 reads the branches of 16300, 16301 and 16303 to 16310, and FND-UI-034 the About
button 16302. `+0152` and `+0164` near-call `+0529` (trampoline `5782:003E`) and then call
`56BD:00BB` and `3D72:0D83`, or overlay 190 `+319E` and the Game Menu's opener overlay 181
`+0480`. The music
button's branch, after `+056C` (FND-CONFIG-218), sets the button from `DS:1436` and draws the
music bar from `DS:26B4`, or 0 when `DS:1436` is 0, over `DS:26B6` (`+0250..+027D`). None of the
13 branches directly writes `DS:26B4`.

## Interpretation

The inspected Preferences branch instructions contain no direct store to the
saved music-level byte; entry stores the library getter's returned byte under
the two stated enable gates. The adjacent arrow pairs directly adjust message
delay and effects volume. This is a local direct-store result, not an exclusion
of every effect of the branches' callees or every control elsewhere in the game.

FND-CONFIG-029 explicitly excludes computed writes, block copies and library
side effects. Combining that census with the local branch reading cannot remove
those exclusions. Its two known direct write paths are Load Game and Preferences
entry; the latter copies a getter result, not just a value from a saved game.
FND-CONFIG-236 resolves the local getter exits and signed setter clamp,
while retaining actual driver result/state and write effects.

FND-UI-065 additionally distinguishes the redraw consumers: full redraw draws
message delay at the top bar, while a music toggle redraws that same position
using the music byte or zero. A persistent music-volume bar on initial entry
is not established by the getter store. No pixel or loudness observation follows.

## Alternatives

- The manual's reading, that the top bar is the music volume and its arrows change it
  (SRC-MANUAL-1994, page 15), is ruled out for this build: the arrows beside the top bar are
  16304 and 16305, whose branches change the word `DS:26B7` and whose hover label is
  `MESSAGE DELAY` (FND-CONFIG-010, FND-UI-034), and no button writes `DS:26B4`.
- The effects of indirect writes, copied state and the dispatcher helpers on
  the saved byte remain unexcluded. Another control or an audible-level change
  without that byte changing is also unresolved (FND-CONFIG-029).
- What `4A6E:011F` returns for admitted live driver states and how getter/setter
  calls affect loudness remain unresolved (FND-CONFIG-236).
- This replacement preserves the inspected branches but withdraws
  FND-CONFIG-219's global no-control and saved-game-only conclusions. It
  does not provide a complete reading, new census or queue answer.

## How to reproduce

This review reuses the cited findings and compares fresh connected Preferences
and library windows retained under GAME_DIR/analysis/music-level-contract-20261107/,
as FND-CONFIG-236 describes. Compare their explicit exclusions and the
full-redraw versus button-redraw consumers. The historical narrow procedure follows.

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 203 8B240 8B735`, and read the 26 words at file
`0x8B735`. Search the listing for `0x26b4` and `0x26b6`. The historical
listing named by FND-CONFIG-219 was not found at its recorded path during
this review. Reproduce a fresh listing under GAME_DIR if instruction review
is needed; the fresh connected listing uses the wider range 8B240..8BB80 and is
retained in the directory above. It does not convert the linear listing
into a complete control-flow or indirect-write census. No original function or game process was run.
