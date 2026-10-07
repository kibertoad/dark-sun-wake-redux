---
id: FND-EXE-089
title: Selected callback changes its first word before old-value release admission and final cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE960..0x005FEA5F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD000..0x005FD00D
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-088 records `0x005FE960` as the third, callback-valued input to
the overlap publication helper. Its actual invocation and caller input
remain conditional on that helper's downstream effects. This callback
saves its first input pointer at frame -96, constructs a record at -92
with callback `0x005F50A0`, metadata `0x006EF6EC` and handler `0x005FEA07`,
and calls `0x006008F0` (FND-EXE-045). On normal return it reloads the saved
pointer, writes full word `0x00759B28` through it, then reads payload from
offset four. There is no local pointer-null guard before either access.
It saves payload minus twelve, with 32-bit arithmetic, at local -100.

Equality of that prefix with `0x0071B270` bypasses decrement and release.
Otherwise it writes one to record state -88 and calls `0x005F5760` with
payload minus four and addend all ones, plus two EDX-valued auxiliary
slots whose semantic meaning is not established. FND-EXE-033 proves the
locked exchange-add returns the old full word. Signed old value greater
than zero bypasses release; zero or negative selects `0x006D4DF0` with
saved prefix -100 first and address of local -40 second. Its extra two
slots receive the old exchange-add result. FND-EXE-040 proves the direct
release helper consumes only its first argument. Neither bypass nor a
normally returning release locally reverses the initial first-word store
or the decrement.

All ordinary routes next reload local -96 and call `0x005FD000`. This
direct helper reads its first stack argument into EAX, stores full word
`0x0075A498` through that pointer, and returns EAX unchanged. It neither
reads offset four nor calls a release helper, clears the pointer, or
normalizes the return. Its direct body ends at `0x005FD00D`; the similar
next function is outside this observation. The callback then calls
`0x00600990` with its record root and returns that cleanup helper's raw
EAX after normal frame restoration. It does not restore the first helper's
pointer return. Equality of the two callback first-word destinations
depends on intervening writes to saved local -96; it reloads that local
before the second helper rather than using an immutable entry value.

The stored handler `0x005FEA07` adds twenty-four to incoming EBP, loads
adjusted-frame pointer word -96 and words -84 and -80, and saves the latter
two at locals -104 and -108 before calling `0x005FD000` with the loaded
pointer. It compares saved full local -108 with all ones. Inequality writes
all ones to state -88 and supplies saved local -104 to `0x00600EB0`
(FND-EXE-052). Equality, or that forwarder's unexpected normal return,
writes all ones to state -88 and calls `0x005F55E0` with saved local -104
(FND-EXE-090). Native handler entry and this adjusted frame's identity
are not established. Padding after the final call provides no demonstrated
normal handler return; the next function starts at `0x005FEA60`.

## Interpretation

The selected callback has a concrete first-word mutation and an old-value
release gate followed by a distinct first-word helper and record cleanup.
These observations do not establish destruction, reference ownership,
successful exception handling or a complete callback lifecycle. Q-EXE-009
retains callback invocation, producer validity, frame admission, aliases,
all payload-counter writers and native downstream effects. Direct cleanup
order is not a guarantee that a callee returns or leaves saved locals intact.

## Alternatives

Using the updated decrement value, releasing only for positive old values,
performing the first-word write after the release, treating `0x005FD000`
as a direct allocation release, or returning its pointer as the callback's
final normalized result is ruled out locally. The fixed-prefix bypass
does not bypass the final first-word helper or record cleanup. The stored
handler's forward input is not inferred from the ordinary callback input.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-088's supplied callback
value. Read one hundred instructions at `005FE960`, retaining only the cited
callback and handler before `005FEA60`. The earlier twenty-six-instruction
prefix alone excludes the decrement and final cleanup paths. Read thirty-five
at `005FD000`, restricting its contract to the first fourteen bytes through
its return. Follow full signed old-value admission, argument slots, fresh
saved-pointer reads, first-word stores, raw final return and handler frame
adjustment. Use FND-EXE-033/040/045/049/052 for the cited callee boundaries.
Keep reports local; execute neither the interpreter nor the game.
