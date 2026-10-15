---
id: FND-COMBAT-039
title: Effect-frame actions use cached IDs for labels and a first matching row's key for removal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A727..0x0008A837
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A3F..0x00068A7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AB13..0x0008AC6F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AFAD..0x0008B12B
tool: tools/research/exec-census/trampoline_target.py, direct_callers.py and overlay_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The effect-icon cache built by overlay 202 +07CF has a separately
exported frame handler at +0673, trampoline 577D:0034. The local
setup path in +0287 attempts to register that callback for identifiers
2BCD..2BEF inclusive, 35 successive values, using the window pointer
at DS:2690 and mask 0066. It calls overlay 182 +01EF once per value
without inspecting its returned AX.

The registration wrapper's actual argument layout is window far pointer,
identifier word, callback far pointer and mask word. For a nonnull
window it forwards the callback to 3F96:02AB and then requests operation
one with the mask through 3F96:02F8. A null window bypasses both calls.
FND-CONFIG-180 and FND-UI-007 separately read those setters and retain
lookup/guard/preservation dependencies. This is a registration attempt,
not proof of 35 existing frame records or successful native dispatch.
Before its setup calls, +0287 copies its owner argument word at BP+0E
hexadecimal into 4E71:0B44. There is no own owner-domain check before
that store; its admitted callers remain required.

The handler reads an identifier word at BP+8 and an event-value word
at BP+0A hexadecimal. It subtracts 2BCD in word arithmetic to obtain
the slot index. It first requests the frame's local coordinate record
through 3F96:01A6 using global window DS:1431, then applies its count
gates. That helper's returned AX is not tested; FND-COMBAT-038 records
its zero/default output and lookup-dependent success branches.

The handler requires signed (DS:9B75 + DS:9B71) > 0 and signed slot
index below that sum. The addition is a word operation. No local
nonnegative-index check precedes these comparisons. A successful
registration/dispatch through exactly the attempted identifier range
would supply indices 0..34, but this does not admit every exported
caller, count value or native dispatcher state.

### Numeric event branches use different cache information

For a passing index, the local branches are:

| Event value (decimal) | Local continuation |
|---|---|
| 2 | Request background-image frame eight, then a label from the effect cache or extra-flag name helper. |
| 4 | Request background-image frame five, then forward fixed DS:26A6 as the text source. |
| 32 | Request background-image frame five, conditionally invoke an action, then request a window refresh. |
| Other | Return zero without one of these local continuations. |

The image calls go through overlay 190 +0000 with the pointer at
4E71:002F and frame coordinates plus window fields +96/+98 hexadecimal.
Their results do not select these subsequent branches. Event meanings
are not assigned here: RULE-UI-001 retains their native interpretation
under Q-UI-001. The registration mask also includes 64, for which this
handler has no listed action branch.

For event two and index below DS:9B75, the label source is the name
of the cached effect ID: sign-extended byte at DS:9B0A + 3*index,
multiplied by 31, added to 4C87:0000. That pointer and identifier 2C06
are forwarded to overlay 190 +06B4 with the window. There is no ICON
request-result test or cached image-pointer test in this label branch.
For an index outside the effect portion, the handler maps the index
through +0B0D, passes the result to overlay 176 +0AF8 and forwards
that helper's returned DX:AX as its text pointer. Its text contents
and successful display remain dependencies.

For event 32 (0x20), an index below DS:9B73 bypasses both action modes.
An index at least that first-count boundary but below DS:9B75 invokes
+0B66 with mode zero; an index at least DS:9B75 invokes it with mode
one. After either call, or after the first-count bypass, the handler
requests 3A8E:060D with the window pointer. It does not inspect the
action return before requesting this refresh. Every handler exit
explicitly returns AX = 0.

### Mode zero searches the active table for a key

Exported +0B66, trampoline 577D:003E, reads a low-byte mode and a word
index. Mode values other than zero or one leave through its epilogue.
There is no own index/count validation before mode zero reads the
effect byte at DS:9B0A + 3*index.

Mode zero sign-extends that cached byte and scans the ten-byte table
at 4E71:024D from row zero. Its loop tests signed row index against
DS:2100 before entering the body. It compares each sign-extended row
effect byte at +6 with the cached ID, then the owner word at +0 with
4E71:0B44. The first matching row ends the scan. A missing match exits
without the later key-removal call.

This is a fresh linear ID/owner search, not a stored row index or key
in the three-byte cache. For duplicate matching effects it chooses
the first matching active row in current table order. It does not
recover a distinct duplicate instance from the ordinal of a cache slot.
Count, table, cache lifetime and current owner must still be admitted.

