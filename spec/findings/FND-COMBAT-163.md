---
id: FND-COMBAT-163
title: The RDFF reader distinguishes minus one and minus two and its common metadata check can change which cleanup path the presentation loader takes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:03B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0EFF..31E0:128C
tool: tools/research/exec-census/resident_listing.py at 60dda268; Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

Q-COMBAT-045 retains RDFF-enabled temporary-slot lifetime. FND-PARTY-049/051
already describe the resource requests and chunk handlers. This complete
own-body reading follows the status local through common cleanup and its
test in the presentation loader. It does not establish every handler's
transitive effects or a native save-time state.

### Reader status assignments and common continuation

2D40:000A initializes status word SS:BP-10 hex to zero and the allocated
resource pointer to null. Its own status stores are:

- A signed double-word resource number at most zero sets FFFE at +0035,
  then enters common cleanup.
- A number in the skipped 9000..13998 interval, a nonzero size-request
  result or a nonzero data-request result reaches +00B5, which sets FFFF
  and enters common cleanup. Resource allocation diagnostics have their
  own returning/nonreturning dependency; they do not directly set this
  status merely because a null pointer was diagnosed.
- A null chunk-handler result, an out-of-range chunk dispatch byte or the
  handled-count rejection reaches +02C4, which sets FFFE. If the current
  slot kind is nonzero, it requests 2D40:2410 before common cleanup.
- The common kind-two metadata rejection at +035C sets FFFF, independently
  of the prior status value, then requests 2D40:2410.

Common cleanup at +030F releases a nonnull resource buffer through
444C:0092 and stores its returned pointer. It then checks the current
three-byte-table kind. For kind two, it uses the current record number
to select a 49-byte record under DS:19C9. An unsigned word +04 at least
9999 takes the +035C rejection. Thus the encoded continuation can replace
an earlier FFFE with FFFF if this metadata condition remains true after
intervening calls. Whether a particular earlier cleanup leaves that state
is not established here.

The final overlay 187 +1A89 postprocessing call is reached only while
status is zero and the slot is unsigned below 9999. The body then writes
9999 to DS:157C, reloads the status into AX and returns. Under intact
locals and returning callees, nonzero status skips that final postprocessing;
it does not skip the earlier release and common kind-two check.

### Presentation loader tests the final value

31E0:0EFF first obtains OJFF and initializes the presentation record.
When flags mask 04 hex is set and the signed object number is outside
9000..13998, it requests RDFF through 2D40:000A. It saves returned AX.
Zero continues at +1040. For nonzero AX it compares the saved word with
FFFE; only equality jumps to +0F47's release/fill failure route.
Every other nonzero result, including FFFF, continues at +1040.

The +1040 test independently checks the indexed presentation entry word
for FFFF. A passing entry can proceed into temporary-slot allocation and
the remaining loader work despite RDFF having returned FFFF. This does
not guarantee that those later prerequisites hold or that loading succeeds.
If the final RDFF result is FFFE, the failure route requests release for
the slot, fills its presentation record with FF and returns FFFF.

## Interpretation

This removes the RDFF reader's own status-propagation distinction and the
parent's final-value decision from Q-COMBAT-045's unread lifetime work.
Neither a failed resource request nor an early assignment of FFFE is
enough to infer the parent's immediate cleanup route without the final
status, current metadata and intervening effects.

It does not close RDFF-enabled save admission. The size/data reader,
allocator/release, chunk handlers, reclamation, metadata validity and
postprocessing still have their own state/callback requirements. A
conditional replacement of FFFE with FFFF is not evidence that it occurs
for canonical resources or that a populated temporary slot reaches Save.
No native run, visible defect, new gameplay behavior or parity promotion
is claimed.

## Alternatives

Treating all negative reader results alike loses the parent's explicit
FFFE comparison. Treating FFFE as irrevocable loses the common metadata
store. Conversely, that later store alone does not prove a reachable
status replacement: earlier 2410 calls may alter the tested kind/state.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. Use the project-local
evidence Python and resident_listing.py on complete half-open ranges
2D40:000A..2D40:03B9 and 31E0:0EFF..31E0:128C. The approved reader
verifies size 634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
The RDFF listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-rdff-reader.txt;
the complete parent remains in temporary-normal-callset.txt there.
Trace every store to BP-10, common cleanup, the +0354 unsigned check,
the +0399 final gate and parent's +1033/+1037 tests. Reuse existing
FND-PARTY-049/051 and FND-COMBAT-149 for handler/parent context.
Original-derived output remains only in GAME_DIR.
