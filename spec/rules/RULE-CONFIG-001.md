---
id: RULE-CONFIG-001
title: The on-off settings and their keys
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-218, FND-CONFIG-010, FND-CONFIG-014, FND-COMBAT-025, FND-INPUT-012, FND-COMBAT-023, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-INPUT-003]
---

## Summary

Music, sound effects, voice effects and animations can each be turned on or off on the Preferences
screen. `F4` turns music on or off, `F5` sound effects, and both `A` and `F6` animations. Voice
effects exist only in the CD version and have no key. Without sound, music and sound effects
stay off, and animations cannot be changed while they are locked.

## When it runs

`settings_key` runs when the player presses a key that the map's own key routine passes on
(RULE-INPUT-003, FND-INPUT-012); `toggle_setting` when the player clicks one of the four on-off
buttons of the Preferences screen (SCR-UI-007) [FND-CONFIG-010, FND-CONFIG-218].

## Parameters

`key_word`, the BIOS keyboard word of the key: `0x3E00` for `F4`, `0x3F00` for `F5`, `0x4000`
for `F6`, and `0x1E41` or `0x1E61` for `A`. `setting`, the button clicked: 0 for music (`16300`), 1
for sound effects (`16301`), 2 for voice effects (`16310`) and 3 for animations (`16303`).

## Inputs

`music_on`, `sound_effects_on`, `voice_on`, `animations_on`, `animations_locked` and
`master_sound_on`.

## Procedure

```text
define toggle_setting(setting):
    let music = music_on
    let effects = sound_effects_on
    if setting == 0:
        music = not music_on
    else if setting == 1:
        effects = not sound_effects_on
    else if setting == 2:
        voice_on = not voice_on
        return
    else if setting == 3:
        if not animations_locked:
            animations_on = not animations_on
        return
    if not master_sound_on:
        music = false
        effects = false
    music_on = music
    sound_effects_on = effects

define settings_key(key_word):
    if key_word == 0x3E00:
        toggle_setting(0)
        if music_on:
            emit MessageShown(message_music_on)
        else:
            emit MessageShown(message_music_off)
    else if key_word == 0x3F00:
        toggle_setting(1)
        if master_sound_on:
            emit MessageShown(message_sound_effects_on)
        else:
            emit MessageShown(message_sound_effects_off)
    else if key_word == 0x1E41 or key_word == 0x1E61 or key_word == 0x4000:
        if animations_locked:
            emit MessageShown(message_animations_locked)
        else:
            toggle_setting(3)
            if animations_on:
                emit MessageShown(message_animations_on)
            else:
                emit MessageShown(message_animations_off)
```

## Outputs

The setting changed. The game saves its settings with every saved game (FMT-CONFIG-003).

## Edge cases

- After `F5` the message reports `master_sound_on`, not the new setting, so with sound present
  it reads `SOUND EFFECTS ON` also when `F5` has turned the effects off [FND-CONFIG-218].
- The keys work at any turn, in combat too, and the letter `A` in either case; `Shift` with a
  function key gives another key word and does nothing here [FND-CONFIG-218].
- Changing the music setting starts or stops the music, and turning animations on or off passes
  one of two groups of four values to overlay 182 (FND-CONFIG-218); turning animations off
  speeds the game up on slower systems, by the manual. What these calls do is not read.

## What the sources say

SRC-MANUAL-1994, page 15: music on / off, sound effects on / off and animations on / off toggle
those settings; voice effects on / off toggles voice effects if the player has the CD version;
turning the animations off helps speed up the game on slower systems. Page 77, hotkeys: `A` toggles
animations on/off, `F4` toggles music on/off, `F5` toggles sound effects on/off, `F6` toggles
animations on/off.

## Differences between builds

None known.

## Open questions

- The starting values of the settings in a new game, and which code sets `animations_locked`;
  in the load image all three settings and `master_sound_on` are on and the lock is off
  (FND-CONFIG-218, Q-CONFIG-002).
- The voice button changes only `voice_on`, while saved games carry the second speech byte
  (FND-CONFIG-014); whether any path brings the two together (Q-CONFIG-002).
