---
id: FND-SOUND-090
title: ADV 11 initialization resets count and all registered slots before child calls
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
tool: adv_literal_operands.py at a0f58c56; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 RecoverCitedFunctions.java, ReportDecompileWindow.java and ReportInstructionWindow.java; reused FND-SOUND-087/088/089 and FND-CONFIG-237
environment: null
---

## Observation

Q-CONFIG-014 retains writers beyond the lifecycle transitions in
FND-SOUND-089. The controlled literal census identifies initialization's
count reset in the table-declared function 0066 at 3634. Driver offsets
are hexadecimal. Installed ADV 11 is the 16357-byte resource with
XXH3-128 0faa31676d7d993a393da11bde9e7885 (FND-CONFIG-237).

### Scope of the writer census

The committed census verifies RESOURCE.GFF's identity and the selected
resource, three independent function-table mappings and five known operand
controls. First-match table roots and direct near-transfer decoding produce
4435 structural instruction starts spanning 10809 distinct resource bytes.
Its 293B..295D query lists 37 matching operands. These figures are tool
coverage, not a committed function inventory or proof of native reachability.

The six listed memory-write starts are count reset 3644, slot publications
38A2/38A7, count increment 3973, slot-segment clear 39C4 and count decrement
39CB. FND-CONFIG-237 and FND-SOUND-089 already read the latter five.
The raw pass retains 45 additional candidates, all overlapping structural
decodes. They are neither promoted entries nor unconditionally rejected
alternate starts. One unresolved far call remains at 2FF0, the callback
interface in FND-RNG-082. Computed, implicit, block and aliased accesses,
changed code/segments and external effects remain excluded; the report's
negativeUsable is false.

### Reset prefix includes an implicit slot-table writer

3634 saves its frame/registers/flags, uses CLI and CLD, clears reentry word
CS:295F at 363D and count CS:295B at 3644. It sets ES from CS, then uses
bounded forward word stores to initialize adjacent data arrays. In
particular, 3660 sets AX zero, 3663 sets DI to 293B, 3666 sets CX to
sixteen and 3669 performs REP STOSW. This clears exactly 32 bytes,
CS:293B..295B: both words of all eight registered far pointers.

That implicit store is not a direct displacement write in the census.
The census instead retains the literal address formation; the connected
instruction reading proves its destination and bound. Thus the literal
writer list alone would be insufficient even for the known initialization.

There is no conditional branch or child call between the count clear and
this table clear. With valid stable code/frame/segment backing and ordinary
instruction completion, the prefix reaches an empty table and zero count
before the first child at 36A3. If reinitialization starts with occupied
slots, the interval after count clear but before table clear is temporarily
inconsistent; completed-prefix consistency must not be extended to that
interval. Actual execution exclusion and entry timing remain admission
conditions, rather than a claim that an observer cannot see it.

### Connected continuation and its limits

After the clears, a nine-iteration mapping loop writes one byte at
CS:283B plus a zero-extended mapping byte. Even the full encoded byte range
places that particular write below 293B, so this local mapping loop cannot
overwrite the registered table through its explicit destination.

The immediate children are 1383 (port-base words), 158E (fixed-loop port
initialization), 19CA (cache/voice/channel data initialization), and 3830
(callback-pointer clear). Their decompilations were read together. 158E
uses the output tail already read in FND-SOUND-088; it does not establish
device success. The remaining initializer loops issue channel requests
through the existing 2635 interface. Their actual argument/table/frame
admission and downstream effects remain conditions before asserting that
the whole initializer returns successfully and preserves every state field.
The reset-prefix result above does not require that later assertion.

## Interpretation

Q-CONFIG-014 can remove unread initialization count/table reset from its
writer discovery. The known reset establishes FND-SOUND-089's empty
count/occupancy relation at a reached prefix boundary, not solely in the
source-initialized bytes. Retain actual entry/completion and serialization,
other computed/block/alias writers and the unresolved callback, table/data
backing, valid handles and the actual histories reaching Preferences.

This is a bounded writer assessment, not a global no-other-writers claim,
a whole initializer or getter complete reading, a queue closure or an
observed runtime fault. No original game or emulated function was executed.

## Alternatives

- Initialization only resets the count: its sixteen forward zero-word
  stores clear all eight registered far pointers before any child.
- A literal writer list covers every known table write: this reset uses
  implicit stores through ES:DI, requiring connected address/count reading.
- Completed-prefix consistency proves consistency at every instruction:
  the count clear precedes the table clear.

## How to reproduce

Run tools/research/exec-census/adv_literal_operands.py using the pinned
evidence Python with -I and the licensed installed RESOURCE.GFF. Retain the
complete JSON at GAME_DIR/analysis/slot-count-writer-census-20261107/census.json,
including raw candidates, controls, exclusions and the unresolved transfer.
No report output belongs in Git.

In the existing isolated ADV11 Ghidra project under
GAME_DIR/analysis/music-driver-contract-20261107, process ADV11.bin read-only
without analysis. Recover table entry 3634, decompile lines 1..140 and
inspect 82 instructions. Recover 1383, 158E, 19CA and 3830 and decompile
lines 1..80/100/100/60 respectively. Outputs are retained under
GAME_DIR/analysis/driver-init-count-20261107 as init-contract.log and
init-children.log. Existing lifecycle, cleanup and port-output readings are
reused; no source-derived code or data is committed.
