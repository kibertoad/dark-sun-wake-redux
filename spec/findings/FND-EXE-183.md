---
id: FND-EXE-183
title: Shipped provider pointer producer packs a bounded word-counter return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004C11A0..0x004C11D9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00568EA2..0x00568EF8
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

At `0x00568EA2`, the caller writes dword one to its outgoing stack slot,
then calls `0x004C11A0` at `0x00568EA9`. The helper saves EBX and reserves
eight stack bytes. It zero-extends the word at `0x0070D962` and the word
at its current `ESP + 0x10`: after its push and reservation, this latter
slot is the first caller argument. It computes their sum at dword width
and compares it unsigned with `0xCFFF`.

When the sum is at most `0xCFFF`, the helper writes the sum's low word
back to `0x0070D962`, releases its eight reserved bytes, returns the old
zero-extended counter in EAX, restores EBX and returns near at
`0x004C11CC`, without extra argument cleanup. On a greater sum, it puts
`0x00725CA0` in its outgoing stack slot and calls `0x0058F890` at
`0x004C11D4`. The continuation and effects of that helper are not read
here; this does not establish that failure returns, leaves all state
unchanged, or terminates execution.

The fingerprinted source traversal reaches all 57 bytes of
`0x004C11A0..0x004C11D9`, lists the success return and the failure call,
and remains incomplete: its assumed post-call continuation reaches the
exclusive region end. The bound intentionally excludes that continuation.

After the counter helper returns, the caller shifts EAX left 16 and
subtracts `0xFFF0` at dword width. It stores the result at `0x0240D640`
at `0x00568EC8`, the dword that FND-EXE-182's service arm reads.
For a success return C, the stored dword is
`((C << 16) - 0xFFF0) mod 2^32`; its low word is `0x0010` and its upper
word is `(C - 1) mod 2^16`. This includes wraparound when C is zero.
No initial or admitted value of C is established by this bounded reading.

Before calling `0x00417020` at `0x00568EF3`, the caller places five
dword arguments in successive outgoing stack slots: the dword from
its `ESP + 0x64` plus eight; `0x005685A0`; sixteen; the stored pointer's
upper word shifted left four plus its low word masked with `0xFFF0`;
and `0x007379F8`. The addition is at dword width. This names the explicit
local argument writers, not their object, callback, allocation-unit or
text semantics. The incoming stack field and callee contracts remain unread.

## Interpretation

This resolves one concrete producer of FND-EXE-182's stored pointer and
separates its counter update from subsequent callback setup. The one-unit
request cannot yet be equated with a byte count or an allocation extent.
The admitted counter range, every writer and lifetime, caller/root admission,
setup's writes and failure effects, guest-field identities and preservation
remain Q-EXE-001 and Q-EXE-010 obligations. No complete reading follows.

## Alternatives

A far-pointer-looking constant alone would not explain the stored value.
Here the bounded arithmetic follows the actual callee return into both
pointer words and the later outgoing argument. Conversely, a success-path
return does not establish the greater-sum path's behavior. Treating the
counter as an allocator or assuming its unit from an external source name
would leave its initial state and storage extent unproved.

## How to reproduce

Use the shipped interpreter with XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Run the committed wrapper's
`x86-bounds` command with sourceKind `pe32`, entry file offset `0x000C05A0`
(787872), and one named region start 787872, exclusive end `0x000C05D9`
(787929), entries `[787872]`. Describe it as the bounded success arm and
failure call with failure continuation excluded. Omit segment/ip, seeds
and callee summaries. The reader derives the PE mapping from section
virtual start `0x00401000` and raw offset `0x400`; retain its incomplete
result and continuation assumption rather than enlarging the claim.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x004C11A0`, count 35, and at `0x00568E81`,
count 48. Restrict this observation to the two declared intervals; exclude
neighboring routines and the later initialization loop. Run
ReportInstructionContext at `0x00568EC8` and ReportReferences at
`0x0240D640`. Its positive writer and FND-EXE-182 consumer are leads only;
no empty reference result or analyzer ownership establishes completeness.
Keep rich reports in the local licensed-source store, outside Git.
