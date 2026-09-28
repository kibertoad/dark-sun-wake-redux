---
id: RULE-CONFIG-005
title: The Preferences message-delay adjustment
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-010, FND-UI-034, FND-TIME-004, FND-CONFIG-017, FND-CONFIG-018, FND-CONFIG-030, FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033, FND-CONFIG-035]
conflicting: []
split_with: []
related: [SCR-UI-007]
---

## Summary

The first pair of Preferences arrows has the hover label `MESSAGE DELAY` and
changes a word in steps of eight. The left arrow stops at 20; the right branch
has no upper clamp of its own (FND-UI-034, FND-CONFIG-010).

## When it runs

The player clicks `BUTN/16305` to lower the value or `BUTN/16304` to raise it
on the Preferences screen (SCR-UI-007, FND-CONFIG-010).

## Parameters

`button`, one of those two control numbers.

## Inputs

`message_delay_value`, the unsigned word the game keeps at `DS:26B7` in
BLD-GOG-EN-1.1. It is 50 in the executable's loaded image, but a new game's
starting value has not been established. It is not one of the nine bytes of
`PREF/100` (FND-CONFIG-010).

## Procedure

```text
if button == 16305:
    if message_delay_value <= 28: message_delay_value = 20
    else: message_delay_value = message_delay_value - 8
else if button == 16304:
    message_delay_value = (message_delay_value + 8) modulo 65536
```

## Outputs

The screen redraws the upper bar from `message_delay_value - 20`. An overlay
172 routine multiplies the word by 100 with 16-bit arithmetic and passes the
result as milliseconds to the timer-chip wait. At the loaded-image value 50,
that call requests 5,000 ms. Several text-message paths call that routine;
the wait follows successful acquisition and setup of `WIND/10501`. Which
calls pass that gate in live states still needs a complete reading
(FND-CONFIG-010, FND-TIME-004, FND-CONFIG-017, FND-CONFIG-018).

## Edge cases

The lower boundary is 20 even when a loaded value is already below it. The
increase branch has no explicit upper clamp, so 16-bit wrap is possible
(FND-CONFIG-010).

## What the sources say

SRC-MANUAL-1994 calls the upper bar music volume, in conflict with the shipped
hover text and click branch (RULE-CONFIG-003).

## Differences between builds

None known.

## Open questions

- Whether another path imposes an upper limit on `message_delay_value`, and
  whether a new game replaces the loaded-image value 50 (Q-CONFIG-007).
  One reading is that 50 remains the starting value; another is that new-game
  setup writes a different value. A complete reading of initialization and
  indirect or block writers would distinguish them.
- Which caller paths enter overlay 172's shared message routine
  (Q-CONFIG-008). FND-CONFIG-017 identifies resident calls, and
  FND-CONFIG-035 inventories 56 direct overlay calls. FND-CONFIG-042
  through FND-CONFIG-058 read each direct site's local condition, but
  FND-CONFIG-070 finds no address-taking fixup to this entry from another
  overlay. FND-UI-037 and FND-SAVE-010 trace the Save Game path into one
  call; FND-CONFIG-071 traces the save-capacity call from startup, and
  FND-CONFIG-072 exhausts the direct resident relocation sites.
  FND-CONFIG-073 adds five internal calls from the same overlay, and
  FND-CONFIG-074 traces the callback's frame registration and dispatch;
  FND-CONFIG-075 finds six of its seven targeted frames in the window.
  FND-CONFIG-076 traces the direct incoming routes to overlay 187's save
  and cinematic message sites. FND-CONFIG-077 traces the direct handler
  and script routes to overlay 204's rest message sites. FND-CONFIG-078
  traces overlay 171's list message sites to a choice and window callback;
  FND-CONFIG-079 identifies the resident event-dispatch route into it, and
  FND-CONFIG-080 identifies two event-record discriminators for its message
  branch; FND-CONFIG-081 traces the conditional pointer-hit route to one,
  while FND-CONFIG-082 and FND-CONFIG-083 trace the keyboard packet and
  global fallback path for the other. FND-CONFIG-084 bounds one temporary
  global-callback replacement and restoration path. FND-CONFIG-085
  identifies overlay 182 paths that install a resident key callback or zero,
  without establishing their timing relative to the list. FND-CONFIG-086
  classifies the other twelve direct setter sites, but their order and
  reachability relative to the list remain unread. FND-CONFIG-087 shows
  overlay 209's no-other-classes message precedes and excludes its own
  callback registration; event exits can restore the saved prior pointer.
  FND-CONFIG-088 traces an overlay 213 callback's guarded event-two route
  into another shared message sink, with the event producer still open.
  FND-CONFIG-089 identifies a conditional button-pointer producer for that
  event identifier in one of the shipped windows. FND-CONFIG-090 traces
  that callback's event-bit threshold to the mouse packet before the shared
  message branch. FND-CONFIG-091 traces guarded overlay 178 callers into
  overlay 175's two message branches.
  Other shared sinks' incoming paths and computed or unrelocated pointer
  calls remain open. One
  reading is that all listed sites can be reached under their local
  guards; another is that some are blocked by earlier caller state.
  Complete incoming-path and indirect-call readings would separate them.
