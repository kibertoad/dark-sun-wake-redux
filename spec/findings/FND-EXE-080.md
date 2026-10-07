---
id: FND-EXE-080
title: Startup guard publishes before reverse-order callback dispatch and returns the later atexit result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C45C0..0x005C45DC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4570..0x005C45BD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401250..0x0040125B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB310..0x002EB38B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035949A..0x003594A0
tool: Ghidra 12.1.3 PUBLIC bounded instruction windows and physical PE table/import reading
environment: null
---

## Observation

FND-EXE-079's later startup callee directly calls `0x005C45C0`. This helper
reads the full word at `0x02427BC0` and tests it against zero. Any nonzero
word restores the frame and returns with that loaded word still in the
return register. Zero instead directly stores one to that word, restores
the frame and tail-jumps to `0x005C4570`, retaining the original caller's
return address. The store precedes every table read and callback in that
callee. No atomic compare/update, completion check or local rollback is
present. The guard's initial value and other writers remain conditional.

The callee first reads the full word at `0x006EBF10`. Exactly all ones selects
its sentinel-count scan. Any other word is used directly as the callback
count: zero skips dispatch, while nonzero enters dispatch from that count.
There is no signed-negative rejection or local maximum for this direct
count; four-byte scaled indexing and arithmetic are at 32-bit width.
The original helper reads no caller-supplied count or table-bound argument.

For the all-ones header, a counter begins at zero and the first word at table
plus four is compared against zero. A jump preserves those comparison flags
into the loop condition. While the compared word is nonzero, the counter
increments and the next word at table plus four plus counter times four is
read and tested. The first zero stops counting; the counter becomes the
number of nonzero words before it. This code has no local scan limit or
independent end pointer, and this static reading does not give arbitrary
mutated tables a bound.

The physical table at shipped offset `0x002EB310` contains an all-ones
header, 29 nonzero full words, then a zero word at `0x002EB388`. The
terminator ends at `0x002EB38B`, and each queried word is physically mapped
through the PE section. An initial sixteen-word source-query cap did not
reach the terminator; a bounded 128-word query did. The initial cut was not
reported as an empty or complete table.

### Dispatch and registration

The dispatch counter is held in a saved working register. While nonzero,
the callee freshly reads/calls the full pointer at table plus counter times
four, without pushing a newly populated argument. After normal return it
decrements the counter and repeats until zero. It does not test, save or
combine the callback's return. Correct callback ABI preservation of the
saved counter, normal return and an unchanged table remain dependencies
of the composed sequence; later callbacks have not all been read.

For the shipped table, this selects slots 29 down through one, under those
conditions. The first selected prefix is physically:

| Selection order | Table slot | Shipped slot offset | Target |
|---|---|---|---|
| First | 29 | `0x002EB384` | `0x005FF590` |
| Second | 28 | `0x002EB380` | `0x005FDB40` |
| Third | 27 | `0x002EB37C` | `0x005FDAB0` |
| Fourth | 26 | `0x002EB378` | `0x005FC860` |

FND-EXE-081 reads these four targets, keeping neighboring entries apart.
No target is identified by a nearby initializer or inferred function name.
The other selected targets and possible table/alias effects remain open.

After dispatch reaches zero, including an initially zero count, the helper
reserves twelve outgoing bytes and pushes `0x005C4540` in one full-word slot,
then calls `0x00401250`. That wrapper loads PE slot `0x02431958`, restores
its frame and jumps through the loaded full pointer. Physical import-table
reading identifies the slot as msvcrt.dll's atexit; the cited name range
includes NUL. The wrapper retains the helper's outgoing argument and return
address, with no extra call-return layer. FND-EXE-024/048's known malloc,
free and increment bindings provide physical-import controls.

The table helper restores its own saved register/frame and returns the
atexit result unchanged. It does not convert that result to a Boolean or
restore the guard on a nonzero result. The guard helper's zero-input route
therefore passes this later raw result through, whereas its nonzero-input
route returns the loaded guard. FND-EXE-079's immediate caller makes another
call without testing this result. Registration success, callback lifetime
and the stored `0x005C4540` callback's effects are not established here.

A guard value of one can thus precede incomplete callback processing, and
normal callback returns of zero, one or all ones do not locally stop the
counter loop. An exceptional transfer or failed registration has no shown
guard rollback. This is publication order, not a claim that the original
actually reenters, runs these paths concurrently or initializes successfully.
No interpreter, callback or API was executed.

## Interpretation

The later startup helper now has a precise guard-publication, header-count,
reverse dispatch and registration-return contract, with physical admission
for its first selected callbacks. Q-EXE-009 retains the remaining callbacks,
guard initializers/writers, ABI/interleaving, table lifetime and external
registration/cleanup effects. This does not establish a complete initialization
lifecycle or a successful original launch.

## Alternatives

Publishing only after all callbacks, interpreting every nonzero guard as one,
rejecting non-marker negative counts, dispatching forward from slot one,
calling the zero terminator, stopping on a callback return, registering only
for a nonempty table, or returning a saved success Boolean is ruled out.
The initial source-query cap cannot replace the reader's actual terminator;
the unchanged shipped count is not an all-input runtime bound.

## How to reproduce

Verify FND-EXE-011's PE identity and FND-EXE-079's incoming startup call. Read
22 instructions from `005C45C0`, 42 from `005C4570` and five from `00401250`,
restricting claims to the cited bodies and excluding later entries after gaps.
Map `0x006EBF10` to physical bytes and read consecutive full words, checking
complete four-byte section bounds. Use a sixteen-word cap as a partial-query
control, then 128 words, stopping only on the first zero after the header and
failing if the cap is reached. Record the header, terminator, count and exactly
slots 29, 28, 27 and 26 as concrete target inputs; leave unread targets open.
Resolve exact import slot `0x02431958`, with known controls `0x02431858`,
`0x024319D0`, `0x024319A0`. Require import descriptor termination inside its
directory, lookup termination within 4096 entries and names within 128 bytes.
Trace the full guard, pre-dispatch publication, tail frame, preserved scan flags,
fresh target loads, decrement after normal callback return, outgoing registration
slot and raw return after restoration. Keep reports local and execute no original.
