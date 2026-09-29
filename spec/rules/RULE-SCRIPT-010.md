---
id: RULE-SCRIPT-010
title: Script-cache lookup and resource transfer
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SCRIPT-001, FND-SCRIPT-003, FND-SCRIPT-006, FND-SCRIPT-019, FND-SCRIPT-020, FND-SCRIPT-021, FND-SCRIPT-022, FND-SCRIPT-023, FND-CONFIG-151, FND-CONFIG-161, FND-CONFIG-162, FND-CONFIG-163, FND-CONFIG-164, FND-CONFIG-165, FND-CONFIG-166, FND-CONFIG-167, FND-CONFIG-168, FND-CONFIG-179, FND-CONFIG-170, FND-CONFIG-171, FND-CONFIG-172, FND-CONFIG-173, FND-CONFIG-174, FND-CONFIG-175, FND-CONFIG-176, FND-CONFIG-177, FND-CONFIG-178, FND-CONFIG-180, FND-CONFIG-181]
conflicting: []
split_with: []
related: [FMT-SCRIPT-001, RULE-SCRIPT-002]
---

## Summary

The loader can retain the current script, select a matching cache slot,
or request a GPL or MAS resource through the archive reader. These paths
have different age updates and failure effects. A transfer appends a stop
byte only after a zero reader result; a failed transfer does not locally
roll back bounds and ages already written.

## When it runs

The interpreter calls the loader when entering a script or returning to
a caller script (RULE-SCRIPT-002). The function's own early paths can
bypass cache scanning and resource I/O.

## Parameters

`load_script(number, selector)`: unsigned 16-bit resource number and
selector. The fresh-transfer path accepts selector one for `GPL ` and
two for `MAS `, and rejects number FFFF. The early current-pair and
matching-slot paths do not apply that validation independently.

## Inputs

`script_stopped`, `loaded_script_number`, `loaded_script_selector`,
`script_cache_numbers`, `script_cache_selectors`, `script_cache_starts`,
`script_cache_ends`, `script_cache_ages`, `script_buffer`,
`script_buffer_size`, and the fields read by `fn_172C_31ED`.
The archive reader's selected records and I/O results are also inputs;
the present archive filename alone does not determine them.

## Procedure

The signatures below name the entry points of the numbered procedure.
The numbered steps specify the loader and fill branches; the age loop is
shown explicitly because both call sites use the same operation. The
slot range is half-open: zero through 15.

```text
define load_script(number: UINT16, selector: UINT16) -> bool:
    # Steps 1, 2 and 9 specify the early returns, scan, fill and final updates.

define fill_script_slot(number: UINT16, selector: UINT16) -> bool:
    # Steps 3 through 8 specify selection, I/O, ordered writes and failure exits.

define age_script_slots():
    for slot in 0..16:
        if script_cache_ages[slot] >= 0 and script_cache_ages[slot] < 127:
            script_cache_ages[slot] = script_cache_ages[slot] + 1
```

The ordered branches and their conditions are specified below.

1. Return false when `script_stopped` equals one. Otherwise, return
   true when both requested words match the loaded current pair.
   These two paths do not age slots or call `fn_172C_31ED`.
2. On other paths call `fn_172C_31ED`, then inspect all 16 slots.
   Each number-and-selector match assigns the current number, selector
   and code start and resets that slot's age to zero. The scan does not
   stop at the first match or validate that the start differs from FFFF.
   The last matching slot supplies the final code start.
3. If no slot matched, try to fill a slot. Reject number FFFF or any
   selector other than one or two. Choose the first FFFF start, or use
   `fn_172C_07BB` when none exists. That helper selects the first slot
   with the largest signed age, writes its start, end and number to
   FFFF and age to minus one, and leaves its selector unchanged.
   If the selected slot already has the number requested, bypass the
   resource calls and slot initialization; the slot's selector is not
   part of this bypass test. Under unchanged valid state the replacement
   route cannot take this bypass, since its number is now FFFF and a
   requested FFFF number was rejected. The first-free route can.
4. Otherwise query the selected tag and number's length through the
   archive reader. Its length output is a double word. A nonzero result
   calls `fn_5702_00B1` and, if it returns, leaves the local fill result
   false. There is no own slot-bound write before this query succeeds.
