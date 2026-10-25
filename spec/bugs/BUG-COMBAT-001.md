---
id: BUG-COMBAT-001
title: After combat ends by itself, a click in the view before a leader key overwrites the July and August month lengths and makes every later pass run a combat round step
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-RNG-069, FND-RNG-070, FND-RNG-067, FND-RNG-068, FND-RNG-074, FND-RNG-075, FND-RNG-076, FND-RNG-077, FND-PARTY-176, FND-COMBAT-058, FND-COMBAT-059, FND-COMBAT-060, FND-COMBAT-061, FND-COMBAT-062, FND-COMBAT-063]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Symptom

When combat ends because no combatant is left to act, and the player clicks in the view before
pressing a key that picks a leader, two things change. The month lengths the clock conversion uses
for July and August become the two bytes of a free record's number (14 and 0 in the recorded run),
so a later reseed from the DOS date counts dates from August on with the wrong number of days
(RULE-RNG-001). From then on, about 15 times a second, the game runs one combat round step and the
end of combat again with no combat going on, and the dword `4C13:035B`, which otherwise rises by 1 every 667 ms, rises by 60 on
each pass. A leader key does not end this; it went on for the 17 s observed. The click itself
moves the view back to where it was before any scrolling, and the view otherwise stays as it was;
the steps pause while the right button holds the pointer in another mode and go on when it
returns to the first one (FND-COMBAT-059).

Each pass also ends every maintained psionic power after charging its upkeep once, effect `0x45`
on the party and the effects the end of a fight removes; makes timed effect ends come due about
600 times faster through the extra 60; stops the sample playing; drops queued keys and clicks;
stalls while the left button is held; and puts creatures from slot 5 up back on their fight-start
walk records (FND-COMBAT-060). After each pass the map loop's walker step `28C9:1E41` runs as in normal play, moving walkers,
running idle scripts and freeing idle walk records, which the end of combat had re-linked to the
leader; the passes draw no random numbers (FND-COMBAT-061, FND-COMBAT-062).

## Trigger conditions

- Combat ends at overlay 173 `+059A` with no next combatant, which leaves the current slot
  `DS:426D` at 9999 (FND-RNG-070); in the recorded run that happened 66 ms after the two-character
  play state was restored (FND-RNG-069).
- No leader key and no other store sets `DS:426D` before a click in the view (FND-RNG-069).
- A click in the view with combat off sends slot 9999 to `2D40:2F34` (FND-RNG-067).

## Mechanism

`2D40:2F34` accepts any slot of `0x30` or more. Record 9999 of the 37-byte records at `DS:67BB`
falls at `DS:0CE6` in 16-bit arithmetic, inside a help message; the routine copies it to a free
record `r`, follows the copy's link words `0x6F66` and `0x6D72` as record numbers and stores `r` at
`DS:819C` and at `DS:3956`, the July and August entries of the month lengths, then fills record
9999 with `0xFF` (FND-RNG-068).

`28C9:1EDB` runs on every pass with combat off. While record 9999 holds the text it returns
without a store; once the record's word `+0x01` is -1 it calls overlay 173 `+059A` with combat word
0 and the address of `DS:426D` (FND-RNG-074, FND-RNG-075). With no combatant to keep, that runs
the round step `+0C65`, which adds 60 to `4C13:035B` and calls the per-round routines, and then the
end-of-combat sequence, and stores 9999 back (FND-RNG-076, FND-RNG-077). The map loop calls
`28C9:1EDB` with combat off only while the pointer mode word `DS:1440` is 1 (FND-COMBAT-058).

The leader keys 1 to 4 store a slot in `DS:426D` only when `28C9:156C` finds bit `0x20` clear in
byte `+0` of the record `DS:426D` names (FND-PARTY-176). For slot 9999 that byte is `DS:0CE6`, which
holds `0x0A` before the click and `0xFF` after it (FND-RNG-068, FND-RNG-075), so after the click
every leader key is refused (FND-RNG-077).

## Frequency

Each time combat ends with no next combatant and a view click comes before a leader key. The month
lengths stay changed until something rewrites them; the round step repeats on every pass of
`28C9:1EDB` for at least 17 s, with or without a leader key (FND-RNG-077), whenever the pointer is
in its first mode (FND-COMBAT-058, FND-COMBAT-059).

Record 9999's bytes are never rewritten until the program restarts, so once the state has been
left, a later end of combat that leaves 9999 starts the passes again with no click
(FND-COMBAT-063).

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether block copies or file reads, such as loading a saved game, can put 9999 in `DS:426D` or
  `4C13:0369` (FND-RNG-087, FND-RNG-091, FND-RNG-092, Q-RNG-031, Q-RNG-037, Q-RNG-038, Q-RNG-039), and whether `31E0:2C86` can pass 9999, which needs a placed-object entry naming slot 9999
  (FND-RNG-085, FND-RNG-086, Q-RNG-034). Overlay 173 `+059A`
  stores 9999 only when it ends combat (FND-RNG-078, FND-PARTY-089).
- Whether opening a full-screen screen such as Load Game with F2 and leaving it ends the state, as
  the static reading predicts (FND-COMBAT-063; Q-COMBAT-035).
