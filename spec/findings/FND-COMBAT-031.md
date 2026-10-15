---
id: FND-COMBAT-031
title: Effect insertion shifts rows before their owner group and increments the count before field writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F95F..0x0007F9E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F9E6..0x0007FB29
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FF07..0x0007FF4F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FFA6..0x0007FFF0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004F100..0x0004F102
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:02AE..289B:02BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:02BD..289B:02DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:02DB..289B:02EA
tool: tools/research/exec-census/immediate_search.py, direct_callers.py, trampoline_target.py, overlay_listing.py and resident_listing.py; tools/evidence/report.mjs table; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The encoded-operand candidate pass for word 2100 recovers the known
effect-count reads, the equality guard at overlay 193 +1697 and the
increment at +1749. The starting-index helper 3143:000D supplies an
independently known read control (FND-COMBAT-029). This is a candidate
pass over immediates/displacements, not complete writer coverage:
computed, far/aliased, overlapping and bulk state writes are not
excluded. Other candidate count mutations remain unqualified here.

The guard and increment belong to distinct exported entries.
Trampoline 573B:0039 reaches overlay 193 +166F; 573B:003E reaches
+16F6. The former directly calls the latter at +16EC. Both have
caller-cleaned far frames. FND-PARTY-096 identifies the application
wrapper, and FND-PARTY-167 reads the duration/countdown branches.

The shipped initial word at mapped DS 57E0:2100 is zero, read from
file offset 4F100. That file value is not a census of subsequent
initialization, restoration or other runtime writers.

### Wrapper gates are separate from insertion

The wrapper tests only the low effect-code byte. Zero exits. A negative
signed byte calls the removal entry +192F with the byte negated and
exits. A positive byte proceeds only when DS:2100 is not exactly 192.
This is an equality test, not an unsigned greater-than-or-equal bound.
The positive code domain at this wrapper is therefore 1..127, not
necessarily the 0..112 domain of the effect-name table.

After that gate the wrapper obtains a DATA record. Unless its word at
offset seven is -1 it calls removal with owner and code. It then calls
overlay 195 +0AF9, passing a far pointer to its last argument byte.
A nonzero returned byte permits insertion; zero bypasses it. The
possibly changed argument byte is forwarded along with owner, second
object, DATA number, code byte and duration. There is no capacity
recheck between that handler return and the insertion call.

The exported insertion entry +16F6 itself has neither the wrapper's
code-sign gate nor its count-equals-192 test. Thus the wrapper guard
does not prove a global count or effect-ID invariant. Complete callers,
intervening handler/removal effects and other writers remain admission
dependencies, rather than being treated as harmless returning calls.

### Boundary, key and record movement

Insertion obtains an index from 3143:000D and an owner-local byte key
from +1CB6. For an unchanged nonnegative count no greater than 192,
owners in 0..47 and rows already sorted by owner, the binary search
returns the first index whose owner is at least the requested owner,
or the count when none is. The search's word sum cannot overflow in
that conditional count domain. It does not establish that its inputs
always satisfy those assumptions.

Key selection tries byte values 0 through 255 in order. For each it
calls +1C17, which searches the matching-owner rows for that key in
record byte seven. The first absent value is returned in AL; exhausting
the loop returns zero. The key is separate from the effect ID at byte
six. The key search, like the collector, reads the boundary row's owner
before testing index against count. Under a valid nonnegative count,
an index equal to count cannot enter its row body even if that owner
word matches. No special sentinel *value* is required for that stop,
although the preceding owner read still requires backing storage.

If the insertion index i is below count n, the body requests a move
of (n - i) * 10 bytes from 4E71:024D + 10*i to the address ten bytes
higher. Both the byte count and the source/destination far pointers
are explicitly supplied. The copy entry 289B:01DC normalizes the far
addresses, compares their physical ordering and selects backward
copying for this upward overlapping move. Its reached normalization
helpers and word-copy loop preserve the moved bytes in this direction.
It restores the caller's DS, SI and DI and returns with DF clear.

