# preferences

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-CONFIG-001 (queue/CONFIG.md, Live session), for SCR-UI-007,
  RULE-CONFIG-001 to RULE-CONFIG-005 and FMT-CONFIG-003.
- Blocks: slice 3.
- Length: about 20 minutes.

Establishes the native Preferences defaults, selected and unselected frame
feedback, the visible message-delay lower endpoint and sound-effects volume
endpoints, description placement, and the About
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
6. P5, Q-CONFIG-001. Hover over the upper bar and capture its description.
   Click its decrease control once, then increase once to restore. Capture
   each state. Next, click decrease until the first state with no further
   visible change, stopping after 12 clicks even if it keeps changing.
   Capture that state and report the click count. Click increase three times,
   capturing the first changed state; report whether each click changed the
   bar. Leave this setting as reached, and note it for the P9 comparison.
7. P6, Q-CONFIG-001. For Sound Effects Volume, click decrease until the
   first state with no further visible change, then click increase until the
   first state with no further visible change. Capture the initial state,
   the first changed state in each direction, and each endpoint. State the
   number of clicks to each endpoint, then restore the initial bar position.
8. P7, Q-CONFIG-001. Move Difficulty in one direction until the first state
   with no further visible change, then traverse to the opposite endpoint.
   Captures: every distinct visible difficulty and description state and both
   endpoints. State the number of clicks between successive distinct states,
   then restore the initial state.
9. P8, Q-CONFIG-001. Open About. Captures: the first About page, then dismiss
   it by the native action and capture the Preferences page it returns to.
   Record the dismissal input used.
10. P9, Q-CONFIG-001. Use Return, then reopen Preferences through the Game
    Menu. Capture the reopened page. Compare the on-off and difficulty
    controls with P0; report the upper bar separately because P5 left it at
    a different value.
11. P10, Q-CONFIG-001. Return to the map. Press `F6` once and reopen
    Preferences. Capture the animation and voice button states, return to
    the map, then press `F6` again and reopen Preferences. Capture the states
    again and report whether they return. Do not
    interpret which setting changed from the key alone; describe the visible
    states and actions.

Agent, after confirmation: measure each control's frame, the slider positions
and the description placement against P0, and record the dismissal and restore
behaviour as dynamic findings with each capture's `xxh3`.
