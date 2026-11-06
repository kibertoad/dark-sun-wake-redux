---
id: FND-PARTY-336
title: Queue drain decrements the post-client index and the cast client has a separate post-hit lifetime
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065528..0x00065591
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E795..0x0007E907
tool: tools/research/exec-census/overlay_listing.py at c693a7e8 and 07ea3d35; locked Python 3.12.10 with Capstone distribution 5.0.9; existing FND-PARTY-288/328/332/335 and FND-MAGIC-018/019/020
environment: null
---

## Observation

The first reflected handoff and later/reentrant queue processing are
independent lifetimes. Q-PARTY-137 retains the former under the named
179+0C7F root. Q-PARTY-138 preserves other roots, client effects and
later iterations. This split transfers unanswered scope; it closes no item.

### Post-client transition

Runner 179+11DC forwards its saved number to 193+003C for kind zero
or to hit +0D2F for kind one (FND-PARTY-288/332). After either ordinary
client return, it removes the outgoing arguments and reaches +129F.
That instruction decrements the current byte DS:0915. It does not restore
or decrement a saved pre-call index. The following signed comparison
continues only when the resulting byte is greater than -1. Continuing
re-enters the record-pointer and number-read prefix with the global byte.
Other kind values skip the calls and reach this same decrement.

Let j be the byte value immediately before that decrement, after any
client effects. The next byte is (j - 1) modulo 256. A nonnegative signed
result selects the next iteration; a negative result ends this invocation.
Under unchanged gates, returning frames and no further intervening writes:

| Client-return state | Local next action |
|---|---|
| j is the retained entry index i in 0..5 | Continue with i-1, or finish at -1 for i=0. |
| Client appended one entry and returns with j=i+1, i in 0..4 | Continue at i: the previously selected record index, not the new top index i+1. |
| Client returns after a nested drain with j=-1 | Decrement to -2 and finish, not to the canonical initial -1 value. |
| j=-128 | Byte decrement wraps to 127, which passes the signed nonnegative loop test; there is no upper-capacity check in this runner prefix. |

These are conditional byte/control-flow consequences. They do not prove
that a shipped client actually returns with any particular changed index,
that the previously selected record is unchanged, or that repeated processing
terminates. Returning with i+1 also does not prove the appended record is
permanently lost: subsequent publication, overwrites or nested processing
need their own interval evidence.

FND-PARTY-328's enqueue admits a signed index below five without a lower
bound. Thus an admitted later enqueue from the conditional -2 state would
increment to -1 and form a wrapped record offset. This names a concrete
state-producer question; no reached native malformed queue or defect is
established by the arithmetic alone.

### Connected returning reflection route

The changed-index case has a concrete encoded producer, not only an
arithmetic example. Reuse FND-PARTY-288/328/332: the kind-one client is
hit +0D2F, whose admitted reflection path calls 197+1760. Its successful
shared enqueue calls 179+12B1 at 197+180F. If the queue byte is the
runner's selected i in 0..4 when that enqueue begins, the non-full branch
increments it to i+1 and publishes into record i+1. Under the stated
record/frame/segment and no-intervening-writer conditions, the enqueue
returns with that byte unchanged.

The reflection caller cleans its arguments, sets AL0 and jumps directly
to its +1A90 epilogue. The hit caller tests that zero result at +0EA6 and
jumps to its +11D5 epilogue. This is the same short hit continuation read
in FND-PARTY-332, but its immediate caller here is the drain's kind-one
call, not the +0C7F wrapper. Consequently the client returns directly to
the drain's argument cleanup and live index decrement: i+1 becomes i,
and the next iteration reloads record i. No encoded game-helper call or
nested drain occurs between this successful enqueue and that decrement
on this selected path. Overlay return machinery, asynchronous effects
and actual backing retain their existing conditions.

