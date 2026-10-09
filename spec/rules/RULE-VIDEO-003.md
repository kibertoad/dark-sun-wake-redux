---
id: RULE-VIDEO-003
title: When an FLI cannot play, the cinematic's still pictures are shown for up to 8 seconds each
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-INPUT-005, FND-VIDEO-008, FND-VIDEO-004, FND-VIDEO-010, FND-VIDEO-006, FND-TIME-004]
conflicting: []
split_with: []
related: [RULE-TIME-001]
---

## Summary

Each cinematic has two or three still pictures among the `BMP ` resources. When its FLI cannot
play, the game shows those instead, each for up to 8 seconds, and holding Shift moves on to the next.
The opening first shows three more pictures, and Shift during those skips the opening. The last
cinematic goes on to further pictures.

## When it runs

From `play_cinematic` (RULE-VIDEO-001) when the FLI cannot play, and at startup in place of
`play_cinematic` when digital sound, sound or cinematics are off (FND-VIDEO-010).

## Parameters

`n`, a `UINT8`: the cinematic, 0 to 5.

## Inputs

`g_57E0_14E5`, `fli_skip_allowed`, the `BMP ` resources, and what `fn_44B6_0011` reports.

## Procedure

```text
define show_cinematic_slides(n: UINT8):
    let counts: UINT16[6] = [3, 3, 3, 3, 2, 3]
    let firsts: UINT16[6] = [11009, 11150, 11158, 11161, 11153, 11155]
    let kept = g_57E0_14E5
    fn_5787_005C()
    g_57E0_14E5 = 0
    if n == 1:
        show_cinematic_slides(0)
        if fn_44B6_0011() & 3 != 0:
            g_57E0_14E5 = kept
            return
    fn_1BF3_4723(1)
    fn_1BF3_4723(0)
    fli_skip_allowed = 1
    let key = 0
    let i = 0
    while i < counts[n]:
        let number = firsts[n] + i
        fn_56BD_0057(number)
        let picture = fn_38FF_04AB(0x20504D42, number)
        if picture != 0:
            emit SlideShown(resource("RESOURCE.GFF", "BMP ", number))
            fn_1BF3_4C09(1)
            key = 0
            let waits = 0
            while waits < 800 and key == 0:
                wait_ms(10)
                if fli_skip_allowed != 0 and fn_44B6_0011() & 3 != 0:
                    key = 1
                waits = waits + 1
            if key == 0:
                fn_1BF3_4FEB(0)
            fn_1BF3_4723(0)
        if n == 0 and key != 0:
            i = counts[n]
        i = i + 1
    fn_1BF3_4723(1)
    if n == 5:
        fn_187_2A21()
    g_57E0_14E5 = kept
```

## Outputs

No return value. It emits `SlideShown` for each picture it finds, sets `fli_skip_allowed` to 1,
and leaves `g_57E0_14E5` as it found it.

## Edge cases

A picture stays 800 times 10 ms, 8 seconds, after it is drawn. A number with no resource is
skipped without a wait; every number in the table is a resource of the installed `RESOURCE.GFF`
(FND-VIDEO-006). For the opening, when a Shift key is held right after the
pictures of 0, the pictures of 1 are skipped. Only `n` from 0 to 5 is in the
table.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What `fn_5787_005C`, `fn_56BD_0057`, `fn_1BF3_4723`, `fn_1BF3_4C09`, `fn_1BF3_4FEB` and
  `fn_187_2A21` do: the drawing, a fade that Shift skips, and the further pictures of cinematic 5
  are inferred (FND-VIDEO-006, Q-VIDEO-002).
- Which archive `fn_38FF_04AB` reads the pictures from; the numbers are found in `RESOURCE.GFF`
  (FND-VIDEO-006, Q-SCRIPT-003).
- What `g_57E0_14E5` is for (FND-VIDEO-006, Q-VIDEO-002).
- What the BIOS reports in `fn_44B6_0011`, the shift-key flags, under an emulator or a modern
  keyboard layer (FND-INPUT-005, Q-VIDEO-001).
