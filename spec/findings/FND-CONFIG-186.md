---
id: FND-CONFIG-186
title: The temporary caller byte is cleared before a separately gated mode and number update
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0139
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:070E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:06CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:250A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0C8D
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-179's later two/three gate sets current byte
DS:143F to one, calls resident 2C5F:0139, then calls
28C9:250A and clears that byte again. The complete
0139 span is `0x00021929..0x00021972`. It passes
word one and current words DS:1408/140A to 362C:035C,
then re-reads those words and passes them with words one
and three to 31BA:000E. It calls the word-eight setter
362C:06B9 bounded in FND-CONFIG-185, local far 070E
with word one, then local far 06CB with word one and
word zero. It ignores each returned result, clears current
byte DS:143F itself and returns without normalizing AX.
No further callee intervenes after that own byte clear.
A normal returning path therefore does not keep the byte
at one throughout the parent's subsequent 250A call.
DS-relative fields mean DS at each instruction; complete
external state and segment preservation remain conditions.

070E's complete span is `0x00021EFE..0x00021F81`.
It saves caller SI/DI, retains its word argument initially
in DI and calls 1BF3:282D with that argument and four
word coordinates 10,10,38,26. It retains the returned
word initially in SI; FFFF skips the remainder. Otherwise
it passes current SI and word DS:1400 to 2D40:3B30,
then passes post-call SI to 2D40:3BD1. FND-CONFIG-184
bounds that latter wrapper's signed at-most-one bypass
and its VGA-service dependency. There is no success gate
on either returned result.

Only after those calls return does 070E read word
mapped 4C10:0019. Zero or one skips the second request;
other words call 1BF3:282D with post-call DI and word
coordinates 215,4,315,38. FFFF skips its following
calls; otherwise it passes current SI and DS:1402 to
3B30 and then post-call SI to 3BD1. The function
restores the caller's saved SI/DI at normal return but
that does not by itself prove preservation between its
calls. Complete 282D, 3B30, word/handle producers and
rendered effects remain open. No own DS:143F clear
occurs until the later parent 0139 instruction.

06CB's complete span is `0x00021EBB..0x00021EFE`.
It saves SI and retains the first word there, then passes
it, mapped word 4E47:0000 and the low byte of the
second argument to local far 0334. A re-read mapped
word 4C10:0019 of zero or one skips local 03F1;
other words pass post-call SI and that same stacked byte
to 03F1. The named 0139 call supplies a zero byte.
FND-COMBAT-007 previously bounds this shared gate;
this reading retains post-call register and mapped-word
conditions without extending its presentation claims.
It restores saved SI at local return, without an overall
result normalization. Full children and graph/state effects
remain separate.

28C9:250A's complete span is
`0x0002039A..0x000203B2`. It loads current word
DS:4596 and compares it with DS:1440. Equality returns
without a call. Difference stores that loaded word in
DS:1440 before passing it to 2C5F:0C8D. No returned
result protects or rolls back that field assignment.
It has no own DS:143F or DS:0DAB store. The initial
mode gate in the parent's 0DAB does not itself settle
these separate 4596/1440 words.

0C8D's complete code span is
`0x0002247D..0x00022516`, followed by its five-word
CS jump table through `0x00022520`. It starts with a
local byte one, decrements its word argument and applies
an unsigned upper bound four before indexing the table.
Thus arguments one through five select the five exact
entries below; zero and all other word values take the
fallback. The table entries are 0CCE, 0CDC, 0CE1,
0D05 and 0CA7, verified directly from loaded words.

| Argument | Local choice before the later number gate |
|---|---|
| 1 | Signed current DS:426D below four selects word 4A9D (19101); other values select 4AA6 (19110). |
| 2 | Selects word 4AA3 (19107). |
| 3 | Calls 572F:0025. Nonzero returned AL calls 572F:0020, uses its returned AX as the selected word and changes the local byte to zero. Zero AL selects 4A9D and writes both current DS:4596 and DS:1440 to one. |
| 4 | Selects word 4A9F (19103). |
| 5 | Nonzero far field 4E71:0A5D is passed with word zero to 3D72:12ED. Whether made or skipped, that returning path sets current byte DS:9BE7 to one and returns without the later number gate. |
| Other | Selects word 4AA6 (19110). |

For branches reaching the number gate, equality of current
word DS:4273 with the selected word returns without a
resource helper call. Difference stores that word at
DS:4273 before passing it and the local byte to overlay
182 entry 56BD:006B. Its returned result is ignored;
there is no restoration of the earlier mode or number
fields on a returning failure. FND-CONFIG-187 reads
the resource helper's additional cache and size gates.
All normal paths leave AX from local or external work;
there is no own overall normalized result or 0DAB store.

All resident external call and mapped segment operands
above were checked through header-derived MZ relocations.
The local helpers have far frames through push-CS/near-call.
The 0C8D jump table is an explicitly bounded code-local
table, not an inferred target name. Unknown children,
callbacks, aliases and actual rendered/runtime outcomes
remain conditions rather than assumed atomic state changes.

## Interpretation

The parent flag's apparent bracket contains an earlier
clear in its first callee. Its subsequent mode/number
logic has independent state comparisons and assignments
before external work. A returning resource failure does
not restore the cached mode or selected number. The named
mode-three service can also replace both mode words with
one, while mode five takes a separate pointer/byte route.
These local contracts do not prove any native screen,
accepted resource, stable callback graph or final mode.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
coordinate, mapped-word, flag, mode and number producers,
SI/DI/DS and stack preservation, graph/handle/capacity
inputs, all external children and actual runtime/rendered
outcomes. One reading supplies stable fields and returning
successful services; another changes a gate or returns a
failure after the own mode/number write. Native reachability
and the effects of later callbacks remain incompletely read.

A reading that keeps current DS:143F at one until the
parent's final clear is ruled out by 0139's own clear
before its return. A reading that changes 1440 only after
a successful 0C8D result is ruled out by the store-before-
call order. The 426D signed comparison admits negative
words locally; it does not prove negative values occur in
the original. No native or emulated result is claimed.

## How to reproduce

Read 2C5F:0139 through 0181, 070E through 0790,
06CB through 070D and 28C9:250A through 2521
from their entries. Verify relocated call/mapped segments
and local far frames. Track the ignored results, fresh
word reads, post-call SI/DI, mapped zero/one gates and
own byte clear before return. Read 2C5F:0C8D through
0D25 and exactly five table words at CS:0D26. Map
word arguments one through five and the unsigned fallback,
then follow the signed 426D gate, AL-dependent mode-three
calls, mode-five bypass and 4273 store before 56BD:006B.
Keep children, aliases, input provenance and rendered effects
separate from the exact local gate and assignment ordering.
