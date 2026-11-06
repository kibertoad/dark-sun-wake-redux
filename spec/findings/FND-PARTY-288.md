---
id: FND-PARTY-288
title: Reflected-cast queue routes distinguish literal 307, a tested spell interval and an unbounded forwarded number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B60..0x00084E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654BC..0x000655E4
tool: tools/research/exec-census/overlay_listing.py at afd293c6; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-MAGIC-020 and FND-PARTY-286/287
environment: null
---

## Observation

Q-PARTY-134 retains actual number domains and preservation for the
three word-input clients. FND-MAGIC-020 already identifies reflected
casts and the shared queue. Installed DSUN.EXE is 634416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`. Offsets below are
hexadecimal; 1001 and the number 307 are decimal.

### Three producer paths are not one domain

Overlay 197 +1760 saves the incoming number BP+12 in DI at +176B.
The shared queue call at +180F is reached with these number sources:

| Route | Number domain and preservation scope |
|---|---|
| Fallthrough from +1808 | Literal 0133 (307) is pushed at +180A. No child call lies between that push and +180F. This producer excludes 1001. |
| Effect-block byte +0A mask 20 route | +186E..+1876 rejects signed DI below zero or at least 235. Subsequent tests and calls precede pushing DI at +18CE. Under retained DI, the number is 0..234 and excludes 1001. |
| Effect-block byte +0A mask 10 route | After record and caster tests, +1857 can jump to +18CA. This path has no corresponding 0..234 number gate; it forwards current DI at +18CE. |

The bounded mask-20 route reaches overlay 177 +09C5 twice, the
shared getter 2C5F:0003 and overlay 193 +1AA4 before the final
push. Its post-test DI preservation is therefore a named obligation,
including these reached calls and saved-frame/alias effects. The
interval test alone does not prove the final pushed value survives.

The mask-10 route calls overlay 208 +0885 with the number and
literal one; a returned signed word at most eight admits the queue
branch. That is a helper-result bound, not an independent numeric
bound on DI. Its incoming producer and DI/frame preservation remain
open; the mask-20 interval must not be applied to this alternative.

### Enqueue field and the runner's actual read

Overlay 179 +12B1 reads its second argument into DX and tests the
signed queue-index byte DS:0915 against five. On the less-than-five
branch it increments that byte, sign-extends it, multiplies by record
size 0E and forms a record pointer in segment 4DA1. It writes its
first argument to record +0, second to +2, third number argument
BP+0A to word +4, and the low byte of its fourth argument to +0C.
There is no child call on this publication branch. Valid queue index,
record/frame backing and distinct storage remain admission conditions.

On the other branch it recursively calls +12B1 with literal number
307 and level six. It does not first decrement the same queue byte
or independently create room. No termination or successful publication
is inferred for that branch; its literal producer still does not
generate 1001. This reading does not substitute an intended full-queue
error or discard behavior for the encoded recursive request.

Runner +11DC forms its record from current DS:0915, loads word +4
at +1206 and saves it to SS:BP-2. It separately reads kind word
4DA1:(0054 + 2 * sign-extended index). Kind zero pushes the saved
number at +1255 into overlay 193 +003C at +125C. Kind one instead
pushes the saved number at +1291 into the hit routine at +1299.
Other kinds skip these calls. The enqueue routine above does not
write that separate kind field; its provenance cannot be inferred
from this enqueue call.

There is no child call between the runner's number load and its
outgoing push on either selected branch. Retained queue contents,
index/kind state, frame and segment identity and disjoint record/global
reads remain conditions. Later processing retains the caller/client
preservation obligations of Q-PARTY-134.

## Interpretation

Two producer-domain components are settled: the literal route generates
307, and the mask-20 test admits only 0..234 before the named reached
calls. Neither intrinsically supplies positive 1001. Their queue and
later-call preservation remains explicit. The mask-10 route retains
the actual incoming number question. FND-PARTY-287's separate signed
script-remainder bound remains unchanged.

This does not establish all queue producers, writers, kinds or native
queue lifetime, and excludes no whole getter site. Eighty sites under
Q-PARTY-124 plus three under Q-PARTY-134 remain 83 total, with repeated
equipment-hook preservation and indirect/bypass inputs. FMT-PARTY-007
stays supported; no question closes or complete reading is declared.

## Alternatives

- A bound on one reflection branch is not a bound on the other branch.
- Literal production and publication are separate: full-queue recursion
  need not reach a record store.
- Writing the queue's number field does not establish its separate kind
  or guarantee which consumer branch runs.

## How to reproduce

Run overlay_listing.py on the fingerprinted executable for descriptor
197 at 84B60..84E94 and descriptor 179 at 654BC..655E4. File bounds
are hexadecimal and half-open. Reports reflected-cast-producers.txt
and reflected-cast-queue.txt remain under
GAME_DIR/analysis/class-table-closure-20261106/. The exploratory queue
report extends into the next routine; use the exact two-entry range
above. Follow each incoming path to +180F, the actual +12B1 number
store, and both runner number pushes. No original game ran.
