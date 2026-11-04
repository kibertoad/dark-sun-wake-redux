---
id: FND-COMBAT-143
title: One native load selects the working record for SAVE twenty-four and twenty-six with lengths 1920 and two
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and request_provider.py at a50bfab8; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed probe repeated the verified native-save F2/Enter,
successful copy and selector-zero reader-entry route of FND-COMBAT-140/141/142.
It then continued the original reader, supplying no setup writes or further
input. FND-COMBAT-035 describes that reader's request order;
FND-CONFIG-151 and FND-COMBAT-120 identify the core arguments and selected
provider locals.

The probe admitted each target at resident 38FF:07B1 through the 04AB
wrapper's return +04D0, with SAVE type, resource number 24 or 26, start zero,
count FFFFFFFF and a nonnull far address of the in/out destination slot.
It stopped at +094A, after selecting the destination and before preparing
the archive or seeking its payload. SS/BP matched the target core entry.

All lengths, offsets and handles in this table are decimal. Destinations
use the declared modeled mapping, not the current run's relocated segments.

| Selected field | SAVE/24 | SAVE/26 |
| --- | ---: | ---: |
| Recorded resource length | 1920 | 2 |
| Recorded resource offset | 39990 | 42162 |
| Selected transfer count | 1920 | 2 |
| Provider record's +28 handle | 14 | 14 |
| Provider record's +4 I/O handle | 11 | 11 |
| Destination | 4E71:024D | 57E0:2100 |

The provider pointer was equal for both requests and equalled each stop's
active and head pointers. Each selection snapshot retained reuse one,
working handle 14, region handle zero, game mode four, region 50, traversal
mode two and issuance counter 15. These are endpoint observations, not a
trace of every intervening write or archive visited during lookup.

SAVE/24 continued to the same core frame's +09D4 before LEAVE/RETF.
AX was zero, global archive error was zero, and the reported completed count
was 1920. In the static core contract this is the successful complete-count
path after matching seek and read results. No destination bytes were captured
or compared, so this does not establish payload identity or retained effect
semantics. The probe continued to SAVE/26's selection stop and ended there;
its preparation, seek, transfer and return were not observed.

The accepted run used 90 loop continuations and recorded no RNG draw. Its
owned muted session closed. An earlier diagnostic run reached admitted reader
entry but rejected redundant removal of an already-consumed callback
breakpoint; it observed no target resource request. That diagnostic is
retained, and the corrected tooling did not bypass a game-state guard.

## Interpretation

This removes the unobserved request-time provider/length tuple for this
specific native-save route. Both requests selected the currently observed
working-record handle and the nominal lengths previously mapped by
FND-COMBAT-116; those lengths are now observations of runtime selection.
The two I/O and archive handles are separate fields, not interchangeable IDs.

Equal provider, active and head pointers at selection do not establish the
entire graph, absence of fallback traversal, record lifetime, stable bindings
through child calls, or which private file currently backs the I/O handle.
The experiment does not read the directory entry itself, identify payload
bytes, measure destination capacity, observe SAVE/26's result or prove
completed loading. Missing-resource and alternative fallback paths remain
unobserved. Q-COMBAT-064 retains those obligations, with destination/lifetime
and child effects under Q-COMBAT-065/066. No parity status or complete-reading
promotion follows from this single route.

## Alternatives

- The native reader necessarily uses an unobserved length for both targets:
  the selected locals here explicitly held 1920 and two with matching counts.
- SAVE/24 and SAVE/26 must select different provider records: their selected
  pointers matched in this run. Other states and fallback paths remain open.
- Observing both selected lengths proves both payloads were restored:
  SAVE/26 stopped before preparation; SAVE/24's success result was not a
  destination-byte or later-state comparison.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Use the
project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q064-providers-scoped-20261104 saved-providers`.
Choose a new output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q064-providers-scoped-20261104/.
The earlier diagnostic is v181-q064-providers-20261104 under that parent.
The report's resource_requests records arguments, stops, selected locals,
archive snapshots and SAVE/24's result. Its continuations counts every loop
iteration; the older polls field counts only generic observation paths.
Synthetic frame/range controls verify tooling rejection behavior, not native
resource provenance or all runtime paths.
