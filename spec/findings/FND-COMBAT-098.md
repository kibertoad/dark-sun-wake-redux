---
id: FND-COMBAT-098
title: The three known handler-restoration calls use a late local snapshot or a reread shared slot before further window work
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000595BF..0x000598F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059B76..0x00059C46
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094405..0x00094458
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000948DB..0x0009493C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794E1..0x0007950D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418..39D1:0430
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and trampoline_target.py at 3c878c68; Capstone 5.0.9
environment: null
---

## Observation

FND-UI-051 and FND-EXE-587 retain three calls that register a value
previously read from overlay 190 +11C1: overlay 172 +0479 and +0775,
and overlay 209 +178E. This reading binds their local sources, gates
and ordering, not an exhaustive new caller or writer census.

### Which handler is saved

The getter +11C1, file 79501..7950D, returns the segment word from
current DS:61A4 in DX and the offset word from DS:61A2 in AX. It
does not read resident DS:A0F1 or validate a target or window.
The setter +11A1, file 794E1..79501, writes its full argument to
DS:61A2, retains a local copy and passes that copy to 39D1:0418.
The resident setter writes DS:A0F1 after its stack-limit guard.
FND-CONFIG-162/FND-CONFIG-163 record this two-store ordering and
guard dependency. The getter's stored mirror is not an independent
measurement that both callback fields currently agree.

FND-UI-051 locates the Save/Load setup's registration of trampoline
5736:002F, which enters overlay 192 +0785, in both a window field
and this global setter. That target can therefore be the value returned
by the getter under the relevant state conditions; the following callers
do not test specifically for it.

### Local-frame snapshot in overlay 172

Overlay 172 +018F, file 595BF..598F5, does substantial setup and
window work before its getter call at +03A6. In particular it has
already requested window activation at +034E, an optional +0D9A
operation after a zero activation result, control/text changes and
28C9:2A81(1). The snapshot is not taken at routine entry.

Returned DX and AX are stored separately at SS:BP-2 and SS:BP-4.
The routine then registers trampoline 566A:0052, entering its own
+04C5, through the global setter. Its event-read loop continues while
current word DS:40D8 equals FFFF. When that test permits continuation,
further control-state and 28C9:2A81 calls precede the restore request.
At +0475 it pushes the full local double word SS:BP-4; +0479 passes
that value to the setter. There is no own intervening assignment of a
new handler value to that local slot after the snapshot.

The restore call is followed by local +0127 and, when DS:1431 is
nonnull, more state-dependent calls including activation of that window
through 3A8E:060D. Thus the restore request precedes cleanup and
possible callbacks; it is not the final operation before return. Valid
local storage and preservation through the intervening callees remain
conditions on the value actually pushed.

### Shared-slot producer in overlay 209

On overlay 209 +123E's setup branch at +12A5, 28C9:2ABD precedes
the getter call at +12AA. The caller stores returned DX to 4C4D:0011
at +12B4 and AX to 4C4D:000F at +12B9. These are the two words of
one shared far pointer, not a separate local snapshot for each restorer.
It then registers trampoline 57B0:0048, entering its own +169C,
before requesting its window through overlay 182 +016B. This interval
has no own save of a prior value of the shared slot for later nesting.
This finding does not establish every path that reaches that setup branch.

### The two shared-slot restore calls

| Caller and gate | Source read for the setter | Own continuation after the restore request |
| --- | --- | --- |
| Overlay 172 +0746, file 59B76..59C46; returns immediately when 4C4D:0007 is null. | After 28C9:2ABD and state clears, +076F pushes current double word 4C4D:000F; +0775 calls the setter. | Clears the separate A0F5 callback through 39D1:0430(0), calls 3A8E:0A26 with current 4C4D:0007, then clears that window slot without testing the removal AX. A 4C4D:0006 gate and DS:1431 gate admit later view/window operations. |
| Overlay 209 +177B, file 948DB..9493C; no initial window-pointer gate. | After 28C9:2ABD, +1788 pushes current double word 4C4D:000F; +178E calls the setter. | Clears A0F5 through 39D1:0430(0). Only DS:1431 zero admits a release request for the window at 55BD:0004. It then clears 55BD:0004 regardless of that branch and calls 28C9:2A81(1). |

Both sites reread the shared slot after an intervening call rather than
using a local value captured on their own entry. Neither has an own
clear or pop of 4C4D:000F after forwarding it. The first site's null
window exit bypasses restoration entirely, even if the shared slot still
holds a value. The second site does not itself require its window slot
to be nonnull before restoration. Later calls can affect the final
registration; this reading does not assume their effects are inert.

None of the three restore sites compares the forwarded pointer with
the Save/Load target, verifies the Save/Load window at 50AF:0000 or
gates forwarding on its mode byte 50AF:0004. Those are separate
lifetime and event-admission obligations, not tests performed here.

## Interpretation

The known restoration sites are not interchangeable with an unconditional
stack of live window handlers. One captures the stored mirror late in
its own setup and restores a local value; two forward a reread shared
slot whose producer is in a different setup path. Each restore request
also precedes additional work that can call into window or view code.

Q-COMBAT-046 can reuse these three local restoration sources and orderings.
Its remaining work is actual caller admission, mirror/shared-slot producers
and preservation, live window identity and event delivery. A forwarded
Save/Load pointer alone does not prove that its window remains live or
that a later event reaches the numbered-load branch. No native stale
handler, nested activation or completed window transition is inferred.

## Alternatives

With unchanged valid local/shared storage, admitted guards and preserving
callees, the requests can reinstall the previously saved target. A later
setter, a changed shared slot or additional window work can instead change
what is active at event delivery. The known producer does not prove the
shared slot is private to one activation, and the getter does not prove
the resident callback equals its mirror at every point.

FND-EXE-587/FND-UI-051 retain their original census scopes. This reading
neither excludes other computed writers/restorers nor establishes these
paths' native reachability. The event-loop input and exit word, validity of
window pointers and all callee preservation remain explicit dependencies.
No complete-reading declaration or callback execution is claimed.

## How to reproduce

Use overlay_listing.py on the licensed installed DSUN.EXE with descriptor
172 and hexadecimal ranges 595BF..598F5 and 59B76..59C46;
descriptor 209, 94405..94458 and 948DB..9493C; descriptor 190,
794E1..7950D. Inspect 209's containing 9439E..947E2 routine for
context without treating its unrelated rendering/rule children as read.
Use resident_listing.py for 39D1:0418..39D1:0430. The tools verify
the build length and XXH3-128 and map MZ relocations/FBOV fixups.

Resolve 566A:0052, 57B0:0048, 571F:0034 and 571F:0039 with
trampoline_target.py: they enter 172+04C5, 209+169C,
190+11A1 and 190+11C1 respectively. Descriptor 96 binds the shared
slot to 4C4D; descriptor 117 binds the Save/Load state to 50AF.
Track both pointer words, preserve the null-window bypass, and follow
the post-restore calls instead of treating the setter as a final-state
measurement. No game process or emulated function call is used.