5. Pass the length's low word plus one, in word arithmetic, to
   `fn_172C_0698`. Return false immediately when `script_stopped` is
   one afterwards. Otherwise assign the selected start and the end as
   start plus low length word plus one, also in word arithmetic.
6. Age all 16 slots whose signed age is zero through 126. Form a
   destination from `script_buffer` plus the allocated offset and ask
   the reader for the complete selected resource length. The transfer
   has no destination-capacity argument. This full recorded length is
   distinct from the low-word allocation arithmetic.
7. A nonzero transfer result calls `fn_5702_00B1` and, if it returns,
   exits the fill path false. No local rollback restores the earlier
   bounds or ages. A zero result appends byte 31 at the buffer start
   plus the allocated offset plus low length word, assigns the selected
   number and selector and resets that slot's age to zero.
8. The successful fresh fill and selected-number bypass assign the
   current number, selector and selected slot's start and return true.
9. After the scan or fill returns, a nonzero result updates the loaded
   current pair. Then the wrapper ages all signed ages from zero through
   126 and returns the retained result, including false results that
   reach this path normally.

The room-search helper `fn_172C_0698` starts with candidate zero and a
clear local found flag. It rejects a zero-extended word size not below
`script_buffer_size`, unsigned, by calling `fn_5702_00B1` and returning
zero if it returns. Otherwise it scans the 16 slots when candidate plus
size is signed-less than capacity and found is clear. It skips free
starts, unsigned ends below the candidate, and unsigned starts above
candidate plus size. A remaining slot collides: set candidate to its
end plus one in wrapped word arithmetic, zero-extend it, and restart
from slot zero. After reaching slot 16, mark found only when candidate
plus size is signed-less than capacity. Found returns the candidate's
low word. When found remains clear and the sum is not signed-less,
call `fn_172C_07BB` and retry at candidate zero if size is still
unsigned-below capacity; otherwise return the candidate's low word.
There is no own allocation, compaction or scan-restart bound
(FND-SCRIPT-022).

`fn_5702_00B1` first calls a shared helper. If that returns, it can
request the diagnostic message under its DS-relative byte gate. Its
common continuation then sets `script_stopped` to one, clears the
iterator flag and returns far. The shared helper has a polling loop
and multiple external dependencies before this continuation; the error
entry's final return does not prove those calls return. Its first local
callee installs a fallback callback before two more polls. The resident
setter has a prior guard route through interrupts and a mode-one runtime
path, so entering the setter does not prove registration completes
(FND-CONFIG-162, FND-CONFIG-163). The error entry makes no own cache
rollback (FND-SCRIPT-023, FND-CONFIG-161). The shared-helper loops'
common poll returns BX from interrupt 33h service three, and they test
its bit zero. Their two output pointers alias the same scratch word;
the later DX store overwrites CX and is not their loop predicate.
Actual driver results remain external (FND-CONFIG-164). The pointer
wrapper's normal return clears the supplied fields via returned zero;
that is not evidence of successful release (FND-CONFIG-165). Runtime
dispatch has a route whose local bounds rejection returns FFFF before
an interrupt, and that result is discarded by the outer returned zero.
Its DS restoration depends on a shared slot (FND-CONFIG-167). Another
state-clear/status pair can make the following poll return zero at its
entry under valid stable state. Changed state and active service effects
remain separate dependencies (FND-CONFIG-166). Three other caller fields
are cleared after a returning zero-selector consumer without testing its
result. That consumer has a conditional child-result exit after list/count changes;
its error helper adds another return dependency (FND-CONFIG-168). The
following mode helper requests callbacks and resources behind separate
state gates. Its resource results do not locally gate continuation, and
assigned output pointers do not establish valid content (FND-CONFIG-179).
Its local registration wrapper passes callback 28C9:0061 and mask 0166;
the first setter result does not guard the second request. Its earlier
bitmap helper does gate the second resource request on the first result
(FND-CONFIG-180). Its filename helper clears a stored archive handle
without a close-result test, then requests another open under near/far
segment and input conditions. A returned failure does not locally stop
the parent's continuation (FND-CONFIG-181).
The collector restores the retained consumer result on its local normal
return, with near-buffer and guard dependencies remaining (FND-CONFIG-170).
Following resident calls test callback fields, reload their targets after
intervening calls, and retain further register/state dependencies. Their
caller ignores the zero/FFFF result before the final fixed-segment word
writes (FND-CONFIG-171). MENU cleanup's FFFF branch only propagates
recursive FFFF; its finite valid leaves return zero, so that branch is
not itself an originating error. The EBOX result is ignored before the
caller's common child-pointer operation (FND-CONFIG-172). The intervening
list helper has signed count gates and several checked FFFF exits;
its caller ignores that returned result too (FND-CONFIG-173). The guarded
EBOX dependency also locally returns zero while discarding nested results;
its encoded error edges supply no own origin on valid normal returns
(FND-CONFIG-174). The callback bracket's copy/coordinate and shared-CS
pointer helpers preserve retained SI and DS on valid guard-bypass paths;
actual guards, aliases and target preservation remain open (FND-CONFIG-175).
Region append has a concrete count-16 FFFF origin, distinct from setup
helpers' normal zero/one returns (FND-CONFIG-176). Combining wrappers use
private outputs and later copies, with conditional pair expansion able to
reach that rejection. Their actual inputs, complete region effects and
success still remain open (FND-CONFIG-177). The larger region helper's
33 append sites all check private capacity errors before its final output
copy. The later same-pointer helper is a full-buffer self-copy, not
compaction; full geometry and actual inputs remain open (FND-CONFIG-178).

