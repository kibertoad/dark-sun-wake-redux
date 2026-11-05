---
id: FND-SOUND-032
title: Setup stream flush separates nonnegative reset paths from negative-count writeback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:292B..1000:29F8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:29F8..1000:2A3A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2EB3..1000:2EF1
tool: tools/research/exec-census/resident_listing.py at 4fdccb8c; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

FND-SOUND-030 leaves flush child 292B conditional. This reads its local
state transitions and table-scan callers. Offsets and masks are hexadecimal.
All pointers, counts, table entries and child effects remain conditional
on valid retained storage. No successful native stream is inferred.

With a nonzero far stream pointer, 1000:292B compares the word at stream
offset 12 with the supplied pointer's offset word. A mismatch returns
FFFF without writing stream state. This checks no segment identity.
For a match it tests the signed count word at offset 00.

For a nonnegative count, flag 0008 at offset 02 forces count zero.
Without that flag it first checks whether the current pointer at 0C/0E
equals the supplied stream segment and its offset plus five, using
word addition. A different pointer returns zero without clearing count.
A matching pointer clears count. After either count-clear route, the
same pointer equality is tested: only a match replaces current pointer
0C/0E with base pointer 08/0A. Every such route returns zero and calls
no child. There is no lower write on these nonnegative branches.

For a negative count it computes SI as the low word of size at 06 plus
count plus one. Before calling a child it subtracts SI from count and
resets current pointer from base pointer. It then calls 3B03 with the
signed byte handle at 04, base far pointer and SI as requested count.
Equal AX and SI returns zero. A mismatch also returns zero if flag
0200 is set in the stream after the child; otherwise it ORs 0010 into
the stream flags and returns FFFF. Count and pointer changes made before
the call are not undone. The child's preservation and write/result
contract is not established here.

A zero stream pointer instead calls 29F8. That helper walks DS:DBA8
with stride 14 for the word iteration count taken from DS:DD38. Each
entry with either bit of mask 0003 calls 292B with its nonzero table
pointer. It counts calls, ignoring their results, and returns that count.
The zero-pointer 292B caller discards this count and returns zero.

The read-side helper 2EB3 of FND-SOUND-030 uses a different selector:
exactly twenty entries at the same base and stride, with both bits of
0300 required before a 292B call. It ignores each child's result.
Its nonzero table pointers cannot take 292B's zero-pointer scan branch
under this fixed local DS/offset model. Actual table extent and contents
still need admission; the two scan counts are not interchangeable.

## Interpretation

This removes the local identity/count/pointer reset and flush-result
dispatch obligations from Q-CONFIG-006. Under a nonnegative count the
called flush path can return without any lower write, with distinct
retained-count and cleared-count outcomes. Under a negative count the
state changes precede the lower write and survive its mismatched result.
Read-side scanning ignores that failure; success of its caller cannot
prove all scanned entries flushed successfully.

## Alternatives

Always clearing a matched stream count contradicts the nonnegative
different-pointer branch. Assuming a failed write restores the old
pointer/count contradicts the operation order. Stream/table allocation,
identity and flag/count producers, aliases and child preservation,
3B03's lower write contract and actual OS outcomes remain Q-CONFIG-006.
This finding establishes no runtime result, full parser admission,
complete reading or parity status.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57, and resident_listing.py with -I and
the three location ranges above in order. Retain UTF-8 output under
GAME_DIR/analysis/disc-adv-survey-20261104/ as setup-flush-state.txt
and setup-flush-state-repeat.txt. Both reports have SHA-256
24b16e5afd14a135676b54a900d51085c14676ebfd0f2c47b777ed416f99894b.
Track each pointer comparison separately, the signed count split,
pre-call mutations, post-call flag read, and the distinct scan selectors.
No original program is executed or source-derived listing committed.
