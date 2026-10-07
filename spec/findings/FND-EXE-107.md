---
id: FND-EXE-107
title: Callback wrappers combine a zero-extended word with an object field and tail-transfer to insertion or pair removal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A2260..0x005A2287
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A2290..0x005A22B1
tool: Ghidra 12.1.3 PUBLIC bounded callback-wrapper reading
environment: null
---

## Observation

FND-EXE-105 records insertion at `0x004F2490` and target-argument pair
removal at `0x004F2600`. The wrapper `0x005A2260` reads only the
low word of its second argument at entry ESP plus eight, zero-extends it,
and shifts that full result left two. It reads the full first argument O
at entry ESP plus four and the full third argument D at plus twelve.
It ORs the shifted word with the full field at O plus `0x010C` to form
V; this is a bitwise combination, not addition or replacement of a proved
bit partition. No mask clears any bits of that object field.

The wrapper writes retained D as a full word at ESP plus eight, writes
full constant `0x005A4BB0` at ESP plus four, then writes V at ESP plus
twelve. It makes an unconditional tail transfer to `0x004F2490` without
changing ESP or replacing the entry return address. Thus the insertion
helper receives that fixed callback target, D's bits interpreted by its
single-precision second-argument load, and full V as callback argument.
No numeric conversion of D occurs in this wrapper. The return is the
insertion helper's return; the wrapper normalizes no success word.

The wrapper `0x005A2290` also zero-extends the low word of its second
argument, reads first argument O and full O plus `0x010C`, shifts the
zero-extended word left two, and ORs with that field. It replaces its
second full argument with V and its first with `0x005A4BB0`, then
tail-transfers to `0x004F2600` with the entry return address intact.
Consequently it requests the pair predicate from FND-EXE-105, including
that helper's repeated-match removal, rather than target-only removal.
Neither wrapper validates O, indexes a local table, calls the callback,
or establishes bounds or bit separation for the field it combines.

## Interpretation

These are concrete producers of the insertion target and callback argument,
and a corresponding removal request. Matching V depends on the object's
field and caller-supplied word at each invocation; a common construction
does not prove that separately reached calls produce equal V. Input word
producers, object-field writers and lifetime, float admission, callback
body and caller return tests remain Q-EXE-009 in FMT-EXE-006. Nothing
here establishes actual PATH behavior or native callback effects.

## Alternatives

- Upper bits of the incoming second argument do not survive its word load.
  Upper bits of the object field do survive the unmasked full OR.
- The third insertion input is relocated as raw full bits; no integer-to-float
  conversion is present in this wrapper.
- Removal tests the fixed target and constructed argument together, not only
  the target, and does not change the helper's repeated-match contract.
- A tail transfer preserves the caller's return address; these wrappers do
  not introduce a second return frame.

## How to reproduce

Verify FND-EXE-011's executable length/hash and FND-EXE-099's physical
controls. In the saved Ghidra program with -noanalysis, run
ReportReferences.java for `0x004F2490`, `0x004F2600` and
`0x004F2680`, cap 200 references each. The tail transfers at
`0x005A2287` and `0x005A22B1` are leads only, not proof of complete
caller coverage. ReportInstructionWindow.java at `0x005A2260` limit
35 covers both wrappers; restrict observations to the two declared ranges,
excluding the following procedure. Track entry ESP unchanged, word versus
full loads, old third argument retention before overwrite, full OR and
both jump destinations. FND-EXE-105 supplies independently read helper
argument positions and predicates. Keep reports in GAME_DIR/analysis/exe-batches;
commit no original listings or bytes and execute neither interpreter nor game.