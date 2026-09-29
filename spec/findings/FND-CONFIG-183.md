---
id: FND-CONFIG-183
title: The intervening overlay initializer gates on a byte and checks allocation and handle results before cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5773:0025
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:27A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2707
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-179's initial mode-two path passes double word
000092E0 to overlay 200 entry 0025 after its other setup
calls. Descriptor 200 maps segment 5773 and its verified
resident trampoline 0025 to code 0386, complete file span
`0x000894E6..0x000895CF`, ending with far return at
`0x000895CE`. The caller ignores its returned AX.
DS-relative fields below mean DS at each instruction.

The entry compares its double-word argument with 000092E0
behind a byte DS:14E3 gate, but both comparison outcomes
join code 03A4. The next comparison of byte DS:265C
supplies the actual branch flags. Equality with one returns
AL zero without the later work. Other byte values first
set that byte to one, then pass double words one and the
supplied argument to 444C:0008. Returned DX:AX is stored
in far field DS:2652. A null returned pointer branches to
failure cleanup before the second request. Nonnull passes
one and 00001200 to the same helper; returned DX:AX is
stored at DS:6573 and another null result branches to cleanup.
No resource tag or resource-number lookup occurs here.
The named 92E0 is forwarded to the allocation helper;
accepted capacity and complete allocation effects remain open.

After two returning nonnull requests, the body makes three
calls to resident 1BF3:27A8, storing each returned word
before testing full AX against FFFF:

| Ordered call | Four word arguments, first through fourth | Stored word |
|---|---|---|
| First | 0, 0, 319, 63 | Current DS:9BFC |
| Second | 0, 0, 63, 199 | Current DS:9BFA |
| Third | 0, 0, 63, 63 | Current DS:9BF8 |

FFFF from any call takes the same failure branch and skips
later requests. Other values continue. After the third
accepted word it calls runtime 1000:3FA2 to fill 1,280
bytes at far current DS:95FB with zero, then writes the
supplied argument at DS:95FB and the current far field
DS:2652 at DS:95FF. FND-CONFIG-176 bounds the fill
helper's pointer result, offset wrap and lack of capacity
checks. This success continuation returns AL one. It does
not independently normalize AH or prove all other state valid.
The initial byte remains one unless another effect changes it.

At the shared failure branch, a local far call to 046F
runs cleanup before a near argument DS:2670 is passed
to 56B2:0034. FND-CONFIG-184 reads that cleanup's
ordered gates and writes. FND-CONFIG-062 previously
traces the error helper through a runtime process-termination
request. If both calls return, the body returns AL zero.
A returning failure therefore does not itself stop the
parent's later mode re-read; operating-system termination
or a nonreturning callee is a separate dependency. A
pointer assigned or handle stored before a later failure
is not a completed initialization claim.

1BF3:27A8's complete resident span is
`0x000138D8..0x0001395D`, including both result returns.
It saves DS, SI and DI and selects DS=CS. A local far
call through push-CS/near-call to 2707 selects a slot.
2707's complete span is `0x00013837..0x0001384B`: it
examines 256 words at CS:C04 with stride two for bit
0080. A found bit returns its SI offset with carry clear;
exhaustion sets carry. It reads no pointer supplied by
the overlay caller and does not itself reserve a slot.

27A8 returns FFFF on exhausted slots. Otherwise, using
its four word arguments a,b,c,d, it forms width
(c >> 2) - (a >> 2) + 1 and height d-b+1 with word
arithmetic. Unsigned multiplication with a nonzero high
word, or low product above 16,000, returns FFFF.
The admitted product is rounded to paragraphs by adding
15 then shifting right four. Word addition to CS:E4E
followed by an unsigned comparison against CS:E4C
provides another FFFF route. There is no explicit wrapped-
addition rejection. An accepted request advances CS:E4E,
stores the old paragraph position and count and four
arguments in arrays at slot offset plus 4, 204, 404,
604, 804 and A04, clears that slot's C04 word, and
returns slot offset divided by two. It restores saved
DS/SI/DI at both local returns. It contains no interrupt,
port access or further call besides the complete slot scan.
Native free-slot/capacity producers remain incompletely read.
For the named geometry arguments, the local products are
5,120, 3,200 and 1,024, requesting 320, 200 and 64
paragraphs respectively, conditional on admitted state.
This does not establish their rendered purpose or availability.

Every segment operand in the initializer was resolved
through declared descriptor-200 FBOV fixups. The cleanup
call's push-CS/near-call has a far frame and target 046F.
No own DS:0DAB store occurs in the initializer, but
allocator, cleanup, error, alias and segment effects remain
conditions on the parent's later mode comparison.

## Interpretation

The intervening call's argument is used in allocation setup,
with a pre-request state byte and ordered failure gates.
The entry has genuine result branches, unlike the parent's
later ungated resource requests. Its local return distinguishes
AL zero and one, which the parent ignores. Failure invokes
cleanup and an error path that requests process termination;
a local return alone does not decide the original's outcome.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the initializer's
other callers, DS/byte/pointer/slot/capacity producers,
complete allocation and termination effects, cleanup inputs,
aliases, accepted storage and native I/O outcomes. One
reading begins with byte one and skips requests; another
begins with another byte and reaches success or a checked
failure. Direct bodies settle their local ordering, not which
state the original reaches during the named parent call.

FND-COMBAT-009 preserves an unreconciled mapped-decompiler
reading of this caller as 4758:01D5. The declared fixup,
descriptor-200 trampoline and complete code-0386 body used
here resolve the raw-file call to 5773:0025. They do not
explain that other analysis environment's mapping or rename
this behavior as combat. Further mapped-image provenance,
not a decompiler target name alone, would reconcile those
identities. No native or emulated outcome is claimed;
Q-SCRIPT-007 cannot execute this overlay.

## How to reproduce

Resolve the declared descriptor-200 call operand in
FND-CONFIG-179, validate trampoline 0025 and read code
0386 through 046E from the entry. Follow both ignored
92E0 comparisons to the actual byte-one gate, each stored
allocation/handle result, the zero fill, fixed fields and
AL returns. Verify every declared fixup and the cleanup's
far frame. Read resident 1BF3:27A8 through 282C,
including its alternate FFFF return, and 2707 through
271A. Derive word/unsigned geometry, slot and paragraph
cases without accepted-state or rendered-purpose assumptions.
Keep allocation, cleanup, error and actual process outcomes
separate from the parent's ignored result.
