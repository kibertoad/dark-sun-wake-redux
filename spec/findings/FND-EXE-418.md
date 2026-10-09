---
id: FND-EXE-418
title: Record creation retries aligned loads and publishes state before a time-dependent word update
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1049..1425:10D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:10D7..1425:1146
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The body at 1049 saves BP and SI and allocates ten local bytes.
Its listed spans contain 140/111 bytes and 62/40 instructions. The
intervening two-byte jump at 10D5 is separately decoded but has no
established entry-path edge in this reading; it is not included in the
listed spans or reached coverage.

### Dirty prefix and record scan

SI starts at zero and visits six records, indexed by SI times
000E. Each nonzero current DS:3EF8 dirty word calls 0BB4
with the indexed DS:3EF4 identity and a far pointer to the
DS:3EEC head. Six outgoing bytes are removed and full returned
AX tested through DX. Nonzero returns immediately. Zero clears the
current indexed dirty word before proceeding. Earlier clears and callee
effects remain if a later record fails. FND-EXE-415 describes 0BB4.

After the six-record prefix, SI takes current DS:00CE. It calls
0C04 with SI and far local destination SS:BP-0A, removes
six outgoing bytes and tests full returned AX through DX. Zero
continues to the destination test. Nonzero returns unless current SI
has all low eleven bits clear and DX equals eleven.

Only that aligned/error-eleven combination calls 0CDD with no
arguments and tests its full AX. Nonzero returns. Zero retries 0C04
with current SI and the same destination once; another nonzero result
returns directly. This gate tests the returned word, not which internal
lookup or target produced eleven. FND-EXE-414 and FND-EXE-416
describe the load and node-initialization dependencies.

After a zero load result, SI increments at word width before the
first destination word at BP-0A is compared with one. A different
word repeats the load with the incremented SI. The body adds no
iteration limit or wrap rejection to this scan. The eight-byte local
destination is not initialized before the first request or refreshed by
local stores between requests. Zero return alone does not establish
fresh destination bytes; only its first word is tested here.

### Selection and publication

When that first word equals one, the body calls 0FA3 with
current SI minus one and far output pointer SS:BP-2. Six
outgoing bytes are removed and full AX tested through DX. Nonzero
returns without the following creation stores. FND-EXE-414 describes
the selector's own earlier effects and publication on some failures.

Zero sets current DS:3EF8 indexed by local BP-2 times 000E
to one, sets indexed DS:3EEC to zero and sets indexed
DS:3EF0 doubleword to zero, in that order. It then passes far
pointer current DS:00D2 to 15F3:010C and removes four
outgoing bytes. No returned result is tested. FND-TIME-001 records
that helper's BIOS-time-dependent word update; its runtime effects
and incoming state are not established by this caller reading.

The body re-reads the local selector, copies current DS:00D2
into indexed DS:3EEE, writes current SI to DS:00CE, writes
the local selector through incoming far output pointer SS:BP+6
and returns AX zero. These later publications do not validate or
undo the earlier dirty, head and quantity stores. Their widths are
word except the explicitly doubleword quantity at 3EF0.

### Preservation and admission

The body does not save or restore DS. SI is saved only for its
final return and not locally saved around helpers. Current DS, SI,
local selector and destinations are re-used after calls; aliases and
register/segment preservation remain separate dependencies. The aligned
retry tests current SI after the failed load, and the selected identity
uses current SI after a successful load and increment. Native changes
to those values cannot be ignored. The selector output, six-record
storage, callback destination, count/link producers and word-update
state all need admission before the local sequence becomes a complete
creation contract.

## Interpretation

This adds six-record head/quantity/dirty/scalar and DS:00CE writers
to the consumer chain. Q-EXE-007 retains complete callers and input
admission, current-segment/register preservation, callback-output freshness,
stable linked structure, other writers and native/time-dependent contracts.
No complete reading, admitted unlimited scan, successful native transfer,
atomic creation or observed original bug is declared.

## Alternatives

Retrying every load failure contradicts the alignment and result-eleven
gate. Treating that result as necessarily the lookup's missing-link case
ignores the load wrapper's other result source. Treating the scan as
bounded by six confuses its separate dirty prefix with the later identity
loop. Treating state publication as conditional on a checked word-update
success adds a test absent from the caller. Treating zero load return
as initialized output adds native writes not proved here.

## How to reproduce

At revision 5ba5c941 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode.
Require 140/111 bytes and 62/40 instructions. Follow dirty clear
ordering, DS:00CE input, aligned/error-eleven retry, destination freshness,
increment-before-test, selector argument/output, creation stores, untested
word-update call and final publications. Retain the two-byte unproved
gap. Use FND-EXE-415/414/416 and FND-TIME-001 for dependencies;
preserve caller, alias, writer and native-effect gaps. Licensed bytes remain
outside Git; no game, DOSBox or emulated call runs.
