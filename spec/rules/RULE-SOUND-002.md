---
id: RULE-SOUND-002
title: A spoken line plays INTR files from the disc below 50 and SPCH files from the installation or the disc from 50 up
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SOUND-001, FND-SOUND-006, FND-SOUND-007, FND-SOUND-008, FND-SOUND-010, FND-SOUND-011, FND-SOUND-012, FND-SOUND-013, FND-CONFIG-005]
conflicting: []
split_with: []
related: [RULE-SOUND-003, FMT-SOUND-001, FMT-CONFIG-001, FMT-CONFIG-003]
---

## Summary

To speak a line the game stops the music, finds the voice file of the line's number, stops any
sample still playing and plays the file. Lines below 50 come from the disc's `INTR` directory;
the others from the installed `SPCH` files, or from the disc's `SPEECH` directory when the
installed file is missing. In some states it then waits for the line to end and sets the music
mode for combat or for the rest of the game.

## When it runs

When overlay 187 or 204 speaks a line (FND-SOUND-008). Which lines they speak, and when, is not
known.

## Parameters

`n`, a `UINT16`: the number of the line.

## Inputs

`sound_on`, `digital_sound_on`, `speech_on`, `g_57E0_1439`, `sound_config`, `install_path`,
`g_57E0_0D9C`, `g_57E0_6554`, `g_57E0_14E8`, `combat_state`, and what `fn_56BD_0034`,
`fn_49E9_00FD` and `fn_4611_0051` report.

## Procedure

```text
define stop_music_for_speech() -> UINT16:
    if sound_on == 0:
        return 0
    fn_4A32_0185()
    set_music_mode(0)
    return 1

define resume_music_after_speech(mode: UINT16) -> UINT16:
    g_57E0_4263 = 1
    set_music_mode(mode)
    g_57E0_4275 = 0
    return 1

define play_speech(n: UINT16) -> UINT16:
    let result: UINT16 = 1
    if sound_on == 0 or digital_sound_on == 0:
        return 0
    if sound_config.unk_08 == 113 or speech_on == 0 or g_57E0_1439 == 0:
        return 0
    stop_music_for_speech()
    let path = sprintf("CD:INTR/INTR%u.VOC", n)
    if n >= 50:
        path = sprintf("%sSPCH%u.VOC", install_path, n)
        if UINT8(fn_56BD_0034(path)) == 0:
            path = sprintf("CD:SPEECH/SPCH%u.VOC", n)
            g_57E0_4275 = 1
    if UINT8(fn_56BD_0034(path)) == 0:
        return 0
    if fn_49E9_00FD() != 0:
        fn_49E9_0142()
    if digital_sound_on != 0 and fn_4611_0051() != 0:
        fn_4611_03A5()
    if fn_4611_0177(path) == 0:
        result = 0
    else:
        emit SpeechPlayed(resource(path))
    if g_57E0_0D9C != 0 and g_57E0_6554 == 0:
        if digital_sound_on != 0:
            let playing = fn_4611_0051()
            while playing != 0:
                playing = fn_4611_0051()
        if g_57E0_14E8 != 0:
            let handle = fn_44DE_0086(sprintf("CD:RESOURCE.GFF"), 1)
            if handle != 0xFFFF:
                fn_44DE_003A(handle)
        if combat_state != 0:
            resume_music_after_speech(3)
        else:
            resume_music_after_speech(2)
    return result
```

## Outputs

`play_speech` returns 0 when speech is off, when the file is missing or when the player refuses
the file, and 1 otherwise. It sets
`music_mode` to 0, and to 2 or 3 afterwards when it waits; it sets `g_57E0_4275` to 1 when it
plays the disc's copy of a line, and back to 0 after waiting. It emits `SpeechPlayed`.

## Edge cases

The music stops even when the file is then missing, and the mode stays 0 in that case, so no
music is chosen until something sets the mode again. When `g_57E0_0D9C` is 0 or `g_57E0_6554` is
not 0 the routine returns while the line plays and leaves the mode at 0. The original builds the
disc paths from the drive letter `'A'` plus `cd_drive`, as `%c:\INTR\INTR%u.VOC` and
`%c:\SPEECH\SPCH%u.VOC`; here they are written as the build entry writes disc paths.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What `g_57E0_0D9C`, `g_57E0_6554`, `g_57E0_14E8`, `g_57E0_4263` and `g_57E0_4275` stand for,
  and why the disc's `RESOURCE.GFF` is opened and closed at once (FND-SOUND-008, Q-SOUND-003).
- Whether `g_57E0_1439`, `unk_08` of `PREF`, is the Preferences voice setting (FMT-CONFIG-003,
  Q-SOUND-001).
- What `fn_56BD_0034`, `fn_4611_0177`, `fn_4611_0051`, `fn_4611_03A5`, `fn_49E9_00FD`,
  `fn_49E9_0142`, `fn_44DE_0086`, `fn_44DE_003A` and `fn_4A32_0185` do beyond testing for a file,
  playing, checking and stopping a sample, opening and closing a file and stopping the music
  (FND-SOUND-008, Q-SOUND-002).
- Which lines overlays 187 and 204 speak, and when (Q-SOUND-003).