For a found row, it tests the DATA-number word at +4 using signed
comparisons: 235 <= number < 269. When that passes and the second-object
word at +2 equals the current owner, it clears bit zero at:

```text
4E71:09CD + 34*owner + (DATA_number - 235)
```

The encoded expression uses base 08E2 plus 34*owner plus DATA number;
the expression above is the same byte address in that range. Other
bits are retained. The meaning and admitted lifetime of these 34-byte
records are not established merely by this address correspondence.

Whether or not that conditional bit clear occurred, the function reads
the found row's byte seven, pushes its low byte as the owner-local key,
and calls 573B:0048 with current owner. FND-COMBAT-032 reads that key
wrapper and its later handler/removal calls. The bit clear precedes the
request and has no own rollback branch based on the returned result.
This caller does not pass the cached effect ID as the removal key.

### Mode one maps an extra ordinal to a flag byte

Mode one calls exported helper +0B0D (577D:002A) with the supplied
index. That helper starts a word counter at index + 1 - DS:9B75,
then scans candidate offsets 24..33 in increasing order. It excludes
25, 26, 29, 31 and 32, and decrements its counter only when bit zero
is set in the current owner's byte at 4E71:09CD + 34*owner + offset.
The loop stops when the counter is zero or the candidate reaches 34,
and returns candidate minus one.

For admitted stable counts/flags and a valid ordinal in the extra
portion, this maps in order over enabled offsets 24, 27, 28, 30, 33.
The helper itself supplies no distinct not-found result: exhaustion
returns 33, while an initial counter of zero returns 23. These are
local branches, not claims that the frame handler reaches invalid
ordinals or that any native flag is cleared incorrectly.

After the helper returns, mode one clears bit zero at the returned
offset in that owner's 34-byte record. It does not invoke the active
effect-row key wrapper. Neither action mode explicitly normalizes AX
to a success flag on return; the event handler ignores that register.

### Request failure does not itself disable cache interaction

FND-COMBAT-038 records that a nonzero ICON number leaving a null
resource pointer retains the cache slot and count. This handler's
label/action gates read those IDs and counts, not a per-slot resource
success field. A retained slot therefore is not excluded by an own
reader-success check in these branches. Successful native dispatch,
admitted state and later window behavior remain separate questions.

The first-count boundary also remains a separately stored word.
FND-COMBAT-038's zero-icon compaction decrements total DS:9B75 but
does not decrement DS:9B73. As a synthetic composition example,
start with one first-category zero-icon entry followed by one kept
second-category entry. Compaction leaves first-count one, total one
and the kept entry at index zero. Event 32 at index zero then takes
the first-count bypass. This checks the combination of local branches;
it is not a native fixture, encounter or observed UI defect.

## Interpretation

This answers the local post-draw cache label/action contract in
Q-COMBAT-010 and qualifies another owner/key-removal caller relevant
to Q-COMBAT-009. Rendering success is not its action-admission field;
cached effect ID, stored count boundaries and current active rows
drive the reviewed branches.

It does not finish native callback/graph admission, event meanings,
resource validity, state synchronization, text/draw/refresh outcomes
or the removal callees' effects. Q-COMBAT-010 retains those cache and
presentation obligations, Q-COMBAT-009 the active row/key state, and
Q-UI-001 the event interpretation. No gameplay rule or UI action was
implemented and no original run was made.

## Alternatives

- A frame index is not the owner-local key, nor does it distinguish
  duplicate cached IDs in the fresh first-match active-row search.
- Mode one clears a flag through an ordinal map; it is not the same
  request as mode zero's active-row key removal.
- A null ICON pointer is not an own gate in the later label/action
  handler, and total-count compaction does not update first-count here.
- Mask registration and numeric event branches do not establish what
  physical input generates them or prove that a frame was installed.
- Conditional bit clearing before a removal request does not prove
  successful removal, rollback or a native defect on failure.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Resolve 577D:0034 and 56BD:004D with trampoline_target.py. Run
direct_callers.py for 202+0673, 202+0B0D and 202+0B66. Read
overlay_listing.py descriptor 202, 8A727..8A837, 8AB13..8AC6F and
8AFAD..8B12B; descriptor 182, 68A3F..68A7D. Bounds are hexadecimal
and half-open. The last range includes the one-byte far return.

Track registration argument widths and ignored setter results; the
identifier subtraction, signed count gates and numeric event branches;
cache name pointers, first-match ID/owner scan, DATA/second-object
bit-clear guard, key-byte forwarding and the extra-ordinal counter.
Compare producer/count behavior with FND-COMBAT-038, the key callee
with FND-COMBAT-032 and registration forwarding with FND-CONFIG-180.
Keep original-derived listings in GAME_DIR/analysis/combat-effects/.
