---
id: FND-CONFIG-189
title: The replacement services have distinct callback checks and commit state after some failed handle requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0B84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0942
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:07D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0699
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and header-derived MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-188's pointer wrapper calls resident 3D72:0B84
before its state assignment and 0942 afterward. They also
occur in FND-CONFIG-171 and FND-CONFIG-174's shared
paths. Their complete local bodies span
`0x000334A4..0x000336A3` and
`0x00033262..0x0003347F` respectively. Both save
SI/DI before a stack-limit guard and restore those saved
registers at local normal return. Neither supplies an own
overall AX success/error normalization. DS-relative fields
mean DS at each instruction; complete child preservation,
pointer validity and aliasing remain conditions.

### 0B84

A nonzero word DS:332E skips everything after the guard.
Zero snapshots word DS:2FB8 into the local frame, then
reads word DS:A155. FFFF exits without active mode work
or the later snapshot-restoration instruction. Other values
select the following distinct paths.

Value two first calls 39D1:0609, then reads the far
record pointer at DS:A15B and its double word +5A.
There is no own null-record-pointer check. A zero +5A
exits without calling 39D1:05E1. Nonzero re-reads the
record pointer and calls its current +5A far target,
without a returned-result gate, then calls 05E1. It
copies current word DS:A13D to DS:A155 and exits
without the later DS:2FB8 restoration. FND-CONFIG-171
and FND-CONFIG-175 bound the bracket helpers; fresh
record/target and callback effects remain open. Checking
one callback value does not itself freeze the later target.

Value three dispatches current word DS:A141. One
passes word DS:A033, double words DS:A1AD/A115
and the XOR of words DS:A197/A1A5 to 1BF3:7AB5.
Two builds four local words. Its signed comparison of
A1AD with A111 selects either (A1AD,A115) or
(A115,A1AD) as the first/third words; its signed
comparison of A1AF with A113 similarly selects
(A1AF,A117) or (A117,A1AF) as second/fourth.
The comparison fields differ from one of the selected
fields, so these are not assumed generic min/max pairs.
It uses runtime 1000:0699 to copy those eight SS-frame
bytes into call arguments for local far 07D9. Other
A141 values skip both children. Each returning value-three
path copies current A13D to A155, then reaches the
DS:2FB8 snapshot-restoration instruction.

For other non-FFFF A155 values, it enters a separate
buffer path only when word DS:3330 is nonzero,
DS:33B6 differs from DS:33B8 and DS:33B8 differs
from DS:A033. Otherwise, both DS:A167 and DS:A169
must differ from FFFF to call 2D40:3B1D with those
words; a sentinel skips that call. Both routes here
reach the saved 2FB8 restoration without testing its result.

The buffer path snapshots far DS:A14D and word
DS:A151. A null pointer exits without saved 2FB8
restoration. Nonnull asks 1BF3:76C2 and 771C for
word results, retaining them in SI and DI; either signed
result below one likewise exits without that restoration.
Other results undergo signed, word-wrapped adjustments
using current coordinates A115/A117 and bounds
A039/A03B. It subtracts a positive coordinate-plus-size
minus bound from each corresponding size. There is no own
positive-size recheck after those adjustments.

It requests two 1BF3:282D handles for the adjusted
coordinates, first with word DS:33B8 then DS:33B6.
It stores their returned words and calls 2D40:3B1D
without checking either for FFFF. It subsequently calls
1BF3:28C5 only when each selected signed handle is
greater than one. The second decision uses post-call SI,
so intervening preservation is a condition on closing the
originally returned second handle. FND-CONFIG-184 reads
28C5's VGA port and compaction dependencies. This
returning path reaches the 2FB8 restoration. Thus snapshot
restoration is path-specific, not a general rollback or
caller-state preservation promise.

### 0942

Nonzero word DS:332C skips active work. Zero snapshots
DS:2FB8, then independently passes current DS:A167 and
DS:A169 to 1BF3:28C5 only when each is signed-greater
than one. Their returned results are ignored. Both current
fields are set to FFFF afterward regardless of those results
or skipped calls. Only then does word DS:A13D choose
the remaining mode path.

Mode two calls 39D1:0609, passes word DS:A033 and
double word DS:A10D, and calls the current far record
DS:A15B's +5E target. There is no own record or
callback null check on this path. A returning callback is
followed by 39D1:05E1 and the shared commit described
below; its returned result does not guard that commit.
This differs from 0B84's own +5A zero-target exit.

Mode three dispatches DS:A141. One passes A033,
double words A1AD/A10D and XOR(A197,A1A5) to
1BF3:7AB5. Two builds an eight-byte rectangle argument
from signed comparisons of A1AD/A10D and A1AF/A10F,
using the compared fields as its ordered pairs, and calls
local far 07D9 through runtime 0699. Other values
skip those children. All returning mode-three routes reach
the shared commit without child success tests.