This settles the encoded returning-enqueue route for that admitted
reflection branch. It does not establish its actual inputs, all effects
before enqueue, an unchanged selected record/kind, repeated reflection,
termination, or permanent omission of record i+1. In particular, enqueue
does not initialize the separate kind table. Other client paths and
nested drains remain Q-PARTY-138 obligations. The initial i=5 full-queue
case cannot use this successful-publication argument.

### Two roots and a longer kind-zero client

Wrapper 193+0000 forwards its arguments to +003C, retains the result
byte, then calls 179+11DC at +0032 before returning the retained byte.
The queue runner instead calls +003C directly. It does not automatically
invoke the wrapper's post-call drain merely because the client shares
that implementation.

The kind-zero client's hit call is at 193+04DE. Its post-hit continuation
runs through +0617, not an immediate epilogue. It accumulates the hit
result, can enter combat-state-dependent calls to 28C9:2E0E and
1695:02AD, advances its target loop and may process another hit. Its
later branch can gather another target list through 208+0000 and invoke
371D:050F for its entries. A result/caster gate can call 195+09A7.
These reached interfaces and their actual effects cannot be replaced
with the hit wrapper's no-additional-child return interval.

The queue's own kind-zero argument setup supplies a null dword for
+003C's optional pointer at BP+16 (FND-PARTY-332). At the tail's +05DF
zero test, an intact unchanged argument therefore skips its final pointed-
byte increment/reset. Other callers or changed argument frames retain
that output writer. The remaining target, flag, loop and child effects
are not excluded by this null argument.

## Interpretation

The runner is not a simple pop of a captured index across its client.
Its next selection consumes the client's final global-index state. Actual
client-side enqueue or nested drain therefore needs a connected scheduler
reading, distinct from preserving the number before the first handoff.
The kind-zero client has concrete post-hit calls and loops; it cannot
inherit FND-PARTY-332's short return path for the 179+0C7F wrapper.

Q-PARTY-138 retains admission of the identified reflection route, other
index-changing client routes, nested drain
entry/return, selected record/kind/frame and child/native/asynchronous
bindings. The conditional i+1 and -1 outcomes identify what to seek in
those routes. Q-PARTY-137 retains its actual first-handoff admission.
Neither question is answered by splitting it, and no complete reading,
observed bug or status promotion is declared.

## Alternatives

- The runner always pops the record index it selected before calling:
  contradicted by its live global-byte decrement.
- Calling 193+003C is equivalent to calling the 193+0000 wrapper:
  only the wrapper has its explicit post-call drain.
- A reflection's short hit return makes every cast client return directly:
  contradicted by the kind-zero client's target loop and post-hit tail.
- A conditional changed-index example proves a reachable queue defect:
  actual admission, nested-call and state-preservation evidence is
  still needed; no such observation occurred here.

## How to reproduce

Use licensed DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, locked evidence Python and
`tools/research/exec-census/overlay_listing.py`. Read descriptor 179
`65528 65591`, descriptor 193 `7E2F0 7E32C` and `7E795 7E907`.
The existing reader checks source identity and resolves fixups. The
committed inventory places the full +003C body at 7E32C..7E907;
this finding reads its named late portion, not its entire earlier body.

Trace both returning runner branches into the same decrement and signed
test, and derive the table at byte width. Compare the wrapper's explicit
drain with the client's own post-hit loop/return. Reuse FND-PARTY-332 for
the null optional argument and first-handoff contract, and the cited cast
findings for the known earlier hit/target interfaces. Do not model unknown
children as harmless or imply that the conditional scheduler states were
executed. For the connected reflection route, reuse FND-PARTY-332's
established instruction boundaries and read descriptor 179 `65591 655E4`,
`6517E 6518D`, `654B5 654E5`, and descriptor 197 `84C0F 84C1C`,
`84E90 84E94`. Do not start the hit caller at 65160: it is inside an
instruction, so a linear decode there supplies no entry or path evidence.
No original game ran and no source-derived listing is committed.
