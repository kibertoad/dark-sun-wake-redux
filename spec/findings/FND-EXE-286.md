---
id: FND-EXE-286
title: Corrected cleanup argument writer replaces the pushed AX word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0040..44D0:0053
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0075..44B6:00B5
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-284's two preceding far calls resolve through declared MZ
relocations to 44D0:0040 and 44B6:0075. The first body writes zero
to incoming DS-relative bytes 0x33D8 and 0x33D9, makes one far call
whose contract is unread here, sets AX to zero and far-returns. It does
not locally save or restore DS. The two byte publications precede the
nested call; a normal zero return does not prove that call succeeded.

The second body pushes incoming DS and AX and loads DS from an immediate
segment operand. It tests DS-relative byte 0x33D4 for exact equality with one.
Other values write 0x0802 to DS-relative word 0x33BE and set AX to
0xFFFF, then reach the common restoration. Equality clears byte 0x33D4,
saves flags and disables maskable interrupts. It pushes CS-relative
words 0x000E and 0x000C, then AX. It temporarily establishes BP from
SP and writes word nine at SS-relative BP plus two, replacing the
previously pushed AX word. It restores BP and
makes a far call whose contract is unread here.

On ordinary continuation it removes six outgoing argument bytes,
restores flags and reaches the same suffix. Both paths pop saved AX
and DS and far-return. Thus the locally assigned all-ones AX on the
rejected gate is discarded, and the admitted path also returns saved
incoming AX rather than the nested callee's result. The byte cleared
before that nested call is not locally restored. Stack corruption,
nonlocal returns and interrupt effects are outside this ordinary-path
reading.

## Interpretation

On an ordinary return from the first callee, FND-EXE-284 reaches the
second with AX zero. The second's ordinary suffix preserves that zero
on both gate paths. If the outer caller bypasses its later return-wrapper
call, its ordinary AX can therefore remain zero despite the second's
local rejection and error-word publication. Neither zero proves completed
cleanup. If it reaches the later call, that later callee's result can
replace AX instead.

The first callee's DS is not locally preserved across its nested call.
The second restores its own incoming DS, not necessarily the outer
caller's original segment. Q-EXE-001 and Q-EXE-010 retain nested target
contracts, relocated data-segment admission, byte/word writers, stack
argument contracts, aliases and interrupt effects. No complete reading,
valid saved pair or successful release is established.

## Alternatives

This supersedes FND-EXE-285: its temporary-BP interpretation placed the
word-nine store in the second outgoing argument. The extra saved BP makes
BP plus two refer to the first argument, the pushed AX word. The remaining
callee-body observations are retained here; the CS-relative pointer words
are not locally overwritten by that store.

With temporary BP at the saved-BP word, the stack words at BP plus zero,
two, four and six are respectively saved BP, pushed AX, the word from
CS-relative 0x000C and the word from CS-relative 0x000E. Restoring BP
removes only the saved-BP word before the far call. The corrected store
therefore supplies nine as the first argument and preserves both following
pointer words. This is the explicit frame accounting missing from the old
interpretation.

Treating the second's local error AX as its return ignores the final pop.
Treating its first outgoing word as the pushed AX value ignores the
last stack writer. Treating its saved DS as restoring
the outer caller's initial DS assumes the first callee preserved it.
Cleared byte state and zero returns do not prove nested operation success.

## How to reproduce

At revision 24533e1, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. Resolve
operand sites 0x00039897 with targetOffset 0x0040 and 0x0003989C
with targetOffset 0x0075 using the committed operand reporter and load
segment 0x1000. With locked Capstone 5.0.7 in sixteen-bit x86 mode,
decode shipped half-open ranges 0x00039F40..0x00039F53 and
0x00039DD5..0x00039E15 at initial IPs 0x0040 and 0x0075. Follow
the gate, ordered publications, every saved register and the outgoing
argument's last stack writer. Cross-check FND-EXE-284's call order and
later bypass/call paths. Nested calls and the second immediate's data
segment remain unresolved here. No original execution or complete caller
search is claimed; source bytes and reports stay outside Git.
