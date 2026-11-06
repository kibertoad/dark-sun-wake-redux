---
id: FND-PARTY-308
title: Member-switch state brackets do not lock events and their forced release can reach graphics callbacks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2A81..28C9:2AD0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B4C..4328:0BD5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794D5..0x000794DD
tool: resident_listing.py at dac2b0ba; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with packaged ReportDecompileMatches.java; FND-CONFIG-171/189/197 and FND-PARTY-307
environment: null
---

## Observation

FND-PARTY-307 leaves admission of a handler use within the member
publication-to-count-clear interval. This resolves the own behavior
of its named resident state brackets and reuses the already read
graphics-service contracts. The installed DSUN.EXE identity is size
634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

28C9:2ABD's complete 19-byte body writes byte zero to current
DS:2FB8 and DS:33BA, increments word DS:0D9E with word wrap,
and returns. It has no child call, interrupt, port access or interrupt
flag instruction. Its own writes therefore neither poll nor dispatch
an event and do not disable CPU interrupts. The two flag writes are
bytes, not assignments of the words some consumers later test.

28C9:2A81 tests only the low byte of its argument. A nonzero byte
first clears word DS:0D9E. A signed-positive current count is then
decremented; other nonzero values are left unchanged. If the resulting
word is zero, it calls local far 2AAE and then 4328:0B4C. Otherwise
it returns without those calls. The 15-byte 2AAE body writes byte
one to DS:2FB8 and DS:33BA and returns; it has no further child.

Thus a zero argument is not a universal decrement-and-clamp operation:
zero stays zero and calls the services, a positive word decrements,
and a high-bit word stays nonzero and skips the services. Conversely
argument one forces the zero/service branch regardless of the prior
counter under unchanged DS/argument bindings. It does not merely
release one level of a nested count.

### The known member-switch release can reach indirect graphics calls

190+0D89's below-eight return path reaches +1195, which passes word
one to 28C9:2A81 at +1197. This precedes the return to the cast
handler and its later selected-mode/count-zero branch. The inner
190+05A9 helper instead passed zero at +06AC, as FND-PARTY-307 records.
Therefore a retained positive nested count alone cannot exclude the
outer release's refresh-service call.

FND-CONFIG-171 establishes 4328:0B4C's complete gates: nonzero
word DS:2FB8, unequal DS:33B6/33B8 and nonzero word DS:33BA.
Failed gates return without later calls. Its continuing path calls
409B:1675, the graphics-state services, 1BF3:5814 and conditionally
4400:000E. It clears word DS:2FB8 between its two service stages.
These are not calls to the APFM event dispatcher in its own body.

FND-CONFIG-197 binds the nonzero-3330 wrapper paths to 3D72:0942
and 0B84; zero-3330 paths call those services directly in the other
order. FND-CONFIG-189 identifies the actual indirect mechanisms:
the mode-two 0B84 path tests then calls current DS:A15B record +5A;
the mode-two 0942 path calls current record +5E without its own
null-target test. Their word/mode/record gates, bracket calls, target
rereads and storage conditions remain as recorded there.

This provides a concrete possible callback mechanism within the
interval, not evidence that it invokes the cursor handler. Actual
service gates, mode-two admission, record/target histories and the
callback's reached behavior are still needed. Unrelated graphics
output and general rendering success are outside this obligation.

## Interpretation

Q-PARTY-134 no longer needs an unspecified event-lock assumption for
these brackets: 2ABD does not supply such a lock, and the known
2A81(1) call forces its own refresh route. The remaining callback
admission names the reached graphics record +5A/+5E mechanisms,
their guards and actual target behavior, along with other transfers
and interrupts already outside this local reading. Q-PARTY-136 keeps
argument, segment and frame preservation. No native interleaving,
whole-client exclusion, queue closure or complete reading is asserted.
FMT-PARTY-007 remains supported.

## Alternatives

- Calling 2ABD prevents CPU interrupt delivery: its complete body
  contains no instruction changing interrupt flags.
- An outstanding positive counter blocks the known outer release:
  its nonzero low-byte argument first clears that counter.
- Reaching a graphics callback proves a cursor event: the target and
  its behavior must still be bound; the two kinds are not equated.

## How to reproduce

Use resident_listing.py for 28C9:2A81..2AD0 and compare 4328:0B4C's
existing complete reading in FND-CONFIG-171. Reuse cast-member-selection190
for +1195/+1197 and FND-PARTY-307's selection interval. The fresh
resident listing is cast-state-brackets.txt under
GAME_DIR/analysis/handler-census-20261106/.

In the read-only Ghidra project, ReportDecompileMatches.java at
28c9:2a81 with patterns 0d9e and 0b4c recovers the known counter
and callee controls. Instructions decide low-byte argument and
signed-word comparisons. An initial pattern 0xd9e matches no text
because the decompiler uses a symbolic field spelling; it supplies
no absence evidence. Retain cast-brackets-ghidra and controls logs
in the same local store. No original game, emulator session or
emulated call ran.
