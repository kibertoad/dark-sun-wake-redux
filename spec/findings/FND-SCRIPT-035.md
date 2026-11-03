---
id: FND-SCRIPT-035
title: The cursor target finder returns a freshly read placement word rather than a normalized metadata index
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2DBD..31E0:31A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2620..28C9:269F
tool: resident_listing.py at 34fbedad with MZ relocations; locked evidence Python; FND-PARTY-252/253 for the previously recorded caller/classifier gates
environment: null
---

## Observation

Q-SCRIPT-022 requires admitted getter inputs, rather than just the field
bindings in FND-SCRIPT-034 and FND-ACTOR-016. FND-PARTY-253 already reads
the mode-two cursor caller; FND-PARTY-120 covers the target finder's early
placement filters. This reading accounts for the complete 1002-byte local
finder body to identify every local source of its returned word.

The finder calls 31E0:000A with its first two arguments and those words
plus 320 and 200. Those additions prepare that call's coordinates; they
do not compare a returned object handle with a slot maximum. The finder
then starts a local placement cursor at DS:656F minus one, forms a far
pointer from DS:67B7 plus eight times that cursor, and walks backward until
its signed cursor falls below DS:6571. It decrements the pointer offset
by eight and its cursor by one for each skipped entry.

At 2E00 it tests the current entry's word +6 as signed and skips negative
values. At 2E22 it rereads that word to form the 37-byte placed-object
record address. Other local tests concern entry coordinates, object image
fields and bitmap/pointer results. They do not test an object metadata kind
or independently compare the selected word against the metadata-table extent.

Every local successful-return branch converges on 2ED1..2ED8. That block
reloads the saved entry pointer, reads its word +6 again into AX at 2ED4,
then jumps to the common epilogue at 31A3. It does not return the placement
cursor, the 37-byte record's object-number word, or a metadata record number.

The two local return-value sources are:

| Continuation | AX source |
|---|---|
| Successful selection, 2ED1..2ED8 | Fresh read of the current placement entry's word +6 |
| Failure continuation, 31A0 | Literal FFFF |

The failure continuation is reached both when the placement scan ends and
when the child called at 2F1C returns AL zero. The common epilogue restores
DI, SI and BP and far-returns at 31A6. There is no other local return.
The bounding-box bypass and each later selected bitmap-result path join the
same successful-read block; this is not a reading of those children's effects.

In the already recorded mode-two caller, returned AX is copied to SI.
FFFF selects its failure-image continuation. Other values feed coordinate
and classification work; the fallback at 2692 pushes SI to 1695:0834.
The kind-one/record-index checks in FND-PARTY-253 occur after this fallback,
not before it. The caller's retained SI identity across its intervening
calls still requires their preservation contracts.

## Interpretation

For a valid stable placement entry and ordinary returning children, the
early signed test limits the selected word to the nonnegative signed-word
range. It does not establish the narrower domain required by a particular
object/record table. Without preservation, the checked, record-address and
returned reads cannot be equated merely because they use the same offset.

This supplies a concrete return mechanism and rules out interpreting a
coordinate constant or placement cursor as an independent handle-range
guarantee. It does not settle Q-SCRIPT-022's admitted-domain requirement.
The remaining evidence must cover the runtime entry-word producers and
lifetime, viewport/placement bounds, caller register preservation and later
metadata/record bindings. Serialized ETAB object identifiers are not substituted
for this runtime word without a producer reading.

No admitted out-of-range input, erroneous rendered cursor, complete reading
or evidence-status promotion is claimed. No original game was run.

## Alternatives

- A positive placement cursor is not the returned handle; the result comes
  from the selected entry's word +6.
- Seeing 320 in the prologue does not establish a handle bound: the local
  instruction adds it to a coordinate argument passed to 31E0:000A.
- The first signed check does not prove the value of a later read across
  unread children or aliased state changes. No such mutation is observed here.
- A non-FFFF local result does not prove a live metadata kind, stride/base
  table or paired record is valid. Conversely, absence of an own check does
  not prove an invalid value can reach the native getter.
- FND-CONFIG-130 describes a different wrapper's conditional remapping.
  This cursor caller invokes 31E0:2DBD directly, so that wrapper's changed
  return value is not silently attributed to it.

## How to reproduce

Use installed DSUN.EXE size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b. With locked evidence Python run
resident_listing.py on the inventory's complete 31E0:2DBD..31E0:31A7 body.
Retain the loop-pointer locals, all entry-word reads and all transfers to
31A0/31A3. The successful transfer is 2ED8; the explicit child-failure
transfer is 2F26; the exhausted-scan branch is 319B; the far return is 31A6.

Check the bounding-box bypass, the object-number special cases and every
later bitmap-result branch against the same 2ED1 block. Do not infer child
purity or native input feasibility from the local branch listing. Reuse
FND-PARTY-252/253 for the caller/classifier and FND-CONFIG-130 for the
distinct wrapper. The caller's mask argument is visible at 28C9:2691/2692.

The complete retained finder listing is target-finder.txt under
GAME_DIR/analysis/trigger-getter-admission-20261103/. The preliminary
cursor-mask-caller.txt is bounded preparation only; it is not a complete
reading of every pointer mode. No original instruction listing is committed.
