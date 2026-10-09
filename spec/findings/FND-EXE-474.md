---
id: FND-EXE-474
title: Sound utility pathname continuation uses inclusive byte loop and unbounded reverse separator search
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:011A..158E:01D3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:002B..158E:002D
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-377's byte-3A delimiter path continues at 158E:011A. It
forms a temporary saved quantity minus one at double-word width and
tests that temporary high word signed. A negative high word selects
zero quantity; a positive high word selects the original saved quantity.
High word zero also selects the original quantity: the low-word OR clears
carry and the following unsigned-above-or-equal branch is consequently
taken. The selected quantity and mode zero are passed with the retained
record pair to 2CC8, whose result is ignored after ten-byte cleanup.
FND-EXE-376 reads the called helper's own stores and result checks.

The caller calls 2F81 with that record pair, removes four argument bytes
and stores AL at SS:BP-09. Byte FF takes the already recorded AX-zero
common exit; full AX is not tested. It then makes two more 2F81 calls,
each storing AL at BP-0A with no intervening test. The second overwrites
the first stored byte. The helper's byte/full-word distinction and SI
restoration are recorded in FND-EXE-379.

The next loop calls 2F81 again, stores AL at BP-0A and at SS-relative
BP+SI-00E8, then increments SI. Byte 0A ends the loop. Otherwise
signed SI less than or equal to 0050 repeats. On exit, SI exactly 0050
takes the AX-zero common exit. Any other value is decremented, and
the indexed byte is replaced with zero. No full-word read-result test,
independent source bound or segment adjustment occurs in this loop.

The entry initializes SI zero at 158E:002B. Applying that initial value
here requires every intervening call's preservation and admitted frame/state,
including FND-EXE-378's native position path. Under noninterfering continuation
with SI zero at loop entry, up to 81 byte stores occur before the signed
index test ends a non-0A sequence. A 0A at the eightieth store produces
SI=0050 and selects the early exit; a sequence reaching its eighty-first
store produces SI=0051 and reaches the terminator write at index 0050.
The test is therefore not a simple rejection of every sequence reaching
eighty stores. These are conditional arithmetic paths, not admitted input
or storage capacities.

After the terminator write it calls 2873 with the retained record pair,
removes four bytes and ignores the result. FND-EXE-358 reads that cleanup
root. It then decrements current SI and reads the indexed byte until it
equals 5C. There is no local lower-bound, count or wrap test. Equality
replaces that indexed separator with zero and continues at 158E:01D3,
outside this reading. The first reverse probe is before the terminator;
its readable extent and eventual separator are not proved locally.

Both indexed regions use BP+SI through SS. Their effective offsets wrap
at word width; identifying them with the reserved caller frame requires
index admission. The cleanup's ordinary SI restoration and possible native
effects are distinct requirements. No direct interrupt or original execution
occurs in this bounded caller body.

## Interpretation

This follows the actual pathname consumer beyond its first delimiter. Its
quantity selection tests a temporary decrement but passes the original saved
quantity on the nonnegative-high-word branch. Read-result decisions vary by
site: one AL-only FF test, two overwritten untested bytes, then an inclusive
signed-index loop. Terminator publication precedes unchecked cleanup and a
reverse search whose stopping condition does not prove an admitted extent.

Q-EXE-007 retains later continuation from 01D3, actual index and frame
admission, native preservation/results, record and source producers, buffer
and separator extents, aliases and lifetime/re-entry. No playable behavior,
complete reading or execution exclusion is claimed.

## Alternatives

Treating the selected quantity as the tested temporary overlooks the original
pair pushes. Treating the low-word branch as a signed-value test ignores
carry clearing by OR. Treating the byte loop as an eighty-store upper bound
ignores its inclusive signed test. Treating every read failure as checked
ignores the overwritten and loop bytes. Treating the reverse separator search
as frame-bounded adds a lower bound absent from the local body.

## How to reproduce

At revision 48de372 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00006DFA..0x00006EB3 at IP 011A
and 0x00006D0B..0x00006D0D at IP 002B, modeled CS 158E and
MZ header size 1400, with locked Capstone 5.0.7 in sixteen-bit mode.
Use FND-EXE-360/377's native call bindings and retained frame pairs.
Trace temporary versus pushed quantity, OR flags, each AL store and
overwrite, inclusive signed loop, exact-index exit, cleanup result discard
and reverse-search first probe. Keep caller preservation and frame extents
open. Original bytes stay outside Git; no original process, DOSBox or
emulated call is executed.
