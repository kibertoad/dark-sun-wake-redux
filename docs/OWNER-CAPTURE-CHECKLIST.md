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
   and the checklist identifiers that were completed. Alternatively, authorize
   inspection of a specific local date-and-hour slot including UTC offset; that
   means the interval from `HH:00` through, but not including, the next hour.
3. Capture every listed frame with DOSBox's built-in **Ctrl+F5** command. Do
   not use an operating-system screen capture or crop the result.
4. Do not rename, edit, or move the generated files before confirmation. Unless
   a durable folder-wide authorization is recorded below, the agent inspects
   only screenshots in the configured DOSBox capture folder whose timestamps
   fall within the owner-confirmed time window or owner-authorized date-and-hour
   slot. It may report provisional geometric or pixel observations, but will
   ask the owner to confirm a proposed semantic
   label—such as a named screen, actor, turn, action, or transition—before
   recording or using that label as evidence.
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

## Shipped-party gate

Purpose: identify the four resources selected by **START GAME**, and capture
the native party-overview composition without substituting a created party.

Start a fresh normal launch. Do not choose CREATE CHARACTERS, load a save, or
enter the character-transfer utility for this sequence.

| ID | Native action | Required capture/record |
|---|---|---|
| S0 | At the initial start screen, choose START GAME once. | Capture the first fully populated party-overview screen after the transition finishes. |
| S1 | If the overview exposes a native member-select or View Character action, open the first visible member and then return. | Capture the member screen and the returned overview. Record the exact input used to open and return. |
| S2 | Repeat S1 for each other distinct visible member, in on-screen order. | Capture each member screen and returned overview. Do not click an unknown blank/application area. |

If START GAME reaches an unexpected screen, or if a member action is not
available, capture the reached state once, stop, and report the exact point.
That outcome is preferable to inferring a slot partition.

## Inventory-selection gate

Purpose: establish the native inventory screen's visible item-label geometry
and the first result of selecting one observed label. It does **not** ask for
equipping, using, transferring, dropping, rearranging, saving, or inspecting
an unknown control.

Start a fresh normal launch and use the shipped party. Keep the default window
size for the complete sequence. The known `Longsword` and `Dagger` labels are
source-vocabulary leads only; their record identity, slot, quantity, owner,
and behavior remain unestablished.

| ID | Native action | Required capture/record |
|---|---|---|
| I0 | From a stable exploration or party screen, press the documented inventory key (`I`) once. | Capture the first settled inventory page. Record the exact input, the owner-confirmed active member/screen label, and the logical coordinates or bounding rectangles of every clearly visible item label. Do not infer a slot or item ID from placement. |
| I1 | If `Longsword` is visibly labelled, click once at the center of its rendered label. | Capture the first settled result. Record the pointer coordinate, every visible change, and whether a new panel/control appears. Do not click a newly revealed or unknown control; if a modal/panel opens, stop this sequence after its capture. |
| I2 | Only if I1 leaves the same inventory page stable and no new panel/control appears, restore the I0 state with the native Back/Return action that was visibly available, then click once at the center of the visible `Dagger` label. | Capture the first settled result and record the pointer coordinate and every visible change. Do not use, equip, transfer, drop, or rearrange either item. |

If the inventory key reaches an unexpected page, an observed label is absent,
or a selection causes an unexpected state, capture the reached state once,
stop, and report the exact point. An unchanged result is valid evidence; do
not repeat the click until it appears to work.

## Opening-combat gate

Purpose: establish the first native combat state's entry, available controls,
one accepted action, turn progression, and exit before a playable combat system
is designed. It does **not** ask for repeated outcomes, save-file inspection,
speed measurements, or a general campaign playthrough.

Start a fresh normal launch with default Preferences. Reach one ordinary combat
through the game's normal input only; do not load a save, alter difficulty,
use a debug facility, or deliberately repeat the encounter to seek a different
result. Keep the default window size for every capture.

| ID | Native action | Required capture/record |
|---|---|---|
| C0 | Immediately before the normal action that begins combat. | Capture the stable pre-entry frame. Record the input used to begin the transition and the active cursor/mode if visible. |
| C1 | Wait for the first stable combat frame. | Capture it. Record every visibly available command/control, the selected party member or active indicator if any, target feedback if any, and whether all party members are displayed. |
| C2 | If combat remains stable, press documented target-next (`N`) once and then target-previous (`P`) once only if the first input leaves combat active. Do not assume either produces a target-selection state. | Capture each resulting stable state, including an unchanged state. Record the exact keys and whether any cursor, actor, panel text, or other feedback visibly changed. Do not continue cycling. |
| C3 | Click one visible enemy once using the normal action/mouse mode that begins combat interaction. Do not use a confirmation click or separately attempt to select a target first. | Capture immediately before the click and the first stable state after it resolves. Record the pointer location, exact input sequence, visible cursor feedback, approach/strike presentation, and every changed display or message. If the click produces no visible action, capture that state and stop this sequence. |
| C4 | From the next available native turn, use exactly one visibly available turn command: Guard (`G`), Wait (`W`), or end turn (`Q`). | Capture immediately before and after it resolves. Record the key used, whether control/selection advanced, and every changed command or target state. Do not try the other two commands in the same encounter. |
| C5 | Allow the next native active-state change to settle without additional input. Do not wait for a visual turn-transition treatment. | Capture the resulting stable state. Record whether the same party member, a different party member, or a non-party actor appears active; report only what is visibly indicated. An unchanged visual transition is valid evidence. |
| C6 | Continue only through the ordinary remaining encounter until its first native return to exploration, if it occurs without a long or ambiguous sequence. | Capture the last stable combat state and the first stable exploration state. Do not wait for or manufacture a dedicated exit frame. Record the visible outcome and transition destination. If the encounter cannot be safely concluded from the observed state, stop after C5 and report that fact rather than manufacturing an outcome. |

If combat starts in an unexpected state, a listed action is unavailable, or a
capture differs before C0, capture the reached state once, stop that sequence,
and report the exact point. That is evidence; do not infer a turn model from
the manual or retry until it appears to work.

## What to reply with

Reply with one line containing:

```text
Preferences captures P0-P9 and/or shipped-party captures S0-S2: YYYY-MM-DD HH:MM ±HH:MM to YYYY-MM-DD HH:MM ±HH:MM; exceptions: <none or IDs/reason>.
```

For a less formal review, the owner may instead authorize one local hour, for
example `review 2026-09-20 21:00 +03:00`. The agent then inspects only files
timestamped from 21:00:00 through 21:59:59 in that local offset, and asks the
owner to confirm any proposed semantic screenshot label before relying on it.

The owner may instead authorize explicit screenshot filenames, for example
`review files dsun_009.png, dsun_011.png, dsun_012.png`. This authorizes only
those exact files; semantic labels still require owner confirmation.

**Durable owner authorization (2026-09-21):** the repository owner authorizes
inspection of every screenshot in the configured DOSBox `capture` folder. This
does not authorize copying, committing, distributing, renaming, or modifying
captures. It also does not assign a semantic identity to any image: the owner
must still confirm a proposed screen, actor, action, turn, or transition label
before it is recorded or used as behavior evidence.

For a new bounded combat request, include `combat captures C0-C6` in that line.
The durable folder-wide authorization above permits inspection of screenshots
whose filenames lack timestamps; it does not assign their semantic labels.

After semantic confirmation, the repository workflow may rely on the matching
DOSBox screenshots. Until then, Preferences defaults, range/step behavior,
frame-state mapping, About geometry, and combat behavior remain unknown.
