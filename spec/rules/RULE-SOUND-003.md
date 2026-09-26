---
id: RULE-SOUND-003
title: Music is chosen from DJ.DAT by region at startup and by party health in combat, and plays as a disc audio track
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SOUND-002, FND-SOUND-007, FND-SOUND-008, FND-SOUND-009, FND-SOUND-010, FND-SOUND-011, FND-SOUND-012, FND-SOUND-013, FND-RNG-003, FND-RNG-006, FND-CONFIG-005, FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-SOUND-002, FMT-SOUND-002, FMT-CONFIG-001, FMT-COMBAT-001, FMT-COMBAT-002]
---

## Summary

The game's music comes from a table of 38 songs in `DJ.DAT`. At startup it plays a song tied to
the current region, or a random one of the songs for any region. After a line spoken in combat
it switches to combat music: each time the last song has ended it picks one of the combat songs
for how badly hurt the party is, preferring one tied to the region, and after a failed chance
waits a while before trying again. A song plays as the disc's audio track one above its number.
The scripts' music opcode does nothing (FND-SOUND-009).

## When it runs

`start_music` runs once at startup, in the sound setup of overlay 180 (FND-SOUND-011).
`music_tick` runs once per pass of the main loop (FND-SOUND-012). `set_music_mode` runs from
RULE-SOUND-002 around each spoken line.

## Parameters

None for `start_music` and `music_tick`. `choose_music` takes `region`, an `INT16`, and
`distress`, an `INT16`. `play_song` takes `song`, a `UINT16`.

## Inputs

`sound_on`, `digital_sound_on`, `sound_config`, `current_region`, `combatants`,
`combatant_details`, `music_table`, `music_count`, `music_retry_passes`, `music_mode`,
`music_previous_mode`, `music_waiting`, `music_passes`, `g_57E0_348E`, `g_57E0_217A`, the
`g_4E71_*` values below, the state of `rng` through `random_mod` and `chance_in_ten`, and what
`fn_4ABF_01E5` reports.

## Procedure

```text
define set_music_mode(mode: UINT16):
    music_mode = mode

define wait_before_music():
    music_waiting = 1
    music_passes = 0

define play_cd_track(track: UINT16):
    if g_4E71_0C3F == 0 or track > g_4E71_0C43:
        return
    emit MusicPlayed(resource(sprintf("MUSIC/Track%02u.ogg", track)))

define play_song(song: UINT16):
    if sound_config.unk_08 == 113 and sound_config.unk_14 & 2 == 0:
        return
    if g_4E71_0B51 == 3 or g_4E71_0B51 == 5:
        return
    if sound_config.unk_14 & 2 != 0:
        if g_57E0_348E != 0:
            play_cd_track(song + 1)
        return
    if g_57E0_217A[song] & 4 == 0:
        return
    g_4E71_0C1F = 0
    if g_4E71_0C22 == 1:
        fn_4ABF_006B(1)
    g_4E71_0B56 = 0
    fn_47E5_01C4(song)
    if g_4E71_0B56 == 1:
        return
    fn_4A32_0310()
    fn_4A6E_0064(g_4E71_0C14)
    g_57E0_3468 = UINT8(song)
    g_4E71_0C28 = 1

define start_music():
    if sound_on != 0 and digital_sound_on == 0:
        play_song(2)

define measure_party_distress():
    let hit_sum: INT32 = 0
    let max_sum: UINT32 = 0
    for i in 0..4:
        if combatants[i].character_id != 0:
            max_sum = max_sum + combatant_details[i].max_hit_points
            hit_sum = hit_sum + combatants[i].hit_points
    let band: UINT32 = 0
    if max_sum != 0:
        band = UINT32(hit_sum * 9) / max_sum
    party_distress = UINT8(10 - UINT8(band))

define choose_music(region: INT16, distress: INT16) -> UINT16:
    if music_mode == 2:
        return 0
    if music_mode == 0:
        return 1
    if region >= 0x38 and region <= 0x3A:
        region = 0x38
    music_passes = music_passes + 1
    if music_mode == 3 and (music_previous_mode == 1 or music_previous_mode == 2):
        fn_4A32_0185()
        music_waiting = 0
    if music_waiting != 0:
        if music_passes <= music_retry_passes:
            return 0
        music_waiting = 0
    if digital_sound_on != 0:
        if g_4E71_0C3F != 0 and UINT8(fn_4ABF_01E5()) != 0:
            return 0
        if UINT8(fn_4ABF_01E5()) != 0:
            return 0
    if music_mode == 1 and music_count > 0:
        let pool = []
        for i in 0..music_count:
            let record = music_table[i]
            if record.mode == 1 and INT16(record.region) == region:
                play_song(record.song)
                music_previous_mode = 1
                music_mode = 2
                return 1
            if record.mode == 1 and record.region == -1:
                append(pool, i)
        let pick = pool[random_mod(UINT16(count(pool) - 1))]
        play_song(music_table[pick].song)
        music_previous_mode = 1
        music_mode = 2
        return 1
    if music_mode == 3:
        let band_1 = []
        let band_2 = []
        let band_3 = []
        for i in 0..music_count:
            let record = music_table[i]
            if record.mode == 3:
                if record.health_band == 1:
                    append(band_1, i)
                if record.health_band == 2:
                    append(band_2, i)
                if record.health_band == 3:
                    append(band_3, i)
        let pool = band_1
        let last: INT16 = 0
        if distress < 4:
            last = count(band_1) - 1
        if distress >= 4 and distress < 8:
            pool = band_2
            last = count(band_2) - 1
        if distress > 8:
            pool = band_3
            last = count(band_3) - 1
        let found = false
        let found_index = 0
        for j in 0..last + 1:
            if not found and INT16(music_table[pool[j]].region) == region:
                found_index = pool[j]
                found = true
        let pick = pool[random_mod(UINT16(last))]
        if found and chance_in_ten(music_table[found_index].chance):
            if chance_in_ten(music_table[found_index].chance):
                play_song(music_table[found_index].song)
            else:
                wait_before_music()
        else:
            if chance_in_ten(music_table[pick].chance):
                play_song(music_table[pick].song)
            else:
                wait_before_music()
        music_previous_mode = music_mode
        return 1
    if g_4E71_0C1D > g_57E0_08C0:
        g_57E0_08C0 = g_4E71_0C1D
    return 0

define music_tick():
    measure_party_distress()
    if sound_on != 0:
        choose_music(INT16(current_region), INT16(INT8(party_distress)))
```

