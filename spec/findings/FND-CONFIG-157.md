---
id: FND-CONFIG-157
title: Startup attempts OBJEX.GFF registration before the metadata initializer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0B66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:002F
tool: Python 3.14.7 and Capstone 5.0.7 raw loaded-byte pattern search, entry-based bounded instruction checks and declared FBOV fixup mapping
environment: null
---

## Observation

The exact null-terminated OBJEX.GFF pattern has one match
in the resident load image, at file offset `0x0004DB66`,
DS:0B66. This is a raw byte result, not an ASCII-analyzer
classification or a claim about recognized Ghidra references.

Entry-based decoding of FND-CONFIG-149's caller 56B2:0020
reaches `0x000675B9..0x000675D8` before the pre-setup
branch join. It clears double word DS:1456, pushes a
double-word zero, the far output address DS:1456 and
filename offset 0B66, then calls 56BD:002F at
`0x000675CC`. The segment operand at `0x000675CF` is
a declared overlay-180 fixup: raw 05B0 names descriptor
182, mapped 56BD. This call follows the earlier RGN0FF.GFF
attempt and precedes the RESOURCE.GFF/RESFLOP.GFF attempt
in FND-CONFIG-039.

The complete wrapper at `0x00068850..0x000688A6` copies
the game's directory at DS:44F2 to its local path buffer,
then passes the supplied DS-relative filename and word 80
to appending helper 2D40:3DC2. It forwards the output
far pointer and two option words to 38FF:0066; the second
option is zero-extended to a double word. The caller's
double-word zero supplies two zero option words. The
wrapper returns AL zero when the resident return word is
FFFF, and AL one for other return words. Its path-building
callees' complete malformed-path behavior is not read here.

FND-CONFIG-037 reads the resident archive-open success
path: it links a new record and assigns the list head and
active pointer. FND-CONFIG-067 reads its separate numeric
handle output, which would be written to DS:1456 here.
That numeric handle is not the far record pointer.

At `0x000675D4`, AL zero enters a failure branch that
formats the existing missing-file pattern with the directory
and OBJEX.GFF name and calls local target 0867, file offset
`0x00067A47`. FND-CONFIG-062 reads that shared helper's
runtime termination request. AL nonzero skips the failure
branch. If the termination helper returns, the failure
branch also falls through; the code alone does not prove
the operating system ends the process there.

The subsequent resource-archive attempt and calls at
`0x0006765C`, `0x0006766D` and `0x00067679` precede
the DS:13F7 branch in FND-CONFIG-149. Both local mode
branches then join at setup 5702:00D4, followed by 00C0
and the FNFO-request initializer 00B6. Thus the OBJEX.GFF
attempt is before both mode branches and before those
requests, not supplied by 00C0's callee in FND-CONFIG-156.
The intervening callees' complete archive effects remain
unread.

Under a successful OBJEX.GFF registration, retained list
membership and unchanged mode two, FND-CONFIG-040's
wraparound lookup can still reach it after the later
resource archive becomes active. FND-CONFIG-153 supplies
the installed FNFO/1 and FNFO/2 entries in OBJEX.GFF;
neither a later active archive nor its lack of FNFO alone
excludes this fallback route. Actual selection, record
stability and successful reads remain conditional.

## Interpretation

There is a concrete earlier registration attempt for the
installed archive that holds the requested metadata. The
post-setup callee need not open that archive to supply a
possible lookup route. File presence, the open attempt and
the reader's fallback mechanism are distinct evidence;
together they still do not prove a successful native load.

## Alternatives

Q-CONFIG-008 retains the archive-open outcome and all
intervening effects on list membership, active archive,
traversal mode and selected records. One reading successfully
registers OBJEX.GFF and retains it through the FNFO requests;
another fails the open or changes archive state before them.
The local call order rules out a claim that the attempt
occurs only inside one of the two pre-setup mode branches.
Reading intervening callees and archive state changes, or
observing the bounded I/O outcome, would distinguish the
remaining states. No new native run was performed.

## How to reproduce

Search the raw resident load image for the exact null-terminated
filename and check the byte span at DS:0B66. Decode from
56B2:0020 to verify the push and call boundaries rather than
treating a raw filename as behavior evidence. Read
`0x000675B9..0x00067693`, checking the declared segment
operand, both result branches and order relative to setup.
Resolve wrapper 56BD:002F and read it through its far return,
including argument widths and the FFFF comparison. Combine
its open call with FND-CONFIG-037 and FND-CONFIG-067,
its failure helper with FND-CONFIG-062, and its conditional
lookup route with FND-CONFIG-040 and FND-CONFIG-153.
Do not replace the open or intervening-state conditions
with the installed catalog alone.
