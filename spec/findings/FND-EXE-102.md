---
id: FND-EXE-102
title: Fallback helper publishes a depth-indexed record while conditional callback restoration differs from normal cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417CF0..0x00417DF3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417C40..0x00417CE1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401850..0x0040186D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004119F0..0x00411A24
tool: Ghidra 12.1.3 PUBLIC bounded actual call and stored-target reading
environment: null
---

## Observation

FND-EXE-099 and FND-EXE-101 ground caller admission to `0x00417CF0`,
including original address, computed indexed-word offset and third value five
on the state-three route. This helper pushes EBX and reserves fifty-six more
stack bytes. It saves full words from `0x01BA29E0`, `0x01BA29E4`,
`0x01BA29E8`, `0x01BA29EC`, `0x01BA29F0` and `0x01BA29F4`
in local slots sixteen through thirty-six, in their corresponding order.
The saved values are not a claim that every native machine field is covered.

It reads full depth N from `0x01B7BB48`, saves previous callback word
from `0x0075B0D0` in EBX, and publishes exact target `0x00417C40`
to that callback slot. It increments and publishes N modulo thirty-two bits.
Using retained old N shifted left four, it fills a sixteen-byte record at
`0x01B7BB4C + 16*N`. First it zero-extends the current sixteen-bit word
at `0x0075B102` and writes it as a full first record word. It publishes its
full first argument to `0x0075B6C4`, writes current `0x0075B200` at
record offset four, its full second argument at offset eight, and current
`0x0075B144` at offset twelve. No depth bound or record-extent guard
precedes those stores. Address/shift arithmetic is thirty-two-bit.

It reads its full third argument, writes full three to `0x0075B144`, then
calls `0x004119F0` with full fourteen first and that third argument second.
The helper's inputs are current ESP plus sixty-four, sixty-eight and
seventy-two: the saved EBX and local reservation together account for sixty
bytes before the caller's entry argument slots. Publication of callback, depth,
record and flag word precedes the call and is not undone by an earlier return
value test.

On normal return from that call it calls `0x00401850`. Only after normal
return from both does it restore the previous callback at `0x0075B0D0`,
decrement the freshly stored full depth word at `0x01B7BB48`, and restore
the six saved global words in order. The decrement is not an assignment of
saved N and does not clear the selected record. There is no direct restoration
of `0x0075B6C4` or unconditional restoration of `0x0075B144` in this
ordinary tail. After the sixth restoration EAX is the saved sixth word from
`0x01BA29F4`; restoring ESP and EBX does not replace it with the wait-loop
result. This ordinary return is not a success boolean. Exceptional completion
and callee aliases remain unresolved.

The wrapper `0x004119F0` reads its two full arguments, freshly reads
`0x0075B200`, and publishes the second input to `0x0075B18C`. It
compares the first input unsigned with eight and selects full two if below
eight, otherwise six. It calls `0x00411A30` with the first input, selected
two/six and saved current `0x0075B200` as its three outgoing arguments,
then returns after removing its local reservation. Thus this helper's input
fourteen selects six and its original third argument reaches the published
word. The wrapper does not undo that store before returning; effects of the
larger callee are not established by this direct wrapper reading.

The wait helper `0x00401850` reserves twelve bytes and repeatedly calls
through full pointer slot `0x0075B050`. Each normally returned full EAX
zero repeats; any nonzero full value exits with that EAX unchanged. It has no
local iteration bound or pointer-null guard. It does not directly read the
newly published callback slot `0x0075B0D0`. A relationship between those
slots needs the selected target and its producers; it cannot be inferred from
the caller's order or similar addresses.

The stored callback `0x00417C40` reserves twelve bytes, reads full
`0x0075B0E8`, adds it to full accumulator `0x006F00A4`, writes full one
to `0x0075B0E8`, and calls `0x00463410`. On normal return it retains
that full result, freshly reloads `0x0075B0E8`, and adds that value to the
same accumulator. Both additions use thirty-two-bit wrap. The freshly read
second addend need not remain one; the call lies between the write and reload.

A signed negative full callee result supplies fixed pointer `0x0071F3C4`
to transfer callee `0x0058F890` in FND-EXE-099. Its physical unexpected
normal continuation supplies `0x0071F3F2` to that same transfer callee.
A positive full result returns it unchanged without examining the record.
A zero result instead freshly reads depth. Zero depth supplies the latter
transfer pointer directly. Nonzero depth selects address
`0x01B7BB3C + 16*depth`, without a local bound check.

That selected record address is the previous helper's record only if current
depth still equals old N plus one with the required address validity. It adds
fresh `0x01D4A380` to selected record field eight and tests mask one in
the addressed byte. An absent mask returns full zero. A present mask requires
record field zero to equal the zero-extended current word at `0x0075B102`,
then record field four to equal fresh `0x0075B200`. Either mismatch returns
full zero. Only when all checks pass does it load record field twelve,
publish that full word to `0x0075B144`, and return all ones. This is signed
negative as a result, but also nonzero to the separate wait helper if a future
reading proves that this callback's result reaches it. Callback selection and
native invocation are not established here.

## Interpretation

The fallback helper stages a record and temporary callback before its indirect
continuation. Ordinary cleanup restores selected saved words and decrements
fresh depth, whereas the callback restores the prior flag word only after its
record and current-state checks pass. Neither path is a universal rollback.
The wait-loop slot differs from the callback-publication slot, and unresolved
target effects determine whether the helper returns at all. Q-EXE-009 in
FMT-EXE-006 retains those links, initialization, storage lifetime and aliases.

## Alternatives

- The third incoming value is not the address published at `0x0075B6C4`:
  the first value goes there; the third is passed through the fourteen/six
  wrapper and published at `0x0075B18C`.
- Normal depth cleanup decrements current storage rather than restoring old N.
  A callback's record selection therefore needs its own current-depth proof.
- Calling onward after installing a callback does not prove the wait loop
  invokes it: the loop reads `0x0075B050`, not `0x0075B0D0`.
- The accumulator's post-call addend is a fresh read, not necessarily one.
- The callback's positive return bypasses its record checks, zero may fail
  those checks, and all ones follows flag restoration. These meanings must
  not be collapsed into one generic error/success convention.
- Restoring the six saved globals and previous callback does not itself
  restore every published word or normalize the helper's EAX return.

## How to reproduce

Recheck FND-EXE-011's identity and FND-EXE-099's six physical table slots.
Use the saved Ghidra program with -noanalysis. Read entry `0x00417CF0`
with limit 110, exact stored target `0x00417C40` with limit 65, wait entry
`0x00401850` with limit 65, and wrapper `0x004119F0` with limit 45.
Restrict each claim to its location above; windows entering later functions
are not part of this finding. Target `0x00417C40` is grounded by its full
stored-value writer at `0x00417D3A`, not inferred from proximity.

Track saved EBX plus the local reservation to map all three full arguments.
Follow record writes, callback and depth publication, both calls, fresh depth
decrement and ordered saved-word restoration. Compare old-index record
address with the callback's fresh-depth address conditionally. Follow signed
callee result tests, fresh accumulator addend and the separate zero/nonzero
wait test. FND-EXE-099 supplies only the shared transfer route's bounded
contract; this finding does not read all of `0x00411A30`, `0x00463410`
or the target behind `0x0075B050`. Keep reports and identity controls in
GAME_DIR/analysis/exe-batches, with no native or emulated execution and no
original listings or bytes in Git.