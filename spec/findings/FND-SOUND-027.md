---
id: FND-SOUND-027
title: Setup's plain string formatter preserves short label bytes and appends one terminator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0F25..1000:0FFE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:11C7..1000:1204
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:128B..1000:1348
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1398..1000:13B8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:30EB..1000:310F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:390E..1000:3970
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000027B8..0x000027E8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D5B5..0x0001D615
tool: tools/research/exec-census/resident_listing.py at b5bd8e13 and tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

This removes the formatter dependency from FND-SOUND-022's conditional
small-label profile. Its input is the already measured percent-s-NUL
format and a nonnull far source string of length zero through forty,
with readable termination and valid, disjoint, nonwrapping source,
destination and stack spans. It is not a general formatted-output bound.
Addresses below are hexadecimal; lengths and capacities are decimal.

1000:394E first writes NUL to the supplied destination, then invokes
1000:0F25 with callback 390E, the address of the destination far pointer,
the format and the variadic argument list. The formatter clears direction
and initializes an eighty-byte output scratch buffer, total zero, error
zero, flags 20, width FFFF and precision FFFF for the conversion.

The format lookup subtracts ASCII 32 and admits indices below 96 into
DS:DE55. It then admits classes zero through 23 into CS:13B8's word
dispatch table. Controlled measurements map s to class 17 and 11C7,
the string arm. The independent c control maps class 16 to 11A7,
the one-character arm. The exact plain format sets no padding, prefix,
suppression, width or precision modifiers.

The string arm uses flag 20 to fetch a far source pointer and advance
the variadic list by four bytes. A nonnull pointer takes the source path;
the separate null-pointer replacement is outside this profile. Helper
0F3F scans through NUL and returns its preceding byte count in CX.
The unsigned precision comparison leaves these short lengths unchanged
against FFFF. The arm jumps directly to 128B, bypassing numeric sign
and precision processing.

For the plain flags, width FFFF is signed -1 and supplies no leading
padding. The prefix and zero-fill paths are inactive. The output loop
copies exactly CX bytes from the source into the scratch buffer. With
length at most forty, its eighty-byte flush threshold is not reached
inside that loop. The trailing padding test also admits no spaces.
The saved format pointer then reaches its final NUL.

For a positive length the common final path flushes the accumulated
scratch bytes through callback 390E. The callback invokes 30EB with
exactly that length, advances the destination offset by it, and writes
NUL at the new offset. 30EB copies length/2 words and an odd final
byte after clearing direction; it restores DS, SI, DI and BP and
returns the destination far pointer. The callback returns the positive
length, so the formatter records no output error and returns that count.
For length zero no flush occurs; the initial destination NUL remains
and the formatter returns zero. The formatter restores saved ES, SI,
DI and BP on its common return.

## Interpretation

Under this profile, the destination contains precisely the source label
bytes followed by one NUL, with no added spaces, case changes or newline.
It occupies at most forty-one bytes. Therefore it fits the label builders'
eighty-byte local destination of FND-SOUND-021; their subsequent supported
NUL-copy contract does not gain extra bytes from formatting. The measured
installed positive groups' maximum source lengths, 26 and 25, fall inside
this profile (FND-SOUND-023). Source-to-record production and retained
native spans remain independent obligations, not conclusions of formatting.

## Alternatives

Null pointers, unterminated or wrapping storage, aliased stack/output,
changed format/table state and longer or modified formats are outside
this proof. In particular the internal scratch buffer is not an external
destination-capacity check. No native chooser outcome, universal formatter
guarantee, full scanner/formatter reading or parity validation is claimed.
Q-SOUND-010 retains actual records/heap/state, vector and chooser-child
admission beyond the local formatter path.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 with resident_listing.py's fixed load
model. Retain source-derived output only under
GAME_DIR/analysis/disc-adv-survey-20261104/.
setup-label-format-wrapper.txt contains 390E..3970; its earlier prefix
is outside this claim. setup-label-format-core.txt covers the formatter
entry, string arm and 30EB copy primitive. setup-label-format-tail.txt
completes the output loop, flush and common return through 13B8.
Reuse FND-SOUND-022's exact format report, not a new format-string guess.

Run report.mjs table twice using setup-print-classes-controls.json and
setup-print-jumps-controls.json. The report/repeat pairs compare exactly;
SHA-256 values respectively are
681408b9bdd271f8e5b9ef8b94526a509e3e284b1ee3a0af7c9441128d0cab02
and 43460b88c92abeaf8b16086ea2d5bcc7e8b2e897125ddf20ae59d72cf14fb49d.
Check the string/character controls against their branch instructions and
follow plain flags through both zero-length and positive short-string
continuations. No original program was executed or translated into a fixture.
