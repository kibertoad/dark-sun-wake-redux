---
id: FND-PARTY-335
title: The strict play-ready restore has EMS selected and disjoint observed overlay and stack backing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/verify_checkpoints.py and overlay_backing.py at 1d638bc3; frame_links.py and extended snapshot at 55fbe24d; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic15000 cycles; SB16 base220 IRQ5 DMA1; no Ultrasound; muted host output
---

## Observation

Q-PARTY-137 retains actual backing and transfer admission after
FND-PARTY-332/333/334. The existing strict restore verifier now captures
fixed named overlay/queue fields at its paused play-ready state. This
observation establishes a baseline point, not an execution of reflection
or a cache copy.

On 2026-11-06 the committed verifier restored play-ready from
v181-q046-play-ready-20261104 into a new owned session. Executable and
mapping checks, restored CS:IP/time, named state, frame and checked files
passed. A deliberate different sound IRQ was refused before launch.
The stop was load at 2F8B:097D, emulated time 195166973150 ns.
The observed interval remained paused: the verifier supplies no gameplay
input, continuation or guest setup write. It records RULE-RNG-001 state
1718646187; it does not claim a new gameplay draw trace.

The restored frame SHA-256 was
`5bd2b5caca7597a8219c7c97b48543f209a1b0af1ae3d1b1825de968f903defe`.
The named state had leader one and slot kinds [0, 2, 0, 2], as required
by the source verifier. Runtime code load segment was 05BA, with the
recorded relocation delta -0A46. The fixed metadata reads yielded:

| Field | Observed runtime value |
|---|---|
| DS / SS | 4D9A / 4D9A |
| BP / SP | FFD6 / FF48 |
| Cache mode byte | 2: EMS selected by the recorded dispatcher |
| EMS page-frame segment / handle | E000 / 1 |
| Extended move target / handle / base | 0000:0000 / 0 / 0 |
| Near cache hook | 0D11 |
| Far notification | 05BA:02FC, corresponding to the default RETF leaf |
| Overlay buffer first / end / next | 5D9C / 662E / 62AD, paragraph units |
| Loaded overlay head | 4CBC |
| Queue index | -1 |
| Six dispatch-kind words | all zero |

The queue/kind interval at the relocated 435B:0000 is linear
435B0..43610, half-open. The observed overlay-buffer bounds give
5D9C0..662E0, disjoint from that interval. All offsets in the observed
SS segment lie in the unwrapped window 4D9A0..5D9A0, also disjoint
from the queue/kind span. These numerical comparisons do not establish
BP-chain validity, source/return frame contents or a permanent stack floor.
The EMS page-frame segment starts at E0000; the observed field is a
binding value, not an observation of a particular native mapped page or
copy.

The verifier saved and strictly reloaded its checked same-process
roundtrip without resuming gameplay. Named state/frame comparisons
passed again, and the complete backing snapshot was identical. The owned
session closed; no owned emulator remained afterward. Reports, private
files, image and state sidecars remain in the approved local store.

### Bounded saved-frame observation at the same point

A separate owned verification on 2026-11-06 used the extended observer
at 55fbe24d, with the same source and play-ready state. Its output is
GAME_DIR/captures/rust-dos-regenerated/v181-q137-frame-links-20261106-play-ready/restore-check.json.
The strict source, mapping, instruction/time, state/frame/file checks and
sound-IRQ mismatch refusal passed again. The fixed backing snapshot,
including its frame-link observation, was identical after the checked
same-process save/reload. No guest continuation, input or setup write
was performed; no new gameplay RNG sequence is claimed.

At the reported SS=4D9A and BP=FFD6, the saved link word was FFE8.
It is nonzero even, so FND-EXE-234's walk would inspect this node's
return-segment field. The observed offset/segment pair was 015D/05BA.
The next link, at SS:FFE8, was zero: the walk terminates there before
comparing that node's return segment. Exactly two link words were read;
only the first node's return pair was read. Neither a cycle nor the
64-node cap nor a crossing ended this observation.

This admits a terminating observed link graph at the checkpoint. It
removes an unknown about that point's graph, not about the future INT3F
handler's incoming BP. It does not identify these words as genuine live
far-return frames, admit a matched rewrite, or infer a first match from
the resident-looking segment value. The graph can change before the
reflection interval or while the interrupt-enabled loader runs. All
observed SS-relative fields remain inside FND-PARTY-335's disjoint SS
window at this point; actual later segment/backing preservation remains
Q-PARTY-137. The owned session closed and released its run lock.

## Interpretation

For observations starting from this exact checkpoint, EMS is a concrete
selected backend rather than an unknown choice between EMS and extended
memory. Buffer and SS-window separation from queue storage are measured
at this point, and the default notification is actually selected here.
The absence of queued entries and the zero kinds describe this baseline;
they do not prove immutable kind words or successful reflected publication.

Q-PARTY-137 now needs preservation or changed-state evidence from this
baseline through the selected publication/return interval, actual frame
and cache/header graph admission, native page mapping/copy bindings and
asynchronous effects. Other checkpoints, histories and backends remain
possible elsewhere. No complete reading, parity promotion, observed
reflection or whole queue closure follows.

## Alternatives

- The default profile necessarily selected extended memory: contradicted
  at this restored point by the observed mode byte and hook state.
- The overlay buffer or the observed entire SS window overlaps the queue:
  contradicted by these measured unwrapped intervals at this point.
- A successful paused roundtrip proves later reflection preservation:
  it contains no such gameplay interval or populated queue.
- Zero kind words prove that no writer can change them: unsupported by
  this point observation and explicitly not claimed.

## How to reproduce

Use the pinned evidence Python and committed observer at 1d638bc3:
`tools/research/rust-dos-runs/verify_checkpoints.py
v181-q046-play-ready-20261104 <new-run-prefix> play-ready`.
The command verifies the licensed executable and existing checked source,
uses the package-owned muted rust-dos profile and private drive, tests the
hardware mismatch refusal and performs strict paused roundtrips.

Retained result: GAME_DIR/captures/rust-dos-regenerated/
v181-q137-overlay-backing-20261106-play-ready/restore-check.json,
restored.png and the checked roundtrip sidecars. Require passed,
same_process_roundtrip, overlay_backing_roundtrip_equal and the hardware
mismatch refusal, and compare the named values/frame digest above. The
report contains the full restored stop; source metadata need not contain
registers. The helper reads only the fields justified by
FND-PARTY-328/333/334 and FND-EXE-562/566. Five synthetic controls prove
tooling admission only; the integration gate passed all 758 .NET tests.
No original pixels, runtime payloads or state files are committed.

For the bounded link observation, run the same committed verifier with
source `v181-q046-play-ready-20261104`, a new unused output prefix and
state `play-ready`, using 55fbe24d or its unchanged observer. Inspect
`overlay_backing.frame_links` and require equality across the checked
roundtrip. Interpret links using FND-EXE-234, including zero/one early
termination and odd-link suppression; termination is not frame authenticity.
The six synthetic controls in `check_frame_links.py` and five backing
controls passed before the full integration gate (758 .NET tests).
