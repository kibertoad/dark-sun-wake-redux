---
id: FND-SOUND-100
title: Two fresh post-label setup stops preserve populated records and match every written label prefix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at 884b1cec; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains label storage and later record lifetime beyond
FND-SOUND-041's parser return. FND-SOUND-021/022/023/026/027/037
supply the static/source attempt: group/label pointers, signed loops,
short printable names, allocation rounding and plain formatting. On
2026-11-09 two fresh owned RustDosSession attempts used884b1cec's
fixed `--labels` profile. The only inputs were installed SOUND_DS.EXE,
204593 bytes,XXH3-128236c2dc23c071eca421eb5b427caee57, and SOUND.INI,
54669 bytes,XXH3-128aecc7469098e0c188e0e3d056c5fd530.

Both attempts first reproduced the fixed successful parser return and
complete populated record comparisons from039/041. They then continued
once to1ABD:0092, after all three label-preparation calls and before
producer dispatch. Added guards covered the intervening notice and
label-allocation failure; neither guard was reached. No input, guest
writes, frames, producer selection, driver execution or gameplay was
requested. Each continuation observed its own existing operation handle;
no fixed emulated-time advancement was used.

At the paused later stop the named record counts, group pointers and mode
matched the earlier parser endpoint:13 MainGroup records,13 DigitalGroup
records and signed count-1 for MusicOnlyGroup, with mode word1. All
populated records again matched the independent source projection across
3302 bytes per group,6604 bytes total. Both complete later native payloads
were retained separately from their earlier parser-endpoint copies.

The probe modeled all six record/label blocks from offset-four pointers
and source-derived paragraph requests, requiring pairwise disjoint extents
outside loaded data and the active stack window, below physical A0000.
Every first label size word was checked before any label-prefix read.
The second prefix word, rounded slack and negative-group payloads were
not read. The following results agree across the fresh runs; pointer
values are hexadecimal, sizes/counts decimal:

| Group | Record pointer | Record header paragraphs | Label pointer | Label header paragraphs | Written label bytes compared | Differences |
| --- | --- | --- | --- | --- | --- | --- |
| MainGroup |2415:0004 |207 |45E2:0004 |66 |243 |0 |
| DigitalGroup |24E4:0004 |207 |35A4:0004 |66 |248 |0 |
| MusicOnlyGroup |25B3:0004 |4081 |35E6:0004 |4092 |0 |not compared |

Each populated label prefix is exactly the projected record's printable
terminated name. Only its name bytes plus NUL were read from its80-byte
slot. All26 prefixes,491 bytes total, match; uninitialized slot tails
are not included in that claim. Each positive group's modeled payload
capacity is1052 bytes for its1040-byte label-array request. The negative
group's65536-modulo request65456 has modeled capacity65468, but no
label is produced or read under its signed loop. Header agreement is
not interpreted as an initialized empty label array.

The concatenated MainGroup label-prefix expected/native SHA256 is
238ea7eb8e8254d6ccb17ccfdb0990386aa36fa6e0f8efe3a48e188534d47545;
DigitalGroup is
6e788ec6e1d981409f5cbc74ec0b7b289df992fb5bc0921ad0e9927ab61f87c6.
The record hashes remain041's independently projected values. Paused
named fields and label pointers were rechecked after the reads. The
complete label-comparison objects, including the repeated record checks,
are equal between runs; no whole-session/timing equivalence is claimed.

All four private input copies were reverified after execution. Both
session records report host_sound muted, writes empty, run_failure null
and event_log null; emulator arguments include `--audio-output null`.
Both owned session contexts closed, and process inspection found no
remaining owned emulator or Python probe. No absent input/RNG journal
is reconstructed.

## Interpretation

This admits reaching the fixed post-label stop, its named reads and the
populated installed-profile label/record coherence at that endpoint.
Q-SOUND-010 no longer needs a hypothetical successful positive label
allocation or expected-prefix claim at this stop. The three observed
label headers also corroborate the local allocation request model,
including the negative-count allocation without a payload assertion.

Intermediate ownership, heap/break history and transient aliases are not
proved by endpoint equality. Preserve later vector/chooser allocations,
far-zero vector state, BIOS register effects, port progress, mode history,
later frame/backing lifetime and nonprofile inputs. Neither native dialog
index admission nor full setup selection is established. No complete
reading, whole queue closure, format-status promotion or parity validation
is claimed.

## Alternatives

Matching label prefixes does not mean the entire80-byte slots were
initialized. The negative group's nonzero pointer does not manufacture
a populated record or label. Two equal paused endpoints do not establish
every intervening write or a general allocator/formatter contract.
This remains utility evidence with no gameplay or rule-replay claim.

## How to reproduce

Run the checkout-local Python with
`-I tools/research/rust-dos-runs/setup_parser.py <new-run-name> --labels`.
The fixed source identities, entry/return/later stops, added guards and
header-before-prefix controls are described in ADV-SURVEY and the committed
synthetic tests. No address, program target or input sequence is selectable.

Retained attempts are GAME_DIR/captures/rust-dos-setup-parser/
v181-setup-labels-20261109-a/ and v181-setup-labels-20261109-b/.
Each contains setup-parser.json, session.json, private inputs, the earlier
record pairs, later-record pairs and populated label-prefix pairs.
The report SHA256 values respectively are
96b6b82698a8179d7b8cfbc13fd2ede76d4899cc751adce853d9a63fd75747ed
and96978f82d947fed600bd78f61572a90045cc9822987a64f8a4a97d07dd314b01.
Compare semantic objects and payload hashes, retaining all original-derived
bytes only in the approved local store.
