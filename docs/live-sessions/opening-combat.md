# opening-combat

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-COMBAT-001 (queue/COMBAT.md, Live session).
- Blocks: slice 4.
- Length: about 20 minutes.

Establishes the first native combat state's entry, its available controls, one
accepted action, turn progression, and exit. It does not ask for repeated
outcomes, save-file inspection, speed measurements, or a general playthrough.
Start a fresh normal launch with default Preferences. Reach one ordinary combat
through the game's normal input only; do not load a save, alter difficulty,
use a debug facility, or repeat the encounter to seek a different result. Keep
the default window size for every capture.

## Script

1. C0, Q-COMBAT-001. Immediately before the normal action that begins combat.
   Captures: the stable pre-entry frame. Record the input used to begin the
   transition and the active cursor or mode if visible.
2. C1, Q-COMBAT-001. Wait for the first stable combat frame. Captures: that
   frame. Record every visibly available command or control, the selected
   party member or active indicator if any, target feedback if any, and
   whether all party members are displayed.
3. C2, Q-COMBAT-001. If combat remains stable, press target-next (`N`) once,
   then target-previous (`P`) once only if the first key leaves combat active.
   Do not assume either produces a target-selection state. Captures: each
   resulting stable state, including an unchanged one. Record the exact keys
   and whether any cursor, actor, panel text, or other feedback changed. Do not
   continue cycling.
4. C3, Q-COMBAT-001. Click one visible enemy once, in the normal action mode
   that begins combat interaction. Do not use a confirmation click or try to
   select a target first. Captures: immediately before the click and the first
   stable state after it resolves. Record the pointer location, the exact input
   sequence, cursor feedback, approach and strike presentation, and every
   changed display or message. If the click produces no visible action,
   capture that state and stop.
5. C4, Q-COMBAT-001. On the next available turn, use exactly one visibly
   available turn command: Guard (`G`), Wait (`W`), or end turn (`Q`).
   Captures: immediately before and after it resolves. Record the key used,
   whether control or selection advanced, and every changed command or target
   state. Do not try the other two commands in the same encounter.
6. C5, Q-COMBAT-001. Let the next active-state change settle without further
   input. Captures: the resulting stable state. Record whether the same party
   member, another party member, or a non-party actor appears active, as far as
   the screen shows it.
7. C6, Q-COMBAT-001. Continue only through the ordinary remaining encounter
   until its first return to exploration, if that happens without a long or
   ambiguous sequence. Captures: the last stable combat state and the first
   stable exploration state. Record the visible outcome and where the game
   returns to. If the encounter cannot be concluded safely, stop after C5 and
   say so.

Agent, after confirmation: record entry, the controls shown, the accepted
action, the active-state changes and the exit as dynamic findings with each
capture's `xxh3`. A run from a fixed starting state with repetitions becomes an
experiment instead.
