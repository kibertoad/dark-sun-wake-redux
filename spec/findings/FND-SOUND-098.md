---
id: FND-SOUND-098
title: Chooser display children separate pure attribute and copy preservation from BIOS-dependent output
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:13E8..1000:1487
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:14BB..1000:1512
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1530..1000:1654
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:167E..1000:16A0
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:16DB..1000:1766
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:18EF..1000:190A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:192B..1000:198C
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1FEA..1000:20DF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:21E4..1000:2281
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:22A8..1000:2491
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2491..1000:25FD
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:26E7..1000:26F1
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:06AB..1000:06C2
tool: Ghidra 12.1.3 ReportFunctionSummary.java; tools/research/exec-census/resident_listing.py
environment: null
---

## Observation

Q-SOUND-010 retains held-index and frame preservation across the children
listed in FND-SOUND-097. This reads the connected attribute, cursor, text,
delay and rectangle interfaces. Source identity is SOUND_DS.EXE,204593
bytes,XXH3-128236c2dc23c071eca421eb5b427caee57. Numbers are hexadecimal
unless explicitly called counts. Valid disjoint stack/source/destination
spans, admitted segment mappings and retained global state are conditions.
No original, port model or emulated call was run; byte movement does not
establish screen pixels, native BIOS behavior or elapsed time.

### Pure attribute setters

1000:14E4 replaces DS:DEBC's bits selected by8F from the low byte of its
argument, retaining old bits70. 14F9 retains old bits8F and replaces
bits70 with the low argument byte shifted left four at byte width and
masked7F. Each has one explicit byte store, restores BP, makes no child
or interrupt call and does not change SI,DI,DS or ES. Both return far
without removing their incoming word. Their local preservation no longer
needs a child assumption, subject to the destination not aliasing saved
state. The chooser's foreground/background requests are ordinary inputs
to these exact masks, not independent evidence of rendered colors.

### Rectangle save and restore

2033 and2089 each receive left,top,right,bottom words and one far backing
pointer. They save SI/DI/BP. The pure241A helper admits only positive
ordered coordinates, with both horizontal endpoints at most zero-extended
byte DS:DEC0 and vertical endpoints at most byte DEBF. Its ordering tests
are signed; its upper extent tests are unsigned. In the admitted positive
byte-sized domain this is an inclusive rectangle inside those extents.
It returns0/1, restores SI/DI/BP and removes its eight argument bytes.

Rejected coordinates return AX0 from2033/2089 without a transfer call.
Admitted coordinates give width=right-left+1. For each top-through-bottom
row,13E8 constructs a far location whose segment is word DS:DEC5 and
whose offset is twice the low-word sum of word DEC3, zero-extended byte
DEC0 times row-minus-one, and column-minus-one. It performs no extent or
overflow check of its own, does not change SI/DI/segments and returns
DX:AX, removing its four near-call argument bytes.

2033 passes that location as source and the backing pointer as destination
to23DE, with width as a word count. 2089 reverses source and destination.
Each advances only the backing offset word by twice width after every
row, retaining its segment. Neither receives or checks backing capacity.
An admitted disjoint nonwrapping backing span needs twice width times
row-count bytes. Both return AX1 after all rows and restore SI/DI/BP;
the caller removes the twelve far-call argument bytes. Their return does
not certify a successful native device operation, and097's chooser does
not use it to guard later interpretation.

23DE selects140D only when byte DS:DEC1 is zero and word DEC7 is nonzero;
otherwise it selects2314. It forwards the same count and two far pointers
and removes its ten near-call argument bytes. No indirect call is present
in this dispatch. Actual mode/flag history remains unadmitted.

140D saves SI/DI/DS/BP. Zero count skips copying. With nonzero count it
loads the source and destination far pointers, chooses ascending or
descending word traversal from the unsigned comparison of their offset
words alone, and copies the requested number of words. It does not
compare full segmented addresses when choosing direction.

With byte DEC2 zero it uses an ordinary word string copy. With DEC2
nonzero it polls bit0 of port03DA: a one phase must end, then a zero
phase must end, before a transfer. Distinct source/destination segments
use one such phase pair per word copy; equal segments use separate pairs
for load and store. These branches include explicit CLI/STI operations,
so preservation of entry interrupt flags is not claimed. All normal joins
clear direction and restore DS/SI/DI/BP, but do not restore ES after a
nonzero-count copy. There is no child call or interrupt instruction here.
Port-phase progress is needed for termination; it is not established by
the CPU reading. Valid nonaliasing spans are needed to preserve saved
registers and surrounding frame bytes.

2314 saves SI/DI/BP, obtains cursor state through26E7 and separately
compares each pointer's segment with DS:DEC5. A matching source segment
chooses a BIOS-mediated read per word; a nonmatching source loads a word
from memory and increments only its offset by two. A matching destination
chooses a BIOS-mediated write; a nonmatching one stores to memory and
increments its offset by two. It restores the cursor before returning.
Thus segment equality selects behavior, not proof that a pointer denotes
video memory. 22A8 converts an offset to a packed row/column using byte
DEC0 as divisor, requiring nonzero width. 22E0 conditionally sets a
cursor and updates the two supplied packed-position words. These local
helpers do not add a span or native-result check.

2314's internal remaining count is held in DI across BIOS calls and its
word in SI across a destination operation. Its entry saves restore the
caller's registers only if reached children return with required local
state and stack spans intact. The BIOS-dependent branch is therefore not
promoted to unconditional transfer or frame preservation.

### Cursor, text and BIOS boundary

