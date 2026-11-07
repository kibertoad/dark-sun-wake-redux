---
id: FND-SOUND-086
title: ADV 11 start separates sentinel bypass, fresh reset and already-playing cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0CEE..4842:0D00
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081DDF..0x00085DC4
tool: resident_listing.py and adv_function_stores.py at 7409edca; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 RecoverCitedFunctions.java, ReportDecompileWindow.java, ReportInstructionWindow.java and ReportDataBytes.java; reused FND-CONFIG-237 and FND-RNG-082
environment: null
---

## Observation

Q-CONFIG-014 retains effects across the start request before the source
handle is copied to the getter handle (FND-SOUND-085). This connected
reading separates the selected driver's fresh-start and restart cases.
DSUN.EXE has 634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
The selected ADV 11 resource has 16357 bytes and XXH3-128
0faa31676d7d993a393da11bde9e7885 (FND-CONFIG-237). Driver addresses below
are offsets within that resource; game addresses use relocated load 1000.

### Wrapper and selected entry

The actual start wrapper 4842:0CF4 sets AX to AA and jumps to the common
dispatcher at 03F4. The adjacent wrapper 0CEE selects 9F instead. An initial
9F candidate report is therefore excluded from the start-path assessment.
FND-RNG-082 supplies the slot/table dispatch contract; current slot and
table admission remain required. ADV 11's AA table entry is 39DC.

39DC saves BP, DS, SI, DI and flags, then disables maskable interrupts.
It reads the sequence handle from SS:[BP+8]. Its local cases are:

| Entry condition | Local continuation |
| --- | --- |
| Handle FFFF | Bypasses sequence lookup and all body calls, then restores saved state and returns far. |
| Other handle, sequence state +1A not one | Calls reset 2B73, derives the playback pointer from sequence fields +08/+0A plus eight, stores that pointer at +0C/+0E, then sets +1A and +18 to one. |
| Other handle, sequence state +1A one | First calls stop helper 3A71, then joins the same reset and publication path. |

For non-FFFF handles it loads the sequence far pointer from
CS:[293B+handle]. The start body does not validate handle alignment, range
or a nonnull segment. These remain actual slot/backing admission conditions.
The incoming handle is a byte displacement, not an array index to multiply
again (FND-CONFIG-237).

FND-CONFIG-237 already reads reset 2B73..2C1D completely: it has bounded
loops, no child calls, and writes sequence current and target level +24/+26
to 100. It reobtains the sequence from the supplied handle. Applying the
connected fresh-start contract therefore requires the table, sequence
binding, source fields and call frame to remain valid and mutually admitted
through the reset. The start body retains its own DS:SI across that call.

Under those conditions, with sequence/driver/frame storage disjoint from
the game's handle words, the fresh-start case has no child beyond reset,
no direct callback, interrupt or port access, and no write to the game's
source/getter handle words. It resets level to 100 and changes sequence
state to one before the game resumes its post-call handle copy. The
getter's AE state check can consequently pass, and AF reads the current
level field, subject to intervening caller activity and the getter's own
gates (FND-CONFIG-236/237). This does not prove a persistent readback of 100:
the song caller also invokes the existing level setter afterward.

### Already-playing cleanup is a distinct effect boundary

3A71 first rejects FFFF or a table entry with zero segment. For an admitted
entry it loads the sequence and continues only for state +1A equal to one.
It calls 2C9F and 2CF3 with that far sequence pointer, then stores state
+1A zero. The start caller then executes reset and sets it back to one.
Children changing the binding, retained registers or backing can invalidate
that simple sequence, so it is not an unconditional postcondition.

The connected 2C9F reading has a bounded 32-entry loop. For a non-FF note
slot it clears that slot, derives a channel from the sequence table,
decrements the driver's channel count and invokes 2635. It finally clears
sequence word +20. Child 2CF3 and the channel-dispatch descendants of 2635
remain outside this bounded fresh-start proof. The recursive candidate
report is not a substitute for their effects reading. No claim of complete
restart preservation or hardware output follows.

### Return controls

Start's epilogue calls the IRET at 3A66, an offcut inside the linear OR
instruction at 3A65, to restore its saved flags; stop uses the same pattern
at 3AAC inside the instruction at 3AAB. Independent seven-byte windows
verify both patterns. The ordinary epilogues restore DI, SI, DS and BP and
return far at 3A70 and 3AB6 respectively. The decompiler's offcut warnings
and inferred flags/extra parameters are not source-level call contracts.

## Interpretation

Q-CONFIG-014 can remove the unread sentinel/fresh-start local effect cases
for admitted ADV 11. It retains actual initial state, table/backing/frame
admission, restart cleanup effects, other selected drivers and later writers.
Thus FND-SOUND-085's unchanged-source condition is justified for this finite
fresh-start bundle with disjoint storage, rather than assumed for every
possible start request. It is not established for native histories generally.

No complete reading, answered queue deletion, actual runtime level default
or audible outcome is claimed. No game, driver or emulated call was executed.

## Alternatives

- Every start requires broad cleanup: the non-playing case bypasses stop
  and calls only the already-read reset helper.
- 0CF4 selects 9F: the relocated wrapper explicitly selects AA.
- A successful registration alone ensures valid start: start trusts the
  current handle and table, with no local bounds or null guard.
- Fresh reset proves Preferences always sees 100: subsequent setters,
  writers, state changes and getter gates still decide the actual readback.

## How to reproduce

Use resident_listing.py on installed DSUN.EXE for 4842:0CEE..0D00.
Use adv_function_stores.py on installed RESOURCE.GFF with AA only as a
candidate map. Reuse FND-CONFIG-237's independently validated resource
dimensions, source-order table controls and raw ADV 11 project.

In the existing isolated ADV11 Ghidra project under
GAME_DIR/analysis/music-driver-contract-20261107, process ADV11.bin read-only
without analysis. Recover table-declared entry 000039DC; decompile lines
1..120 and inspect 90 instructions. Recover 00003A71 and 00002C9F, decompile
lines 1..90 and 1..100, and inspect 36 instructions at 3A71. That last window
crosses a gap after 3AB6; later instructions are not part of stop. Read seven
bytes at 3A65 and 3AAB for the offcut controls. Reuse reset evidence from
FND-CONFIG-237 rather than reinterpreting a recursive candidate walk.

Outputs are retained under GAME_DIR/analysis/start-driver-contract-20261107/:
start-wrapper.txt, start-AA-candidates.txt, ADV11-start-recovered.log and
ADV11-start-helpers.log. ADV11-start.log records the initial undisassembled
entry refusal; start-candidates.txt is the rejected 9F context. Neither
refusal nor adjacent selector is evidence about the start body's behavior.
No licensed listing or decompiler output is committed.
