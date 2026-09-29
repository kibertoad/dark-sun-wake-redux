---
id: FND-CONFIG-173
title: The intervening list helper has signed count gates and checked or ignored result paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0003
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-168's pointer consumer decrements current
word DS:A103 and calls local far 3A8E:0003 before its
child scan. It ignores that call's AX. FND-UI-008 first
identified this routine; the present reading bounds its
local gates and call-result ordering on this cleanup path,
without changing that earlier entry or establishing all
region semantics. The complete body occupies file span
`0x0002FAE3..0x0002FD93`, ending with far return at
`0x0002FD92`.

After saving SI and DI and its stack-limit guard through
1000:2E48, it passes current DS:9F06 by far address to
4237:0003. It loads the far pointer DS:A105 and starts
index zero. Its two list passes both compare this word
index with current word DS:A103 using signed comparisons.
The first record is the loaded pointer; subsequent records
follow the prior record's far link at EE. Neither pass
has its own pointer-null, membership or cycle check.
The count is read again at each loop comparison.

In the first pass, word mask 0004 at record+9E skips the
record's later processing. Even when that bit is clear,
all indices other than zero also skip it: this is the
physical first record, not the first record with bit clear.
That record's children use the same wrapped word offset
formula as FND-CONFIG-168: record offset, zero-extended
byte +F2, 30 times the child index, and 0105, retaining
the record segment. Its unsigned word +F3 bounds the
child-index comparison. Child word +1C with bit 8000
set skips the child; other tags besides MENU also skip.

For an admitted MENU child, the far pointer at child+0
starts another loop. A null pointer ends it. A nonnull
pointer passes its address at offset +0C to 4072:0078
with a far local output at SS:BP-24. It then reads current
DS:A11D without an own null check and passes that pointed
record's words +96/+98 with the local output to 4400:0062.
Next it calls 4237:0E7C with DS:9F06 as both output and
one input, plus that same local output as the other input.
Returned AX FFFF immediately returns FFFF from 0003.
No local restoration of earlier callee effects precedes
this return.

Other 0E7C results read the current MENU object's word
+22. FFFF sets the local pointer to zero. Other values
pass the object, that word and word one to 3BA6:049F,
then dereference the returned record's far pointer at +16
as the next loop pointer. FND-CONFIG-172 bounds the named
049F branch. This loop has no own cycle/progress bound
or null check on that returned record before its read.

After the first pass, 0003 calls 4237:0C8A with DS:9F06
as both output and one input and DS:9E7C as the other.
It calls 4237:0076 with DS:9E7C and local output
SS:BP-AE, then 4237:0D3F with that local output as both
output and one input and DS:9F06 as the other. Those
three returned AX values are not checked. The shared
0076 count-copy contract is read in FND-CONFIG-099;
valid source/output state and aliases remain inputs.

The second list pass reloads DS:A105 and starts index
zero. Word mask 0004 at record+9E again skips the record, but
other indices are now admitted. It passes record+0A6
to 4072:0078 with local output SS:BP-18, then that output
and record words +96/+98 to 4400:0062. It calls
4237:0027 with record+0C and the local output; AX FFFF
returns FFFF. Other results call 4237:0C8A with record+0C
as both output and one input and local SS:BP-AE as the
other. FFFF again returns FFFF. Other results call
4237:0D3F with local SS:BP-AE as both output and one
input and record+0C as the other. FFFF again returns FFFF.
These are exact local result predicates, not proof of
those external operations' failure origins or outcomes.

For a continuing record, current word DS:9DEE bit 0400
set or a nonzero list index skips the final two calls.
Otherwise it passes the record's words +96/+98 to
4328:009E and record+0C to 4328:005A. FND-CONFIG-099
bounds their direct fixed-field copies. Finished passes
return zero. All supplied record-offset additions use
word arithmetic in the original supplied segment.

All named external segments were verified through their
declared MZ relocations. DS means its value at each
instruction. SI/DI are restored on the body's normal
return; internal scan progress still depends on external
callees preserving relevant index and field state.

## Interpretation

The helper reads a changed list after the caller's
link/count operations. It has signed outer count gates,
unsigned child gates, several unbounded pointer walks,
aliased region-operation arguments and different result
checking at different calls. FFFF exits can follow earlier
changes, but the caller proceeds to child cleanup after
a returning 0003 without testing AX. Successful region
or presentation work is not established by that continuation.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain count/head/link and
child producers, MENU selected-index provenance, valid
region inputs and capacities, all callers, aliases,
DS/index preservation and complete external callee effects.
One reading supplies finite valid graph and continuing
results; another reaches changed inputs or a locally
checked FFFF. Complete producer/callee evidence would
distinguish their reachable effects. Neither loop syntax
nor a signed count gate proves valid pointers or termination.

The count after decrement can select a different pass
gate from its prior value, including word wrap. This is
a conditional local distinction, not an observed invalid
native list. Resident cases remain in Q-SCRIPT-007 after
supported layouts and the harness exist. No native or
emulated result or visual outcome is claimed.

## How to reproduce

Read 3A8E:0003 through 02B2 from its entry. Follow both
signed list passes, first-record-only processing, flag and
unsigned child gates, MENU pointer loop and its selector
helper. Retain the output/input aliases and distinguish
the three unchecked middle calls from the explicitly
checked FFFF returns. Check word offset arithmetic,
DS:A11D dereference and final bit/index gates. Verify
all declared MZ segments, compare FND-CONFIG-099's copy
contracts and FND-CONFIG-168's ignored AX at the call.
Keep complete region semantics and actual outcomes separate.