1FEA consumes only each argument's low byte, subtracts one and adds the
corresponding origin byte DEB8/DEB9 modulo256. Unsigned comparisons
against origin and DEBA/DEBB bound the resulting coordinates. A rejected
pair makes no BIOS call. An admitted pair supplies AH02,BH0 and the
packed coordinates to16DB. The pure rejection path leaves SI/DI/segments
unchanged; the accepted path inherits BIOS preservation conditions.

16DB explicitly saves/restores BP and DS, selecting DS0040 while making
its interrupt10 calls. AH other than00/0F goes directly to that interrupt
and the shared restoration. The other branches add mode-related requests
and conditional bytes in that low-memory segment. It does not save SI,
DI or ES. Therefore an admitted return restores BP/DS from the saved
stack but does not independently guarantee SI/DI/ES across native BIOS
execution. 26E7 requests AH03,BH0 through16DB and returns its DX in AX;
two calls by1530 are separate cursor reads, not an atomic snapshot.

167E obtains string length through3AE4, then calls1530 with that length
and the original far string; it does not append a new terminator. 1530
saves SI/BP, holds the remaining character count in SI and advances only
the source offset. Its ordinary character path either transfers one
character/attribute word through140D or uses BIOS cursor/write calls,
according to the same DEC1/DEC7 condition. Bytes07/08/0A/0D have separate
local control paths; row/column updates and boundary checks can request
scrolling through2491. It returns the final consumed byte zero-extended,
not the input length, after a final cursor operation. Required native
SI/DI preservation is not supplied by its saved SI alone.

14BB requests a whole-window operation through2491 and then a cursor
operation. 2491 chooses its direct-memory scrolling branch only for
DEC1 zero, DEC7 nonzero and incoming count byte1. It increments the four
coordinate argument bytes modulo256. Service byte6 requests an upward
one-row move, with bottom row filled; other service bytes on this branch
request a downward move, with top row filled. It does not use full
argument words for these decisions. Other conditions forward the supplied
service/count/coordinates and current attribute to16DB.

The direct branch calls21E4 to move the retained rectangle,2033 to save
one cell of the vacated row into its160-byte local scratch,246F to replace
scratch words with character20 and current attribute, then2089 to copy
that row back. It does not test the rectangle children's return values.
This is a bounded byte/word contract, not proof that the screen scrolled.
The scratch span admits at most80 filled words; larger or unordered
incoming coordinates are not admitted merely by the local allocation.

21E4 checks both source and derived destination rectangles with241A,
returning AX0 before copying if either fails. Otherwise it copies inclusive
rows through13E8/23DE, taking bottom-first order when source top is less
than destination top, and top-first otherwise. Width and displaced
row/column arithmetic are word-sized. It returns AX1 after traversal and
restores SI/DI/BP, conditional on the existing transfer/stack contracts.
246F's signed inclusive endpoint loop writes consecutive attribute/20
words, advances only the destination offset by two and removes eight
near-call argument bytes. It makes no child call and changes no SI/DI/DS;
it restores BP but leaves ES as the loaded destination segment when a
store occurs. Disjoint valid scratch and saved-state spans remain required.

Thus the local scroll copy/fill graph is assigned; native BIOS preservation,
port-phase termination and input/scratch extent admission still condition
the reached alternatives. No pixel or whole-text preservation claim follows.

### Delay preserves registers without proving time or termination

192B saves SI/BP and does not locally modify DI,DS or ES. It reads
18F0's word sample, combines the incoming duration word with the
double-word coefficient at DS:DF02/DF04 through pure06AB arithmetic,
then polls word samples with unsigned comparisons and local wrap tracking.
06AB restores its saved SI and changes no frame or segment state.
18F0 saves flags, disables interrupts, writes zero to port43, reads
port40 twice separated by calls to the one-instruction RET at18EF,
inverts the combined word and restores flags. These instructions do not
change SI/DI/BP or segment registers. On a normal terminating call192B
therefore restores held index/count registers under valid stack/nonalias
conditions. Actual timer progress, coefficient provenance and elapsed
duration are separate evidence obligations; no timing was measured.

## Interpretation

Q-SOUND-010 no longer needs local child-preservation assumptions for the
two pure attribute setters, rectangle rejection/bounds/address arithmetic,
or normal terminating delay and140D register restoration. Rectangle
transfer direction/count and mode gates are explicit. Retain the precise
BIOS SI/DI/ES boundary, port termination, scrolling scratch/coordinate admission,
heap/backing extents, mode history, aliases/reentry and native lifetime.
Those prevent a full held-index or native chooser-admission claim.
No complete-reading declaration, format-status promotion, queue-item
closure or parity validation is claimed.

## Alternatives

Saving caller SI/DI is not proof that a callee's own held loop state
survives its children. ES is not restored by140D, and its direction test
uses offsets without establishing cross-segment aliasing. AX1 after a
rectangle traversal does not prove that BIOS operations succeeded or that
any pixels were drawn. Timer polling is not a verified wait or measured
delay in this research session.

## How to reproduce

Use the verified read-only SETUP-blocks project in
GAME_DIR/analysis/config-block-construction-20261108/.
chooser-display-backing-ghidra.log summarizes the eight direct interfaces;
chooser-display-children-ghidra.log, chooser-transfer-children-ghidra.log
and chooser-scroll-children-ghidra.log retain their connected children. Regenerate
chooser-display-backing-instructions.txt with resident_listing.py for
the first eight location ranges above plus1000:2314..1000:246F and
1000:26E7..1000:26F1. chooser-display-extra-instructions.txt covers
1000:06AB..1000:06C2,1000:22A8..1000:2314 and1000:2491..1000:25FD.
chooser-scroll-children-instructions.txt covers1000:21E4..1000:2281 and
1000:246F..1000:2491. Verify actual near/far cleanup, saved registers,
memory destination spans, byte-versus-word arguments and hardware gates.
Retain all original-derived reports only in GAME_DIR.
