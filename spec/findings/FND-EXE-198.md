---
id: FND-EXE-198
title: Forwarding supplies a live selected-local slot above the nested callback and reader frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600EB0..0x00600EF5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AD0..0x00600B25
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F513B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

Let F be the conventional frame established by FND-EXE-052's forwarding
body at `0x00600EB0`. It saves three dwords and reserves twelve local
bytes, leaving ESP at F -24. On the nonnull-shared, nonnegative,
current-zero-mode path it loads shared offset 40, stores that value at
F -16 and F -20, then examines its argument's offset-12 word. Zero
prepares EDX as the address F -20 and EAX as the saved argument before
the direct selector call at `0x00600EF0`. No intervening call occurs
between the selected-local stores and this selector call on this path.
The selected-local dword at F -20 lies inside the reserved local region,
not in an outgoing argument reservation.

FND-EXE-053's selector establishes its frame S at F -32 after the call's
return-address push and its own saved-frame push. Three saved-register
dwords and twelve local bytes leave ESP at F -56. It retains EDX as
the selected-local address and prepares eight outgoing dwords before
the callback call at `0x00600B23`. The sixth slot is that retained
address. With normal stack effects, callback entry ESP is F -92 and
its conventional frame C is F -96. Consequently the passed selected-local
address F -20 equals C +76, whereas the sixth argument's own storage
is C +28. The pointer value and the slot containing it are distinct.

Conditional on selecting FND-EXE-165's callback, its nested setup record
starts at C -108. FND-EXE-167's local frame geometry distinguishes that
record from the incoming sixth slot; this comparison additionally places
the pointed-to selected local at C +76. On ordinary nested execution the
forwarding frame remains active while selector and callback execute. No
direct frame restoration or saved-stack transfer precedes the cited
callback call. This establishes the local reservation's lifetime on that
nested path, not the lifetime or validity of the record pointer it contains.

After ordinary balanced setup return and removal of its sixteen outgoing
bytes, the callback's ESP is C -184. On FND-EXE-166's reader path it
reserves twelve bytes, pushes the reloaded sixth slot, and calls the
reader. Reader entry ESP is C -204 and its saved-frame slot begins at
C -208. Under preservation of the incoming sixth slot, the reader's
supplied address therefore names C +76, numerically 284 bytes above
its four-byte saved-frame store. Its local prologue does not directly
overwrite that pointed-to local in this admitted frame geometry.

Frame-relative accesses use SS; the reader's pointer dereference and
setup's supplied-pointer stores use DS. These numeric separations imply
storage separations only under equal admitted segment bases and valid
stack extents. They do not establish DS/SS identity. Nor do they prevent
an aliased shared-base publication, indirect write, exceptional transfer
or callee effect from changing the argument slot or selected local.

## Interpretation

This narrows the selected-local's origin and ordinary nested lifetime
for one forwarding/selector route. Q-EXE-009 retains setup preservation,
segment identity, shared-base aliases, record/link/field writers, actual
callback selection, imported and exceptional effects, and excluded incoming
uses. FND-EXE-197's concrete metadata prefix cannot be substituted for
every record fetched from this local. No complete reading follows.

## Alternatives

Treating the sixth argument's storage as the selected local itself loses
one level of indirection. Treating the selected local as storage belonging
to the callback or reader contradicts the forwarding-frame construction.
Conversely, a live stack-local container does not admit the record it
contains or prove its value survives setup and indirect calls.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x00600EB0`, count 60; `0x00600AD0`,
count 45; `0x005F50A0`, count 24; and `0x00600A80`, count seven.
Combine the callback prefix with FND-EXE-166's ninety-instruction callback
window for the later reader call. Restrict claims to the cited intervals
and the specified zero-mode and reader paths. Account for each call's
return-address push, saved-frame push, register saves, local reservation,
all eight callback slots and deferred outgoing cleanup before deriving
F, S and C. Keep balanced return, callee preservation and segment-base
identity conditional. Rich reports remain in the local licensed-source
store; execute no original program.
