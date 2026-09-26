# audio-presence

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-SOUND-001 (queue/SOUND.md, Live session).
- Blocks: slice 7.
- Length: about 10 minutes.

Establishes only whether the game audibly changes under two Preferences
controls at stable screens. It does not identify a file, codec, device, helper
process, volume scale, sample rate, loop point, or timing unit, and it does not
ask for an audio recording. Use a normal fresh launch with working speakers or
headphones and default Preferences, at the default window size. Record each
audible result as exactly `yes`, `no`, or `uncertain`; `uncertain` is preferred
whenever the host mixer, outside noise, or the game's initial state prevents a
confident observation. Screenshots are still required.

## Script

1. A0, Q-SOUND-001. Reach the first settled start screen without clicking any
   control. Captures: that screen. Record whether any continuous game audio is
   audible once it settles, and separately whether a one-shot effect has been
   heard. Do not infer a track or source file.
2. A1, Q-SOUND-001. Choose START GAME once and wait only until the first
   settled party-overview or in-world screen. Captures: that screen. Record the
   same two observations and any audible difference from A0. Do not use a
   save, alter a setting, or wait for a guessed loop boundary.
3. A2, Q-SOUND-001. From the Preferences P0 state (see `preferences.md`),
   click Music once, then click it again to restore. Captures: after each
   click. Record whether continuous game audio changes after each click. Do
   not derive a delay, volume value, file mapping, or default from the result.
4. A3, Q-SOUND-001. From the restored P0 state, click Sound Effects once, then
   again to restore. Captures: after each click. Record whether either click
   itself produces an audible one-shot effect and whether any continuous audio
   changes. Do not trigger an unrelated gameplay event to seek a sound.

An unchanged or silent result is evidence of that observation only; do not
retry until sound appears.

Agent, after confirmation: record each yes, no or uncertain answer as a
dynamic finding with the capture's `xxh3` and the host audio set-up the owner
reports.
