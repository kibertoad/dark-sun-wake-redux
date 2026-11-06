---
id: FND-PARTY-320
title: Known timer nesting-reset callers belong to gated sound initialization and the zero-mode setup branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:00B0..4734:010F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0293..4734:02D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:07AC..4734:07FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067681..0x00067698
tool: direct_callers.py, window_operands.py, resident_listing.py and overlay_listing.py at 82331f1d; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; existing FND-CONFIG-149/155/159, FND-SOUND-011, FND-TIME-013 and FND-PARTY-042/319
environment: null
---

## Observation

This addresses Q-PARTY-136's known nesting-reset caller admission.
Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hex.

### Resident lead is sound initialization, with an outer ready gate

FND-PARTY-319's resident file 3C7FC call is canonically
4734:02BC, inside sound initializer 4734:0133. Its paragraph
alias 475F:000C is not a separately established function entry.
The file-offset identity is unchanged.

At 0293, current DS:3444 zero jumps directly to the reset call.
Nonzero first acquires the music driver through 47E5:0074; a null
returned pair takes the initializer's error route before the reset.
A nonnull pair stores one at DS:33EE, then reaches the reset.
After the reset returns, both paths store two at DS:33EE and
retest DS:3444. Thus the pre-reset store of one is not performed
by the zero-music branch. Disabling music does not skip this reset.

The encoded near-call search for 0133 recovers its known call at
00DE after PUSH CS. Its outer far wrapper 00B0 first compares
byte 4E71:0B53 with one. Equality returns AX zero without calling
0133 or publishing the input pointer. Otherwise a null incoming far
pointer likewise returns zero. The continuing path stores its pointer
at DS:3411 and invokes 0133. Returned AL nonzero takes its error
reporting path and then the zero return; AL zero stores one into
4E71:0B53 and returns AX one. These paths require the recorded
local frames and normal child continuation; no successful device or
native initialization is inferred from the wrapper's result.

With this ready byte unchanged at one, another ordinary entry through
00B0 cannot reach the reset. A value other than one is not generally
excluded by the gate. Direct entry into 0133 or its suffix bypasses
the wrapper and remains outside that conditional exclusion.

### Ready-byte literal writers and the teardown clear

The controlled overlap window 0B50..0B54 has 38 established
operands and 56 raw candidates outside their starts. Its five write
operands are word 0B51 at 45E1:008B, byte 0B53 at
4734:0102/07F4 and byte 0B50 at 47E5:02AD/4A32:011F.
The adjacent word ends at 0B53 and does not write the ready byte;
the byte-0B50 stores do not either, regardless of segment binding.
The wrapper's known byte store is a positive control, not a negative
search inferred from an empty result. Wider, adjusted-pointer, copied,
aliased, dynamic and alternate-stream writes remain excluded.

At the end of 4734:0591's teardown route, 07F4 explicitly clears
4E71:0B53 after the preceding releases and state clears, including
DS:3411, 340D, 343E, 3409 and 3436. Its normal suffix restores
SI/BP and returns far. The suffix does not establish whether teardown
is reached or its earlier callees return. Therefore persistent readiness
cannot be assumed across admitted teardown or unknown writers.

### Overlay lead and existing setup evidence

FND-CONFIG-149 and FND-PARTY-042 already establish the second
reset call, overlay 180 +04AE, inside exported root +0141.
At +04A1 it tests DS:13F7: zero calls the reset; nonzero calls
local +0026. Both returning paths join before setup +0CD7 of
overlay 188. FND-CONFIG-159 and FND-SOUND-011 already bind
+0026's sound setup call to 4734:00B0; its direct call is at
overlay 180 +00C0. The controlled caller search recovers that call.
Neither initial branch decision establishes the value of a later reload
after child calls; their existing state-preservation conditions remain.

The known reset callers are therefore classified setup/initialization
routes, rather than unspecified timer callback bodies. This does not
prove they occur only once: actual root re-entry, ready/mode histories,
intervening teardown and excluded incoming transfers remain admission
questions for the particular getter interval.

## Interpretation

Q-PARTY-136 can reuse the wrapper's ready-one exclusion and the
two mode branches. It retains actual ready/mode state and writers,
root/teardown admission during the relevant interval, direct or indirect
bypasses and FND-PARTY-319's vector/frame conditions. No whole
retained-number contract, queue closure or complete reading follows;
FMT-PARTY-007 stays supported.

## Alternatives

- Zero music skips the reset: 0298 jumps directly to it.
- Every reset path first stores one at DS:33EE: the zero-music path
  bypasses that store.
- A successful wrapper prevents every future reset: teardown clears its
  ready byte, and the separate zero-mode route does not use that gate.
- The paragraph alias is another callable root: the established containing
  function and encoded call identify 4734:0133/02BC instead.

## How to reproduce

Reuse FND-CONFIG-149/155/159, FND-SOUND-011, FND-TIME-013 and
FND-PARTY-042 before extending the caller graph. Run direct_callers.py
for 4842:06CF, 4734:0133 and 4734:00B0; preserve its encoded
transfer scope. Decode the resident locations above and overlay 180
67681..67698. Run window_operands.py with the installed inventory
and 0B50..0B54, retaining controls, widths and raw candidates.

Reports under GAME_DIR/analysis/handler-census-20261106/ use prefix
getter-timer-reset- for resident-admission, sound-root-callers,
sound-outer, sound-outer-callers and ready-writers. The earlier
resident-caller/overlay-caller listings start before established boundaries;
their exploratory prefixes are not entry evidence. getter-timer-ready-clear.txt
retains the teardown reading. No original game, emulator session or
emulated call supplied evidence.
