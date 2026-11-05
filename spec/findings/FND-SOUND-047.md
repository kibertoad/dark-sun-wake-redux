---
id: FND-SOUND-047
title: Setup hands off chunk words and a playback byte while CardGroup four substitutes the DMA summary argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:0006..1429:00DF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1429:042D..1429:077C
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000F803..0x0000F809
tool: tools/research/exec-census/sound_ds_reach_config.py, resident_listing.py and tools/evidence/report.mjs x86-operand-candidates/table at 60f6c45e; locked scientific-method-engine/executable-reader; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

Q-CONFIG-012 retains consumer effects of CardGroup, driver chunk numbers
and SamplePlaybackMode. FND-SOUND-037 binds their parser fields;
FND-CONFIG-214 identifies output stores without naming the input fields;
FND-SOUND-020 binds name-copy and current-record producers. This follows
the selected-record handoff and its direct CardGroup consumer, with a
controlled exact-literal census rather than assuming every equal offset
refers to the same record. Addresses and field offsets are hexadecimal.

### Handoff and width

After its mode-selected driver-name copies, 1429:042D copies CardGroup
word +2A to DS:E04D, digital chunk word +D6 to E051 and music chunk word
+D8 to E053. It writes constant four to E04F. Playback field +DA is read
as one byte into AL and copied to E055, despite the parser's word store.
These are SOUND.CFG offsets +32, +36, +38 and +3A from the E01B output
base, with the constant at +34 (FND-CONFIG-213/214).

The separate 1429:063A routine has two input-selection branches, split
by record word +EC being zero. Both copy +D6 as a word to E051 and +DA
as a byte to E055, then choose the digital name by the existing mode
word. Neither branch copies CardGroup or the music chunk field there.
Which selection reaches either routine and preservation through children
remain Q-SOUND-007/010. These local statements assume valid, disjoint,
nonwrapping record/output/stack storage and retained segments.

### CardGroup summary argument

In 1429:0006's prefix, if DS:E01F is FFFF, the existing absent-value
formatting path is used before any CardGroup test. Otherwise 009A compares
E04D with four. Equality supplies word FFFF to formatter 1000:394E;
inequality supplies E01F. Both write to the same stack-local destination.
The two three-byte formats at DS:00A3 and 00A6 are independently read as
the same percent-d-NUL pattern. Thus this branch changes the supplied
numeric argument, not the conversion pattern. FND-SOUND-020's settings
copy identifies E01F as the first block's DMA word. Numeric formatter
completion, screen drawing and native device effects are not established
by this argument trace.

### Controlled literal census

The FND-SOUND-045 setup base is reused with its documented omission of
unsupported indirectCalls. Each query scans 58208 starts in 25 declared
regions, is untruncated, reports partialSearch=false and retains 59 walk
gaps. Known selected-record reads/stores are independent positive controls.
All eight reports are identical to their repeats.

| Literal | Verified memory operands | Verified other operands | Rejected overlaps | Unresolved boundaries |
| --- | ---: | ---: | ---: | ---: |
| 002A | 1 | 4 | 7 | 3 |
| 00D6 | 9 | 1 | 10 | 0 |
| 00D8 | 1 | 1 | 1 | 0 |
| 00DA | 5 | 0 | 9 | 6 |
| E04D | 2 | 0 | 1 | 0 |
| E051 | 3 | 0 | 3 | 0 |
| E053 | 1 | 0 | 1 | 0 |
| E055 | 3 | 0 | 3 | 0 |

The global E04D query verifies its store and the comparison above. The
other three global queries verify only the already identified stores.
This is not proof they are unread: settings-buffer access through a
pointer, block I/O and aliases are excluded. Several small-offset hits
use CS or another object and cannot be assigned to this record from
their literal alone. The three 002A and six 00DA unresolved boundary
candidates remain visible in the retained reports.

## Interpretation

This removes the local handoff widths, the two digital-copy alternatives
and CardGroup-four summary-argument distinction from Q-CONFIG-012. The
chunk words' persisted destinations connect to FND-CONFIG-023/024's
existing game-side ADV selector reading; no device or driver identity is
inferred merely from a numeric match. Full consumers, incoming selection,
pointer/implicit access, aliases and later driver effects remain open.

## Alternatives

Treating playback mode as a persisted word would include a second byte
the handoff does not copy. Treating CardGroup four as a measured device
mode overstates a local formatter argument branch. Zero verified global
reads do not prove a field unused. No original was run, no complete
reading declared and no parity status promoted.

## How to reproduce

Use approved SOUND_DS.EXE length 204593 and XXH3-128
236c2dc23c071eca421eb5b427caee57. Recreate the FND-SOUND-045 base/query
procedure for the eight literals above, with scanLimit=1000000 and
limit=10000. In row order the positive-control file sites are 5C9A,
5CAA, 5CB5, 5CC0, 5C9E, 5CAF, 5CBA and 5CC5. Read the two location
ranges with resident_listing.py in their listed order. For the formats,
use report.mjs table at file F803, two stride-three records with three
width-one fields, bounded by the adjacent DS:00A3/00A6/00A9 references.

Reports remain under GAME_DIR/analysis/disc-adv-survey-20261104/.
Each setup-field-<literal>-report.json has a matching -repeat.json:

| Literal | SHA-256 |
| --- | --- |
| 002a | 878841176ded411c37fb38b65210d59c9202bdc6e05a65e0a6556e1d418102a2 |
| 00d6 | 9e064e9bd5b9142fc46982ee5647a3a2cfa47afe8a54ff4db7518bfb8cf9bf8f |
| 00d8 | 34c0d781297d376356f9a643f28ed5021aad51fb70715020ff36766477170380 |
| 00da | 5a4c0071d80ae661b13cf042fa0d67f7f0da4eb7de4ea59f1d0fc84dd9fed1e6 |
| e04d | f9ea42a2c44cadf023e482ed95e4a436c9dfe32906a5bd7af2a008f850a0a602 |
| e051 | 7383f947dae1089f86d50d9faa32145d4b91d8e456d9b11f22cebf1ec320c0a4 |
| e053 | b4a3e559752df00b0885b64f6ebf7d0ff5811a945bf9c9265990b50df55e220e |
| e055 | 81bb4e9a5525ed119fb1bb8f9ecd5005b382a5c568423d6941dada26e33decc9 |

setup-field-handoff.txt and its -repeat pair have SHA-256
590d6f8264f439205d4e488de7a13c0411bdbdac2e7672e550ddd244da803dad.
setup-cardgroup-formats-report.json and its -repeat pair have SHA-256
8caefb88e0749c49382f65e2582df90327b57a78d9f86c0301bc4c3107177fb3.
Local query/table configurations remain beside them; commit no original
bytes, strings, listings or numeric reports.
