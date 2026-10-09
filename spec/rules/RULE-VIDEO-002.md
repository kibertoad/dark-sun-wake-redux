---
id: RULE-VIDEO-002
title: The FLI player shows the first record after a wait and then one record every given number of milliseconds, until the header's frame count or a Shift key
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-INPUT-005, FND-VIDEO-001, FND-VIDEO-008, FND-VIDEO-003, FND-VIDEO-006, FND-SOUND-008, FND-SOUND-010, FND-SOUND-013]
conflicting: []
split_with: []
related: [RULE-VIDEO-004, RULE-SOUND-002, RULE-SOUND-003, FMT-VIDEO-001]
---

## Summary

The player switches the screen to 320x200 with 256 colours, starts the cinematic's song, and
waits a set time before the first frame. It then shows a frame every `ticks` milliseconds,
counting the time spent reading a frame against the next wait, and stops after as many frames as
the file's header gives, or while a Shift key is held when skipping is allowed.

## When it runs

From `play_cinematic` (RULE-VIDEO-001), the only caller found (FND-VIDEO-008, FND-VIDEO-004).

## Parameters

`path`, a `char[]`: the file. `song`, a `UINT8`: the song to start. `ticks`, an `INT8`: the
milliseconds between frames. `delay`, a `UINT32`: the milliseconds before the first frame.

## Inputs

`fli_ticks`, `fli_skip_allowed`, `g_57E0_1436`, `g_4E71_0C4A`, the file, and what
`fn_44B6_0011` reports.

## Procedure

```text
define play_fli(path, song: UINT8, ticks: INT8, delay: UINT32) -> UINT16:
    fn_1BF3_2973(0x13)
    let fli = read_file(path, FMT-VIDEO-001)
    play_song(UINT16(song))
    if g_57E0_1436 != 0:
        let polls: UINT32 = 0
        while true:
            fn_2660_04F3()
            polls = polls + 1
            if g_4E71_0C4A > song or polls >= 20000:
                break
    if ticks == 0:
        ticks = 1
    if delay != 0:
        fli_ticks = 0
        let spins: UINT32 = 0
        # may run: RULE-VIDEO-004
        while UINT32(fli_ticks) < delay:
            spins = spins + 1
            if spins > 50000 and fli_ticks == 0:
                break
    let result: UINT16 = 0
    if count(fli.records) == 0:
        fn_1000_1C32()
        result = 3
    else:
        emit FliRecordShown(fli.records[0])
        let shown: UINT16 = 1
        let carry: INT32 = 0
        while result == 0:
            let wait = UINT16(INT16(ticks))
            if carry != 0:
                if INT32(ticks) >= carry:
                    wait = UINT16(INT32(ticks) - carry)
                    carry = 0
                if INT32(ticks) < carry:
                    carry = carry - INT32(ticks)
                    wait = 0
            fli_ticks = 0
            # may run: RULE-VIDEO-004
            let now = fli_ticks
            while now < wait:
                now = fli_ticks
            fli_ticks = 0
            if shown >= count(fli.records):
                result = 3
            else:
                emit FliRecordShown(fli.records[shown])
                shown = shown + 1
                if shown >= fli.frame_count:
                    result = 4
                else:
                    carry = carry + INT32(INT16(fli_ticks))
                    if fn_44B6_0011() & 3 != 0:
                        if fli_skip_allowed != 0 or INT16(shown) > 240:
                            result = 1
    stop_music_for_speech()
    return result
```

## Outputs

Returns 1 when a Shift key ended the cinematic, 3 when a record could not be read and 4 after
the last frame. It emits `FliRecordShown` for each record it draws, in file order, and changes
`fli_ticks`. It leaves the screen in BIOS mode `0x13` and the music stopped (`music_mode` 0).

## Edge cases

When the file cannot be opened, or its first 128 bytes cannot be read or lack the magic, the
original returns 2 before starting the song or waiting; `read_file` stands for its opening and
reading of the header and of each record in turn. A record the original cannot read, or whose type
is not `0xF1FA`, ends playback with 3 at that record, and at the first record it also calls
`fn_1000_1C32`. A record of 16 bytes, with no chunk, changes nothing on screen but still takes
its turn.

The player reads records only while the count is below `frame_count`, so of the
`frame_count + 1` records of each installed file the last is never shown. A `frame_count` of 0
or 1 still shows two records. After the last frame there is no wait. The wait before the first
frame gives up after 50,000 passes when `fli_ticks` has not moved, as when the timer is not
running. Shift is only looked at after a frame, and no other key is read; with `fli_skip_allowed`
at 0 Shift ends playback
only after frame 240. `ticks` is a signed byte: a value from 128 up gives a wait near 65,535 ms.

The player does not read the header's `speed`; the frame time comes from the caller
(RULE-VIDEO-001).

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- How the chunks of a record are drawn and how the palette changes: the routines behind
  `FliRecordShown` were not read (FND-VIDEO-008, Q-VIDEO-001).
- What `fn_1BF3_2973` does beyond setting the mode (FND-VIDEO-003), and what `fn_2660_04F3`,
  `fn_1000_1C32` do, and what the BIOS reports in `fn_44B6_0011` under an emulator or a modern
  keyboard layer (FND-VIDEO-008, FND-INPUT-005, Q-VIDEO-001).
- What `g_57E0_1436` and `g_4E71_0C4A` hold, read here as whether to wait for the disc track and
  the track playing (FND-VIDEO-008, Q-SOUND-004).
- How the original removes the timer slot at the end, which was not read (FND-VIDEO-008). (Q-VIDEO-001)
