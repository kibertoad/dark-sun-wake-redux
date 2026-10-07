---
id: FND-EXE-032
title: A collection field helper chooses publication through a signed preceding-word guard
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6C70..0x006D6CED
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6CF0..0x006D6D20
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6D42..0x006D6D64
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-031 passes node offsets eight and twelve as destinations to
`0x006D6C70`, with local temporary-word addresses as sources. This callee
builds a local record and calls `0x006008F0` before reading its arguments.
It dereferences the original 32-bit source argument to obtain a payload
pointer, saves that pointer and the destination, then reads the 32-bit word
four bytes before the payload. It branches on that word's sign bit, with
no local null or preceding-storage bound check.

For a nonnegative word it compares payload minus twelve with `0x0071B270`.
Equality skips an auxiliary call. Otherwise it calls `0x005F5770`, placing
payload minus four and one in the first two outgoing slots, with two saved
register words in later slots. Their consumption and the callee's effects
remain unread. After normal completion its return is discarded. Both arms
reload the saved payload and destination and store that payload there as
a full 32-bit word. They then call `0x00600990` with the local record address
and restore the frame normally. The source field is not reread before
publication. No local rollback occurs; indirect effects remain conditional.

For a negative word it calls `0x006D5030` after writing local state value
two. Outgoing slots contain payload minus twelve, a local address at frame
offset minus forty, zero, and a register value whose producer and consumption
are not established here. These contents are not a complete parameter
contract. On normal return it stores that callee's full 32-bit return into
the saved destination without a local null or status test. It then calls
`0x00600990` with the local record address and restores the frame normally.
Allocation, copying, sharing and exceptional effects remain unread.

## Interpretation

This narrows the field-helper boundary in FND-EXE-031 to a signed guard and
two publication sources: saved original payload on the nonnegative path,
or an unread helper's return on the negative path. It does not establish
reference counting, ownership or deep copying. Q-EXE-009 still needs
preceding-word producers, auxiliary callees, temporary construction, aliases
and cleanup. Destination stores precede record cleanup; local call order
alone does not establish transactional behavior.

## Alternatives

An unsigned or byte-only guard, always calling the auxiliary helper for
the fixed-address case, using its return on the nonnegative path, or cleaning
up before publication are ruled out by the bounded instructions. Sharing
and copying remain possible readings requiring callee evidence. A register
word in an outgoing slot might be padding or an input; unread consumption
prevents choosing either interpretation.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006D6C70` and
read 60 instructions from its entry and twelve from `0x006D6D42`. Restrict
claims to the cited bodies, excluding following functions. Follow the saved
pointer and destination, preceding word's width and sign, fixed-address
comparison, both publication sources and cleanup order. Use FND-EXE-031 for
argument last writers. Separate explicit inputs, later outgoing words and
unknown consumption rather than accepting decompiler guesses. Cover negative
and nonnegative words and both fixed-address arms conditional on valid
storage and normal callees. Keep rich reports local and execute no
interpreter or game.
