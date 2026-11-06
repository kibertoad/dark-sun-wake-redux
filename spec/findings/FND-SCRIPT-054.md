---
id: FND-SCRIPT-054
title: Script cleanup clears cached setup separately from the state-gated working buffers and allocation failure flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000576BA..0x000577BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000579A2..0x00057AD2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007330C..0x00073340
tool: overlay_listing.py, direct_callers.py and window_operands.py at ce4c96d1; Python 3.12.10, Capstone distribution 5.0.9; Ghidra 12.1.3 bounded matches in repaired overlay view
environment: null
---

## Observation

This connected failure-lifecycle reading reuses FND-CONFIG-149/156's
setup wrapper, FND-SCRIPT-026's allocation/release layout,
FND-SOUND-081's zero-return release helper and FND-SCRIPT-053's flag
binding. Its question is which local cleanup operations can admit another
setup attempt, not whether a native failed allocation has been observed.
DSUN.EXE is 634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.

### Working-buffer cleanup requires state exactly two

Overlay169 +0422 first compares word4C0E:000B with two. Any other
value exits at +0534 without its pointer-release or status stores.
On the equality path it passes these seven current far pointers to
local release helper +0536, in order, publishing returned DX:AX to
each pointer after its call:

| Pointer field in 4C13 | Role from FND-SCRIPT-026 |
| --- | --- |
| 0249 | 32-byte working allocation |
| 032F | Local strings |
| 0333 | Local big numbers |
| 0337 | Local numbers |
| 033B | Local flags |
| 0251 | 300-byte working allocation |
| 0255 | Script buffer |

The complete +0536 helper conditionally calls 444C:0092 for a nonnull
input, then explicitly sets DX and AX to zero. Thus, under intact frames,
bindings and returning calls, each reached publication nulls its pointer
independently of the external release service's result. This does not
prove the service succeeds or returns. After all seven publications,
+0422 writes state one to 4C0E:000B and word one to 4C0E:0009.

### Outer cleanup has different unconditional publications

Overlay169 +0174 calls +0422 first, then passes the four global variable
block pointers 4C13:033F, 0343, 0347 and 034B and the trigger-pool
pointer at DS:40C0 to +0536. It publishes the returned zero pair after
each. These five releases are not guarded by a local state-two test.
Finally it writes state zero to 4C0E:000B and FFFF to 4C0E:0009.

The initializer's post-script failure branch provides a concrete call
ordering where this distinction matters: it writes FFFF to script state
at +015C before calling +0174 at +0165 (FND-CONFIG-156). With that
state retained, +0422 skips the working-pointer releases. Outer cleanup
still performs its five publications and final status writes. It therefore
does not locally null the seven working pointers on that path. Any
previously allocated working buffers are not passed to +0536 by this
cleanup path; no general claim about external allocator ownership follows.
An earlier inner initialization failure likewise supplies no automatic
state-two admission to the working releaser.

Bounded decompiler matches in the repaired overlay project confirm the
state-two guard, state-one publication and outer call/state-zero ordering.
Exact pointer words, call arguments and return widths are checked in the
instructions. No inferred decompiler signature supplies a success test.

### Clearing the retained result does not clear every failure mechanism

FND-EXE-614 already identifies wrapper overlay188 +0491. Its complete
body calls +0174 and then clears byteDS:193D before returning. The
known direct caller is overlay180 +07AA. The controlled direct-call query
also recovers the initializer's +0165 and wrapper's +0494 calls to +0174.
Computed calls remain outside this query.

An exact displacement window for DS:193D recovers the setup wrapper's
two reads and returned-AL store at +046C, plus +0491's zero store.
Both producer sites agree with independent FND-CONFIG-149/FND-EXE-614
controls. Its one raw extra candidate starts inside the established
returned-AL store instruction; it is not another established writer.
Adjacent wider writes, indexed/aliased destinations and other mechanisms
remain outside that exact displacement census.

The following three gates must therefore be kept distinct:

| State | Local consumer |
| --- | --- |
| DS:193D, retained result byte | Nonzero skips another call to the initializer |
| 4C0E:000B, script-state word | Exactly two admits working-buffer cleanup |
| 4C0C:0000, allocation flag byte | Zero admits allocation; exactly one fails the separate check |

The cleanup bodies and cache-clear wrapper have no own store clearing
the allocation flag. Under valid independent bindings and child effects
that preserve it, flag one can therefore remain after the cache is cleared.
The next setup wrapper then attempts initialization again, but the first
allocation is suppressed and the check fails; FND-CONFIG-156's FFFF
return can again become cached FF. Clearing DS:193D alone is not a
demonstrated allocation-recovery mechanism. This is a conditional local
sequence, not a native failure/retry observation or a permanent flag
invariant across all callers and aliases.

## Interpretation

Q-SCRIPT-019 has a finite cleanup/retry contract to reuse: working-pointer
nulling is state-gated; global/pool nulling and cached-result clearing are
separate later operations. Neither calling cleanup nor clearing the cache
proves that all pointers are null, that live storage is valid, or that a
later initialization succeeds. Actual phase admission, partial allocations,
flag/history and alias/restore mechanisms remain open. No whole-entry
complete reading, native allocator-failure outcome or queue closure follows.

## Alternatives

Treating +0174 as always releasing all working buffers omits +0422's
guard and the initializer's prior FFFF store. Equating the cached byte,
script state and allocation flag loses their independent producers and
tests. A zero pointer publication also does not prove external release
success; it is the local helper's explicit return value.

## How to reproduce

Use overlay_listing.py on the licensed source for descriptor169 ranges
576F4..577BB and 579A2..57AD2, and failure dispatch576BA..576F4.
Use descriptor188 range7330C..73344, with the wrapper ending73340.
Retain trigger-global-cleanup-stream.txt, trigger-working-cleanup-stream.txt,
trigger-failure-cleanup-dispatch.txt and trigger-setup-cache-wrappers.txt
under GAME_DIR/analysis/handler-census-20261106/.

Run direct_callers.py for188+0491 and169+0174 into
trigger-cleanup-direct-callers.txt. Run window_operands.py with the current
installed DSUN.EXE inventory and window193D..193E into
trigger-cached-setup-byte-window.txt. Require both independent stores and
classify the raw interior candidate against the wrapper instruction stream.

In repaired project INST-overlay-20261029g, program INST-MAPPED.EXE,
use ReportDecompileMatches.java read-only with analysis disabled at
6238:0422 (patterns000b and the equality-to-two test) and6238:0174
(patterns0422 and000b). Require the containing-function markers and
matched branches; retain trigger-cleanup-state-decompile.log. The mapped
addresses are navigation only; shipped locations above identify the claim.
No original game or emulated call supplied new evidence here.
