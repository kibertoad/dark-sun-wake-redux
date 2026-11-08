---
id: FND-SOUND-096
title: Repeated installed startup initializes the disc table and reaches the music gate with enable zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/disc_startup.py at d5a7ccd7; dinorefurb-dosbox-session 1.0.0; project-local evidence Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-SOUND-092/093/094/095 establish the local initializer, known callback,
response acceptance and table-write paths but leave actual startup input
and state admission open. On 2026-11-08 two independent fresh sessions ran
the unchanged manifest-verified installed files with DSUN.EXE arguments
-W0 -L. Each mounted game.ins read-only, recorded 54 events at named stops
and closed normally. No input, guest write, saved-state restore or frame
capture was supplied. Source DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses and encoded words are hex;
track counts, sizes and event counts are decimal.

### Admitted configuration and callback path

The executable entered at 05BA:0000. At every named stop the checked DS
and SS were 4D9A, the relocated source data segment. The configuration
pointer was 68D0:0004; all 59 loaded configuration bytes equaled the
installed SOUND.CFG bytes and were identical at every captured stop in
both runs. The configuration word was11 decimal, matching FND-SOUND-092's
source profile. No different configuration was constructed for the probe.

The sequence began at initializer entry 3CEE:0133, then reached its
callback gate and indirect-call site. The observed callback pointer
selected 1C1A:052D, the relocation of the known 2660:052D target. Its
entry frame returned to 3CEE:019D, verified by an exact four-byte read.
It reached the presence return with stored presence word one, then the
metadata return with both disc-enable and metadata-ready bytes one.
Thus this observed route neither skipped the callback nor took its
presence/metadata failure exit.

### Track-table acquisition and final write

At metadata return the first and last bytes were 1 and 41. They retained
those values at the later captured points. The response lead-out dword
was 00472122. The allocation-call stop exposed a four-byte size argument
301, agreeing with FND-SOUND-095's seven-times-last-plus-two geometry.
The allocation return had DX:AX=68E3:0004; at that exact stop the pointer
store had not executed and the current table pointer was still null.
Subsequent iteration returns observed 68E3:0004 in that pointer.

There were 41 per-track return stops, with SI consecutively 1 through 41.
Before the final write SI was 42. The before/after pair identified the
same table, last-track and target 68E3:012C. The four-byte target value
changed from zero to 00472122, matching the held lead-out field. This
measures the reached dword copy to the named target, not the heap block's
allocated extent, every preceding slot's response bytes or audible output.
The probe did not infer table capacity from readable real-mode memory.

The verified destination's real-mode linear span is 68F5C..68F60,
half-open. The observed data segment places the captured DS-relative library
pointer, music-gate and handle fields within 50DB1..50E2A. These intervals are
disjoint. The actual final dword store therefore is not an aliasing writer
of those fields in this observation. This excludes that one reached store,
not other table writes, allocation/request children or external writes.

The callback returned AX 2101, a nonzero word with AL one. The caller
reached its ordinary join and then the music gate at 3CEE:0293. That is
the last observed instruction boundary; it was not resumed afterward.

### Music and RNG state at the captured points

Across all 54 snapshots the music-enable word was zero, library slot was
FFFF, source/alternate/getter handles were FFFF, and both retained-buffer
pointers were null. These are point observations spanning the callback
and the pre-gate region, not a trace of every write between them.

The initial RNG state was one and every named snapshot still read one.
Generator and seed-setter breakpoints were installed before continuation;
neither guard was reached, so no draw or reseeding was executed by the
observed route. Both reports have empty draws, inputs and setup-write lists.
This is a startup observation before the guarded seed boundary, not a
multi-seed gameplay experiment or rebuild replay validation.

The complete 54-event arrays, including named fields, register snapshots
and configuration contents, are identical between the two independent
sessions. Their full report hashes differ because report metadata also
contains the session's own entry/stop details. No unreported event journal
was reconstructed; session.json records event_log as null.

## Interpretation

Q-CONFIG-014 now has actual default-profile configuration, known callback,
successful metadata branch, allocation request/return, iteration and final
write admission at these fixed startup boundaries. Its local disc effects
are not an unread hypothetical callback for this route, and its paired
final store is disjoint from the captured library fields. The observed
zero music-enable value at the last gate supplies the selecting state for
FND-SOUND-092's local bypass branch; later reloaded gates were not observed.

Retain heap extent and alias admission, per-track response freshness and
unobserved write histories, other configurations/reinitialization and
unknown callback targets. In particular, equal handles and gate values at
these stops do not establish continuous preservation, the later getter's
state, or an unchanged configuration on every execution path. The remaining
readback bundle extends from this music gate through subsequent startup
and the actual Preferences entry/getter/byte store. RULE-CONFIG-003 remains
supported, with no complete-reading or parity promotion and no queue closure.

## Alternatives

- The known callback is skipped on this fixed installed route: its paired
  call/entry, child returns and ordinary return were observed twice.
- Metadata failure or null allocation explains this route: the captured
  success bytes, nonnull result and reached table write contradict that reading.
- Music-driver work must follow successful disc initialization: the observed
  music-enable word is zero at the separate gate; disc success sets no such
  nonzero word in these point observations.
- Identical endpoints prove continuous state preservation or valid heap size:
  the probe watches neither every writer nor allocation metadata.

## How to reproduce

Use the pinned project-local evidence Python, current rust-dos and unchanged
installed manifest. Run each command with a new name in its own owned session:

```text
python -I tools/research/rust-dos-runs/disc_startup.py <fresh-run-name>
```

The accepted directories under GAME_DIR/captures/rust-dos-disc-startup/ are
v181-q014-disc-startup-a-20261108 and v181-q014-disc-startup-b-20261108.
Their disc-startup.json SHA-256 values respectively are
cc0dddd9204cdc7bb1ad0cc14961fa0f169993dc2ebbbac9f67048245eda8c43 and
db6f82c5548b953426470937a7590a2c22e4767ad499a9bb560599d352fcf0c6.
Check complete=true, 54 events, empty setup/draw lists and the named
callback/metadata/allocation/table/music-gate sequence. Compare the events
arrays as JSON and decode each configuration field for byte equality with
the verified installed SOUND.CFG; compare per-track SI and the paired
final target separately. Retain all original-derived reports locally.

The eleven synthetic controls in check_disc_startup.py cover admission,
short/wrapping reads, paired storage, RNG guards and caps. Those controls
do not supply original evidence; the two native attempts above do.
