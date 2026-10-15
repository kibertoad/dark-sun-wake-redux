---
id: FND-COMBAT-036
title: The status selector separates a 114-byte output span from its bitmap and clamps only after collection
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A4BC..0x0007A5A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064105..0x00064127
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FFF0..0x000800A0
tool: tools/research/exec-census/direct_callers.py and overlay_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

Exported overlay 190 entry +2190 (571F:0093) reserves 132 local bytes
and saves SI/DI below them. Its three argument words are owner,
ordinal and a low-byte category-selection flag. The status wrapper
+217C (571F:0052) supplies owner, ordinal zero and flag one, as
identified by FND-COMBAT-029. Reading the complete local selector
body supplies the frame relationships left open in Q-COMBAT-012.

All offsets in the following table are relative to that selector's
own BP, before entering the helpers. Spans are half-open:

| Span | Local use |
|---|---|
| BP-132 .. BP-18 | 114 bytes before the bitmap start; output begins at BP-132. |
| BP-18 .. BP-2 | 16 bytes reserved before the flag; bitmap begins at BP-18. |
| BP-2 | Swap flag, initialized to one before preparation. |
| BP-1 | Temporary byte during an adjacent output swap. |

The selector forms both helper arguments with LEA from BP and pushes
only their near offsets. Preparation +1BA3 clears 15 bytes through
DS:bitmap; for admitted IDs 0..112 its OR writes remain in those same
15 bytes. Thus BP-3, the sixteenth reserved bitmap byte, is not cleared
by this call. Collection +1D00 reads the bitmap through DS and appends
output bytes through DS. It admits at most 113 output bytes per call,
without a terminator (FND-COMBAT-029).

The selector's sort accesses its output through BP+SI, hence SS by
default. Its final selected-byte load constructs BX from the output
offset plus ordinal and uses DS. None of these local bodies assigns
DS equal to SS. Their matching near offsets therefore do not alone
identify the helpers' DS storage with the selector's SS locals.
FND-PARTY-215 records a conditional startup producer of that segment
relationship; it does not establish it on this later call path.

### The status wrapper's single collection

The first collection always requests category zero, with output at
BP-132 and bitmap at BP-18. For the status wrapper's flag one, the
second collection is skipped. Assuming admitted matching DS/SS,
nonwrapping frame storage, IDs 0..112 and the helper's other row/count
prerequisites, its at-most-113 appended bytes occupy
BP-132 .. BP-19. They do not reach the bitmap at BP-18 or the swap
flag at BP-2. One byte at BP-19 remains outside that maximum output.

After collection, the selector takes the lesser of the returned count
and 35 using signed word comparisons. It returns zero for a zero
clamped count, and otherwise accepts an ordinal only when it is signed
less than that count. The status wrapper's ordinal zero passes for a
positive count. The existing sort and key behavior remain those of
FND-COMBAT-022/029; this finding does not replace their key table.

### The exported two-category path has a separate bound

With flag zero, the selector instead makes a second +1D00 call for
category one. It reuses the original bitmap offset and supplies output
offset BP-132 + first_count. It adds the second returned count to the
first, then applies the same 35-element clamp. It does not pass the
remaining size of the local output span into the second collector.

Each collector's local cap remains 113, not 35. Consequently the two
local caps alone permit an aggregate upper bound of 226 appended
bytes, before considering admitted rows, IDs, categories and bitmap
contents. The interval calculation is separate from a claim that any
native call attains that bound. If actual aggregate writes exceed
114 under matching DS/SS and nonwrapping offsets, they reach the
bitmap area: aggregate output index 114 addresses its first byte.
Later bitmap reads can then depend on earlier output writes. The
clamp occurs after both calls and cannot establish prior separation.
No original overlong list or displayed defect was observed here.

The direct-call candidate query for +2190 also finds a far call in
overlay 178 at +106F. In the listed local context it pushes flag zero,
the previous value of its local ordinal word (then increments that
word), and its DI owner. This qualifies an actual encoded caller of
the two-category interface, not a completed proof of its reached row
counts, ordinal range or buffer lifetime. Its containing inventory
entry begins at file 63FCF; the rest of that caller and any computed
calls remain admission work.

### Ordinal and lifetime limits

The selector has no local nonnegative-ordinal test. Its signed
ordinal-less-than-count branch is not a complete 0..count-1 domain
check. Negative ordinals or changed segment/frame state therefore
cannot be admitted from this local comparison. Neither known caller
is claimed here to supply a negative ordinal.

The selector owns the local frame from its prologue through LEAVE;
the helper frames have their own BP and caller-cleaned argument slots.
They receive the selector's offsets as values, not pointers into their
own local frames. This resolves which frame supplies the buffers, but
stack capacity, mapped DS/SS identity, original table separation and
preservation across every relevant callee still need admission.

## Interpretation

This answers the exact local-frame extent, helper argument provenance,
segment uses and clamp ordering in Q-COMBAT-012. A 35-element result
limit is not an output-allocation bound. The status wrapper's flag-one
path has a conditional local non-overlap argument that the exported
flag-zero interface does not acquire from its clamp.

It does not raise RULE-COMBAT-009 to established. Q-COMBAT-009 retains
row/count/ID admission; Q-COMBAT-011 retains bitmap producers and
lifetimes. Q-COMBAT-012 retains full caller coverage, DS/SS and stack
admission, callee preservation and the two-category aggregate bound.

## Alternatives

- Calling the output span 113 bytes conflates the collector's cap with
  the 114-byte distance between output and bitmap starts.
- Calling the bitmap clear 16 bytes conflates its reserved span with
  the actual 15-byte fill and admitted ID range.
- Equal near offsets do not prove DS/SS identity. The sort's SS access
  and final DS selection must agree with the helpers on an admitted run.
- The arithmetic overlap case is a local contract obligation, not a
  native experiment, new gameplay behavior or claim of a game defect.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run direct_callers.py for 190+2190 and 190+217C. Read overlay_listing.py
descriptor 190, 7A4BC..7A5A7; descriptor 178, 64105..64127; descriptor
193, 7FE93..7FF07 and 7FFF0..800A0. Bounds are hexadecimal and
half-open. Qualify the status wrapper and exported selector rather than
decoding the subsequent key-function jump table as ordinary code.

Track the selector's BP across each outgoing call and distinguish
DS:[BX] from default SS:[BP+SI]. Check the two collection destinations,
per-call cap, later clamp and ordinal comparison. The synthetic span
checks are arithmetic: 132-18 = 114; 113 bytes end before output index
113, and index 114 is the bitmap start. They are not licensed-data
fixtures or original observations. Keep listings in
GAME_DIR/analysis/combat-effects/. No original function was executed.
