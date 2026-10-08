---
id: FND-EXE-103
title: Grounded wait target turns negative callback results into completion and separates indexed dispatch from retry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0040F59A..0x0040F5A8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004017C0..0x0040182F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401850..0x0040186E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F26E0..0x004F270F
tool: Ghidra 12.1.3 PUBLIC bounded reference, instruction and grounded-entry recovery reports
environment: null
---

## Observation

FND-EXE-102 records the wait loop at `0x00401850` reading pointer slot
`0x0075B050`, separately from the temporary callback slot `0x0075B0D0`.
An actual writer at `0x0040F5A8` stores full EAX to the wait slot. The
immediately preceding instruction supplies literal `0x004017C0` to EAX,
with no intervening call or branch. This grounds one published target, not
all pointer writers or initialization admission. Recovery of that exact entry
adds inventory start `0x004017C0` with 105 owned body bytes; no old row
is removed or changes size.

The selected target reserves twelve stack bytes and calls `0x004F26E0`.
It tests only returned AL. Nonzero AL calls through the freshly selected
full pointer at `0x0075B0D0`, without a local pointer-null guard. It tests
that callback's full EAX signed. A negative value returns full one directly.
Full zero loops back to the admission helper. Positive values greater than
127 return full zero. Positive values from one through 127 call a full target
word in array `0x0075B460` indexed by that value times four. An indexed
callee's full zero retries admission; any full nonzero return exits unchanged,
including a signed negative one. No local target-null check or complete array
producer reading accompanies the signed range check. Slot zero is not selected
by this path.

Returned AL zero instead calls `0x00520810` before freshly reading full
counter `0x0075B040`. Nonzero counter calls `0x004F28B0`, then decrements
the current stored full counter and retries admission. That decrement does not
assign the earlier snapshot minus one: a call separates the read and update.
Zero counter calls `0x00401280`, then explicitly returns full zero. Its
callee's EAX is discarded by the explicit XOR. The effects of these three
callees, counter writers and native pacing remain unresolved.

All retry routes reload admission and, when admitted again, the callback
pointer. There is no local iteration bound. A zero wait-target return feeds
FND-EXE-102's outer wait loop, which freshly calls through `0x0075B050`
again. That outer loop uses the full zero/nonzero return, not the low-byte
admission gate or the signed callback gate. Thus both loops have independently
conditional progress and mutable pointer selection.

Under the observed wait target and FND-EXE-102's temporary callback publication,
an admitted callback invocation can select `0x00417C40`. When that callback's
record checks pass, it restores `0x0075B144` and returns all ones. The
wait target reads those all ones as signed negative, replaces them with full
one, and returns; the outer wait loop exits on that nonzero one. A callback
record mismatch returns zero and retries admission instead. A positive callback
result bypasses those record checks and takes the indexed-range decisions
above. This links the two slots through an actual conditional call path;
it does not prove that any particular native invocation reached it, that the
published pointers remained unchanged, or that the helper terminated.

The admission helper prefix at `0x004F26E0` pushes three saved registers,
reserves sixteen stack bytes, reads full `0x0075B0E8` and `0x006F00A4`,
adds them modulo thirty-two bits and tests the sum signed. A sum at most zero
branches to `0x004F2817`, outside this prefix reading. A positive sum reads
`0x006F00A0` and pointer `0x01D291E4`, subtracts the sum from that first
word modulo thirty-two bits and branches on whether the pointer is zero.
Later floating-point and callback paths lie outside this finding. In particular,
the admission return cannot be assigned an invented value from the prefix or
FND-EXE-102's accumulator updates alone.

## Interpretation

The wait loop has a grounded target whose admitted path calls the temporary
callback slot. The callback's negative result is a completion route here,
whereas zero retries and bounded positive indices select separate callees.
The full nonzero outer completion test is another contract. These observed
links reduce the previously unresolved slot relationship without establishing
all pointer producers, admission-helper effects, indexed targets, termination
or the final PATH outcome. Q-EXE-009 remains open in FMT-EXE-006.

## Alternatives

- Similar pointer addresses did not establish the link; the stored literal
  target and its actual conditional call do. Unreviewed writers remain open.
- A negative callback result is not automatically a propagated error: this
  target explicitly returns one instead. An indexed handler's negative return,
  by contrast, is retained because its gate is only full zero/nonzero.
- Callback 128 and larger positive values do not index this table; zero retries
  without selecting slot zero. The caller's signed tests define this range.
- Testing AL from the admission helper does not imply a full EAX boolean.
  Callback and handler decisions use different widths and signedness.
- A nonzero counter snapshot does not prove the later decrement decreases
  that same value, or that the retry loop terminates.
- Conditional linkage is not a native observation or a complete reading of
  the admission, event, indexed-handler or pacing effects.

## How to reproduce

Recheck FND-EXE-011's identity and FND-EXE-099's physical target controls.
Use the saved Ghidra program with -noanalysis. ReportReferences.java queries
`0x0075B050` and `0x0075B0D0`, capped at 200 analyzer references each,
select leads only. Read ReportInstructionContext.java at writer `0x0040F5A8`
and callback call `0x004017D9`, each with the tool's eight-instruction context
on either side. The known indirect call at `0x00401860` and callback writer
`0x00417D3A` in FND-EXE-102 are independent read/write controls. No negative
search, exhaustive writer list or all-reference-kind claim is made.

Read ReportInstructionWindow.java at `0x004017C0` limit 50 and
`0x004F26E0` limit 55; restrict claims to the locations above rather than
following the latter's floating-point or callback paths. Recover only exact
entry `004017C0`, grounded by the literal's writer, and export permitted
function start/body-size metadata. The delta adds one size-105 row only.

Track signed callback branches separately from handler and outer-loop full
zero tests, and admission's AL test separately from them all. Follow the event
call before the counter read, the intervening call before its decrement, and
the explicit zero after the no-counter callee. Compose FND-EXE-102's restored
flag/all-ones callback route with the actual negative-to-one conversion only
under unchanged pointer/admission assumptions. Keep all reports, identity
controls, recovery input and analyzer database in GAME_DIR/analysis/exe-batches.
Do not run the interpreter or commit listings, bytes or original content.