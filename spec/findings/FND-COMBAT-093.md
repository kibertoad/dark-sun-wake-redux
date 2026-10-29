---
id: FND-COMBAT-093
title: Window registration publishes the graph before region and child setup and keeps its own earlier stores on local failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3..3A8E:05EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068946..0x00068978
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A7D..0x00068A93
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at 14ef5f87; Capstone 5.0.9
environment: null
---

## Observation

Resident 3A8E:02B3 receives a window far pointer and two placement
words, retained in SI and DI before its stack-limit guard. A null
window returns AX FFFF. Its bounds gates compare wrapped word sums
of placement plus window dimensions +BE/+C0 with the corresponding
display bound plus one, using unsigned comparisons. Failure sets
DS:9DB0 to 1A and returns FFFF before publishing the window.
FND-CONFIG-033 and FND-CONFIG-036 identify the display-bound producers.

An admitted window first passes DS:A03D by far address to 1BF3:5945.
FND-CONFIG-175 reads that helper's pointer-setting effects. After its
return, the registration body tests current word DS:A103. If nonzero,
it stores current far DS:A105 into window+EE. If zero, it leaves
window+EE unchanged. It then stores the supplied window pointer into
DS:A105 at 032F and increments word DS:A103 at 0333, with word wrap.
There is no own membership, duplicate-window or old-link validation.
Thus this producer does not by itself establish a well-formed list.

Only after publishing that head and count does the body write placement
to window+96/+98, clear words +A6/+A8 and copy dimensions +BE/+C0
to +AA/+AC. It passes window+0A2 to 3CFA:0006. A nonzero returned
AX takes the common FFFF return without an own head/count restoration.
This is before the child loop, not after successful registration of all
controls. The helper's full window-specific input and alias contract is
not established by FND-COMBAT-088's separate APFM call form.

The child loop begins at index zero and tests the unsigned word at
window+F3 on each iteration. It computes each entry with the window's
segment and wrapped offset window+105+zero-extended byte +F2+30*i.
It passes entry tag+4, number+8 and a zero optional-output far pointer
to 38FF:0438, as FND-COMBAT-089 identifies. After that call returns,
03BF stores DX into entry+2 and 03C3 stores AX into entry+0. These
are separate segment-word then offset-word stores. The routine does
not first free or preserve the prior pointer as its local child result.

The full pointer is tested only after those stores. Null calls
39D1:0632, formats a diagnostic through 1000:374E and takes the
FFFF return if those callees return. This local error path has no own
list unlink, count decrement or release of earlier resolved children.
It has already overwritten the failing entry's previous pointer with
zero. The diagnostic callees' effects and storage bounds remain conditions.

For a nonnull pointer, the entry tag is read again after acquisition.
FND-PARTY-044 resolves the five-way dispatch to MENU at 0440,
BUTN at 04B3, APFM at 0514, ACCL at 0537 and EBOX at 0543.
An unrecognized tag skips construction and advances the loop. The
MENU, BUTN, APFM and EBOX constructor results are compared with the
full AX value FFFF; equality takes the same FFFF return. No own
release, pointer clear or graph rollback occurs on those branches.
Other constructor results advance the loop after each branch's own
field work. The entry pointer remains stored before the constructor
call. FND-COMBAT-088 supplies the bounded APFM constructor effects;
it does not prove all earlier or later children preserve that record.

Only exhausting the child loop reaches the successful tail. It clears
the low five bits of window+9E and sets mask value 0002, copies current far
DS:A0F1 to window+F5, clears +F9 and +FD as double words and returns
AX zero. Failures before this tail do not perform those own final stores.
FND-CONFIG-031 records the significance of these callback clears for
the later activation path, and FND-UI-045 records the flags separately.

Overlay 182's registration wrapper +00F6, file 68946..68978, rejects
a null input with SI FFFF. A nonnull input is passed unchanged with
the placement words to resident 3A8E:02B3. The returned AX is retained
in SI. Nonzero SI calls local +022D, then AX is restored from SI on
return; zero SI skips that helper. The wrapper has no own unlink or
release call. +022D, file 68A7D..68A93, passes near destinations 4592
and 4542 to 39D1:0460, then loads DS:4592 into AX. FND-CONFIG-170
reads the collector and its SI restoration; bounded input, distinct
storage, expected DS, guard and normal-return conditions remain explicit.
Under those conditions, a registration FFFF remains FFFF through this
wrapper rather than being replaced by the collected diagnostic code.

The window opener at overlay 182 +016B, file 689BB..68A3F, tests
that wrapper's AX before setting the window callback and activating it.
Nonzero branches to a diagnostic request through overlay 180 +0867.
If it returns, the opener returns a null far pointer. There is no own
unlink or release in that failure continuation. The request's effects,
including whether execution continues, are not replaced by a harmless
return assumption. FND-COMBAT-040 describes the opener's acquisition
and creation path independently.

## Interpretation

This producer's local order is graph publication, window-region setup,
per-child acquisition and pointer storage, per-child construction, then
success-state and callback initialization. Being in the published graph
does not alone prove that every child was successfully constructed or
that the success tail initialized the window's callbacks. Likewise,
the caller receiving null does not itself prove that earlier graph or
child stores were rolled back before that return.

Q-COMBAT-051 can reuse this particular registration producer and its
local failure ordering. The remaining questions concern inputs reaching
it, other graph/table producers, payload identity, actual admitted failure
or success paths, lifetime, aliases and preservation through callees and
later writers. No nonzero APFM callback or reachable partial native window
is inferred from the existence of the local failure branches.

## Alternatives

The inspected bodies have no own rollback, but callees, other callers
or a terminating diagnostic can change which states remain observable.
A registered pointer could also be re-used or changed outside this
producer. The zero-count path's unchanged link is not evidence that a
nonzero stale link is admitted in the original. No complete caller or
writer census is claimed.

The tag passed to acquisition and the tag used for construction are
separate reads with a call between them. A preservation proof must bind
those bytes, the pointer slot and the payload across that interval; this
reading does not freeze them. The split pointer stores establish their
own order, without asserting concurrent observation or a timing effect.

## How to reproduce

Use resident_listing.py on the licensed installed DSUN.EXE for
3A8E:02B3..3A8E:05EF. Use overlay_listing.py with descriptor 182
and file ranges 68946..68978, 689BB..68A3F and 68A7D..68A93.
All range values are hexadecimal; the tools verify the build length
and XXH3-128. The resident tool applies MZ relocations at load segment
1000; the overlay tool checks descriptor/fixup mappings. The wrapper
call to 3A8E uses descriptor 40 and the collector call descriptor 39;
the opener's error call uses descriptor 180's +0867 trampoline target.

Follow each failure edge from before and after 032F/0333 to the common
return, retain 03BF before 03C3, and compare the success-only tail
with every earlier exit. Reuse FND-PARTY-044's five tag targets and
FND-CONFIG-170's error-collector contract, rather than assuming all
error calls restore registration state. No game process or emulated
function call is used.
