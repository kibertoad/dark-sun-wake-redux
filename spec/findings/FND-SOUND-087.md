---
id: FND-SOUND-087
title: Restart cleanup admits a quiet case but active channel remapping traverses registered sequences
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
tool: Ghidra 12.1.3 RecoverCitedFunctions.java, ReportDecompileWindow.java and ReportInstructionWindow.java; reused FND-SOUND-086, FND-CONFIG-237 and FND-RNG-082
environment: null
---

## Observation

Q-CONFIG-014 retains the already-playing cleanup reached by ADV 11's
start request. This reading connects cleanup 2CF3 and dispatcher 2635 with
the immediate note/channel children, reusing stop and note sweep 2C9F from
FND-SOUND-086. Driver offsets are hexadecimal. ADV 11 is the 16357-byte
installed resource with XXH3-128 0faa31676d7d993a393da11bde9e7885,
independently identified in FND-CONFIG-237.

### Quiet restart and signed flags

2CF3 loads its far sequence argument and visits sixteen channel positions.
Four byte tables at sequence +F8, +118, +128 and +138 independently
select cleanup actions. At each comparison the branch is JL after CMP
with 40: the action runs only for signed bytes at least 64, namely encoded
40..7F. Encoded 80..FF are negative and bypass it. The tests are not
unsigned high-bit or generic nonzero tests.

| Active table at channel c | Local action |
| --- | --- |
| +F8+c | Clears driver byte CS:29A7+c and requests 2635 with status B0 OR c, controller 40 and value zero. |
| +118+c | Calls 2C1D with c, calls 3F01 with zero and zero-extended sequence mapping byte +68+c plus one, then restores that mapping byte to c. |
| +128+c | Clears bit 40 of driver byte CS:2A37+c. |
| +138+c | Requests 2635 with status B0 OR c, controller 70 and value zero. |

The loop has no child when all four tables contain only values outside
40..7F. FND-SOUND-086's preceding 2C9F note sweep has no child when all
32 note-slot bytes at sequence +148 are FF; it still clears word +20.
Together these conditions define a quiet already-playing restart: with
stable valid frames/bindings and disjoint storage, the cleanup makes only
that note-count clear, stop clears state +1A, and the already-read reset
and start publication restore level 100 and state one. No broader channel
or hardware helper is required on that path. This is a conditional code
contract, not proof that a particular native restart has those flags.

### Active remapping changes the backing obligation

2C1D uses driver word CS:295B as a count, starting with displacement zero
into the registered far-pointer table CS:293B. Zero count returns. To
obtain each counted entry it advances by four until it finds a nonzero
segment word. The scan has no explicit eight-slot end test. The count is
decremented once per occupied entry, rather than once per slot inspected.
FND-CONFIG-237 establishes eight registration slots; applying that bounded
table requires the current count and occupied entries to agree, not merely
that one requested sequence handle is valid. Otherwise this local reading
does not bound the scan to that table.

For a selected registered sequence with nonzero word +20, 2C1D examines
32 note slots. A slot equal to the requested channel is set to FF. Its
mapping byte at +68 supplies an index for a decrement of driver byte
CS:2A27+mapping; it requests 2635 with status 80 OR mapping, the stored
note byte and zero, then decrements the selected sequence's word +20.
These are byte/word decrements, with no local floor check. The routine
can therefore write sequences other than the one originally restarted.
Actual pointer lifetimes, mapping bounds and all admitted registered
blocks belong to this path's preservation boundary.

### Immediate channel/note dispatch boundaries

2635 extracts a low-nibble channel and high-nibble status from its first
word, and byte arguments for the following values. The reached B0/70
request writes a driver channel byte and returns locally. B0/40 with zero
writes its driver byte and calls 25FA. That helper scans twenty driver
voices and can call 2483. The 80 note-off request also reaches 2483; that
routine scans twenty voices and can either mark driver voice state or
invoke 1B1D before clearing a matching voice's active marker.

3F01, when its driver flag permits, clears driver state and replays stored
channel controls through 2635. Its descendants can include 1B88 through
the controller-update loop, besides the note-release route above.
FND-RNG-082 already identifies 3F01 as writing driver/frame storage in its
candidate census; that is not proof of every descendant's effect domain.
The connected decompilations here classify local loops and requests, not
the full effects of 1B1D/1B88 and all descendants. They remain an explicit
boundary for active cleanup. No generic read-only or hardware-preservation
claim is inferred from the absence of a direct game-global store.

## Interpretation

Q-CONFIG-014 can remove the quiet-restart bundle and local active-cleanup
selection/count semantics from unread work. The remaining active cases
require count/occupancy and all selected sequence-block admission, mapping
values, descendant effects and return/preservation, alongside actual state
histories and other driver targets. A proof about one disjoint sequence
block is insufficient for remapping that selects other blocks.

This narrows the dependency to reachable actions and storage, without
declaring native flags or count invariants. It closes no queue item and
establishes no complete reading, audible output or observed failure.
No original process or emulated function was run.

## Alternatives

- Restart always requires channel descendants: quiet note/flag state
  bypasses them while retaining the count clear and reset effects.
- Any flag byte at least 40 unsigned activates cleanup: signed JL excludes
  encoded 80..FF.
- Remapping touches only the caller's block: 2C1D scans registered entries.
- A count at most eight alone bounds the table search: the scan skips empty
  slots without decrementing that count, so occupied entries must agree too.

## How to reproduce

Process ADV11.bin in the existing isolated ADV11 project under
GAME_DIR/analysis/music-driver-contract-20261107 read-only without analysis.
Recover entries 2CF3 and 2635; decompile lines 1..100 and 1..120. Recover
2483, 25FA, 2C1D and 3F01 and decompile lines 1..100 at each. Inspect
62 instructions at 2C1D and 90 at 2CF3 to verify count/slot progression,
signed flag tests and child arguments. Stop the local bodies at RETF
2C9E and 2DA5: requested windows also print later functions across gaps.
Do not treat those extra instructions as part of either body.

Retained outputs in GAME_DIR/analysis/start-driver-contract-20261107 are
ADV11-restart-cleanup.log, ADV11-restart-children.log and
ADV11-cleanup-controls.log; local entry lists reproduce the recovery.
Reuse FND-SOUND-086's start/stop/note sweep and FND-CONFIG-237's reset
instead of a new linear reading of those bodies. The legacy AA recursive
candidate report remains a candidate map, not a complete effects proof.
No original-derived code or data is committed.
