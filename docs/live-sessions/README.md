# Live session requests

One file per run of the original an agent has asked for, named after it:
`docs/live-sessions/opening-combat.md`. The
[work protocol](../../vendor/upstream/work-protocol.md#live-sessions) (lines 239-249) sets the
rules. In this repository coding agents never launch, control, capture or stop
DOSBox, so every live session is run by the repository owner alone: the owner
plays the script and takes the captures, and the agent reads them afterwards.
A request's Script is therefore the exact checklist the owner follows, and its
Agent step says what the agent measures in the captures once the owner has
confirmed them.

The owner answers a request by editing its Status line. Until it says
`accepted`, the work goes on without it. After the session the Status becomes
`held` with the date, the results go into the spec in one research batch per
area, and the last of those batches deletes the request. A declined request
stays, so that it is not asked again without something new. The files here
are the list of open requests.

A request:

```markdown
# opening-combat

Status: requested
<!-- or: accepted, YYYY-MM-DD / declined: the owner's reason / held, YYYY-MM-DD -->

- Build: BLD-GOG-EN-1.1, the owner's GOG installation under its own DOSBox launcher.
- Settles: Q-COMBAT-001 (queue/COMBAT.md, Live session).
- Blocks: slice 4.
- Length: about 20 minutes.

## Script

1. C0, Q-COMBAT-001. Native action. Captures: what to capture and record.
```

## Capture protocol

1. Start the supplied GOG edition through its normal DOSBox launcher. Do not
   use a different executable, saved game, mod, display scaler, or automated
   input tool.
2. Before the first capture, note the local start time including UTC offset.
   After the last capture, note the matching end time. Reply with both times
   and the script step identifiers that were completed. Alternatively,
   authorize inspection of a specific local date-and-hour slot including UTC
   offset; that means the interval from `HH:00` through, but not including,
   the next hour.
3. Capture every listed frame with DOSBox's built-in Ctrl+F5 command. Do not
   use an operating-system screen capture or crop the result. DOSBox saves the
   emulated video memory at the game's 320x200 canvas in the palette the game
   set, which is the form the standard asks a capture to take.
4. Do not rename, edit, or move the generated files before confirmation. Unless
   the durable folder-wide authorization below applies, the agent inspects only
   screenshots in the configured DOSBox capture folder whose timestamps fall
   within the owner-confirmed time window or owner-authorized date-and-hour
   slot. It may report provisional geometric or pixel observations, but asks
   the owner to confirm a proposed semantic label (a named screen, actor, turn,
   action, or transition) before recording or using that label as evidence.
5. Where a step says "restore", return the control to the observed initial
   state before continuing. If a control does not visibly change, capture that
   state once and record it as "no visible change"; do not keep clicking.
6. If a screen differs before a listed step, a control is unavailable, or a
   step causes an unexpected state, capture the reached state once, stop that
   script, and report the exact point reached. That is evidence; do not
   substitute an inferred sequence.

Captures stay local-only original content. A finding that uses one gives its
`xxh3`, and the owner keeps the file under `GAME_DIR/captures/` named by that
hash.

### Replying

Reply with one line naming the request, the steps completed, the time window
and any exceptions:

```text
opening-combat C0-C6: YYYY-MM-DD HH:MM +HH:MM to YYYY-MM-DD HH:MM +HH:MM; exceptions: <none or IDs/reason>.
```

For a less formal review, the owner may instead authorize one local hour, for
example `review 2026-09-20 21:00 +03:00`. The agent then inspects only files
timestamped from 21:00:00 through 21:59:59 in that local offset. The owner may
instead authorize explicit screenshot filenames, for example
`review files dsun_009.png, dsun_011.png, dsun_012.png`, which authorizes only
those exact files. Semantic labels still require owner confirmation either way.

### Durable owner authorization

On 2026-09-21 the repository owner authorized inspection of every screenshot
in the configured DOSBox `capture` folder, including screenshots whose
filenames carry no timestamp. This does not authorize copying, committing,
distributing, renaming, or modifying captures. It also does not assign a
semantic identity to any image: the owner must still confirm a proposed screen,
actor, action, turn, or transition label before it is recorded or used as
evidence.
