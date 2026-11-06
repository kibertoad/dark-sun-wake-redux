---
id: FND-PARTY-282
title: The timer segment store excludes the canonical alias value while five draw stores depend on step and residual state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:0770..371D:0795
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:06C4..362C:0CD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:066F..362C:06B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:0EE5..362C:0F12
tool: tools/research/exec-census/resident_listing.py at 019ca5ad; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-TIME-010 and FND-EXPLORE-006
environment: null
---

## Observation

Q-PARTY-124 retains twenty unclassified writer values after
FND-PARTY-281. Six are the segment store 371D:0784 and five
stores in 362C:06C4. FND-PARTY-275 supplies their census identities.
Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Offsets below are hexadecimal;
1001 denotes the decimal resource number, word 03E9.

### The timer pointer's segment word

The complete 371D:0770 body has no call and no segment assignment.
It stores a callback pointer, an enable byte, the current DS word at
DS:9C12, offset 145E at DS:9C10 and byte four at DS:9C0B.
FND-TIME-010 already connects that pointer with the tick readers.

For the selected canonical physical word 57E0:9C12 in the resident
alias census, this fixed-offset store reaches those bytes when DS is
57E0, and therefore writes 57E0, not 03E9. Changing DS to 03E9
changes both the stored value and destination segment; it does not
write 03E9 to the same canonical word. This excludes this instruction's
1001 result at that canonical location, independently of its callers.
Other segment aliases, computed offsets and the getter's segment
identity remain outside this fixed-offset store result.

### Five drawing-state values

The complete 362C:06C4 body confirms these immediate source expressions:

| Store | Value at the store |
|---|---|
| 07DD to DS:9BEE | Current word at DS:9BF0, loaded at 07DA after the drawing call. |
| 086B to DS:9BEE | Current word at DS:9BF2, loaded at 0868 after the drawing call. |
| 0929 to DS:9BEE | The 091B sample of 9BF2 when unsigned greater than the 091E comparison operand at 9BF0; otherwise the 0926 reload of 9BF0. With retained source state this is their unsigned maximum. |
| 08F1 to DS:9C00 | Word sum of 9C00 and 9BF0 loaded at 08EA..08ED. |
| 0C5F to DS:9C00 | Word sum of 9C00 and 9BF0 loaded at 0C58..0C5B. |

The two sums wrap modulo 10000. Their exact local condition for a
1001 result is that the sampled old 9C00 equals 03E9 minus the
sampled 9BF0 modulo 10000. Under retained comparison/reload source state, the unsigned maximum is 03E9 exactly
when one sampled operand is 03E9 and the other is no greater than it.
The two copy stores require their sampled source to be 03E9.
No screen-coordinate interpretation alone supplies a bound.

FND-EXPLORE-006 already identifies two step setters, nine direct
overlay-setter callers and no direct resident-setter caller. Its recorded
caller values for 9BF0 and 9BF2 are 4, 16 or 32 decimal. The resident
0EE5 body copies arguments two and one respectively into those globals
without a range check, then calls 2707:06B9. It is not a validating
interface. Under the recorded caller-value and preservation conditions,
the two copies and unsigned maximum exclude 1001; actual other entry
forms, source writers, aliases and reached-child effects remain open.

### The sums include a conditional residual write

Before 08F1, the body passes near addresses of its BP-4 local and
global 9C00 to 362C:066F, along with a direction byte. That helper
returns unchanged when DS:29EA is zero or the direction byte is
nonnegative. Otherwise it compares signed negative word 9BF4 with
the input-local word minus 40, using word-width arithmetic.

On the greater-than branch it writes the negation of
input minus 40 plus current 9BF4 through its second near pointer,
then zeroes the input through its first pointer. On the other branch
it subtracts 40 minus 9BF4 from the first pointed word and leaves
the second pointed word unchanged. These near pointers are dereferenced
through DS, including the offset obtained from the caller's stack local.
DS/SS backing and distinct storage must therefore be admitted before
treating that offset as the caller's local. This helper does not establish
an unconditional small residual or reset of 9C00.

The later 0C5F sum rereads current 9C00 after intervening calls and
stores. It is not proved to retain the earlier value or an intended
residual. Keep its sampled state and preservation separate from the
first sum's immediate helper contract.

## Interpretation

Fourteen other writer values remain unclassified. Of these six sites,
one is excluded at the canonical census location by its coupled
segment/destination semantics; five now have explicit sampled-source,
modular-sum and preservation obligations. The 36 literal constant-store
exclusions remain a separate count. FND-PARTY-279/281's count and
archive conditions remain open, as do 83 other direct getter sites
and repeated-hook preservation. FMT-PARTY-007 stays supported; no
question closes or complete reading is declared.

## Alternatives

- Recorded literal callers do not prove those step values survive every
  child call, or exclude indirect/bypass entry and alias writes.
- Unsigned maximum is not signed minimum. The branch and operand widths
  determine the local condition even for values outside intended steps.
- A near stack-local offset does not imply SS addressing in a callee.
  The helper's actual default segment must be retained in storage admission.

## How to reproduce

Run resident_listing.py on the fingerprinted DSUN.EXE with the four
half-open locations above, starting at the established function entries.
Retained reports are draw-state-alias-producers.txt and
draw-state-alias-helpers.txt under
GAME_DIR/analysis/class-table-closure-20261106/.
Reuse FND-TIME-010 for pointer meaning and FND-EXPLORE-006 for the
existing setter caller census; retain its direct-only limitations.
Check DS as both source and destination segment at 371D:0784,
JBE at 362C:0922, and the helper's default-DS pointer dereferences.
No original game ran.
