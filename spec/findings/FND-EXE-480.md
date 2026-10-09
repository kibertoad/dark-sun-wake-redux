---
id: FND-EXE-480
title: Sound utility final configuration continuation copies a scanned string and ignores optional-call result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:06B0..158E:06FF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:008C..158E:008F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3A7A..1000:3AA3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:394E..1000:3970
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000336..0x00000342
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD73..0x0000FD76
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-479's continuation at 158E:06B0 passes current DS:0616,
current DS:0613 and SS:BP-019C to 1000:394E, then removes twelve
incoming bytes. It does not test the result. The local 394E wrapper first
stores zero into the first byte of its destination pair. It then passes
callback offset 390E, a pair naming its destination argument on the stack,
its second incoming pair and the offset of its remaining incoming arguments
to near 0F25. It restores BP and returns far. Cleanup and output production
inside 0F25 remain outside this reading. Under admitted DS 1E36, shipped
FD73..FD76 supplies the percent-s conversion marker and a zero terminator;
this does not establish the formatter's interpretation or output extent.

The caller next passes SS:BP-019C as source and current DS:B93C as
destination to 1000:3A7A, removes eight bytes and ignores its returned pair.
The copy helper saves SI and DI, clears direction, loads source ES:DI and
holds its initial offset in SI. With AL zero and CX FFFF it scans for zero
using repeated byte comparison, then complements CX. It saves DS, uses
the source segment as DS, loads the destination into ES:DI and copies that
many bytes from DS:SI. It restores DS, returns the incoming destination
pair in DX:AX, restores SI, DI and BP and returns far without argument
cleanup. It performs no destination-capacity or overlap check.

With admitted noninterfering source and destination and a terminator among
the 65535 scanned bytes, complemented CX equals the scanned byte count,
including the terminator. If every scanned byte is nonzero, CX exhausts
to zero and its complement is FFFF: the helper still copies 65535 bytes,
without appending a terminator or reporting exhaustion. Both scan and copy
advance offsets at word width without segment adjustment. The fixed scan
limit is not proof of either allocation's extent. Overlap and concurrent
changes can alter the copied bytes; no stable string snapshot is established.

After copying, the caller tests its incoming word at SS:BP+6. Zero skips
the optional call. Nonzero passes current DS:D244 and current DS:B93A
to 190F:0000 and removes eight bytes without testing any result. Both paths
set AX one and jump through 008C to 06F9, where DI and SI are restored,
SP is reset from BP, BP is restored and the caller returns far without
incoming cleanup. This local success word does not validate any preceding
output or optional operation.

The three far-call segment words are MZ relocation records 192, 191 and
190 respectively. Encoded segments zero, zero and 090F bind to modeled
segments 1000, 1000 and 190F at load segment 1000.

## Interpretation

The final local continuation publishes a copied buffer and optionally enters
another routine before returning one. Its result checks cannot establish
successful formatting, a valid terminated destination or success of that
optional routine. The copy has a fixed traversal count but no admitted
source or destination capacity here. Q-EXE-007 retains formatter 0F25 and
callback behavior, actual DS and source producers, frame/global extents,
aliases and state lifetime, and 190F:0000's complete consumer paths.
No launch exclusion or complete configuration reading is claimed.

## Alternatives

Treating the scan cap as a valid string bound ignores allocation admission
and the unterminated exhaustion path. Treating AX one as confirmation of
the optional operation ignores the unconditional overwrite. Naming the
conversion marker as proof of exact output assumes unread formatter behavior.

## How to reproduce

At revision eb72696 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode with MZ header size 1400 and
modeled load segment 1000. Decode shipped half-open offsets
7390..73DF at IP 06B0 and 6D6C..6D6F at IP 008C, modeled CS 158E;
4E7A..4EA3 at IP 3A7A and 4D4E..4D70 at IP 394E, modeled CS 1000.
Inspect shipped FD73..FD76 only for the conversion marker and terminator.
In the relocation table at 003E with 958 records, inspect records 192,
191 and 190, whose segment words are shipped 73A3, 73B6 and 73CE.
Track argument order, result overwrite, REP exhaustion, complemented count,
offset wrap and local cleanup. Licensed bytes stay outside Git; no original
process, DOSBox or emulated call runs.
