---
id: FND-CONFIG-182
title: The path append helper uses signed length gates and a prefix-dependent truncation terminator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:40D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3FC1
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit readings and header-derived MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-181's local open helper passes its far local
path, a far source formed from current DS and its near
filename argument, and limit word 80 (50 hexadecimal) to
resident 2D40:3DC2. Its local allocation is 82 bytes
(52 hexadecimal), with the later returned AX saved in
the final word at BP-2. The append call's limit covers
80 bytes; the earlier prefix copy has no capacity argument.
This does not resolve the filename's near-DS/far-SS identity
or unchanged prefix/source contents.

3DC2's complete resident span is `0x000263C2..0x00026434`.
Its inputs are destination far pointer at BP+6, source far
pointer at BP+0A and limit word at BP+0E. It retains the
limit in SI and requests destination length through
1000:40D7. Call that returned word D and the limit N.
A signed D greater than or equal to N skips all source
processing and returns the original destination far pointer.
Otherwise it requests source length through the same helper;
call that word L. Word addition forms D+L, including wrap,
and a signed comparison chooses the remaining branch.

A signed sum less than N calls 1000:3FC1 with the original
far destination and source. In the other branch it forms
word C=N-D-1, calls 1000:3F5A for C bytes from source
to destination offset plus D, then writes a zero byte at
the original destination offset plus C-1. The explicit
zero store uses the original destination, not the advanced
append destination. Its position is therefore N-D-2 in
word arithmetic. Both branches return the original far
pointer in DX:AX, without a success/error normalization.
Normal returns restore the caller's SI and DI.

For disjoint valid strings with nonnegative short lengths,
no word/offset wrap and the named limit 80, D=0 and L=80
copies 79 source bytes then zeroes original destination
byte 78. D=3 and L=77 copies 76 bytes beginning at byte
three and zeroes original destination byte 75. D=40 and
L=40 copies 39 bytes beginning at byte forty and zeroes
byte 38, within the earlier prefix. D=80 skips the source
and changes no bytes in this helper. These are conditional
static branch derivations, not observed native filenames,
reachability, corruption or a declared bug fix.

1000:40D7's complete span is `0x000092D7..0x000092F6`.
A null far pointer returns zero. Otherwise it scans at
most FFFF bytes for zero and derives a word length from
the remaining count. A reached zero at offset k returns
k; no reached zero returns FFFE. The latter is not an
error result distinct from the final possible zero offset.
It preserves DI, leaves ES from the supplied pointer and
clears the direction flag on the scanning path. A returned
word at least 8000 is negative in the caller's signed gates.
No scan result independently establishes destination capacity.

1000:3F5A's complete span is `0x0000915A..0x0000917E`.
It copies exactly the supplied word count forward from
far source to far destination, using word copies and the
remaining odd byte. It restores DS, SI and DI, returns
the original destination pointer and leaves ES changed
and the direction flag clear. It adds no terminator,
capacity check or general overlap preservation guarantee.
The path helper supplies the explicit zero afterward.

1000:3FC1's complete span is `0x000091C1..0x00009200`.
It scans destination and source for zeros with separate
FFFF-byte maxima, derives the destination append position
from its scan and copies the source scan's consumed count,
including a reached terminator. A source scan without a
zero supplies FFFF bytes. A destination scan without a
zero still supplies a derived append position; it is not
rejected. It copies forward, restores DS, SI and DI,
returns the original destination pointer and leaves ES
changed and the direction flag clear. Neither scan nor
copy has the path helper's limit argument. Aliases, offset
wrap, unterminated strings and invalid far storage remain
separate from the short-string branch derivations.

The four far-call segment operands in 3DC2 resolve to
1000 through the relocation count/table read from the MZ
header, not an assumed table location. Descriptor 25
covers the resident span; descriptor 24 shares its segment
but covers only an earlier short range. Only declared
relocations and complete helper spans are used here.
No interrupt, operating-system call or indirect call occurs
in these four bodies. Their original callers, all producers
and reachable input conditions remain incompletely read.

## Interpretation

The path helper has a limit-based branch decision but is
not a complete defensive capacity contract. Signed word
length gates, wrapped arithmetic, bounded scans without an
absence error, forward copying and the precise separate
zero store each matter. In the named long-source branch,
the terminator position also depends on the original prefix
length. None of this establishes that the original takes
that branch during the mode transition or opens any archive.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain complete callers,
prefix/source/limit producers, near-DS/far-SS provenance,
valid terminated strings, capacities, aliases, offsets and
actual archive/error-helper outcomes. One reading provides
short stable filenames and prefix whose sum is below 80;
another supplies a returning path with a different prefix,
long source or signed/wrapped length. The local cases differ
without establishing which native state is reachable.

A conventional append reading would zero at original offset
N-1, or at the append start plus C. The actual operand
instead uses original destination plus C-1. The complete
local branch rules out that conventional grouping. Source
and destination aliasing or earlier prefix-copy overflow
would require separate input evidence, not a blanket claim
that the path helper preserves a valid buffer. No native
run or emulated experiment is claimed.

## How to reproduce

Read 2D40:3DC2 through 3E33 from its entry and verify
the four runtime segments using MZ header fields and exact
relocation positions. Track BP+6/+0A/+0E, both signed
comparisons, word addition/subtraction, advanced copy offset
and original-base zero store separately. Read 1000:40D7
through 40F5, 3F5A through 3F7D and 3FC1 through
3FFF, preserving null/scan-limit behavior, source count,
copy direction, odd-byte routes and register results.
Compare descriptor 182 local 0000 through 0055 for
its 52-hexadecimal allocation, 50-hexadecimal limit,
SS path pointer and later BP-2 save. Derive the named
short, equality, truncation and skip cases only under the
explicit storage, termination, alias and arithmetic conditions.
