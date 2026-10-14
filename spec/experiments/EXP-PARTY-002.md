---
id: EXP-PARTY-002
title: Which party START GAME begins play with after ADD to boxes 1 and 3 on the Create Characters screen
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11 Pro 10.0.26200; dinorefurb-dosbox-session 0.9.0 RustDosSession with rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
starting_state: new-game
recording: null
repetitions: 2
fixture: EXP-PARTY-002.json
---

## Question

Does START GAME begin play with a created party of two characters, each in the box it was added
to, and which box does `leader` name then? EXP-PARTY-001 added one character, to box 2.

## Setup

As EXP-PARTY-001: each run starts from the start-window checkpoint that
`tools/research/rust-dos-runs/plans/start.json` saves, in a new run of the session package with
C: a copy of the installed files whose `SOUND.CFG` names IRQ 7, D: the CD image `game.ins`, and
`DSUN.EXE -W0 -L`. Run 2 writes `rng_state` once at the checkpoint by a guarded write; run 1 keeps
the value the start left there, 44976. A segment `S` of the repository's load image is `S - 0A46`
in the run.

## Procedure

Two runs from the checkpoint (`plans/exp-add-two-start.json` and `exp-add-two-start-seed.json`),
given in full in the fixture, with mouse clicks in the game's 320 x 200 pixels: Create
characters; right-click box 1 (at (121, 47)); ADD; ADD on the ADD screen with its first stored
character selected (Clk'whrr); right-click box 3 (at (121, 107)); ADD; the second row of the list
(Meggyn); ADD; Esc twice; START GAME; Esc 40,000 and 60,000 ms after the click; end 90,000 ms
after it. Held constant: everything but the seed, which is 12345 in run 2. The probe records each
draw of `rng` as EXP-PARTY-001 describes and checks each result against the original's.

## Observations

`object_slot_kinds`, `object_slot_combatants`, `leader`, the byte at `DS:0DAB` and the four
66-byte character box records (FMT-COMBAT-002) were read at the steps the plans name.
Screenshots are kept in `GAME_DIR/captures/` under their `xxh3`; both runs gave the same frame at
every step.

| Point | Kinds of slots 0 to 3 | Combatants | `leader` | `DS:0DAB` | Screen |
| --- | --- | --- | --- | --- | --- |
| start window | 0, 0, 0, 0 | 0, 0, 0, 0 | 0 | 0 | `549642f4ebba591fdc15f89380f04667` |
| box 3's menu, after the first ADD | | | | | `066b5791989c23c8cf429875a6b250a1`, Clk'whrr in box 1 |
| ADD list for box 3 | | | | | `756edbf400b6850976b1ad1174ee7906`, Clk'whrr greyed, Meggyn selected |
| after the second ADD | 0, 2, 0, 2 | 0, 1, 0, 3 | 0 | 0 | `d8ce5f22ac79e44a2aaa96a96de18c1c`, Meggyn in box 3 |
| after Esc twice, and 10,000 ms after START GAME | 0, 2, 0, 2 | 0, 1, 0, 3 | 0 | 0 | `142b4f783734ed2000f0c163d92a1860`, then `2f8d8e234820c8c91cdeba249d0830de` |
| 60,000 and 90,000 ms after START GAME | 0, 2, 0, 2 | 0, 1, 0, 3 | 1 | 1 | `53bc28736610911212c97d8e140fb99d`, a room with no conversation |

After START GAME the box records' `unk_0E` are 0, `0x8018`, 0 and `0x8017`.

## Results

| Run | Seed | Draws: rule, bound, result | End state |
| --- | --- | --- | --- |
| 1 | 44976 | RULE-SCRIPT-006 99 32, 1 0, 1 1; RULE-SOUND-003 8 4; RULE-SCRIPT-006 4 1, 99 24, 2 2; RULE-SOUND-003 3 0 | slots 1 and 3 of kind 2, `leader` 1 |
| 2 | 12345 | RULE-SCRIPT-006 99 46, 1 1, 1 0; RULE-SOUND-003 8 1; RULE-SCRIPT-006 4 2, 99 31, 2 2; RULE-SOUND-003 3 2 | the same as run 1 |

The draws are those of EXP-PARTY-001's runs with one added character under the same seeds.

## Conclusion

START GAME began play with both added characters, each in the box it was added to, and `leader`
went from 0 to 1, the lower of the two filled boxes, by the time play began. With EXP-PARTY-001,
where the only character was in box 2 and `leader` became 2, this fits `leader` naming the lowest
filled box; two cases do not establish that, and FND-PARTY-185 reads the code that sets it. The runs
do not cover three or four created characters, nor a party built with NEW.
