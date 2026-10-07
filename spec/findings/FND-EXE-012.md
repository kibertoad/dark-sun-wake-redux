---
id: FND-EXE-012
title: Compiled batch-label search calls cleanup that restores saved shell fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0059E6C0..0x0059E886
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0059EFB0..0x0059F162
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0059F490..0x0059F5B5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006017F0..0x0060181D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D80..0x00601D86
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00356800..0x0035680C
tool: Ghidra 12.1.3 PUBLIC, bundled bounded reporters
environment: null
---

## Observation

The direct callee from FND-EXE-011, `0x0059E6C0`, saves four registers and
requests `0x102C` stack bytes through `0x006017F0`. Reading that probe shows
that it lowers the caller's stack pointer by the requested amount before
returning. The object and target incoming arguments are consequently at
post-probe stack offsets `0x1040` and `0x1044`; the decompiler's apparent
extra arguments are not used as a signature.

The callee passes the object's field at `0x1C` to an opening helper and tests
its low-byte result. Its read loop passes a 16-bit handle from object offset
`0x04`, a one-byte buffer and a 16-bit requested count of 1 to another helper.
It gathers bytes greater than 31, recognizes label lines after a trim-helper call,
skips leading classified whitespace and equals signs after the initial colon,
and terminates a label at classified whitespace, equals or NUL. It compares
the resulting label and the supplied target through `0x00601D80`, testing
the full 32-bit result for zero at `0x0059E82D`. This thunk jumps through
import slot `0x02431940`, identified in the PE import mapping as
`MSVCRT.DLL::_stricmp`; the import was resolved by its slot, not a guessed name.

The match continuation sets object offset `0x08` to zero, passes that field
to a seeking helper with origin argument 1, calls the closing helper and
returns 1. The exhausted-search path calls the closing helper before the
failure continuation. The failed-open path enters failure without that close.
At `0x0059E77B`, a nonnull object makes an indirect call through the second
32-bit entry of the object's pointer table, then returns 0. These describe
the routine's decisions on helper results, not proof that host file operations
succeed or that every helper preserves the assumed state.

On the inspected object-initialization path in `0x0059EFB0`, the object's
pointer-table field receives `0x00759600`. That table maps to shipped offset
`0x00356800` and contains targets `0x0059F700`, `0x0059F490` and `0x0059E890`.
Thus its second entry resolves the failure-path indirect call to
`0x0059F490`, conditional on this initialized object reaching the search.
The same initialization path copies the host's 32-bit field at `0x28` into
object offset `0x14`, its byte at `0x2C` into object offset `0x0C`, and its
pointer into object offset `0x10`.

The normal continuation of `0x0059F490` reads that saved host pointer,
saved 32-bit value and saved byte. At `0x0059F55E` it writes the saved
32-bit value into the host's field at `0x28`; at `0x0059F561` it writes
the saved byte into host offset `0x2C`. Earlier string-release helpers,
exceptional cleanup and later deallocation were not fully read. Restoration
is therefore a conditional normal-path observation, not an all-path guarantee.

## Interpretation

The compiled search has a failure cleanup call with concrete field-restoration
effects on its normal continuation. This supports part of the previous-batch
and echo-restoration reading suggested by SRC-DOSBOX-GOG-0742. It goes beyond
observing a false return while keeping object provenance, external operations
and exceptional paths explicit. No complete reading of batch chaining or
missing-label shell output is claimed.

## Alternatives

A failure path that merely returns false without any cleanup call is ruled out
for a nonnull object reaching `0x0059E77B`. Unconditional restoration on every
host failure is not established: the external helpers, initialization inputs
and exceptional paths still need their own readings. A truncated comparison
test is ruled out here by the full-width result test; FND-EXE-011 separately
records the search caller's low-byte test.

## How to reproduce

Use the file identity and PE import procedure of FND-EXE-011. Recover entries
`0059E6C0`, `006017F0` and `0059F490` only if missing, without rerunning the
original. With the pinned reporters, summarize each and inspect instruction
windows at `0x0059E6C0` (160 instructions), `0x006017F0` (27),
`0x0059EFB0` (110), and `0x0059F490` (95). Stop interpretation at the
reported function bodies. Verify the stack probe before assigning incoming
arguments, the two distinct failure paths, full-width comparison test and
indirect target. Query `ReportSymbolReferences.java` with fragment `stricmp`
and `ReportReferences.java` for `0x02431940` to check the actual imported slot.

Read 12 bytes at `0x00759600` with `ReportDataBytes.java`; encode its three
observed targets as little-endian words and search the shipped file with
`ReportPhysicalBytePattern.ps1`, maximum 16 matches. The recorded match was
`0x00356800`. Trace the explicit pointer-table store and saved-field stores
in the initializer, then the restoration stores in the cleanup target.
Do not treat the initializer's no-return/exception annotations, inferred types
or a bounded caller list as proof of complete object provenance. Export only
starts and body sizes with `ExportFunctionInventory.java`; keep every richer
report outside Git. This reading used no original-program execution.