Mode zero snapshots far DS:A14D with word DS:A151;
other modes snapshot far DS:A145 with word DS:A13D.
Current A13D FFFF or a null snapshot exits without
shared commit or saved 2FB8 restoration, after the earlier
handle releases/clears. Nonnull asks 76C2 and 771C
for two word results. A signed result below one takes
the same early exit. Admitted results undergo the same
signed/wrapped bound adjustments, now using coordinates
A10D/A10F. Again no own post-adjustment positive-size
check occurs.

The admitted path requests a 1BF3:282D handle with
current A033 and adjusted coordinate words, storing AX
at current A167. It then requests 1BF3:27A8 with
words zero, zero, current SI and DI, storing AX at
current A169. FND-CONFIG-183 bounds 27A8's slot,
product and paragraph gates. Either stored FFFF skips
its later 3B1D and 2D40:3BEC calls but still reaches
the shared commit. Two nonsentinel handles request 3B1D,
then 3BEC with current A033, coordinates A10D,
word zero, the local far pointer and local index word.
Neither returned result protects the subsequent commit.

The shared commit copies words A109 to A111,
A10B to A113, A10D to A115, A10F to A117,
and A13D to A155, sets word A165 to one, then
restores current DS:2FB8 from its saved word. These
own assignments occur even when an admitted request
returned a stored FFFF and rendering children were skipped.
They do not prove valid handles, performed presentation,
accepted contents or a successful service. Early exits
already described bypass these assignments.

### The rectangle and argument helpers

The complete local 07D9 span is
`0x000330F9..0x000331D1`. It takes four word
coordinates. Equality of first/third and second/fourth
selects a 1BF3:7F58 call with A033, the first
coordinate pair and XOR(A197,A1A5). One equal axis
selects a 7AB5 call; two differing axes select four
7AB5 calls, with word increments/decrements on the
inner endpoints. It ignores returned results and provides
no own coordinate validity/capacity or AX normalization.
Complete primitive effects and rendered geometry remain open.
This bounded call selection is not a pixel-layout claim.

Runtime 1000:0699's complete span is
`0x00005899..0x000058BA`. FND-CONFIG-128 previously
reads its stack-argument copy contract. Here CX eight and
source DX:AX=SS:local offset reserve/copy exactly eight
bytes on the stack for the next far call. The local caller
adds those eight bytes back after 07D9 returns. That
explicit SS copy does not require equating near DS and
SS pointers, but valid stack, aliases and child effects
remain caller conditions.

All resident direct call segments were verified through
header-derived MZ relocations. Indirect record calls retain
their +5A/+5E fields, not an invented fixed target or a
relocation check on an indirect operand. No own 0DAB,
1440, 0FCC or 0FCF write occurs in these bodies.
VGA children, callbacks, guard/runtime effects and possible
aliases remain separate from that direct-write inventory.

FND-CONFIG-190 subsequently reads the frame dimension helpers;
FND-CONFIG-191 reads the handle request, forwarding wrapper and
coordinate gates. Wrapper invocation remains separate from an admitted
primitive call, and accepted source/slot storage remains conditional.

## Interpretation

The active replacement services do not have a universal
success or restoration path. Their mode-two callbacks have
different null-target handling, and later state commits can
occur after stored handle-request failures. Early exits may
follow prior releases and bypass the saved-word restoration.
The wrapper's zero result in FND-CONFIG-188 cannot
collapse these distinctions into completed presentation or
unchanged caller state.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
word/byte/pointer/mode/coordinate/bound/handle and callback
producers, SI/DI/DS and stack preservation, valid buffers,
capacity, aliases, primitive/runtime/VGA and actual native
outcomes. One reading supplies admitted valid resources and
returning callbacks; another has a word guard, early exit,
null target or stored FFFF with a later state commit.
Their local routes are now bounded, but native reachability
and accepted presentation require further evidence.

A reading that both mode-two callbacks have an own null
check is ruled out by 0942's unchecked +5E call. A
reading that FFFF handle requests always block state commit
is ruled out by the shared 0942 continuation. A reading
that every snapshot reaches its restoration is ruled out by
the distinct early exits and 0B84's value-two path.
No native or emulated outcome is claimed. The unresolved
indirect targets and VGA children prevent treating complete
local bodies as established end-to-end behavior.

## How to reproduce

Read 3D72:0B84 through 0D82 and 0942 through
0B5E from their entries, including every early-exit target.
Follow word guards, saved 2FB8, signed/unsigned predicates,
+5A/+5E target reads, per-mode argument construction,
handle-result gates and explicit shared state assignments.
Track entry/exit paths with and without snapshot restoration
and pre-commit releases. Read 07D9 through 08B0 and
1000:0699 through 06B9 for coordinate call selection
and the exact eight-byte SS stack copy. Verify direct MZ
segments only on immediate far calls; retain indirect target
and caller/register/storage provenance. Use FND-CONFIG-171,
FND-CONFIG-175, FND-CONFIG-183 and FND-CONFIG-184
for the bounded bracket, allocator and VGA dependencies.
