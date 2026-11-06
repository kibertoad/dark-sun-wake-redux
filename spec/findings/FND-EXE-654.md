---
id: FND-EXE-654
title: The current-pin startup constructor return has the game-data segment and the checked environment root mirror
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/clock_source.py, check_clock_source.py and probe_drive.py at 9104ff27; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; default muted host output
---

## Observation

For Q-EXE-037, FND-PARTY-339 already classifies all nine direct-store
candidates retained by FND-EXE-582. They are not nine unread bodies.
The five role groups have explicit incoming/captured or native/driver-
preserved segment conditions, including the even/odd staging distinction.
FND-RNG-020's earlier constructor observation used session 0.10.0; it
was not silently treated as a current-pin run.

On 2026-11-07 the existing fixed clock_source.py ran the approved
DSUN.EXE -W0 -L through an owned RustDosSession under the pins above.
The existing nine synthetic caller, frame and bounded-metadata controls
passed first. The probe used the manifest-verified private C: and licensed
game.ins on D:, with its existing stops and reads unchanged. It issued no
input, guest setup write, environment mutation or screenshot request.
The generator guard remained armed; the final stop was the seed routine's
entry before it ran, with an empty draw record. No gameplay continuation
or new memory-profile capability was used.

The actual load entry was 05BA:0000, giving delta -0A46 from the retained
load-1000 model. These constructor stops were matched breakpoints:

| Stage | Actual CS:IP | DS / ES / SS | SP | AX |
| --- | --- | --- | --- | --- |
| Payload allocator returned | 05BA:29B7 | 4D9A / 4D9A / 4D9A | FFD8 | A6F4 |
| Pointer-array allocator returned | 05BA:29DA | 4D9A / 4D9A / 4D9A | FFD8 | A73C |
| Constructor about to return | 05BA:2A06 | 4D9A / 4D9A / 4D9A | FFDC | A73C |

All three declared near-frame/initialization-record checks passed. The
constructor metadata was payload length 67, source segment 0529 and
pointer-array length eight. The checked roots were A6F4 and A73C.
The metadata-only array check found three increasing members within the
payload and a terminating null pointer within the declared array.
No environment-string contents are recorded here.

At the last stop the probe also required the word at current DS:3920 and
the root mirror at current DS:0088 to equal A73C. That check passed.
The source store at 1000:2A03 and its segment conditions remain those of
FND-PARTY-339; this is the matched native post-store point, not an
instruction-by-instruction trace of every preceding writer.

Under this load mapping the observed game-data field is 4D9A:0088,
linear 4DA28. The intended manager field 55CE:0088 retained by
FND-EXE-582 maps to 4B88:0088, linear 4B908. These numerical field
locations are disjoint without wrap. This point therefore binds the
observed root mirror to game-data storage; it does not observe the
manager field's value or establish every write's effective segment.

The fixed probe completed normally at 05BA:0811 before seeding, after
its other existing startup checks. Those additional clock outputs remain
in the local report; they are not new gameplay or clock-rule claims here.
The session closed, no rust-dos process from it remained, and its machine
run lock was absent after completion. The owner-provided files were read
through the manifest-bounded preparer; guest filesystem activity used
the private drive.

## Interpretation

Reuse FND-PARTY-339 instead of repeating the nine-store classification.
This run supplies a current-pin constructor-return point and checked
root mirror for one of those candidate paths. It agrees with the retained
game-data interpretation while leaving the earlier get-vector stores,
argument-constructor capture/reload and odd-byte XMS intervals separate.

No continuous native DS invariant, absence of additional aliases or
successful header-preservation contract follows from three points.
Actual incoming/captured DS, intervening writer/interrupt effects,
callback targets and their header-relevant effects remain Q-EXE-037.
Q-EXE-034 retains general frame/return admission. FMT-EXE-003 remains
supported; no complete reading, queue closure or parity change is made.

## Alternatives

- Nine equal displacements identify nine manager-hook updates. Their
  already recorded roles and segment conditions do not support that inference.
- A historical native result automatically admits the current runtime pins.
  The current attempt was made separately and is identified here.
- Matching DS/ES/SS at return proves all intervening native calls preserved DS.
  These are stopped points, not a continuous register or write trace.
- A correct game-data root mirror proves the manager notification untouched.
  This probe neither reads that notification nor excludes every aliasing writer.
- A completed clock probe establishes all clock or gameplay behavior.
  This finding uses only the scoped constructor metadata and run boundary.

## How to reproduce

Use the licensed installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the existing manifest-bounded
private-drive preparation. Require session 1.0.0 and the exact pinned
rust-dos binary. Run check_clock_source.py and then clock_source.py
with a fresh plain run name from the repository root using the pinned
EVIDENCE_PYTHON. Do not add inputs, stops, guest setup writes or substitute
an untraced runner. Any generator-guard or unexpected stop ends the attempt.

This run is retained under GAME_DIR/captures/rust-dos-clock-source/
v181-q037-constructor-point-20261107/. Its clock-source.json SHA-256 is
0194be665815e153e4d34b0071bd8605cf64d71de61819dd793ac7615ec225a7.
The session provenance, request journal and console log remain local;
the sibling q037-constructor-point-20261107-console.log is under
GAME_DIR/captures/. Compare the constructor stop records, validated
caller flag, root mirror check and bounded array result with the table.
Do not copy original-derived captures or guest files into Git.
