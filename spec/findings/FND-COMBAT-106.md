---
id: FND-COMBAT-106
title: The named-44 setter narrows after a full-width gate and its indexed inputs are mostly other named words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:151C..172C:1587
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0AB6..2D40:0AE5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0D94..2D40:0E1D
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: resident_listing.py at 947bd876 with actual MZ relocations; retained script_decode.py index of FND-COMBAT-100; PowerShell exact-line operand classification
environment: null
---

## Observation

FND-COMBAT-105 leaves six selector sites whose first operand is kind-9
number 44, and two whose first operands are numbers 38 and 40. Number 44's
initial pointer names 4C13:0361. This reading distinguishes one concrete
setter's own value conversion from admission of its inputs and other writers.

### The script setter uses two different numeric widths

FND-SCRIPT-005 identifies opcode 0x41's dispatch target as 172C:151C.
The complete local body reads one number through 172C:3278 and retains
its signed 32-bit result in a four-byte local. It calls 2D40:0AB6 when
that full result is negative or exactly 9999. The lookup receives only the
low word, and its returned AX is sign-extended back to the four-byte local.

After a lookup, result 9999 takes the failure branch: the four-byte field
4C13:031B becomes zero and the handler makes no store to 4C13:0361.
Every other lookup result, and every original nonnegative input except
9999, reaches the success branch. It stores the local's low word into
4C13:0361, then assigns one to 4C13:031B. Both paths join the same return.
There is no positive upper-bound check in this body.

The full-width gate precedes narrowing. Conditional input 32766 would
therefore store 0x7FFE directly; 98302 has the same low word and also
bypasses lookup. These are arithmetic examples, not observed script inputs.
Even a negative full-width input cannot be excluded merely by its sign:
-32770 narrows to positive 32766 before the lookup's own signed-word test.
Whether any such input reaches this handler remains open.

### The lookup does not normalize every input to a small index

The bodies agree with FND-CONFIG-141. Wrapper 2D40:0AB6 supplies a zero
cursor to 2D40:0D94 and returns its result word, defaulting to 9999.
The helper immediately returns a nonnegative input word itself and writes
320 to the cursor. A negative input scans indices from that cursor while
signed less than 320, skipping inactive rows and comparing selector-zero
lookup results. A match returns the scan index; exhaustion returns 9999.

For the wrapper's negative-word route, indices start at zero and advance
to 319, conditional on preservation of the helper's scan register, locals,
segments and cursor storage through its selector-zero calls. These children
are not modeled as harmless, and this reading does not establish those
preservation conditions. In particular, the nonnegative passthrough means
that calling the lookup is not itself a universal range restriction.

### The retained index partitions 316 top-level setter operands

The index identity is unchanged from FND-COMBAT-104:
SHA-256 649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Selecting canonical top-level opcode-0x41 lines and classifying the entire
remaining operand, rather than substrings inside expressions, gives:

| Operand class | Count |
|---|---|
| Simple signed decimal literal | 16 |
| Exactly kind-9 number 37 | 8 |
| Exactly kind-9 number 38 | 59 |
| Exactly kind-9 number 39 | 74 |
| Exactly kind-9 number 40 | 156 |
| Exactly kind-9 number 44 | 2 |
| Other expression | 1 |
| Total | 316 |

All 16 literals lie between -3330 and -64. Their low words are still
negative signed words: none is an encoded positive 0x7FFE input. Under
the lookup-preservation conditions above, they either store an index from
zero through 319 or leave the prior field on lookup failure. Failure does
not clear a prior sentinel value. This excludes those literals as direct
sentinel-producing inputs under those conditions, not every effect or
later value of their executions.

The 299 named operands do not come with value bounds. The two reads of
number 44 may feed its current value back into its setter. The sole other
operand is the path expression at GPL/268 offset 0x0647. Its output domain
is not supplied by counting it. Positive line controls include GPL/1
0x0035's negative literal and GPL/5 0x0001's number-38 operand.

These counts cover decoded top-level starts only. They exclude nested
instructions, the GPL/37 gap, independent entry/stream changes and native
argument mutation. No universal writer census follows. In particular,
FND-RNG-068 already records the native replacement routine updating all
four words 4C13:0361/0363/0365/0367; the script setter is not their only
known producer. Initial named-pointer bindings remain conditional.

## Interpretation

The number-44 field needs its own producer and preservation proof, rather
than a bound inferred from object-like uses or an assumption that every
negative full-width setter input is normalized. Its six first-operand
sites can be researched independently of the number-38 and number-40
sites. Q-COMBAT-061 takes this field-specific obligation; Q-COMBAT-057
keeps the other two first-word sites. Q-COMBAT-058 still owns their common
selector and joint timing with current region.

This is a local handler and encoded-input result, not an admitted sentinel
execution, absence proof, complete reading or gameplay rule. No original
runtime was started and no evidence status or parity status is promoted.

## Alternatives

- A permanent small-index bound is not enforced by the setter: its
  nonnegative full-width bypass stores a low word without an upper test.
- A negative full-width input is not necessarily a negative lookup word;
  narrowing can change its signed interpretation before the lookup gate.
- The 16 literal sites do not settle the other 300 indexed inputs, other
  native writers, storage aliases or retention on failed lookup.
- The setter alone does not establish pointer-table preservation or joint
  admission of the later first operand and its zero context selector.

## How to reproduce

Use the fingerprinted DSUN.EXE of FND-COMBAT-100 and the locked evidence
Python with resident_listing.py at the stated tool commit. Read the three
half-open resident ranges in Locations; verify the four-byte signed gate,
two-byte argument push, sign extension, final low-word store and distinct
failure branch. Reuse FND-SCRIPT-005's dispatch identity and
FND-CONFIG-141's lookup contract; preserve its child/input limits.

Reuse the exact retained index, checking the SHA-256 above. In PowerShell,
select lines with case-sensitive regex
`^[A-Z]+/[0-9]+ [0-9A-F]{4}: 41 `.
Classify whole lines using
`^[A-Z]+/[0-9]+ [0-9A-F]{4}: 41 (-?[0-9]+)$`
and then
`^[A-Z]+/[0-9]+ [0-9A-F]{4}: 41 var9\[([0-9]+)\]$`;
retain every unmatched selected line. Parse literal captures as signed
integers and group named captures by number. Require total 316, literal
16, named 299 and remainder one, plus both positive line controls above.
Compare the 16 literal low words at signed-word width before applying
the conditional scan result. Do not treat substring matches as operands.

The original index stays under
GAME_DIR/analysis/reader-admission-20261029/. Bounded handler-41.txt stays
under GAME_DIR/analysis/selector-domain-census-20261103/. Input helpers
and replacement-root listings, plus diagnostic partial writer/reference
candidate scans and selected opcode-41 lines, stay under
GAME_DIR/analysis/named-word-producers-20261103/. Replacement-root.txt
confirms the previously recorded FND-RNG-068 context; it is not a new
complete writer census. No original listing or script text is committed.
