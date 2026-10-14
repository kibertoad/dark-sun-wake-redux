---
id: EXP-PARTY-001
title: Which party START GAME begins play with after ADD, and after ADD then DROP, on the Create Characters screen
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
environment: Windows 11 Pro 10.0.26200; dinorefurb-dosbox-session 0.9.0 RustDosSession with rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
starting_state: new-game
recording: null
repetitions: 4
fixture: EXP-PARTY-001.json
---

## Question

Can play begin with no character in a party box? A character added to box 2 can be dropped while
the leader is still slot 0, and the static readings leave open whether `DROP` empties the box and
whether START GAME then starts with the supplied party, with an empty party, or with nothing
(FND-PARTY-170, FND-PARTY-171, FND-PARTY-172). What START GAME does with one added character is
the comparison.

## Setup

Each run starts the installed game in a new run of the session package, as `docs/RUNTIME.md`
describes: C: a copy of the installed files whose `SOUND.CFG` names IRQ 7 in place of IRQ 5, D: the
CD image `game.ins`, and `DSUN.EXE -W0 -L`. The plan `tools/research/rust-dos-runs/plans/start.json`
runs to 180,000 ms of emulated time, pressing Esc every 10,000 ms from 30,000 to 130,000 ms through
the opening, and saves a checkpoint at the start window (SCR-UI-001); every run below starts from
that checkpoint. A run with a seed writes `rng_state` once at the checkpoint, by a guarded write;
the other runs keep the value the start left there, 44976. Memory is read at stops: the program's
first segment is `05BA`, so a segment `S` of the repository's load image is `S - 0A46` in the run.

## Procedure

Four runs from the checkpoint, given in full in the fixture. Mouse clicks are in the mouse
driver's coordinates, which are the game's 320 x 200 pixels.

1. `add-drop-start`: Create characters; right-click box 2 (at (70, 107)); ADD; ADD on the ADD
   screen with its first stored character selected (Clk'whrr); right-click box 2; DROP; YES;
   right-click box 2; Esc twice; START GAME; Esc 40,000 and 60,000 ms after the click; end
   90,000 ms after it.
2. `start-control`: START GAME, then the same keys and end as run 1.
3. `add-start`: as run 1 up to the ADD of Clk'whrr, then Esc twice, START GAME and the same keys and
   end as run 1.
4. `add-start-seed`: run 3 with `rng_state` written to 12345.

Held constant: the build, the emulator and its settings, the checkpoint, the stored characters and
the timing of every input. Varied: the input before START GAME, and in run 4 the seed. The probe
stops at each call of the generator `rng` (FND-RNG-001) and records the draw under the rule of the
code that made it: script instruction `0x52` under RULE-SCRIPT-006, with the parameter as bound
and the value it stored to `script_accumulator` as result, and `random_mod` under the rule of the
routine that called it or called `chance_in_ten`, with the divisor as bound and the remainder
`random_mod` computed as result. Each result is checked against the value the original computed
before the run goes on.

## Observations

`object_slot_kinds`, `object_slot_combatants`, `leader` and the 66-byte character box records
(FMT-COMBAT-002, through the far pointer at `DS:19C5`) were read at the steps the plans name, and
the byte at `DS:0DAB` with them. Screenshots are kept in `GAME_DIR/captures/` under their `xxh3`.

| Run | Point | Kinds of slots 0 to 3 | `leader` | `DS:0DAB` | Box 2 `unk_0E` | Screen |
| --- | --- | --- | --- | --- | --- | --- |
| all | start window | 0, 0, 0, 0 | 0 | 0 | 0 | `549642f4ebba591fdc15f89380f04667` |
| 1, 3, 4 | after ADD | 0, 0, 2, 0 | 0 | 0 | `0x8018` | `9821ec9adfcbfe95e72a86b21d47cb15`, Clk'whrr in box 2 |
| 1 | DROP | | | | | `06355dc9f7c20e0e8ecc1c16227ff110`, a confirmation with YES, NO and CANCEL |
| 1 | after YES | 0, 0, 0, 0 | 0 | 0 | 0 | `01263fc76e3e51efe084023ac2d30e46` |
| 1 | right-click box 2 | | | | | `9ea6c2d7414ef248dbb677e5869fc932`, the empty box's NEW, ADD and CANCEL |
| 1 | after Esc twice | 0, 0, 0, 0 | 0 | 0 | 0 | `6b68838cce8135f572fa9b499357c3fc`, the start window |
| 1, 2, 3, 4 | 10,000 and 30,000 ms after START GAME | | | | | `2f8d8e234820c8c91cdeba249d0830de`, the same opening picture |
| 1, 2 | 60,000 and 90,000 ms after START GAME | 2, 2, 2, 2 | 0 | 2 | `0x8017` | `de4e7db011dd785bfc4f2a4b15ef550e`, a conversation in a room |
| 3, 4 | 60,000 and 90,000 ms after START GAME | 0, 0, 2, 0 | 2 | 1 | `0x8018` | `53bc28736610911212c97d8e140fb99d`, a room with no conversation |

In runs 1 and 2 `object_slot_combatants` is 0, 1, 2 and 3 after START GAME, and the four box
records' `unk_0E` are `0x8019`, `0x8018`, `0x8017` and `0x8016`. In runs 3 and 4 it is 2 for slot
2 and 0 for the others, and only box record 2's `unk_0E` is nonzero. Every run made 8 draws, all
after the opening picture, and every result the probe computed matched the original's.

## Results

| Run | Seed | Draws: rule, bound, result | End state |
| --- | --- | --- | --- |
| 1 | 44976 | RULE-SCRIPT-006 99 32, 1 0, 1 1; RULE-SOUND-003 8 4; RULE-SCRIPT-006 99 22, 2 0, 99 80, 2 0 | four slots of kind 2, `leader` 0 |
| 2 | 44976 | the same as run 1 | the same as run 1 |
| 3 | 44976 | RULE-SCRIPT-006 99 32, 1 0, 1 1; RULE-SOUND-003 8 4; RULE-SCRIPT-006 4 1, 99 24, 2 2; RULE-SOUND-003 3 0 | slot 2 alone of kind 2, `leader` 2 |
| 4 | 12345 | RULE-SCRIPT-006 99 46, 1 1, 1 0; RULE-SOUND-003 8 1; RULE-SCRIPT-006 4 2, 99 31, 2 2; RULE-SOUND-003 3 2 | the same as run 3 |

The fixture gives every input, draw and end state value exactly.

## Conclusion

`DROP` after `ADD` emptied the box: its kind byte and its record's `unk_0E` went to 0, and the box
offered NEW, ADD and CANCEL again. START GAME after that began play exactly as START GAME from the
start window does, with the four supplied characters, the same draws and the same screens. So play
did not begin with an empty box on this path. With one added character START GAME began play with
that character alone, in its box, with `leader` naming its slot, under two seeds. The draws differ
from the supplied party's from the fifth on, which shows the script that runs differs with the
party.

The runs cover box 2 and one stored character, and a drop of the only member. They do not show
what START GAME does when a party of several created characters is reduced to one, nor what sets
`leader` to the occupied slot (Q-PARTY-071). The runs used IRQ 7 for the sound card where the GOG
configuration uses IRQ 5; nothing in them reads the sound card's settings after start-up.
