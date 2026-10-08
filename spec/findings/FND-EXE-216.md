---
id: FND-EXE-216
title: Ordinary pair decoders have direct write intervals separate from the saved loop counter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F50AC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5156..0x005F51CC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5340..0x005F536A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4D30..0x005F4D66
tool: Ghidra 12.1.3 PUBLIC, engine 13.6.0 bounded instruction reporter
environment: null
---

## Observation

Let F be the ordinary callback's frame base after its initial frame setup.
Its three saved registers and 172-byte reservation leave the stack pointer
at F minus 184. FND-EXE-165's callback removes setup's outgoing space on normal
return and likewise removes its offset-28 reader's outgoing space. FND-EXE-196's metadata and modifier calls use register inputs; the
count getter's twelve reserved bytes and one pushed word are removed by
its sixteen-byte adjustment. Neither pair call reserves or pushes arguments.
Thus each pair decoder is entered with its return word at F minus 188,
and its frame base is F minus 192, conditional on the preceding calls'
ordinary stack restoration.

The callback saves the parsing cursor in the full word F minus 152 and
the fetched count in the full word F minus 160. Its first pair call supplies
F minus 112 as the output address; the second supplies F minus 116.
These four-byte outputs occupy F minus 112 through F minus 109 and
F minus 116 through F minus 113. Neither overlaps the saved cursor or
counter under equal DS and SS bases and valid, nonwrapping frame storage.

FND-EXE-059's decoder retains the output address in a saved register.
Its direct writes are its stack saves, its local cursor word, and one
four-byte output store on termination. Including the caller's return-word
push, its stack-write interval is F minus 208 through F minus 185.
That interval is separate from both outputs, the counter and saved cursor.
There are no calls, imported operations, indirect writes through the input
cursor, or global stores in this decoder body. The first of its two final
pops into the same register discards the scratch cursor word; the second
restores that register's original saved value. It restores the other saved
registers and frame before returning, balancing all its own stack space.

After the first call, the callback writes its full return to the saved
cursor and invokes the second decoder with that same return still in the
input register. After the second, it decrements the saved counter and writes
the second full return to the cursor. This intervening cursor store does
not alter flags: the backward conditional jump tests the decrement's zero
result, rather than either decoder's output or flags. Each re-entry reloads
the saved cursor and prepares the first output address anew.

## Interpretation

The two direct decoder output stores cannot themselves overwrite this
caller's saved counter or cursor under the stated segment/frame conditions.
Once a positive count is admitted, each normally terminating pair decrements
it exactly once; no additional callee-preservation assumption is needed for
these leaf decoders' direct counter writes. FND-EXE-212's finite concrete
pairs can use these explicit intervals in place of an unexplained generic
leaf-preservation assumption.

This does not admit the count before the loop, equal segment bases, readable
source extent, or unchanged source. Stack saves and output stores may still
alias input bytes unless source storage is separately admitted. A decoder
can fail or continue indefinitely on other inputs; other threads or external
writes are not excluded by a leaf body. Q-EXE-009 retains those dependencies,
preceding setup/helper effects and record selection. No complete_reading
declaration or format-status promotion follows.

## Alternatives

The reading that the supplied direct outputs overlap the saved counter is
ruled out under equal segment bases by their complete four-byte intervals.
The reading that either decoder's returned flags determine loop repetition
is ruled out by the later decrement and flag-preserving store. Assuming
that disjoint outputs alone prove unchanged input, valid stack storage or
termination remains unsupported.

## How to reproduce

Use FND-EXE-011's hash-verified shipped PE and the saved project read-only,
analysis disabled. Run ReportInstructionWindow at 0x005F50A0 count ten,
0x005F5156 count forty, 0x005F5340 count thirty, and 0x005F4D30 count
thirty; also read 0x005F50B8 count eighteen as the setup-adjustment control.
Restrict claims to the cited half-open spans and FND-EXE-165's control span. Compose FND-EXE-165,
FND-EXE-196 and FND-EXE-059 for preceding stack adjustments and input
registers. Count every call push, frame save, saved register and scratch
word; verify both pops into the same register, the terminal return, output
widths, frame-relative destinations and the loop's last flag writer. Keep
segment equality, source extent and aliases explicit. Keep rich reports in
the licensed-source store and execute no original program.