- Whether later state can prevent `WIND/10501` acquisition or
  registration during a message (Q-CONFIG-010). The wait requires a
  nonzero far pointer at `0300:0007`, set by the
  `WIND/10501` acquisition and setup call. The shipped image fields bypass
  image registration failures, and all graphics initializer bounds admit the
  window; resource acquisition and possible later bounds writes
  remain open (FND-CONFIG-011, FND-CONFIG-017, FND-CONFIG-018,
  FND-CONFIG-030, FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033,
  FND-CONFIG-034, FND-CONFIG-035, FND-CONFIG-036, FND-CONFIG-063,
  FND-CONFIG-037,
  FND-CONFIG-038, FND-CONFIG-039, FND-CONFIG-040, FND-CONFIG-041,
  FND-CONFIG-042, FND-CONFIG-043, FND-CONFIG-044, FND-CONFIG-045,
  FND-CONFIG-046, FND-CONFIG-047, FND-CONFIG-048, FND-CONFIG-049,
  FND-CONFIG-050, FND-CONFIG-051, FND-CONFIG-052, FND-CONFIG-053,
  FND-CONFIG-054, FND-CONFIG-055, FND-CONFIG-056,
  FND-CONFIG-057, FND-CONFIG-058, FND-CONFIG-059,
  FND-CONFIG-060, FND-CONFIG-061, FND-CONFIG-062,
  FND-CONFIG-064, FND-CONFIG-065, FND-CONFIG-066,
  FND-CONFIG-067, FND-CONFIG-068, FND-CONFIG-069,
  Q-TIME-003).
  One reading is that the present resource makes every call entering the
  routine pass the
  gate (FND-UI-001); another is that resource acquisition, bounds or child
  registration fails depending on state (FND-CONFIG-018, FND-CONFIG-030,
  FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033, FND-CONFIG-034,
  FND-CONFIG-037, FND-CONFIG-038, FND-CONFIG-039,
  FND-CONFIG-040, FND-CONFIG-041, FND-CONFIG-042,
  FND-CONFIG-043, FND-CONFIG-044, FND-CONFIG-045,
  FND-CONFIG-046, FND-CONFIG-047, FND-CONFIG-048,
  FND-CONFIG-049, FND-CONFIG-050, FND-CONFIG-051,
  FND-CONFIG-052, FND-CONFIG-053, FND-CONFIG-054,
  FND-CONFIG-055, FND-CONFIG-056, FND-CONFIG-057,
  FND-CONFIG-058, FND-CONFIG-059, FND-CONFIG-060,
  FND-CONFIG-061, FND-CONFIG-062, FND-CONFIG-064,
  FND-CONFIG-065, FND-CONFIG-066, FND-CONFIG-067,
  FND-CONFIG-068, FND-CONFIG-069).
  Reading indirect archive and bounds changes, and the remaining cleanup
  callers, would separate the code-decided parts.
- Whether the Save Game completion message passes the window gate and its
  visible duration changes with Message Delay in the owner's GOG build
  (Q-CONFIG-009). FND-CONFIG-046 establishes the direct success-branch
  message call, while FND-CONFIG-018 and FND-CONFIG-030 through
  FND-CONFIG-034 locate later gates. One reading is that acquisition
  succeeds and the message waits; another is that a live I/O or setup
  outcome bypasses the wait. The two-setting owner session in
  `docs/live-sessions/preferences.md` would distinguish them for this
  specific path, without settling every message caller.