For valid n at most 192 the move is even-sized and at most 1920 bytes,
so this case needs neither a partial-byte tail nor a second FA00-byte
chunk. If i equals n the move is skipped. The body then increments
DS:2100 and writes the new row at i. Consequently, assuming the
admitted unchanged sorted inputs and sufficient backing space, this
producer preserves owner-group order and prepends to an existing
same-owner group. It does not append within that group or sort effects
by numeric ID. This is a conditional producer invariant, not a claim
about every writer or later table order.

### Field initialization and later duration work

Relative to the new ten-byte row, the insertion body assigns:

| Offset | Width | Value source |
|---|---|---|
| 0 | Word | Owner argument. |
| 2 | Word | Second-object argument. |
| 4 | Word | DATA-number argument forwarded by the wrapper. |
| 6 | Byte | Low effect-code byte. |
| 7 | Byte | Chosen owner-local key. |
| 8 | Byte | Zero for nonnegative duration; negated low duration byte for negative duration. |
| 9 | Byte | Low final-argument byte, which the wrapper's handler may replace. |

The order is effect ID, owner, second object, DATA number, key, final
argument, then the duration-dependent byte. Count increment precedes
all of these stores. There is no local decrement/rollback branch after
that increment.

For nonnegative duration the body prepares an owner/key packet and
passes it to 371D:01F9 with request seven and a computed time. This
requires the separate clock divisor/source and callee effects; it is
not a completed timer contract. Negative durations initialize byte
eight without that call. FND-PARTY-167 separately records the countdown
uses, including the -9999 case. No native duration or expiry was run.

## Interpretation

This answers the common insertion producer's local record assignments,
move direction, within-owner placement and mutation order for
Q-COMBAT-009. It explains how reverse insertion order can reach the
collector that FND-COMBAT-029 reads in ascending table order, without
assuming all later operations preserve it.

It also separates a checked wrapper from an exported unchecked-by-that-
wrapper insertion entry. A complete count/ID/owner invariant still needs
the initial state, all admitted callers and every relevant writer.
The source's initial zero and one full-count comparison cannot stand in
for that proof. Boundary backing storage remains necessary, while an
assumed sentinel owner value is not the local stopping condition.

## Alternatives

- Treating the insertion as append-to-owner order reverses the observed
  placement of existing same-owner records.
- The wrapper's equality check does not describe a saturating counter or
  a guard present on every exported entry. No native over-capacity case
  or game defect is claimed.
- Clearing or setting membership bits does not initialize this table.
  Its count, row bytes, key and timer state have distinct producers.
- The move is read only for these conditional small, even, upward-
  overlapping spans. No general arbitrary-pointer runtime model or
  implementation was added. Other mutation candidates and aliases remain.

## How to reproduce

Use the locked project evidence Python and approved installed DSUN.EXE.
Run immediate_search.py with 2100, retaining the known boundary-helper
read and wrapper guard as controls. Qualify the increment from the
exported entries with trampoline_target.py for 573B:0039 and
573B:003E, and direct_callers.py targets 193+166F and 193+16F6.
Read the listed overlay and resident ranges from their established
entries. Resolve the copy helper's two stored normalization targets
from their assignments rather than treating its indirect near calls
as unresolved or harmless-return stubs.

Check sign tests on the code byte, exact count equality, callee ordering,
seven argument words, copy source/destination/byte count and the count
increment before record stores. Follow the key-search count test after
its owner read. Read the initial count through report.mjs table with
the build XXH3, start 4F100 hexadecimal, count/limit one, stride two,
and a width-two field at offset zero. Its location follows from mapped
DS 57E0:2100 and MZ header 5200. Reports/configurations remain only in
GAME_DIR/analysis/combat-effects/. No original function was executed.
