---
id: FND-COMBAT-144
title: One native load completes both target resource transfers with destination bytes equal to the copied working file spans
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and request_provider.py at 9f774a43; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated FND-COMBAT-143's native-source
F2/Enter, successful copy, region return and selector-zero reader route.
It retained that finding's SAVE/24 and SAVE/26 wrapper-frame, argument,
provider-local and core-frame checks, then admitted both matched core
returns before LEAVE/RETF at resident 38FF:09D4. No setup writes or new
input were introduced.

At copy return, the private darkrun.GFF and native SAVE02.SAV each had
length 49739 and equal whole-file SHA-256. Both target selection snapshots
matched FND-COMBAT-143's lengths, offsets, destinations and provider fields:
1920 bytes at file offset 39990 for SAVE/24 and two bytes at offset 42162
for SAVE/26. The selected record had archive-handle field 14 and I/O-handle
field 11 and equalled active/head at both selection stops. The named archive
state tuples matched the earlier observation.

For each target, the return stop had the original core SS/BP, AX zero,
global archive error zero and reported completed count equal to that
target's selected length. While stopped, the probe verified the fixed
private working file's whole-file identity against its copy-return snapshot,
read only the selected file span, and verified whole-file identity again.
It then read the selected destination in bounded chunks without segment or
real-mode wrapping and compared bytes directly. The local report retains
digests and comparison results, not a committed payload dump.

| Target | Selected and reported count | Destination in modeled mapping | Comparison with private file span |
| --- | ---: | --- | --- |
| SAVE/24 | 1920 | 4E71:024D | All bytes equal |
| SAVE/26 | 2 | 57E0:2100 | All bytes equal |

The driver accepted 91 loop continuations and no RNG draw. It stopped at
SAVE/26's core return before subsequent wrapper/reader work and closed its
owned muted session. It did not capture a post-load checkpoint.

## Interpretation

For this one admitted native-save route, both target destination spans at
their respective successful core returns contain exactly the selected spans
of the working file whose identity still matches the copied native save.
This supplies a bounded payload-content comparison and SAVE/26 completion,
beyond FND-COMBAT-143's provider selection and SAVE/24 result alone.

The comparison does not independently resolve DOS I/O handle 11 to a host
file, inspect directory entries, prove graph membership or absence of
fallback, or establish every intervening lifetime and alias. Equal bytes
are a content observation, not a unique provenance proof. The first target's
bytes were not sampled again at the second return. Other writes, later
effect-state retention, destination capacity for other lengths, malformed or
missing resources, alternative saves and completed loading remain open.
Q-COMBAT-064 retains source/directory/handle and alternative-path obligations;
Q-COMBAT-065/066 retains destination lifetime and child effects. No rule,
parity validation or complete-reading status is promoted by these comparisons.

## Alternatives

- Successful core results alone suffice to prove destination content:
  the experiment additionally compared the bytes, within the fixed target
  spans, and records equality separately from AX and count.
- SAVE/26 completion remains unobserved on this native route: its matched
  core return, zero error and two-byte reported count were reached here.
- Matching destination bytes prove unique file-handle ownership or completed
  restoration: neither handle resolution nor subsequent reader work was
  observed. Those claims remain outside this experiment.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q064-payloads-20261104 saved-payloads`.
Use a new output name on repetition. Reports, debugger journals and private
files remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q064-payloads-20261104/.
The report's resource_requests retains each selected tuple, matched result,
count and payload comparison. Fixed-file checks reject links, oversize files,
changed identities and invalid spans; memory comparisons admit only this
route's observed target sizes. Synthetic mismatch, short-read and boundary
controls establish tooling behavior, not broader game parity.
