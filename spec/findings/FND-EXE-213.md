---
id: FND-EXE-213
title: Concrete count-one metadata takes a negative-pair lookup and saved-six return on the nonmatching-signature path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F53A7..0x005F53CB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F53EC..0x005F5415
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5446..0x005F546D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F54B8..0x005F54EE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F550F..0x005F5536
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EDC73..0x002EDC75
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EDC78..0x002EDC79
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

Extend FND-EXE-212's field-helper count-one composition with unchanged
metadata address B = `0x006EE868`. The shipped bytes at B +11 and
B +12 supply terminating signed payloads 127 and 125; B +16 supplies
a terminating unsigned payload zero. Their file offsets are the cited
data ranges. No intervening source extent or later stream is inferred.

Require the same valid disjoint storage, selected-record/count preservation,
segment admission and ordinary callee behavior as FND-EXE-212. Additionally
require the second original argument's bit eight to be clear and the third/
fourth argument pair to differ from FND-EXE-165's exact signature. The
selector in FND-EXE-053 prepares second arguments two or six, both with
bit eight clear and bit one set; actual callback selection remains conditional.

The remaining branch initializes two separate local flag bytes to zero.
Its clear-bit route checks the signature, and the nonmatching pair enters
the route initializing a separate saved object word to zero before signed
pair decoding. It does not load that word from the fifth-derived base on
this route; that load belongs to the matching-signature arm.

The initial signed input is FND-EXE-212's derived B +11. The first
FND-EXE-063 decoder writes all ones, signed minus one, and returns B +12.
The caller saves this cursor and supplies it to the second decoder. That
decoder writes signed minus three and returns B +13. The caller then
reloads the first decoded word into its return register without storing
the second cursor in the saved cursor local. Thus the saved cursor remains
B +12, not B +13. Neither output is a helper success status.

The first word is nonzero and signed negative, selecting the negative
continuation. The saved object word is zero, so the object-matching helper
is bypassed. The caller complements the first word at 32-bit width and
adds metadata output offset twelve, which FND-EXE-210 gives as B +16.
Complementing all ones yields zero, so the unsigned decoder input is B +16.
FND-EXE-059 writes full zero and returns B +17 on that terminating byte.

The caller compares that output local with zero, then jumps to the earlier
conditional branch without changing flags. The branch therefore consumes
the output comparison, not the decoder return's low byte. Zero does not
continue matching: it reaches the second flag's byte store of one. The next
flag test selects classification three and saves the signed first decoded
word in its associated local before joining FND-EXE-196's classification path.

Under the selector's bit-one-set argument and the unchanged nonmatching
signature, classification three skips the five signature-only saved-state
stores, saves return status six and enters nested-record cleanup. Normal
cleanup with saved-status/frame preservation returns six, as recorded in
FND-EXE-196. This is a conditional complete local path to that return,
not proof of cleanup completion or an actual native callback invocation.

## Interpretation

This closes the remaining matching-prefix outcome for one concrete count-one,
nonmatching-signature composition. It follows both signed returns separately,
the unsigned output's comparison flags and the final saved status. Q-EXE-009
retains actual selection and input preservation, matching-signature and other
flag routes, continuing pair loops, object helpers, aliases and cleanup effects.
No complete-reading declaration or format-status promotion follows.

## Alternatives

Saving the second cursor, testing its low byte instead of the output local,
treating the unsigned zero as permission to continue, or returning the cleanup
result would change this path. A nonmatching signature selects zero for the
saved object word; it does not admit a read through the fifth-derived base.

## How to reproduce

Hash-check shipped XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Map B through the pinned PE loader and inspect only relative bytes eleven,
twelve and sixteen for the source facts above. In the saved project read-only,
analysis disabled, run ReportInstructionWindow at `0x005F53A7`, count
72, and `0x005F54E4`, count 35, restricting claims to cited spans.
Compose FND-EXE-053/059/063/165/196/210/212. Track each return-register
overwrite, saved cursor, initialized flag/object word, signed guard, complement,
output comparison and flag-preserving jump, classification and saved six after
cleanup. Keep rich reports local and execute no original program.
