---
id: FND-CONFIG-160
title: The post-setup script call is status-gated and resets working buffers before loading MAS number 99
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0299
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 565C:0020
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-156's call at file offset 000576C2 supplies
three word arguments to 172C:000C: number 99, start zero
and selector two. The preceding double-word push 00020000
packages the start and selector words; it is not one
32-bit selector argument.

The complete script entry `0x0000C4CC..0x0000C561`
compares word 4C0E:000B with two before calling anything.
Different returns immediately without the working-buffer
reset, frame resets or script loader. Equal calls 31ED,
whose four guarded fills are read in FND-SCRIPT-020.
The entry then sets 4C0E:0009 to one, clears the stop
byte and the depth/control bytes described in
FND-SCRIPT-007, and forwards the three words and low
frame-depth byte to its local execution helper 00A1.
After that helper returns it clears the stop byte again.
It does not recheck 4C0E:000B after the reset helper.

For the named nonzero number, 00A1 calls 0299 unless the
stop byte is one. The complete 0299 body,
`0x0000C759..0x0000C7DF`, sets the status word to one,
increments script depth, stores number and selector for
that depth and calls loader 0388 with number 99 and
selector two. A zero AL result calls 5702:00B1. When the
stop byte is subsequently zero, it pushes the supplied
start through 01C1. FND-SCRIPT-006 reads that frame helper;
FND-SCRIPT-019 corrects the loader's branch and size contracts.

Selector two's fresh-transfer branch asks for MAS/99.
A cached current pair, matching slot or selected-number
bypass can avoid that transfer, so the supplied arguments
alone do not prove that a resource is newly loaded.
FND-SCRIPT-001 records the installed MAS/99 source;
archive selection, stability and successful I/O remain
separate conditions.

00A1 subsequently fetches and dispatches opcodes while
the signed frame-depth byte is at least the supplied low
threshold byte and the stop byte is zero. This is a local
loop condition, not proof of script completion. The opcode
bodies and their reachable resource inputs are not all read
here. A returning loader-error helper can still affect the
stop test and later path; no observed termination is claimed.

In the initializer's own call order, an inner allocation-
check failure returns FFFF, which the outer body stores
at 4C0E:000B before this script call. If that value is
unchanged at entry, the script entry bypasses execution.
The earlier outer allocation-check failures in
FND-CONFIG-156 return before this call altogether. Under
the ordinary inner status-two result, the entry instead
reaches its resets and loader. A preexisting signed-positive
outer status can bypass the entire initializer and this
call. These are distinct local paths, not one startup
success or failure state.

The initializer later tests 4C0E:0009, while this entry
writes it to one before loading. Script dispatch, error
handling and other intervening writes can change it again.
The setter does not establish its value at the initializer's
later test or prove that the following FNFO requests run.

## Interpretation

The post-setup call has a concrete status gate and an
explicit possible MAS/99 load route. It is not an
unconditional script run or successful metadata initialization.
The reset helper is bounded, but script execution and
resource errors remain state-changing dependencies between
archive registration and FNFO requests.

## Alternatives

Q-CONFIG-008 retains the loader's actual cache state,
archive and I/O outcomes, valid reset buffers, error entry,
reachable MAS/99 instructions and changes to 4C0E:0009.
One reading reaches ordinary status two and executes the
requested script; another bypasses at the status gate or
stops/fails while loading. The local gate and callers
separate those paths, without choosing a native outcome.
FND-SCRIPT-019 and FND-SCRIPT-020 narrow the helper
contracts; neither establishes complete script effects.

## How to reproduce

Verify the call's declared overlay fixup and packed three-word
argument layout against the entry's BP-relative word loads.
Read 000C through its far return, local 00A1 through its
loop exits and 0299 through its near return. Follow the
status check before reset, every direct status/depth store,
loader result and later stop test. Combine the reset and
loader findings with FND-CONFIG-156's earlier result store
and later status test. Keep resource loading, error-helper
returns, opcode effects and native I/O outcomes conditional.
