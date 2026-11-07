---
id: FND-SOUND-091
title: Known music callback binds saved game DS and can request a sequence branch through the current driver slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45CD:035F..45CD:044C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D30..4842:0D36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D5A..4842:0D60
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00081DDF..0x00085DC4
tool: resident_listing.py and adv_function_stores.py at ee25d45a; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 RecoverCitedFunctions.java, ReportDecompileWindow.java, ReportInstructionWindow.java and ReportDataBytes.java; reused FND-RNG-082/084 and FND-SOUND-087/088/090
environment: null
---

## Observation

Q-CONFIG-014 retains the callback at ADV 11 offset 2FF0. This connected
reading supplies its known installer, call interface and callback-selected
sequence child. Driver offsets are hexadecimal; game addresses use load
1000. Source identities are those of FND-SOUND-090 and FND-CONFIG-237.
No original process or emulated function was run.

### Known binding and invocation

45CD:035F tests byte 4E71:0C22. Zero requests 4842:0D5A with current
DS:343E and callback pointer 4602:0040, then publishes byte 0C22 equal
to one without testing a success result. That pointer is the same physical
load-image address as 45CD:0390 in FND-RNG-082: the segment difference
contributes 350 bytes and the offset contributes 40. Nonzero 0C22 skips
the request. The 0373 context is inside the segment PUSH; the admitted
routine starts at 035F, not that offcut.

0D5A selects BD in the current driver slot. In ADV 11, BD at 380A saves
incoming DS at CS:2961 and the supplied far pointer at CS:2963/2965.
Clear function 3830 zeros the pointer words; initialization calls that
clear (FND-SOUND-090). Those are local producers, not an exhaustive writer
census or proof that the current slot is ADV 11.

ADV 11's controller helper 2EE8 takes a sequence far pointer and event
arguments. Its value is zero-extended from a byte, with a possible lookup
through the sequence's +12 table. On its controller-77 branch it stores
the value at sequence word +10. A zero callback far pointer skips the call.
Otherwise it saves flags and stack position, pushes that value and sequence
word +16, loads DS from CS:2961 and calls through CS:2963 at 2FF0.
Thus the known target receives first the sequence handle word and second
the event value word. The encoded CS prefix at the indirect call is
independently verified; loading game DS does not change pointer selection
to DS:2963. After an ordinary return it restores the saved stack position
and flags before its local continuation. Actual target, saved DS, frame,
sequence fields and returning callback remain admission conditions.

### The known target's selected child

FND-RNG-082 already reads 45CD:0390's direct state writes: values 7F,
7E and 24..4C select its status/counter stores; 23 selects the two
DS-relative table-byte stores; zero and other excluded values return.
For unsigned values 1..22, it requests 4842:0D30 with current DS:343E,
the callback's first argument and its value, then publishes its existing
status/table updates. This uses the current game driver word, not a driver
identity supplied by the caller. A changed slot can select another target.

0D30 selects B5. In admitted ADV 11, B5 at 3BC7 rejects handle FFFF;
otherwise it trusts the indexed sequence pointer. A zero segment word in
sequence pointer +04, or a mismatching branch-table marker, skips its scan.
It reads a word count at branch table +08 and six-byte records from +0A,
comparing the low byte of the requested value with each record's first byte.

The comparison precedes LOOP at 3C12. Count zero therefore does not mean
an empty scan: with no match it can make 65536 comparisons, advancing
six bytes with sixteen-bit offset wrap. Positive counts also require actual
table bounds; there is no local segment-span guard. No-match returns
without branch publication. On a match it combines the record's displacement
plus eight with the sequence's +08 far source, normalizes the result and
stores it at sequence +0C/+0E. It clears word +1E, invokes the existing
2C9F note sweep, then resets four words at +60..+66 to FFFF.

The segment assignment at 3C5B is checked from bytes and the independent
Capstone operand report: it loads ES from computed BX before storing ES
at +0E. Ghidra's listing reverses this register-form segment MOV, the known
rendering issue in docs/GHIDRA.md; that text is not evidence of the opposite
assignment. The callback and B5 offcut IRET controls are also byte-checked.
FND-SOUND-087/088 supply the note-sweep and voice/output effects, rather than
assuming that child is read-only or opening its dependency chain again.

## Interpretation

For the admitted saved target, DS and ADV 11 slot, the callback is a known
state-update/sequence-branch interface, rather than an arbitrary unread
function. Its local explicit stores do not assign the game's source/getter
handle words, and B5's local stores do not assign sequence current/target
level. Applying that separation still requires disjoint valid backing,
indices, frames and admitted child effects. Branch payload/count/offset
admission can change control flow and later readback histories.

Q-CONFIG-014 can remove unread known callback binding and selected B5
semantics. Retain actual registration success, slot/target/DS preservation,
other callback writers or targets, branch-resource bounds and state histories.
FND-RNG-084's older no-driver runs do not establish those loaded-driver
conditions; their observed profile is not generalized or rerun here.

No complete reading, queue closure, observed callback fault or audible
outcome is claimed. Existing findings are retained; this extends the connected
contract without changing their recorded observations.

## Alternatives

- The indirect call uses game DS for its pointer lookup: its CS prefix
  retains driver-relative pointer selection after the DS load.
- The callback always calls the originating driver: it reloads DS:343E.
- Zero branch count skips the scan: the first comparison precedes LOOP.
- The segment-MOV rendering proves BX receives ES: original bytes and
  independent operands establish ES receives BX at this site.

## How to reproduce

Retained reports are under GAME_DIR/analysis/callback-binding-contract-20261107.
Use resident_listing.py for 45CD:035F..0390 and 0390..044C and the two
wrapper ranges. registration-admitted.txt supplies the aligned installer;
game-binding.txt's initial 0373 decode is rejected context for that prefix.

Process the existing isolated ADV11 project read-only without analysis.
Recover 380A, 3830, 2EE8 and 3BC7; decompile the focused installer,
controller and B5 windows in driver-binding.log. Inspect the callsite,
installer and B5 scan/result instructions in binding-controls.log,
branch-controls.log and result-segment-controls.log. Byte controls cover
2FEB..2FF5, 3C5B..3C65 and the saved-flags patterns at 3824, 2FF8 and
3C83. Run adv_function_stores.py with B5 for independent segment operands,
retaining B5-independent-operands.txt as a candidate report, not a complete
effects proof. No original code or data is committed.
