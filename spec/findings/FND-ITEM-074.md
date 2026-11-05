---
id: FND-ITEM-074
title: Shipped queue bounds admit overlap-preserving odd and even insertion shifts under retained storage
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:01F9..371D:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004FA63..0x0004FA73
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table/operand at ac85dc95; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0; composition with FND-COMBAT-031/032 and FND-ITEM-073
environment: null
---

## Observation

Q-PARTY-116 retains queue-shift effects after FND-ITEM-073's deferred
insertion reading. FND-COMBAT-031/032 establish the mover for bounded
even-sized ten-byte rows. Queue rows have odd length seventeen, so that
result alone does not cover every queue shift. This reads the reached
odd tail and binds its bounds to the shipped queue metadata. Addresses
and offsets are hexadecimal; lengths/counts are decimal.

### Actual initial pointers and capacities

The executable's MZ header is 0x5200 (20992) bytes, unlike the utility's
0x1400 (5120) bytes.
The startup segment operand at file 5201 is a real MZ relocation to
57E0; target offset 2A63 maps to file 4FA63. A bounded sixteen-byte
structure read there gives two far pointers, two counts and two capacities.
The pointer segment words at file 4FA65/4FA69 are also MZ relocations,
verified independently by the operand reader.

| Queue | Initial relocated pointer | Count | Capacity | Nominal byte span in segment 551D |
| --- | --- | ---: | ---: | --- |
| 0 | 551D:0352 | 0 | 100 | [0352, 09F6) |
| 1 | 551D:0000 | 0 | 50 | [0000, 0352) |

These adjacent spans contain respectively 1700 and 850 bytes. Their
source-file locations start at 4A722 and 4A3D0 and end at 4ADC6 and
4A722. No buffer content or native preservation is inferred from these
pointer/capacity values. These source-specific mappings must not be
replaced by the setup utility's different header/data layout.

### Insertion footprint and direction

For retained capacity C and admitted 0<=i<n<C, 371D:01F9 requests a move
of 17*(n-i) bytes from queue row i to row i+1. Source rows i..n-1 become
destination rows i+1..n; the capacity gate leaves room for row n. For
queue one the largest shift is 833 bytes, for queue zero 1683. The
append case i=n skips the move. Word pointer arithmetic does not wrap
within either nominal span under these initial-pointer conditions.

289B:01DC converts far addresses to physical high/low words and compares
their unsigned ordering. Destination is seventeen bytes above source
here, selecting backward copying. The assigned normalizers are fixed
local targets 02DB and 02BD, not runtime-selected external routines.
With these positive lengths below FA00 and below one-megabyte physical
addresses, the entry positions both pointers at their respective start
plus length minus two, selects an even word-copy length and performs the
backward word copies. There is no second FA00-byte chunk on this profile.

For even length the word copies cover the whole interval. For odd length
they copy offsets length-2/length-1 down through 1/2. The remaining count
is one; the backward-tail branch increments both resulting pointer
offsets and copies the byte at offset zero. Thus the leading byte is
included once, after the later bytes, without overwriting an unread
source byte. This also covers the seventeen-byte single-row shift.
The forward-copy and larger-count branches are not needed to establish
this queue profile. No general multi-chunk mover claim follows.

The mover restores DS/SI/DI/BP and returns with direction clear; ES and
scratch registers are not preserved claims. It has no DOS/port operation
or external child on this path. Valid source/destination backing, disjoint
stack and retained pointer/count/capacity identity are required. Under
those conditions the queue caller retains its insertion index, writes
time/kind/payload and increments count as FND-ITEM-073 describes. Existing
sorted timestamps remain sorted, with insertion after equal times.

## Interpretation

This removes the local shift child, including its previously unproved odd
tail, from Q-PARTY-116 for the actual initial queue bounds. It does not
prove that a native call retains those pointers/capacities/counts, the
prior queue is ordered, a constructor row survived, or a queued event is
later dispatched. These storage/lifetime obligations remain distinct
from the mover's admitted local effect. No redraw or device behavior is
established by a successful byte move.

## Alternatives

Applying only the old even-length proof omits half the possible queue
shift lengths. Treating capacities as live invariants omits other
writers/restores and pointer aliases. The inventory-backed operand
search used to locate producers retains candidates and implicit-access
limits and supplies no global no-writer claim. No original was run,
complete reading declared or parity status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with the two code
ranges above in order. Under GAME_DIR/analysis/item-removal-20261104/,
effect-queue-move-contract.txt and its -repeat pair have SHA-256
4ccb02f4859f4fa82f417e1366f9e529301a1375d377a044673bc0b913f657da.

Use report.mjs table at file 4FA63, count one, stride sixteen, with eight
width-two fields at successive even offsets. effect-queue-initial.json
and its -repeat pair have SHA-256
76950b968c7d0bbadd1f9a0d19526d293c0b41e2e1a2405d168179f69e927afc.
Use report.mjs operand at loadSegment 1000 with site/targetOffset pairs
5201/2A63, 4FA65/0352 and 4FA69/0000. Retained effect-queue-dgroup,
effect-queue0-pointer and effect-queue1-pointer JSON/repeat pairs have
SHA-256 respectively:

- f7314dccfd8c950b0642513983be1f766a6414cebadf267494b2f578472ea193
- 07094d7f098fe8e843512c7246fb925c6c7485a9dc435c6aad8f7a1689617b5e
- 4a12bde3b8aa041a52df7d5011aa727b2201907166db591ac97d53054e10a6a2

Local controls remain beside the reports. window_operands.py's local
effect-queue-table-operands.txt used the committed DSUN inventory and
2A63..2A73 window to locate candidates; it is not a writer-completeness
claim. Commit no source-derived listings, buffer bytes or numeric reports.