`fn_172C_31ED`'s reset is bounded in FND-SCRIPT-020. Cache-input
provenance, valid bounds and error-entry effects retain the dependencies
in Q-SCRIPT-003; this procedure does not assign their unknown outcomes.

## Outputs

The function returns a byte describing the local branch result and may
change the current pair, code start, cache bounds, identities, ages and
script-buffer bytes. True on a reuse branch does not independently
validate the cached contents or pointer. False after a read failure is
not a transactional-state guarantee.

## Edge cases

An unchanged current pair returns before either age loop. A matching
scan resets all matching ages; the wrapper then increments eligible ages.
A successful new transfer can run two age loops, with the selected slot
reset between them. Under valid unchanged state its final age is one.
A fill rejection or failed size query can still reach the wrapper's age
loop. Error-entry effects are separate from these local writes.

Low-word length increment and offset arithmetic can wrap. The reader's
full recorded transfer length need not equal the allocation argument.
No general capacity invariant or observed oversized-load consequence is
established here. A matching number in the chosen fill slot bypasses
loading regardless of its stored selector or free-start marker.

The allocator's end comparisons include equality, and a collision's
end-plus-one can wrap to zero and restart without progress. A capacity
with its high bit set also differs between its unsigned size checks and
signed room checks. FND-SCRIPT-022's conditional repeated-state examples
do not establish that ordinary game inputs reach them. A returned zero
is not a separate allocator-failure signal; the fill caller checks the
stop byte. Cache writers and error handling remain necessary evidence.

The appended 31 byte is the stop instruction (RULE-SCRIPT-003).

## What the sources say

SRC-OPENDS-5C6CBD7 (`docs/gpl-bytecode.md`, section 1) places GPL and MAS
scripts in GPLDATA.GFF. The installed corpus is recorded in
FND-SCRIPT-001. The runtime reader uses registered archive state,
so this outside description does not replace the selection conditions.

## Differences between builds

None known.

## Open questions

- Which cache and capacity state reaches `fn_172C_07BB`'s selection and
  `fn_172C_0698`'s search, and which shared-helper/message effects and
  returns precede `fn_5702_00B1`'s stop assignment (Q-SCRIPT-003).
  One reading supplies valid distinct buffers and sufficient space;
  another reaches changed state, aliasing or failure. Their full
  callers, pointer producers and error paths distinguish them.
- Which registered archive and record the resource calls select, whether
  the record stays stable between size and read requests, and whether
  I/O succeeds (Q-SCRIPT-003). A source catalog supplies a possible
  record, not the native outcome of every request.
- Whether duplicate matching or selected-number slots, including a slot
  of the other selector, can occur at an ordinary invocation
  (Q-SCRIPT-003). Cache writers and replacement paths would settle it.
- The bounds and semantic identities of the working buffers cleared by
  `fn_172C_31ED`, and their other reset timing (Q-SCRIPT-005).
- Resident emulated-call cases for the local early, scan and validation
  branches after the harness exists (Q-SCRIPT-007). They cannot establish
  operating-system I/O outcomes or replace complete input provenance.
