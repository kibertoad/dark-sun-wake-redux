# preferences

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-CONFIG-001 (queue/CONFIG.md, Live session).
- Blocks: slice 3.
- Length: about 20 minutes.

Establishes the native Preferences defaults, selected and unselected frame
feedback, slider endpoints and steps, description placement, and the About
page. It does not ask for save-file inspection, audio recording, combat, or
campaign progress. Enter any stable in-world area, open the Game Menu, then
Preferences, and keep the game at its default window size for the whole
script.

## Script

1. P0, Q-CONFIG-001. Open Preferences without first changing a Preferences
   control. Captures: the initial page. Record which difficulty text,
   description, and control frames appear selected, if any.
2. P1, Q-CONFIG-001. Click Music on/off once, then again to restore.
   Captures: after each click.
3. P2, Q-CONFIG-001. Click Sound Effects on/off once, then again to restore.
   Captures: after each click.
4. P3, Q-CONFIG-001. Click Animations on/off once, then again to restore.
   Captures: after each click.
5. P4, Q-CONFIG-001. Click Voice Effects on/off once, then again to restore.
   Captures: after each click.
6. P5, Q-CONFIG-001. For Music Volume, click the decrease control until the
   first state with no further visible change, then click increase until the
   first state with no further visible change. Captures: the initial state,
   the first changed state in each direction, and each endpoint. State the
   number of clicks from the initial state to each endpoint, then restore it.
7. P6, Q-CONFIG-001. Repeat P5 for Sound Effects Volume, with the same
   captures and counts, then restore.
8. P7, Q-CONFIG-001. Move Difficulty in one direction until the first state
   with no further visible change, then traverse to the opposite endpoint.
   Captures: every distinct visible difficulty and description state and both
   endpoints. State the number of clicks between successive distinct states,
   then restore the initial state.
9. P8, Q-CONFIG-001. Open About. Captures: the first About page, then dismiss
   it by the native action and capture the Preferences page it returns to.
   Record the dismissal input used.
10. P9, Q-CONFIG-001. Use Return, then reopen Preferences through the Game
    Menu. Captures: the reopened page. Note whether it matches P0 after all
    restores.

Agent, after confirmation: measure each control's frame, the slider positions
and the description placement against P0, and record the dismissal and restore
behaviour as dynamic findings with each capture's `xxh3`.
