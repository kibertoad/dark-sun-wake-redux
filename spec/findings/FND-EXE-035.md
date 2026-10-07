---
id: FND-EXE-035
title: Payload storage rounds a capacity word and initializes its three-word prefix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D50A0..0x006D51AD
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D51B0..0x006D51CB
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-034 calls this helper with a requested count, a source word from
header offset four, and a third outgoing word. After setting up a local record
through `0x006008F0`, the direct body reads the first two original 32-bit
arguments. It does not directly read the third. This is not a claim about
all callers or indirect access by its record machinery.

All arithmetic below is 32-bit, including additions and doubling; comparisons
are unsigned. The first argument is a mutable local argument slot. An initial
request above 1073741820 calls `0x005F77D0` with `0x0075443A` after changing
the local record state to minus one. If that unread helper returns normally,
the body joins the ordinary capacity processing without repeating this guard.
Neither a thrown exception nor a guaranteed rejection is established here.

Let R be the current first word and C the second word. If R is greater than C,
R is greater than 4067, and the wrapped value of twice C is greater than R,
the body replaces R with that doubled value. Otherwise it retains R.
It then computes wrapped R plus 29 and R plus 13 separately.

- If wrapped R plus 29 exceeds 4096, it adds
  4096 minus the low twelve bits of wrapped R plus 29 to R. It then clamps
  this wrapped updated R to 1073741820 if it exceeds that value, and forms
  the allocation argument as wrapped updated R plus 13.
- Otherwise, if wrapped R plus 13 exceeds 128, it adds
  128 minus the low seven bits of wrapped R plus 29 to R, then forms
  the allocation argument as wrapped updated R plus 13. There is no clamp
  in this branch.
- Otherwise it retains R and uses the already computed wrapped R plus 13.

Both padding expressions add a full block when their masked value is zero.
The initial guard alone does not prove a bound for later doubled or rounded
values. No allocator-overhead meaning is assigned to the separate constant 29.

The body marks its local record state as one and calls `0x005F7E10` with the
computed argument. FND-EXE-024 records that helper's malloc import and local
retry boundary; callback and exceptional contracts remain partial. After normal
return, this body rereads the mutable first argument and saves the returned
base. It writes that reread word at base offset four, then zero at offset eight,
then zero at offset zero, each as a 32-bit word. There is no local null check.
It calls `0x00600990` with its local record, reloads the saved base and returns
that raw base after normal frame restoration. It does not initialize a payload
terminator here; FND-EXE-034 performs the later payload and terminator writes.

## Interpretation

Under normal allocation and record-helper behavior with valid storage, this
supplies a twelve-byte prefix whose initial length and offset-eight word are
zero and whose offset-four word holds the reread adjusted capacity. The
allocation request includes thirteen beyond the adjusted first word, but
caller ranges, aliases, allocator effects and lifetime are not established.
Q-EXE-009 retains those dependencies. This is a bounded producer reading,
not a complete format, ownership or interpreter outcome description.

## Alternatives

Rounding directly from R plus 13 in both padded branches, adding zero when
already aligned, clamping every branch, returning base plus twelve, or reading
the third argument directly are ruled out by the bounded body. Treating the
initial upper-limit call as an unconditional stop requires a reading of its
callee: the local normal-return path explicitly continues. Treating every
arbitrary input as reachable requires caller evidence not supplied here.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006D50A0`, then
read 85 instructions from that entry and five from `0x006D51C4`. Restrict the
second window to the two instructions within the cited body; later instructions
belong to another function. Verify original argument reads, the unsigned
comparisons, 32-bit doubling/addition, both padding masks, branch-specific
clamping, the upper-limit normal-return join, allocation argument, argument
reread, header write order, record cleanup and saved base return. Follow
FND-EXE-034's three outgoing writers and FND-EXE-024's allocation boundary.
Keep unread record, upper-limit and allocation effects conditional. Keep rich
reports local and execute no interpreter or game.
