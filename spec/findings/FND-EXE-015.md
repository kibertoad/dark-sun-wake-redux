---
id: FND-EXE-015
title: Compiled command lookup tests the supplied name then COM EXE BAT before PATH candidates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005933F0..0x005939FC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593A64..0x00593BD7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B87E0..0x004B8832
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CC0..0x00601CD6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D60..0x00601D66
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601E40..0x00601E46
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003157A0..0x003157AC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00338060..0x0033806F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00338077..0x0033807C
tool: Ghidra 12.1.3 PUBLIC, pinned bounded reporters, executable-reader 2.2.0 XXH3
environment: null
---

## Observation

The lookup called by FND-EXE-014 is `0x005933F0`. It reserves `0x10C`
stack bytes; its incoming object and name are at post-reservation offsets
`0x110` and `0x114`. On normal continuation from registration and string
helpers, it obtains the name's byte length and rejects values greater than
79 by unsigned comparison, with a zero selected result. This is a
NUL-terminated-string precondition, not evidence that arbitrary pointers are
safe. The name length is retained at local offset `0x44`.

The direct availability calls occur in this order:

| Candidate | Call | Result branch |
|---|---|---|
| Supplied name unchanged | `0x00593479` | Nonzero low byte selects the incoming name pointer |
| Supplied name plus `.COM` | `0x005934FB` | Nonzero low byte selects shared candidate storage |
| Supplied name plus `.EXE` | `0x0059354A` | Nonzero low byte selects shared candidate storage |
| Supplied name plus `.BAT` | `0x00593598` | Nonzero low byte selects shared candidate storage |

The suffixes are uppercase shipped strings. Their pointers are read from
`0x00716BA8`, `0x00716BA4` and `0x00716BA0`, respectively. Each suffixed
candidate is freshly copied from the supplied name into shared storage at
`0x0240F208`, then appended; suffixes are not accumulated. Each successful
branch enters cleanup instead of issuing the next direct availability call.
The common normal return reads the selected 32-bit pointer from local
`0x48`. The unchanged-name success and suffixed success therefore have
different pointer identity and lifetime; no caller copying or storage capacity
is inferred from this alone.

Only after all four low-byte tests fail does the routine query its object
through `0x0058CD50` with key PATH and an output string field. A false
low-byte result, null output pointer, absent equals delimiter or exhausted
candidate sequence reaches a zero selected result through cleanup. The
normal scanning path begins after the first equals sign in that returned
string; neither the environment helper nor its full writer set is read here.

For ordinary PATH segments, semicolons separate entries and leading repeated
semicolons are skipped. The copy loop admits at most 80 bytes; the special
count-80 continuation is separately recorded in FND-EXE-016. A segment with
length zero or greater than 77 is skipped. An admitted segment lacking a
trailing backslash receives one plus NUL through a 16-bit store of value
`0x005C`; its retained length increases by one. The sum of supplied-name
length, resulting directory length and 1 is computed at 32-bit width and
must be at most 79 by unsigned comparison before appending the name.

For each admitted directory/name combination, the same four candidates are
then tested at `0x005937D0`, `0x0059381A`, `0x00593864` and `0x005938AE`:
unmodified combination, plus `.COM`, plus `.EXE`, plus `.BAT`. Each is
freshly copied to shared storage. Positive tests choose that shared pointer
through cleanup; four failures resume PATH scanning. String-release,
registration, exceptional paths and storage aliasing are not a complete reading.

The helpers are resolved by their actual import slots: `0x00601CD0` jumps
through `0x02431A50` to `MSVCRT.DLL::strlen`; `0x00601CC0` through
`0x02431A44` to `strcpy`; `0x00601E40` through `0x02431A34` to `strcat`;
and `0x00601D60` through `0x02431A38` to `strchr`.

Every direct availability call uses `0x004B87E0`. That predicate passes the
candidate, a local output buffer and a one-byte output field to
`0x004B4FC0`. A zero low-byte result returns zero. Otherwise the produced
byte indexes the pointer array at `0x01BE6D60`; the selected object's
pointer-table entry at byte offset `0x34` is called with that object and
the local output buffer. Its low byte is zero-extended into the returned
32-bit value. The index producer, object provenance and indirect target
are not fully read. This boundary does not prove host file availability.

## Interpretation

The bundled source's candidate-order reading in SRC-DOSBOX-GOG-0742 now has
direct compiled support, conditional on normal helper continuation and
valid inputs. For a bare name, failure of the unchanged, COM and EXE
predicates precedes the BAT predicate, and all local candidates precede
PATH candidates. The declared wrapper's `ravager` and `sound` tokens are
below the initial length rejection, but actual drive selection, overlay
shadowing and the selected file remain Q-EXE-009.

## Alternatives

A PATH-first search, accumulated COM/EXE/BAT suffixes, or EXE-before-COM
order is ruled out on the inspected local candidate path. An unconditional
claim that a manifest-listed BAT file opens is not supported: the predicates
cross an unread normalization and drive-object boundary. No complete caller,
writer, exception, capacity or operating-system reading is claimed.

## How to reproduce

Verify the manifest identity from FND-EXE-011; the recorded read retained
size 3802624 and XXH3 `09861838aa3018346f9f15c9a4f5925c`. Reuse the saved
PE32 project at preferred base `0x00400000` without automatic reanalysis.
Summarize `0x005933F0`. Read pinned instruction windows from `0x005933F0`
(160 instructions), `0x00593660` (200), `0x00593960` (160), `0x005935B5`
(35) and `0x00593B84` (23). Interpret only the stated code ranges,
excluding adjacent functions printed after gaps. Verify outgoing arguments,
low-byte tests, unsigned length branches and selected-result stores directly.

Read 12 bytes at `0x00716BA0` and 24 bytes at `0x0073AE60`, then 5 bytes
at `0x0073AE77`. Independently search the shipped file with
`ReportPhysicalBytePattern.ps1`, maximum 16 matches, for little-endian
pointer words `0x0073AE60`, `0x0073AE65`, `0x0073AE6A`, and separately
for ASCII `.BAT`/NUL, `.EXE`/NUL, `.COM`/NUL concatenated. Recorded
matches were `0x003157A0` and `0x00338060`. The PATH/NUL query returned
six matches; use the mapped key's corresponding `0x00338077`, not a claim
that the key occurs only once.

Inspect one instruction at each string thunk and use
`ReportSymbolReferences.java` for slot fragments `02431a50`, `02431a44`,
`02431a34`, `02431a38`, and names strlen, strcpy, strcat and strchr to
verify their PE import provenance. Summarize `0x004B87E0` and inspect its
30-instruction window, stopping at `0x004B8832`. Keep the unread drive
boundary explicit. Existing start/size coverage contains both functions;
no richer report or original content is retained in Git. Run no original.
