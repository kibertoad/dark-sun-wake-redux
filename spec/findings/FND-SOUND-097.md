---
id: FND-SOUND-097
title: Setup chooser completion uses a registered ordinal and byte-wide transformed polling state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:009A..19CF:03AB
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 19CF:03AE..19CF:0ADD
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:040F..1A7C:041A
tool: Ghidra 12.1.3 ReportFunctionSummary.java; tools/research/exec-census/resident_listing.py
environment: null
---

## Observation

Q-SOUND-010 retains chooser children and continuation beyond
FND-SOUND-022's conditional small-label proof. This binds the chooser's
actual call arguments and event consumption to the existing helper
contracts in FND-EXE-487/488/489, rather than repeating their readings
or the established row arithmetic. The source is SOUND_DS.EXE,204593
bytes,XXH3-128236c2dc23c071eca421eb5b427caee57. Offsets and byte values
are hexadecimal; counts below are decimal.

The admitted profile remains022's positive N<=21 printable labels,
retained row count N, valid initial index, readable far-zero comparison
storage, and valid disjoint frame/table spans. Reached children must return
with the held SI/DI/BP, required segments and undeclared local/argument
state preserved. Actual preservation, native input delivery and termination
are not proved by these caller instructions.

### Registration supplies the completion value

After loading its initial index into DI and clamping only negative values,
the chooser calls1A7C:040F. That pure helper writes word zero to DS:18F8,
restores BP and returns far, with no child or interrupt and no local
SI/DI/DS/ES change. The chooser then calls0195, whose interrupt-dependent
flag and preservation conditions remain FND-EXE-487's contract.

After row construction/layout and successful backing allocation, the
registration loop at0545..0584 calls1A7C:01C6 once per retained row.
It passes left, row-Y, right, row-Y as four sign-extended coordinate words
and a fifth argument whose low byte is row index plus41. It saves each
returned AX in a local word array, then advances row-Y and the loop index.
FND-EXE-487 establishes that01C6 consumes only the fifth argument's low
byte and returns the previous registration index, with no child call and
local SI/DI/segment preservation. Fresh global index reads and admitted
nonaliasing remain part of that contract.

The following call at0599 supplies four coordinate words0063 and low
byte0D. Its returned full AX is stored at BP-0026 as the completion
ordinal. Under the zero count, N registrations and retained table/count
conditions, that value is N, not a fixed literal0D. The total registered
count becomes N+1. Registered row identifiers are0..N-1 followed by this
completion identifier. Table capacity and intervening writers still require
admission; the count is not bounded locally by01C6.

### Polling truncates before testing

The loop calls1A7C:0164, then repeatedly calls03D7 with current DS:18F6
as one word. At0716 it stores returned AL, ignoring AH; ALFF alone
repeats the poll. This differs from testing full AXFFFF. For example,
any returned word ending in FF repeats, regardless of its high byte.
After another byte is admitted it calls0195 before interpreting the
saved byte. Native register/frame preservation across that call remains
an obligation, not an inferred ABI guarantee.

Reuse FND-EXE-487's03D7 mode dispatch and FND-EXE-488's022A results:
the latter can return a registered row ordinal, a transformed unmatched
byte, or FFFF. It is not a row-only result channel. This chooser consumes
the low byte from all those alternatives in the same way.

For saved byte7F, it calls1000:1F8C again and retains AL as the next
saved byte. Value50 applies022's forward wrapped index update;48 applies
the backward update. Other values leave DI unchanged at this stage.
This branch then joins the repaint/final-comparison path; it does not
fall through to the other byte transformation described below. These are
literal local classifications, not an observation of particular keys.

For an admitted byte other than7F:

1. The comparison against1B has a zero-distance conditional branch;
   both outcomes reach the same following instruction. There is no local
   cancel return or special index assignment at that test.
2. The saved byte is sign-extended and compared with the full completion
   ordinal. Equality skips direct-index transformation.
3. Otherwise41 is added at byte width and the transformed byte is saved.
   Its signed value must be at least41 and below SI+41 to assign DI to
   that value minus41. Under the retained small positive count, these
   admitted assignments are precisely0..N-1. An admitted assignment also
   replaces the saved byte with the completion ordinal's low byte.
4. A failed direct-index gate leaves DI unchanged but retains the
   transformed byte; it does not restore the original poll byte.

After repainting,08F0 sign-extends the current saved byte and compares
it with full word BP-0026. Equality leaves the event loop; inequality
repeats it. Thus this final test is neither a full poll-word comparison
nor necessarily a comparison of the original returned byte.

For a concrete conditional example, take N=13, retained completion
ordinal000D, and a03D7 return with ALCC. It passes the non-FF and
non-7F tests, initially differs from000D, and adding41 modulo256 makes
the saved byte0D. That byte fails the signed lower direct-index bound41,
so DI is unchanged; the final comparison nevertheless equals000D and
leaves the loop. This is an algebraic consequence of the caller's byte
operations, not proof that any native input produces ALCC. Conversely,
AL1B has no dedicated cancellation path; its later transformation and
final comparison still govern continuation.

### Return and remaining child boundary

Loop completion proceeds through the two-iteration highlight/delay path,
backing restoration,0164 and conditional backing release, with optional
mode restoration. Only after these calls does0AD2 copy held DI into AX
and join the common register-restoring epilogue. A completion byte does
not bypass those children. The separate layout rejection assigns AXFFFF
before that epilogue, as021/022 already record.

The complete root's distinct far-call targets, retained in the instruction
report, are1000:14BB/14E4/14F9/167E/192B/1ACC/1BE0/1F8C/1FEA/
2033/2089/394E/3A4A/3AE4,190F:08E5,1A7C:00B2/0164/0195/
01C6/03D7/040F and1BC3:00DE/00EC. The small-label proof bypasses
some alternative layout/mode paths; this list does not admit every
callee's register/frame effects. Existing formatter/string/input findings
settle their specified local profiles only. Drawing, delay, backing
allocation/release, mode-change and interrupt preservation, table/frame
aliases and reentry remain concrete dependencies before native admission.

## Interpretation

This removes the local registration-ordinal, poll-width and transformed
completion alternatives from Q-SOUND-010. SOUND-022's index proof remains
conditional: these event paths do not establish a valid initial native
index or preserve held state across unknown child effects. They also do
not promise that only a registered completion action can end the loop.
Retain actual group/label/heap/vector lifetime, child preservation and
nonprofile cases. No native chooser was run, no complete reading or status
promotion is claimed, and neither format nor parity is validated by this.

## Alternatives

The supplied low byte0D is a registration key, not necessarily the returned
completion ordinal. A full-word no-result claim would miss the caller's
AL-only rejection. A special cancel action cannot be inferred from the
1B comparison with the same target on both outcomes. Rejected transformed
bytes are not automatically inert for the final completion comparison.

## How to reproduce

Use the verified read-only SETUP-blocks project in
GAME_DIR/analysis/config-block-construction-20261108/.
chooser-continuation-ghidra.log contains the complete19CF:009A root;
chooser-event-children-ghidra.log cross-checks the existing input and
registration interfaces. Reuse EXE-487/488/489's independent contracts.

Generate chooser-event-binding-instructions.txt with resident_listing.py
for19CF:009A..19CF:03AB,19CF:03AE..19CF:0ADD and
1A7C:040F..1A7C:041A. These are the established body ranges, excluding
the intervening gap. Check entry reset, all registration arguments,
0716..071B's byte test,0759..078F's joins and08F0's final sign extension,
plus the cleanup calls before0AD2. Keep original-derived code and reports
only in GAME_DIR.
