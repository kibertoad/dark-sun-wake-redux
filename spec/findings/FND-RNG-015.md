---
id: FND-RNG-015
title: The clock wrapper retains both return words and the converter directly consumes three time bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:046E..1000:0494
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:386D..1000:3986
tool: tools/research/exec-census/resident_listing.py; installed-build fingerprint and MZ relocation validation; Capstone 5.0.9 with default word operand widths checked
environment: null
---

## Observation

The far clock wrapper reads one word-sized optional near output pointer.
It reserves twelve local bytes, obtains two four-byte service outputs,
and passes their near offsets to the converter, date first and time second.
Both service wrappers store the returned CX word followed by the DX word
through DS. Their selecting requests are INT 21 with AH 2A and AH 2C.
Neither wrapper tests an error flag or validates the returned values.

After conversion the wrapper stores both return words in its SS locals.
For a zero optional output pointer it skips the copy. Otherwise it writes
the high word through DS at pointer+2, then the low word at pointer.
Both paths reload the saved pair and return that pair in DX:AX. The
wrapper preserves SI and uses caller cleanup of its word argument. The
two recovered seed callers pass zero and subsequently use only AX
(FND-RNG-009); truncation is in those callers, rather than this wrapper.

Within the complete converter body, the first near argument is used for
a word at date offset 0 and bytes at date offsets 2 and 3. The second is
used for unsigned bytes at time offsets 1, 0 and 3. The direct arithmetic
uses offset 1 in its hours calculation and its conditional daylight call,
multiplies offset 0 by 60, and adds offset 3. There is no direct read of
time offset 2 in this body. Its callees receive either arithmetic operands
or derived date/hour values; the environment initializer receives no
buffer argument. This is a local access observation, not proof against
an unrelated alias or an indirect global access by a callee.

The converter initializes its accumulator from a DS global pair plus
315532800, calls the environment initializer before reading that pair,
and conditionally calls a daylight helper according to another DS word.
Consequently three observed time-byte reads alone do not establish the
full formula or the admissible date/time domain. FND-RNG-014 supplies
the measured startup DS/SS identity, not every possible caller's identity.

## Interpretation

The optional full-width output contract and direct buffer access widths
can be reviewed independently of environment conversion and reseeding
reachability. Q-RNG-004 now holds the former conversion obligations;
Q-RNG-003 retains admitted paths, their memory identity and seed writes.

## Alternatives

- A word-sized seed does not mean the clock wrapper returns only a word:
  its saved pair, optional copy and return retain both words.
- No claim is made that time offset 2 is irrelevant to every indirect
  effect, that all service requests succeed, or that every possible
  optional pointer addresses safe independent storage.
- Nonzero optional outputs were not observed natively. An output aliasing
  the wrapper's local frame can affect the subsequent reload; the static
  write order is retained rather than assuming pointer independence.
- No complete reading, rule-status promotion or replay fixture is claimed.

## How to reproduce

Use the locked project evidence Python with resident_listing.py on the
approved installed DSUN.EXE. Read the three listed half-open ranges.
Check the far frames, caller cleanup, DS stores and SS local reloads,
then enumerate each converter reference through its two near arguments.
Check unprefixed byte sign extensions in the date arithmetic as word
operations; the time byte loads explicitly clear the upper byte.
Keep source-derived listings only in GAME_DIR. The observed startup
buffer case is reproduced separately by FND-RNG-014's fixed probe.
