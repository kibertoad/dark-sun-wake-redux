---
id: FND-CONFIG-162
title: The shared helper's first callee installs a fallback callback and waits through two polls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0BC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0034
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV/MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-161's first local callee is overlay 199 code
0BC1, file span `0x000884C1..0x00088521`, ending with
far return at `0x00088520`. Its caller supplies the far
return frame with push CS followed by a near call. There
is no argument load in this callee.

It tests the double word at current DS:6554. Nonzero
passes that value to 444C:0092 and stores returned DX:AX
back into the field; zero skips the call and store. It
then writes zero at current DS:143D and passes the far
address 28C9:0CFF to 571F:0034. Declared FBOV fixups
resolve the pointer's segment and setter call separately.
FND-CONFIG-086 previously records this registration site.

The complete setter 571F:0034 selects overlay 190 code
11A1, file span `0x000794E1..0x00079501`. It stores
its supplied pointer at current DS:61A2, saves a local
copy, and forwards that copy to resident 39D1:0418.
It does not invoke the supplied callback itself. The
resident setter writes it at current DS:A0F1 only after
an unsigned comparison of current DS:009C and SP, with
a conditional call to 1000:2E48 before the write.
FND-CONFIG-163 bounds that guard branch. FND-CONFIG-083
reads the resulting fallback dispatch route; registration
alone is not a dispatch observation.

After the setter returns, 0BC1 passes the same SS:BP-2
local far pointer twice to 45B9:0034, repeating the call
while bit zero of returned AX is set. A returned result
with that bit clear continues to resident 4611:03A5.
After that call returns it tests current DS:14E3. Zero
reaches the far return. Nonzero repeatedly calls resident
4611:0051 until AL is zero, then reaches that return.
Neither poll has its own count or timeout, and the second
loop does not recheck DS:14E3 on each iteration.

There is no own DS write in 0BC1 or the overlay setter.
Each has external callees whose DS preservation, results
and other effects remain separate conditions. A store to
current DS in these bodies is not an unconditional claim
that the segment is still 57E0 after an earlier call.
The body does not locally clear script-cache bounds,
identities, ages or script-buffer bytes.

## Interpretation

The overlay setter writes DS:61A2 before the resident
setter's guard and DS:A0F1 write. Entering that guard
therefore does not imply that both copies remain unchanged.
This is ordered state modification, not atomic registration.

The shared helper's first callee can change a supplied
pointer field and the global fallback registration before
reaching two result-driven polls. Its successful returning
path is conditional on those callees and returned values.
The outer shared helper's later poll in FND-CONFIG-161
is additional to these two loops. No immediate-return
or universal DS-preservation contract is established.

## Alternatives

FND-CONFIG-164 subsequently reads the common poll's local register
contract and the two aliased output arguments. Its driver/input outcomes
and the remaining external dependencies below stay open.

Q-CONFIG-008 and Q-SCRIPT-003 retain 444C:0092,
45B9:0034, 4611:03A5 and 4611:0051, DS preservation,
pointer/gate producers and incoming state. One reading
uses valid ordinary state and clearing poll results;
another changes state, fails before registration finishes
or continues receiving non-clearing results. Complete
callee readings and their inputs distinguish reachability.

The installed callback can later participate in event
fallback, but 0BC1 does not dispatch it directly. Whether
it runs during a poll, how it changes the return sequence,
and whether the former fallback is restored remain open
under Q-CONFIG-008. The existing route in FND-CONFIG-083
and registration inventory in FND-CONFIG-086 are leads
for those inputs, not proof of their occurrence here.

## How to reproduce

Read overlay 199 code 0BC1 through 0C20 from its entry,
then map each declared fixup and the immediate pointer
argument. Resolve 571F:0034 to code 11A1 and read through
11C0. Independently read resident 39D1:0418 through
042F and map its MZ-relocated guard call. Follow each
poll back edge and its exact AX-bit or AL predicate.
Combine FND-CONFIG-161's caller order, FND-CONFIG-083's
setter/dispatch reading and FND-CONFIG-163's guard
without assuming that an earlier call or poll returns.
