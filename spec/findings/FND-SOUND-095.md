---
id: FND-SOUND-095
title: Disc initialization publishes an unchecked lead-out after conditional track-table acquisition
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:052D..2660:05CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0614..2660:0871
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:035E..2660:0404
tool: resident_listing.py at f21c0f63; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 ReportDecompileWindow.java; reused FND-SOUND-013/067/094
environment: null
---

## Observation

This connected reading answers the local table-acquisition and write-order
part of Q-CONFIG-014's known configuration callback. Installed DSUN.EXE is
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Addresses and
field offsets are hexadecimal; formulas, sizes and counts below are decimal.
FND-SOUND-094 retains selection, presence failure and caller word acceptance.

### Metadata acceptance is not response validation

After successful presence, 052D sets enable byte 4E71:0C3F and calls 066C.
That helper writes command byte 0A at 4E71:0C41 and requests command three
through 035E, with that far address and length seven. FND-SOUND-067 records
the request packet, interrupt interface and status propagation. No local
clear of the other six response bytes precedes this request.

Exactly returned AX FFFF makes 066C clear 4E71:0C3D and return AL zero.
Every other word sets 0C3D to one, copies response dword 0C44 to 0C31,
and stores zero-extended last byte 0C43 minus zero-extended first byte
0C42 plus one, at word width, in 0C37. It returns AL one. There is no
local first/last ordering test, first-track lower bound, response-freshness
test or additional packet-status test. A non-FFFF result admits these
local assignments even when an external response has not been established.

### Conditional acquisition and unconditional final publication

On AL one, 052D initializes SI from the first byte and tests it against
the zero-extended last byte using signed word comparison. Each admitted
iteration calls 06F8 with that track and three zero words, ignores its
dword return, increments SI, and reloads the last byte for the next test.
Stable byte-valued first/last inputs cannot themselves cause signed overflow
in this loop: SI reaches at most 256. Changing inputs or child effects
remain separate obligations.

06F8 rechecks metadata when 0C3D is zero. If that byte remains zero it
returns zero without table writes. Otherwise a null DS:3482 far pointer
requests an allocation of seven times last-track plus two, with the sum
inside the multiplication. A null allocation is published back to that
pointer and returns zero. A nonnull incoming pointer bypasses allocation
and any capacity check.

With a nonnull table, 06F8 initializes only the command and track bytes
of its seven-byte SS-local response. It calls 035E, ignores its result,
and copies all seven response bytes to the selected table slot. The
remaining response bytes need actual external writes or admitted prior
contents; the instruction sequence supplies no successful-response proof.
FND-SOUND-067 retains the subsequent conversion of the returned dword.

After its loop, 052D reloads last-track, loads the current DS:3482 pointer,
and copies dword 4E71:0C44 to table displacement seven times last-track
plus one, then plus two. There is no pointer or allocation-result test
between the loop exit and this final write. It then returns AL one.
Thus an admitted first greater than last makes no per-track acquisition
call but still attempts the final publication. Repeated null allocation
returns likewise do not select a local failure return from 052D. These
are conditional software paths, not observations that native execution
reaches invalid storage or completes such a write.

### Finite storage conditions for stable metadata

Let F and L be the stable zero-extended first and last bytes. If F is at
most L, the caller requests L-F+1 tracks; otherwise it requests none.
The allocation request is N = 7*(L+2), between 14 and 1799 bytes, without
word-width multiplication overflow. A completed seven-byte slot write
for track i occupies relative bytes 7*i through 7*i+6. The final dword
occupies 7*(L+1)+2 through 7*(L+1)+5, ending exclusively at N-1.
It therefore fits a successful N-byte allocation with an admitted base
offset, segment and nonwrapping addressing; the last allocated byte is
not written by that final dword. It does not initialize a full extra slot.

These bounds assume metadata remains stable through the request and
allocation children and that their returns preserve the needed frame,
register and segment state. A reused nonnull pointer has no local size
revalidation. A table allocated under an earlier smaller last-track value
does not gain the new required extent from the nonnull test. Actual
capacity, pointer histories, response writes and near/far aliases remain
unproved. Merely observing a finite byte count does not establish them.

## Interpretation

Q-CONFIG-014 can remove unread local metadata acceptance, loop selection,
allocation-result consumption and final table-publication ordering. Its
remaining table obligation is now concrete: admit the current pointer's
extent and nonwrapping base for the current last-track value, response
freshness, metadata stability and disjointness from readback/startup state,
including request, allocation and external writes. First-greater-than-last
and all-null acquisition paths have no local final-pointer guard.

Known direct stores are to the named disc fields, pointer, pointed table,
request packet and frames. This is not a whole-program writer census or
proof that these domains are disjoint from the state Preferences later
reads. It neither proves device success nor closes Q-CONFIG-014, establishes
RULE-CONFIG-003, or qualifies a whole-entry complete reading. No original
process or emulated function ran.

## Alternatives

- Accepted metadata proves an ordered track range: only the returned
  FFFF sentinel is rejected locally.
- No loop iterations means no table access: the final dword copy follows
  the loop independently.
- Allocation failure aborts initialization: the caller ignores 06F8's
  result before its final pointer reload and write.
- A reused table is resized for current metadata: nonnull bypasses allocation.
- A slot copy proves a successful fresh response: request status is ignored
  and five local response bytes have no preceding local initialization.

## How to reproduce

Use resident_listing.py at the revision above on the three location ranges.
Retain table-request-controls.txt under
GAME_DIR/analysis/disc-table-contract-20261108/. Check the 066C sentinel
branch and complete returns, the 052D pretest/back edge/final write, the
06F8 null/reuse branches, and the request and copy widths. Derive the
formulas from those widths, keeping dynamic last-byte reloads explicit.

Query the retained INST-handler-20261106 project under
GAME_DIR/analysis/runtime-error-inventory-20261107/projects read-only,
with analysis disabled, using ReportDecompileWindow.java for 2660:066C
lines 1..100 and 2660:06F8 lines 1..120. table-decompile.log under the
new report directory contains complete 20-line and 33-line results.
The decompiler omits some explicit far-call arguments and narrows its
displayed return type; the relocated instruction listing decides those
widths and pushed arguments. Reuse FND-SOUND-094's complete 052D reading
and FND-SOUND-067's request/allocation interface. Original-derived reports
remain local and none is committed.
