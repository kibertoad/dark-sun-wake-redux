---
id: FND-SOUND-088
title: ADV 11 active voice cleanup closes at driver-data writes and fixed-count port output
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081DDF..0x00085DC4
tool: Ghidra 12.1.3 RecoverCitedFunctions.java, ReportFunctionSummary.java, ReportDecompileWindow.java and ReportInstructionWindow.java; reused FND-SOUND-086/087 and FND-CONFIG-237
environment: null
---

## Observation

Q-CONFIG-014 retains active cleanup's voice-effect roots 1B1D and 1B88
after FND-SOUND-087. This reading covers both bodies and their connected
output helpers 13A8, 13C4 and 13DF. Offsets are hexadecimal within installed
ADV 11, the 16357-byte resource with XXH3-128
0faa31676d7d993a393da11bde9e7885 identified in FND-CONFIG-237.
No driver, game or emulated function was executed.

### Release and update write domains

1B1D takes a voice index. If driver byte CS:0DBD+index is FF, it returns
without body writes or calls. Otherwise it clears bit 20 of that voice's
CS:0E71 byte, sets mask 01 in its CS:0E49 update mask and calls 1B88.
After return it reloads the channel and assignment bytes, decrements the
channel's CS:1339 counter, clears the assignment tables at CS:1349 and
conditionally CS:134C, and sets the voice's assignment byte to FF. Voice
types zero and three also clear its CS:0D95 active marker. The decrement
has no floor check; no saturation is inferred.

1B88 also returns immediately for assignment FF. Otherwise it reads voice,
channel and assignment tables through CS, prepares frame-local parameters,
and services update-mask bits 80, 40, 20, 10, 08 and 01. Their outputs
cover operator/register parameters, level-related parameters and pitch/key
parameters. This assessment does not assign audible meanings or reproduce
their arithmetic as gameplay rules. Its nonstack memory writes are to
CS:0D1C, the indexed voice mask CS:0E49 and cached voice byte CS:0E5D.
It calls only the three output helpers below.

Each serviced bit is cleared before returning to the next mask test.
Voice type three uses a second parameter set: its frame flag is cleared
once and the saved original mask is restored for that second pass. The
flag cannot request a third pass under an unchanged admitted frame. Other
types use one pass. Its pitch normalization loops add or subtract fixed
positive steps with signed comparisons; they move toward the tested
nonnegative bounded ranges rather than depending on an external input.
The inspected division uses the nonzero constant 127.

The complete body ranges are 1B1D..1B88 and 1B88..2161, half-open. The
update's ordinary RETF is at 1E3F, with later branch blocks returning to
its mask tests; stopping a linear reading at that early return would miss
those blocks. Function-summary diagnostics report no cut call. The full
decompilation and successive instruction windows agree on the direct
transfers and write domains. Neither root has a dynamic call or an
interrupt request. Both restore saved DS, SI, DI and BP on their ordinary
returns, subject to valid frames and unchanged code.

### Output is finite code, not a device-ready wait

13A8 and 13C4 prepare register/data bytes from CS-indexed mapping tables
and arguments, then jump through 13DE to a shared tail at 13DF. That tail
can also be called directly with register inputs. It obtains the base
port from CS:0645, adds twice the supplied bank byte, writes the register
byte there, performs six input reads, writes the data byte to the next
port, and performs eight input reads from the first port before RETF.

The six/eight bounds are loaded into AH; IN overwrites AL. No device-read
value selects either loop's continuation. The tail has no nonstack memory
store, callback, interrupt request or further child. The wrappers' POP BP
at 13DE balances their own frames; direct callers enter at 13DF instead.
The byte/word distinction of the two wrappers' first-argument indexing is
preserved in the instruction evidence and is not normalized by inferred
decompiler parameter types.

### Applying the bundle to handle preservation

The remaining active-voice dependency is admission, not an unread callee:
incoming voice indices, reloaded channel/assignment bytes, mapping tables
and write destinations must belong to valid driver data, disjoint from
executing code, call frames and the game's handle words. The mapping bytes
are trusted by these bodies; no local range validation establishes those
properties. Disjoint driver allocation alone is insufficient if bad indices
can overwrite its own code or frame state.

With those bindings and code stable, valid nonoverlapping frames and
ordinary port-instruction completion, this bundle's software writes remain
in driver data/frames and do not replace the source/getter handle words.
There is no remaining arbitrary callback in this particular voice bundle.
Actual port configuration, hardware behavior, asynchronous effects and
audible output are not established by the static reading. No whole-game
preservation invariant or native restart history follows.

## Interpretation

Q-CONFIG-014 can remove the unread 1B1D/1B88 descendants from its active
cleanup work. Keep FND-SOUND-087's count/occupied-slot, selected-block and
mapping admission, the concrete driver write domains above, actual native
histories, other driver targets and later setters. Repeating these output
helper readings cannot settle those remaining conditions.

This closes the finite voice-effects reading component, not the whole
queue item, a complete_reading declaration or an audible parity claim.

## Alternatives

- The output tail waits until a device reports ready: its loops count six
  and eight reads, without testing the returned values.
- An early RETF bounds the update's whole body: later directly reached
  branch blocks are part of that same function.
- Driver-relative writes automatically preserve all state: actual indices
  and alias/code/frame separation still require evidence.

## How to reproduce

Use the existing ADV11 project under
GAME_DIR/analysis/music-driver-contract-20261107, processing ADV11.bin
read-only without analysis. Recover 1B1D/1B88 and 13A8/13C4/13DF. Decompile
1B1D lines 1..100, 1B88 lines 1..160 and 161..260, and each output helper
lines 1..60. ReportFunctionSummary for both roots verifies body ranges
and cut-call diagnostics.

Inspect instruction windows at 1B1D (44), 1B88 (200), 1E06 (200),
1FF7 (145) and 13A8 (58). Restrict interpretation to the body/tail ranges:
oversized windows also print adjacent functions. Verify mask progression,
normalization branches, CS/SS writes, direct calls and fixed port counters.
Retained outputs in GAME_DIR/analysis/voice-effects-contract-20261107 are
voice-roots.log, output-bundle.log, body-summary.log, output-controls.log
and voice-instructions-1/2/3.log. No original-derived code or data is committed.
