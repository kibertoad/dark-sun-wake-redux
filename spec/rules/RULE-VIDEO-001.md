---
id: RULE-VIDEO-001
title: A cinematic plays its FLI from the installation, copying it from the disc first when it can, and falls back to still pictures
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-VIDEO-002, FND-VIDEO-003, FND-VIDEO-004, FND-VIDEO-005, FND-VIDEO-006, FND-VIDEO-007, FND-SOUND-006, FND-SOUND-007, FND-SOUND-008, FND-SOUND-010, FND-SOUND-011, FND-SOUND-012, FND-COMBAT-023, FND-COMBAT-026]
conflicting: []
split_with: []
related: [RULE-VIDEO-002, RULE-VIDEO-003, RULE-SOUND-002, FMT-VIDEO-001]
---

## Summary

The game has five cinematics, `1.FLI` to `5.FLI`. The opening, 1, plays at startup and the others
when a script asks for them. A cinematic plays from the installation directory. When it is not
installed, the game copies it from the disc's `CINE` directory if the disk has room, deleting
another installed cinematic to make room if needed; the opening is never copied. Entering four
regions copies the next cinematic ahead of time. When the file cannot be had, or digital sound,
sound or cinematics are off, the cinematic's still pictures are shown instead (RULE-VIDEO-003).
Each FLI plays with song `n + 35`, one frame every 107 ms.

## When it runs

`play_cinematic` runs at startup with 1, or with the number the `-C` test switch gives, when
digital sound, sound and cinematics are on, and from script opcode `0x22` with 6 as its first
parameter and `n` as its third (FND-VIDEO-005). `prefetch_cinematic` runs each time a region is
loaded, after `current_region` is set, with the region's number (FND-VIDEO-007).

## Parameters

`n`, a `UINT8`: the cinematic, 1 to 5. `region`, a `UINT16`: the region being entered.

## Inputs

`digital_sound_on`, `sound_on`, `cinematics_on`, `install_path`, `install_type`,
`cinematic_start_delay`, `cinematic_frame_ticks`, `combat_state`, `g_57E0_4263`,
`installed_fli_lengths`, the installed files and those of the disc, and what `fn_56BD_0034` and
`fn_187_2802` report.

## Procedure

```text
define free_cinematic_space(n: UINT8) -> UINT8:
    let installed = 0
    for i in 1..6:
        if i != n and not (n == 5 and i == 3):
            if UINT8(fn_56BD_0034(sprintf("%s%d.FLI", install_path, i))) != 0:
                installed = installed + 1
    if installed < 2:
        return 0
    let deleted: UINT8 = 0
    for i in 1..6:
        if i != n and not (n == 5 and i == 3):
            let path = sprintf("%s%d.FLI", install_path, i)
            if UINT8(fn_56BD_0034(path)) != 0:
                let empty = false
                let handle = fn_44DE_0086(path, 1)
                if handle != 0xFFFF:
                    if fn_44DE_0127(handle) == 0:
                        empty = true
                    fn_44DE_003A(handle)
                if fn_44DE_02A9(path) != 0xFFFF:
                    deleted = 1
                installed_fli_lengths[i] = 0
                if not empty:
                    return deleted
    return deleted

define play_cinematic(n: UINT8):
    let resume = g_57E0_4263
    if digital_sound_on == 0 or sound_on == 0:
        show_cinematic_slides(n)
        return
    stop_music_for_speech()
    fli_skip_allowed = 1
    let disc_path = sprintf("CD:CINE/%d.FLI", n)
    let path = sprintf("%s%d.FLI", install_path, n)
    if UINT8(fn_56BD_0034(path)) == 0:
        if n == 1 or cinematics_on == 0 or UINT8(fn_56BD_0034(disc_path)) == 0:
            show_cinematic_slides(n)
            return
        if UINT8(fn_56BD_0034(path)) == 0:
            let room = false
            if fn_187_2802(disc_path) != 0:
                room = true
            else if free_cinematic_space(n) != 0 and fn_187_2802(disc_path) != 0:
                room = true
            if not room:
                show_cinematic_slides(n)
                return
            fn_4544_0000(disc_path, path, 0, 0)
    if UINT8(fn_56BD_0034(path)) == 0 or cinematics_on == 0 or sound_on == 0:
        show_cinematic_slides(n)
        return
    g_57E0_6298 = 0
    if n == 2:
        g_57E0_6298 = 1
    let delay = UINT32(INT32(cinematic_start_delay))
    if n == 3:
        delay = 1000
    else if n == 5:
        delay = 4000
    play_fli(path, n + 35, INT8(cinematic_frame_ticks), delay)
    fn_1BF3_2973(0x113)
    fn_1BF3_4723(1)
    fn_1BF3_4723(0)
    fn_3D72_0D83()
    if resume != 0:
        if combat_state == 0:
            resume_music_after_speech(1)
        else:
            resume_music_after_speech(3)

define prefetch_cinematic(region: UINT16):
    if install_type != 0 and digital_sound_on != 0:
        fn_187_23E3()
    fn_187_206E(fn_187_2424(region))
    if digital_sound_on == 0:
        return
    fn_44DE_0569()
    let number = 0
    if region == 0x43:
        if install_type & 2 != 0:
            fn_44DE_02A9(sprintf("%s1.FLI", install_path))
            fn_44DE_02A9(sprintf("%s2.FLI", install_path))
        number = 5
    else if region == 0x44:
        number = 3
    else if region == 0x42:
        number = 4
    else if region == 0x3E:
        number = 2
    else:
        # the original goes on with two path buffers it never filled (Edge cases)
        return
    let disc_path = sprintf("CD:CINE/%d.FLI", number)
    let path = sprintf("%s%d.FLI", install_path, number)
    let handle = fn_44DE_0086(path, 1)
    if handle != 0xFFFF:
        fn_44DE_003A(handle)
    else if fn_187_2802(disc_path) != 0:
        fn_187_21EA(disc_path, path)
```

