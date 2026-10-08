---
id: FND-EXE-093
title: PATH producer advances before key admission and returns one after untested output-helper completion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0058CD50..0x0058CEDD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D80..0x00601D86
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-015's command lookup queries PATH through `0x0058CD50` only after
four direct candidate tests fail. This callee saves four registers and
reserves 1084 bytes; with ESP then unchanged its object, key and output
arguments are at offsets 1104, 1108 and 1112. It loads object offset twelve,
reads the word through that pointer, adds 44 at 32-bit width and supplies
that address to `0x004F6570`. FND-EXE-094 records this word-read helper's
direct and virtual branches. The caller zero-extends AX and shifts it left
four to obtain its initial source position, discarding any upper result bits.
Object, nested pointer and returned position validity are not locally tested.

It saves the output argument at stack offset 28, reads the output's first
word and the full word twelve bytes before that pointed value, and calls
`0x006D69A0` with output pointer, zero, that preceding word and zero in
four outgoing slots. That helper's complete effects remain unresolved; this
caller does not test its return. Only after normal completion does it reload
the key argument and test its first byte. An empty key returns full zero.
Thus failure does not prove the caller skipped the output-helper call or
left its output unchanged. All stack offsets here are relative to ESP after
the fixed reservation; this body uses outgoing slots without later pushes.

For a nonempty key, each iteration calls `0x004F7330` with current source
position, local buffer at stack offset 32 and limit 1024. FND-EXE-094 records
its copy/terminator contract separately. A zero first buffer byte returns
full zero. Otherwise the caller scans full words until it finds a NUL-byte
candidate, using the standard byte-zero detection arithmetic, and computes
the byte length from the start. It advances source position by that length
plus one at 32-bit width before querying the copied buffer for byte 61,
the equals delimiter, through the strchr thunk in FND-EXE-015.

No equals result continues from the already advanced source position. A
nonnull result is saved and the delimiter byte is replaced with NUL. The
caller scans that key prefix for length, obtains requested key length through
the strlen thunk in FND-EXE-015, and compares the two full words. Unequal
lengths continue the next record. Equal lengths call `0x00601D80` with
requested key first and local prefix second, testing its full result for
zero. FND-EXE-012 identifies this thunk's exact import slot `0x02431940`
as MSVCRT.DLL::_stricmp; the current thunk reading reaches the same slot.
Nonzero comparison likewise continues the next record. This observation
does not substitute host-library behavior for the shipped import.

Only equal lengths and a zero comparison restore byte 61 through the saved
delimiter pointer. The caller then computes the length of the restored
whole local record and calls `0x006D5750` with saved output pointer, buffer
pointer and that length. On normal return it writes full EAX one and restores
its frame. It does not preserve or test the output helper's return, supply
only the value after equals, or locally validate the resulting output field.
Its helper call and aliases remain unresolved. Failure after a nonmatch
need not restore the delimiter in the old local buffer, which the next read
uses again. No finite record-count or source-position wrap guard is present.

## Interpretation

The PATH lookup is a source-position loop with delimiter and two separate
key-match gates, followed by an untested output-helper completion. This
narrows FND-EXE-015's environment-output boundary without proving a PATH
value, record-list extent, output lifetime or successful bare-command
resolution. Q-EXE-009 retains object and memory-map producers, output helper
contracts, imported comparison effects, complete input bounds and aliases.
Returning one is evidence of this caller's admitted normal route, not an
independent guarantee that the output pointer changed to a valid string.

## Alternatives

Advancing source only after a matching key, treating a prefix-length match
alone as success, passing only the text after equals to the final helper,
returning the final helper's status or delaying every output-helper call
until after key validation is ruled out locally. A limit of 1024 on one
copy does not prove a 1024-byte total list or a finite number of records.
No local empty-key return establishes unchanged output.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-015's PATH call site.
Read eighty instructions at `0058CD50` and sixty-five at `0058CE66`,
restricting claims through `0058CEDC` and excluding the next function.
Read two at `00601D80`, retaining only the first thunk; use FND-EXE-012
for its exact import mapping and FND-EXE-015 for strlen/strchr slots.
Trace ESP from entry through the fixed reservation, every outgoing slot,
low-word source production, delimiter store, source-position update and
full return gates. Use FND-EXE-094 for the called record-read boundaries.
Keep reports local, assume neither valid object storage nor helper success,
and do not execute the interpreter or a shell harness.
