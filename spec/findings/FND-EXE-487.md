---
id: FND-EXE-487
title: Sound utility row registration consumes a byte and polling selects downstream modes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:0195..1A7C:022A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:03D7..1A7C:040F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:21D2..1000:21E4
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000C6E..0x00000C82
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-485's row-registration call enters 1A7C:01C6. For each
of four word writes, it separately reloads current DS:18F8, multiplies
that word signed by nine and uses only the low product as a displacement
in current DS. The incoming words at SS:BP+6, +8, +0A and
+0C are stored at that displacement plus DF34, DF36, DF38
and DF3A respectively. A fifth independent index reload and product
stores the incoming byte at SS:BP+0E into displacement plus DF3C.
Only that byte is consumed locally; FND-EXE-485's retained high byte
is not part of this store. The helper increments current DS:18F8,
reloads it into AX and decrements AX before returning far without
incoming cleanup. It restores BP and makes no call or interrupt.
It does not locally change SI, DI, DS or ES, and does change BX,
DX, AX and flags. There is no index, product, extent or wrap check.

Under admitted stable nonaliased state, this stores a nine-byte row and
returns its original index, with count increment at word width. Fresh
index reads, writable aliases and the final reload prevent extending that
conditional observation into an atomic registration claim. Table capacity,
count writers and row semantics remain separate questions.

The nearby helper 0195 tests current DS: ECD0 for byte value one.
Only equality selects its call path. It reserves 0020 stack bytes,
sets word SS:BP-10 to two and passes SS:BP-20, SS:BP-10
and word 0033 to 1000:20DF, then removes ten argument bytes and
stores zero into current DS:ECD0. Other bytes of those local records
are not initialized here. It resets SP from BP, restores BP and returns
far without incoming cleanup. It defines no normalized return word or
local SI/DI/segment preservation across the external call. The flag-clear
store follows the call; its target requires actual DS preservation to
identify it with the earlier flag read.

FND-EXE-486's poll wrapper 03D7 saves BP and SI and calls
local far-returning 00EA with push-CS/near-call. Nonzero full AX
and nonzero incoming word SS:BP+6 select downstream 022A with
one incoming word zero. Otherwise it calls 1000:21D2. Nonzero
full AX from that helper selects 022A with incoming word one;
zero returns FFFF without the downstream call. Each 022A call also
uses push-CS/near-call and the wrapper removes its two incoming bytes.
Its returned AX is held in SI and forwarded unchanged into AX.
The wrapper restores SI and BP and returns far without incoming cleanup.
It does not normalize the downstream result or admit its meaning.

The 21D2 helper first tests current DS:DF08 as a byte. Nonzero
returns AX one without an interrupt. Zero sets AH to 0B and executes
interrupt 21, then opcode 98 sign-extends returned AL into AX at
sixteen-bit operand width. It returns far. Thus the local wrapper tests
the entire resulting word for zero, not a specified native readiness code.
Native interrupt behavior and preservation remain outside this reading.

The segment words of 0195's call at 01B5 and 03D7's call at
03F9 are MZ relocation records 783 and 780 respectively, encoded
zero and bound to modeled segment 1000 at load segment 1000.
The local 00EA and 022A calls supply CS explicitly instead.

## Interpretation

This resolves the registration helper's consumed argument width and local
preservation, and binds the polling wrapper's two mode choices to its
readiness results. Neither an index return nor a nonzero readiness result
is an admitted native operation or table-capacity guarantee. Q-EXE-007
retains 00EA, 022A and 20DF behavior, readiness and count/flag producers,
actual DS, table/frame extents, aliases, re-entry and native preservation,
other callers and the local 08E5 interface. No complete input reading,
device or key meaning, successful configuration or launch exclusion is claimed.

## Alternatives

Treating registration as one retained-index write ignores fresh reloads.
Treating the incoming fifth word as fully consumed ignores the byte load.
Treating 0195 as a normalized predicate invents a return assignment.
Treating the poll wrapper as only one input mode ignores its readiness
branches and independent incoming-word test.

## How to reproduce

At revision 8ec0c0c require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped BD55..BDEA at IP 0195
and BF97..BFCF at IP 03D7, CS 1A7C; and 35D2..35E4
at IP 21D2, CS 1000. Verify opcode 98 at 21E2 as byte-to-word
sign extension rather than relying on the displayed mnemonic. In the
relocation table at 003E with 958 records, inspect records 783 and
780, targeting shipped BD78 and BFBC. Track repeated index reads,
incoming argument widths, final count reload, flag-clear ordering,
full-AX branch tests and near/far cleanup. Licensed bytes stay outside Git;
no original process, DOSBox or emulated call runs.