## Outputs

`play_cinematic` and `prefetch_cinematic` return nothing. Through `play_fli` (RULE-VIDEO-002) or
`show_cinematic_slides` (RULE-VIDEO-003) a cinematic emits `FliRecordShown` or `SlideShown`. The
procedures may copy and delete files of the installation directory, and `free_cinematic_space`
sets the lengths it records to 0. `play_cinematic` sets `fli_skip_allowed` to 1 and stops the
music; after an FLI it leaves `music_mode` at 0, or at 1 or 3 when `g_57E0_4263` was set.

## Edge cases

The original builds the disc paths from the drive letter `'A'` plus `cd_drive`, as
`%c:\CINE\%d.FLI` and, in `prefetch_cinematic`, from four fixed strings such as `%c:\CINE\5.FLI`;
here they are written as the build entry writes disc paths. It builds the installed path a second
time from the name `1.FLI` with its first character replaced by the digit of `n`, which gives the
same path for `n` from 1 to 9.

`1.FLI` is never copied: the opening plays only when it is installed. The installed check comes
before the check of `cinematics_on`, so an installed FLI is copied or kept whatever it holds, but
plays only when `cinematics_on` is set. With digital sound or all sound off, the pictures are
shown and the music keeps playing. An FLI that stops early still restores the screen mode.

`free_cinematic_space` counts the installed cinematics other than `n`, and other than `3.FLI` for
the last cinematic, and does nothing when fewer than two are there; otherwise it deletes them in
order up to and including the first whose length is not 0. When a copy fails after the room was
found, the original goes on to the final check, which then fails and shows the pictures.

For a region other than `0x3E`, `0x42`, `0x43` and `0x44`, the original's `prefetch_cinematic`
opens nothing and calls `fn_187_2802`, and when that returns 1 `fn_187_21EA`, with two path buffers
on its stack that it has not filled.

`install_type` with bit 1 set deletes `1.FLI` and `2.FLI` on entering region `0x43`.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Which scripts use opcode `0x22` with request 6 and which numbers they pass, and so when
  cinematics 2 to 5 play (FND-VIDEO-005, Q-VIDEO-002).
- What `fn_187_2802` compares the length with: `fn_44DE_04A1` fills in words of which it
  multiplies the first two (FND-VIDEO-004, Q-VIDEO-002).
- What `fn_4544_0000`, `fn_187_21EA`, `fn_44DE_0127`, `fn_44DE_02A9` and `fn_44DE_0569` do beyond
  copying, measuring and deleting files; what `fn_1BF3_4723`, `fn_3D72_0D83`, `fn_187_23E3`,
  `fn_187_2424` and `fn_187_206E` do when a region is entered or a cinematic ends
  (FND-VIDEO-004, FND-VIDEO-007, Q-VIDEO-002).
- What `g_57E0_6298` is for: no read of it was found (FND-VIDEO-004, Q-VIDEO-002).
- What the original's `prefetch_cinematic` does for the other regions with its unfilled buffers
  (FND-VIDEO-007, Q-VIDEO-002).
- Whether anything reads `installed_fli_lengths` (FND-VIDEO-007, Q-VIDEO-002).
- What `fn_56BD_0034`, `fn_44DE_0086` and `fn_44DE_003A` do beyond testing for, opening and
  closing a file, and what `fn_1BF3_2973` does beyond setting the screen mode (FND-VIDEO-003,
  Q-SOUND-002).
- What `g_57E0_4263` stands for beyond marking that the music mode was set after a spoken line
  (FND-SOUND-008, Q-SOUND-003).
