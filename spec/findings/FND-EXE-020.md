---
id: FND-EXE-020
title: Compiled filename scan counts consumed bytes separately from emitted bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B507E..0x004B512D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B5160..0x004B51B6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B53EC..0x004B53F8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B547B..0x004B54AD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B54CE..0x004B5507
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B5515..0x004B553C
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction reporter
environment: null
---

## Observation

This follows FND-EXE-017's selector phase in the filename helper, with the
input pointer already advanced past an optional two-byte drive prefix. It
reads the byte-normalization scan and output-prefix initialization, not the
later component parser or the whole helper's successful output contract.

The consumed-byte counter and emitted-byte counter are separate 32-bit
values, both initialized to zero. An empty remaining input skips the scan.
Otherwise each iteration increments the consumed counter before classifying
the byte at its previous index. Accepted bytes are emitted to the local
buffer starting at frame offset 48 and increase the emitted counter once.
Space increases only the consumed counter. No branch in this byte classifier
emits more than one byte for an input byte.

The complete local classifier has these outcomes:

| Input bytes | Outcome |
|---|---|
| ASCII lowercase a through z | Emit the corresponding uppercase byte. |
| ASCII uppercase A through Z and digits 0 through 9 | Emit unchanged. |
| ASCII space | Emit nothing. |
| ASCII slash | Emit backslash. |
| ASCII exclamation, bytes 35 through 43 inclusive, hyphen, dot, question mark, at sign, backslash, caret, underscore, grave accent, left brace, right brace and tilde | Emit unchanged. |
| Bytes 160, 189, 229, 246 and 255 | Emit unchanged. |
| Every other nonzero byte | Select the value-3 failure path described in FND-EXE-017. |

These are byte tests in the executable, not a locale-dependent classification
call or a claim about the high bytes' displayed characters.

After an admitted or skipped byte, the continuation reads the next input
byte at the consumed-count index before evaluating whether that count is
at most 79. It continues only when both the next byte is nonzero and the
unsigned count is at most 79. On leaving the scan, a second unsigned count
check rejects values greater than 79. Therefore a scan that consumes 80
bytes fails even if many are skipped spaces; the input byte at index 80
has already been read by that continuation. Earlier invalid bytes can fail
before reaching this limit. Rejection is not a successful truncation to
79 output bytes.

On the admitted exit, the helper appends NUL at the emitted-count index.
The emitted count is at most the consumed count, so this local terminator
is at an index no greater than 79. The counter proof concerns this local
buffer only; it does not establish every later output write's bound.

The first normalized local byte then selects output-prefix initialization.
Backslash writes NUL at the caller's output-name pointer, starting with an
empty prefix. Otherwise the helper reloads the pointer array at
`0x01BE6D60` using the selector byte cached at frame offset 38, adds four
to that selected pointer, and passes that address as the source of the
imported strcpy thunk identified in FND-EXE-015. The destination is the
caller's output-name pointer retained at frame offset 44. This copy has
no explicit length argument or local source-length guard in this phase.
The selected object's layout, the source string's producer and maximum
length remain unread.

After either initialization, a forward scan of the output name starts at
index zero and retains the index of each backslash that has a nonzero
following byte. The retained index starts at zero; a trailing backslash
does not update it. This is a local setup for subsequent component handling,
whose truncations, appends, error paths and return semantics are outside this
finding.

## Interpretation

Dropping spaces cannot be used to admit an arbitrarily long source name:
the limit counts consumed input, not the shorter normalized result. The
optional drive prefix is excluded from this scan because its input pointer
has already advanced. Conversely, the local scan's bounded output does not
prove that the copied object prefix or eventual caller output is bounded.
The helper has already written the selector in FND-EXE-017 before this scan
can fail; no transaction or unchanged-output guarantee follows.

## Alternatives

A limit based on emitted length, successful truncation after 80 consumed
bytes, an expansion of one byte to several in this classifier, and use of
the host CRT's case conversion for this scan are ruled out by the direct
branch sequence. Naming the object-plus-four string as a current directory
may fit the source lead, but requires its compiled producers and consumers
before becoming a layout claim. The component parser can still affect the
final name and result; this phase alone does not settle wrapper resolution.

## How to reproduce

Use FND-EXE-011's verified PE and image base, and FND-EXE-017's argument and
selector reading. Read the retained 160-instruction window from
`0x004B4FC0`, the 160-instruction window from `0x004B5160`, and 130
instructions from `0x004B53A8`. Interpret only the cited intervals, excluding
adjacent functions. Trace all classifier branches, especially unsigned
byte subtraction for letter and digit ranges, space's direct continuation,
slash's replacement store and the five admitted high bytes. Keep consumed
and emitted counters distinct; compare the continuation's next-byte read
with both count tests and the terminating store. Check the root-prefix
branch at `0x004B53EC` and the pointer-plus-four copy arguments, then follow
the output scan's retained index. Enumerate 79-byte NUL-terminated input,
80-byte input with a readable following byte, skipped-space input and empty
post-prefix input as static paths, conditional on prior selector admission.
These are path checks, not executed experiments. Retain rich reports locally
and launch no interpreter or game.
