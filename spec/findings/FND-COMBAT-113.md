---
id: FND-COMBAT-113
title: The restore file copy checks 128-byte transfers but ignores final close results and has no own rollback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4544:0000..4544:016B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0014..44DE:01AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:022C..44DE:02A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at 7b8840c1; locked evidence Python and Capstone
environment: null
---

## Observation

FND-SAVE-008 identifies resident 4544:0000 as the file-copy helper;
FND-COMBAT-043/084 identify its numbered-load caller. This reading
qualifies its transfer and final-result contract for Q-COMBAT-018's
runtime archive provenance. Offsets below are hexadecimal.

### The restore caller selects the internal buffer

Overlay 187 +0EBE constructs the working pathname and passes its incoming
far source path, that working destination, a null far buffer and zero
double-word capacity. Its own path construction remains conditional on
FND-CONFIG-181/182's storage, string and append prerequisites.

4544:0000 starts a word success flag at one and both handle registers
at FFFF. A null supplied buffer takes the default branch, replacing the
buffer argument with SS:BP-8A and capacity with 00000080. Thus this
caller uses a 128-byte stack buffer. FND-RNG-091 independently records
the null-buffer arguments of its known callers.

The helper calls 455B:0001 separately for the source and destination,
each with an explicit far local output. A nonzero result from either
goes to the handle checks with both handle registers still FFFF. The
path helper's complete effects and actual normalized paths are not
established here.

### Destination opening precedes the combined handle check

If both path calls returned zero, the helper opens the source through
44DE:0086 with mode one, saves AX in SI, then opens the destination with
mode 3002 and saves AX in DI. It does not test SI before the destination
open. Only afterward does it reject either handle equal to FFFF.

In 44DE:0086 the destination mode's 3000 bits select an initial call
to 44DE:0014, which issues INT 21h with AH 3C, the pathname in DS:DX
and attributes zero for this call. On an admitted successful result,
0086 closes that intermediate handle and then requests AH 3D with
AL one. The intermediate close result is not tested there. This is a
sequence of external file requests, not a temporary-file replacement.
The copy body has no own test establishing distinct source and
destination file identities, and a failed source open does not prevent
that destination request sequence.

### Transfer count and final result

With both handle words admitted, 4544 calls 44DE:0127 for a double-word
length. Returned FFFFFFFF clears success. Otherwise the loop uses that
returned quantity as remaining bytes. For a stable nonnegative admitted
length and default capacity, each request is the smaller of remaining
and 128, until no bytes remain or success becomes zero.

Each iteration calls 44DE:01AF and compares both returned words with
the requested count. A mismatch clears success and skips the write.
An exact read is followed by 44DE:022C, whose returned double word
must also equal the requested count. Either mismatch clears success;
the body subtracts the requested count, then exits on the flag rather
than retrying or rolling back. For these at-most-128-byte requests,
the write helper bypasses its large-count loop and makes one nonzero
AH 40 request. Carry-clear accumulates its actual AX; carry-set records
the error and returns FFFFFFFF. The corresponding read contract is
FND-CONFIG-152 and FND-COMBAT-082.

The length helper is not proof of an admitted source position or size.
It requests current position, end position and restoration through
44DE:0064. Its carry branches follow ADD SP,8 instructions, which
themselves change carry; those branches cannot simply be read as
preserving the earlier seek result's carry. Its successful return and
the source's stable contents/position therefore remain prerequisites
to interpreting the loop as a complete copy of a particular file.
No seek failure or resulting native file state was observed here.

At the shared ending, each handle not equal to FFFF is passed to
44DE:003A, source first, destination second. Neither returned AX is
tested or assigned to the success flag. The final AX reloads the flag
from BP-2. Consequently successful count comparisons can return one
even if a final close reports failure, under preserved locals and
returning child calls. A zero admitted length makes no transfer request
but still follows the earlier destination-opening and closing steps.

## Interpretation

The local copy protocol is now bounded beyond FND-SAVE-008's summary:
128-byte requests for this load caller, exact count comparisons, no
retry on mismatch, independent final close results and no own rollback,
rename or content validation. A zero result does not by itself establish
that the old working archive survived unchanged; one does not establish
successful final closes, durable storage or valid SAVE resources.

Q-COMBAT-018 still needs admitted source/destination identity and paths,
the path helper, source length/position and external results, subsequent
open/selection and fallback, destination aliases and restored values.
This removes the unread local copy-loop and result-combination questions
without claiming a successful native restore. No rule status changes.

## Alternatives

- Opening the destination is not conditional on a successful source handle.
- Exact transfer counts do not imply that final closes succeeded.
- Stopping after a mismatch does not restore earlier destination contents.
- A returned byte count alone does not identify the source archive or admit
  its directory; the copy performs no GFF or SAVE checks.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py for
4544:0000..4544:016B, 44DE:0014..44DE:01AF and
44DE:022C..44DE:02A9, and overlay_listing.py for descriptor 187,
70CEE 70DE7. Keep argument order, AX versus DX:AX, flag-changing stack
cleanup and the final reload distinct. Reuse the earlier read/close
contracts rather than assuming operating-system success.

Retain original-derived reports only under
GAME_DIR/analysis/hit-incoming-20261104/: restore-file-copy.txt and
restore-copy-caller.txt. No original listing or file payload is committed.
