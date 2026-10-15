---
id: FND-COMBAT-046
title: The shipped DATA effect-byte domain includes one code 127 and its reviewed local route does not impose the name-table bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00017A29..0x00017A72
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000650F1..0x00065222
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082269..0x000822A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008268E..0x00082724
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00082724..0x0008274A
tool: tools/research/exec-census/data_bytes.py and overlay_listing.py; tools/evidence/report.mjs table; DarkSunWakeRedux.Inspect gff; installed-build fingerprints and fixup checks
environment: null
---

## Observation

FND-PARTY-108 identifies the signed byte at DATA offset 19 hexadecimal
as one producer of the code passed to overlay 193 +166F. A fresh
directory-based census of all 323 DATA resources in the supported
RESOURCE.GFF gives the following domain partition:

| Signed-byte class | Resources |
|---|---:|
| Zero | 143 |
| Positive, within 1..112 | 172 |
| Negative, with magnitude within 1..112 | 7 |
| Positive 127 | 1 |

There are no missing bytes at that offset. The seven negative values
use five distinct signed codes, all with magnitude at most 104. The
sole positive code above 112 is in DATA/313, size 73. The approved
archive inspector locates it at decimal 96809; a separate hash-checked
bounded query confirms byte 19 is 127. The complete archive identity
is XXH3 92fe031616f36062f99484b0f728210e, size 5724669.

This is a shipped-source domain, not the domain of every acquired DATA
buffer or every runtime effect row. FND-COMBAT-031 separately records
the wrapper's low-byte sign distinction and its positive 1..127 gate,
which is wider than the 113-record effect-name table.

### The DATA-derived caller forwards the byte conditionally

In overlay 179 +0D2F, the reviewed +0E11 continuation requests DATA
for SI unless SI equals FFFF. It sign-extends byte 19 and stores the
word at BP-0A. The no-DATA branch sets that local word to zero.
The following code also prepares a separate duration local and calls
three overlay-197 helpers; their outputs can end this path before the
generic effect request.

After those gates, the +0ECF continuation requires SI not FFFF and
calls its saving-throw helper. A nonzero returned byte takes other
work and bypasses the generic insertion request. With zero returned
byte and nonzero local BP-0A, +0F24..+0F3E forwards the local's low
byte, DATA number SI, the two object arguments, sign-extended duration
and trailing zero to 573B:0039, entering overlay 193 +166F.
Its own instructions impose no 112 upper bound or code-127 replacement
between the field load and that argument load. Preservation and
non-aliasing of the local across intervening calls remain prerequisites.

The separate immediate-code-59 request for DATA number 313 on the
roll-20 path in FND-PARTY-096 is a distinct call site. It does not by
itself establish that this DATA-derived path replaces 127 with 59 or
that both paths are reached by the same admitted inputs.

### The reviewed handler default does not reject code 127 locally

FND-COMBAT-031 reads the application wrapper's DATA/removal/handler
sequence before insertion. Its handler is overlay 195 +0AF9. That
handler first requests bitmap preparation through 573B:0025, then
searches 19 word keys at local CS:0FB4. The independently bounded
table query recovers code 59 as a positive control and finds no 127.
Thus stable argument word 127 takes the unmatched-key jump to +0F1E.

On that local continuation, the successive code tests concern 35,
102, 20, 7, 21, 69, 91 and 66, all decimal. For 127 they bypass the
associated record/bitmap branches and reach +0FAE, which sets AL one
and returns far. There are no further child calls along that selected
default continuation after the initial bitmap preparation. This does
not admit the preparation's effects, current segments or mutable locals.

Consequently the presence of a handler gate is not a local proof that
the DATA-derived positive byte must become at most 112 before the
unchecked insertion body. A complete admitted path through the earlier
helpers and wrapper is still needed before claiming an actual stored
127, classifier/name access or visible original-game failure.

## Interpretation

This answers the shipped DATA-byte domain and the reviewed local
code-127 handler-default parts of Q-COMBAT-009. It supplies a concrete
source exception to a proposed blanket 0..112 producer assumption,
without declaring an out-of-domain runtime row or an original bug.

Remaining questions include actual DATA acquisition/selection,
input/callee and local-state admission, computed or aliased callers,
unchecked insertion entry points, restoration, and all count/ID/order
invariants. No gameplay behavior or parser was implemented and no
original function was executed.

## Alternatives

- The 113-record name table does not constrain every shipped producer
  byte: DATA/313 independently contains positive 127.
- The code-59 special call does not rewrite the generic caller's saved
  field byte in the instructions reviewed here.
- A default handler return of one is a local branch result, not proof
  that all preceding native/callee state admits insertion and display.

## How to reproduce

Use the locked evidence environment, the archive identity above and
DSUN.EXE size 634416, XXH3 e296af55ba2ecde7e77f555c90f33d0b.
Run data_bytes.py on RESOURCE.GFF with offset 19 hexadecimal and
decimal filter 127; its aggregate tally still covers every DATA resource.
Use the approved Inspect gff command to locate DATA/313. Independently
run report.mjs table with the archive hash, start 96809 decimal,
count/limit one, stride 73 and width-one effect_byte at offset 25 decimal.

Read overlay_listing.py descriptor 179, 650F1..65222; descriptor 195,
82269..822A8 and 8268E..82724. Bounds are hexadecimal and half-open.
Query the executable table at 82724 hexadecimal, count/limit 19,
stride two and width-two code at zero; the handler's CX assignment
supplies count evidence. Follow the unmatched jump and every later
comparison for code 127. Compare the application/insertion gate with
FND-COMBAT-031 and preserve the intervening-call admission gaps.
Local data-313-effect-byte and effect-handler-keys configurations and
original-derived output remain in GAME_DIR/analysis/combat-effects/.
No original bytes, disassembly or resource export is committed.
