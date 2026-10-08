---
id: FND-EXE-072
title: Published head target uses unsigned mode admission and tail-forwards an adjusted payload after an optional callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FAE80..0x005FAEC8
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-025 already records the failure-publication helper's prefix writes:
for its supplied payload address P, the base is P minus 80; the signature
words lie at base offsets 48/52 and the concrete target `0x005FAE80` at
base offset 56. It passes P minus 32 to the later helpers. This equals base
plus 48, the input relation used by FND-EXE-067/070, subject to publication,
selection and alias provenance. FND-EXE-071 then reads input offset eight,
which is base offset 56. Thus the publication writer supplies a concrete
candidate for that indirect cleanup call; it does not prove that every
selected head carries this writer's value.

At `0x005FAE80`, the target creates a conventional frame, saves one register
and reserves another word. It reads its second original full-word argument
as an input address and forms input minus 48 at 32-bit width as a base.
It compares its first original argument with one at unsigned 32-bit width.
Zero and one take the same admitted branch; every larger unsigned word,
including all ones, takes the other branch. FND-EXE-071 prepares one and
its input address in those first two slots, so its normally reached target
invocation selects the admitted branch. The two additional slots that caller
prepares are not read by this target's local body.

The admitted branch reads the full word at base offset four as another
callback target and computes input plus 32 as a payload address, both at
32-bit width. With the prefix relation above, that payload is P again.
A zero callback skips its invocation. A nonzero callback receives that
payload in one full outgoing stack slot after twelve bytes of reservation,
and is called indirectly. Its result is not tested; its concrete consumption,
mutations and exceptional effects remain conditional.

After normal callback return, or directly on the zero-callback branch, the
body overwrites its first original stack-argument slot with the saved payload
address. It restores the saved register and frame and jumps to `0x005FCEF0`.
This is a tail jump with the original caller's return address retained,
not a new call or a direct local return. The target's first argument has
changed from mode to payload before the tail helper reads it. The second
original argument and auxiliary slots are not locally rewritten. No result
or resource-release contract for that tail helper is established here.

The larger-mode branch instead reads base offset twelve, prepares that
full word in one outgoing stack slot after twelve bytes of reservation,
and calls FND-EXE-041's indirect-target finalization helper. There is no
local null guard on the input or base before either branch's field read,
and no ordinary return or tail-helper join established beyond finalization.
The unsigned guard is a mode admission test, not a pointer bounds check.

FND-EXE-025 gives separate writers for the two target fields: base offset
four receives the publication helper's third original argument, while base
offset twelve receives the word read through its shared finalizer-storage
pointer. FND-EXE-036's capacity-limit caller supplies `0x006D8BD0` as that
third argument, furnishing one candidate for the admitted callback target.
Its body and effects remain unread; it is not named as a destructor from its
position alone. Later writes and aliases can change either selected word.

For FND-EXE-071's cleanup invocation, the context-head change still precedes
target selection and this body's later callback-field read. A zero callback
does not bypass the tail helper. A nonzero callback that returns zero does
not select a different path: its return is ignored. There is no direct
free import, field clear or rollback in this body. Indirect callback and
tail-helper effects prevent any inference that the overall operation is
effect-free or that the payload remains allocated.

## Interpretation

The publication writer now connects to a concrete cleanup target's local
mode, pointer-adjustment, callback and tail-dispatch contract. Q-EXE-009
retains selected-head identity, field mutation, the concrete callback,
tail-helper effects and allocation/fallback lifetime, alongside dispatcher
frame and static-context provenance. No complete release operation, exception
lifecycle or shell outcome is established or implemented.

## Alternatives

Using a signed mode comparison, admitting all ones as a negative small mode,
calling the second argument directly, passing the prefix base instead of
the adjusted payload, branching on callback truthiness, skipping the tail
helper for a null callback, or leaving the original mode in its first slot
is ruled out. An allocation/publication writer gives a candidate target and
relative layout, not proof that every current head is that allocation.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read twenty-five
instructions from `0x005FAE80` and five from `0x005FAEBC`, restricting claims
to the cited body. Use FND-EXE-025 for the prefix writer, FND-EXE-036 for the
capacity caller's callback value, FND-EXE-067/070/071 for selected-head and
cleanup inputs, and FND-EXE-041 for finalization. Track full argument widths,
unsigned zero/one/two/all-ones admission, both pointer adjustments, separate
callback fields, null bypass, ignored callback return, rewritten original
argument, restored-frame tail jump and larger-mode finalization. Keep target
selection, callback/tail effects, aliases and lifetime conditional. Keep rich
reports local and execute no interpreter or game.
