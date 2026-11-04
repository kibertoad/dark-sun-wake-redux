---
id: FND-SOUND-022
title: Setup's small printable-label profile preserves row cardinality and local index bounds under explicit vector and callee conditions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0000..1000:0028
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:012A..1000:0151
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0174..1000:01ED
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3A0B..1000:3AA3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:000D..19CF:0280
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:070C..19CF:0792
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:08F0..19CF:0904
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:0AD2..19CF:0ADD
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001035A..0x00010363
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001CC28..0x0001CC2C
tool: tools/research/exec-census/resident_listing.py at 05d904c6 and tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

FND-SOUND-021 keeps actual list-choice admission open. This removes two
local alternatives for a bounded profile: separator text need not add a
selectable extra row, and the far-zero argument need not mean an empty
string. It gives a conditional local cardinality/index proof, not a native
choice observation or a bound imposed by the program on arbitrary inputs.
Offsets and masks below are hexadecimal; counts and lengths are decimal.

### Exact formatting inputs and concatenation

All three label-preparation format strings identified in FND-SOUND-021
are percent-s followed by NUL. The two strings 19CF:000D appends after
its first and later labels are both LF followed by NUL. These are bounded
reads at the exact independently identified argument addresses. They do
not establish formatter success or the actual labels' lengths by themselves.

The complete 1000:3A0B concatenation helper has no child call. It clears
direction, scans the destination for NUL, retains its terminator position,
scans the source through NUL and copies that complete source there. An
odd source address causes a first byte copy, then word copies and a final
odd byte when needed. SI, DI and DS are saved/restored; the destination
far pointer is returned. It receives no destination capacity. Both source
and destination must be valid terminated, nonwrapping, disjoint spans;
a capacity is a caller condition, not an inferred original check.

For positive N labels, this produces each label followed by one LF, then
the final NUL. With each label at most L bytes, its footprint is at most
N*(L+1)+1. For 1<=N<=21 and L<=40 that is at most 862 bytes, inside the
wrapper's 1024-byte local buffer. N=13 has a corresponding bound of 534.
These conditions are a profile to admit at use time. The file's declared
card counts and comment about name length do not prove native values or
exclude later writers, failed parsing and unexpected formatted text.

### LF rows and padding are not byte-identical labels

The dialog's first row pass reads each input byte through the next LF or
NUL. At an LF it terminates that row, increments its row count and resets
the row column. A final NUL is written at the following row's beginning
without incrementing the count. Thus a string of N newline-terminated
labels reaches this stage with N counted rows and a following zero byte,
not N+1 counted rows. Embedded LF or earlier NUL in a label would require
a different profile.

When a new maximum row length is odd, the code rounds its working column
up before placing the row terminator. That case can leave the LF inside
the stored row before its NUL. Shorter rows need not take that path. The
count result is not a claim that a row equals the input label byte for byte.
For L<=40 the row's writes need at most 42 bytes of its 80-byte stride.
The extra final NUL at row N makes the complete maximum touched span
N*80+1 bytes from the row base; for N<=21 this is at most 1681 bytes.
It ends before the separately addressed locals above BP-0026 and lies
inside the declared stack frame, subject to a valid frame and disjoint
other storage. No native stack contents were inspected.

For N<=21 the first-pass count is below the signed 22 comparison, so the
alternative reparse/spacing path is not taken. Larger lists, including
their geometry and storage limits, are not covered by this profile.

### What the far-zero comparison depends on

1000:3A4A scans its second far string, then compares it with the first.
There is no null-pointer shortcut: the row-adjustment call's second
argument 0000:0000 selects actual real-mode storage. Its first byte is
part of the interrupt-zero vector, not a known NUL from the game image.

Startup calls the installer at 1000:017D before its initializer dispatch
and before the main application call. The installer saves the earlier
vector and requests interrupt-zero handler CS:0174 through DOS service
2500. If that request is honored and the vector remains installed,
the first word at far zero is 0174, hence its first two bytes are 74 and
01. The adjacent restorer requests the saved vector later; other vector
writers and actual OS/handler state at use remain unestablished here.

Under that explicit retained-prefix condition, no row made from printable
ASCII label text can equal the far-zero string. A row with a different
first byte differs immediately. If its first byte is lowercase t, its
second byte is printable text, LF or NUL, never 01. Empty labels similarly
produce LF or NUL first. The compare is therefore nonzero for every row
in this profile; no counted row is removed by the adjustment loop, and
the local row count remains N. This conclusion uses the requested vector
prefix, not an assumption that reading a null pointer returns an empty
string. It is conditional on readable terminated comparison spans and
does not prove a live vector value, service success or retention.

### Local index arithmetic after row construction

Assume the retained row count is N in this profile, the initial index is
below N or negative, and reached children return while preserving the
held SI/DI/BP and undeclared local/argument state. The entry's negative
index becomes zero; a nonnegative index is not upper-clamped. With a valid
initial index, the local navigation arithmetic preserves 0..N-1:

- Down increments below N and otherwise wraps to zero.
- Up decrements above zero and otherwise wraps to N-1.
- A direct-index event is admitted only by the signed comparisons against
  the alphabetic base and that base plus N, then subtracts the base.
  Its byte arithmetic and sign extension therefore yield only 0..N-1
  for this small positive N.
- Other events leave the index unchanged; a completion event reaches the
  normal return with the current index.

The normal return copies DI to AX. The separate layout-rejection path
returns FFFF. Conditional on termination and the stated child/storage
contracts, the returned set is therefore FFFF or 0..N-1. This does not
prove event delivery, loop termination, geometry acceptance or those
contracts for an actual run. It also does not admit an initially positive
index at least N, a nonpositive count, larger/nonprintable labels, altered
vector storage or caller paths that use FFFF again after the initial guard.

## Interpretation

Q-SOUND-010 now has an explicit bounded local computation and the exact
assumptions that make row/index correspondence follow. Actual profile
producers/retention, vector state, formatter/allocator outcomes and reached
child/register/frame effects still need evidence. Other profiles retain
the full original question. Q-SOUND-007 retains later loader/state effects;
Q-CONFIG-006 retains parsing/count admission. There is no native selection,
bug, complete-reading, parity or confidence promotion.

## Alternatives

Counting the final NUL as a counted row contradicts the missing count
increment on that path. Assuming all stored rows equal their labels omits
maximum-length rounding. Assuming the far-zero string is empty substitutes
a byte the source does not supply. Assuming the requested vector prefix
must still be present omits OS/service and later-writer obligations.
Treating the conditional index proof as a universal return guarantee
would discard the listed profile and child conditions.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Retain source-derived reports only under
GAME_DIR/analysis/disc-adv-survey-20261104/. setup-startup-and-concat.txt
and setup-zero-vector.txt add the startup/concat/installer/restorer ranges
to FND-SOUND-020/021's string and dialog listings. Inspect the LF/NUL count
branches, maximum-length rounding, row comparison arguments and each local
index update; keep child calls and nonprofile paths explicit.

Run report.mjs table twice for three three-byte formats at file 1035A,
reading each byte, and two two-byte separators at file 1CC28. Their
addresses come from the label-preparation and wrapper argument pushes,
not a signature search. The setup-label-formats and setup-label-separators
controls/reports/repeats compare byte for byte. SHA-256 values are
ed97a3d614096312b81bb41ea5a9af8f9e316b87c97079695c6d8864737c477f and
039a7daeb81a4586bbcecd3a5f8bbb2268084e931c6a7ffdee38905309dbbbf7.
The bounds above are the stated algebra from those local contracts; no
translated dialog implementation, original fixture or native execution is
used as proof.
