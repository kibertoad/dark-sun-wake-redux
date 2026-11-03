---
id: FND-COMBAT-105
title: Ninety-eight nonliteral-first sites have nonzero literal selectors and eight retain a named context selector
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00041330..0x0004136C
tool: script_selector_census.py and report.mjs table/operand at 8d11e57d; census SHA256 ef16fb9194ffa976835f2501073b2f8a74e887ec6a211066e77fa62b5990910e; pinned executable-reader and locked evidence Python
environment: null
---

## Observation

FND-COMBAT-100 leaves 106 indexed opcode-0x5E sites whose first operand
is not a simple integer. This reading classifies their second operands
before requiring a runtime domain for every possible first operand.

### Controls and scope

The unchanged index has the SHA-256 recorded in FND-COMBAT-104. The
committed census reproduces all positive controls: 912 indexed top-level
starts, 218 exact sentinel literals, 588 other integer first operands and
106 nonliteral first operands. The sentinel group's 207 literal and 11
nonliteral second operands also agree with FND-COMBAT-100.

The known GPL/37 stop remains at 0x0D39, with analysis resuming at 0x0D58.
The matching inline diagnostic and final summary count as one decoder error,
not two gaps or additional instruction starts. Repeated reports are
byte-identical. Synthetic controls distinguish nested commas, unknown values,
the full-width first-operand sentinel and zero/nonzero selector low words,
including high-word-only and negative literals.

This is a census of the retained top-level instruction starts. Nested
instructions, the undecoded gap, changed streams and native entry/parameter
preservation are not established by these counts.

### Ninety-eight sites do not require a first-operand domain to exclude their encoded zero selector

Of the 106 nonliteral-first sites, 98 have a simple integer second operand.
Their selectors range from 1 to 255; none has a zero low word. Sixty-five
of those 98 selectors are 255. The remaining eight second operands are all
kind-9 number 36, not local numbers or arithmetic expressions.

FND-COMBAT-099 supplies the consequence: even if one of the 98 first
operands equals the full sentinel, its decoded second operand cannot select
the zero-selector 61-source reader branch. This excludes that site's own
encoded zero selector under the argument-preservation conditions, not every
effect of executing it. Actual parameter mutation and changed code contexts
remain Q-COMBAT-055.

### The remaining eight sites have three first-word sources

| Resource | Instruction offsets | First operand |
|---|---|---|
| GPL/163 | 0x148D | kind-9 number 44 |
| GPL/176 | 0x0451 | kind-9 number 38 |
| GPL/185 | 0x0885, 0x0893, 0x0A49, 0x0A57 | kind-9 number 44 |
| GPL/301 | 0x073D | kind-9 number 40 |
| GPL/301 | 0x0748 | kind-9 number 44 |

Each uses kind-9 number 36 as its selector. A bounded read of the initial
15-entry named-pointer table, with actual MZ relocation checks for each
selected segment word, gives:

| Kind-9 number | Initial pointed-to field |
|---|---|
| 36, the independently known alignment control | 4C10:0021 |
| 38 | 4C13:0367 |
| 40 | 4C13:0363 |
| 44 | 4C13:0361 |

These are initial bindings, not proof of pointer-table preservation. Under
FND-SCRIPT-010's kind-9 reading, these three first operands are sign-extended
16-bit words. With their bindings intact, matching the full sentinel therefore
requires the selected word to be 0x7FFE. No actual value or admitted writer
domain for those words is inferred from the pointer table.

### Excluding one instruction's zero selector does not preserve a later local

GPL/119's earlier 0x0522 instruction is one of the 98: its first operand
is kind-9 number 38 and its second operand is literal 65. The local-number
selector at 0x0547 still follows it in the common 0x04E4 prefix of the four
call sites in FND-COMBAT-101. Native requests 9 and 49 are conditional in
that prefix; request 37 joins before the later selector. Their local-bank
effects remain unproved.

The visible backup/restore requests 29 at 0x0567 and 30 at 0x0573 occur
after that selector, beyond its local return at 0x0552, in the later
0x0553 routine called from 0x0314 and 0x0BF8. They do not bracket this
prefix in the indexed sequence. This local ordering is not a census of
backup work hidden inside native callees. FND-SCRIPT-026 records what
requests 29/30 copy; their nearby presence cannot supply the missing
preservation proof for the earlier local-number read.

## Interpretation

Q-COMBAT-057 no longer needs first-value domains for 98 sites merely to
exclude their own encoded zero selectors. Its remaining eight sites reduce
to three named first-word sources and the shared named context selector.
Q-COMBAT-058 now covers the seven literal-sentinel and eight conditional
named-context sites, with the latter still requiring Q-COMBAT-057's first
operand condition. Joint value timing and preservation remain necessary.

No native zero execution, global preservation invariant, gameplay behavior
or complete reading is established. Q-COMBAT-060 retains GPL/119's later-local
preservation question; Q-COMBAT-056 keeps the other three local sites and
Q-COMBAT-059 the malformed span.

## Alternatives

- Requiring every nonliteral first value before checking its literal selector
  leaves 98 unnecessary dependencies. The low-word selector already excludes
  their own encoded zero-reader branch, subject to the stated preservation.
- Treating kind-9 number 36 as always equal to current DS:140C would replace
  the producer/lifetime proof still open in Q-COMBAT-058 and FND-COMBAT-102.
- Nearby backup requests, initial pointer bindings and ordinary object-like
  uses do not prove storage preservation or the absence of 0x7FFE in the
  three first-word fields. Their actual writers and admitted contexts remain open.

## How to reproduce

Reuse FND-COMBAT-100's source identities and FND-COMBAT-104's exact retained
index. Run script_selector_census.py with the explicit seven baseline
controls above and the index SHA-256. Read the nonliteral-first group's
literal/nonliteral selector buckets and retain all eight site identities.
Compare repeated reports including their source, tool, parser and config hashes.

Use report.mjs table on the fingerprinted DSUN.EXE at file 0x41330,
15 rows of stride four, with unsigned two-byte fields at offsets zero and
two. For numbers 36, 38, 40 and 44, verify the segment word's MZ relocation
with report.mjs operand at table start plus four times (number minus 32)
plus two, using that row's offset as targetOffset. Reuse FND-SCRIPT-010
for the signed-word reading, not an inferred role for the resulting field.

Accepted configs, census-c.json/census-d.json and identical census-research.json,
plus named-pointer-table and named-N-operand configs/reports, remain under
GAME_DIR/analysis/selector-domain-census-20261103/. The earlier empty failed
report is diagnostic only. The bounded GPL/119 prefix and local-call controls
remain under GAME_DIR/analysis/local-bank-preservation-20261103/; they reuse
the same index. No decoded script, original bytes or save content is committed.
