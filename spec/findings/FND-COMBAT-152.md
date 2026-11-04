---
id: FND-COMBAT-152
title: One native reverse conversion retains its GPLI scan inputs and matches predicted chained-pair and trigger results before release and at reader return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and script_restore.py at 5ba486bb; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed native-save probe repeated the admitted route
through copying, target SAVE transfers, map reconstruction and matched
selector-zero reader return. It added the reverse script-converter
observations below, using FND-COMBAT-151's local contract. No setup writes
or additional input were introduced. This stage did not repeat
FND-COMBAT-150's expensive object-entry census.

### Caller, size/data provider and retained scan inputs

At the resident GPLI/1 size-wrapper entry, the probe matched the converter's
+184C return and loaded +1847 call, output slot SS:BP-08, initially null
table pointer, low argument byte one and reader +0CC0 caller. DS and SS
both denoted the admitted relocated resident data segment. It repeated
that segment relationship check after data transfer and before release.

At the nested size lookup's common successful-output point, the selected
type was GPLI with its indexed flag set. Its provider record carried
archive handle one and I/O handle six; selected length was 7896 decimal.
The matched size-wrapper epilogue returned AX zero and wrote that length.

The actual complete data request used the converter's +1879 caller and
SS:BP-04 in/out slot, still null at request entry. At selection, its
provider pointer and both handle fields matched the size-query snapshot.
Recorded length and selected count were 7896, with resource offset
2068351 decimal. The matching core epilogue had AX zero, completed count
7896 and a nonnull destination matching the current local table pointer.
The local queried length was still 7896 immediately after that transfer.

At the later matched release-request boundary, the retained local length
was again 7896 and the computed word scan bound was 1316 decimal. These
are explicit local-field observations, not a bound inferred only from a
matching output. Matching source snapshots do not independently resolve
the I/O handle to a host file or exclude intervening reuse.

### Captured inputs and prediction

After the data transfer and before remapping, the probe cached all 7896
table bytes in its own process memory. It followed the current script
chain within the admitted 48-record resident pool, rejecting repeated or
out-of-range indices. Sixteen nodes were reached, each supplying two pairs
of entry/script words and a signed-byte next link. It also captured the
current trigger pointer and all 2600 bytes of its 200 thirteen-byte records.
Reads were bounded to 261 bytes each and rejected segment/real-mode wrapping.

The sampled table, trigger block, converter-local span and each observed
chain pair/link field had no physical range overlap. This is an input-span
observation, not a complete allocator, metadata or effect-state alias census.

Using the actual captured table order, the probe computed first-match
reverse conversion, preserving unmatched values, zero-script trigger gates,
links and other trigger bytes. The prediction counts were:

| Input group | Matched pairs | Pairs differing from captured input | Unmatched eligible pairs |
| --- | ---: | ---: | ---: |
| Two pairs in each of 16 chain nodes | 32 | 12 | 0 |
| Trigger records with nonzero script word | 103 | 103 | 0 |

The other 97 captured trigger records had zero script words and were
skipped by the prediction. These are counts derived from captured inputs,
not a trace of every internal compare or branch in the original.

### Before release and at the returning reader boundary

At 444C:0092 entry, the probe matched the original converter SS/BP,
+1A27 caller and current selected table-pointer argument. The release
helper had not executed yet. A fresh bounded snapshot matched the predicted
chain fields and links exactly; all 2600 trigger bytes matched, with zero
different bytes. The head and trigger-pointer bindings were unchanged.
Thus the result included actual changed pairs, not only unchanged data.

At the full reader's matched +0F2F caller return, the same comparisons
again matched, including all trigger bytes and the sampled chain fields.
The probe did not dereference the released table at that later boundary:
its prediction used the cached transfer-time bytes. It did not independently
observe the release helper's result or prove that allocation was released.

The map still matched its saved-plane prediction and both effect spans
and source-slot bindings matched transfer-time references. Reader AL was
one. The final accepted run used 145 loop continuations, recorded no RNG
draw or reader-return relocation and stopped before the caller consumed AL.
The owned muted session closed; no post-load checkpoint was saved.

An earlier successful observation also matched the conversion prediction,
but lacked the final stage's explicit retained-length/scan-bound resampling.
It is retained separately rather than relabelled as containing those checks.

## Interpretation

This removes the unobserved normal reverse-conversion input/result case
for this fixed native route: size/data provider snapshots and lengths,
the DS/SS relationship, retained arithmetic inputs, bounded chain/trigger
bindings and prediction-matched endpoints now have direct observations.
The normal mapping summaries in FND-SCRIPT-017 and FND-SAVE-023 are no
longer the only evidence for this one runtime use.

The comparisons do not establish every branch, incoming route, provider
file identity, continuous record/allocation lifetime or absence of
intermediate writes. Other nineteen-byte chain fields were not compared;
the full trigger block was. Input-span separation does not establish all
possible aliases, and a release request does not prove cleanup success.
Other table lengths, failed/skipped transfers, diagnostics, allocation and
unmatched-entry cases remain outside this native observation.

Q-COMBAT-066 retains those child/state alternatives and later caller work;
Q-COMBAT-065 retains general storage/alias obligations, with incoming and
trigger-lifetime questions under Q-SCRIPT-020/019. No queue item, complete
reading, format confidence or parity validation is promoted.

## Alternatives

- The size/data requests necessarily disagree or lack a usable scan input:
  this case matched provider snapshots and retained 7896/1316 at the named
  boundaries. Other states and intervening identity changes remain open.
- Prediction equality means no script state changed: twelve chain pairs
  and 103 trigger pairs differed from their captured inputs.
- Sampling only the final reader proves where conversion occurred:
  the probe also sampled before the matched release helper executed, but
  it still did not trace every internal write or later child instruction.
- Endpoint equality proves heap release or all-record lifetime: no such
  allocator or continuous-lifetime evidence was collected.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-script-bound-20261104 saved-script-conversion`.
Use a fresh output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-script-bound-20261104/.
The earlier observation is v181-q066-script-conversion-20261104 under that
parent. The script_trace record retains named frames/providers, size/count
checks, bounded input metadata, prediction statistics and both comparisons.
Raw table and trigger bytes are not committed. Synthetic controls test
first-match, unmatched/gated inputs and refusal boundaries, not additional
native save states or complete conversion parity.
