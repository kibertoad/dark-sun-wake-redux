---
id: FND-CONFIG-255
title: Forwarded P reaches paired getter and music byte store through the declared unloaded keyboard return header
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/preferences_saved.py and preferences_readback.py at 16a6fba8; diagnostic retention at 782821f6; dinorefurb-dosbox-session 1.0.0
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-CONFIG-254 admits the positive keyboard route to the Preferences
trampoline, then refuses the nested outer pair. Corrected diagnostic run c
retains the actual pair4CD9:0000 at getter4028:011F, sequence57 and
197334990150ns. DS/SS is4D9A, SPFE64, BPFE68 and saved parent BPFECC.
Both menu pointers remain null. The candidate is unaccepted and phase stays
getter. The prior trampoline sample's pair was61F7:1EC9. These are sampled
representations of the same declared stack slot, not a continuous write trace.

FND-EXE-562 independently describes the overlay manager replacing a return
pair with the unloaded overlay header:0 and retaining the old offset at
header+2. The observed header4CD9 is relocated571F, the keyboard overlay's
known header. Source-specific tooling now admits only the original loaded
pair or exactly this header pair, requiring header INT3F, saved offset1EC9
and the known key trampoline's trap form with target139B. It does not follow
an arbitrary address or retarget an execution breakpoint.

Two independent 2026-11-08 runs d/e use the same checked native post-Save
source and strict restore/frame/private-save checks as FND-CONFIG-254.
Their baseline, first eight accepted keyboard samples and Tab completion
agree with that finding. They add these matching observations:

| Phase | Stop | Sequence | Time ns | SP | BP | AX | Music byte |
|---|---|---|---|---|---|---|---|
| Getter entry | 4028:011F | 57 | 197334990150 | FE64 | FE68 | 0000 | 100 |
| Byte store completed | 5FE8:0041 | 62 | 197334991616 | FE68 | FE68 | 00FF | 255 |

Addresses, registers and offsets are hexadecimal; times and music values
are decimal. Both nested keyboard samples have DS/SS4D9A, parent BPFECC,
event kind6, key1970, mode2, turn1, callback4C73:002A and handler
return65A5:0039. The active outer slot is4CD9:0000 at both, with the checked
header saved offset1EC9 and declared trap representation. SP/BP and saved
parent satisfy the existing frame checks. All ten keyboard samples are
accepted, although overall completion is withheld below.

The paired readback records independently include actual getter return
5FE8:003E with AX00FF and music still100, followed by the store above with
AX unchanged and music255. The resident getter target, caller continuation,
AL-store operand and stack binding are checked. Master sound and music are
enabled; music slot and source/getter handles areFFFF, card122 and mode4.
This is the sentinel-handle path, not admission of an actual driver query.

The nine-field history records one changed value: music100 to255 at the
store, sequence62 and197334991616ns. It supplies an actual byte mutation
delivery control, but no completed interval marker. No other watched value
change or RNG draw is recorded over15 outer observations. P delivery is
still pending at the store; only Tab is recorded complete. The observer
refuses with Preferences click delivery incomplete at endpoint and does
not resume beyond that instruction. Overall, keyboard and history complete
remain false. Owned watches and muted sessions close normally.

## Interpretation

This removes the fixed positive route's sampled outer-pair representation
and paired getter/store admission gaps from Q-CONFIG-014. Completed P
delivery and the later native endpoint remain open, as do continuous frame
and producer histories, other paths and the independent storage/driver
obligations. RULE-CONFIG-003 remains supported without complete reading.

## Alternatives

Treating the header pair as an unrelated frame would ignore the matching
active slot, SP/BP, saved parent and declared unloaded representation.
These checks do not prove every intervening manager write or an actual
return through INT3F; neither native run resumes to that return.
Pending P delivery is a probe endpoint limitation, not original failure.

## How to reproduce

Run the fixed saved-keyboard command in FND-CONFIG-254 with tooling at
16a6fba8 and an unused run name. Corrected diagnostic c is retained under
GAME_DIR/captures/rust-dos-preferences-readback/v181-q014-saved-key-c-20261108/.
Paired getter/store runs use siblings d/e. Their keyboard, readback, draw
and named-history records agree. All remain explicitly incomplete.
