---
id: RULE-CONFIG-001
title: The on-off settings and their keys
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Music, sound effects, voice effects and animations can each be turned on or off on the Preferences
screen. `F4` turns music on or off, `F5` sound effects, and both `A` and `F6` animations. Voice
effects exist only in the CD version and have no key.

## When it runs

`settings_key` runs when the player presses a key while the map is shown; `toggle_setting` when
the player clicks one of the four on-off buttons of the Preferences screen (SCR-UI-007,
SRC-MANUAL-1994, pages 15 and 77).

## Parameters

`scan_code`, the scan code of the key the player pressed, as the PC keyboard reports it: `0x1E`
for `A`, `0x3E` for `F4`, `0x3F` for `F5` and `0x40` for `F6`. `setting`, the button clicked: 0 for
music, 1 for sound effects, 2 for voice effects and 3 for animations.

## Inputs

`music_on`, `sound_effects_on`, `voice_on` and `animations_on`.

## Procedure

```text
define toggle_setting(setting):
    if setting == 0:
        music_on = not music_on
    else if setting == 1:
        sound_effects_on = not sound_effects_on
    else if setting == 2:
        voice_on = not voice_on
    else if setting == 3:
        animations_on = not animations_on

define settings_key(scan_code):
    if scan_code == 0x3E:
        toggle_setting(0)
    else if scan_code == 0x3F:
        toggle_setting(1)
    else if scan_code == 0x1E or scan_code == 0x40:
        toggle_setting(3)
```

## Outputs

The setting changed. The game saves its settings with every saved game (FMT-CONFIG-003).

## Edge cases

Turning animations off speeds the game up on slower systems, by the manual; what it leaves out is
not known.

## What the sources say

SRC-MANUAL-1994, page 15: music on / off, sound effects on / off and animations on / off toggle
those settings; voice effects on / off toggles voice effects if the player has the CD version;
turning the animations off helps speed up the game on slower systems. Page 77, hotkeys: `A` toggles
animations on/off, `F4` toggles music on/off, `F5` toggles sound effects on/off, `F6` toggles
animations on/off.

## Differences between builds

None known.

## Open questions

- The manual gives animations two keys and voice none; whether `F6` toggles voice in the game is
  not known (Q-CONFIG-001).
- The Preferences click paths use saved bytes at `PREF/100` offsets `0x06` for music,
  `0x05` for sound effects and `0x07` for animation control, while the voice button
  changes an additional runtime byte. Their new-game starting values, the animation
  byte's on/off polarity and the relation between the voice byte and saved speech gate
  remain open (FND-CONFIG-010, FMT-CONFIG-003, Q-CONFIG-001, Q-CONFIG-002).
- The glossary claims `music_on`, `sound_effects_on`, `voice_on` and
  `animations_on` still need complete enable-state and hotkey readings before
  their true/false descriptions can be treated as established (Q-CONFIG-002).
