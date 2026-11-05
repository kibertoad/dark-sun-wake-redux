---
id: FND-SOUND-041
title: Two fresh setup parser returns match every populated record byte and the predicted group size headers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py and tools/research/exec-census/setup_record_projection.py at 5d12821d; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

FND-SOUND-020 through FND-SOUND-027, FND-SOUND-029 through
FND-SOUND-038 and FND-SOUND-040 supply the static/source attempt for
Q-CONFIG-006/011 and Q-SOUND-010. FND-SOUND-039 supplies the fixed
native parser-entry/return capability. On 2026-11-05 two new owned
RustDosSession runs used the same verified SOUND_DS.EXE and SOUND.INI
identities, pins, device configuration and guards. No input, guest writes,
frames, driver load or continuation beyond parser return was requested.

At that paused return the probe required AX=1, counts 13/13/-1, the
declared relocated data segment and offset-four nonzero group pointers.
It modeled pairwise disjoint allocation extents outside loaded code/data
and the active stack, below A0000 physical. It read all three first-prefix
words before reading either populated payload. Counts, pointers and mode
were unchanged on a final named-field reread.

| Group | Pointer (hex) | Header paragraphs | Modeled payload capacity | Compared bytes | Differences |
| --- | --- | --- | --- | --- | --- |
| MainGroup | 2415:0004 | 207 | 3308 | 3302 | 0 |
| DigitalGroup | 24E4:0004 | 207 | 3308 | 3302 | 0 |
| MusicOnlyGroup | 25B3:0004 | 4081 | 65292 | 0 | not compared |

Both populated payloads equal the independent source projection across
all thirteen 254-byte records, including zero-initialized bytes, source
order and duplicate stores, absent primary fields, strings, alternate
lists, nested bits and the documented hexadecimal spelling truncation.
The negative-count group's payload and every group's slack were unread.
The second prefix word was neither interpreted nor followed.

The MainGroup native and expected SHA-256 is
bd2c43beab14eedf90b683c6ddf65e0ac8b86550415d3e850529a6a5c53f6385;
DigitalGroup is
ed1e7c4d37d83654ee89e8ff3537d24963e379fd2efa8968c5cba5ecfddd5b6b.
The phase and comparison objects are equal across fresh runs. Whole
reports differ; no timing or whole-journal equivalence is claimed.

Both private input copies were reverified after execution. Package records
show muted host sound, writes=[], run_failure=null and event_log=null.
Both session contexts closed and process inspection found no remaining
owned emulator. No absent event/RNG journal is reconstructed.

## Interpretation

This settles complete populated record-byte coherence and the modeled
group size/header bounds at this installed-source normal parser return.
It corroborates the static/source projection without promoting a whole
format or parity status. Intermediate line/tag/value aliases and writers,
later ownership/lifetime, stream errors, label/vector/chooser allocation
and child/frame behavior remain separate obligations.

## Alternatives

An endpoint equality cannot exclude transient aliases or prove a complete
allocation history. Header-derived disjoint extents do not prove later
ownership. Other source profiles and failure paths were not executed.
This is utility evidence, with no gameplay rule or RNG replay claim.
No original-derived record bytes are committed.

## How to reproduce

With the exact licensed inputs and project-local Python, run
`python -I tools/research/rust-dos-runs/setup_parser.py <new-run-name>`.
The committed synthetic tests cover field projection, header-before-payload
ordering, bounds, overlap, drift, short reads and retained differences.

Local attempts are GAME_DIR/captures/rust-dos-setup-parser/
v181-setup-records-20261105-a and v181-setup-records-20261105-b.
Each retains setup-parser.json, session.json, private inputs and the two
native/expected payload pairs. Report SHA-256 values respectively are
8eae8c3e8768f07819a80e1da65c748b66aa27fea79645504760ea60c5d48be7
and 2d178b3024fe6f30c77f0b5397e7449c40888bffbb181ec392e764b6bf079e25.
Compare semantic phases/comparison objects and payload hashes; retain all
derived bytes only in that approved local store.
