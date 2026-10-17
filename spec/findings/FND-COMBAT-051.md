---
id: FND-COMBAT-051
title: The end-of-move menu records event choices separately from its final return override
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059430..0x00059450
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059557..0x000599EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000599EF..0x00059A0B
tool: tools/research/exec-census/trampoline_target.py and overlay_listing.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

The hash-checked trampoline resolver maps 566A:0025 to descriptor 172
+018F, file 595BF, and its registered key handler 566A:0052 to +04C5,
file 598F5. FND-PARTY-128 independently identifies that registration
and the later restoration of the saved handler. FND-COMBAT-026 and
FND-COMBAT-047 identify the end-of-move caller's one/two/three cases.

### Construction and result sources

The menu root stores its two coordinate words at DS:40D6 and DS:40D4.
It calls the local +0020 constructor and selects item-ID base 36B5
when that constructor returns 36B1; otherwise the base remains 36B1.
Those are numeric control identifiers, not addresses of item text.

The root sets word DS:40CF to zero when the low byte of its argument
at SS:BP+0E is nonzero, and to one otherwise. It copies three far
label pointers from BP+10, BP+14 and BP+18 into local storage and
stores their initial bytes at DS:40D1..40D3. These first-byte reads
precede the loop's null-pointer/empty-string tests. Each iteration
sets the corresponding control property and text; the local loop
does not establish valid pointers or successful control creation.

Before entering its event loop, the root calls 3A8E:060D with the
current window pointer and saves AX in SI. Only when SI is zero
does it call 3A8E:0D9A with the current window pointer, replacing SI
with that AX. Thus the first nonzero result wins; these results are
not locally converted into menu-choice numbers.

It then saves the registered handler, registers 566A:0052, and calls
28C9:2A44 with one. In the loop it asks 39D1:0854 for a 24-byte local
record. When that call returns zero, it copies the record to stack
arguments through 1000:0699 and calls +04C5 directly. It ignores the
handler's AX. Whether or not that direct call occurs, it reloads word
DS:40D8 and repeats while that word is FFFF. No direct initialization
of this choice word occurs in the reviewed root before the loop;
preceding children and callbacks remain state producers to resolve.

### The local event handler

At +04C5 SI starts at zero. Event type is the word at SS:BP+6.
Types other than two and six leave DS:40D8 unchanged and return zero.

For type six, test the stored first label bytes in order. A nonzero
byte is sign-extended to a word and ORed with 20 hexadecimal; compare
it with the low byte of SS:BP+12, zero-extended and ORed with 20.
The first match stores one, two or three, respectively. Earlier
matches win. This is a bitwise comparison, not a general text case
conversion: a stored byte with its high bit set is sign-extended.

If no label matches, the exact word 011B at BP+12 stores zero only
when DS:40CF is zero; otherwise the handler stores FFFF. It also
stores FFFF for any other unmatched type-six input. The handler
returns FFFF when its resulting choice word is not FFFF, and zero
otherwise. The root's direct call nevertheless consumes the shared
choice word rather than that return value.

For type two, the handler searches exactly seven code-segment words
for the word at BP+8, advancing two bytes per comparison. A match
jumps through the paired target at an offset of fourteen bytes
from the matched key. The bounded key and target arrays are:

| Input word | Target within descriptor 172 | Choice stored at DS:40D8 |
|---|---|---|
| 2844 | +0594 | 0 |
| 36B1 | +059C | 1 |
| 36B2 | +05A4 | 2 |
| 36B3 | +05AC | 3 |
| 36B5 | +059C | 1 |
| 36B6 | +05A4 | 2 |
| 36B7 | +05AC | 3 |

An unmatched type-two input stores FFFF. All type-two paths return
zero, including ones that store a selection or cancellation. The
2844 branch does not test DS:40CF; it differs from the unmatched
type-six 011B branch. Naming these types as mouse or keyboard
delivery requires the surrounding event contract, not this table.

### Cleanup precedes the final choice reload

After the loop stops, the root makes two control-property calls
interleaved with 28C9:2A81 calls. Each property selection rereads
DS:40D8. Zero uses control identifier 2844; a nonzero choice uses
the low word of item-ID base plus choice minus one, sign-extended
for the property call. The root then restores the saved handler,
calls local +0127 cleanup, and conditionally services the pointer
at DS:1431 through further child calls.

Only after these children does the root decide its returned AX:
if SI is nonzero it returns SI; otherwise it reloads DS:40D8. The
choice value is not captured before cleanup. This result expression
is conditional on those children preserving SI and on the identity
and lifetime of the shared choice storage.

Local +0127 calls 3A8E:0A26 with the current window pointer and
clears that pointer at 4C4D:0000. When byte 4C4D:0004 equals one,
it additionally makes the mode-dependent 571F:007A call, then
3D72:0D83, 56BD:00BB and 28C9:2A81 with one. It finally clears the
byte flag. Child effects and errors remain dependencies.

The separately registered callback 566A:0061 resolves to +0000.
It passes zero, two words from its pointed record at offsets 98
and 96, and thirteen to 571F:00B1, then returns zero. It does not
directly write the choice word; its child remains unresolved here.

## Interpretation

This answers Q-COMBAT-015's local event-to-choice mapping, direct
handler consumption, return precedence and cleanup order. With a
zero saved child result and preserved choice storage, selection
values one/two/three reach the existing caller branches; zero
reaches none. This is not evidence that every native close or key
event actually arrives as these types or that cleanup preserves
the choice. Initialization, full children, storage aliases, event
delivery and control admission remain open. No original function
was executed and no pixels or native cancellation were observed.

## Alternatives

- Treating handler AX as the menu selection confuses type-two's
  zero return with the separate shared choice write.
- Assuming the root always returns the choice omits the saved
  nonzero result and the cleanup calls before the final reload.
- Assuming both cancellation branches test the same flag omits
  type-two 2844's unconditional zero store.
- Treating the two item-ID triples as six choices conflicts with
  the paired table targets, which store the same three values.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Resolve 566A:0025, 566A:0052 and 566A:0061 with trampoline_target.py.
Read overlay_listing.py descriptor 172, ranges 59430..59450,
59557..595BF, 595BF..598F5 and 598F5..599EF, hexadecimal,
half-open. Follow the resolved root and handler, near cleanup
target and seven-entry selector; do not decode table bytes as code.

Use report.mjs table with the same source/hash, count and limit
seven, stride two and one width-two field at offset zero. Query
start 599EF for keys and 599FD for targets. Count evidence is the
handler's CX seven, two-byte advance and LOOP. Check that key
2844 reaches the directly read zero-store block and that both
triples reach the three directly read selection blocks. The
unmatched loop and unmatched event type are separate controls;
this is a finite selector reading, not an absence census.

Reports/configurations remain in
GAME_DIR/analysis/combat-actions/menu-results-20261018/.
Trace signed byte extension, SI assignments, ignored handler AX,
each choice reread and final return after cleanup. Use the caller
reading in FND-COMBAT-047 for the existing action consumption.
