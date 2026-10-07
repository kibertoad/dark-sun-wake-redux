---
id: FND-EXE-039
title: Null-input failure route constructs a payload field before signed decrement and shared publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7C80..0x005F7D34
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7D98..0x005F7DD0
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE590..0x005FE5FD
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-038 reaches `0x005F7C80` for unequal endpoints with a zero start.
This helper sets up a local record through `0x006008F0`, storing
`0x005F7D35` as a handler target. It reads its first original 32-bit argument
and calls `0x006D6BE0` with a local destination-word address, that argument
and another local address. FND-EXE-038 reads the temporary producer.

After normal completion it overwrites the first outgoing slot with eight
and calls `0x005FCD70`, saving its result. FND-EXE-025 records that allocator's
80-byte prefix: the ordinary augmented request here is 88 bytes. It then
calls `0x005FE590` with the saved pointer and the local payload-word address.
No local result-null check precedes construction.

The constructor saves its first original 32-bit pointer before local record
setup through `0x006008F0`, storing handler target `0x005FE600`. After normal
setup it reloads that destination, reads its second original argument and
writes `0x00759CA8` as the destination's first 32-bit word. It forms destination
plus four, marks local state one and calls `0x006D6C70` with that adjusted
destination and the second argument. FND-EXE-032 through FND-EXE-035 read
the payload-field publication branches and their storage boundaries. After
normal completion it calls `0x00600990` with its local record and returns.
Unlike FND-EXE-037's outer wrapper, this direct constructor performs no later
first-word replacement. Stored-word contents and handler bodies remain unread.

After construction, the failure helper rereads its local payload pointer,
saves payload minus twelve and compares it with `0x0071B270`. Equality joins
publication directly. Otherwise it sets local state one and calls
`0x005F5760` with payload minus four and minus one in the first two outgoing
slots, followed by two copies of payload minus twelve. FND-EXE-033 reads
only the first two inputs and establishes return of the pre-addition word.
The caller tests that old value as signed 32-bit. Greater than zero joins
publication. Zero or negative calls `0x006D4DF0` with the saved prefix address,
a local address and two copies of the old value, then joins publication after
normal completion. Release effects remain unread.

Publication reloads the allocated pointer, sets local state minus one and
calls `0x005FAED0` with that pointer, `0x00755DC8` and `0x005FE390` in the
first three outgoing slots, followed by a preserved register word. FND-EXE-025
reads the consumer's first three inputs and prefix effects. The analyzer
has no fall-through at this call; following bytes are also the stored handler
target. They are not treated as a normal-return cleanup sequence here.

## Interpretation

This narrows the null-input route to temporary construction, an augmented
object request, payload-field publication, a signed old-value decrement
decision and the shared failure consumer. Its constructor and consumer words
differ from the capacity-limit route in FND-EXE-036 and FND-EXE-037. It does
not establish exception identity, rejection, termination, ownership or cleanup
safety. Q-EXE-009 retains handler, release, consumer and caller contracts.
All post-call paths remain conditional on normal completion and valid storage.

## Alternatives

An unaugmented eight-byte malloc request, testing the updated decrement value,
or applying the decrement on the special prefix-address branch are ruled out
by the bounded body. A final constructor first-word overwrite matching
FND-EXE-037 is absent from this constructor's direct body. Inferring the same
failure identity from similar call order, or a guaranteed throw from the
analyzer's no-return flag, is not supported.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F7C80` and
`0x005FE590`. Read seventy instructions from the former, twenty from
`0x005F7D98`, sixteen from the constructor entry and twenty-four from
`0x005FE5BD`. Restrict claims to the cited ranges, excluding stored handlers
and later functions. Follow original argument reads, temporary and allocation
slots, saved pointer, constructor stores and field publication, prefix guard,
old-value signed test, conditional helper and final consumer writers. Use the
cited findings for callee boundaries; keep unread effects conditional. Keep
rich reports local and execute no interpreter or game.
