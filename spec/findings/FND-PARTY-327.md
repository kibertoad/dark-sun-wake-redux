---
id: FND-PARTY-327
title: Declared segment and offset-word queries find no modeled far-pointer pair for the resident mode-write candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0AFA..2C5F:0B25
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071F0C..0x00071F11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E0B2..0x0008E0B7
tool: alias_far_words.py and far_pointer_words.py at 1f9c33e8; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-PARTY-325/326
environment: null
---

## Observation

Q-PARTY-136 retains far/aliased entry to the resident operand-interior
candidate after FND-PARTY-325's normal-stream and resident near-branch
classification. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

The target is canonical 2C5F:0B07, paragraph alias 2D0F:0007.
FND-PARTY-325 establishes it as the last immediate byte of a
DI-equals-one instruction on the known 2C5F:0AFA stream.

alias_far_words.py reads all 12965 MZ-relocation/FBOV-fixup segment
words, with zero unreadable segment mappings. For each declared segment
that can form the target with a word offset, it searches the twelve-byte
neighborhood on both sides for the required raw offset word. It finds
zero candidates for 2C5F:0B07.

The control target 2C5F:0AFA recovers the two independently recorded
far calls from FND-PARTY-325: overlay 187 +20DC, file 71F0C,
and overlay 204 +22F2, file 8E0B2. Their fixed-up segment words
are at 71F0F and 8E0B5; each offset word immediately precedes
its segment word. The report also finds a nearby-word coincidence
for control target alias 28C9:445A in overlay 190. That extra
candidate has no established pointer consumption here and is not
used as a positive control. No target pair is inferred from it.

The separate far_pointer_words.py query checks every raw word
occurrence of canonical offset 0B07 across the source. It finds
23: two in the header, eight in the load image, eleven in overlay
code and two in overlay fixup lists. None is immediately followed
by a declared relocation/fixup segment word. Its control 0AFA again
classifies the two known far calls. Raw word occurrences, including
exploratory instruction renderings, are not established instructions or
evidence that a consumer reads those bytes as an offset.

### Exact limits of the negative result

Together these results exclude the modeled unchanged raw-offset forms:
a neighboring declared segment pair within twelve bytes, including the
adjacent far-call/jump form, and an adjacent declared segment pair for
the canonical offset. The result is not a whole-program far-entry proof.

Segments formed at runtime, words paired over a larger distance,
register/memory targets, runtime arithmetic, changed storage/code and
offset words reused after their own relocation remain outside this model.
The nearby-word query compares raw neighboring offsets; it does not
resolve a consumer's later transformations. FND-PARTY-323's separate
relocation correction illustrates why a candidate requires that validation.
The resident near-branch search still covers only its stated canonical
inventory-row scope. Other code/segment spellings and indirect/table
entry retain their existing conditions.

## Interpretation

Q-PARTY-136 can remove unqueried declared-segment/raw-offset pair
forms for this named resident candidate. The actual remaining entry
question is the excluded construction/transfer mechanisms above, together
with actual mode/frame/state retention. FND-PARTY-326's two-overlay
near-pattern exclusion remains a separate result. No queue closure,
complete reading or unconditional mode invariant follows;
FMT-PARTY-007 remains supported.

## Alternatives

- A raw 0B07 word is itself an incoming pointer: none has an adjacent
  declared segment, and actual pointer consumption is not established.
- The empty candidate set has no working source control: both queries
  recover the two independently known far calls to the containing root.
- No modeled pair means no possible far entry: runtime-built, modified,
  distant-pair and relocated-offset-reuse forms remain excluded.

## How to reproduce

Run alias_far_words.py and far_pointer_words.py with installed DSUN.EXE
and both targets 2C5F:0B07 and 2C5F:0AFA. Check the target's
empty nearby-pair result, 12965 readable segment mappings, 23 raw
canonical offset occurrences without adjacent declared segments and
both known far-call controls. Reuse FND-PARTY-325's source controls.

Retain getter-mode-resident-far-alias-controls.txt and
getter-mode-resident-offset-controls.txt under
GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call supplied evidence here.
