---
id: FND-ITEM-067
title: A paused native placed table has an ordered counted prefix and a zero extra row that breaks the extended ordering
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/native_save.py and getter_metadata.placed_table at 3b761cf4; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-ITEM-065/066 retain actual placed-table and extra-row contents after
settling constructor ordering and modeled allocation capacity. This observes
those named fields at the already verified saved-map boundary, rather than
assuming rounded capacity is zeroed or a counted prefix remains ordered
after extending it by one row. It removes that endpoint uncertainty from
Q-PARTY-109; it does not observe a constructor or removal call.

### Paused observation contract

On 2026-11-04 native_save.py verify restored source
v181-q063-native-save-input-20261104 in a new owned muted session. The
existing approved executable/configuration, strict stopped instruction/time,
map/input state, native save-file digest and frame checks passed. The
restored stop was 2F8B:097D, emulated time 197002506750 ns, with DS=SS=4D9A.
The probe made no gameplay continuation, input or guest memory write and
recorded no RNG draw. It saved and loaded its strict local roundtrip state,
then reproduced the complete table observation and the existing metadata,
allocation-header, item-type and frame checks. The owned session closed.

The added observation reads only the named placed count and far pointer,
its four-byte header, one marker byte and exactly the counted prefix plus
one eight-byte following row. It requires count at most 1500, P:0004,
751 paragraphs, marker 77, nonwrapping physical spans and complete reads.
It does not follow a predecessor or free-list pointer, inspect a heap list,
execute a game function or request a general memory dump.

### Measured table state

| Field | Observed value |
| --- | --- |
| Counted placed rows | 873 |
| Table base | 7879:0004 |
| Header extent | 751 paragraphs |
| Marker | 77 hex |
| Bytes captured, including extra row | 6992 |
| Counted-prefix ordering by unsigned y, unsigned x, signed byte four | Ordered |
| Same ordering including the extra row | Unordered |
| Counted rows with flag 20 hex | 678 |

The extra row at index 873 is eight zero bytes: both coordinates, signed
byte four, flags and slot word are zero. The counted prefix is ordered by
the exact key of FND-ITEM-065's search, with no adjacent inversion; adding
the zero extra row breaks that order. The captured table bytes have SHA-256
a77a5411cff41432dc3e8c6fd6fa50d9e2c55bc3228dbbf01fe51735a4643168.
The complete pre/post-roundtrip observation, including header and extra
row, compared equal.

This header and marker supply an actual instance of FND-ITEM-066's modeled
capacity. The current read ends at returned-pointer offset 6992, before
the marker at offset 12011. It does not inspect the far-end rounded padding
or determine how any unobserved row was initialized. The 678 flagged rows
demonstrate that flagged placed rows exist at this boundary, not that any
one is selected by D=42 or retained through a later child.

### Publication consequence and limit

Were FND-ITEM-065's constructor entered from this preserved count, it would
advance 873 to 874 and search those 874 rows, including the observed zero
extra row. The ordered-prefix fact alone therefore cannot justify treating
that extended search input as sorted. This is a conditional composition
with the static contract, not execution of the constructor or a computed
native insertion result. No new coordinate/slot argument or returned K was
observed, and no flagged row's allocation history was reconstructed.

## Interpretation

Q-PARTY-109 retains constructor/use-time state, actual key and insertion
position, intervening writers, table/slot identity and later removal/flag
effects. Q-PARTY-077 retains broader DUAL lifetime. A strictly restored
endpoint does not establish continuous lifetime, a lost effect, erroneous
unload, complete loading or parity.

## Alternatives

Assuming the extra row was unknown at this endpoint is ruled out by its
bounded read and exact roundtrip comparison. Assuming every byte of the
whole allocation is zero or that the extended prefix is ordered would go
beyond, or contradict, the observation. Inferring a particular constructor
failure or K from ordering alone would omit the actual key and call.

## How to reproduce

With the project-local evidence Python, pinned session/emulator and the
licensed local source, run:

```text
python -I tools/research/rust-dos-runs/native_save.py v181-q063-native-save-input-20261104 <fresh-run-name> verify
```

The accepted run is
GAME_DIR/captures/rust-dos-native-save/v181-q109-placed-table-20261104/.
Its native-save.json contains placed_table, strict restore/roundtrip checks
and empty inputs/writes/draws. Its SHA-256 is
51954f9e4ce1f89567a75df736bb2b9fba9efadd357c74803c79082862365a71.
Compare placed_table.rows keys in y/x/signed-byte-four order, then include
extra_row and check the zero bytes. The observation bounds are defined in
the [checkpoint metadata plan](../../docs/implementation-plans/CHECKPOINT-METADATA.md). Original
bytes, native state, screenshots and local report stay outside Git.
