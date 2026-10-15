---
id: FND-RNG-020
title: Native startup confirms the constructed environment array and distinct timezone initialization caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession; tools/research/rust-dos-runs/clock_source.py; FND-RNG-019 bounded static prerequisites
environment: Windows; Python 3.12; pinned rust-dos v1.7.2 SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220 IRQ 5 DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:29AA..1000:2A07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3B56..1000:3CD7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811..1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000509B2..0x000509BE
    kind: file-data
---

## Observation

FND-RNG-019 reads the constructor and its initialization records.
On 2026-10-15 the extended fixed clock_source.py ran the unchanged
approved DSUN.EXE -W0 -L through the pinned muted session package,
using the manifest-verified private C: and licensed game.ins on D:.
It took and released the normal machine lock. No input, memory write,
environment mutation, screenshot or fixed-time state selection was used.

| Stage | CS:IP | DS, ES and SS | SP | Instruction count | Emulated ns |
|---|---|---|---|---|---|
| Payload allocator returned | 05BA:29B7 | 4D9A | FFD8 | 25,922 | 5,335,883 |
| Array allocator returned | 05BA:29DA | 4D9A | FFD8 | 25,999 | 5,345,416 |
| Constructor about to return | 05BA:2A06 | 4D9A | FFDC | 26,026 | 5,351,416 |
| Startup TZ lookup returned | 05BA:3B61 | 4D9A | FFD4 | 26,159 | 5,360,416 |
| Startup timezone function about to return | 05BA:3CD6 | 4D9A | FFDA | 26,225 | 5,365,216 |

Before each constructor metadata read, the probe required DS and SS
to match the relocated declared data segment. At the two allocation
stops it read the caller frame after the constructor's saved SI/DI;
at its return it read the now-exposed near frame. Each contained return
offset 0260 and saved ES 4D9A. The selected six-byte record at DS:39B2
had consumed mode FF, priority 16, target 29AA and unused segment word
zero, matching the declared near initialization call.

The size/source words were payload size 67, source segment 0529 and
array size eight. They remained unchanged at both allocation stops
and constructor return. The first allocator's returned near offset
was A6F4 and the second was A73C. The retained payload offset and
destination segment matched at the second stop. The requested spans
were nonwrapping and independent: A6F4..A737 and A73C..A744.

At constructor return, SI and DI had regained dispatcher bounds 3994
and 39BE. Both DS:3920 and its DS:0088 mirror held A73C. Reading only
array words at that root found three members followed by a zero word.
The first member equaled the payload start; all three were strictly
increasing and inside the requested payload span. No member pointer
was dereferenced. The probe read neither copied payload bytes nor
source-segment contents, and recorded no environment names or values.

The subsequent standalone initializer's name pointer matched 3980
and its constant name TZ. Its lookup returned AX zero. At its return
the far frame contained offset 0259, caller segment 05BA and saved
ES 4D9A. Its selected record at DS:39B8 had consumed mode FF, priority
30, target 3B56 and relocated segment 05BA. The offset/enable record
held 18000 and one before the function returned to startup dispatch.
This call is distinct from the converter's later call observed by
FND-RNG-017.

The existing source/near-buffer/caller checks then passed. The later
clock conversion again returned 797618096 and the validated seed entry
received low word 44976. The probe stopped before seeding and admitted
no known generator draw. Its owned session closed afterward.

The run is retained under
GAME_DIR/captures/rust-dos-clock-source/constructor-and-timezone-20261015/.
A repeated run on the final pre-read segment guards, under
constructor-and-timezone-20261015-verified/, produced the same metadata,
default records, conversion result and seed input. Nine synthetic
controls passed. No private drives, memory records or journals were committed.

## Interpretation

This supplies actual constructor-frame, requested-span and terminated-
array metadata for FND-RNG-019's startup producer. The measured lookup
array is initialized and nonempty; a zero TZ lookup cannot simply be
attributed to an uninitialized zero root in this case. Which member
strings it contains was deliberately not inspected.

It also verifies the standalone record-backed timezone call and its
default result before the later clock-source call. The two measured
calls must not be collapsed into one startup invocation. Q-RNG-005's
native constructor/default-initializer prerequisite is answered;
complete writer/lifetime, input and other-path obligations remain.

## Alternatives

- Requested spans and observed in-range pointers establish this bounded
  case, not every allocator capacity, failure path, alias or later writer.
- No payload-string admission or absence of a particular member name
  was inferred from unread contents. Only the requested lookup return
  is observed.
- Only the unchanged startup/default environment was admitted. Parsed
  TZ branches, arbitrary environments and all callback preservation
  paths remain unobserved. The probe refuses more than 1024 pointer words
  or a missing terminator within the requested array; that is its scope,
  not a new limit imposed on the original.
- No gameplay state route, pixels, input readiness, emulated helper
  fixture, validated rebuild replay or complete-reading claim follows.

## How to reproduce

Set EVIDENCE_PYTHON to the locked project interpreter and run
clock_source.py with a new plain run name. This finding's names were
constructor-and-timezone-20261015 and its -verified repeat. The fixed
probe validates the near/far initialization frames and selected records,
checks declared data/stack identity before reads, and validates sizes,
nonwrapping independent spans and array progression before proceeding
to the existing source/conversion/seed-input stops. It reads pointer
words only to their first zero within the stated bounds.

check_clock_source.py exercises wrong or incomplete frames/records,
wrong segments before memory access, size bounds, wrapping/overlapping
spans, empty/nonempty arrays and invalid or unterminated members using
synthetic segments and payloads. The original source, paths, sound/media,
build admission and remaining observation contract are those documented
by FND-RNG-017 and FND-RNG-019; all reports stay in GAME_DIR.
