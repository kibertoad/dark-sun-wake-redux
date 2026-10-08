---
id: FND-EXE-166
title: Selected-record reader has one decoded direct-call site and consumes a post-setup sixth-slot reload
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F5156
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5187..0x005F518C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AD0..0x00600B25
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600CC0..0x00600D62
tool: Ghidra 12.1.3 PUBLIC reference and instruction reporters; physical whole-file address-occurrence search
environment: null
---

## Observation

The reference-manager query for FND-EXE-056's reader at `0x00600A80`
reports a direct unconditional call at `0x005F5136`. A separate enumeration
of all decoded instructions, testing their rendered operands without using
function boundaries to select them, finds that same call and no additional
instruction containing this exact target token. The positive direct-call
control is `0x00600A50`: both searches recover the independently read call
at `0x005F5187`. Neither search reaches its output cap.

A physical search of all 3802624 shipped bytes for the little-endian
32-bit value `0x00600A80` finds no occurrence. Its positive address-word
control is `0x005F50A0`, independently observed as the callback value stored
by FND-EXE-165; that search finds 748 occurrences without reaching its cap.
Those are raw value occurrences, not 748 callers or proved record writers.
The whole-file search covers bytes outside decoded instructions and defined
data, but it does not recognize relative calls, computed addresses, relocated
runtime values or pointers written only after loading. The decoded search
does not recognize an indirect target represented only by a register or
memory operand, unresolved symbolic rendering, or an undecoded instruction.
The two domains therefore narrow incoming references without proving all
possible callers or indirect uses absent.

At the known direct call, FND-EXE-165's callback has already invoked the
record-setup helper. It then reserves twelve outgoing bytes, reloads the
full word at its frame offset plus 28 into a register and pushes that word.
A subsequent direct store changes its own nested record's offset-four word;
no intervening local call or dereference through the reloaded word precedes
the reader call. On normal reader return the callback removes sixteen
outgoing bytes. The last direct writer of the reader's four argument bytes
is that push, rather than one of the unused reserved slots.

The reader creates a conventional frame, consumes only its first four-byte
argument, restores the old frame register, dereferences the supplied word,
then reads the resulting record's full offset-28 word and returns it. The
first read obtains the current selected value through the supplied address;
the second obtains the current field through that value. Its prologue writes
the saved frame word to its own stack; the body performs no store through
the supplied address or fetched record, and has no other local branch,
call or pointer guard. Disjointness of the supplied storage and that stack
write still needs caller admission. Its ordinary RET
removes no argument bytes. The callback saves and tests the full returned
word as recorded by FND-EXE-165; a zero field is not a calculated boolean.

FND-EXE-053's selector prepares the selected-local address as the sixth
four-byte slot of its eight-slot indirect callback call. Both indirect call
sites in FND-EXE-054 likewise prepare that address as their sixth slot and
remove thirty-two bytes on normal return. A conventional entry to the
callback maps that slot to frame offset plus 28. This proves the direct
slot formation and local consumption, conditional on that target being
selected and the intervening code preserving the slot and address. The
callback's setup runs before its reload, so these local instructions do
not prove the consumed value still equals the selector's original address.
The selected record and its offset-28 field are also reread after setup,
not frozen when the selector first obtains its target.

## Interpretation

The reader's direct input and return contract is bounded, and independent
decoded-reference and physical-value domains narrow its incoming-use leads.
An argument-count guess is unnecessary for the known direct call: one word
is pushed, one word is consumed, and the caller removes its sixteen-byte
outgoing reservation. These checks do not establish selected-local lifetime,
record-field admission, setup preservation or a complete reading of the
launch format. Q-EXE-009 retains those writers and aliases, possible computed
or indirect uses, callback target admission and external effects. No
complete_reading declaration follows from a small body or a single decoded
call site.

## Alternatives

Reading one of the twelve unused outgoing bytes as the reader's argument,
consuming extra original argument words, returning a computed truth value,
or having the reader remove its own argument is ruled out by the direct
instructions. Equating the reader's post-setup input with the selector's
earlier address without a preservation proof remains open. A claim of no
other callers based solely on the analyzer's containing-function list is
not made; the searches exclude the reference kinds named above.

## How to reproduce

Use FND-EXE-011's source length, XXH3 and preferred-base identity. Open its
saved Ghidra snapshot read-only with automatic analysis disabled. Query
ReportReferences for `0x00600A80` and control `0x00600A50` (cap 200 per
target), then enumerate decoded instructions with ReportInstructionText
tokens `00600a80` and `00600a50` (combined cap 256). Compare with the
independently read callback call sites and FND-EXE-056's getter body.
Do not infer excluded reference kinds from an empty result.

Use tools/ghidra/ReportPhysicalBytePattern.ps1 at repository revision
061cec4 with MaximumMatches 16384 to search the complete shipped PE for
the four-byte little-endian encodings of the target word `0x00600A80`
and positive control word `0x005F50A0`. These query words are addresses,
not signatures for code. Verify the control's stored value independently
from FND-EXE-165; keep the occurrence list local.

Read seven instructions from the reader, ninety from `0x005F50A0`,
eighty-five from `0x00600AD0` and seventy-five from `0x00600CC0`.
Restrict claims to the cited spans. Follow push order, the callback's frame
mapping, setup before reload, the direct nested-record store, reader frame
restoration before both dereferences, full-width return and each caller's
cleanup. Use FND-EXE-053, FND-EXE-054 and FND-EXE-056 for their remaining
body contracts. Keep reports local and execute no original program.
