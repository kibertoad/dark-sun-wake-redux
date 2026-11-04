---
id: FND-COMBAT-150
title: One native reader reconstructs 125 admitted object slots with sixteen-byte OJFF selections and no temporary-slot allocation stop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and object_restore.py at 3444a367; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed native-save probe repeated the admitted route of
FND-COMBAT-140/141/142/143/144/145/146/148 through native copying, the two
target SAVE transfers, GMAP reconstruction and matched reader return.
It added an object-loop census and resident loader/OJFF/child observations
using FND-COMBAT-077/149's static boundaries. No setup writes or further
input were introduced.

### Pre-loop census and admitted loader calls

After the matched GMAP transfer, before the object reconstruction loop,
the probe sampled the entry-index word of each of the 320 resident
37-byte presentation records. Exactly 125 differed from FFFF. Slot 319's
entry-index word was FFFF. These are observations of memory at that
boundary, not an independent parse of the saved file or a proof of the
temporary slot's state at every earlier point.

The observed ordered 31E0:0EFF loader entries matched that 125-slot list
exactly. Each entry had the declared overlay-187 +0C3F return, loaded
+0C3A far-call instruction, slot below 320, resident DS and flags word
zero. The probe retained the supplied object number, placed-entry index,
slot and low category byte as local report metadata, not a committed
resource or executable dump.

All 125 invocations reached resident +1288 with their original SS/BP
identity, before the epilogue popped saved registers. Each had its incoming
slot in returned AX. These are matched resident epilogue observations;
the probe did not claim a separate per-object far-return stop.

### Actual OJFF request and local span

Each invocation reached the declared OJFF request through the complete
resource wrapper. The probe matched the core +04D0 return, enclosing loader
+0F40 return, requested sign-extended object number, zero start and
complete-count sentinel. The in/out slot was the loader's SS:BP-0C,
containing its declared SS:BP-24 local destination.

At each core selection stop, the complete selected count was 16 bytes
and the destination still matched that local buffer. All selected provider
records carried archive-handle field three. This identifies the observed
provider field, not its file name, full graph or continuous lifetime.
The selected length matches FMT-ACTOR-001's nominal OJFF size, but here
it was read from the actual selected core locals rather than assumed.

All 125 matched core epilogues reported AX zero and completed count 16.
Each following loader +0F40 stop also held AX zero. No selected OJFF
payload bytes were compared with a source file by this stage; success/count
and the bounded local destination are distinct observations from content
identity or semantic validity.

### Named child-entry census

The fixed resident entry breakpoints reported the following calls while
a matched loader frame was active:

| Named child | Entry stops |
| --- | ---: |
| Record initializer 31E0:0E1B | 125 |
| Image selector 31E0:2670 | 125 |
| Script-chain helper 2D40:0B54 | 2 |
| Replacement allocator 2D40:0461 | 0 |
| Eviction helper 31E0:0D09 | 0 |
| Release helper 2D40:03C1 | 0 |
| Animation setter 31E0:271E | 0 |

The script-chain helper arguments named slots one and three. A helper
entry is not proof that its conditional chain writes occurred. Each
image-selector call's supplied low flags byte was zero, consistent with
FND-COMBAT-149's no-child branch for that helper. No monitored child-entry
stop was reported outside a loader frame during the observation period.
These are scoped breakpoint counts, not a whole-program indirect-call
census or an exclusion of unmonitored children and interrupt effects.

At the full reader's matched caller return, slot 319's entry-index word
was again FFFF. The map still matched FND-COMBAT-148's bit-plane prediction,
and both effect spans and their source-slot bindings matched transfer-time
references. Reader AL was one. No reader-return relocation or RNG draw
was recorded.

The accepted run used 1139 loop continuations within its explicit
4096-observation bound. It ended before the reader caller consumed AL,
and the owned muted session closed. No post-load checkpoint was made.

## Interpretation

This supplies one actual ordinary re-placement route with its ordered
slot census, caller/flags admission, selected OJFF stack spans and matched
core/loader epilogues. It does not rely on the skipped RDFF branch alone
to describe what this invocation reached. The temporary-slot allocation
and eviction route of FND-COMBAT-149 was not observed in this case.

The result does not settle whether another original-made save can admit a
populated slot 319. Its pre-loop marker is not a complete earlier-state or
saved-file history. Q-COMBAT-045 retains that admission and the consequences
under actual free-list/victim state; repeating this same empty baseline
would not answer it.

OJFF file/content identity, live provider and alias preservation, individual
initializer/chain writes, unmonitored children, alternative object states,
allocation/failure routes and later caller work remain. Endpoint effect
retention does not exclude intermediate writes or prove child-by-child
preservation. Q-COMBAT-065/066 retains those obligations. No queue item,
complete reading, rule or parity validation is promoted by this one route.

## Alternatives

- The reader's active slots might differ from the sampled pre-loop list:
  the observed ordered calls matched it exactly in this run.
- Every re-placement loads RDFF or takes an allocation path: this admitted
  flags-zero route reported neither the named replacement/eviction entries
  nor a populated temporary-slot call. Other states remain open.
- The nominal sixteen-byte format alone admits the local transfer size:
  the probe separately checked every selected core count and destination.
- Two script-chain entries establish two insertions: the helper's own gates
  were not observed at internal stops, so only the calls are established.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-objects-20261104 saved-objects`.
Use a fresh output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-objects-20261104/.
The object_trace record retains the pre-loop census, ordered admitted
entries, OJFF selections/results, named child arguments/counts and matched
epilogues. reader_return and map_request retain final comparisons. Compact
stop records omit unrelated repeated debugger state. Synthetic controls
exercise caller/frame/length refusals, epilogues and reporting; they establish
tooling behavior, not additional native saves or effect states.
