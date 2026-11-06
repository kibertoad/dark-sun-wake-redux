---
id: FND-PARTY-277
title: All remaining raw writer decodes have identified enclosing instructions and no tested direct incoming call, with nine resident pointer pairs unresolved
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:11E6..3BA6:11EB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:05BA..409B:05BF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:14F0..409B:14F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:01C6..4328:0A6D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05E4..4AE5:05E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006CAF5..0x0006CB7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073BE1..0x00073BE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D4B2..0x0007D4B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008FB2C..0x0008FB2E
tool: tools/research/exec-census/direct_callers.py, alias_far_words.py, resident_listing.py and overlay_listing.py, tools/ghidra/ReportFbovOverlayMap.ps1 at 18e24bc1; Ghidra 12.1.3 ReportInstructionWindow.java from scientific-method-engine 19.10.0; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-264/275/276 and FND-EXE-641 controls
environment: null
---

## Observation

Q-PARTY-124 retains eighteen unclassified raw writer candidates after
FND-PARTY-276's sixteen operand-prefix interiors. Reuse that exact
34-candidate set and the installed DSUN.EXE identity: 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.

### Remaining enclosing instructions

The read-only reconciled Ghidra listing positively identifies an enclosing
instruction for each of the eighteen starts. Bounded source readers then
confirm every identified instruction's bytes and width independently.
Overlay mapping comes from ReportFbovOverlayMap, not segment ordering.

| Candidates | Enclosing source instructions | Apparent raw destination |
|---|---|---|
| 3 | Five-byte ES-relative word PUSH at 3BA6:11E6 and 409B:05BA/14F0, each reading record +96. | 9A50 |
| 7 | Three-byte ADD AX with immediate A2BE at 4328:01C6, 030F, 0458, 05D4, 08D7, 08F4 and 0A6A. | 9A50 |
| 5 | Three-byte local-address LEA at overlay 183 +1B15/+1B28/+1B61/+1B9A and overlay 192 +01B2. | 9A50 |
| 1 | Three-byte PUSH of 08A3 at overlay 188 +0D41. | 9A08 |
| 1 | Two-byte PUSH of one at overlay 206 +11DC. | 98E8 |
| 1 | Five-byte ADD to ES-relative byte +001B at 4AE5:05E4. | 98E8 |

The apparent raw writes start inside operands and can consume following
instruction bytes. They are not fixed DS stores to those three words in
the enclosing stream. This does not exclude implicit stack effects or
an effective ES alias; those are different address mechanisms.

The saved listing misses the known store at 39D1:007A: the reporter
explicitly says undisassembled, not absent. Its independently recorded
sibling store at 39D1:0088 is recovered as one aligned instruction.
The eighteen positive enclosing classifications agree with the source
readers, but no general negative Ghidra-reference or ownership proof is
used. No analyzer repairs or mutations are made.

All 34 raw decodes now have an instruction-boundary explanation:
sixteen prefix interiors from FND-PARTY-276 and these eighteen other
operand interiors. Whether control can enter an interior remains a
separate question, not answered by its surrounding decode.

### Bounded direct-call and resident pointer checks

The existing direct_callers query for all 34 exact interior targets gives
no direct near/far call or overlay trampoline. Resident targets are also
normalized to their descriptor segment; an `as` line is a spelling
conversion, not an incoming site. The recovered controls are the known
resident near call 3A8E:107A to 060D, the overlay-193 near calls
+04B0/+1431 to +1F88, and the seven overlay far calls through the
power-name trampoline in FND-EXE-641. These control the searched near
and fixed-up overlay/trampoline far-call forms.

For the 27 resident interior targets, alias_far_words examines all
12,965 readable relocated/fixed-up segment words, with none unreadable.
It recovers nine nearby-word pairs and no far-call-form hit to those
targets. The separate DATA-getter control recovers all 87 known far
calls. FND-PARTY-264's independent separated registration and initialized
stored-pair controls are recovered too, with the same source and reporter.

The nine pairs remain candidates, not admitted incoming pointers:

| Segment-word file site | Target spelling | Nearby matching offset word |
|---|---|---|
| 6773C | 39D1:0133 | -7 bytes |
| 8A9BD | 39D1:0133 | +8 bytes |
| 59BB3 | 39D1:0300 | +6 bytes |
| 59A32 | 39D1:0300 | +10 bytes |
| 5A004 | 39D1:0300 | +6 bytes |
| 59FDF | 39D1:0300 | +10 bytes |
| 35297 | 39D1:0B0A | +4 bytes |
| 33ED0 | 3CFA:28E9 | +12 bytes |
| 4B220 | 4228:18D9 | -6 bytes |

Targets in the last two rows are segment aliases of their raw candidate
addresses. Nearby operands can be unrelated; no pointer interpretation
is assigned until their producers are read. This pair query covers only
the resident target subset. It does not model runtime overlay-code segment
bindings or manufacture an overlay pointer from its mapped Ghidra address.

## Interpretation

Remove unidentified raw instruction boundaries for this finite set and
the tested direct-call mechanisms to its exact interior targets. Retain
the nine pair producers, jumps, unsupported near aliases, computed or
unrelocated targets, target parts more than twelve bytes apart, copied
targets and runtime overlay bindings. No far-jump absence is asserted
without its own independent form control.

The fixed-writer remainder is still 27 inventory value matches. All 34
raw candidates are classified as interiors, with incoming-transfer scope
now narrowed by the tested call forms. Larger/block/indexed/copied and
segment-alias writes remain separate. No whole getter site closes: the
83 other direct sites and repeated-hook preservation remain. FMT-PARTY-007
stays supported; no complete reading or answered queue deletion follows.

## Alternatives

- A decoded parent and no tested call are not whole-program reachability.
  An unsearched transfer can enter an operand or omit a prefix.
- The missed aligned control is an analyzer coverage gap; it supplies no
  evidence that the source instruction or its callers do not exist.
- Nine nearby-word pairs do not establish nine callbacks. Constants and
  segment operands must be distinguished from actual pointer producers.

## How to reproduce

Reuse the FND-PARTY-275/276 raw set and normalize every physical address.
For each of the eighteen remaining starts, run ReportInstructionWindow.java
with count one in the read-only INST-overlay-20261029g project,
INST-MAPPED.EXE. Its expected interior diagnostic names the holder. Read
that exact holder range from resident_listing.py or overlay_listing.py.
Retain the missed 007A control and recovered 0088 control separately.

Run direct_callers.py on all 34 targets, keeping resident versus overlay
notation, then the three control targets 3A8E:060D, 193+1F88 and
176+0AF8. Run alias_far_words.py for the 27 resident targets and the
1425:0009, 1000:0387 and 2C5F:0003 controls. Keep all nine pair sites
and every excluded transfer form; do not interpret address-normalization
lines as calls or mapped overlay segments as runtime pointers.

Reports under GAME_DIR/analysis/class-table-closure-20261106/ have
raw-writer-boundary-queries, raw-writer-boundaries,
raw-writer-containing-instructions, raw-writer-parent-resident-source,
raw-writer-parent-overlay-source, raw-writer-aligned-control,
raw-writer-direct-target-candidates, raw-writer-resident-pointer-candidates,
raw-writer-pointer-call-control and raw-writer-overlay-call-control stems.
No original game ran; all original-derived reports remain local.