## Outputs

`choose_music` returns 1 when it chose or tried to choose a song, or when the mode is 0, and 0
otherwise; `music_tick` discards it. The procedures change `music_mode`, `music_previous_mode`,
`music_waiting`, `music_passes`, `party_distress` and `g_57E0_08C0`, and `play_song` the
`g_4E71_*` values it sets. `play_cd_track` emits `MusicPlayed` with the Ogg file GOG plays as the
disc track.

## Edge cases

`music_mode` starts at 1 and `music_previous_mode` at 1 (FND-SOUND-012). In mode 1 the random
pick takes an index below the pool's length minus 1, so the last song of the pool, song 20, is
never picked at startup; with an empty pool the original reads a stale byte of its stack. In
mode 3 a `distress` of 8 takes the first band with a last index of 0, so only its first song is
looked at and picked. An empty band would give a last index of -1: the original then divides by
-1 in `random_mod`, which gives 0, and reads a stale byte. The installed table has no empty band
and no empty pool. Region numbers `0x38` to `0x3A` share the songs of `0x38`.

Without digital sound (`digital_sound_on` 0) the selector does not wait for a song to end, and
`g_57E0_348E` is not set at startup (FND-SOUND-013), so with disc music chosen in `SOUND.CFG`
`play_song` plays nothing. `play_cd_track` gives the Ogg file for the track: song `n` is
`MUSIC/Track{n+1:02}.ogg` (FND-SOUND-002).

The 18 records of mode 2 are never chosen, since the selector returns at once in mode 2.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Whether other code sets `music_mode`, and so what chooses the songs of mode 2, the music of
  exploration after startup (FND-SOUND-012, Q-SOUND-004).
- What `fn_4ABF_01E5` reports, what `fn_4A32_0185` stops, and how `play_cd_track` starts the track
  and how long it plays: the original reads the start of each track from the disc's table of
  contents into a table at `g_57E0_3482`, and the routine it keeps at `g_57E0_348E` tests
  `g_4E71_0C3F` and `g_4E71_0C43`, which it sets from the disc (FND-SOUND-013, Q-SOUND-004).
- What the FM or MIDI branch of `play_song` does: `g_57E0_217A`, `g_4E71_0B51`, `g_4E71_0B56`,
  `g_4E71_0C14`, `g_4E71_0C1F`, `g_4E71_0C22`, `g_4E71_0C28`, `g_57E0_3468`, `fn_4ABF_006B`,
  `fn_47E5_01C4`, `fn_4A32_0310` and `fn_4A6E_0064` were not read (FND-SOUND-013, Q-SOUND-004).
- What `g_4E71_0C1D` and `g_57E0_08C0` count (FND-SOUND-012, Q-SOUND-004).
- Whether `current_region` changes only through the stores FND-SOUND-012 lists (Q-SOUND-004).
