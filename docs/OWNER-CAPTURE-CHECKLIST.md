# Owner capture checklist

Use this checklist only when a listed evidence gate needs a controlled native
observation. It is deliberately small and does not ask for a general
playthrough. Screenshots remain local-only original content and must not be
committed or sent through Git.

## Capture protocol

1. Start the supplied GOG edition through its normal DOSBox launcher. Do not
   use a different executable, saved game, mod, display scaler, or automated
   input tool.
2. Before the first capture, note the local start time including UTC offset.
   After the last capture, note the matching end time. Reply with both times
   and the checklist identifiers that were completed.
3. Capture every listed frame with DOSBox's built-in **Ctrl+F5** command. Do
   not use an operating-system screen capture or crop the result.
4. Do not rename, edit, or move the generated files before confirmation. The
   agent will inspect only screenshots in the configured DOSBox capture folder
   whose timestamps fall within the owner-confirmed time window.
5. Where a step says “restore,” return the control to the observed initial
   state before continuing. If a control does not visibly change, capture that
   state once and record it as “no visible change”; do not keep clicking.

## Preferences gate

Purpose: establish native defaults, selected/unselected frame feedback,
bounded slider behavior, description placement, and About-page presentation.
It does **not** ask for save-file inspection, audio recording, combat, or
campaign progress.

Enter any stable in-world area, open the Game Menu, then Preferences. Keep the
game at its default window size for the entire sequence.

| ID | Native action | Required capture/record |
|---|---|---|
| P0 | Open Preferences without first changing a Preferences control. | Capture the initial page. Record which difficulty text, description, and control frames appear selected, if any. |
| P1 | Click Music on/off once, then click it again to restore. | Capture after each click. |
| P2 | Click Sound Effects on/off once, then click it again to restore. | Capture after each click. |
| P3 | Click Animations on/off once, then click it again to restore. | Capture after each click. |
| P4 | Click Voice Effects on/off once, then click it again to restore. | Capture after each click. |
| P5 | For Music Volume, click the decrease control until the first state that has no further visible change; then click increase until the first state that has no further visible change. | Capture the initial state, the first changed state in each direction, and each endpoint. State the number of clicks from initial to each endpoint. Restore the initial state. |
| P6 | Repeat P5 for Sound Effects Volume. | Capture the initial state, the first changed state in each direction, and each endpoint. State the number of clicks from initial to each endpoint. Restore the initial state. |
| P7 | Move Difficulty in one direction until the first state with no further visible change, then traverse to the opposite endpoint. | Capture every distinct visible difficulty/description state and both endpoints. State the number of clicks between successive distinct states. Restore the initial state. |
| P8 | Open About. | Capture the first About page. Dismiss it by the native action and capture the immediately returned Preferences page. Record the dismissal input used. |
| P9 | Use Return, then reopen Preferences through the Game Menu. | Capture the reopened page and note whether it matches P0 after all restores. |

If a required control is unavailable, disabled, or the screen differs before P0,
stop the sequence, capture that frame, and report the exact point reached. That
is evidence; do not substitute an inferred sequence.

## What to reply with

Reply with one line containing:

```text
Preferences captures P0-P9: YYYY-MM-DD HH:MM ±HH:MM to YYYY-MM-DD HH:MM ±HH:MM; exceptions: <none or IDs/reason>.
```

After that confirmation, the repository workflow permits bounded inspection of
only the matching new DOSBox screenshots. Until then, Preferences defaults,
range/step behavior, frame-state mapping, and About geometry remain unknown.
