---
id: FND-SOUND-042
title: Setup stream construction failure closes the handle and clears active fields while exhausted lookup can inspect two extra slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:27B6..1000:27FC
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2873..1000:292B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2AF6..1000:2C46
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:37F1..1000:390E
tool: tools/research/exec-census/resident_listing.py at 6d7e5d56; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

Q-CONFIG-011 retains exhausted-slot and construction-failure cleanup
after FND-SOUND-033's successful binary-open reading. This reads those
local branches and the direct close helper. Addresses, masks and field
offsets are hexadecimal; slot indices and sizes are decimal. All storage,
child preservation and DOS results remain explicit conditions.

### Exhausted lookup

The 2BC7 finder tests the current handle byte as signed, returning a
negative-byte slot. Otherwise it saves the current offset, advances by
14, and compares the saved offset unsigned against DBA8+14*DD38, using
the low word of the product. For retained DD38=20 and no offset wrap,
if indices zero through nineteen are nonnegative, the loop next tests
index twenty. A negative handle there returns that pointer. Otherwise
it advances to index twenty-one, fails the loop comparison using the
saved index-twenty offset, and tests index twenty-one in its final check.
That final negative handle also returns its pointer; otherwise the result
is null. This is not bounded to the twenty nominal stream records.

The caller 2C11 returns null directly for a null finder result. It has no
additional range or identity check before passing a nonnull result into
2AF6. An exhausted lookup therefore cannot be treated as an unconditional
null result, nor can bytes beyond the nominal table be assumed to form
valid stream records. Their actual values/lifetime are not admitted here.

### Construction failure and cleanup

2AF6 stores the mode child's flags. A zero flags result, or negative AL
from lower open, sets handle FF and flags zero and returns a null far
pointer without calling stream close. It does not clear other fields on
that branch. A nonnegative handle reaches buffer construction.

Under FND-SOUND-033's retained first-free-slot conditions (valid identity,
zero count/size/auxiliary word, read-only flags and no prior owned buffer),
37F1 clears flags 000C, sets size zero and makes both base and current
pointers address the in-record scratch byte at offset five. A null
512-byte allocation returns FFFF without setting ownership flag 0004.
The wrapper calls 2873 for every nonzero construction result, ignores
that child's return, and returns null. This differs from its early
mode/open failure branch.

2873 rejects null or identity mismatch with FFFF. For a valid stream it
only considers flushing/freeing a buffer when size is nonzero. Within
that branch a negative count invokes 292B; nonzero child return exits
before handle close or active-field clearing. An owned buffer is passed
to 1ACC only after that flush gate. These general child effects remain
outside this finding's zero-size construction-failure case.

With size zero, neither flush nor buffer release is called. A nonnegative
signed handle byte is sign-extended and passed to 27B6. Regardless of
that child's return, 2873 then sets flags, size and count to zero and
handle to FF. Base/current pointers and identity are not cleared. With
the retained auxiliary word zero it skips the 05F5/0EF5 child route.
The close return is the lower close result, or FFFF when no handle was
attempted; 2AF6 does not use it. Actual DOS failure can therefore leave
the stream locally inactive without proving the underlying handle closed.

27B6 rejects unsigned handle >=DD38 through error 0006. Otherwise it
clears the corresponding handle-table word before calling 27DE. The
latter issues interrupt 21/AH=3E with BX=handle. Carry set delegates AX
to FND-SOUND-031's 04CE error helper; carry clear clears a table word
indexed by the returned BX and returns zero. That second table access
requires preserved BX for the same-handle interpretation. DOS failure
does not restore the table word already cleared by 27B6.

## Interpretation

This removes local exhausted-loop ordering and the zero-size, retained
initial-slot construction-failure cleanup from Q-CONFIG-011. It does not
establish storage beyond the nominal table, child preservation, DOS close
success or general cleanup with nonzero size/count/auxiliary state. The
normal parser endpoint of FND-SOUND-041 does not test these failure paths.

## Alternatives

Assuming a failed buffer allocation leaves an open active stream omits
the explicit close call and field resets. Assuming null return proves
DOS close success ignores the discarded close result. Assuming cleanup
zeros all pointers contradicts the surviving base/current fields.
Exhausted lookup cannot be summarized as testing only the nominal slots.
No original was run and no complete-reading or parity status is promoted.

## How to reproduce

Use the approved installed SOUND_DS.EXE (length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57). Run resident_listing.py with -I and
the four location ranges above in order. Retained reports are
GAME_DIR/analysis/disc-adv-survey-20261104/
setup-stream-failure-complete.txt and setup-stream-failure-complete-repeat.txt;
both SHA-256 values are
716e8ad738b7354e867a3bbf305d8cb11eaa1cc87f6f770703dc1ed3ceff832f.
Check the preincrement/saved-offset loop, nonzero construction-result
branch, size-zero cleanup bypass and field stores independently. Keep
listings and original bytes only in that approved local store.
