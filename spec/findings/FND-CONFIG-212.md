---
id: FND-CONFIG-212
title: Heap request arithmetic separates a signed high-word gate from wrapped far-address normalization
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0562
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:060B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1831
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident readings with selected shift count, signed branch flags and explicit word widths
environment: null
---

## Observation

FND-CONFIG-211 leaves two arithmetic helpers in the 1831 request
body open. Near entry 0562, file 0x00005762..0x00005782,
converts its near-call return frame to a far-return frame. Its body
branches on unsigned CL below 16. The known caller supplies CL four,
AX current DS:00A4 and DX zero. On that selected branch it shifts
the low and high words and merges the low word's upper bits into the
high word. The result is exactly the unsigned segment word times 16.
The other shift-count branch is encoded but is not selected by this
call form; no general large-count shift contract is assigned here.

The request then adds current offset DS:00A2 and its double-word
increment with carry between the two words. Let S and O be those
unsigned segment and offset words and D the increment's 32-bit
pattern. The preliminary result is (16*S + O + D) modulo 2^32.
At 184E..1858 it compares the high word with 15 using signed JL
and JG. A signed high word below 15 passes; above 15 rejects. At
equality it compares the low word unsigned against FFFF and passes
on JBE, so every low word passes that equality branch. As an unsigned
high-word set, the preliminary gate therefore admits 0000..000F
and 8000..FFFF, and rejects 0010..7FFF. It is not a general
unsigned linear-address upper-bound predicate.

Near entry 060B begins at file 0x0000580B. It converts its near
return frame to a far-return frame and selects addition or subtraction
by the signed high word of the increment pair. A negative increment
is negated in double-word arithmetic, then uses shared subtraction
continuation 064D..066A. A nonnegative increment uses 061E..0639.
The addition path accounts for low-offset carry by adding 1000 to
the segment, incorporates the low nibble of the increment's high
word into segment bits 12..15, and folds the resulting offset's
paragraphs into that segment. The subtraction path similarly accounts
for borrow, subtracts that high-word contribution and normalizes the
remaining offset. All segment operations are word-sized; the returned
offset is masked to 0..15. The negative path is shared with separately
entered 063A, whose other entry branch is not needed by this call.

For either selected path, the helper's numeric result can be described
as L = (16*S + O + D) modulo 2^20, returned as segment floor(L/16)
and offset L modulo 16. Using signed D gives the same residue because
2^32 is divisible by 2^20. This is a description of the helper's
word arithmetic, not a claim about physical memory, A20 state or a
valid allocation. The helper itself has no address bounds check.

For example, S zero, O zero and increment pattern 80000000 passes
the preliminary signed-high gate and normalizes to 0000:0000.
S FFFF, O 000F and increment one fails the preliminary gate even
though the helper alone would normalize its sum to 0000:0000.
These are arithmetic consequences of the local reading, not executed
experiments, admitted native requests or reported game defects.

FND-CONFIG-210's paragraph admission yields counts 1..FFFF for
nonzero admitted byte requests. FND-CONFIG-211's allocation wrappers
supply a nonnegative byte increment no greater than FFFF0, or zero
or an alignment increment 1..15. With any unsigned S and O, those
increments put the preliminary unwrapped sum at most 20FFDF, below
the signed high-word region and below 32-bit overflow. For these
specific forms the preliminary gate does reject sums above FFFFF.
They cannot be used to prove the predicate safe for all other callers
or arbitrary increment patterns.

After that gate, 1831 still compares the normalized candidate against
current lower and upper far bounds through FND-CONFIG-167's 07F0,
then calls 177C. Bound identity and ordering, current break producers,
other callers, runtime state, interrupt outcomes and lifetime remain
open. Local arithmetic admission is not successful heap growth.

## Interpretation

The two arithmetic dependencies in the named allocation request path
now have bounded local descriptions. The same signed preliminary gate
has a narrower effective range when its callers supply the recorded
small nonnegative increments. Preserving that input provenance avoids
both an unsupported universal safety claim and an unsupported claim
that a large wrapped case occurs during ordinary allocation.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain other 1831 callers and their
increment ranges, current break/bound writers, linked heap state,
direction-flag provenance, runtime outcomes and resource lifetime.
One reading uses the named nonnegative allocation forms and valid
bounds; another supplies a different increment pattern or changed
state. Caller and producer coverage would settle reachability and
subsequent outcomes. No native or emulated result is claimed.

The reading that the preliminary gate compares the high word unsigned
is ruled out by JL/JG. The reading that normalization retains an
unbounded linear address is ruled out by segment word arithmetic and
offset masking. The large-pattern example is not evidence of native
reachability or a reproduced allocation defect.

## How to reproduce

Read 0562..0582 and follow its known CL-four branch from 1831.
Read 060B..0639 and its negative branch through shared 064D..066A,
checking negation carry, offset carry/borrow, segment contribution and
final nibble masking. Re-read 1831's 1841..1858 additions and signed
branches, keeping the later far-bound predicates separate. Compare
FND-CONFIG-210's byte-to-paragraph admission and FND-CONFIG-211's
request argument widths and ranges. Derive examples as static arithmetic
only; retain other callers and bound producers as open evidence.
