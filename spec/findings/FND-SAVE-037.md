---
id: FND-SAVE-037
title: A zero numbered-writer result bypasses explicit Save's close and copy before the ordinary failure-message continuation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070BFD..0x00070CEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D6FB..0x0007D8ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA85..0x0007E014
tool: overlay_listing.py at 63d66222; existing FND-SAVE-004/018/021/023/035 and FND-COMBAT-084/096/110/113/121/122 interfaces
environment: null
---

## Observation

This answers RULE-SAVE-001/Q-SAVE-007's local nonfatal Save-failure
cleanup question. The complete established wrapper and its immediate
Save-screen consumer were read together. Source: installed DSUN.EXE,
634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
Overlay187 offsets below are relative to file6FE30; overlay192 to7D300.

### Writer failure branches around cleanup, not through it

Overlay187 +0DCD initializes result byte BP-2 to1, calls working-open
helper+1BE7 and saves its returned AL in BP-1. It then calls the
numbered resource writer+08E3 with selector0 and the description pointer.
After argument cleanup, +0E5D tests returned AL. Zero jumps directly
to+0EB5, sets result byte0 and returns that byte through AL.

That branch bypasses the entire saved-BP-1 test, local close helper+1C26,
path construction and numbered-file copy. Even if this wrapper opened
the working archive, it makes no own close request after writer AL0.
The close obligation is not discharged by an unconditional cleanup tail.

Nonzero writer AL instead tests saved BP-1. Nonzero ownership calls
+1C26 and stores its AL in BP-2; zero ownership skips that close and
keeps result byte1. A nonzero result byte reaches4544:0000's numbered
copy; its AL replaces BP-2. Zero skips the copy. These are byte tests,
not independent checks of full AX or every preceding operation.

| Returning continuation | Wrapper's own close/copy behavior |
| --- | --- |
| Writer AL0, either saved ownership byte | Skip close and copy; return AL0 |
| Writer AL nonzero, saved ownership0 | Skip close; attempt numbered copy |
| Writer AL nonzero, saved ownership nonzero | Call close helper; attempt copy only for its nonzero AL |

FND-COMBAT-084/110 qualify ownership: already-open DS:167A makes
the opener return AL0; successful new opening uses option2, without
the write-through bit of FND-SAVE-035. A failed close invokes the shared
fatal diagnostic. FND-CONFIG-224 excludes its ordinary return under
fixed preserved exit arguments/frame. The encoded close-AL0 continuation
therefore does not establish ordinary recovery from a native close error.

### The failure message is followed by an exit-mark test

Overlay192 +03FB zero-extends the wrapper's AL and tests it. Zero
pushes the failed-save text and jumps at+055C to the shared+05E2 call
of overlay172 +05DB, the ordinary message-window entry. This is distinct
from overlay180 +0867's fatal diagnostic. It bypasses the CHARSAVE
opener and subsequent PREF/GREQ operations on the successful arm.
FND-EXE-642 corrects FND-EXE-621's wrong message-target description.

After the message call's returning continuation, +03FB returns to the
Save-screen callback. That callback has no test of the Save routine's
AX/AL before releasing its record buffer and testing DS:627C at+0C0F.
Nonzero mark clears DS:1462 and selects its exit return; zero enters
the existing play-view reconstruction continuation (FND-SAVE-018/025).
A failed attempt is not independently excluded from this mark test.
Actual message return, release and later reconstruction still require
their own admitted children and storage; this is not a claim that every
message or failed save returns to play.

### Pending metadata remains a separate interval question

FND-COMBAT-121/122 and FND-SAVE-035 establish that a failed payload
transfer can publish pending1 and bypass the writer's optional preparation.
If the numbered writer then returns AL0, the wrapper above adds no close
before its ordinary failure-message continuation. This supplies a concrete
local route to investigate, unlike the fatal region-failure branch.

It does not prove pending1 survives to Quit. Reverse conversion and
bitmap cleanup occur before the numbered writer returns (FND-SAVE-021/023),
and message/callback children occur afterward. They may affect archive
selection, preparation, backing or the flag. Initial allocation/selection
failures can also return AL0 without this attempt publishing dirty metadata.
Actual failed transfer, same-record lifetime, preservation through those
named intervals and later shutdown handle/file binding remain open.

The reused-open successful branch's copy before a wrapper close is a
different saved-file contract; its actual preexisting-open and pending
state belongs to RULE-SAVE-002/Q-SAVE-008 rather than extending this batch.

## Interpretation

The explicit Save wrapper does not guarantee close after a failed writer.
The caller's failed-save message is an ordinary message request, and the
later exit-mark consumer does not test save success. These local branch
obligations are answered for Q-SAVE-007. No native failure, surviving dirty
record, failed-save defect, complete reading or parity promotion is asserted.

## Alternatives

- A close-on-every-opened-return reading is ruled out by the AL0 jump.
- A failed-save message is not a call to the CHARSAVE opener or fatal entry.
- Writer AL0 does not itself prove a new dirty record or successful rollback.
- A failed Save need not bypass the caller's exit-mark consumer.
- Skipping this close does not exclude preparation inside another child.

## How to reproduce

Use the locked evidence Python and overlay_listing.py for descriptor187,
70BFD 70CEE; descriptor192, 7D6FB 7D8ED and7DA85 7E014. Retain
save-wrapper-error-contract.txt, save-wrapper-result-consumer.txt and
save-failure-screen-continuation.txt under
GAME_DIR/analysis/handler-census-20261106/. Follow the writer-AL0 jump,
two byte locals, close/copy gates, failure-message jump and callback's
post-return buffer release/exit-mark test. Reuse the cited child contracts;
do not infer native failures from synthetic control flow. No original
listing or game content is committed and no original function was executed.
