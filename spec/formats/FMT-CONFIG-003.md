---
id: FMT-CONFIG-003
title: Saved settings in the PREF resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 9
text: false
definition: fmt_config_003.ksy
evidence: [FND-CONFIG-001, FND-CONFIG-002, FND-CONFIG-009, FND-CONFIG-010, FND-CONFIG-012, FND-CONFIG-013, FND-CONFIG-014, FND-UI-033, FND-SAVE-004, FND-SAVE-005, FND-SOUND-007, FND-SOUND-008, FND-SOUND-011]
conflicting: []
split_with: []
related: []
---

## Layout

`PREF` resource 100, the only one, which the game removes and writes again each time it saves a
game and reads back each time it loads one [FND-CONFIG-001, FND-CONFIG-002, FND-SAVE-004,
FND-SAVE-005]. Each field is copied from and to one global of the game, given here by its
address in BLD-GOG-EN-1.1.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `difficulty_index` | The word at `DS:143A` indexes the four difficulty labels in order, 0 to 3. The shipped resource holds 0. | supported | FND-CONFIG-001, FND-CONFIG-009, FND-SAVE-004, FND-SAVE-005 |
| `0x02` | 1 | `UINT8` | `music_level_request` | The byte at `DS:26B4` is passed to the sound library's music-level setter after loading and when music is enabled. The setter caps it at 100, or 90 for one driver type. The shipped resource holds 255. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005, FND-CONFIG-012 |
| `0x03` | 1 | `UINT8` | `sound_effects_volume` | The byte at `DS:26B5` is changed by the sound-effect volume arrows in steps of seven, normally within 0 to 127. The shipped resource holds 63. | supported | FND-CONFIG-001, FND-CONFIG-010, FND-SAVE-004, FND-SAVE-005, FND-SOUND-011 |
| `0x04` | 1 | `UINT8` | `music_bar_denominator` | The byte at `DS:26B6` scales the filled span of the Preferences music-level bar. It receives the unmodified request when the sound library returns a smaller level. The shipped resource holds 255. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005, FND-CONFIG-012, FND-CONFIG-013 |
| `0x05` | 1 | `UINT8` | `sound_effects_enabled` | The byte at `DS:1435` changes through the sound-effects button and stops effect playback at zero. The shipped resource holds 1. | supported | FND-CONFIG-001, FND-CONFIG-010, FND-SAVE-004, FND-SAVE-005, FND-SOUND-007 |
| `0x06` | 1 | `UINT8` | `music_enabled` | The byte at `DS:1436` changes through the music button. The shipped resource holds 1. | supported | FND-CONFIG-001, FND-CONFIG-010, FND-SAVE-004, FND-SAVE-005 |
| `0x07` | 1 | `UINT8` | `animation_control_state` | The byte at `DS:1437` is passed as the drawn state of filmstrip button `BUTN/16303`. After loading, the game passes 4, 4, 16, 16 to a routine when it is not 0 and 16, 16, 16, 16 when it is. 0 in the shipped resource; its on/off polarity is not established. | supported | FND-CONFIG-001, FND-UI-033, FND-SAVE-004, FND-SAVE-005 |
| `0x08` | 1 | `UINT8` | `speech_gate` | The byte at `DS:1439` stops speech playback at zero. The on-screen voice button instead changes the separate runtime gate `DS:14E4`, which is absent from this resource. The shipped resource holds 1. | supported | FND-CONFIG-001, FND-CONFIG-010, FND-CONFIG-014, FND-SAVE-004, FND-SAVE-005, FND-SOUND-008 |
| `0x09` | | | | Total size 9 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `PREF` resource [FND-CONFIG-001].

## Coverage

The one `PREF` resource of the installed `CHARSAVE.GFF` [FND-CONFIG-001].

## Open questions

- Whether the requested music level is adjustable outside the ordinary
  Preferences arrows, and how the driver translates a level into audible
  volume remain open (FND-CONFIG-010, FND-CONFIG-012,
  FND-CONFIG-013, Q-CONFIG-002).
- Which polarity of `animation_control_state` means animations on, and what initializes it for a
  new game (FND-CONFIG-010, Q-CONFIG-002, Q-CONFIG-001).
- Why the saved `speech_gate` and the on-screen runtime gate both block speech
  remains open. One reading is that they stay independent after launch:
  FND-CONFIG-014 supports this in the bounded launcher, Preferences and
  save/load paths, but does not cover indirect writes or new-game setup. A
  competing reading is that another path synchronizes them; no direct
  evidence supports that path yet. A bounded writer and caller survey would
  distinguish these readings (Q-CONFIG-002).
