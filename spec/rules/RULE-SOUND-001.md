---
id: RULE-SOUND-001
title: A sound effect plays the BVOC resource of its number, or the installed SOUND file when there is no such resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SOUND-001, FND-SOUND-006, FND-SOUND-007, FND-SOUND-008, FND-SOUND-009, FND-SOUND-010, FND-SOUND-011, FND-SOUND-013, FND-CONFIG-005]
conflicting: []
split_with: []
related: [RULE-SCRIPT-007, FMT-SOUND-001, FMT-CONFIG-001, FMT-CONFIG-003]
---

## Summary

When a script or the game asks for a sound effect, the game stops any sample still playing and
plays the effect of that number: the `BVOC` resource of `RESOURCE.GFF` when there is one, and
otherwise the file `SOUND` and the number in three digits from the installation directory. Nothing
plays while sound or sound effects are off or the setup chose "No Sound".

## When it runs

As the only handler of `SoundRequested`, which a script's sound opcode emits (RULE-SCRIPT-007,
FND-SOUND-009), and from 19 other places in the game's code: one resident caller and 18 in
overlays 173, 179, 193, 201, 203 and 206 (FND-SOUND-007). What those others play is not known.

## Parameters

`sound`, a `UINT16`: the number of the effect. Only its low byte is used.

## Inputs

`sound_on`, `g_57E0_1435`, `sound_config`, `digital_sound_on`, `install_path`, and what
`fn_4611_0051`, `fn_49E9_00FD`, `fn_38FF_05B5` and `fn_56BD_0034` report.

## Procedure

```text
define play_sound_effect(sound: UINT16):
    let n: UINT8 = UINT8(sound)
    if sound_on == 0 or n == 0xFF or g_57E0_1435 == 0:
        return
    if sound_config.unk_08 == 113:
        return
    if digital_sound_on != 0 and fn_4611_0051() != 0:
        fn_4611_03A5()
    if fn_49E9_00FD() != 0:
        fn_49E9_0142()
    if fn_38FF_05B5(0x434F5642, UINT32(n)) == 0:
        fn_4654_04FE(n, 6000, 6001)
        emit SoundEffectPlayed(resource("RESOURCE.GFF", "BVOC", n))
        return
    let path = sprintf("%sSOUND%03u.VOC", install_path, n)
    if UINT8(fn_56BD_0034(path)) != 0:
        if fn_4611_0177(path) != 0:
            emit SoundEffectPlayed(resource(path))
```

## Outputs

No return value. It may stop a sample that is playing, and emits `SoundEffectPlayed` with the
resource or file it starts.

## Edge cases

Effect 255 never plays. A number above 255 is cut to its low byte, so effect 256 plays effect 0.
The installed `SOUND` numbers 12, 132, 138 and 147 are also `BVOC` numbers, so those files never
play through this rule: the resource is found first (FND-SOUND-001). A number with neither a
resource nor a file plays nothing. `0x434F5642` is the tag `BVOC` read as a little-endian 32-bit
value.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What `fn_4611_0051`, `fn_4611_03A5`, `fn_49E9_00FD` and `fn_49E9_0142` check and stop, and what
  `fn_38FF_05B5`, `fn_56BD_0034`, `fn_4611_0177` and `fn_4654_04FE` do beyond looking up, testing
  for and playing a sample; what the arguments 6,000 and 6,001 mean (FND-SOUND-007, Q-SOUND-002).
- Whether `g_57E0_1435`, `unk_05` of `PREF`, is the Preferences sound-effects switch
  (`sound_effects_on`), and at what rate and volume the library plays the samples (FMT-CONFIG-003,
  Q-SOUND-001, Q-SOUND-002).
- What the 19 other callers play (Q-SOUND-003).
