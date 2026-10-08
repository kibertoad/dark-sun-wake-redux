---
id: FND-EXE-170
title: Shared-record reader decodes thirty-two atom-name bytes after a nonzero import result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600530..0x006005A9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602530..0x00602536
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035903A..0x00359046
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601F80..0x00601F86
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006005A9..0x006005B3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00358518..0x0035851C
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

The bounded reader at `0x00600530` masks its incoming return-register word
to the low sixteen bits and passes that value, a local buffer address and 66
to `0x00602530`. The thunk jumps through slot `0x02431820`; bounded PE
import reading identifies GetAtomNameA from KERNEL32.dll. The cited shipped
name includes NUL. Independent malloc/free slots match FND-EXE-024 as controls.
This is a register input, not an inferred original stack argument.

Before the call the body clears its prospective decoded word. After normal
return it tests only whether the full returned word is zero. Zero selects
a call to `0x00601F80` with locally written outgoing values.
The body does not compare the returned length against 32 before decoding.

For nonzero return it initializes a byte index to 31 and a 32-bit mask to one.
At each index it compares the local byte with 65. Equality ORs the current
mask into the decoded word; inequality leaves that word unchanged. Both paths
double the mask at 32-bit width, decrement the index and continue while the
index is nonnegative. Thus indices 31 through zero are examined exactly once;
index 31 supplies the low bit, and index zero the high bit. Other byte values
are not rejected locally. The local buffer is not directly initialized before
the import call; unread import effects and actual caller inputs remain conditions.

After decoding it reads the first 32-bit word through the decoded address
and compares it with 60, without a local decoded-address-null check. Equality
returns that decoded address after normal frame restoration. Inequality calls
`0x00601F80` with a separately written set of outgoing values. The import boundary and conditional continuations are described below. No claim
of rejection, process termination or malformed-input reachability is made.

The failure target at `0x00601F80` is a six-byte indirect jump through
`0x02431918`. Physical PE import-table reading identifies that slot as
`msvcrt.dll!_assert`, from import lookup value `0x020327E4`, descriptor 5,
slot index 5. The cited four-byte shipped slot is at `0x00358518`.
Independent `malloc` and `free` slot controls match FND-EXE-024.
This identifies the import, not the implementation loaded at runtime.

The mismatch path prepares four outgoing words: the current return-register
value, line number 234, a filename pointer and an expression pointer, then
calls the thunk. The zero-result path prepares the current index-register
value, line number 228, the same filename pointer and another expression
pointer. SRC-MS-CRT-ASSERT supplies a published three-parameter contract;
the fourth prepared word is not evidence of a fourth consumed parameter.

If the mismatch call returns normally, the next local instruction starts the
zero-result preparation. There is no local argument removal between these
calls. If the second call returns normally, the following seven-byte padding
instruction leaves its destination register unchanged, then execution reaches the
initializer entry described by FND-EXE-043 without a new local call or reader
frame restoration. Neither conditional continuation proves that the loaded
import returns, preserves registers, or supplies a usable resulting frame.

## Interpretation

This supplies a bounded reader for the existing-record lead in the shared-target
initializer. The reader's acceptance test is a decoded record word equal to 60,
not a local validation of all 32 encoded bytes or a returned-length check.
Q-EXE-009 retains initializer branches, encoded-name production, admitted inputs,
import writes, record validity and failure-helper contracts. It does not establish
all shared-target producers or a complete storage lifetime.

The reader range in FND-EXE-042 ended inside its final five-byte call.
This replacement includes the whole call and records the previously unread
failure boundary. Complete input admission and the loaded import's effects
remain unresolved; no complete_reading declaration is made.

## Alternatives

Decoding only the returned number of bytes, treating every non-65 byte as an
error, reversing the bit/index correspondence, or guarding a zero decoded address
before dereference are ruled out by the bounded instructions. A full-length
name supplied by all callers is possible but requires producer evidence; absence
of a local guard alone does not prove a reachable short-name defect.

An unconditional termination interpretation cannot be justified by the
analyzer's no-return label or the imported name. Conversely, the published
contract's possible continuation is not proof of a native return here.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600530` and
read forty instructions from its entry and eight from `0x0060059F`. Restrict
claims to the cited body, excluding subsequent functions. Read one instruction
from `0x00602530` and independently map its slot through bounded PE sections,
descriptors and terminated lookup thunks with malloc/free controls. Follow the
register mask, buffer/count slots, zero-result branch, index and mask updates,
both byte-test arms, decoded dereference, equality return and failure calls.
Keep loaded-import effects and admitted caller inputs conditional. Keep rich
reports local and execute no interpreter or game.

Additionally read fourteen instructions from `0x0060057F` and one from
`0x00601F80`, with no analysis or project writes. Use
`node tools/evidence/report.mjs x86-imports <local-config.json>` with
sourceKind `pe32`, the source and XXH3 from FND-EXE-011, and controls
`0x024319D0` / `msvcrt.dll` / `malloc` and
`0x024319A0` / `msvcrt.dll` / `free`. Inspect target `0x02431918`.
Executable-reader 2.4.0 checks physical PE sections, descriptors and lookup
thunks. Keep the reports outside Git and do not execute the original.