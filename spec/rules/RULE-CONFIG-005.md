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
  call; FND-CONFIG-123 traces the save-capacity call from startup, and
  FND-CONFIG-072 exhausts the direct resident relocation sites.
  FND-CONFIG-073 adds five internal calls from the same overlay, and
  FND-CONFIG-074 traces the callback's frame registration and dispatch;
  FND-CONFIG-075 finds six of its seven targeted frames in the window.
  FND-CONFIG-076 traces the direct incoming routes to overlay 187's save
  and cinematic message sites. FND-CONFIG-077 traces the direct handler
  and script routes to overlay 204's rest message sites. FND-CONFIG-124
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
  message branch. FND-CONFIG-092 separates overlay 175's setup from its
  registered frame handler: its value-32 branch enters the helper with
  two conditional message paths. FND-CONFIG-093 supplies the six matching
  shipped frames and enabled mask. FND-CONFIG-094 traces mouse-packet
  bit 4 through the resident APFM dispatcher to the value-32 handler.
  FND-CONFIG-095 traces hit-selection refresh and earlier helper gates;
  FND-CONFIG-096 shows the intervening window return is ignored and its
  child branches skip the shipped APFM graph. Physical input mapping
  remains unread. FND-CONFIG-097 and FND-CONFIG-098 bound the
  selection and old-window helpers to their handled tags; FND-CONFIG-099
  bounds the position/region writes away from the selected-pointer fields.
  Other prior control types and runtime state changes remain open.
  FND-CONFIG-100 traces overlay 176's two message sites to overlay 193's
  range- and byte-gated selector. FND-CONFIG-101 inventories eleven
  declared calls into that selector. FND-CONFIG-102 traces overlay 172's
  zero-gate code source, with a resident value-64 producer in FND-CONFIG-104.
  FND-CONFIG-103 excludes overlay 173's direct selector route by its
  literal nonzero byte. FND-CONFIG-125 reads overlay 174's three
  zero-gate routes and their local code sources; the producing helpers,
  table contents and callers remain open. FND-CONFIG-106 excludes
  overlay 189 and 213's three literal-nonzero routes. FND-CONFIG-126
  reads overlay 211's two zero-gate code sources, and FND-CONFIG-108
  reads overlay 208's stored gate and code. All eleven declared calls
  have local gate readings; their producing state, remaining guards and
  upstream reachability remain open. FND-CONFIG-109 traces overlay
  208's input stores to setup arguments; FND-CONFIG-111 reads six
  declared setup calls. FND-CONFIG-110 traces two local selector-caller
  routes, including conditional repetition. Data producers, later writes,
  prior guards and helper effects remain open. FND-CONFIG-127 traces
  overlay 208 entry 006B to a resident mode-five dispatch that forwards
  two input words. FND-CONFIG-128 traces a resident event-five/value-64
  state-one branch that forwards the input record to that dispatch; the
  upstream entry's incoming routes and event producer remain open.
  FND-CONFIG-114 records bounded negative reference inventories,
  while FND-CONFIG-129 reads state-two/three paths that need a later
  state-one value to dispatch. One reading is that computed registration
  supplies a reachable producer; another is that the entry is unreachable
  in this build. Registration and indirect-dispatch readings would
  distinguish them; the inventories alone do not. Helper state writes
  also remain open. FND-CONFIG-130 reads a record-taking wrapper
  whose result can be remapped, without settling its callee's effects.
  FND-CONFIG-117 identifies a separate temporary-five assignment and
  saved-word restoration; its relation to the event handler remains open.
  FND-CONFIG-131 reads the intervening wrapper and pending-record drain
  with state-one/five return-region gates. FND-CONFIG-119 traces seven
  local wrapper calls in two guarded groups; earlier inputs, local
  producers, transitive effects and handler timing remain open.
  FND-CONFIG-132 reads first-pass threshold/index producers and the
  second-pass grouped-call consumption. FND-CONFIG-133 reads the
  iterator selection contract; FND-CONFIG-144 and FND-CONFIG-135
  read local index, table and flag producers. FND-CONFIG-136 traces
  the flag writer to pre-handler opcode dispatch. FND-CONFIG-137
  reads nested parameter dispatch and ordinary save/restore spans,
  without establishing rollback of the flag. FND-CONFIG-143 reads the
  setup callee's local output and conditional table writes. FND-CONFIG-145
  reads its traversal helpers and conditional count consumption; buffer
  validity, the near-pointer segment relationship, table producers and
  actual input ranges remain open. FND-CONFIG-148 bounds the literal
  selector-base query without finding a producer. FND-CONFIG-147 resolves
  the zero-gate setup's cleared slot and local traversal return; setup
  invocation, bypass state and later iterator inputs remain open.
  FND-CONFIG-149 reads the declared direct setup caller's local join.
  FND-CONFIG-150 reads direct post-setup selector assignments and a
  metadata-buffer argument; resource-call effects, gate producers and
  later inputs remain open. FND-CONFIG-151 reads the metadata length
  and transfer contracts; FND-CONFIG-152 traces the bounded request
  through pointer normalization to an interrupt. Record stability, resource
  bytes and operating-system outcomes remain open. FND-CONFIG-153
  identifies the installed FNFO source and lengths; FND-CONFIG-154 gives
  conditional selector cases, retaining the width-prefix direction flag.
  FND-CONFIG-155 reads the zero-mode
  pre-setup helper without finding a direct gate producer or registration.
  FND-CONFIG-156 reads the following memory initializer's local
  effects and failure-word truncation; passing the caller's low-byte
  zero check does not establish successful initialization. FND-CONFIG-157
  locates an earlier OBJEX.GFF registration attempt before both mode
  branches, retaining the open outcome and intervening archive effects.
  FND-CONFIG-158 bounds the three pre-join graphics helpers' local
  writes and DS restoration, retaining BIOS and hardware outcomes.
  FND-CONFIG-159 reads the nonzero-mode helper's local branches,
  pointer setter and video-reset target; external effects remain open.
  FND-CONFIG-160 reads the post-setup script's status gate and local
  entry order. FND-SCRIPT-019 corrects the cache and transfer paths,
  while FND-SCRIPT-020 bounds the guarded buffer reset. FND-SCRIPT-021
  reads replacement selection and invalidating writes; FND-SCRIPT-022
  bounds allocation search and restart paths. FND-SCRIPT-023 and
  FND-CONFIG-161 read error-entry ordering and its shared helper's local
  branches and polling gate. FND-CONFIG-162 reads the first local callee's
  registration and polls; FND-CONFIG-163 bounds its setter guard and
  mode-one callback-loop bypass. FND-CONFIG-164 traces the common poll's
  driver-result predicate and aliased outputs. FND-CONFIG-165 reads
  the pointer wrapper's returned-zero path; FND-CONFIG-166 bounds
  state-clear/status entry gates. FND-CONFIG-167 reads runtime dispatch,
  returned-result conversions and shared-slot DS restoration.
  FND-CONFIG-168 reads a child failure after list/count changes and the
  caller's ignored result. FND-CONFIG-179 bounds the following helper's
  separate mode gates, corrected callback/mask layout and ungated resource
  results. FND-CONFIG-180 reads the local bitmap/registration gates;
  FND-CONFIG-181 bounds archive-handle, filename and segment conditions.
  FND-CONFIG-170 resolves local retained-result restoration through the
  collector; FND-CONFIG-171 reads the following resident callback gates,
  fresh targets and fixed word writes. FND-CONFIG-172 distinguishes child
  recursive error propagation from an origin; FND-CONFIG-173 bounds the
  intervening list helper's gates and checked/ignored results.
  FND-CONFIG-174 bounds the EBOX dependency's common zero result;
  FND-CONFIG-175 bounds bracketed SI/DS preservation on guarded-helper
  bypass paths, retaining aliases, shared fields and actual guards.
  FND-CONFIG-176 distinguishes region setup returns from a count-16 append
  rejection; FND-CONFIG-177 bounds staged outputs, sentinel bypasses and
  pair-expansion capacity cases. FND-CONFIG-178 bounds the larger helper's
  private checked appends and rules out compaction at the named self-copy.
  Full region geometry and input evidence stays open. Actual cached
  inputs, resource and opcode effects, I/O and error outcomes remain open.
  Runtime registration, loads, valid records, reachable inputs, other writers
  and intervening helper effects remain open.
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
  FND-CONFIG-046, FND-CONFIG-121, FND-CONFIG-048, FND-CONFIG-049,
  FND-CONFIG-050, FND-CONFIG-051, FND-CONFIG-052, FND-CONFIG-053,
  FND-CONFIG-054, FND-CONFIG-055, FND-CONFIG-056,
  FND-CONFIG-122, FND-CONFIG-058, FND-CONFIG-059,
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
  FND-CONFIG-046, FND-CONFIG-121, FND-CONFIG-048,
  FND-CONFIG-049, FND-CONFIG-050, FND-CONFIG-051,
  FND-CONFIG-052, FND-CONFIG-053, FND-CONFIG-054,
  FND-CONFIG-055, FND-CONFIG-056, FND-CONFIG-122,
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
