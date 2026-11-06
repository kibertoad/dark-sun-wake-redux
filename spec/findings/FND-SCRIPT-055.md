---
id: FND-SCRIPT-055
title: Early and inner script allocation failures reach different cached result bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057580..0x000579A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007330C..0x00073331
tool: overlay_listing.py at 90de12fc; Python 3.12.10, Capstone distribution 5.0.9; Ghidra 12.1.3 bounded decompiler matches; FND-CONFIG-149/156/160 and FND-SCRIPT-053/054 composition
environment: null
---

## Observation

This completes the local allocation-check stage classification for
Q-SCRIPT-019 by reading the initializer, working setup, script entry gate,
cleanup and caller's result consumption together. It does not infer that
the original experiences any particular failed allocation. Source DSUN.EXE
is 634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.

For FND-CONFIG-149's supplied arguments 200 and10000, the nonpositive-
status initialization path has four allocation checks. Each named group
publishes its returned pointer fields before its check; a group is not a
transaction. Under FND-SCRIPT-053's ordinary flag-zero/one behavior, a
failed allocation sets flag one and later requests in that group return
zero without calling the external allocator. They still publish those
returned null pointers. Actual responses and independent storage remain
conditions, not assumptions derived from the request sizes.

| Check in overlay169 | Requests since the preceding check | Destination pointers |
| --- | --- | --- |
| +004E | Trigger pool, 2600 bytes | DS:40C0 |
| +0104 | 101,400,80,420 bytes | 4C13:034B,0347,0343,033F |
| +02F7 | 300,9,64,40,420,32 bytes | 4C13:0251,033B,0337,0333,032F,0249 |
| +038A | Script buffer,10001 bytes | 4C13:0255 |

There is no separate check immediately after the 300-byte request. All
six working-group fields are published before +02F7. A passing +02F7
admits the four local-variable clears, followed by the script-buffer
request and its separate check. The script cache initialization and
state-two publication follow a passing +038A, not merely the first
working-group check (FND-SCRIPT-026).

### Early outer failures return directly

On the first check's zero AL, +0055 sets AX=FFFF and jumps to the outer
return. Failure at +0104 jumps to that same exit. Neither path calls the
inner setup, MAS/99 entry or outer cleanup. Script state4C0E:000B and
result word4C0E:0009 were set to FFFF before these requests.

With intact bindings and returning allocation calls, failure at the first
stage leaves the published pool pointer null. Failure in the global
group can leave a valid pool and a mixture of earlier nonnull and later
null global pointers. The direct exit contains no local release of them.
Fields in stages not reached retain their prior values rather than being
initialized by a stage that never runs.

The wrapper at overlay188 +046C stores returned AL into DS:193D.
Thus these direct FFFF returns become cached FF, selecting the
following nonzero branch described by FND-CONFIG-149. That branch does
not establish valid buffers or ready script state.

### Inner failures reach cleanup and return zero overall

The inner +023B body requires script state one. On that path it explicitly
sets result word4C0E:0009 to FFFF at +0254 and state to one at +0260.
A zero AL from either +02F7 or +038A returns AX=FFFF through the
inner failure exit at +02FE. Neither failure reaches the later ready-state
or cache initialization stores.

The outer caller publishes that returned FFFF to script state and calls
172C:000C. FND-CONFIG-160's complete entry guard reads this state
before any child call or reset and returns immediately because it is not
two. Consequently, with the same bindings retained, it does not change
the inner body's FFFF result word to the ordinary script-entry value one.
The outer test at +014F therefore takes its result-not-one branch,
stores FFFF to state again and invokes outer cleanup at +0165.

FND-SCRIPT-054 supplies the cleanup effect: the working releaser sees
state FFFF and skips its seven fields; the outer cleanup performs the
global/pool zero publications and finally stores script state zero. The
initializer reloads that state into AX at +016D and returns zero overall.
The wrapper consequently caches byte zero and selects its caller's
zero-result branch. The inner failure's FFFF is not the outer return.

On a first working-group failure, six working fields have been published,
but the script-buffer pointer has not been assigned by this attempt.
On a script-buffer failure, the six preceding working fields and local
clears were reached, and the script-buffer pointer is null. In both cases
the working release is skipped on this local failure path. This records
which fields are published or passed to release, not an independent
guarantee of heap ownership, leaks or external release success.

| Failed check | Outer return AX | Cached DS:193D | Outer cleanup reached |
| --- | --- | --- | --- |
| +004E, trigger pool | FFFF | FF | no |
| +0104, globals | FFFF | FF | no |
| +02F7, six working requests | 0000 | 00 | yes; working release skipped |
| +038A, script buffer | 0000 | 00 | yes; working release skipped |

The table assumes the named checks fail through the recorded flag-one
path and the cited returning/preservation conditions. Nonboolean failure
flag values remain FND-SCRIPT-053's separate admission case. A preexisting
signed-positive script state bypasses allocation entirely, as already
FND-CONFIG-156 records; it is not one of these four failures. MAS/99
effects after successful initialization are also separate from these
pre-script failure paths.

Bounded decompiler matching confirms both checks in each body, the inner
result-word initialization and the outer failure cleanup/state reload.
Widths, branch ordering and the no-child script-entry bypass use the
instruction readings and FND-CONFIG-160, not an inferred decompiler ABI.

## Interpretation

The initializer's allocation failures do not have one uniform result or
cleanup history. Q-SCRIPT-019 can reuse this finite stage table instead of
leaving those local branches unidentified. Actual allocation responses,
incoming state/flag, independent pointer storage and histories reaching
later users remain open. No native failure observation, queue deletion
or whole-entry complete reading is established.

## Alternatives

Propagating the inner FFFF straight to cached FF omits the state guard,
retained result word, cleanup and outer state reload. Treating early
outer failure as zero omits its direct FFFF exit. Treating an unentered
stage's pointers as null invents initialization not performed by this path.

## How to reproduce

Use the pinned evidence Python and overlay_listing.py descriptor169
57580..576F4 and577BB..579A2; retain
trigger-outer-allocation-failure-stages.txt and
trigger-inner-allocation-failure-stages.txt under
GAME_DIR/analysis/handler-census-20261106/. Trace the four checks and
both return layers; compare pointer stores before each check. Reuse
FND-CONFIG-160 for the complete no-child state guard, FND-SCRIPT-054's
cleanup streams, and FND-CONFIG-149's setup wrapper and caller test.

For navigation, use repaired INST-overlay-20261029g, program
INST-MAPPED.EXE, read-only with analysis disabled. Run the bounded
ReportDecompileMatches.java at6238:0000 with patterns0174,0009,000b
and at6238:023B with patterns0600,0009. Require both function markers
and actual matched checks; retain trigger-allocation-stage-decompile.log.
Mapped addresses are not shipped citations. No original game or
emulated call supplied new evidence for this assessment.
