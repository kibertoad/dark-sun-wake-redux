---
id: FND-CONFIG-154
title: The installed FNFO bytes give bounded traversal selectors ordinary-word and temporary-slot cases
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0002037E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D4
tool: Python 3.14.7 bounded source-resource reads; Capstone 5.0.7 entry-based 16-bit instruction checks
environment: null
---

## Observation

FND-CONFIG-153 identifies the matching installed FNFO/1
and FNFO/2 spans. FND-CONFIG-150 assigns traversal
selectors for slot bytes one, two and five. Reading only those
selector cases gives:

| Slot byte | Selector | FNFO/1 signed offset word | FNFO/2 metadata byte |
|---|---|---|---|
| 1 | 5 | 10 | 13 |
| 1 | 2 | 4 | 8 |
| 1 | 4 | 8 | 7 |
| 2 | 15 | 4 | 12 |
| 2 | 16 | 8 | 7 |
| 2 | 17 | 10 | 7 |
| 2 | 4 | 12 | 7 |
| 5 | 2 | 2 | 8 |

The metadata byte is at FNFO/2's start plus selector. The
corresponding offset word is at FNFO/1's start plus 196
times (slot byte minus one), plus twice selector, matching
FND-CONFIG-140's getter layout after the offered buffer load.
Each tested offset is nonnegative, not the minus-one rejection
value, and leaves room for two bytes within the corresponding
stride 23, 49 or 23 in FND-CONFIG-150. This does not
validate an arbitrary paired index or a far-base pointer.

For comparison, selector zero's metadata byte is nine at
file offset 0x0002031C. It is not the initial zero byte
in the executable's DS:60ED load image recorded by
FND-CONFIG-141. A successful resource load is therefore
material to the later selector-zero reading.

With the direction flag clear, the setup's common prefix before
its DS:193E gate fills 23 width bytes from DS:040C with
value two, then changes metadata positions zero and one to one and positions four
and five to four. The relevant instructions occupy
`0x00073B7C..0x00073B9E`, within
FND-CONFIG-144's complete entry. The string stores do not
have a preceding local direction clear in this prefix. Therefore,
with that entry condition and no later width changes, metadata
seven, eight, nine, twelve and thirteen select width two.
The prefix also occurs on the nonzero gate path; neither the
gate nor FND-CONFIG-155's saved-flags return establishes
the incoming direction flag here.

FND-CONFIG-140's getter initially reads a signed word for
all the named metadata cases under that width condition,
provided the selected record and pointer are valid. Metadata
seven, eight and nine remain ordinary value returns. Metadata twelve calls its local slot
writer with type three; metadata thirteen calls it with type
four, because that argument is metadata minus nine. The
writer stores the low value word in the special slot and returns
DS:60EB sign-extended. These are conditional getter paths,
not observed record values or confirmed active traversal nodes.

For the slot-byte-one selector five and slot-byte-two selector
fifteen cases, a successful getter therefore reaches the special-
slot write. FND-CONFIG-145's queue helper saves and later
restores that slot when the queued index equals DS:60EB.
Its metadata-eight continuation is also a concrete possible
case for selectors two, subject to the caller's other conditions.
No gameplay identity is assigned to these fields or slot types.

## Interpretation

The direct selector assignments in FND-CONFIG-150 reach
more than a read-only getter branch when the installed metadata
is successfully loaded and its record inputs are valid. The
source supplies concrete cases for the already-read temporary-
slot behavior and metadata-eight branch, rather than leaving
all selector types at their executable's initial zero values.
This narrows the conditional helper graph without proving
activation of iterator slot 523, equality of stored indices,
a successful transfer or reachable rest-time termination.

## Alternatives

Q-CONFIG-008 retains actual loads, the direction flag at the
width prefix, later width changes, base-pointer and paired-index
validity, stored record words, special-slot ranges, later
metadata changes and reachable traversal calls. A valid selected
record can take one of these measured source-backed branches;
a missing or invalid record can exit earlier, and a later writer
can change its inputs. Initial executable bytes and successfully
loaded resource bytes remain separate states. No full FNFO
schema or behavioral-rule status is promoted.

## How to reproduce

Use FND-CONFIG-153's fingerprint-matching source and catalog
positions. Read the eight offset words and the seven metadata
cases, including selector zero, without retaining a broad byte
dump. Derive each first-resource position from its 196-byte
region and two-byte word stride. Compare the direct selector
assignments in FND-CONFIG-150, full getter branches in
FND-CONFIG-140 and queue save/restore in FND-CONFIG-145.
Decode the width-prefix instructions from the exported setup
entry, verifying that the gate follows those writes and retaining
the incoming direction flag as an unresolved condition. Preserve
all load, width, record-validity and later-state conditions; do not
execute the original or turn the measured cases into gameplay.
